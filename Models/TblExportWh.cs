using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblExportWh
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public string? Unit { get; set; }

    public int? QtyEx { get; set; }

    public string? Whlocation { get; set; }

    public string? Progress { get; set; }

    public int? IdUser { get; set; }

    public string? Note1 { get; set; }

    public DateTime? DateImport { get; set; }

    public TimeSpan? TimeImport { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
