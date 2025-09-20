using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblRole
{
    public int IdRole { get; set; }

    public string? RoleName { get; set; }

    public string? RoleCode { get; set; }

    public string? RoleDescriptions { get; set; }

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();
}
