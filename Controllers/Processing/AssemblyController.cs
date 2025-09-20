using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace MPLUS_GW_WebCore.Controllers.Processing
{
    public class AssemblyController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public readonly IWebHostEnvironment _environment;

        public AssemblyController(MplusGwContext context, Classa classa, IWebHostEnvironment hostEnvironment) {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("/lap-rap")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Lắp ráp", Url = Url.Action("Index", "Assembly"), IsActive = true },
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
                                      where l.LocationName == "In Chuôi"
                                      select new
                                      {
                                          l.LocationCode,
                                          l.IdLocation
                                      }).FirstOrDefaultAsync();
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
                        if (getDataCalcTimeProds.Count > 0)
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
                var idLocationProcessing = await (from l in _context.TblLocations
                                        where l.LocationCode == "01050"
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

                        string itemCodeWithCut = item.ItemCode != null && item.ItemCode.Length >= 11 ? item.ItemCode.Substring(4, 7) : "";
                        string lotNoSearch = item.LotNo != null && item.LotNo.Length >= 5 ? item.LotNo.Substring(0, 6) : "";
                        //string itemCodeSearch = itemCodeWithCut.Length > 1 ? itemCodeWithCut.Substring(0, itemCodeWithCut.Length - 1) : "";

                        DateTime.TryParseExact(item.DateProd, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateProd);

                        var existItem = await _context.TblDivLineProds
                            .Where(x => x.ItemCode != null && x.ItemCode.Contains(itemCodeWithCut) 
                            && x.LotNo != null && x.LotNo.Contains(lotNoSearch)
                            && x.IdLocation != idLocation && x.IdLocation != idLocationProcessing)
                            .FirstOrDefaultAsync();

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

                        if (existItem != null)
                        {
                            existItem.Character = item.Character ?? "".ToUpper();
                            existItem.Line1 = item.Line1;
                            existItem.Line2 = item.Line2;
                            existItem.Line3 = item.Line3;
                            existItem.Line4 = item.Line4;
                            await (from s in _context.TblPreImportItems
                             where s.WorkOrder == existItem.WorkOrder
                             select s).ForEachAsync(x =>
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

    }
}
