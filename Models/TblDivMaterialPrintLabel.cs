using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivMaterialPrintLabel
{
    public int IdMaterial { get; set; }

    public int? ProdMcid { get; set; }

    public string? WorkOrder { get; set; }

    public string? MaterialCode { get; set; }

    public string? ShiftLabel { get; set; }

    public string? MachineShift { get; set; }

    public int? QtyDiv { get; set; }

    public string? LotMaterial { get; set; }

    public virtual TblDivMcprod? ProdMc { get; set; }
}
