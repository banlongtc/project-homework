using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetEntryValue
{
    public int EntryValueId { get; set; }

    public int FormEntryId { get; set; }

    public string? JsonValue { get; set; }

    public string? JsonErrorValue { get; set; }

    public string? JsonNoteValue { get; set; }

    public virtual TblChecksheetFormEntry FormEntry { get; set; } = null!;
}
