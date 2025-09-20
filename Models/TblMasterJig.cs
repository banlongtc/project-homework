using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterJig
{
    public int Id { get; set; }

    public string? JigCode { get; set; }

    public string? JigName { get; set; }

    public string? IdjigParent { get; set; }

    public string? Progress { get; set; }

    public int? IdLocation { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }
}
