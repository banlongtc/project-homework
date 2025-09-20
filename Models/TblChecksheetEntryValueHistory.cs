using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblChecksheetEntryValueHistory
{
    public long HistoryId { get; set; }

    public int? OriginalEntryValueId { get; set; }

    public int FormEntryId { get; set; }

    public string? JsonValue { get; set; }

    public string ActionType { get; set; } = null!;

    public string ActionBy { get; set; } = null!;

    public DateTime ActionAt { get; set; }

    public string? JsonErrorValue { get; set; }

    public string? JsonNoteValue { get; set; }
}
