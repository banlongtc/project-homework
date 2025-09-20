using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Changelog
{
    public int Id { get; set; }

    public string? User { get; set; }

    public DateTime? Time { get; set; }

    public string? Message { get; set; }

    public string? From { get; set; }

    public string? To { get; set; }

    public string? Basestation { get; set; }

    public string? Esl { get; set; }
}
