using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblSection
{
    public int IdSection { get; set; }

    public string? SectionName { get; set; }

    public string? Remarks { get; set; }

    public virtual ICollection<TblLocation> TblLocations { get; set; } = new List<TblLocation>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
