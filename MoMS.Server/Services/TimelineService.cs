using MoMS.Server.Data;
using MoMS.Server.Models.Entities;

namespace MoMS.Server.Services;

public class TimelineService(MoMsDbContext context) : BaseService(context)
{
    public async Task<List<TimelineResult>> GetTimelineAsync(CancellationToken cancellationToken)
    {
        return await QueryRawAsync(Sql, r => new TimelineResult(
            ToStringN(r["machine_name"]),
            ToIntN(r["id_machine"]),
            ToStringN(r["product"]),
            ToIntN(r["id_type"]),
            ToStringN(r["mould"]),
            ToDateTimeN(r["start"]),
            ToDateTimeN(r["finish"]),
            ToDoubleN(r["duration"]),
            ToStringN(r["category"]),
            ToStringN(r["mould_category"]),
            ToDoubleN(r["output"]),
            ToDoubleN(r["plan_output"]),
            ToDoubleN(r["efficiency"]),
            ToIntN(r["shift"]),
            ToDateTimeN(r["production_date"]),
            ToStringN(r["color"])),
            cancellationToken);
    }

    private const string Sql = @"
    WITH timeline AS (
        SELECT id_machine, id_type, mould,
            COALESCE(start, GETDATE()) AS start,
            COALESCE(finish, GETDATE()) AS finish,
            COALESCE(category, 'Undefined') AS category,
            mould_category, shift, production_date
        FROM CMS.dbo.trans_stop
        WHERE production_date =
            CASE WHEN CAST(GETDATE() AS TIME) BETWEEN '00:00:00' AND '05:59:59'
                 THEN DATEADD(DAY, -1, CAST(GETDATE() AS DATE)) ELSE CAST(GETDATE() AS DATE) END
        AND shift =
            CASE WHEN CAST(GETDATE() AS TIME) BETWEEN '06:00:00' AND '17:59:59' THEN 1 ELSE 2 END
        UNION ALL
        SELECT id_machine, id_type, mould,
            COALESCE(start, GETDATE()) AS start,
            COALESCE(finish, GETDATE()) AS finish,
            'Production Running' AS category,
            NULL AS mould_category, shift, production_date
        FROM CMS.dbo.trans_uptime
        WHERE production_date =
            CASE WHEN CAST(GETDATE() AS TIME) BETWEEN '00:00:00' AND '05:59:59'
                 THEN DATEADD(DAY, -1, CAST(GETDATE() AS DATE)) ELSE CAST(GETDATE() AS DATE) END
        AND shift =
            CASE WHEN CAST(GETDATE() AS TIME) BETWEEN '06:00:00' AND '17:59:59' THEN 1 ELSE 2 END
    )
    SELECT
        mm.machine_name, tl.id_machine, lc.type AS product, tl.id_type, tl.mould,
        tl.start, tl.finish,
        DATEDIFF(MINUTE, tl.start, tl.finish) / 60.0 AS duration,
        tl.category, tl.mould_category,
        (mm.shot * mm.qty_perct) AS output,
        CASE
            WHEN DATEPART(HOUR, GETDATE()) BETWEEN 6 AND 17 THEN
                (((DATEPART(HOUR, GETDATE()) - 6) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
            WHEN DATEPART(HOUR, GETDATE()) < 6 THEN
                (((DATEPART(HOUR, GETDATE()) + 24 - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
            ELSE
                (((DATEPART(HOUR, GETDATE()) - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
        END AS plan_output,
        ROUND(COALESCE(((mm.shot * mm.qty_perct) /
            CASE
                WHEN DATEPART(HOUR, GETDATE()) BETWEEN 6 AND 17 THEN
                    (((DATEPART(HOUR, GETDATE()) - 6) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
                WHEN DATEPART(HOUR, GETDATE()) < 6 THEN
                    (((DATEPART(HOUR, GETDATE()) + 24 - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
                ELSE
                    (((DATEPART(HOUR, GETDATE()) - 18) * 3600 + DATEPART(MINUTE, GETDATE()) * 60 + DATEPART(SECOND, GETDATE())) / (mm.ct / NULLIF(mm.qty_perct, 0)))
            END) * 100, 0), 2) AS efficiency,
        tl.shift, tl.production_date,
        CASE
            WHEN tl.category = 'Production Running' THEN 'rgb(0, 255, 0)'
            WHEN tl.category IN ('No Operator', 'No Schedule', 'Material Drying', 'Others') THEN 'rgb(255, 255, 0)'
            WHEN tl.category IN ('Machine Breakdown', 'Scheduled Maintenance') THEN 'rgb(255, 165, 0)'
            WHEN tl.category = 'Product Buyoff' THEN 'rgb(128, 128, 128)'
            WHEN tl.category IN ('Quality Issue', 'Production Sample', 'Mould Change') THEN 'rgb(255, 0, 0)'
            ELSE 'rgb(255, 255, 255)'
        END AS color
    FROM timeline tl
    LEFT JOIN CMS.dbo.list_ct lc ON tl.id_type = lc.id_type AND tl.mould = lc.mould
    LEFT JOIN CMS.dbo.machine_master mm ON tl.id_machine = mm.id_machine
    WHERE mm.machine_name <> 'M16'
    ORDER BY tl.start DESC;";
}