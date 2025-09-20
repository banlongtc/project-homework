using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetFormEntry
{
    public int FormEntryId { get; set; }

    public int ChecksheetVerId { get; set; }

    public int? ItemAssignmentId { get; set; }

    public string WorkOrderCode { get; set; } = null!;

    public int EntryIndex { get; set; }

    public string ProcessStatus { get; set; } = null!;

    public bool IsAbnormal { get; set; }

    public string? AbnormalReason { get; set; }

    public DateTime? AbnormalReportedAt { get; set; }

    public string? AbnormalReportedBy { get; set; }

    public bool IsStopped { get; set; }

    public string? StopReason { get; set; }

    public DateTime? StoppedAt { get; set; }

    public string? StoppedBy { get; set; }

    public bool IsLeaderApproved { get; set; }

    public string? LeaderApprovalReason { get; set; }

    public DateTime? LeaderApprovedAt { get; set; }

    public string? LeaderApprovedBy { get; set; }

    public DateTime? RestartTimeUtc { get; set; }

    public string? RestartApprovedBy { get; set; }

    public DateTime? RestartApprovedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsExported { get; set; }

    public string? ExportedBy { get; set; }

    public DateTime? ExportedAt { get; set; }

    public string? PositionCode { get; set; }

    public int? QtyOfReads { get; set; }

    public int? QtyProduction { get; set; }

    public int? QtyOk { get; set; }

    public int? QtyNg { get; set; }

    public string? TrayNo { get; set; }

    public virtual TblChecksheetVersion ChecksheetVer { get; set; } = null!;

    public virtual TblChecksheetItemAssignment? ItemAssignment { get; set; }

    public virtual ICollection<TblChecksheetEntryValue> TblChecksheetEntryValues { get; set; } = new List<TblChecksheetEntryValue>();
}
