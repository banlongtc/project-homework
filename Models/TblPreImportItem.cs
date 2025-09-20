using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblPreImportItem
{
    public int Id { get; set; }

    public string? WorkOrder { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public int? Qty { get; set; }

    public string? CharacterAlp { get; set; }

    public DateTime? DateImport { get; set; }

    public TimeSpan? TimeImport { get; set; }

    public DateTime? DateProd { get; set; }

    public TimeSpan? TimeProd { get; set; }

    public string? ProgressMes { get; set; }

    public int? IdUser { get; set; }

    public int? IdLocation { get; set; }

    public string? StatusEx { get; set; }

    public string? ValueJson { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }

    public virtual TblUser? IdUserNavigation { get; set; }

    public virtual ICollection<TblDetailsPreMaterial> TblDetailsPreMaterials { get; set; } = new List<TblDetailsPreMaterial>();
}
