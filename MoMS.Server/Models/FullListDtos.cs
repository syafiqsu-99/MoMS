namespace MoMS.Server.Models;

public record FullListCreateDto(
    string? Item,
    string SNum,
    string? Type,
    string? Rack,
    string? Level,
    string? Status,
    string? Remark,
    long? PlanUsage,
    DateTime? PlanServ,
    int? Repeat);

public record FullListUpdateDto(
    string? Item,
    string? Rack,
    string? Level,
    string? Location,
    string? Status,
    string? Remark,
    long? Usage,
    DateTime? LastServ);
