using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Image
{
    public string Mac { get; set; } = null!;

    public int PageId { get; set; }

    public int ImageId { get; set; }

    public int Status { get; set; }

    public string? Md5 { get; set; }

    public byte[]? Image1 { get; set; }

    public string? Led { get; set; }

    public string? Nfc { get; set; }

    public string? Button { get; set; }
}
