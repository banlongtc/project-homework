using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class WoBrowse
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
}
