using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivideLineForMaterial
{
    public int DivideLineMaterialsId { get; set; }

    public string? WorkOrder { get; set; }

    public string? MaNvl { get; set; }

    public string? LotNvl { get; set; }

    public int? LineNumber { get; set; }

    public int? SoLuongChia { get; set; }

    public string? CongDoan { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
