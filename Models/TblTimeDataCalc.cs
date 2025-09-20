using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblTimeDataCalc
{
    public int Id { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public string? LoaiThoiGian { get; set; }

    public string? Location { get; set; }
}
