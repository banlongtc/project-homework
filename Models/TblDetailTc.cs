using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDetailTc
{
    public int IdDetail { get; set; }

    public string? TenTc { get; set; }

    public string? MoTa { get; set; }

    public string? ValueText { get; set; }

    public int? ValueInt { get; set; }

    public decimal? ValueDecimal { get; set; }

    public string? Unit { get; set; }

    public int? IdTc { get; set; }

    public string? ValueUnit { get; set; }
}
