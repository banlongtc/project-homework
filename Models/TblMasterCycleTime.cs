using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterCycleTime
{
    public int CycleTimeId { get; set; }

    public int? SoLuongSanXuat { get; set; }

    public decimal? CycleTime { get; set; }

    public string? ProcessCode { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Note { get; set; }
}
