using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblItemValTc
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    public int? IdValTc { get; set; }

    public int? IdNhomTc { get; set; }

    public string? Remark { get; set; }
}
