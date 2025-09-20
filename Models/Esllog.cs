using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Esllog
{
    public int Id { get; set; }

    public string? Basestation { get; set; }

    public string? Esl { get; set; }

    public DateTime? Time { get; set; }

    public string? Message { get; set; }
}
