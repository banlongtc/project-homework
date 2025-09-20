using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class User
{
    public string User1 { get; set; } = null!;

    public string? Password { get; set; }

    public string? Rights { get; set; }

    public string? Salt { get; set; }

    public string? Role { get; set; }

    public byte[]? Img { get; set; }

    public string? Language { get; set; }

    public bool? Autoupdate { get; set; }

    public bool? Colorblind { get; set; }

    public string? Apikeyhash { get; set; }
}
