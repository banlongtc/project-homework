using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterProductItem
{
    public int IdItem { get; set; }

    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public string? ItemType { get; set; }

    public string? Unit { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
