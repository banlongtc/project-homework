using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class TblProductStaging
{
    public Guid Iditem { get; set; }

    public string? ItemCode { get; set; }

    public string? LotNo { get; set; }

    public int? Qty { get; set; }

    public string? Qrcode { get; set; }

    public string? Barcode { get; set; }

    public string? Line { get; set; }

    public string? ItemType { get; set; }

    public string? TrangThaiSp { get; set; }

    public string? ViTri { get; set; }

    public string? WorkOder { get; set; }

    public string HeThong { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? Remark { get; set; }

    public int? QtyPlan { get; set; }

    public int? QtyNg { get; set; }

    public int? QtyOk { get; set; }

    public int? QtyCdsau { get; set; }

    public int? ReserverQty { get; set; }

    public string? Image { get; set; }

    public string? ImagePath { get; set; }

    public int? TonChuaSd { get; set; }

    public string? SoMeSx { get; set; }

    public string? StatusHold { get; set; }

    public string? StatusUnderqa { get; set; }

    public string? StatusPassed { get; set; }

    public string? StatusNg { get; set; }

    public string? ChungLoaiSp { get; set; }

    public double? QtyFloat { get; set; }

    public string? TenSp { get; set; }

    public string? MaCd { get; set; }

    public string? TenCd { get; set; }

    public string? TrangThaiCd { get; set; }

    public string? NguoiThaoTac { get; set; }

    public string? HanSuDung { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? ExpiryDateWarning { get; set; }

    public string? SoThung { get; set; }

    public string? Template { get; set; }

    public string? Remark1 { get; set; }

    public string? Remark2 { get; set; }

    public string? Remark3 { get; set; }

    public string? Remark4 { get; set; }

    public string? Remark5 { get; set; }

    public int? RInt1 { get; set; }

    public int? RInt2 { get; set; }

    public int? RInt3 { get; set; }

    public double? RFloat1 { get; set; }

    public double? RFloat2 { get; set; }

    public double? RFloat3 { get; set; }

    public DateTime? RDatetime1 { get; set; }

    public DateTime? RDatetime2 { get; set; }

    public DateTime? RDatetime3 { get; set; }

    public bool? RBit1 { get; set; }

    public bool? RBit2 { get; set; }

    public bool? RBit3 { get; set; }

    public string? RImg1 { get; set; }

    public string? RImg2 { get; set; }

    public string? Delete { get; set; }
}
