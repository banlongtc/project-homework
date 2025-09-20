using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblLocation
{
    public int IdLocation { get; set; }

    public string? LocationCode { get; set; }

    public string? LocationName { get; set; }

    public int? IdSection { get; set; }

    public int? IdLocationParent { get; set; }

    public bool? Active { get; set; }

    public bool? XuatKhac { get; set; }

    public virtual TblSection? IdSectionNavigation { get; set; }

    public virtual ICollection<TblChecksheetsUpload> TblChecksheetsUploads { get; set; } = new List<TblChecksheetsUpload>();

    public virtual ICollection<TblDivLineProd> TblDivLineProds { get; set; } = new List<TblDivLineProd>();

    public virtual ICollection<TblDivMcprod> TblDivMcprods { get; set; } = new List<TblDivMcprod>();

    public virtual ICollection<TblImportedItem> TblImportedItems { get; set; } = new List<TblImportedItem>();

    public virtual ICollection<TblLocationC> TblLocationCs { get; set; } = new List<TblLocationC>();

    public virtual ICollection<TblMasterJig> TblMasterJigs { get; set; } = new List<TblMasterJig>();

    public virtual ICollection<TblMasterMachine> TblMasterMachines { get; set; } = new List<TblMasterMachine>();

    public virtual ICollection<TblMasterPosition> TblMasterPositions { get; set; } = new List<TblMasterPosition>();

    public virtual ICollection<TblMasterTool> TblMasterTools { get; set; } = new List<TblMasterTool>();

    public virtual ICollection<TblPreImportItem> TblPreImportItems { get; set; } = new List<TblPreImportItem>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
