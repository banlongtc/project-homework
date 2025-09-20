using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblLocationError
{
    public int Id { get; set; }

    public int? IdLocation { get; set; }

    public int? IdError { get; set; }
}
