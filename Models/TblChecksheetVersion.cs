using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetVersion
{
    public int ChecksheetVersionId { get; set; }

    public int ChecksheetId { get; set; }

    public decimal VersionNumber { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Statusname { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Notes { get; set; }

    public bool? IsChangeForm { get; set; }

    public string? PositionWorkingCode { get; set; }

    public string? FilePath { get; set; }

    public string? SheetName { get; set; }

    public string? FileName { get; set; }

    public string? ChecksheetType { get; set; }

    public virtual TblChecksheetsUpload Checksheet { get; set; } = null!;

    public virtual ICollection<TblChecksheetFormEntry> TblChecksheetFormEntries { get; set; } = new List<TblChecksheetFormEntry>();

    public virtual ICollection<TblChecksheetForm> TblChecksheetForms { get; set; } = new List<TblChecksheetForm>();

    public virtual ICollection<TblChecksheetItemAssignment> TblChecksheetItemAssignments { get; set; } = new List<TblChecksheetItemAssignment>();

    public virtual ICollection<TblChecksheetWorkstationAssignment> TblChecksheetWorkstationAssignments { get; set; } = new List<TblChecksheetWorkstationAssignment>();

    public virtual ICollection<TblChecksheetsUpload> TblChecksheetsUploads { get; set; } = new List<TblChecksheetsUpload>();
}
