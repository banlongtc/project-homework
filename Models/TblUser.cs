using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblUser
{
    public int IdUser { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? EmployeeNo { get; set; }

    public bool? ActiveUser { get; set; }

    public bool? DeactivationUser { get; set; }

    public int? IdSection { get; set; }

    public int? IdLocation { get; set; }

    public bool? IsLocked { get; set; }

    public DateTime? LastPasswordChange { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LastPingAt { get; set; }

    public string? SecondaryPasswordHash { get; set; }

    public bool? Has2Faenabled { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblSection? IdSectionNavigation { get; set; }

    public virtual ICollection<TblDivLineProd> TblDivLineProds { get; set; } = new List<TblDivLineProd>();

    public virtual ICollection<TblDivMcprod> TblDivMcprods { get; set; } = new List<TblDivMcprod>();

    public virtual ICollection<TblExportWh> TblExportWhs { get; set; } = new List<TblExportWh>();

    public virtual ICollection<TblHistoryLogin> TblHistoryLogins { get; set; } = new List<TblHistoryLogin>();

    public virtual ICollection<TblImportedItem> TblImportedItems { get; set; } = new List<TblImportedItem>();

    public virtual ICollection<TblLeadTime> TblLeadTimes { get; set; } = new List<TblLeadTime>();

    public virtual ICollection<TblManageMaterial> TblManageMaterials { get; set; } = new List<TblManageMaterial>();

    public virtual ICollection<TblPreImportItem> TblPreImportItems { get; set; } = new List<TblPreImportItem>();

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();
}
