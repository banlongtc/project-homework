using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Datum
{
    public string? Productcode { get; set; }

    public string? Articleshortname { get; set; }

    public string? Version { get; set; }

    public string? Work { get; set; }

    public string? Inputgoodscode { get; set; }

    public string? Articleshortname1 { get; set; }

    public string? Unitcode { get; set; }

    public double? Seq { get; set; }

    public double? Bomqty { get; set; }

    public string? Output { get; set; }

    public DateTime? EffFrom { get; set; }

    public DateTime? EffTo { get; set; }

    public string? Approvercode { get; set; }

    public string? Approvername { get; set; }

    public DateTime? Approveddate { get; set; }
}
