using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string? Message { get; set; }

    public DateTime Time { get; set; }

    public string Type { get; set; } = null!;
}
