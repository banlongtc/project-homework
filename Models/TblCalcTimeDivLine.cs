using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblCalcTimeDivLine
{
    public int CalcTimeId { get; set; }

    public int? SoTt { get; set; }

    public string? WorkOrder { get; set; }

    public int? LineNumber { get; set; }

    public int? SoLuongDuDinh { get; set; }

    public decimal? ThoiGianSanXuat { get; set; }

    public DateTime? NgayDuDinhSanXuat { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public int? SoLuongTrenNgay { get; set; }

    public string? LocationCode { get; set; }

    public string? Character { get; set; }
}
