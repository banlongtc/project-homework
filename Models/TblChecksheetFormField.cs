using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetFormField
{
    public int FieldId { get; set; }

    public int FormId { get; set; }

    public string FieldName { get; set; } = null!;

    public string LabelText { get; set; } = null!;

    public string StartCell { get; set; } = null!;

    public int RowIndex { get; set; }

    public int? ColIndex { get; set; }

    public bool? IsMerged { get; set; }

    public int? RowSpan { get; set; }

    public int? ColSpan { get; set; }

    public string? InputType { get; set; }

    public string? SectionId { get; set; }

    public string? ColClass { get; set; }

    public string? ElementId { get; set; }

    public int? SectionIndex { get; set; }

    public bool? IsHidden { get; set; }

    public string? ElementType { get; set; }

    public string? DataSource { get; set; }

    public bool? IsTotals { get; set; }

    public virtual TblChecksheetForm Form { get; set; } = null!;
}
