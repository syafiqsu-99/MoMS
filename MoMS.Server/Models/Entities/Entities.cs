using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoMS.Server.Models.Entities;

[Table("preparation")]
public class Preparation
{
    [Column("type")] public string? Type { get; set; }
    [Column("back_plate")] public string? BackPlate { get; set; }
    [Column("base_mould")] public string? BaseMould { get; set; }
    [Column("blow_core")] public string? BlowCore { get; set; }
    [Column("blow_mould")] public string? BlowMould { get; set; }
    [Column("ejector")] public string? Ejector { get; set; }
    [Column("hot_runner")] public string? HotRunner { get; set; }
    [Column("injection_cavity")] public string? InjectionCavity { get; set; }
    [Column("injection_core")] public string? InjectionCore { get; set; }
    [Column("lip_cavity")] public string? LipCavity { get; set; }
}

[Table("list_docket")]
public class ListDocket
{
    [Column("ID")] public int Id { get; set; }
    [Column("ITEM")] public string? Item { get; set; }
    [Column("S_NUM")] public string? SNum { get; set; }

    [Key]
    [Column("PDF_NAME")] public string PdfName { get; set; } = string.Empty;

    [Column("VENDOR")] public string? Vendor { get; set; }
    [Column("DATETIME")] public DateTime? DateTime { get; set; }
    [Column("YEAR_CREATED")] public int? YearCreated { get; set; }
}