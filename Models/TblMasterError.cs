using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterError
{
    public int Id { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorName { get; set; }

    public string? Remarks { get; set; }

    public int? Idcha { get; set; }

    public string? Location { get; set; }

    public string? NameJp { get; set; }
}
