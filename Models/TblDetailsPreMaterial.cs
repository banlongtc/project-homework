using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDetailsPreMaterial
{
    public int Id { get; set; }

    public string? WorkOrder { get; set; }

    public DateTime? DateImport { get; set; }

    public TimeSpan? TimeImport { get; set; }

    public string? StatusExported { get; set; }

    public int? QtyImport { get; set; }

    public int? IdItemImport { get; set; }

    public string? MaterialCode { get; set; }

    public virtual TblPreImportItem? IdItemImportNavigation { get; set; }
}
