using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetWorkstationAssignment
{
    public int WorkstationAssignmentId { get; set; }

    public int WorkstationId { get; set; }

    public int ChecksheetId { get; set; }

    public int? LastUsedChecksheetVersionId { get; set; }

    public DateTime? AssignmentDate { get; set; }

    public bool? IsChecksheetCondition { get; set; }

    public virtual TblChecksheetsUpload Checksheet { get; set; } = null!;

    public virtual TblChecksheetVersion? LastUsedChecksheetVersion { get; set; }

    public virtual TblMasterPosition Workstation { get; set; } = null!;
}
