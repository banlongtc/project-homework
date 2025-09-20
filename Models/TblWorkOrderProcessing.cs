using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblWorkOrderProcessing
{
    public int Id { get; set; }

    public string? Woprocessing { get; set; }

    public string? ProductCode { get; set; }

    public string? LotProcessing { get; set; }

    public int? QtyProcessing { get; set; }

    public int? QtyTotal { get; set; }

    public string? ProcessingStatus { get; set; }

    public string? PositionCode { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string? NextAction { get; set; }

    public virtual ICollection<TblDetailWofrequency> TblDetailWofrequencies { get; set; } = new List<TblDetailWofrequency>();
}
