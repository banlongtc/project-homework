using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class EventAction
{
    public int Id { get; set; }

    public decimal? Enabled { get; set; }

    public string? Trigger { get; set; }

    public string? Event { get; set; }

    public string? Condition { get; set; }

    public string? Action { get; set; }

    public string? Parameters { get; set; }

    public int? CooldownTime { get; set; }
}
