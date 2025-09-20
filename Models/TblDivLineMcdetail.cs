using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivLineMcdetail
{
    public int DivDetailId { get; set; }

    public int? ProdMcid { get; set; }

    public string? WorkOrder { get; set; }

    public string? ShiftLabel { get; set; }

    public string? MachineShift { get; set; }

    public int? QtyDiv { get; set; }

    public string? DateProd { get; set; }

    public string? TypeLabel { get; set; }

    public string? Remarks { get; set; }

    public virtual TblDivMcprod? ProdMc { get; set; }
}
