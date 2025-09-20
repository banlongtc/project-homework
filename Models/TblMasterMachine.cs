using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterMachine
{
    public int Id { get; set; }

    public string? MachineCode { get; set; }

    public string? MachineName { get; set; }

    public int? IdLocation { get; set; }

    public string? LocationCode { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }
}
