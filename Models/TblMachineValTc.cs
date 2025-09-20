using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMachineValTc
{
    public int Id { get; set; }

    public string MachineCode { get; set; } = null!;

    public int IdValTc { get; set; }

    public int IdNhomTc { get; set; }

    public string? Remark { get; set; }
}
