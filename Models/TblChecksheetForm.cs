using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetForm
{
    public int FormId { get; set; }

    public string FormName { get; set; } = null!;

    public int FormOrder { get; set; }

    public bool? IsRepeatable { get; set; }

    public int ChecksheetVersionId { get; set; }

    public bool? IsActive { get; set; }

    public string? FormPosition { get; set; }

    public string? FormType { get; set; }

    public string? JsonFormData { get; set; }

    public virtual TblChecksheetVersion ChecksheetVersion { get; set; } = null!;

    public virtual ICollection<TblChecksheetFormField> TblChecksheetFormFields { get; set; } = new List<TblChecksheetFormField>();
}
