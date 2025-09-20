using ConnectMES;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MPLUS_GW_WebCore.Controllers.Admin.CreateForms;
using MPLUS_GW_WebCore.Controllers.Materials;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Serilog;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MPLUS_GW_WebCore.Controllers.Processing
{
    public class ProcessingController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public readonly IWebHostEnvironment _environment;
        private readonly EslsystemContext _ec;
        private readonly ProductionCalculator _productionCalculator;

        private readonly string apiKey = "CCAE6917-323B-4B5F-A62F-56910FA3F8CF";
        private readonly string einkUrl = "https://10.239.4.40/api/esl";
        private readonly HttpClient httpClient;

        public ProcessingController(MplusGwContext context, Classa classa, IWebHostEnvironment hostEnvironment, EslsystemContext ec, ProductionCalculator productionCalculator)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _ec = ec ?? throw new ArgumentNullException(nameof(ec));
            _productionCalculator = productionCalculator ?? throw new ArgumentNullException(nameof(_productionCalculator));
        }

        #region Chia line NVL cho leader

        [Route("/gia-cong")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Gia công", Url = Url.Action("Index", "Processing"), IsActive = true },
            };
            var currentDate = DateTime.Now;
            var currentDay = currentDate.Date;
            var strLast = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            var strNext = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(2).AddDays(-1).ToString("yyyy-MM-dd");

            List<ListWOStatusInMes> listWOStatusInMes = new();
            List<ListWoProduction> listWoProductions = new();
            List<ListWoDivLine> listWoDivLines = new();
            List<ListDivLineForLot> listDivLineForLot = new();
            var roleName = HttpContext.Session.GetString("RoleName");
            var locationCode = await (from l in _context.TblLocations
                                      where l.LocationName == "Gia Công"
                                      select new
                                      {
                                          l.LocationCode,
                                          l.IdLocation
                                      }).FirstOrDefaultAsync();
            if (roleName == null || !roleName.Contains("User"))
            {
                if (locationCode != null)
                {
                    var loadData = _cl.WO_status(locationCode.LocationCode, strLast, strNext);
                    foreach (DataRow row in loadData.Rows)
                    {
                        string workOrder = row["orderno"].ToString() ?? "";
                        string statusName = row["statusname"].ToString() ?? "";
                     
                        if (statusName != "Creating Orders")
                        {
                            var item = new ListWOStatusInMes
                            {
                                WorkOrderNo = row["orderno"].ToString(),
                                StatusName = row["statusname"].ToString()
                            };
                            listWOStatusInMes.Add(item);
                        }
                       
                    }
                    var getItemAdded = await (from s in _context.TblPreImportItems
                                              join l in _context.TblLocations on s.IdLocation equals l.IdLocation
                                              join w in _context.TblWorkOrderMes on s.WorkOrder equals w.WorkOrder
                                              where s.IdLocation == locationCode.IdLocation && w.QtyUnused > 0
                                              select new
                                              {
                                                  s.WorkOrder,
                                                  s.ItemCode,
                                                  s.LotNo,
                                                  s.Qty,
                                                  s.CharacterAlp,
                                                  s.DateProd,
                                                  s.TimeProd,
                                                  ProcessCode = l.LocationCode,
                                              }).ToListAsync();
                    var oldResult = await (from s in _context.TblDivLineProds
                                           join l in _context.TblLocations on s.IdLocation equals l.IdLocation
                                           where s.IdLocation == locationCode.IdLocation
                                           select new
                                           {
                                               s.WorkOrder,
                                               s.Line1,
                                               s.Line2,
                                               s.Line3,
                                               s.Line4,
                                               s.ChangeControl,
                                               s.Note,
                                           }).ToListAsync();
                    if (listWOStatusInMes.Count > 0)
                    {
                        foreach (var itemMes in listWOStatusInMes)
                        {
                            string? workOrder = itemMes.WorkOrderNo?.ToString();
                            string? statusname = itemMes.StatusName?.ToString();

                            var getInfoWorkOrder = _context.TblWorkOrderMes
                                .Where(x => x.WorkOrder == workOrder)
                                .FirstOrDefault();

                            var getDataCalcTimeProds = _context.TblCalcTimeDivLines
                                .Where(x => x.WorkOrder == workOrder).ToList();
                            List<ProductionLineTimeData> productionLineTimes = new();

                            string? indexNumber = string.Empty;
                            if(getDataCalcTimeProds.Count > 0)
                            {
                                foreach (var itemCalc in getDataCalcTimeProds)
                                {
                                    indexNumber = itemCalc.SoTt.ToString();
                                    productionLineTimes.Add(new ProductionLineTimeData
                                    {
                                        DataLine = itemCalc.LineNumber ?? 0,
                                        Qty = itemCalc.SoLuongDuDinh ?? 0,
                                        Time = (double?)itemCalc.ThoiGianSanXuat ?? 0,
                                        StartDate = itemCalc.NgayDuDinhSanXuat?.ToString("dd/MM/yyyy HH:mm") ?? "",
                                        EndDate = itemCalc.NgayKetThuc?.ToString("dd/MM/yyyy HH:mm") ?? "",
                                        QtyInDay = itemCalc.SoLuongTrenNgay ?? 0
                                    });
                                }
                            }

                            if (getInfoWorkOrder != null)
                            {
                                var stdItem = new ListWoProduction
                                {
                                    SoTT = indexNumber ?? "",
                                    WorkOrderNo = workOrder,
                                    ProductCode = getInfoWorkOrder.ItemCode,
                                    LotNo = getInfoWorkOrder.LotNo,
                                    QtyProd = getInfoWorkOrder.QtyWo.ToString(),
                                    Character = getInfoWorkOrder.Character,
                                    DateProd = getInfoWorkOrder.DateProd,
                                    TimeProd = getInfoWorkOrder.TimeProd?.ToString(@"hh\:mm"),
                                    ProcessCode = locationCode.LocationCode,
                                    StatusName = statusname,
                                    ProductionLines = productionLineTimes
                                };
                                listWoProductions.Add(stdItem);
                            }
                        }
                    }
                    if (oldResult != null)
                    {
                        foreach (var item in oldResult)
                        {
                            var std = new ListWoDivLine
                            {
                                WorkOrder = item.WorkOrder,
                                Line1 = item.Line1.ToString(),
                                Line2 = item.Line2.ToString(),
                                Line3 = item.Line3.ToString(),
                                Line4 = item.Line4.ToString(),
                                ChangeControl = item.ChangeControl,
                                Note = item.Note,
                            };
                            listWoDivLines.Add(std);

                            //Lấy Workorder đã chia line theo NVL
                            var getWODivLineLot = await (from s in _context.TblDivLineForLots
                                                         where s.WorkOrder == item.WorkOrder
                                                         select s).ToListAsync();
                            var groupDivLineLot = getWODivLineLot.GroupBy(x => x.WorkOrder)
                                .Select(x => new
                                {
                                    WorkOrder = x.Key,
                                    Line1 = x.Sum(s => s.Line1),
                                    Line2 = x.Sum(s => s.Line2),
                                    Line3 = x.Sum(s => s.Line3),
                                    Line4 = x.Sum(s => s.Line4),
                                }).ToList();
                            foreach (var item1 in groupDivLineLot)
                            {
                                var std1 = new ListDivLineForLot
                                {
                                    WorkOrder = item1.WorkOrder,
                                    Line1 = item1.Line1.ToString(),
                                    Line2 = item1.Line2.ToString(),
                                    Line3 = item1.Line3.ToString(),
                                    Line4 = item1.Line4.ToString(),
                                };
                                listDivLineForLot.Add(std1);
                            }
                        }
                    }
                }
                
                listWoProductions = listWoProductions
                    .OrderBy(x => string.IsNullOrEmpty(x.SoTT) ? 1 : 0)
                    .ThenBy(x => x.SoTT)
                    .OrderBy(x => string.IsNullOrEmpty(x.Character) ? 1 : 0)
                    .ThenBy(x => x.Character)
                    .ThenBy(x => x.WorkOrderNo)
                    .ToList();

                var totalsPerLine = new Dictionary<int, int>();
                var getLines = _context.TblProdLines.Select(s => s.LineName).ToList();
                foreach (var dataItem in listWoProductions)
                {
                    foreach (var item in dataItem.ProductionLines)
                    {
                        string dataLineName = "Line " + item.DataLine;
                        if (getLines.Contains(dataLineName))
                        {
                            if (totalsPerLine.ContainsKey(item.DataLine))
                            {
                                totalsPerLine[item.DataLine] += item.Qty;
                            }
                            else
                            {
                                totalsPerLine.Add(item.DataLine, item.Qty);
                            }
                        }
                    }
                }
                ViewData["ListWoProduction"] = listWoProductions;
                ViewData["ListWoDivLine"] = listWoDivLines;
                ViewData["ListDivLineForLot"] = listDivLineForLot;
                ViewData["StringWOCalc"] = JsonConvert.SerializeObject(listWoProductions);
                ViewData["StringTotalLine"] = JsonConvert.SerializeObject(totalsPerLine);

                return View();
            }
            else
            {
                return View("UserView");
            }
        }

        [HttpPost]
        public IActionResult CompareQtyWithInventory(RequestData requestData)
        {
            if (requestData.StrDataCheckQty == null)
            {
                return BadRequest("Request json error. Please check again.");
            }
            var currentDate = DateTime.Now;
            var strLast = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            var strNext = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(2).AddDays(-1).ToString("yyyy-MM-dd");

            ItemCheck[]? itemCheck = JsonConvert.DeserializeObject<ItemCheck[]>(requestData.StrDataCheckQty);
            List<Dictionary<string, object>> itemOutInventory = new();
            Dictionary<string, object> row;
            bool checkCompare = true;
            int inventory = 0;
            int qtyUsed = 0;
            if (itemCheck != null)
            {
                foreach (var item in itemCheck)
                {
                    row = new Dictionary<string, object>();

                    var dataAllWO = _cl.WO_status(item.ProcessCode, strLast, strNext);
                    foreach (DataRow woItem in dataAllWO.Rows)
                    {
                        if (int.Parse(woItem["orderno"].ToString() ?? "0") <= int.Parse(item.WorkOrder ?? "0"))
                        {
                            string or = woItem["orderno"].ToString() ?? "";
                            string po = woItem["productcode"].ToString() ?? "";
                            var itemRM = _cl.ShowDataEX(item.ProcessCode, po, or);
                            if (itemRM != null)
                            {
                                foreach (DataRow item2 in itemRM.Rows)
                                {
                                    inventory = InventoryQty(item2["inputgoodscode"].ToString() ?? "", item.ProcessCode ?? "");
                                    if (or == item2["orderno"].ToString())
                                    {
                                        int reservedQty = ReservedValue(or, woItem["inputgoodscode"].ToString() ?? "");
                                        int RMQtyUse = int.Parse(woItem["Input_qty"].ToString() ?? "0") - reservedQty;
                                        if (RMQtyUse < 0)
                                        {
                                            RMQtyUse = 0;
                                        }
                                        qtyUsed += RMQtyUse;
                                    }
                                }

                            }
                        }
                    }
                    inventory -= qtyUsed;
                    if (inventory > 0)
                    {
                        checkCompare = true;
                    }
                    else
                    {
                        checkCompare = false;
                    }
                    if (!checkCompare)
                    {
                        row.Add("WORKORDER", item.WorkOrder ?? "");
                        itemOutInventory.Add(row);
                    }
                }
            }
            if (checkCompare)
            {
                var response = new
                {
                    status = true,
                    message = "Đã đủ nguyên vật liệu cho sản xuất"
                };
                return Ok(response);
            }
            else
            {
                var response = new
                {
                    status = false,
                    message = "Chưa đủ nguyên vật liệu cho sản xuất!",
                    dataOutInventory = itemOutInventory.ToArray()
                };
                return Ok(response);
            }
        }

        public int InventoryQty(string productCode, string location)
        {
            var qty = 0;
            var loadData = _cl.InventoryQty(location, productCode);

            List<InventoryQty> listItems = new();
            foreach (DataRow row in loadData.Rows)
            {
                listItems.Add(new InventoryQty(row["PRODUCT"].ToString(), row["LOTNO"].ToString(),
                    row["QTY"].ToString(), row["LOCATION"].ToString(), row["Type"].ToString()));
            }

            var items = (from tbl in listItems
                         where tbl.Type == "Inventory" && tbl.Productcode == productCode && tbl.Processcode == location
                         select tbl).ToList();
            foreach (var item in items)
            {
                qty += Int32.Parse(item.Quantity ?? "0");
            }
            return qty;
        }

        public int ReservedValue(string orderno, string itemcode)
        {
            //Truy vấn MES
            int qty = 0;
            var loadData = _cl.Receive_qty(orderno);
            List<ItemReserved> list = new();
            foreach (DataRow row in loadData.Rows)
            {
                list.Add(new ItemReserved(row["orderno"].ToString(), row["Type"].ToString(), row["Product"].ToString(), row["lotno"].ToString(), Int32.Parse(row["times"].ToString() ?? "0"), Int32.Parse(row["RE_Qty"].ToString() ?? "0")));
            }

            var result = (from s in list where s.ItemCode == itemcode select s).ToList();
            foreach (var item in result)
            {
                qty += item.QtyReserved;
            }
            return qty;
        }

        [HttpPost]
        public async Task<IActionResult> ConnectingEinkDivLine([FromBody] RequestDataEink requestDataEink)
        {
            if (requestDataEink == null)
            {
                return BadRequest(new { message = "Không tìm thấy yêu cầu gửi lên" });
            }
            
            string materialCode = requestDataEink.MaterialCode;
            string lotMaterial = requestDataEink.LotMaterial;

            var timeSterilization = _context.TblImportedItems
                .Where(x => x.ItemCode == materialCode && lotMaterial == x.LotNo && x.TimeSterilization != "")
                .Select(s => s.TimeSterilization).FirstOrDefault();

            var idLocation = _context.TblImportedItems
                .Where(x => x.ItemCode == materialCode && lotMaterial == x.LotNo && x.TimeSterilization != "")
                .Select(s => s.IdLocation).FirstOrDefault();

            var infoLocation = _context.TblLocations
                .Where(x => x.IdLocation == idLocation)
                .FirstOrDefault();

            string stringFormatTime = string.Empty;
            DateTime? timeLimit = null; // Use nullable DateTime
            int? timeRemaining = null; // Use nullable int

            if (timeSterilization != null && !string.IsNullOrEmpty(timeSterilization))
            {
                if (timeSterilization.Length > 6)
                {
                    stringFormatTime = "yyyyMMdd";
                }
                else
                {
                    stringFormatTime = "yyMMdd";
                }
                DateTime parsedTimeLimit;
                if (DateTime.TryParseExact(timeSterilization, stringFormatTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeLimit))
                {
                    timeLimit = parsedTimeLimit;
                    // Chỉ tính timeRemaining nếu timeSterilization có thể phân tích thành công
                    int timeLimitImported;
                    if (int.TryParse(timeSterilization, out timeLimitImported))
                    {
                        DateTime today = DateTime.Now;
                        string formattedDate = today.ToString(stringFormatTime); // Sử dụng stringFormatTime đã xác định
                        int timeNow;
                        if (int.TryParse(formattedDate, out timeNow))
                        {
                            timeRemaining = timeLimitImported - timeNow;
                        }
                    }
                }
            }

            var MAC = requestDataEink.EinkMac;

            var existEinkConnect = _ec.Links.Where(x => x.Mac == MAC && x.Variant != null)
                   .FirstOrDefault();
            if (existEinkConnect != null) {
                return BadRequest(new { message = "Đã dùng thẻ này vui lòng thử lại!" });
            }


            var existProductEink = _ec.TblProducts
                .Where(x => x.ItemCode == materialCode && 
                x.LotNo == lotMaterial && 
                x.Line == requestDataEink.Line.ToString() && 
                x.HeThong == "M+ GW" && x.MoTa == "Chia line NVL").FirstOrDefault();

            Guid productId = Guid.NewGuid();
            TblProduct newProductEink = new ();

            if (existProductEink != null)
            {
                existProductEink.RInt3 = requestDataEink.QtyDiv;
            } else
            {
                newProductEink = new TblProduct
                {
                    Iditem = productId,
                    ItemCode = materialCode,
                    Line = requestDataEink.Line.ToString(),
                    LotNo = lotMaterial,
                    RInt3 = requestDataEink.QtyDiv,
                    ChungLoaiSp = requestDataEink.ProductCode,
                    Remark1 = requestDataEink.ProductLot,
                    HanSuDung = timeLimit?.ToString("dd/MM/yyyy"),
                    HeThong = "M+ GW",
                    MoTa = "Chia line NVL",
                    RInt2 = timeRemaining,
                    MaCd = infoLocation?.LocationCode,
                    TenCd = infoLocation?.LocationName
                };
                _ec.TblProducts.Add(newProductEink);
                _ec.SaveChanges();
            }
            _ec.SaveChanges();
            // Link eink
            string endpoint = $"{einkUrl}/{MAC}/link/{newProductEink.Iditem}";
            var response = await PostLinkESL(endpoint, httpClient);
            if (response)
            {
                return Ok(new { message = "Link thành công ESl. Vui lòng đợi trong giây lát để xử lý hiển thị dữ liệu." });
            }
            else
            {
                return StatusCode(500, new { message = "Link dữ liệu với ESL lỗi. Vui lòng liên hệ Admin để xử lý." });
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


        /// <summary>
        /// Lưu thông tin chia line
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] RequestData requestData)
        {
            if (requestData.JsonStr == null)
            {
                return BadRequest("Request json is empty. Please try again");
            }
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }
                ItemLineSaved[]? itemLineSaveds = JsonConvert.DeserializeObject<ItemLineSaved[]>(requestData.JsonStrDivLine ?? "");
                var idLocation = await (from l in _context.TblLocations
                                        where l.LocationCode == requestData.ProcessCode
                                        select l.IdLocation).FirstOrDefaultAsync();
                if (itemLineSaveds?.Length > 0)
                {
                    foreach (var item in itemLineSaveds)
                    {
                        var workOrder = item.WorkOrder;
                        var checkItem = await _context.TblDivLineProds
                            .FirstOrDefaultAsync(x => x.WorkOrder == workOrder);
                        var updateCharacter = await _context.TblPreImportItems
                            .FirstOrDefaultAsync(x => x.WorkOrder == workOrder);
                        DateTime.TryParseExact(item.DateProd, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateProd);
                        if (checkItem != null)
                        {
                            checkItem.Character = item.Character ?? "".ToUpper();
                            checkItem.Line1 = item.Line1;
                            checkItem.Line2 = item.Line2;
                            checkItem.Line3 = item.Line3;
                            checkItem.Line4 = item.Line4;
                            checkItem.ChangeControl = item.ChangeControl;
                            checkItem.Note = item.Note;
                        }
                        else
                        {
                            var std = new TblDivLineProd()
                            {
                                WorkOrder = item.WorkOrder,
                                ItemCode = item.ItemCode,
                                LotNo = item.LotNo,
                                QtyUsed = item.QtyUsed,
                                Character = item.Character ?? "".ToUpper(),
                                DateProd = dateProd,
                                TimeProd = item.TimeProd,
                                Line1 = item.Line1,
                                Line2 = item.Line2,
                                Line3 = item.Line3,
                                Line4 = item.Line4,
                                ChangeControl = item.ChangeControl,
                                Note = item.Note,
                                IdUser = idUser,
                                IdLocation = idLocation
                            };
                            _context.TblDivLineProds.Add(std);
                        }
                        if (updateCharacter != null)
                        {
                            await (from s in _context.TblPreImportItems
                                   where s.WorkOrder == item.WorkOrder && s.IdLocation == idLocation
                                   select s
                          ).ForEachAsync(x =>
                          {
                              x.CharacterAlp = item.Character ?? "".ToUpper();
                          });
                        }
                    }
                    _context.SaveChanges();
                }
                return Ok(new { message = "Lưu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Lưu thông tin tính chia line
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveCalcProductionTime([FromBody] RequestData requestData)
        {
            if (requestData == null)
            {
                return BadRequest(new { message = "Not Found" });
            }

            var jsonData = requestData.JsonStrDivLine;
            var processCode = requestData.ProcessCode;

            List<ProductionTimeData>? prodListTimes = JsonConvert.DeserializeObject<List<ProductionTimeData>>(jsonData ?? "");

            // Lấy thời gian làm việc và nghỉ theo location hiện tại
            // Lưu ca làm việc
            TimeSpan? ShiftStart;
            TimeSpan? ShiftEnd;
            // Lưu khoảng nghỉ của các ca
            List<BreakSchedule> breakSchedules = new();
            if (processCode == "01050")
            {
                ShiftStart = _context.TblShiftSchedules
                    .Where(x => x.LocationCode == processCode)
                    .Select(s => s.ShiftStartTime).FirstOrDefault();
                ShiftEnd = _context.TblShiftSchedules
                  .Where(x => x.LocationCode == processCode)
                  .Select(s => s.ShiftEndTime).FirstOrDefault();
                breakSchedules = _context.TblShiftSchedules
                    .Where(x => x.LocationCode == processCode)
                    .Select(s => new BreakSchedule
                    {
                        StartBreakTime = s.BreakStartTime,
                        EndBreakTime = s.BreakEndTime
                    }).ToList();
            } else
            {
                ShiftStart = _context.TblShiftSchedules
                   .Where(x => x.LocationCode == "others")
                   .Select(s => s.ShiftStartTime).FirstOrDefault();
                ShiftEnd = _context.TblShiftSchedules
                  .Where(x => x.LocationCode == "others")
                  .Select(s => s.ShiftEndTime).FirstOrDefault();
                breakSchedules = _context.TblShiftSchedules
                  .Where(x => x.LocationCode == "others")
                  .Select(s => new BreakSchedule
                  {
                      StartBreakTime = s.BreakStartTime,
                      EndBreakTime = s.BreakEndTime
                  }).ToList();
            }
            List<ProductionTimeData> afterCalcResults = new();
            afterCalcResults = ProductionCalculator.ProcessProductionQueue(prodListTimes, ShiftStart ?? TimeSpan.Zero, ShiftEnd ?? TimeSpan.Zero, breakSchedules);
            string formatDateTime = "dd/MM/yyyy HH:mm";
            List<TblCalcTimeDivLine> tblCalcTimeDivLine = new();
            foreach (var dataCalc in afterCalcResults)
            {
                foreach (var itemLine in dataCalc.ProductionLines)
                {
                    DateTime.TryParseExact(itemLine.StartDate, formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime);
                    DateTime.TryParseExact(itemLine.EndDate, formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
                    tblCalcTimeDivLine.Add(new TblCalcTimeDivLine
                    {
                        SoTt = dataCalc.IndexNumber,
                        WorkOrder = dataCalc.WorkOrder,
                        LineNumber = itemLine.DataLine,
                        SoLuongDuDinh = itemLine.Qty,
                        ThoiGianSanXuat = (decimal?)itemLine.Time,
                        NgayDuDinhSanXuat = startTime,
                        NgayKetThuc = endDate,
                        SoLuongTrenNgay = itemLine.QtyInDay,
                        Character = dataCalc.Character,
                    });
                }
            }
            foreach (var itemSaved in tblCalcTimeDivLine)
            {
                var existItem = _context.TblCalcTimeDivLines
                    .Where(x => x.WorkOrder == itemSaved.WorkOrder && x.LineNumber == itemSaved.LineNumber)
                    .FirstOrDefault();
                var locationCode = _context.TblWorkOrderMes.Where(x => x.WorkOrder == itemSaved.WorkOrder).Select(s => s.ProgressOrder).FirstOrDefault();
                if (existItem != null)
                {
                    existItem.SoTt = itemSaved.SoTt;
                    existItem.ThoiGianSanXuat = itemSaved.ThoiGianSanXuat;
                    existItem.SoLuongDuDinh = itemSaved.SoLuongDuDinh;
                    existItem.NgayDuDinhSanXuat = itemSaved.NgayDuDinhSanXuat;
                    existItem.NgayKetThuc = itemSaved.NgayKetThuc;
                    existItem.SoLuongTrenNgay = itemSaved.SoLuongTrenNgay;
                    existItem.LocationCode = locationCode;
                    existItem.Character = itemSaved.Character;
                } else
                {
                    _context.TblCalcTimeDivLines.Add(new TblCalcTimeDivLine
                    {
                        SoTt = itemSaved.SoTt,
                        WorkOrder = itemSaved.WorkOrder,
                        LineNumber = itemSaved.LineNumber,
                        SoLuongDuDinh = itemSaved.SoLuongDuDinh,
                        ThoiGianSanXuat = itemSaved.ThoiGianSanXuat,
                        NgayDuDinhSanXuat = itemSaved.NgayDuDinhSanXuat,
                        NgayKetThuc = itemSaved.NgayKetThuc,
                        SoLuongTrenNgay = itemSaved.SoLuongTrenNgay,
                        LocationCode = locationCode,
                        Character = itemSaved.Character,
                    });
                }

                var getWOMES = _context.TblWorkOrderMes.Where(x => x.WorkOrder == itemSaved.WorkOrder).ToList();
                if (getWOMES.Count > 0)
                {
                    DateTime? dateProdSplit = itemSaved.NgayDuDinhSanXuat?.Date;
                    TimeSpan? timeProdSplit = itemSaved.NgayDuDinhSanXuat?.TimeOfDay;
                    foreach (var itemWO in getWOMES)
                    {
                        itemWO.Character = itemSaved.Character;
                        itemWO.DateProd = dateProdSplit;
                        itemWO.TimeProd = timeProdSplit;
                    }
                }
            }
            _context.SaveChanges();
            var totalsPerLine = new Dictionary<int, int>();
            var getLines = _context.TblProdLines.Select(s => s.LineName).ToList();
            foreach (var dataItem in afterCalcResults)
            {
                foreach (var item in dataItem.ProductionLines)
                {
                    string dataLineName = "Line " + item.DataLine;
                    if (getLines.Contains(dataLineName))
                    {
                        if (totalsPerLine.ContainsKey(item.DataLine))
                        {
                            totalsPerLine[item.DataLine] += item.Qty;
                        } else
                        {
                            totalsPerLine.Add(item.DataLine, item.Qty);
                        }
                    }
                }
            }
            return Ok(new { message = "Đã tính xong. Nhấn Ok để xem kết quả.", dataRender = afterCalcResults, totalLines = totalsPerLine });
        }

        /// <summary>
        /// Update khi có sửa thông tin
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateCalcTimes([FromBody] RequestData requestData)
        {
            if(requestData == null)
            {
                return BadRequest(new { message = "Not Found!" });
            }
            try
            {
                var jsonData = requestData.JsonStrDivLine;
                var processCode = requestData.ProcessCode;
                string formatDateTime = "dd/MM/yyyy HH:mm";
                List<ProductionTimeData>? prodListTimes = JsonConvert.DeserializeObject<List<ProductionTimeData>>(jsonData ?? "");
                if (prodListTimes?.Count > 0)
                {
                    foreach (var itemSaved in prodListTimes)
                    {
                        var getWOMES = _context.TblWorkOrderMes.Where(x => x.WorkOrder == itemSaved.WorkOrder).ToList();
                        var locationCode = _context.TblWorkOrderMes.Where(x => x.WorkOrder == itemSaved.WorkOrder).Select(s => s.ProgressOrder).FirstOrDefault();
                        var dataProductionLines = itemSaved.ProductionLines;
                        foreach (var item in dataProductionLines)
                        {
                            DateTime.TryParseExact(item.StartDate, formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime);
                            DateTime.TryParseExact(item.EndDate, formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
                            var existItem = _context.TblCalcTimeDivLines
                                .Where(x => x.WorkOrder == itemSaved.WorkOrder && x.LineNumber == item.DataLine)
                                .FirstOrDefault();
                            if (existItem != null)
                            {
                                existItem.SoTt = itemSaved.IndexNumber;
                                existItem.ThoiGianSanXuat = (decimal?)item.Time;
                                existItem.SoLuongDuDinh = item.Qty;
                                existItem.NgayDuDinhSanXuat = startTime;
                                existItem.NgayKetThuc = endDate;
                                existItem.SoLuongTrenNgay = item.QtyInDay;
                                existItem.LocationCode = locationCode;
                                existItem.Character = itemSaved.Character;
                            }
                            else
                            {
                                _context.TblCalcTimeDivLines.Add(new TblCalcTimeDivLine
                                {
                                    SoTt = itemSaved.IndexNumber,
                                    WorkOrder = itemSaved.WorkOrder,
                                    LineNumber = item.DataLine,
                                    SoLuongDuDinh = item.Qty,
                                    ThoiGianSanXuat = (decimal?)item.Time,
                                    NgayDuDinhSanXuat = startTime,
                                    NgayKetThuc = endDate,
                                    SoLuongTrenNgay = item.QtyInDay,
                                    LocationCode = locationCode,
                                    Character = itemSaved.Character,
                                });
                            }

                            if (getWOMES.Count > 0)
                            {
                                DateTime? dateProdSplit = startTime.Date;
                                TimeSpan? timeProdSplit = startTime.TimeOfDay;
                                foreach (var itemWO in getWOMES)
                                {
                                    itemWO.Character = itemSaved.Character;
                                    itemWO.DateProd = dateProdSplit;
                                    itemWO.TimeProd = timeProdSplit;
                                }
                            }
                        }
                    }
                  
                }
                _context.SaveChanges();
                Log.Information($"Cập nhật thành công những thay đổi về dữ liệu của các WO {DateTime.Now}");
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (SqlException odbEx)
            {
                Log.Fatal($"Lỗi lưu dữ liệu vào database: {odbEx.Message}");
                throw;
            }
            catch (Exception ex) {
                Log.Fatal($"Lỗi cập nhật lại thời gian tại bảng chia line: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
           
        }
        #endregion Kết thúc xử lý chia line NVL cho leader

        #region Thao tác kiểm tra trước
        /// <summary>
        /// Hiển thị view kiểm tra trước
        /// </summary>
        /// <returns></returns>
        [Route("/gia-cong/kiem-tra-truoc")]
        public async Task<IActionResult> PreOperation()
        {
            var positionCurrent = HttpContext.Session.GetString("PositionWorking")?.ToString();
            var locationId = await (from l in _context.TblLocations
                                   join mps in _context.TblMasterPositions on l.IdLocation equals mps.IdLocation
                                   where mps.PositionCode == positionCurrent
                                   select l.IdLocation).FirstOrDefaultAsync();

            // Lấy tần suất cần kiểm tra tại vị trí
            var locationChildId = await (from l in _context.TblLocationCs
                                    join mps in _context.TblMasterPositions on l.Id equals mps.LocationChildId
                                    where mps.PositionCode == positionCurrent
                                    select l.Id).FirstOrDefaultAsync();
            var listFrequencies = await (from f in _context.TblTansuats
                                         join lf in _context.TblLocationTansuats on f.Id equals lf.IdTansuat
                                         join lc in _context.TblLocationCs on lf.IdLocationc equals lc.Id
                                         where lc.Id == locationChildId
                                         select new TblTansuat
                                         {
                                             Id = f.Id,
                                             Name = f.Name,
                                         }).ToListAsync();
            ViewData["ListFrequencies"] = listFrequencies;
            return View();
        }
        #endregion Kết thúc kiểm tra trước

        [Route("/gia-cong/gia-cong-dau-mut")]
        public async Task<IActionResult> GuidewireProduction()
        {
            var positionCurrent = HttpContext.Session.GetString("PositionWorking")?.ToString();
            var locationId = await(from l in _context.TblLocations
                                   join mps in _context.TblMasterPositions on l.IdLocation equals mps.IdLocation
                                   where mps.PositionCode == positionCurrent
                                   select l.IdLocation).FirstOrDefaultAsync();

            // Lấy tần suất cần kiểm tra tại vị trí
            var locationChildId = await(from l in _context.TblLocationCs
                                        join mps in _context.TblMasterPositions on l.Id equals mps.LocationChildId
                                        where mps.PositionCode == positionCurrent
                                        select l.Id).FirstOrDefaultAsync();
            var listFrequencies = await(from f in _context.TblTansuats
                                        join lf in _context.TblLocationTansuats on f.Id equals lf.IdTansuat
                                        join lc in _context.TblLocationCs on lf.IdLocationc equals lc.Id
                                        where lc.Id == locationChildId
                                        select new TblTansuat
                                        {
                                            Id = f.Id,
                                            Name = f.Name,
                                        }).ToListAsync();
            ViewData["ListFrequencies"] = listFrequencies;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Log.Error("Error: {ID}", Activity.Current?.Id);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class RequestDataEink
    {
        public int Line { get; set; }
        public string ProductCode { get; set; }
        public string ProductLot { get; set; }
        public string MaterialCode { get; set; }
        public string LotMaterial { get; set; }
        public string EinkMac { get; set; }
        public int QtyDiv { get; set; }
    }

    public class DataCheckCondition
    {
        public string Thicknessgauge { get; set; }
        public string LimitedSample { get; set; }
        public string RulerCode { get; set; }
    }

    public class ToolInfo
    {
        public string? ToolCode { get; set; }
        public string? ToolName { get; set; }
    }

    public class FormFieldRender
    {
        public int OrderForm { get; set; }
        public int IdForm { get; set; }
        public List<FormFieldMappingRender> FormFields { get; set; }
    }

    public class FormFieldMappingRender
    {
        public string? SectionId { get; set; }
        public string? FieldName {  get; set; }
        public string? Label { get; set; }
        public string? TypeInput { get; set; }
        public string? ColClass { get; set; }
        public string? ElementId { get; set; }
        public bool Hidden { get; set; }
    }
    public class RequestUpdateTrayError
    {
        public string TrayNo { get; set; }
        public string ErrorInfo { get; set; }
        public string WorkOrderProd { get; set; }
        public string LotMaterial { get; set; }
        public string MaterialCode { get; set; }
        public string PositionCode { get; set; }
        public string EinkMac { get; set; }
    }

    public class RequestGetMenuChild
    {
        public int? ErrorId { get; set; }
        public string? ErrorName { get; set; }
    }

    public class MenuParent
    {
        public int? Id { get; set; }
        public string? ErrorName { get; set; }
        public List<MenuChild> MenuChilds { get; set; }
    }

    public class MenuChild
    {
        public int? Id { get; set; }
        public string? ErrorName { get; set; }
        public int? IdParent { get; set; }
        public List<MenuChild2> MenuChilds2 { get; set; }
    }

    public class MenuChild2
    {
        public int? Id { get; set; }
        public string? ErrorName { get; set; }
        public int? IdParent { get; set; }
        public List<MenuChild3> MenuChilds3 { get; set; }
    }
    public class MenuChild3
    {
        public int? Id { get; set; }
        public string? ErrorName { get; set; }
        public int? IdParent { get; set; }
    }

    public class ProductionTimeData
    {
        public int IndexNumber { get; set; }
        public string WorkOrder { get; set; }
        public string ProductCode { get; set; }
        public string LotNo { get; set; }
        public int QtyWo { get; set; }
        public string Character { get; set; }
        public double CycleTime { get; set; }
        public List<ProductionLineTimeData> ProductionLines { get; set; }
    }
    public class ProductionLineTimeData
    {
        public int DataLine { get; set; }
        public int Qty { get; set; }
        public double Time { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int QtyInDay { get; set; }
    }
}
