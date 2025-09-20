using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class AggregatedCounter
{
    public string Key { get; set; } = null!;

    public long Value { get; set; }

    public DateTime? ExpireAt { get; set; }
}
