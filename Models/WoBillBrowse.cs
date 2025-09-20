using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class WoBillBrowse
{
    public long? Id { get; set; }

    public long? WorkOrder { get; set; }

    public string? PartNumber { get; set; }

    public double? QtyReq { get; set; }

    public long? QtyToIssue { get; set; }

    public double? QtyIssued { get; set; }

    public DateTime? IssueDate { get; set; }
}
