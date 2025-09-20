using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TransEv
{
    public string ItemNumber { get; set; } = null!;

    public string? Lot { get; set; }

    public string? Description1 { get; set; }

    public string? ProdLine { get; set; }

    public string? Location { get; set; }

    public string? TranType { get; set; }

    public string? Order { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? Time { get; set; }

    public DateTime? Effdate { get; set; }

    public double? ChangeQty { get; set; }

    public string? InvenStatus { get; set; }
}
