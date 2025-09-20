using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblLeadTime
{
    public int Id { get; set; }

    public TimeSpan? TimeStart { get; set; }

    public TimeSpan? TimePause { get; set; }

    public TimeSpan? TimeAfterPause { get; set; }

    public TimeSpan? TimeEnd { get; set; }

    public int? Total { get; set; }

    public DateTime? DateOfTime { get; set; }

    public int? IdShift { get; set; }

    public int? IdUser { get; set; }

    public virtual TblShift? IdShiftNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
