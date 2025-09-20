using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblLocationC
{
    public int Id { get; set; }

    public string? LocationCodeC { get; set; }

    public string? LocationNameC { get; set; }

    public int? IdChaC { get; set; }

    public virtual TblLocation? IdChaCNavigation { get; set; }

    public virtual ICollection<TblLocationTansuat> TblLocationTansuats { get; set; } = new List<TblLocationTansuat>();

    public virtual ICollection<TblMasterPosition> TblMasterPositions { get; set; } = new List<TblMasterPosition>();
}
