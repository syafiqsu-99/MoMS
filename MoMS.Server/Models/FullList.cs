using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoMS.Server.Models;

[Table("full_list")]
public class FullList
{
    [Column("ITEM")]
    public string? Item { get; set; }

    [Key]
    [Column("S_NUM")]
    public string SNum { get; set; } = default!;

    [Column("TYPE")]
    public string? Type { get; set; }

    [Column("RACK")]
    public string? Rack { get; set; }

    [Column("LEVEL")]
    public string? Level { get; set; }

    [Column("NO")]
    public int? No { get; set; }

    [Column("LOCATION")]
    public string? Location { get; set; }

    [Column("STATUS")]
    public string? Status { get; set; }

    [Column("REMARK")]
    public string? Remark { get; set; }

    [Column("ACCUM_USAGE")]
    public long? AccumUsage { get; set; }

    [Column("USAGE")]
    public long? Usage { get; set; }

    [Column("PLAN_USAGE")]
    public long? PlanUsage { get; set; }

    [Column("LAST_SERV")]
    public DateTime? LastServ { get; set; }

    [Column("PLAN_SERV")]
    public DateTime? PlanServ { get; set; }

    [Column("REPEAT")]
    public int? Repeat { get; set; }
}
