using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblWorkOrderMe
{
    public int Id { get; set; }

    public string? WorkOrder { get; set; }

    public string? ProgressOrder { get; set; }

    public string? ItemCode { get; set; }

    public string? ItemName { get; set; }

    public string? LotNo { get; set; }

    public int? QtyWo { get; set; }

    public DateTime? TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public DateTime? TimeCreate { get; set; }

    public string? Statusname { get; set; }

    public DateTime? ModifyDateUpdate { get; set; }

    public string? InputGoodsCodeMes { get; set; }

    public decimal? InputGoodsCodeSeq { get; set; }

    public int? QtyUnused { get; set; }

    public string? Character { get; set; }

    public DateTime? DateProd { get; set; }

    public TimeSpan? TimeProd { get; set; }
}
