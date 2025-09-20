using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblProdLine
{
    public int IdLine { get; set; }

    public string? LineName { get; set; }

    public string? Remarks { get; set; }

    public string? LineCode { get; set; }

    public virtual ICollection<TblMasterPosition> TblMasterPositions { get; set; } = new List<TblMasterPosition>();
}
