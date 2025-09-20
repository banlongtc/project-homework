using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblManageMaterial
{
    public int Id { get; set; }

    public string? RequestNo { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public string? ItemName { get; set; }

    public string? ItemCate { get; set; }

    public int? QtyImport { get; set; }

    public int? QtyUse { get; set; }

    public int? QtyInventory { get; set; }

    public string? BookType { get; set; }

    public DateTime? TimeUpdate { get; set; }

    public string? Remarks { get; set; }

    public int? IdUser { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
