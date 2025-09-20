using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblSubMaterial
{
    public int Id { get; set; }

    public string? ProductCode { get; set; }

    public string? ProductName { get; set; }

    public int? Inventory { get; set; }

    public int? SafeInventory { get; set; }

    public int? QtyProdPerDay { get; set; }

    public int? QtyPrintedPerRoll { get; set; }

    public int? QtyCanInput { get; set; }

    public int? InventoryPre { get; set; }
}
