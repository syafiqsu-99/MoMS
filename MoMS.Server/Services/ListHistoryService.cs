using MoMS.Server.Data;
using MoMS.Server.Models.Dtos;
using MoMS.Server.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class ListHistoryService(MoMsDbContext context) : BaseService(context)
{
    private static readonly Dictionary<string, string> TypeToColumn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BP"] = "back_plate",
        ["BS"] = "base_mould",
        ["BC"] = "blow_core",
        ["BM"] = "blow_mould",
        ["ER"] = "ejector",
        ["HR"] = "hot_runner",
        ["CT"] = "injection_cavity",
        ["IC"] = "injection_core",
        ["LS"] = "lip_cavity"
    };

    // GET /api/list-history — raw datetime returned; the client formats it.
    public async Task<List<ListHistoryResult>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT ITEM, S_NUM, [FROM], [TO], DATETIME, STATUS, REMARK, IMG_NAME
            FROM list_history
            ORDER BY DATETIME DESC, ITEM ASC";

        return await QueryRawAsync(sql, r => new ListHistoryResult(
            ToStringN(r["ITEM"]),
            ToStringN(r["S_NUM"]),
            ToStringN(r["FROM"]),
            ToStringN(r["TO"]),
            ToDateTimeN(r["DATETIME"]),
            ToStringN(r["STATUS"]),
            ToStringN(r["REMARK"]),
            ToStringN(r["IMG_NAME"])),
            cancellationToken);
    }

    // Single move: insert one history row and roll the linked full_list forward.
    public async Task AddSingleAsync(ListHistorySingleDto dto, CancellationToken cancellationToken)
    {
        const string sql = @"
            DECLARE @currentDate DATETIME = GETDATE();

            INSERT INTO list_history (ITEM, S_NUM, [FROM], [TO], DATETIME, STATUS, REMARK)
            VALUES (@item, @sNum, @from, @to, @currentDate, @status, @remark);

            UPDATE full_list
            SET LOCATION = @to,
                REMARK = @remark,
                USAGE = CASE WHEN @reset = 1 THEN 0 ELSE USAGE END,
                LAST_SERV = @currentDate,
                PLAN_SERV = CASE WHEN REPEAT IS NULL THEN NULL
                                 ELSE DATEADD(DAY, REPEAT, @currentDate) END
            WHERE S_NUM = @sNum;";

        var parameters = new[]
        {
            new SqlParameter("@item", dto.Item),
            new SqlParameter("@sNum", dto.SNum),
            new SqlParameter("@from", dto.From),
            new SqlParameter("@to", dto.To),
            new SqlParameter("@status", dto.Status),
            new SqlParameter("@remark", (object?)dto.Remark ?? DBNull.Value),
            new SqlParameter("@reset", dto.Reset ? 1 : 0)
        };

        await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    public async Task AddBatchAsync(
        List<ListHistoryBatchItemDto> items,
        CancellationToken cancellationToken)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var item in items)
        {
            if (!TypeToColumn.TryGetValue(item.Type, out var column))
            {
                continue;
            }

            var machDetailsSql =
                $"UPDATE mach_details SET [{column}] = @item WHERE machine_name = @to";

            await Context.Database.ExecuteSqlRawAsync(
                machDetailsSql,
                [new SqlParameter("@item", item.Item), new SqlParameter("@to", item.To)],
                cancellationToken);

            const string historySql = @"
                INSERT INTO list_history (ITEM, S_NUM, [FROM], [TO], DATETIME, STATUS, REMARK)
                VALUES (@item, @sNum, @from, @to, GETDATE(), @status, @remark);

                UPDATE full_list SET LOCATION = @to, REMARK = @remark WHERE S_NUM = @sNum;";

            await Context.Database.ExecuteSqlRawAsync(
                historySql,
                [
                    new SqlParameter("@item", item.Item),
                    new SqlParameter("@sNum", item.SNum),
                    new SqlParameter("@from", item.From),
                    new SqlParameter("@to", item.To),
                    new SqlParameter("@status", item.Status),
                    new SqlParameter("@remark", (object?)item.Remark ?? DBNull.Value)
                ],
                cancellationToken);
        }

        await RecomputeWaitTypesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task RecomputeWaitTypesAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
            WITH RankedMatches AS (
                SELECT
                    t2.machine_name,
                    t1.type AS matched_product_name,
                    ROW_NUMBER() OVER (PARTITION BY t2.machine_name ORDER BY
                        (CASE WHEN t1.back_plate = t2.back_plate THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.base_mould = t2.base_mould THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.blow_core = t2.blow_core THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.ejector = t2.ejector THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.hot_runner = t2.hot_runner THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.injection_cavity = t2.injection_cavity THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.injection_core = t2.injection_core THEN 1 ELSE 0 END) +
                        (CASE WHEN t1.lip_cavity = t2.lip_cavity THEN 1 ELSE 0 END) DESC
                    ) AS rank
                FROM mach_details t2
                JOIN preparation t1 ON
                    (t1.back_plate = t2.back_plate OR
                     t1.base_mould = t2.base_mould OR
                     t1.blow_core = t2.blow_core OR
                     t1.ejector = t2.ejector OR
                     t1.hot_runner = t2.hot_runner OR
                     t1.injection_cavity = t2.injection_cavity OR
                     t1.injection_core = t2.injection_core OR
                     t1.lip_cavity = t2.lip_cavity)
            )
            UPDATE t2
            SET t2.wait_type = rm.matched_product_name
            FROM mach_details t2
            JOIN RankedMatches rm ON t2.machine_name = rm.machine_name AND rm.rank = 1;";

        await Context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}