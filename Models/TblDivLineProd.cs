using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivLineProd
{
    public int Id { get; set; }

    public string? WorkOrder { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public int? QtyUsed { get; set; }

    public string? Character { get; set; }

    public DateTime? DateProd { get; set; }

    public TimeSpan? TimeProd { get; set; }

    public int? Line1 { get; set; }

    public int? Line2 { get; set; }

    public int? Line3 { get; set; }

    public int? Line4 { get; set; }

    public string? ChangeControl { get; set; }

    public string? Note { get; set; }

    public int? IdUser { get; set; }

    public int? IdLocation { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
