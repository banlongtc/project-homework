using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class EvsWo
{
    public long? Id { get; set; }

    public long? WorkOrder { get; set; }

    public string? ItemNumber { get; set; }

    public string? WoStatus { get; set; }

    public string? Lot { get; set; }

    public long? OrderQty { get; set; }

    public long? QtyComp { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? PartNumber { get; set; }

    public string? Description1 { get; set; }

    public string? ProdLine { get; set; }

    public double? QtyReq { get; set; }

    public long? QtyToIssue { get; set; }

    public double? QtyIssued { get; set; }
}
