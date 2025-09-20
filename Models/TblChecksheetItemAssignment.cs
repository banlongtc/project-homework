using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetItemAssignment
{
    public int ItemAssignmentId { get; set; }

    public int ChecksheetId { get; set; }

    public int? LastUsedChecksheetVersionId { get; set; }

    public bool? IsLocked { get; set; }

    public DateTime? AssignmentDate { get; set; }

    public string? ProductItem { get; set; }

    public string? ProductLot { get; set; }

    public bool? IsChecksheetCondition { get; set; }

    public virtual TblChecksheetsUpload Checksheet { get; set; } = null!;

    public virtual TblChecksheetVersion? LastUsedChecksheetVersion { get; set; }

    public virtual ICollection<TblChecksheetFormEntry> TblChecksheetFormEntries { get; set; } = new List<TblChecksheetFormEntry>();
}
