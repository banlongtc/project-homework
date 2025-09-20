using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblRecevingPlme
{
    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public int? Qty { get; set; }

    public string? LocationCode { get; set; }

    public string? LocationName { get; set; }

    public DateTime? ModifyUpdate { get; set; }

    public int NewId { get; set; }

    public string? OrderShipment { get; set; }
}
