using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblHistoryLogin
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public DateTime? TimeLogin { get; set; }

    public DateTime? TimeLogout { get; set; }

    public string? Remarks { get; set; }

    public int? IdPosition { get; set; }

    public int? IdShift { get; set; }

    public virtual TblShift? IdShiftNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }
}
