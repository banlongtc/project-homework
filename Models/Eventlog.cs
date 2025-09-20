using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Eventlog
{
    public int Id { get; set; }

    public string? Mac { get; set; }

    public string? Trigger { get; set; }

    public string? Event { get; set; }

    public string? Value { get; set; }

    public string? Message { get; set; }

    public string? User { get; set; }

    public DateTime? Time { get; set; }
}
