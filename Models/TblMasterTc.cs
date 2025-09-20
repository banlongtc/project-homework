using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterTc
{
    public int IdTc { get; set; }

    public string? TenTieuchuan { get; set; }

    public string? Remark { get; set; }

    public bool? TcMay { get; set; }

    public string? TcCode { get; set; }
}
