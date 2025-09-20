using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblInventoryMe
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public int? Qty { get; set; }

    public string? LocationCode { get; set; }

    public string? Orderno { get; set; }
}
