using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class LinksStaging
{
    public string? Id { get; set; }

    public string? Variant { get; set; }

    public string Mac { get; set; } = null!;

    public string? Delete { get; set; }
}
