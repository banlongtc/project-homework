using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblShiftSchedule
{
    public int ScheduleId { get; set; }

    public TimeSpan? ShiftStartTime { get; set; }

    public TimeSpan? ShiftEndTime { get; set; }

    public TimeSpan? BreakStartTime { get; set; }

    public TimeSpan? BreakEndTime { get; set; }

    public string? TypeShift { get; set; }

    public string? LocationCode { get; set; }

    public bool? PassDay { get; set; }
}
