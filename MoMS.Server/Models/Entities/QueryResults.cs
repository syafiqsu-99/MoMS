namespace MoMS.Server.Models.Entities;

public record StatusResult(
    double SapOutputTime,
    double ActOutputTime,
    double RejectTime,
    double RunTime,
    double DownTime,
    double AvailTime,
    double Avail,
    double Perf,
    double Quality,
    double Oee);

public record MachineResult(
    string? MachineName,
    bool? StatusStart,
    bool? StatusStop,
    string? Category,
    string? Color);

public record MachineMasterResult(
    string? MachineName,
    string? Type,
    string? Category,
    double? Output,
    double? ActCt,
    string Color);

public record TimelineResult(
    string? MachineName,
    int? IdMachine,
    string? Product,
    int? IdType,
    string? Mould,
    DateTime? Start,
    DateTime? Finish,
    double? Duration,
    string? Category,
    string? MouldCategory,
    double? Output,
    double? PlanOutput,
    double? Efficiency,
    int? Shift,
    DateTime? ProductionDate,
    string? Color);

// list_history read shape. DATETIME is returned raw; the client formats it.
public record ListHistoryResult(
    string? Item,
    string? SNum,
    string? From,
    string? To,
    DateTime? DateTime,
    string? Status,
    string? Remark,
    string? ImgName);