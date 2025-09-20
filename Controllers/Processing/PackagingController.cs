using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using System.Data;

namespace MPLUS_GW_WebCore.Controllers.Processing
{
    public class PackagingController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly ConnectMES.Classa _cl;
        public readonly IWebHostEnvironment _environment;

        public PackagingController(MplusGwContext context, ConnectMES.Classa classa, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("/dong-goi")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Đóng gói", Url = Url.Action("Index", "Packaging"), IsActive = true },
            };
     
            List<ListWOStatusInMes> listWOStatusInMes = new();
            List<ListWoProduction> listWoProductions = new();
            List<ListWoDivLine> listWoDivLines = new();
            List<ListDivLineForLot> listDivLineForLot = new();
            var locationCode = await (from l in _context.TblLocations
                                      where l.LocationName == "Đóng Gói"
                                      select l.LocationCode).ToListAsync();
            if (locationCode != null)
            {
                var loadData = await _context.TblWorkOrderMes
                    .Where(x => locationCode.Contains(x.ProgressOrder) && (x.DateProd != null && x.TimeProd != null))
                    .Select(s => new
                    {
                        s.WorkOrder,
                        s.ItemCode,
                        s.LotNo,
                        s.QtyWo,
                        s.Character,
                        s.DateProd,
                        s.TimeProd,
                        s.ProgressOrder,
                        s.Statusname,
                    }).ToListAsync();
                var groupByWorkorder = loadData.GroupBy(x => x.WorkOrder)
                    .Select(s => new
                    {
                        s.Key,
                        s.FirstOrDefault()?.ItemCode,
                        s.FirstOrDefault()?.LotNo,
                        s.FirstOrDefault()?.QtyWo,
                        s.FirstOrDefault()?.Character,
                        s.FirstOrDefault()?.DateProd,
                        s.FirstOrDefault()?.TimeProd,
                        s.FirstOrDefault()?.ProgressOrder,
                        s.FirstOrDefault()?.Statusname,
                    }).ToList();
                foreach (var item in groupByWorkorder)
                {
                    string workOrder = item.Key ?? "";
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

                    listWoProductions.Add(new ListWoProduction
                    {
                        SoTT = indexNumber ?? "",
                        WorkOrderNo = workOrder,
                        ProductCode = item.ItemCode,
                        LotNo = item.LotNo,
                        QtyProd = item.QtyWo.ToString(),
                        Character = item.Character,
                        DateProd = item.DateProd,
                        TimeProd = item.TimeProd?.ToString(@"hh\:mm"),
                        ProcessCode = item.ProgressOrder,
                        StatusName = item.Statusname,
                        ProductionLines = productionLineTimes
                    });
                    var oldResult = await (from s in _context.TblDivLineProds
                                           where s.WorkOrder == workOrder
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
                    if (oldResult != null)
                    {
                        foreach (var itemOld in oldResult)
                        {
                            var std = new ListWoDivLine
                            {
                                WorkOrder = itemOld.WorkOrder,
                                Line1 = itemOld.Line1.ToString(),
                                Line2 = itemOld.Line2.ToString(),
                                Line3 = itemOld.Line3.ToString(),
                                Line4 = itemOld.Line4.ToString(),
                                ChangeControl = itemOld.ChangeControl,
                                Note = itemOld.Note,
                            };
                            listWoDivLines.Add(std);

                            //Lấy Workorder đã chia line theo NVL
                            var getWODivLineLot = await (from s in _context.TblDivLineForLots
                                                         where s.WorkOrder == itemOld.WorkOrder
                                                         select s).ToListAsync();
                            foreach (var item1 in getWODivLineLot)
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
    }
}
