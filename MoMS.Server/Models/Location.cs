using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoMS.Server.Models;

[Table("location")]
public class Location
{
    [Key]
    [Column("LOCATION")]
    public string Name { get; set; } = default!;

    [Column("CATEGORY")]
    public string? Category { get; set; }
}
