using ConnectMES;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MPLUS_GW_WebCore.Controllers.Admin.CreateForms;
using MPLUS_GW_WebCore.Controllers.Processing;
using MPLUS_GW_WebCore.Models;
using MPLUS_GW_WebCore.Services;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Asn1.X509;
using PdfSharp.UniversalAccessibility;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MPLUS_GW_WebCore.Controllers
{
    public class ApiController : Controller
    {
        private readonly CheckHoldingService _checkHoldingService;
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public readonly IWebHostEnvironment _environment;
        private readonly EslsystemContext _ec;
        private readonly string apiKey = "CCAE6917-323B-4B5F-A62F-56910FA3F8CF";
        private readonly string einkUrl = "https://10.239.4.40/api/esl";
        public readonly HttpClient httpClient;
        public ApiController(CheckHoldingService checkHoldingService, MplusGwContext context, Classa cl, EslsystemContext ec, IWebHostEnvironment environment)
        {
            _checkHoldingService = checkHoldingService;
            _context = context;
            _cl = cl;
            _ec = ec;
            _environment = environment;
        }

        [HttpPost]
        public IActionResult CheckHolding()
        {
            var hasData = _checkHoldingService.CheckHolding();
            var oldItemImported = _context.TblImportedItems
                .Where(x => x.TimeImport != null && x.TimeImport.Value.Date == DateTime.Now.Date)
                .Select(x => new
                {
                    x.ItemCode,
                    x.LotNo,
                    idRecev = _context.TblRecevingPlmes
                    .Where(r => r.ItemCode == x.ItemCode && r.LotNo == x.LotNo && r.OrderShipment == x.OrderShipment)
                    .Select(s => s.NewId)
                    .FirstOrDefault()
                })
                .ToList();
            return Json(new { hasDataReturn = hasData, itemImported = JsonConvert.SerializeObject(oldItemImported) });
        }

        [HttpPost]
        public IActionResult GetSubmaterials()
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var dataOld = _context.TblSubMaterials.Select(x => new
            {
                x.ProductCode,
                x.QtyProdPerDay,
                x.QtyPrintedPerRoll,
                x.InventoryPre,
                x.QtyCanInput,
                x.SafeInventory,
                x.Inventory
            }).ToList();
            return Json(new { subMaterials = dataOld, test = recurringJobs });
        }

        [HttpPost]
        public IActionResult GetReservedItem([FromBody] ItemRequest requestData)
        {
            if (requestData?.WorkOrder.ToString() == null && requestData?.ProcessCode == null)
            {
                return BadRequest(new { message = "Not Found" });
            }
            try
            {
                var workOrder = requestData?.WorkOrder.ToString();
                var processCode = requestData?.ProcessCode;
                List<RenderItemDivLine> renderItemDivLine = new();
                var loadData = _cl.Receive_WO(workOrder);
                foreach (DataRow row in loadData.Rows)
                {
                    var std = new RenderItemDivLine
                    {
                        ProductCode = row["InputItem"].ToString() ?? "",
                        LotNo = row["lotno"].ToString() ?? "",
                        Qty = Int32.Parse(row["ReservedQty"].ToString() ?? "0")
                    };
                    renderItemDivLine.Add(std);
                }
                var getOldData = _context.TblDivLineForLots
                    .AsEnumerable()
                    .Where(x => x.WorkOrder == workOrder)
                    .GroupBy(x => x.LotDivLine)
                    .Select(s => new
                    {
                        workOrder = s.FirstOrDefault()?.WorkOrder,
                        productCode = s.FirstOrDefault()?.ProductCode,
                        LotDivLine = s.Key,
                        Line1 = s.Sum(x => x.Line1),
                        Line2 = s.Sum(x => x.Line2),
                        Line3 = s.Sum(x => x.Line3),
                        Line4 = s.Sum(x => x.Line4),
                    })
                    .ToList();
                return Ok(new { dataLot = renderItemDivLine, oldData = getOldData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult CheckAndSavePosition([FromBody] RequestDataPosition requestDataPosition)
        {
            if (requestDataPosition == null)
            {
                return BadRequest("Not Found");
            }
            //Kiểm tra vị trí
            var exsitPosition = _context.TblMasterPositions
                .Where(x => x.PositionCode == requestDataPosition.PositionWorking)
                .FirstOrDefault();


            bool checkPosition = true;
            if (exsitPosition != null)
            {
                checkPosition = true;
                HttpContext.Session.SetString("PositionWorking", requestDataPosition.PositionWorking ?? "");
            }
            else
            {
                checkPosition = false;
            }
            return Ok(new { status = checkPosition, positionWorking = exsitPosition?.PositionCode });
        }

        [HttpPost]
        public IActionResult SaveDivForLot([FromBody] RequestData requestData)
        {
            if (requestData.DataSave == null) { return BadRequest(new { message = "Not Found" }); }
            List<ItemDivForLot>? itemDivForLots = JsonConvert.DeserializeObject<List<ItemDivForLot>>(requestData.DataSave);
            if (itemDivForLots != null)
            {
                foreach (var item in itemDivForLots)
                {
                    var std = new TblDivLineForLot
                    {
                        ProductCode = item.ProductCode,
                        LotDivLine = item.LotMaterial,
                        WorkOrder = item.WorkOrder,
                        Line1 = item.Line1,
                        Line2 = item.Line2,
                        Line3 = item.Line3,
                        Line4 = item.Line4,
                    };
                    _context.TblDivLineForLots.Add(std);
                    _context.SaveChanges();
                }
                var getQtyLines = _context.TblDivLineForLots
                 .GroupBy(x => x.WorkOrder)
                 .Select(x => new
                 {
                     WorkOrder = x.Key,
                     TotalLine1 = x.Sum(s => s.Line1),
                     TotalLine2 = x.Sum(s => s.Line2),
                     TotalLine3 = x.Sum(s => s.Line3),
                     TotalLine4 = x.Sum(s => s.Line4),
                 }).ToList();
                return Ok(new { message = "Lưu thành công" });
            }
            else
            {
                return BadRequest(new { message = "Not Found Data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetItemByWO([FromBody] RequestData requestData)
        {
            if (requestData.JsonStr == null)
            {
                return BadRequest(new { message = "Not Found" });
            }

            string displayName = string.Empty;
            if (HttpContext.Session.GetString("DisplayName") != null)
            {
                displayName = HttpContext.Session.GetString("DisplayName")?.ToString() ?? "";
            }

            var positionWorking = requestData.StrDataCheck ?? "";
            var workOrderRequest = requestData.JsonStr ?? "";

          

            string? workOrder = string.Empty;
            // Check workorder khi đọc workorder xem đã có trong bảng chưa
            // Nếu có sẽ phải check trạng thái xem đang trong quá trình sản xuất hay đã dừng lại
            // Gán workorder mặc định để lấy ra các thông tin cần lấy
            if (!string.IsNullOrEmpty(workOrderRequest)) {
                var existingWoProcessing = await _context.TblWorkOrderProcessings
                    .Where(x => x.Woprocessing == workOrderRequest)
                    .FirstOrDefaultAsync();
                if (existingWoProcessing != null)
                {
                    if (existingWoProcessing.ProcessingStatus == "In Processing")
                    {
                        workOrder = existingWoProcessing.Woprocessing;
                    }
                    else if (existingWoProcessing.ProcessingStatus == "Production end")
                    {
                        return StatusCode(500, new { message = "Chỉ thị đã được dừng lại. Vui lòng liên hệ leader hoặc đọc chỉ thị mới!" });
                    }
                } else
                {
                    workOrder = workOrderRequest;
                }
            } else
            {
                // Lấy workorder nếu có tại vị trí
                var getWorkOrderProcessing = await (
                    from s in _context.TblWorkOrderProcessings
                    where s.PositionCode == positionWorking && s.ProcessingStatus != "Production end"
                    select s.Woprocessing).FirstOrDefaultAsync();
                workOrder = getWorkOrderProcessing != null ? getWorkOrderProcessing.ToString() : workOrderRequest;
            }

            // Lấy thông tin chủng loại, lô khi đọc máng
            string? productCode = requestData.ProductCode;
            string? lotNo = requestData.LotNo;

            var getWOMes = _context.TblWorkOrderMes
                .Where(x => x.ItemCode == productCode && x.LotNo == lotNo)
                .FirstOrDefault();
            if (getWOMes != null)
            {
                workOrder = getWOMes.WorkOrder;
            }
            // Lấy thông tin chia line của chủng loại
            var resultWo = _context.TblDivLineProds
              .Where(x => x.WorkOrder == workOrder)
              .Select(x => new
              {
                  workOrder = x.WorkOrder,
                  itemCode = x.ItemCode,
                  lotNo = x.LotNo,
                  qtyUsed = x.QtyUsed,
                  line1 = x.Line1,
                  line2 = x.Line2,
                  line3 = x.Line3,
                  line4 = x.Line4,
              }).FirstOrDefault();
            if (resultWo == null)
            {
                return Ok(new { status = false, message = "Chỉ thị này chưa được chia line cho sản xuất. Vui lòng kiểm tra lại!" });
            }
            var existItem = _context.TblDivLineForLots.Where(x => x.WorkOrder == workOrder).FirstOrDefault();
            if (existItem == null)
            {
                return Ok(new { status = false, message = "Chỉ thị này chưa được chia NVL hoặc đã kết thúc. Vui lòng kiểm tra lại!" });
            }

            var idLine = _context.TblMasterPositions
                          .Where(x => x.PositionCode == positionWorking)
                          .Select(s => s.IdLine)
                          .FirstOrDefault();
            var lineCode = _context.TblProdLines
                .Where(x => x.IdLine == idLine)
                .Select(s => s.LineCode)
                .FirstOrDefault();
            int? _qtyInLine = 0;
            _qtyInLine = idLine switch
            {
                1 => resultWo.line1,
                2 => resultWo.line2,
                3 => resultWo.line3,
                4 => resultWo.line4,
                _ => 0,
            };

            // Xử lý thêm vào WoProcessing khi đọc workOrder mới
            var idProduct = _context.TblMasterProductItems
                .Where(x => x.ItemCode == resultWo.itemCode)
                .Select(s => s.IdItem)
                .FirstOrDefault();
            if (idProduct < 0)
            {
                return Ok(new { status = false, message = "Chưa có sản phẩm này trong đăng ký Master. Vui lòng kiểm tra lại!" });
            }

            var idLocationChild = _context.TblMasterPositions
                          .Where(x => x.PositionCode == positionWorking)
                          .Select(s => s.LocationChildId)
                          .FirstOrDefault();

            // Kiểm tra item này có được đi qua vị trí này không
            var getLocationChildCode = _context.TblLocationCs.Where(x => x.Id == idLocationChild)
                .Select(x => x.LocationCodeC)
                .FirstOrDefault();
            var existingItemInLocation = _context.TblItemLocations
                .Where(x => x.ItemCode == resultWo.itemCode &&
                x.LocationCode == getLocationChildCode).FirstOrDefault();

            if(existingItemInLocation == null)
            {
                return Ok(new { status = false, message = "Sản phẩm này không được sản xuất trên vị trí này. Vui lòng thực hiện chỉ thị khác." });
            }

            var existingFrequency = _context.TblLocationTansuats
                .Where(x => x.IdLocationc == idLocationChild)
                .ToList();

            var existingWOProcessing = _context.TblWorkOrderProcessings
               .Where(x => x.Woprocessing == resultWo.workOrder && x.PositionCode == positionWorking)
               .FirstOrDefault();

            if (existingWOProcessing == null)
            {
                var newProductProduction = new TblWorkOrderProcessing
                {
                    Woprocessing = resultWo.workOrder,
                    ProductCode = resultWo.itemCode,
                    LotProcessing = resultWo.lotNo,
                    QtyTotal = resultWo.qtyUsed,
                    ProcessingStatus = "In Processing",
                    PositionCode = positionWorking,
                    StartAt = DateTime.Now,
                    EndAt = null,
                    NextAction = existingFrequency.Count > 0 ? "Check Conditions" : "Read Materials",
                };
                _context.TblWorkOrderProcessings.Add(newProductProduction);
            }
            else
            {
                existingWOProcessing.StartAt = DateTime.Now;
            }
            _context.SaveChanges();

            var existingWoCondition = await (from dw in _context.TblDetailWofrequencies
                                             join wps in _context.TblWorkOrderProcessings on dw.WoProcessId equals wps.Id
                                             join pos in _context.TblMasterPositions on dw.PositionId equals pos.IdPosition
                                             where wps.Woprocessing == workOrder && pos.PositionCode == positionWorking
                                             select new
                                             {
                                                 dw.FrequencyId
                                             }).ToListAsync();

            var nextAction = _context.TblWorkOrderProcessings
              .Where(x => x.Woprocessing == workOrder)
              .Select(s => s.NextAction)
              .FirstOrDefault();

            // lấy dữ liệu đã nhập với tại workorder này
            var dataEntried = await (from d in _context.TblChecksheetFormEntries
                                     join e in _context.TblChecksheetEntryValues on d.FormEntryId equals e.FormEntryId
                                     where d.WorkOrderCode == workOrder &&
                                     d.PositionCode == positionWorking && 
                                     d.TrayNo != null
                                     select new
                                     {
                                         e.JsonValue,
                                         d.EntryIndex,
                                         d.ProcessStatus,
                                         d.ChecksheetVerId,
                                         d.QtyOfReads,
                                         d.QtyProduction,
                                         d.QtyOk,
                                         d.QtyNg,
                                         d.TrayNo,
                                         d.CreatedBy,
                                         d.FormEntryId,
                                     }).GroupBy(x => x.TrayNo)
                                     .Select(s => new
                                     {
                                         trayNo = s.Key,
                                         qtyOfRead = s.Select(g => g.QtyOfReads).FirstOrDefault(),
                                         qtyProduction = s.Sum(g => g.QtyProduction),
                                         qtyOK = s.Sum(g => g.QtyOk),
                                         qtyNG = s.Sum(g => g.QtyNg),
                                         entryIndex = s.Select(g => g.EntryIndex).Max(),
                                         formEntryIndex = s.Select(g => g.FormEntryId).Max(),
                                         processEntryStatus = s.Select(g => g.ProcessStatus).ToList(),
                                         processWOStatus = _context.TblWorkOrderProcessings.Where(x => x.Woprocessing == workOrder).Select(s => s.ProcessingStatus).FirstOrDefault(),
                                         jsonSaved = s.Where(x => x.CreatedBy == displayName && x.TrayNo == s.Key).Select(g => g.JsonValue).FirstOrDefault()
                                     }).ToListAsync();

            // Lấy thông tin sản phẩm có gán với version checksheet nào ko
            var itemAssignments = await (from s in _context.TblChecksheetItemAssignments
                                         where s.ProductItem == resultWo.itemCode && s.ProductLot == resultWo.lotNo
                                         select new
                                         {
                                             s.ItemAssignmentId,
                                             s.ChecksheetId,
                                             s.LastUsedChecksheetVersionId,
                                             s.IsChecksheetCondition,
                                             checksheetCode = _context.TblChecksheetsUploads.Where(x => x.ChecksheetId == s.ChecksheetId).Select(s => s.ChecksheetCode).FirstOrDefault(),
                                         }).FirstOrDefaultAsync();

            // Lấy checksheet được gán theo vị trí
            var idPositionMaster = _context.TblMasterPositions.Where(x => x.PositionCode == positionWorking)
                .Select(s => s.IdPosition).FirstOrDefault();
            var checksheetInWorkstation = await _context.TblChecksheetWorkstationAssignments
                .Where(x => x.WorkstationId == idPositionMaster)
                .Select(s => new
                {
                    s.LastUsedChecksheetVersionId,
                    s.ChecksheetId,
                    s.IsChecksheetCondition,
                    checksheetCode = _context.TblChecksheetsUploads.Where(x => x.ChecksheetId == s.ChecksheetId).Select(s => s.ChecksheetCode).FirstOrDefault(),
                }).ToListAsync();

            // Lấy thông tin WO được đọc vào
            var infoProductionWithWO = _context.TblWorkOrderMes
                .Where(x => x.WorkOrder == workOrder)
                .Select(s => new
                {
                    workOrder = s.WorkOrder,
                    productCode = s.ItemCode,
                    lotNo = s.LotNo,
                    qtyOrder = s.QtyWo,
                    qtyInLine = _qtyInLine,
                    line = lineCode,
                    year = DateTime.Now.Year,
                    frequencyIds = existingWoCondition,
                }).FirstOrDefault();


            return Ok(new
            {
                status = true,
                renderData = infoProductionWithWO,
                currentAction = nextAction,
                dataEntries = dataEntried,
                csitemAssignments = itemAssignments,
                csWorkstation = checksheetInWorkstation,
            });
        }

        /// <summary>
        /// Cập nhật WO processing có tần suất kiểm tra như thế nào
        /// </summary>
        /// <param name="requestDataSave"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateFrequency([FromBody] RequestDataSave requestDataSave)
        {
            if (requestDataSave == null)
            {
                return BadRequest(new { message = "Không có dữ liệu được truyền lên" });
            }

            string workOrder = requestDataSave.WorkOrderProd ?? "";
            string postionWorking = requestDataSave.PositionWorking ?? "";
            int frequencyId = requestDataSave.FrequencyId ?? 0;
            string? requestType = requestDataSave.RequestType ?? "";
            // Check wo processing đã đọc điều kiện chưa
            var woProcessing = _context.TblWorkOrderProcessings
                .Where(x => x.Woprocessing == workOrder)
                .FirstOrDefault();
            int woProcessingId = 0;
            if (woProcessing != null)
            {
                woProcessingId = woProcessing.Id;
                woProcessing.NextAction = "Read Conditions";
            }
            _context.SaveChanges();

            var positionId = _context.TblMasterPositions
                .Where(x => x.PositionCode == postionWorking)
                .Select(s => s.IdPosition)
                .FirstOrDefault();
            var existingDetail = _context.TblDetailWofrequencies
                .Where(x => x.WoProcessId == woProcessingId && x.PositionId == positionId && x.FrequencyId == frequencyId)
                .FirstOrDefault();

            if(existingDetail == null)
            {
                var newDetailProcessing = new TblDetailWofrequency
                {
                    WoProcessId = woProcessingId,
                    PositionId = positionId,
                    FrequencyId = frequencyId
                };
                _context.TblDetailWofrequencies.Add(newDetailProcessing);
                _context.SaveChanges();
            } 
           
            
            return Ok(new { message = "Update successed" });
        }

        [HttpPost]
        public IActionResult GetQtyInLine([FromBody] RequestCheckMaterial requestData)
        {
            if (requestData.MaterialCode == null
                && requestData.LotMaterial == null
                && requestData.PouchNo == null && requestData.TimeLimit == null && requestData.WorkOrder == null)
            {
                return BadRequest(new { message = "Not Found" });
            }
            string workOrderProd = requestData.WorkOrder ?? "";
            string materialCode = requestData.MaterialCode ?? "";
            string timeLimit = requestData.TimeLimit ?? "";
            string lotMaterial = requestData.LotMaterial ?? "";
            string pouchNo = requestData.PouchNo ?? "";
            var resultWo = _context.TblDivLineForLots
                .Where(x => x.WorkOrder == workOrderProd
                && materialCode == x.ProductCode
                && x.LotDivLine == lotMaterial)
                .Select(x => new
                {
                    workOrder = x.WorkOrder,
                    productCode = x.ProductCode,
                    lotMaterial = x.LotDivLine,
                    line1 = x.Line1,
                    line2 = x.Line2,
                    line3 = x.Line3,
                    line4 = x.Line4,
                }).GroupBy(x => new { 
                    x.workOrder,
                    x.productCode,
                    x.lotMaterial,
                }).Select(s => new
                {
                    WorkOrder = s.Key.workOrder,
                    ProductCode = s.Key.productCode,
                    LotMaterial = s.Key.lotMaterial,
                    line1 = s.Sum(l => l.line1),
                    line2 = s.Sum(l => l.line2),
                    line3 = s.Sum(l => l.line3),
                    line4 = s.Sum(l => l.line4),
                }).ToList();
            bool checkPouchNo = false;
            var checkTimeLimit = _context.TblImportedItems
                .Where(x => x.ItemCode == materialCode && x.LotNo == lotMaterial && timeLimit == x.TimeSterilization)
                .FirstOrDefault();
            if (checkTimeLimit == null)
            {
                resultWo = null;
            }

            return Ok(new { renderData = resultWo, checkPouch = checkPouchNo });
        }

        [HttpPost]
        public IActionResult GetFormConfig([FromBody] RequestDataGetForm requestDataGetForm)
        {
            string workOrder = requestDataGetForm.WorkOrder ?? "";
            string productCode = requestDataGetForm.ProductCode ?? "";
            string productLot = requestDataGetForm.ProductLot ?? ""; 
            string positionWorkingRequest = requestDataGetForm.PositionWorking ?? "";
            string formType = requestDataGetForm.FormType ?? "";
            int? checksheetVerId = requestDataGetForm.ChecksheetVerId;

            var partFormType = formType.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var lowerFormTypes = partFormType.Select(p => p.ToLower()).ToList();

            var jsonFormFields = _context.TblChecksheetForms
                .AsEnumerable()
                .Where(x => x.ChecksheetVersionId == checksheetVerId &&
                x.JsonFormData != null &&
                x.FormType != null &&
                lowerFormTypes.Any(p => x.FormType.ToLower().Contains(p)))
                .Select(s => new
                {
                    formId = s.FormId,
                    s.ChecksheetVersionId,
                    sections = s.JsonFormData
                }).ToList();

            var allElements = new List<Dictionary<string, object>>();
            foreach (var json in jsonFormFields)
            {
                var sections = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json.sections ?? "");
                if(sections != null)
                {
                    foreach (var section in sections)
                    {
                        if (section.TryGetValue("rows", out var rowsObj) && rowsObj is JsonElement rowsElement)
                        {
                            foreach (var row in rowsElement.EnumerateArray())
                            {
                                var colsElement = row.GetProperty("cols");
                                foreach (var col in colsElement.EnumerateArray())
                                {
                                    var elementsElement = col.GetProperty("elements");
                                    foreach (var element in elementsElement.EnumerateArray())
                                    {
                                        var elementDict = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                                        if(elementDict != null)
                                        {
                                            allElements.Add(elementDict);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var idLine = _context.TblMasterPositions
                         .Where(x => x.PositionCode == positionWorkingRequest)
                         .Select(s => s.IdLine)
                         .FirstOrDefault();
            var lineCode = _context.TblProdLines
                .Where(x => x.IdLine == idLine)
                .Select(s => s.LineCode)
                .FirstOrDefault();
            int? _qtyInLine = 0;
            _qtyInLine = idLine switch
            {
                1 => _context.TblDivLineProds
                                        .Where(x => x.WorkOrder == workOrder)
                                        .Select(x => x.Line1).FirstOrDefault(),
                2 => _context.TblDivLineProds
                                       .Where(x => x.WorkOrder == workOrder)
                                       .Select(x => x.Line2).FirstOrDefault(),
                3 => _context.TblDivLineProds
                                       .Where(x => x.WorkOrder == workOrder)
                                       .Select(x => x.Line3).FirstOrDefault(),
                4 => _context.TblDivLineProds
                                       .Where(x => x.WorkOrder == workOrder)
                                       .Select(x => x.Line4).FirstOrDefault(),
                _ => 0,
            };

            string displayName = string.Empty;
            if (HttpContext.Session.GetString("DisplayName") != null)
            {
                displayName = HttpContext.Session.GetString("DisplayName")?.ToString() ?? "";
            }

            var infoProductionWithWO = _context.TblWorkOrderMes
               .Where(x => x.WorkOrder == workOrder)
               .Select(s => new
               {
                   workOrder = s.WorkOrder,
                   productCode = s.ItemCode,
                   lotNo = s.LotNo,
                   qtyOrder = s.QtyWo,
                   qtyInLine = _qtyInLine,
                   line = lineCode,
                   year = DateTime.Now.Year,
               }).FirstOrDefault();

            // Thông tin người dùng và vị trí thực hiện
            var infoPositionWorking = new
            {
                personWorking = displayName,
                positionWorking = positionWorkingRequest,
                timeWorking = DateTime.Now.ToString("dd/MM HH:mm"),
            };

            // Thông tin người dùng và thời gian thực hiện
            var infoPersonWorking = new
            {
                positionWorking = positionWorkingRequest,
                personWorking = displayName,
                timeWorking = DateTime.Now.ToString("dd/MM HH:mm"),
            };


            // Tiêu chuẩn đường kính ngoài của sản phẩm 
            var getProductCode = _context.TblWorkOrderMes
                .Where(x => x.WorkOrder == workOrder)
                .Select(s => s.ItemCode).FirstOrDefault();
            var getValueDiametersInch = (from s in _context.TblItemValTcs
                                         join ms in _context.TblMasterTcs on s.IdNhomTc equals ms.IdTc
                                         join dt in _context.TblDetailTcs on s.IdValTc equals dt.IdDetail
                                         where ms.TcCode == "DKN" && s.ItemCode == getProductCode && dt.ValueUnit == "inch"
                                         select dt.ValueDecimal).FirstOrDefault();
            var getValueDiametersMinMM = (from s in _context.TblItemValTcs
                                          join ms in _context.TblMasterTcs on s.IdNhomTc equals ms.IdTc
                                          join dt in _context.TblDetailTcs on s.IdValTc equals dt.IdDetail
                                          where ms.TcCode == "DKN" && s.ItemCode == getProductCode && dt.ValueUnit == "mm"
                                          && dt.MoTa == "Min"
                                          select dt.ValueDecimal).FirstOrDefault();
            var getValueDiametersMaxMM = (from s in _context.TblItemValTcs
                                          join ms in _context.TblMasterTcs on s.IdNhomTc equals ms.IdTc
                                          join dt in _context.TblDetailTcs on s.IdValTc equals dt.IdDetail
                                          where ms.TcCode == "DKN" && s.ItemCode == getProductCode && dt.ValueUnit == "mm"
                                          && dt.MoTa == "Max"
                                          select dt.ValueDecimal).FirstOrDefault();
            var infoStdDiameter = new
            {
                stdOuterDiameterInch = getValueDiametersInch,
                stdOuterDiameterMM = $"{(getValueDiametersMinMM.HasValue ? getValueDiametersMinMM.Value.ToString("F2").Replace(",", ".") : "")}~{(getValueDiametersMaxMM.HasValue ? getValueDiametersMaxMM.Value.ToString("F2").Replace(",", ".") : "")}"
            };

            var _dataBlinds = new Dictionary<string, object>();
            foreach (var field in allElements)
            {
                string dataSource = field.TryGetValue("dataSource", out var strSource) ? strSource?.ToString() ?? string.Empty : string.Empty;
                string fieldName = field.TryGetValue("fieldName", out var fieldNameStr) ? fieldNameStr?.ToString() ?? string.Empty : string.Empty;
                if (!string.IsNullOrEmpty(dataSource))
                {
                    var parts = dataSource.Split('.');

                    string objectName = parts[0].Trim();
                    string propertyName = parts[1].Trim();

                    object? source = parts[0] switch
                    {
                        "InfoProduction" => infoProductionWithWO,
                        "InfoPositionWorking" => infoPositionWorking,
                        "StandardDiameter" => infoStdDiameter,
                        "InfoPersons" => infoPersonWorking,
                        _ => null
                    };

                    if (source != null)
                    {
                        var prop = source.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public |
                                                 System.Reflection.BindingFlags.Instance |
                                                 System.Reflection.BindingFlags.IgnoreCase);
                        _dataBlinds[fieldName] = prop?.GetValue(source) ?? "";
                    }
                }
            }

            return Ok(new { formFields = jsonFormFields, dataBlinds = _dataBlinds });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusWOProcessing([FromBody] RequestDataSave requestDataSave)
        {
            if(requestDataSave == null)
            {
                return BadRequest(new { message = "Dữ liệu đẩy lên trống. Vui lòng kiểm tra lại!" });
            }
            string? workorder = requestDataSave.WorkOrderProd;
            string? positionWorking = requestDataSave.PositionWorking;

            string? userWorking = HttpContext.Session.GetString("DisplayName")?.ToString();

            var woProcessingItem = await _context.TblWorkOrderProcessings
                .Where(x => x.Woprocessing == workorder && x.PositionCode == positionWorking)
                .FirstOrDefaultAsync();
           
            if (woProcessingItem != null)
            {
                woProcessingItem.NextAction = "Enter production results";
            }

            // Cập nhật số lượng của đã thao tác của lần nhập
            if(requestDataSave.TrayNo != null)
            {
                var formEntryQty = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workorder && 
                    positionWorking == x.PositionCode && 
                    x.TrayNo == requestDataSave.TrayNo &&
                    x.QtyOfReads != null && x.QtyOfReads >= requestDataSave.QtyProcessing && 
                    x.CreatedBy == userWorking)
                    .FirstOrDefaultAsync();

                var checksheetVerId = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workorder &&
                    positionWorking == x.PositionCode &&
                    x.TrayNo == requestDataSave.TrayNo)
                    .Select(s => s.ChecksheetVerId).FirstOrDefaultAsync();

                var itemAssignmentId = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workorder &&
                    positionWorking == x.PositionCode &&
                    x.TrayNo == requestDataSave.TrayNo)
                    .Select(s => s.ItemAssignmentId).FirstOrDefaultAsync();

                 var qtyOfReads = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workorder &&
                    positionWorking == x.PositionCode &&
                    x.TrayNo == requestDataSave.TrayNo)
                    .Select(s => s.QtyOfReads).FirstOrDefaultAsync();

                var maxEntryIndex = _context.TblChecksheetFormEntries
                      .Where(fe => fe.ChecksheetVerId == checksheetVerId &&
                      fe.WorkOrderCode == workorder &&
                      fe.PositionCode == positionWorking &&
                      fe.TrayNo == requestDataSave.TrayNo)
                      .Max(fe => (int?)fe.EntryIndex) ?? 0;

                if (formEntryQty != null)
                {
                    formEntryQty.QtyProduction = formEntryQty.QtyProduction != null ? formEntryQty.QtyProduction + requestDataSave.QtyProcessing : requestDataSave.QtyProcessing;
                    var logEntry = _context.TblChecksheetFormEntryHistories
                        .Where(x => x.OriginalFormEntryId == formEntryQty.FormEntryId)
                        .FirstOrDefault();
                    if(logEntry != null)
                    {
                        logEntry.QtyProduction = formEntryQty.QtyProduction != null ? formEntryQty.QtyProduction + requestDataSave.QtyProcessing : requestDataSave.QtyProcessing;
                    }
                } else
                {
                    var newEntry = new TblChecksheetFormEntry
                    {
                        ChecksheetVerId = checksheetVerId,
                        ItemAssignmentId = itemAssignmentId,
                        WorkOrderCode = workorder ?? "",
                        PositionCode = positionWorking,
                        EntryIndex = maxEntryIndex + 1,
                        ProcessStatus = "Continue",
                        CreatedAt = DateTime.Now,
                        CreatedBy = userWorking ?? "",
                        QtyOfReads = qtyOfReads,
                        QtyProduction = requestDataSave.QtyProcessing,
                        TrayNo = requestDataSave.TrayNo,
                    };
                    _context.TblChecksheetFormEntries.Add(newEntry);
                    _context.SaveChanges();
                    _context.TblChecksheetFormEntryHistories.Add(new TblChecksheetFormEntryHistory
                    {
                        OriginalFormEntryId = newEntry.FormEntryId,
                        ChecksheetVerId = checksheetVerId,
                        ItemAssignmentId = itemAssignmentId,
                        WorkOrderCode = workorder ?? "",
                        PositionCode = positionWorking,
                        EntryIndex = maxEntryIndex + 1,
                        Status = "Update Status",
                        CreatedAt = DateTime.Now,
                        CreatedBy = userWorking ?? "",
                        QtyOfReads = qtyOfReads,
                        QtyProduction = requestDataSave.QtyProcessing,
                        TrayNo = requestDataSave.TrayNo,
                        ActionAt = DateTime.Now,
                        ActionBy = userWorking ?? "",
                        ActionType = "Continue Production"
                    });
                }
            }
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật thành công" });
        }
        public static int? GetLastNumberFromColClass(string colClass)
        {
            if (string.IsNullOrEmpty(colClass))
            {
                return null;
            }

            int lastHyphenIndex = colClass.LastIndexOf('-');
            if (lastHyphenIndex != -1 && lastHyphenIndex < colClass.Length - 1)
            {
                string numberString = colClass[(lastHyphenIndex + 1)..];
                if (int.TryParse(numberString, out int number))
                {
                    return number;
                }
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra thẻ này đang được dùng chưa
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckEink([FromBody] RequestData requestData)
        {
            if (requestData.JsonStr == null)
            {
                return BadRequest(new { message = "Not Found" });
            }
            try
            {
                string macEink = requestData.JsonStr;
                var existEink = _ec.Links.Where(x => x.Mac == macEink).FirstOrDefault();
                if (existEink != null)
                {
                    return StatusCode(500, new { message = "Đã liên kết trước đó. Vui lòng chọn thẻ khác." });
                }
                else
                {
                    return Ok(new { status = true, message = "Được phép liên kết thẻ đó. Tiếp tục xác nhận." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> CheckConditionPreOperation([FromBody] RequestDataSave requestDataConditions)
        {
            try
            {
                if (requestDataConditions == null)
                {
                    return BadRequest(new { message = "Request from client is empty" });
                }
                //Convert json object thành list
                List<DataCheckCondition>? conditionItems = JsonConvert.DeserializeObject<List<DataCheckCondition>>(requestDataConditions.CheckConditions ?? "");
   
                string? userWorking = HttpContext.Session.GetString("DisplayName")?.ToString();
                string? workOrder = requestDataConditions.WorkOrderProd;
                string? checksheetCode = requestDataConditions.ChecksheetCode;
                int? checksheetVerionId = requestDataConditions.ChecksheetVersionId;
                string? positionWorking = requestDataConditions.PositionWorking;
                string? requestType = requestDataConditions.RequestType;
                int? itemAssignmentId = requestDataConditions.ItemAssignmentId;

                string errorMessage = string.Empty;
                string errorLabelText = string.Empty;

                string nextAction = "Read Materials";
                
                if(requestType == "Read Condition Add-On")
                {
                    nextAction = "Enter production results";
                } else if(requestType == "New Conditions Add-On")
                {
                    nextAction = "Read Materials";
                }

                string entryStatus = "Condition Entered";

                if (conditionItems != null)
                {
                    foreach (var conditionItem in conditionItems)
                    {
                        // Đảm bảo conditionItem không null (nếu có thể null từ nguồn dữ liệu)
                        if (conditionItem == null)
                        {
                            return Ok(new
                            {
                                status = false,
                                message = "Điều kiện bị thiếu. Vui lòng kiểm tra lại.",
                                labelText = ""
                            });
                        }

                        // Kiểm tra xem Value của conditionItem có rỗng không
                        if (string.IsNullOrEmpty(conditionItem.Value) && requestType == string.Empty)
                        {
                            return Ok(new
                            {
                                status = false,
                                message = $"Dữ liệu {conditionItem.Label} trống. Vui lòng kiểm tra lại.",
                                labelText = conditionItem.Label
                            });
                        }
                    }

                    foreach (var conditionItem in conditionItems)
                    {
                        string currentLabel = conditionItem.Label ?? "";
                        var partLabel = currentLabel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var infoTools = _context.TblMasterTools
                                .Where(x => x.ToolCode == conditionItem.Value)
                                .AsEnumerable() 
                                .Where(x => x.ToolName != null && partLabel.All(p => x.ToolName.Contains(p, StringComparison.OrdinalIgnoreCase)))
                                .Select(s => new ToolInfo
                                {
                                    ToolCode = s.ToolCode,
                                    ToolName = s.ToolName,
                                }).ToList();
                        if (!infoTools.Any())
                        {
                            errorMessage = "Không tìm thấy " + currentLabel + " trong Master đã đăng ký. Vui lòng kiểm tra lại.";
                            errorLabelText = currentLabel;
                            break;
                        }
                    }
                    if (errorMessage != "")
                    {
                        return Ok(new
                        {
                            status = false,
                            message = errorMessage,
                            labelText = errorLabelText
                        });
                    }
                    else
                    {
                        foreach (var conditionItem in conditionItems)
                        {
                            string currentLabel = conditionItem.Label ?? "";
                            var partLabel = currentLabel.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            var infoToolsToSave = _context.TblMasterTools
                                .Where(x => x.ToolCode == conditionItem.Value)
                                .AsEnumerable()
                                .Where(x => x.ToolName != null && partLabel.All(p => x.ToolName.Contains(p, StringComparison.OrdinalIgnoreCase)))
                                .Select(s => new ToolInfo
                                {
                                    ToolCode = s.ToolCode,
                                    ToolName = s.ToolName,
                                }).ToList();
                            if (infoToolsToSave.Any())
                            {
                                continue;
                            }
                            else
                            {
                                return Ok(new
                                {
                                    status = false,
                                    message = $"Lỗi khi lưu dữ liệu cho {currentLabel}. Vui lòng thử lại.",
                                    labelText = currentLabel
                                });
                            }
                        }
                    }
                    await CreateEntryDataConditions(checksheetVerionId, requestDataConditions.DataSaveMapping ?? "", userWorking ?? "", workOrder ?? "", positionWorking ?? "", itemAssignmentId, nextAction, entryStatus);
                    return Ok(new
                    {
                        status = true,
                        message = "Tất cả điều kiện đã được kiểm tra và lưu thành công.",
                    });
                } else
                {
                    return StatusCode(500, new { message = "Dữ liệu kiểm tra điều kiện với master trống không thực hiện được!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lưu tạo máng
        /// </summary>
        /// <param name="requestDataSaveTray"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveTrayPreCheck([FromBody] RequestDataSave requestDataSaveTray)
        {
            if (requestDataSaveTray == null)
            {
                return BadRequest(new { message = "Request is not found" });
            }

            string? userWorking = HttpContext.Session.GetString("DisplayName")?.ToString();

            string? workOrderProd = requestDataSaveTray.WorkOrderProd;
            string? jsonDataSave = requestDataSaveTray.JsonSaveDb;
            string? positionWorking = requestDataSaveTray.PositionWorking;
            int? checksheetVerId = requestDataSaveTray.ChecksheetVersionId;
            int? itemAssignmentId = requestDataSaveTray.ItemAssignmentId;
            int? qtyOfReads = requestDataSaveTray.QtyOfReads;
            string? trayNo = requestDataSaveTray.TrayNo;

            string nextAction = "Working Wires";
            string entryStatus = "Created tray";
            try
            {
                await CreatedEntry(checksheetVerId, jsonDataSave ?? "", trayNo ?? "", userWorking ?? "", 
                    workOrderProd ?? "", positionWorking ?? "", itemAssignmentId, nextAction, entryStatus, qtyOfReads);
                var maxFormEntry = _context.TblChecksheetFormEntries
                .Where(fe => fe.ChecksheetVerId == checksheetVerId &&
                fe.WorkOrderCode == workOrderProd &&
                fe.PositionCode == positionWorking)
                .Max(fe => (int?)fe.FormEntryId) ?? 0;
                return Ok(new { status = true, formEntryId = maxFormEntry });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveAbnormal([FromBody] RequestDataSave requestDataSave)
        {
            if (requestDataSave == null)
            {
                return BadRequest(new { message = "Request is not found" });
            }

            string? userWorking = HttpContext.Session.GetString("DisplayName")?.ToString();

            string? workOrderProd = requestDataSave.WorkOrderProd;
            string? jsonDataSave = requestDataSave.JsonSaveDb;
            string? positionWorking = requestDataSave.PositionWorking;
            string? trayNo = requestDataSave.TrayNo;
            int? checksheetVerId = requestDataSave.ChecksheetVersionId;
            int? itemAssignmentId = requestDataSave.ItemAssignmentId;
            int? qtyRead = requestDataSave.QtyOfReads;
            int? qtyProcessing = requestDataSave.QtyProcessing;
            int? qtyOK = requestDataSave.QtyOK;
            int? qtyNG = requestDataSave.QtyNG;
            string? jsonErrorValue = requestDataSave.ErrorInfo;
            int? formEntryId = requestDataSave.FormEntryId;

            string nextAction = "Leader Check Abnormal";
            string entryStatus = "Checked Abnormal";
            try
            {
                var woProcessing = _context.TblWorkOrderProcessings
                  .Where(x => x.Woprocessing == workOrderProd)
                  .FirstOrDefault();

                if (woProcessing != null)
                {
                    woProcessing.ProcessingStatus = "Paused";
                    woProcessing.NextAction = nextAction;
                }
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(jsonDataSave) || !string.IsNullOrEmpty(jsonErrorValue))
                {
                    await SaveDataAbnormal(checksheetVerId, jsonDataSave ?? "", trayNo, userWorking ?? "", workOrderProd ?? "", positionWorking ?? "", itemAssignmentId, nextAction, entryStatus, qtyRead, qtyProcessing, qtyOK, qtyNG, jsonErrorValue, formEntryId);
                }

                return Ok(new { status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        /// <summary>
        /// 
        /// Tạo dữ liệu nhập của điều kiện theo lần nhập
        /// 
        /// </summary>
        /// <param name="checksheetVerionId"></param>
        /// <param name="jsonDataSave"></param>
        /// <param name="userWorking"></param>
        /// <param name="workOrder"></param>
        /// <param name="positionWorking"></param>
        /// <param name="itemAssignmentId"></param>
        /// <param name="nextAction"></param>
        /// <param name="entryStatus"></param>
        /// <returns>Không trả về mà sẽ được lưu vào logs</returns>
        private async Task CreateEntryDataConditions(int? checksheetVerionId, string jsonDataSave, string userWorking, string workOrder, string positionWorking, int? itemAssignmentId, string nextAction, string entryStatus)
        {
            try
            {
                // Cập nhật trạng thái chỉ thị đang sản xuất
                var woProcessing = await _context.TblWorkOrderProcessings
                    .Where(x => x.Woprocessing == workOrder)
                    .FirstOrDefaultAsync();

                if (woProcessing != null)
                {
                    woProcessing.NextAction = nextAction;
                }

                TblChecksheetFormEntry formEntry = new ();
                TblChecksheetEntryValue entryValue = new();
                // Lấy maxEntryIndex để 
                var maxEntryIndex = _context.TblChecksheetFormEntries
                    .Where(fe => fe.ChecksheetVerId == checksheetVerionId &&
                    fe.WorkOrderCode == workOrder &&
                    fe.PositionCode == positionWorking)
                    .Max(fe => (int?)fe.EntryIndex) ?? 0;
                formEntry = new TblChecksheetFormEntry
                {
                    ChecksheetVerId = checksheetVerionId ?? 0,
                    ItemAssignmentId = itemAssignmentId,
                    WorkOrderCode = workOrder,
                    EntryIndex = maxEntryIndex + 1,
                    ProcessStatus = entryStatus,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userWorking,
                    PositionCode = positionWorking,
                };
                _context.TblChecksheetFormEntries.Add(formEntry);
                _context.SaveChanges();
                // Lưu logs lần nhập
                _context.TblChecksheetFormEntryHistories.Add(new TblChecksheetFormEntryHistory
                {
                    OriginalFormEntryId = formEntry.FormEntryId,
                    ChecksheetVerId = formEntry.ChecksheetVerId,
                    PositionCode = formEntry.PositionCode,
                    WorkOrderCode = formEntry.WorkOrderCode,
                    EntryIndex = formEntry.EntryIndex,
                    CreatedAt = formEntry.CreatedAt,
                    CreatedBy = formEntry.CreatedBy,
                    Status = formEntry.ProcessStatus,
                    ActionType = "Add",
                    ActionAt = DateTime.Now,
                    ActionBy = userWorking,
                    ItemAssignmentId = formEntry.ItemAssignmentId,
                });

                // Lưu dữ liệu nhập
                entryValue = new TblChecksheetEntryValue
                {
                    FormEntryId = formEntry.FormEntryId,
                    JsonValue = jsonDataSave,
                };
                _context.TblChecksheetEntryValues.Add(entryValue);
                _context.SaveChanges();
                // Lưu logs dữ liệu nhập
                _context.TblChecksheetEntryValueHistories.Add(new TblChecksheetEntryValueHistory
                {
                    OriginalEntryValueId = entryValue?.EntryValueId,
                    JsonValue = jsonDataSave,
                    ActionType = "Add",
                    ActionAt = DateTime.Now,
                    ActionBy = userWorking,
                    FormEntryId = entryValue?.FormEntryId ?? 0,
                });
            
                _context.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                Log.Fatal(ex, "DbUpdateException");
                if (ex.InnerException != null)
                {
                    Log.Fatal(ex, "Inner Exception");
                }
                else
                {
                    Log.Information("No inner exception found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "General Exception");
                throw;
            }
        }

        /// <summary>
        /// 
        /// Func tạo các lần nhập của dữ liệu và dữ liệu các lần nhập. 
        /// Lưu logs các lần nhập đó vào bảng logs để tiện truy vết sau này.
        /// 
        /// </summary>
        /// <param name="checksheetVerId"></param>
        /// <param name="jsonValue"></param>
        /// <param name="trayNo"></param>
        /// <param name="userWorking"></param>
        /// <param name="workOrder"></param>
        /// <param name="positionWorking"></param>
        /// <param name="itemAssignmentId"></param>
        /// <param name="nextAction"></param>
        /// <param name="entryStatus"></param>
        /// <param name="qtyOfRead"></param>
        /// <returns>Lưu vào Serilog</returns>
        private async Task CreatedEntry(int? checksheetVerId, string jsonValue, string trayNo, string userWorking, string workOrder, string positionWorking, int? itemAssignmentId, string nextAction, string entryStatus, int? qtyOfRead)
        {
            try
            {
                // Cập nhật trạng thái chỉ thị đang sản xuất
                var woProcessing = await _context.TblWorkOrderProcessings
                    .Where(x => x.Woprocessing == workOrder)
                    .FirstOrDefaultAsync();

                if (woProcessing != null)
                {
                    woProcessing.NextAction = nextAction;
                }

                TblChecksheetFormEntry formEntry = new();
                TblChecksheetEntryValue entryValue = new();
                // Lấy maxEntryIndex để 
                var maxEntryIndex = _context.TblChecksheetFormEntries
                    .Where(fe => fe.ChecksheetVerId == checksheetVerId &&
                    fe.WorkOrderCode == workOrder &&
                    fe.PositionCode == positionWorking)
                    .Max(fe => (int?)fe.EntryIndex) ?? 0;
                formEntry = new TblChecksheetFormEntry
                {
                    ChecksheetVerId = checksheetVerId ?? 0,
                    ItemAssignmentId = itemAssignmentId,
                    WorkOrderCode = workOrder,
                    EntryIndex = maxEntryIndex + 1,
                    ProcessStatus = entryStatus,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userWorking,
                    PositionCode = positionWorking,
                    QtyOfReads = qtyOfRead,
                    TrayNo = trayNo,
                };
                _context.TblChecksheetFormEntries.Add(formEntry);
                _context.SaveChanges();

                _context.TblChecksheetFormEntryHistories.Add(new TblChecksheetFormEntryHistory
                {
                    OriginalFormEntryId = formEntry.FormEntryId,
                    ChecksheetVerId = formEntry.ChecksheetVerId,
                    ItemAssignmentId= formEntry.ItemAssignmentId,
                    WorkOrderCode= formEntry.WorkOrderCode,
                    EntryIndex = formEntry.EntryIndex,
                    Status = formEntry.ProcessStatus,
                    CreatedAt = formEntry.CreatedAt,
                    CreatedBy = formEntry.CreatedBy,
                    PositionCode = formEntry.PositionCode,
                    QtyOfReads = qtyOfRead, 
                    TrayNo = trayNo,
                    ActionAt = DateTime.Now,
                    ActionBy = userWorking,
                    ActionType = "Add",
                });
                _context.SaveChanges();

                entryValue = new TblChecksheetEntryValue
                {
                    FormEntryId = formEntry.FormEntryId,
                    JsonValue = jsonValue,
                };
                _context.TblChecksheetEntryValues.Add(entryValue);
                _context.SaveChanges();
                // Lưu logs dữ liệu nhập
                _context.TblChecksheetEntryValueHistories.Add(new TblChecksheetEntryValueHistory
                {
                    OriginalEntryValueId = entryValue?.EntryValueId,
                    JsonValue = jsonValue,
                    ActionType = "Add",
                    ActionAt = DateTime.Now,
                    ActionBy = userWorking,
                    FormEntryId = entryValue?.FormEntryId ?? 0,
                });

                _context.SaveChanges();

            } catch (DbUpdateException ex)
            {
                Log.Fatal(ex, "DbUpdateException");
                if (ex.InnerException != null)
                {
                    Log.Fatal(ex, "Inner Exception");
                }
                else
                {
                    Log.Information("No inner exception found.");
                }
                throw;
            } catch (Exception ex)
            {
                Log.Fatal(ex, "General Exception");
                throw;
            }
        }

        /// <summary>
        /// Lưu thông tin khi có bất thường
        /// </summary>
        /// <param name="checksheetVerionId"></param>
        /// <param name="jsonDataSave"></param>
        /// <param name="userWorking"></param>
        /// <param name="workOrder"></param>
        /// <param name="positionWorking"></param>
        /// <param name="itemAssignmentId"></param>
        /// <param name="nextAction"></param>
        /// <param name="entryStatus"></param>
        /// <param name="qtyRead"></param>
        /// <param name="qtyProduction"></param>
        /// <param name="qtyOK"></param>
        /// <param name="qtyNG"></param>
        /// <param name="jsonErrorValue"></param>
        /// <returns>Lưu thông tin lỗi vào logs</returns>
        private async Task SaveDataAbnormal(int? checksheetVerionId, string jsonDataSave, string? trayNo, string userWorking, string workOrder, string positionWorking, int? itemAssignmentId, string nextAction, string entryStatus, int? qtyRead, int? qtyProduction, int? qtyOK, int? qtyNG, string? jsonErrorValue, int? formEntryId)
        {
            try
            {
                // Cập nhật trạng thái chỉ thị đang sản xuất
                var woProcessing = await _context.TblWorkOrderProcessings
                    .Where(x => x.Woprocessing == workOrder)
                    .FirstOrDefaultAsync();

                if (woProcessing != null)
                {
                    woProcessing.NextAction = nextAction;
                }

                TblChecksheetFormEntry formEntry = new();
                TblChecksheetEntryValue entryValue = new();
                var existFormEntryId = _context.TblChecksheetFormEntries
                    .Where(x => x.FormEntryId == formEntryId)
                    .FirstOrDefault();
                if (existFormEntryId == null)
                {
                    // Lấy maxEntryIndex để 
                    var maxEntryIndex = _context.TblChecksheetFormEntries
                        .Where(fe => fe.ChecksheetVerId == checksheetVerionId &&
                        fe.WorkOrderCode == workOrder &&
                        fe.PositionCode == positionWorking)
                        .Max(fe => (int?)fe.EntryIndex) ?? 0;
                    formEntry = new TblChecksheetFormEntry
                    {
                        ChecksheetVerId = checksheetVerionId ?? 0,
                        ItemAssignmentId = itemAssignmentId,
                        WorkOrderCode = workOrder,
                        PositionCode = positionWorking,
                        EntryIndex = maxEntryIndex + 1,
                        ProcessStatus = entryStatus,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userWorking,
                        QtyOfReads = qtyRead,
                        QtyProduction = qtyProduction,
                        QtyOk = qtyOK,
                        QtyNg = qtyNG,
                        TrayNo = trayNo,
                    };
                    _context.TblChecksheetFormEntries.Add(formEntry);
                    _context.SaveChanges();
                    // Lưu logs lần nhập
                    _context.TblChecksheetFormEntryHistories.Add(new TblChecksheetFormEntryHistory
                    {
                        OriginalFormEntryId = formEntry.FormEntryId,
                        ChecksheetVerId = formEntry.ChecksheetVerId,
                        PositionCode = formEntry.PositionCode,
                        WorkOrderCode = formEntry.WorkOrderCode,
                        EntryIndex = formEntry.EntryIndex,
                        CreatedAt = formEntry.CreatedAt,
                        CreatedBy = formEntry.CreatedBy,
                        Status = formEntry.ProcessStatus,
                        ActionType = "Add",
                        ActionAt = DateTime.Now,
                        ActionBy = userWorking,
                        ItemAssignmentId = formEntry.ItemAssignmentId,
                        QtyOfReads = qtyRead,
                        QtyProduction = qtyProduction,
                        QtyOk = qtyOK,
                        QtyNg = qtyNG,
                        TrayNo = trayNo
                    });

                    // Lưu dữ liệu nhập
                    entryValue = new TblChecksheetEntryValue
                    {
                        FormEntryId = formEntry.FormEntryId,
                        JsonValue = jsonDataSave,
                        JsonErrorValue = jsonErrorValue,
                    };
                    _context.TblChecksheetEntryValues.Add(entryValue);
                    _context.SaveChanges();
                    // Lưu logs dữ liệu nhập
                    _context.TblChecksheetEntryValueHistories.Add(new TblChecksheetEntryValueHistory
                    {
                        OriginalEntryValueId = entryValue?.EntryValueId,
                        JsonValue = jsonDataSave,
                        ActionType = "Add",
                        ActionAt = DateTime.Now,
                        ActionBy = userWorking,
                        FormEntryId = entryValue?.FormEntryId ?? 0,
                        JsonErrorValue = jsonErrorValue,
                    });

                    _context.SaveChanges();
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                Log.Fatal(ex, "DbUpdateException");
                if (ex.InnerException != null)
                {
                    Log.Fatal(ex, "Inner Exception");
                }
                else
                {
                    Log.Information("No inner exception found.");
                }
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "General Exception");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> LeaderCheck([FromBody] RequestLeaderCheck requestLeaderCheck)
        {
            if(requestLeaderCheck == null)
            {
                return BadRequest(new { message = "Dữ liệu gửi lên không có. Vui lòng kiểm tra lại" });
            }
            try
            {
                string? workOrder = requestLeaderCheck.WorkOrderProd;
                string? postionWorking = requestLeaderCheck.PositionWorking;
                string? leaderEmployeeNo = requestLeaderCheck.LeaderEmployeeNo;
                string? leaderPasswordLv2 = requestLeaderCheck.LeaderPassworkLv2;
                string? reasonLeaderConfirm = requestLeaderCheck.ReasonLeaderConfirm;
                int? checksheetVerId = requestLeaderCheck.ChecksheetVerId;

                var employeeParam = new SqlParameter("@pEmployeeNo", leaderEmployeeNo);
                var passwordLv2Param = new SqlParameter("@SecondaryPassword", leaderPasswordLv2);

                var isAuthenticated = new SqlParameter
                {
                    ParameterName = "@pIsAuthenticated",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output,
                };
                var isLeader = new SqlParameter
                {
                    ParameterName = "@pIsLeader",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output,
                };

                var displayNameOutput = new SqlParameter
                {
                    ParameterName = "@pDisplayName",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 200,
                    Direction = System.Data.ParameterDirection.Output,
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AuthenticateAndCheckIfUserIsLeader @pEmployeeNo, @SecondaryPassword, @pIsAuthenticated OUTPUT, @pIsLeader OUTPUT, @pDisplayName OUTPUT",
                    employeeParam, passwordLv2Param, isAuthenticated, isLeader, displayNameOutput);

                bool isAuthenticatedVal = Convert.ToBoolean(isAuthenticated.Value);
                bool isLeaderVal = Convert.ToBoolean(isLeader.Value);
                string? displayName = displayNameOutput.Value.ToString();

                if (isAuthenticatedVal && isLeaderVal)
                {
                    string nextAction = "New WorkOrder";
                    string entryStatus = "Stoped";
                  
                        var woProcessing = _context.TblWorkOrderProcessings
                          .Where(x => x.Woprocessing == workOrder)
                          .FirstOrDefault();

                    if (woProcessing != null)
                    {
                        woProcessing.ProcessingStatus = "Production end";
                        woProcessing.NextAction = nextAction;
                        woProcessing.EndAt = DateTime.Now;
                    }

                    var getEntries = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workOrder && x.PositionCode == postionWorking)
                    .ToListAsync();
                    foreach (var item in getEntries)
                    {
                        item.IsLeaderApproved = true;
                        item.LeaderApprovalReason = reasonLeaderConfirm;
                        item.LeaderApprovedAt = DateTime.Now;
                        item.LeaderApprovedBy = displayName;

                        item.IsStopped = true;
                        item.StopReason = reasonLeaderConfirm;
                        item.StoppedAt = DateTime.Now;
                        item.StoppedBy = displayName;

                        item.ProcessStatus = entryStatus;


                        // Update histories entry
                        var getEntryHistories = await _context.TblChecksheetFormEntryHistories
                            .Where(x => x.OriginalFormEntryId == item.FormEntryId)
                            .FirstOrDefaultAsync();
                        if(getEntryHistories != null)
                        {
                            getEntryHistories.IsLeaderApproved = true;
                            getEntryHistories.LeaderApprovalReason = reasonLeaderConfirm;
                            getEntryHistories.LeaderApprovedAt = DateTime.Now;
                            getEntryHistories.LeaderApprovedBy = displayName;

                            getEntryHistories.IsStopped = true;
                            getEntryHistories.StopReason = reasonLeaderConfirm;
                            getEntryHistories.StoppedAt = DateTime.Now;
                            getEntryHistories.StoppedBy = displayName;

                            getEntryHistories.Status = entryStatus;

                            getEntryHistories.ActionType = "Closed";
                            getEntryHistories.ActionAt = DateTime.Now;
                            getEntryHistories.ActionBy = displayName ?? "";
                        }

                        // Update note value
                        if (checksheetVerId == item.ChecksheetVerId)
                        {
                            var getAllEntryValue = await _context.TblChecksheetEntryValues
                                .Where(x => x.FormEntryId == item.FormEntryId)
                                .FirstOrDefaultAsync();
                            if(getAllEntryValue != null)
                            {
                                getAllEntryValue.JsonNoteValue = requestLeaderCheck.JsonValueNote;

                                var getLogEntryValue = await _context.TblChecksheetEntryValueHistories
                                    .Where(x => x.OriginalEntryValueId == getAllEntryValue.EntryValueId)
                                    .FirstOrDefaultAsync();
                                if(getAllEntryValue != null)
                                {
                                    getAllEntryValue.JsonNoteValue = requestLeaderCheck.JsonValueNote;
                                }
                            }
                        }
                    }
                    _context.SaveChanges();
                    return Ok(new { message = "Đã xác nhận. Vui lòng chuyển lô khác." });
                } else
                {
                    return StatusCode(500, new { message = "Tài khoản này không phải là Leader. Vui lòng kiểm tra lại." });
                }
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /// <summary>
        /// Lấy memu lỗi con để hiển thị theo lỗi đó
        /// </summary>
        /// <param name="requestGetMenuChild"></param>
        /// <returns>Chuỗi json string menu lỗi con</returns>
        [HttpPost]
        public IActionResult GetMenuChild([FromBody] RequestGetMenuChild requestGetMenuChild)
        {
            var idCha = 0;
            if (requestGetMenuChild.ErrorId != null)
            {
                idCha = requestGetMenuChild.ErrorId ?? 0;
            }
            else
            {
                idCha = _context.TblMasterErrors
                    .Where(x => requestGetMenuChild.ErrorName != null && requestGetMenuChild.ErrorName.Contains(x.ErrorName ?? "") && x.Location == "ktt")
                    .Select(s => s.Id).FirstOrDefault();
            }
            var parentName = _context.TblMasterErrors
                    .Where(x => x.Id == idCha && x.Location == "ktt")
                    .Select(s => s.ErrorName).FirstOrDefault();
            var getMenuChild = _context.TblMasterErrors
                .Where(x => x.Idcha == idCha)
                .Select(s => new
                {
                    parentName = parentName,
                    s.Id,
                    s.ErrorName,
                    CheckMenuChild = _context.TblMasterErrors.Where(x => x.Idcha == s.Id).AsEnumerable().ToList().Count() > 0 ? true : false,
                    CountMenuChild = _context.TblMasterErrors.Where(x => x.Idcha == s.Id).AsEnumerable().ToList().Count(),
                }).OrderBy(x => x.CountMenuChild).ToList();
            return Ok(new { menuChild = getMenuChild });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFinalEntry([FromBody] RequestDataSave requestDataSave)
        {

            if (requestDataSave == null)
            {
                return BadRequest(new { message = "Request is not found" });
            }
            try
            {
                string? userWorking = HttpContext.Session.GetString("DisplayName")?.ToString();

                string? workOrder = requestDataSave.WorkOrderProd;
                string? jsonDataSave = requestDataSave.JsonSaveDb;
                string? jsonErrorInfo = requestDataSave.ErrorInfo;
                string? positionWorking = requestDataSave.PositionWorking;
                int? checksheetVerId = requestDataSave.ChecksheetVersionId;
                int? itemAssignmentId = requestDataSave.ItemAssignmentId;
                int? qtyRead = requestDataSave.QtyOfReads;
                int? qtyProduction = requestDataSave.QtyProcessing;
                int? qtyOK = requestDataSave.QtyOK;
                int? qtyNG = requestDataSave.QtyNG;
                int? qtyInLine = requestDataSave.QtyInLine;
                string? trayNo = requestDataSave.TrayNo;

                string nextAction = "Connection Eink";
                string entryStatus = "Connection Eink";

                // Cập nhật số lượng và trạng thái của thẻ Eink
                bool isReadEink = true;
                string? jsonEinkInfo = requestDataSave.EinkInfo;
                EinkInfo? einkInfo = JsonConvert.DeserializeObject<EinkInfo>(jsonEinkInfo ?? "");
                if (einkInfo != null)
                {
                    var existingTrayProduct = await _ec.TblProducts
                     .Where(x => x.SoThung == einkInfo.SoMang && x.ItemCode == einkInfo.ChungLoai
                     && x.LotNo == einkInfo.LoSanXuat && x.Remark1 == einkInfo.LoNVL && x.MoTa == "Eink Máng")
                     .FirstOrDefaultAsync();
                    if (existingTrayProduct != null)
                    {
                        isReadEink = false;
                        nextAction = einkInfo.NextAction;
                    }
                    else
                    {
                        isReadEink = true;
                    }
                }

                // Cập nhật trạng thái chỉ thị đang sản xuất
                var woProcessing = await _context.TblWorkOrderProcessings
                    .Where(x => x.Woprocessing == workOrder)
                    .FirstOrDefaultAsync();

                if (woProcessing != null)
                {
                    woProcessing.NextAction = nextAction;
                }

                if(!isReadEink)
                {
                    entryStatus = "Updated Results";
                }

                var getEntryId = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workOrder &&
                    x.ChecksheetVerId == checksheetVerId &&
                    x.PositionCode == positionWorking &&
                    x.TrayNo == trayNo && x.CreatedBy == userWorking).FirstOrDefaultAsync();

                var totalQtyOK = _context.TblChecksheetFormEntries
                    .Where(x => x.ChecksheetVerId == checksheetVerId && x.WorkOrderCode == workOrder && x.PositionCode == positionWorking &&
                    x.TrayNo == trayNo && x.CreatedBy == userWorking && x.QtyOk != null)
                    .Sum(x => x.QtyOk) ?? 0;

                var totalQtyNG = _context.TblChecksheetFormEntries
                    .Where(x => x.ChecksheetVerId == checksheetVerId && x.WorkOrderCode == workOrder && x.PositionCode == positionWorking &&
                    x.TrayNo == trayNo && x.CreatedBy == userWorking && x.QtyNg != null)
                    .Sum(x => x.QtyNg) ?? 0;

                if (getEntryId != null )
                {
                    getEntryId.QtyOk = totalQtyOK > 0 ? totalQtyOK + qtyOK : qtyOK;
                    getEntryId.QtyNg = totalQtyNG > 0 ? totalQtyNG + qtyNG : qtyNG;
                    getEntryId.ProcessStatus = entryStatus;

                    var existingEntryLog = await _context.TblChecksheetFormEntryHistories
                       .Where(x => x.OriginalFormEntryId == getEntryId.FormEntryId)
                       .FirstOrDefaultAsync();
                    if (existingEntryLog != null)
                    {
                        existingEntryLog.QtyOk = totalQtyOK > 0 ? totalQtyOK + qtyOK : qtyOK; ;
                        existingEntryLog.QtyNg = totalQtyNG > 0 ? totalQtyNG + qtyNG : qtyNG; ;
                        existingEntryLog.Status = entryStatus;
                        existingEntryLog.ActionType = entryStatus;
                        existingEntryLog.ActionAt = DateTime.Now;
                    }

                    var existingEntryValue = await _context.TblChecksheetEntryValues
                        .Where(x => x.FormEntryId == getEntryId.FormEntryId)
                        .FirstOrDefaultAsync();
                    if (existingEntryValue != null)
                    {
                        existingEntryValue.JsonValue = jsonDataSave;
                        existingEntryValue.JsonErrorValue = jsonErrorInfo;

                        // Cập nhật logs
                        var existingEntryValueLog = await _context.TblChecksheetEntryValueHistories
                       .Where(x => x.FormEntryId == getEntryId.FormEntryId &&
                       x.OriginalEntryValueId == existingEntryValue.EntryValueId
                       )
                       .FirstOrDefaultAsync();
                        if (existingEntryValueLog != null)
                        {
                            existingEntryValueLog.JsonValue = jsonDataSave;
                            existingEntryValueLog.JsonErrorValue = jsonErrorInfo;
                            existingEntryValueLog.ActionAt = DateTime.Now;
                            existingEntryValueLog.ActionType = entryStatus;
                        }
                    } else
                    {
                        var entryValue = new TblChecksheetEntryValue
                        {
                            FormEntryId = getEntryId.FormEntryId,
                            JsonValue = jsonDataSave,
                            JsonErrorValue = jsonErrorInfo,
                        };
                        _context.TblChecksheetEntryValues.Add(entryValue);
                        _context.SaveChanges();
                        // Lưu logs dữ liệu nhập
                        _context.TblChecksheetEntryValueHistories.Add(new TblChecksheetEntryValueHistory
                        {
                            OriginalEntryValueId = entryValue?.EntryValueId,
                            JsonValue = jsonDataSave,
                            JsonErrorValue = jsonErrorInfo,
                            ActionType = "Add",
                            ActionAt = DateTime.Now,
                            ActionBy = userWorking ?? "",
                            FormEntryId = entryValue?.FormEntryId ?? 0,
                        });
                    }
                } else
                {
                    TblChecksheetFormEntry formEntry = new();
                    TblChecksheetEntryValue entryValue = new();

                    // Lấy maxEntryIndex để 
                    var maxEntryIndex = _context.TblChecksheetFormEntries
                        .Where(fe => fe.ChecksheetVerId == checksheetVerId &&
                        fe.WorkOrderCode == workOrder &&
                        fe.PositionCode == positionWorking)
                        .Max(fe => (int?)fe.EntryIndex) ?? 0;
                    formEntry = new TblChecksheetFormEntry
                    {
                        ChecksheetVerId = checksheetVerId ?? 0,
                        ItemAssignmentId = itemAssignmentId,
                        WorkOrderCode = workOrder ?? "",
                        PositionCode = positionWorking,
                        EntryIndex = maxEntryIndex + 1,
                        ProcessStatus = entryStatus,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userWorking ?? "",
                        QtyOfReads = qtyRead,
                        QtyProduction = qtyProduction,
                        QtyOk = qtyOK,
                        QtyNg = qtyNG,
                        TrayNo = trayNo,
                    };
                    _context.TblChecksheetFormEntries.Add(formEntry);
                    _context.SaveChanges();
                    // Lưu logs lần nhập
                    _context.TblChecksheetFormEntryHistories.Add(new TblChecksheetFormEntryHistory
                    {
                        OriginalFormEntryId = formEntry.FormEntryId,
                        ChecksheetVerId = formEntry.ChecksheetVerId,
                        PositionCode = formEntry.PositionCode,
                        WorkOrderCode = formEntry.WorkOrderCode,
                        EntryIndex = formEntry.EntryIndex,
                        CreatedAt = formEntry.CreatedAt,
                        CreatedBy = formEntry.CreatedBy,
                        Status = formEntry.ProcessStatus,
                        ActionType = "Add",
                        ActionAt = DateTime.Now,
                        ActionBy = userWorking ?? "",
                        ItemAssignmentId = formEntry.ItemAssignmentId,
                        QtyOfReads = qtyRead,
                        QtyProduction = qtyProduction,
                        QtyOk = qtyOK,
                        QtyNg = qtyNG,
                        TrayNo = trayNo,
                    });

                    // Lưu dữ liệu nhập
                    entryValue = new TblChecksheetEntryValue
                    {
                        FormEntryId = formEntry.FormEntryId,
                        JsonValue = jsonDataSave,
                        JsonErrorValue = jsonErrorInfo,
                    };
                    _context.TblChecksheetEntryValues.Add(entryValue);
                    _context.SaveChanges();
                    // Lưu logs dữ liệu nhập
                    _context.TblChecksheetEntryValueHistories.Add(new TblChecksheetEntryValueHistory
                    {
                        OriginalEntryValueId = entryValue?.EntryValueId,
                        JsonValue = jsonDataSave,
                        JsonErrorValue = jsonErrorInfo,
                        ActionType = "Add",
                        ActionAt = DateTime.Now,
                        ActionBy = userWorking ?? "",
                        FormEntryId = entryValue?.FormEntryId ?? 0,
                    });
                    _context.SaveChanges();
                }

                var totalQtyMaterial = await _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workOrder &&
                    x.PositionCode == positionWorking)
                    .SumAsync(s => s.QtyOfReads);

                if(totalQtyMaterial == qtyInLine)
                {
                    // Cập nhật trạng thái chỉ thị đang sản xuất
                    var updateNewWO = await _context.TblWorkOrderProcessings
                        .Where(x => x.Woprocessing == workOrder)
                        .FirstOrDefaultAsync();

                    if (updateNewWO != null)
                    {
                        updateNewWO.NextAction = "New WorkOrder";
                        updateNewWO.ProcessingStatus = "Production End";
                        updateNewWO.EndAt = DateTime.Now;
                    }
                }
                _context.SaveChanges();

                if (einkInfo != null && !isReadEink)
                {
                    var existingTrayProduct = await _ec.TblProducts
                     .Where(x => x.SoThung == einkInfo.SoMang && x.ItemCode == einkInfo.ChungLoai
                     && x.LotNo == einkInfo.LoSanXuat && x.Remark1 == einkInfo.LoNVL && x.MoTa == "Eink Máng")
                     .FirstOrDefaultAsync();

                    if (existingTrayProduct != null)
                    {
                        var qtyTotalNextProcessing = _context.TblChecksheetFormEntries
                        .Where(x => x.WorkOrderCode == workOrder && x.PositionCode == positionWorking &&
                        x.TrayNo == einkInfo.SoMang)
                        .Sum(x => x.QtyOk);
                        existingTrayProduct.QtyCdsau = qtyTotalNextProcessing;
                        existingTrayProduct.TrangThaiSp = einkInfo.TrangThai;
                        nextAction = einkInfo.NextAction;
                    }
                    _ec.SaveChanges();
                }

                return Ok(new { message = "Lưu kết quả sản xuất thành công", statusEink = isReadEink });
            } catch (DbUpdateException ex)
            {
                Log.Fatal(ex, "DbUpdateException");
                if (ex.InnerException != null)
                {
                    Log.Fatal(ex, "Inner Exception");
                }
                else
                {
                    Log.Information("No inner exception found.");
                }
                return StatusCode(500, new { message = ex.Message });
                throw;
            }
            catch (Exception ex) {
                Log.Fatal(ex, "General Exception");
                return StatusCode(500, new { message = ex.Message });
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> TriggerEink([FromBody] RequestDataSave requestDataSave)
        {
            if (requestDataSave == null)
            {
                return StatusCode(500, new { message = "Dữ liệu gửi lên đang trống hoặc không đúng định dạng. Vui lòng thông báo cho Admin kiểm tra lại!" });
            }
            try
            {
                string? jsonEinkInfo = requestDataSave.EinkInfo;
                string? MAC = requestDataSave.MAC;
                string? workOrder = requestDataSave.WorkOrderProd;
                string? positionWorking = requestDataSave.PositionWorking;

                EinkInfo? einkInfo = JsonConvert.DeserializeObject<EinkInfo>(jsonEinkInfo ?? "");

                string stringFormatTime = string.Empty;
                if (einkInfo != null)
                {
                    // Cập nhật trạng thái chỉ thị đang sản xuất
                    var woProcessing = await _context.TblWorkOrderProcessings
                        .Where(x => x.Woprocessing == workOrder)
                        .FirstOrDefaultAsync();

                    if (woProcessing != null)
                    {
                        woProcessing.NextAction = einkInfo.NextAction;
                    }

                    var entryInfo = await _context.TblChecksheetFormEntries
                        .Where(x => x.WorkOrderCode == workOrder &&
                        x.PositionCode == positionWorking &&
                        x.TrayNo == einkInfo.SoMang)
                        .FirstOrDefaultAsync();
                    if(entryInfo != null)
                    {
                        entryInfo.ProcessStatus = "Entered Results";
                        var logAuditEntry = await _context.TblChecksheetFormEntryHistories
                            .Where(x => x.OriginalFormEntryId == entryInfo.FormEntryId)
                            .FirstOrDefaultAsync();
                        if(logAuditEntry != null)
                        {
                            logAuditEntry.Status = "Entered Results";
                        }
                    }

                    _context.SaveChanges();

                    if (einkInfo.HSD != null)
                    {
                        if (einkInfo.HSD.Length > 6)
                        {
                            stringFormatTime = "yyyyMMdd";
                        }
                        else
                        {
                            stringFormatTime = "yyMMdd";
                        }
                    }
                    DateTime today = DateTime.Now;
                    string formattedDate = today.ToString(stringFormatTime);
                    DateTime.TryParseExact(einkInfo.HSD, stringFormatTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeLimit);
                    bool _ = int.TryParse(einkInfo.HSD, out int timeLimitImported);
                    int timeNow = int.Parse(formattedDate);
                    int timeRemaining = timeLimitImported - timeNow;

                    string qrCodeValue = einkInfo.SoMang + "%" + einkInfo.ChungLoai + "%" + einkInfo.LoSanXuat;

                    TblProduct tblProducts = new();
                    Guid productId = Guid.NewGuid();

                    var qtyTotalNextProcessing = _context.TblChecksheetFormEntries
                        .Where(x => x.WorkOrderCode == workOrder && x.PositionCode == positionWorking &&
                        x.TrayNo == einkInfo.SoMang)
                        .Sum(x => x.QtyOk);

                    var existingTrayProduct = await _ec.TblProducts
                        .Where(x => x.SoThung == einkInfo.SoMang && x.ItemCode == einkInfo.ChungLoai
                        && x.LotNo == einkInfo.LoSanXuat && x.Remark1 == einkInfo.LoNVL && x.MoTa == "Eink Máng")
                        .FirstOrDefaultAsync();
                    if (existingTrayProduct != null)
                    {
                        existingTrayProduct.ItemCode = einkInfo.ChungLoai;
                        existingTrayProduct.LotNo = einkInfo.LoSanXuat;
                        existingTrayProduct.Qrcode = qrCodeValue;
                        existingTrayProduct.SoThung = einkInfo.SoMang;
                        existingTrayProduct.Remark1 = einkInfo.LoNVL;
                        existingTrayProduct.HanSuDung = timeLimit.ToString("dd/MM/yyyy");
                        existingTrayProduct.QtyCdsau = qtyTotalNextProcessing;
                        existingTrayProduct.TrangThaiSp = einkInfo.TrangThai;
                        tblProducts = existingTrayProduct;
                    }
                    else
                    {
                        tblProducts = new TblProduct
                        {
                            Iditem = productId,
                            ItemCode = einkInfo.ChungLoai,
                            LotNo = einkInfo.LoSanXuat,
                            Qrcode = qrCodeValue,
                            SoThung = einkInfo.SoMang,
                            Remark1 = einkInfo.LoNVL,
                            HanSuDung = timeLimit.ToString("dd/MM/yyyy"),
                            QtyCdsau = einkInfo.SoLuongDat,
                            TrangThaiSp = einkInfo.TrangThai,
                            HeThong = "M+ GW",
                            MoTa = "Eink Máng",
                            RInt2 = timeRemaining
                        };
                        _ec.TblProducts.Add(tblProducts);
                        _ec.SaveChanges();
                    }
                    _ec.SaveChanges();

                    string endpoint = $"{einkUrl}/{MAC}/link/{tblProducts.Iditem}";
                    var response = await PostLinkESL(endpoint, httpClient);
                    if(response)
                    {
                        var checkWo = _context.TblWorkOrderProcessings
                            .Where(x => x.Woprocessing == workOrder &&
                            x.PositionCode == positionWorking && x.ProcessingStatus == "Production end")
                            .FirstOrDefault();
                        bool isNewWo = false;
                        if(checkWo != null)
                        {
                            isNewWo = true;
                        }
                        return Ok(new { message = "Link thành công ESl. Vui lòng đợi trong giây lát để xử lý hiển thị dữ liệu.", readNewWo = isNewWo });
                    } else
                    {
                        return StatusCode(500, new { message = "Link dữ liệu với ESL lỗi. Vui lòng liên hệ Admin để xử lý." });
                    }
                }
                else
                {
                    return StatusCode(500, new { message = "Có lỗi liên quan đến xử lý dữ liệu. Vui lòng liên hệ Admin để xử lý." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task<bool> PostLinkESL(string endpoint, HttpClient _client)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, SslPolicyErrors) => true
            };
            _client = new HttpClient(handler);

            _client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            try
            {
                var response = await _client.PostAsync(endpoint, null).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Log.Fatal($"Error: {response.StatusCode} - {content}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Fatal($"HTTP error: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                Log.Fatal("Request timed out.");
                return false;
            }
        }

        [HttpPost]
        public IActionResult GetFormNote([FromBody] RequestDataSave requestDataSave)
        {
            if (requestDataSave == null)
            {
                return BadRequest(new { message = "Dữ liệu gửi lên không có" });
            }

            string? positionWorking = requestDataSave.PositionWorking;
            var getPositionName = _context.TblMasterPositions.Where(x => x.PositionCode == positionWorking)
                .Select(s => s.PositionName).FirstOrDefault();
            var getForm = _context.TblChecksheetForms
                .Where(x => x.FormPosition == getPositionName &&
                x.FormType == "form-notes-operation")
                .Select(s => new
                {
                    formId = s.FormId,
                    s.ChecksheetVersionId,
                    sections = s.JsonFormData
                }).ToList();

            return Ok(new { formField = getForm });
        }
    }

    public class ItemRequest
    {
        public int WorkOrder { get; set; }
        public string ProcessCode { get; set; }
    }
    public class RenderItemDivLine
    {
        public string ProductCode { get; set; }
        public string LotNo { get; set; }
        public int Qty { get; set; }
    }

    public class RequestDataPosition
    {
        public string PositionWorking { get; set; }
    }

    public class ItemDivForLot
    {
        public string? WorkOrder { get; set; }
        public string? ProductCode { get; set; }
        public string? LotMaterial { get; set; }
        public int Line1 { get; set; }
        public int Line2 { get; set; }
        public int Line3 { get; set; }
        public int Line4 { get; set; }
    }

    public class RequestCheckMaterial
    {
        public string MaterialCode { get; set; }
        public string LotMaterial { get; set; }
        public string WorkOrder { get; set; }
        public string PouchNo { get; set; }
        public string TimeLimit { get; set; }
    }

    public class RequestDataGetForm
    {
        public string? PositionWorking { get; set; } = string.Empty;
        public int? ChecksheetId { get; set; }
        public int? ChecksheetVerId { get; set; }
        public string? ProductCode { get; set; } = string.Empty;
        public string? ProductLot { get; set; } = string.Empty;
        public string? WorkOrder { get; set; } = string.Empty;
        public string? FormType { get; set; } = string.Empty;
        public int? OrderForm { get; set; }
    }

    public class RequestDataSave
    {
        public string? ChecksheetCode { get; set; } = string.Empty;
        public int? ChecksheetVersionId { get; set; }
        public string? CheckConditions { get; set; } = string.Empty;
        public string? DataSaveMapping { get; set; } = string.Empty;
        public string? WorkOrderProd { get; set; } = string.Empty;
        public string? PositionWorking { get; set; } = string.Empty;
        public string? JsonSaveDb { get; set; } = string.Empty;
        public string? TrayNo { get; set; } = string.Empty;
        public string? ErrorInfo { get; set; } = string.Empty;
        public int? FrequencyId { get; set; }
        public int? ItemAssignmentId { get; set; }
        public int? QtyOfReads { get; set; }
        public int? QtyProcessing { get; set; }
        public int? QtyOK { get; set; }
        public int? QtyNG { get; set; }
        public int? QtyInLine { get; set; }
        public int? FormEntryId { get; set; }
        public string? RequestType { get; set; } = string.Empty;
        public string? MAC { get; set; } = string.Empty;
        public string? EinkInfo { get; set; } = string.Empty;
    }

    public class RequestLeaderCheck
    {
        public string? WorkOrderProd { get; set; } = string.Empty;
        public string? PositionWorking { get; set; } = string.Empty;
        public string? LeaderEmployeeNo { get; set; } = string.Empty;
        public string? LeaderPassword { get; set; } = string.Empty;
        public string? LeaderPassworkLv2 { get; set; } = string.Empty;
        public string? ReasonLeaderConfirm { get; set; } = string.Empty;
        public string? JsonValueNote { get; set; } = string.Empty;
        public int? ChecksheetVerId { get; set; }
        public int? ItemAssignmentId { get; set; }
    }

    public class DataCheckCondition
    {
        public string Label { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ToolInfo
    {
        public string? ToolCode { get; set; }
        public string? ToolName { get; set; }
    }

    public class DataSaveMapping
    {
        public List<FieldMappingDataSave> FormData { get; set; }
        public int OperatorId { get; set; }
    }

    public class FieldMappingDataSave
    {
        public int? FormId { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
    }

    public class EinkInfo
    {
        public string SoMang { get; set; }
        public string ChungLoai { get; set; }
        public string LoNVL { get; set; }
        public string HSD { get; set; }
        public int SoLuongDat { get; set; }
        public string LoSanXuat { get; set; }
        public string TrangThai { get; set; }
        public string NextAction { get; set; }
    }
}
