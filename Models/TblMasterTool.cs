using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblMasterTool
{
    public int Id { get; set; }

    public string? ToolCode { get; set; }

    public string? ToolName { get; set; }

    public string? Descriptions { get; set; }

    public DateTime? ModifyUpdate { get; set; }

    public int? IdLocation { get; set; }

    public virtual TblLocation? IdLocationNavigation { get; set; }
}
