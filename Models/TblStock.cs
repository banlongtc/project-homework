using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblStock
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    public decimal? QtyStock { get; set; }

    public DateTime? Date { get; set; }

    public string? Lotno { get; set; }
}
