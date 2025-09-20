using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblDetailWofrequency
{
    public int Id { get; set; }

    public int? WoProcessId { get; set; }

    public int? PositionId { get; set; }

    public int? FrequencyId { get; set; }

    public virtual TblTansuat? Frequency { get; set; }

    public virtual TblMasterPosition? Position { get; set; }

    public virtual TblWorkOrderProcessing? WoProcess { get; set; }
}
