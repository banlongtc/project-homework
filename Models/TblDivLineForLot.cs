using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivLineForLot
{
    public int Id { get; set; }

    public string? WorkOrder { get; set; }

    public string? ProductCode { get; set; }

    public string? LotDivLine { get; set; }

    public int? Line1 { get; set; }

    public int? Line2 { get; set; }

    public int? Line3 { get; set; }

    public int? Line4 { get; set; }
}
