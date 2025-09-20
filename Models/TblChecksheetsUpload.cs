using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetsUpload
{
    public int ChecksheetId { get; set; }

    public string ChecksheetCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int? CurrentVersionId { get; set; }

    public int? IdLocation { get; set; }

    public virtual TblChecksheetVersion? CurrentVersion { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual ICollection<TblChecksheetItemAssignment> TblChecksheetItemAssignments { get; set; } = new List<TblChecksheetItemAssignment>();

    public virtual ICollection<TblChecksheetVersion> TblChecksheetVersions { get; set; } = new List<TblChecksheetVersion>();

    public virtual ICollection<TblChecksheetWorkstationAssignment> TblChecksheetWorkstationAssignments { get; set; } = new List<TblChecksheetWorkstationAssignment>();
}
