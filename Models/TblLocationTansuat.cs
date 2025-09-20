using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblLocationTansuat
{
    public int Id { get; set; }

    public int? IdTansuat { get; set; }

    public int? IdLocationc { get; set; }

    public virtual TblLocationC? IdLocationcNavigation { get; set; }

    public virtual TblTansuat? IdTansuatNavigation { get; set; }
}
