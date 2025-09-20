using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblUserRole
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdRole { get; set; }

    public virtual TblRole? IdRoleNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
