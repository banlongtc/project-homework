using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblShift
{
    public int IdShift { get; set; }

    public string? ShiftName { get; set; }

    public TimeSpan? TimeStart { get; set; }

    public TimeSpan? TimeEnd { get; set; }

    public bool? PassDay { get; set; }

    public string? Remarks { get; set; }

    public virtual ICollection<TblHistoryLogin> TblHistoryLogins { get; set; } = new List<TblHistoryLogin>();

    public virtual ICollection<TblLeadTime> TblLeadTimes { get; set; } = new List<TblLeadTime>();
}
