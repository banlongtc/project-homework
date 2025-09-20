using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterPosition
{
    public int IdPosition { get; set; }

    public string? PositionCode { get; set; }

    public string? PositionName { get; set; }

    public int? IdLocation { get; set; }

    public string? PositionDes { get; set; }

    public int? IdLine { get; set; }

    public int? LocationChildId { get; set; }

    public virtual TblProdLine? IdLineNavigation { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblLocationC? LocationChild { get; set; }

    public virtual ICollection<TblChecksheetWorkstationAssignment> TblChecksheetWorkstationAssignments { get; set; } = new List<TblChecksheetWorkstationAssignment>();

    public virtual ICollection<TblDetailWofrequency> TblDetailWofrequencies { get; set; } = new List<TblDetailWofrequency>();
}
