using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblTansuat
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<TblDetailWofrequency> TblDetailWofrequencies { get; set; } = new List<TblDetailWofrequency>();

    public virtual ICollection<TblLocationTansuat> TblLocationTansuats { get; set; } = new List<TblLocationTansuat>();
}
