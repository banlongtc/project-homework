using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Basestationstatus
{
    public string Mac { get; set; } = null!;

    public string? IpAddress { get; set; }

    public int? Port { get; set; }

    public string? Model { get; set; }

    public string? Name { get; set; }

    public string? LanId { get; set; }

    public string? PanId { get; set; }

    public int? Channel { get; set; }

    public string? Version { get; set; }

    public string? Status { get; set; }

    public int? Esls { get; set; }

    public int? MinLoad { get; set; }

    public int? MaxLoad { get; set; }

    public bool? DynamicInts { get; set; }

    public bool? RadioTuning { get; set; }
}
