using MoMS.Server.Data;
using MoMS.Server.Models.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class FullListRepeatService(MoMsDbContext context) : BaseService(context)
{
    public async Task UpdateRepeatAsync(UpdateRepeatDto dto, CancellationToken cancellationToken)
    {
        if (dto.SNum.Count == 0)
        {
            return;
        }

        var serialParameters = dto.SNum
            .Select((s, i) => new SqlParameter($"@s{i}", s))
            .ToArray();

        var inClause = string.Join(", ", serialParameters.Select(p => p.ParameterName));

        var sql = $@"
            UPDATE full_list
            SET PLAN_SERV = @planServ,
                PLAN_USAGE = @planUsage,
                REPEAT = @repeat
            WHERE S_NUM IN ({inClause})";

        var parameters = new List<SqlParameter>
        {
            new("@planServ", (object?)dto.PlanServ ?? DBNull.Value),
            new("@planUsage", (object?)dto.PlanUsage ?? DBNull.Value),
            new("@repeat", (object?)dto.Repeat ?? DBNull.Value)
        };
        parameters.AddRange(serialParameters);

        await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}