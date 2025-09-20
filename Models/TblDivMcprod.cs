using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDivMcprod
{
    public int ProdMcid { get; set; }

    public string? WorkOrder { get; set; }

    public string? ProductCode { get; set; }

    public string? LotNo { get; set; }

    public int? QtyOrder { get; set; }

    public string? Character { get; set; }

    public int? IdUser { get; set; }

    public int? IdLocation { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }

    public virtual ICollection<TblDivLineMcdetail> TblDivLineMcdetails { get; set; } = new List<TblDivLineMcdetail>();

    public virtual ICollection<TblDivMaterialPrintLabel> TblDivMaterialPrintLabels { get; set; } = new List<TblDivMaterialPrintLabel>();
}
