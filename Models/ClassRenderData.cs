using MPLUS_GW_WebCore.Controllers.Processing;
using System.ComponentModel.DataAnnotations.Schema;

namespace MPLUS_GW_WebCore.Models
{
    public class ClassRenderData
    {
    }

    public class RequestData
    {
        public string? JsonStr { get; set; } = string.Empty;
        public string? StrDataCheck { get; set; } = string.Empty;
        public string? DataImport { get; set; } = string.Empty;
        public string? NewFileName { get; set; } = string.Empty;
        public string? StringDate { get; set; } = string.Empty;
        public string? StringTime { get; set; } = string.Empty;
        public string? ByteFileName { get; set; } = string.Empty;
        public string? DataSave { get; set; } = string.Empty;
        public string? StrDataSub { get; set; } = string.Empty;
        public string? StrDataCheckQty { get; set; } = string.Empty;
        public string? JsonStrDivLine { get; set; } = string.Empty;
        public string? ProcessCode { get; set; } = string.Empty;
        public string? StrDataPrintLabels { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public string? PositionWorking { get; set; } = string.Empty;
        public string? ProductCode { get; set; } = string.Empty;
        public string? LotNo { get; set; } = string.Empty;
        public bool? CheckItemRG90 { get; set; } = false;

    }

    public class CustomRenderProcess
    {
        public string? IdProcess { get; set; } 
        public string? ProcessCode { get; set; }
        public string? ProcessName { get; set; }
    }

    public class InventoryQty
    {
        public string? Productcode { get; set; }
        public string? Lotno { get; set; } = string.Empty;
        public string? Quantity { get; set; } = string.Empty;
        public string? Processcode { get; set; }
        public string? Type { get; set; }

        public InventoryQty(string? _productcode, string? _lotno, string? _quantity, string? _processcode, string? _type)
        {
            Productcode = _productcode;
            Lotno = _lotno;
            Quantity = _quantity;
            Processcode = _processcode;
            Type = _type;
        }
    }

    public class ItemAbnormal
    {
        public string Isusable { get; set; }
        public ItemAbnormal(string _issuable)
        {
            Isusable = _issuable;
        }
    }

    public class ItemReserved
    {
        public string? WorkOrder { get; set; }
        public string? Type { get; set; }
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public int TimePicking { get; set; }
        public int QtyReserved { get; set; }

        public ItemReserved(string? _workorder, string? _type, string? _itemCode, string? _lotno, int _timepicking, int _qtyReserved)
        {
            WorkOrder = _workorder;
            Type = _type;
            ItemCode = _itemCode;
            LotNo = _lotno;
            TimePicking = _timepicking;
            QtyReserved = _qtyReserved;
        }
    }

    public class ListItemFlInventory
    {
        public string WorkOrder { get; set; }
        public string ProductCode { get; set; }
        public string LotNo { get; set; }
        public string Character { get; set; }
        public int QtyUsed { get; set; }
        public int QtyUnused { get; set; }
        public DateTime? DateProd { get; set; }
        public string ProcessCode { get; set; }
        public string Statusname { get; set; }
        public string InputGoodsCode { get; set; }
        public decimal InputGoodsSeq { get; set; }
        public ListItemFlInventory(string workOrder, string productCode, string lotNo, string character, int qtyUsed, int qtyUnused, DateTime? dateProd, string processCode, string statusname, string inputGoodsCode, decimal inputGoodsSeq)
        {
            WorkOrder = workOrder;
            ProductCode = productCode;
            LotNo = lotNo;
            Character = character;
            QtyUsed = qtyUsed;
            QtyUnused = qtyUnused;
            DateProd = dateProd;
            ProcessCode = processCode;
            Statusname = statusname;
            InputGoodsCode = inputGoodsCode;
            InputGoodsSeq = inputGoodsSeq;
        }
    }

    public class JsonDataItems
    {
        public string? WorkOrder { get; set; }
        public string? ItemCode { get; set; }
        public DateTime? DateImport { get; set; }
        public string? TimeImport { get; set; }
        public string? ProgressMES { get; set; }
        public string? InputGoodsCode { get; set; }
    }

    public class SearchResultRM
    {
        public string? OrderNo { get; set; }
        public string? ProcessCode { get; set; }
        public string? ProductCode { get; set; }
        public string? RMCode { get; set; }
        public int Inventory { get; set; }
        public SearchResultRM(string? _orderno, string? _processCode, 
            string? _productCode, string? _rmCode, int _inventory)
        {
            OrderNo = _orderno;
            ProcessCode = _processCode;
            ProductCode = _productCode;
            RMCode = _rmCode;
            Inventory = _inventory;
        }
    }

    public class ReturnValAfterSearchRM
    {
        public string? OrderNo { get; set; }
        public string? ProcessCode { get; set; }
        public string? LocationPacking { get; set; }
        public string? ProductCode { get; set; }
        public string? RMCode { get; set; }
        public int QtyUnused { get; set; }
        public int Inventory { get; set; }
        public int QtyCanImport { get; set; }
        public ReturnValAfterSearchRM(string? _orderno, string? _processCode, string? _location,
            string? _productCode, string? _rmCode, int _qtyUnused, int _inventory, int _qtyCanImport)
        {
            OrderNo = _orderno;
            ProcessCode = _processCode;
            LocationPacking = _location;
            ProductCode = _productCode;
            RMCode = _rmCode;
            QtyUnused = _qtyUnused;
            Inventory = _inventory;
            QtyCanImport = _qtyCanImport;
        }
    }

    public class ItemValue
    {
        public string? ItemCode { get; set; }
        public int Qty { get; set; }
        public string? UnitCode { get; set; }
        public string? PositionWH { get; set; }
        public string? LotNO { get; set; }
        public string? Remarks { get; set; }
    }
    public class ProductCodeJSData
    {
        public string? ProductCode { get; set; }
        public string? WorkOrder { get; set; }
    }

    public class JsonConvertDataEX
    {
        public string? Title { get; set; }
        public List<ItemValue>? Value { get; set; }
        public List<ProductCodeJSData>? ProductCode { get; set; }
    }
    public class SubMaterialData
    {
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int Inventory { get; set; }
        public int SafeInventory { get; set; }
        public int QtyProd { get; set; }
        public int QtyPrinted { get; set; }
        public int QtyCanInput { get; set; }
        public int InventoryPre { get; set; }
    }

    public class ListWOStatusInMes
    {
        public string? ProcessCode { get; set; }
        public string? WorkOrderNo { get; set; }
        public string? StatusName { get; set; }
    }

    public class ListWoProduction
    {
        public string SoTT { get; set; }
        public string? WorkOrderNo { get; set; }
        public string? ProductCode { get; set; }
        public string? LotNo { get; set; }
        public string? QtyProd { get; set; }
        public string? Character { get; set; }
        public DateTime? DateProd { get; set; }
        public string? TimeProd { get; set; }
        public string? ProcessCode { get; set; }
        public string? StatusName { get; set; }
        public List<ProductionLineTimeData> ProductionLines { get; set; }
    }
    public class ListWoDivLine
    {
        public string? WorkOrder { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string? Line4 { get; set; }
        public string? ChangeControl { get; set; }
        public string? Note { get; set; }

    }
    public class ListDivLineForLot
    {
        public string? WorkOrder { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string? Line4 { get; set; }
    }

    public class ItemCheck
    {
        public string? ProcessCode { get; set; }
        public string? WorkOrder { get; set; }
        public int QtyUsed { get; set; }
    }

    public class ItemLineSaved
    {
        public string? WorkOrder { get; set; }
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public int QtyUsed { get; set; }
        public string? Character { get; set; }
        public string? DateProd { get; set; }
        public TimeSpan? TimeProd { get; set; }
        public int Line1 { get; set; }
        public int Line2 { get; set; }
        public int Line3 { get; set; }
        public int Line4 { get; set; }
        public string? ChangeControl { get; set; }
        public string? Note { get; set; }
    }

    public class CheckImportMaterial
    {
        public string? ProductCode { get; set; }
        public int? Qty { get; set; }
        public int? IdRecev { get; set; }
        public string? LotNo { get; set; }
        public string? PouchNo { get; set; }
        public string? TimeLimit { get; set; }
        public string? TypeMaterial { get; set; }
        public string? PauseStatus { get; set; }
        public string? RequestNo { get; set; }
    }

    public class GetAllUsers
    {
        public int IdUser { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeNo { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Location { get; set; }
        public string? Section { get; set; }
        public string? Status { get; set; }
        public string? Deactivation { get; set; }
    }

    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
    }
}
