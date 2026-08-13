namespace MoMS.Server.Models.Dtos;

// list-history: single-object branch (POST /api/list-history with one item).
// RESET toggles the USAGE-to-zero reset on the linked full_list row.
public record ListHistorySingleDto(
    string Item,
    string SNum,
    string From,
    string To,
    string Status,
    string? Remark,
    bool Reset);

// list-history: array branch. Each element also carries TYPE, used to resolve
// the target mach_details column via the type→column map.
public record ListHistoryBatchItemDto(
    string Item,
    string SNum,
    string Type,
    string From,
    string To,
    string Status,
    string? Remark);

// update-repeat: applies the same PLAN_SERV / PLAN_USAGE / REPEAT to every
// serial number in the list.
public record UpdateRepeatDto(
    List<string> SNum,
    DateTime? PlanServ,
    int? PlanUsage,
    int? Repeat);

// upload-images-sql: writes the stored comma-separated file names back onto the
// matching list_history row.
public record UploadImagesSqlDto(
    string Item,
    string SNum,
    string From,
    string To,
    DateTime DateTime,
    string Status,
    string ImgName);

// upload-images (PUT): clears IMG_NAME and deletes the named files from disk.
public record DeleteImagesDto(
    string Item,
    string SNum,
    string From,
    string To,
    DateTime DateTime,
    string Status,
    string ImgName);

public record ListOptionCreateDto(
    string Category,
    string Value);

public record ListOptionUpdateDto(
    string Category,
    string OldValue,
    string NewValue);

public record ListOptionDeleteDto(
    string Category,
    string Value);

// dockets: everything needed to build the PDF and insert the list_docket row.
// The image list holds file names already uploaded to the toolset image folder.
public record DocketCreateDto(
    string Item,
    string SNum,
    string Vendor,
    DateTime DateTime,
    string VendorName,
    string PicName,
    string DateOut,
    string TimeOut,
    string DateIn,
    string SelectPurpose,
    string ModelDetails,
    string PartsDetails,
    string RemarksDetails,
    string SelectPrepared,
    List<string> Images);