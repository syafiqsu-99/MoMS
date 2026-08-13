using MoMS.Server.Data;
using MoMS.Server.Models.Entities;
using MoMS.Server.Utilities;

namespace MoMS.Server.Services;

public class DashboardService(MoMsDbContext context) : BaseService(context)
{
    public async Task<StatusResult?> GetStatusAsync(CancellationToken cancellationToken)
    {
        var rows = await QueryRawAsync(StatusSql, r => new StatusResult(
            Convert.ToDouble(r["SAP_Output_Time"]),
            Convert.ToDouble(r["ACT_Output_Time"]),
            Convert.ToDouble(r["Reject_Time"]),
            Convert.ToDouble(r["Run_Time"]),
            Convert.ToDouble(r["Down_Time"]),
            Convert.ToDouble(r["Avail_Time"]),
            Convert.ToDouble(r["Avail"]),
            Convert.ToDouble(r["Perf"]),
            Convert.ToDouble(r["Quality"]),
            Convert.ToDouble(r["OEE"])),
            cancellationToken);

        return rows.FirstOrDefault();
    }

    public async Task<List<MachineResult>> GetMachinesAsync(CancellationToken cancellationToken)
    {
        return await QueryRawAsync(MachinesSql, r => new MachineResult(
            ToStringN(r["machine_name"]),
            ToBoolN(r["status_start"]),
            ToBoolN(r["status_stop"]),
            ToStringN(r["category"]),
            ToStringN(r["color"])),
            cancellationToken);
    }

    public async Task<List<MachineMasterResult>> GetMachineMasterAsync(CancellationToken cancellationToken)
    {
        return await QueryRawAsync(MachineMasterSql, r =>
        {
            var category = ToStringN(r["category"]);
            return new MachineMasterResult(
                ToStringN(r["machine_name"]),
                ToStringN(r["type"]),
                category,
                ToDoubleN(r["output"]),
                ToDoubleN(r["act_ct"]),
                CategoryColors.ForCategory(category));
        }, cancellationToken);
    }

    private const string StatusSql = @"
    DECLARE @date DATETIME;
    IF GETDATE() BETWEEN CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 06:00:00' AS DATETIME) AND CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME)
        SET @date = CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 06:00:00' AS DATETIME);
    ELSE IF GETDATE() BETWEEN CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME) AND CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 23:59:59' AS DATETIME)
        SET @date = CAST(CONVERT(VARCHAR, GETDATE(), 112) + ' 18:00:00' AS DATETIME);
    ELSE
        SET @date = CAST(CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 112) + ' 18:00:00' AS DATETIME);

    SELECT
        COALESCE(SUM(ct.ct * mm.shot), 0) AS SAP_Output_Time,
        COALESCE(SUM(mm.act_ct * mm.shot), 0) AS ACT_Output_Time,
        COALESCE(SUM(ng.defect_second), 0) AS Reject_Time,
        COALESCE(SUM(tu.start_second), 0) AS Run_Time,
        COALESCE(SUM(ts.stop_second), 0) AS Down_Time,
        COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) AS Avail_Time,
        CASE WHEN COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) = 0 THEN 0
             ELSE (CAST(COALESCE(SUM(tu.start_second), 0) AS FLOAT) /
                  (COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0))) * 100 END AS Avail,
        CASE WHEN COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0 THEN 0
             ELSE (CAST(COALESCE(SUM(ct.ct * mm.shot), 0) AS FLOAT) /
                  COALESCE(SUM(mm.act_ct * mm.shot), 0)) * 100 END AS Perf,
        CASE WHEN COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0 THEN 0
             ELSE (CAST(COALESCE(SUM(mm.act_ct * mm.shot), 0) - COALESCE(SUM(ng.defect_second), 0) AS FLOAT) /
                  COALESCE(SUM(mm.act_ct * mm.shot), 0)) * 100 END AS Quality,
        CASE WHEN COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0) = 0
                  OR COALESCE(SUM(mm.act_ct * mm.shot), 0) = 0 THEN 0
             ELSE ((CAST(COALESCE(SUM(tu.start_second), 0) AS FLOAT) /
                   (COALESCE(SUM(tu.start_second), 0) + COALESCE(SUM(ts.stop_second), 0))) *
                   (CAST(COALESCE(SUM(ct.ct * mm.shot), 0) AS FLOAT) /
                   COALESCE(SUM(mm.act_ct * mm.shot), 0)) *
                   (CAST(COALESCE(SUM(mm.act_ct * mm.shot), 0) - COALESCE(SUM(ng.defect_second), 0) AS FLOAT) /
                   COALESCE(SUM(mm.act_ct * mm.shot), 0))) * 100 END AS OEE
    FROM CMS.dbo.machine_master mm
    JOIN CMS.dbo.list_ct ct ON mm.id_type = ct.id_type AND mm.mould = ct.mould
    LEFT JOIN (
        SELECT id_machine, SUM(DATEDIFF(second, start, ISNULL(finish, GETDATE()))) AS start_second
        FROM CMS.dbo.trans_uptime WHERE start BETWEEN @date AND GETDATE() GROUP BY id_machine
    ) tu ON mm.id_machine = tu.id_machine
    LEFT JOIN (
        SELECT id_machine, SUM(DATEDIFF(second, start, ISNULL(finish, GETDATE()))) AS stop_second
        FROM CMS.dbo.trans_stop WHERE start BETWEEN @date AND GETDATE() GROUP BY id_machine
    ) ts ON mm.id_machine = ts.id_machine
    LEFT JOIN (
        SELECT id_machine, COALESCE(SUM((ct / NULLIF(qty_perct, 0)) * qty), 0) AS defect_second
        FROM CMS.dbo.trans_ng WHERE time BETWEEN @date AND GETDATE() GROUP BY id_machine
    ) ng ON mm.id_machine = ng.id_machine
    WHERE mm.machine_name <> 'M16';";

    private const string MachinesSql = @"
    SELECT mm.machine_name, mm.status_start, mm.status_stop,
        UPPER(CASE
            WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
            WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
            WHEN tt.category IS NULL THEN
                CASE WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running' ELSE 'Status Undefined' END
            ELSE tt.category END) AS category,
        CASE UPPER(CASE
            WHEN tt.category = 'Others' AND (ts.problem = 'Prod' OR ts.problem IS NULL OR ts.problem = 'OFF') THEN 'Others Prod'
            WHEN tt.category = 'Others' AND ts.problem = 'Tech' THEN 'Others Tech'
            WHEN tt.category IS NULL THEN
                CASE WHEN mm.status_start = 1 AND mm.status_stop = 0 THEN 'Production Running' ELSE 'Status Undefined' END
            ELSE tt.category END)
            WHEN 'PRODUCTION RUNNING' THEN '#00ff00'
            WHEN 'PRODUCT BUYOFF' THEN '#808080'
            WHEN 'NO OPERATOR' THEN '#ffff00'
            WHEN 'NO SCHEDULE' THEN '#ffff00'
            WHEN 'MATERIAL DRYING' THEN '#ffff00'
            WHEN 'OTHERS PROD' THEN '#ffff00'
            WHEN 'QUALITY ISSUE' THEN '#ff0000'
            WHEN 'PRODUCTION SAMPLE' THEN '#ff0000'
            WHEN 'MOULD CHANGE' THEN '#ff0000'
            WHEN 'OTHERS TECH' THEN '#ff0000'
            WHEN 'SCHEDULED MAINTENANCE' THEN '#ffa500'
            WHEN 'MACHINE BREAKDOWN' THEN '#ffa500'
            ELSE '#ffffff' END AS color
    FROM CMS.dbo.machine_master mm
    INNER JOIN CMS.dbo.trans_time tt ON mm.id_machine = tt.id_machine
    LEFT JOIN (
        SELECT id_machine, problem FROM CMS.dbo.trans_stop
        WHERE finish IS NULL AND category = 'Others' AND id_machine != 16
    ) ts ON mm.id_machine = ts.id_machine
    WHERE mm.machine_name != 'M16'
    ORDER BY mm.id_machine ASC;";

    // The 26-way UNION over machine_log_{n} is generated so the query stays in
    // one place; the loop only emits integer literals, never user input.
    private static readonly string MachineMasterSql = BuildMachineMasterSql();

    private static string BuildMachineMasterSql()
    {
        var unions = Enumerable.Range(1, 26).Select(n =>
            $"SELECT TOP 1 {n} AS id_machine, machine_name, category FROM CMS.dbo.machine_log_{n} ORDER BY start DESC");

        var latestLogs = string.Join("\n        UNION ALL\n        ", unions);

        return $@"
        WITH latest_logs AS (
        {latestLogs}
        )
        SELECT
            mm.machine_name,
            mm.shot * mm.qty_perct AS output,
            mm.act_ct,
            mm.type,
            CASE WHEN (ll.category IS NULL OR ll.category = '')
                      AND mm.status_start = 1 AND mm.status_off = 1
                 THEN 'PRODUCTION RUNNING'
                 ELSE COALESCE(ll.category, '') END AS category
        FROM CMS.dbo.machine_master mm
        LEFT JOIN latest_logs ll ON mm.id_machine = ll.id_machine";
    }
}