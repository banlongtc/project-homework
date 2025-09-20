using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Counter
{
    public string Key { get; set; } = null!;

    public long Id { get; set; }

    public int Value { get; set; }

    public DateTime? ExpireAt { get; set; }
}
