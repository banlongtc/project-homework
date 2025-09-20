using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class JobQueue
{
    public long Id { get; set; }

    public string Queue { get; set; } = null!;

    public long JobId { get; set; }

    public DateTime? FetchedAt { get; set; }
}
