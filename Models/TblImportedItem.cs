using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblImportedItem
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public int? Qty { get; set; }

    public string? RequestNo { get; set; }

    public DateTime? TimeImport { get; set; }

    public string? ItemType { get; set; }

    public int? IdUser { get; set; }

    public int? IdLocation { get; set; }

    public string? Status { get; set; }

    public string? OrderShipment { get; set; }

    public string? TimeSterilization { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
