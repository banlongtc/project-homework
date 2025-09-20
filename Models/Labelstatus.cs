using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Labelstatus
{
    public string? Id { get; set; }

    public string Mac { get; set; } = null!;

    public string? Group { get; set; }

    public string? Description { get; set; }

    public string? ImageFile { get; set; }

    public int? PollInterval { get; set; }

    public int? PollTimeout { get; set; }

    public int? ScanInterval { get; set; }

    public int? BatteryStatus { get; set; }

    public decimal? BatteryVoltage { get; set; }

    public string? Variant { get; set; }

    public string? FirmwareVersion { get; set; }

    public string? FirmwareSubversion { get; set; }

    public int? ImageId { get; set; }

    public int? ImageIdLocal { get; set; }

    public int? DisplayOptions { get; set; }

    public int? Lqi { get; set; }

    public int? LqiRx { get; set; }

    public DateTime? LastPoll { get; set; }

    public DateTime? LastInfo { get; set; }

    public DateTime? LastImage { get; set; }

    public string? BaseStation { get; set; }

    public int? ScanChannels { get; set; }

    public int? Status { get; set; }

    public int? FirmwareStatus { get; set; }

    public int? BootCount { get; set; }

    public int? Temperature { get; set; }

    public string? Lanid { get; set; }

    public string? Panid { get; set; }
}
