using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
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
                    listWoProductions.Add(new ListWoProduction
                    {
                        WorkOrderNo = item.Key,
                        ProductCode = item.ItemCode,
                        LotNo = item.LotNo,
                        QtyProd = item.QtyWo.ToString(),
                        Character = item.Character,
                        DateProd = item.DateProd,
                        TimeProd = item.TimeProd?.ToString(@"hh\:mm"),
                        ProcessCode = item.ProgressOrder,
                        StatusName = item.Statusname,
                    });
                    var oldResult = await (from s in _context.TblDivLineProds
                                           where s.WorkOrder == item.Key
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
                .Where(x => x.DateProd != null && x.TimeProd != null && 
                x.DateProd != DateTime.MinValue &&
                x.StatusName != "Production end")
                .OrderBy(x => x.DateProd)
                .ThenBy(x => x.TimeProd).ToList();

            ViewData["ListWoProduction"] = listWoProductions;
            ViewData["ListWoDivLine"] = listWoDivLines;
            ViewData["ListDivLineForLot"] = listDivLineForLot;
            return View();
        }
    }
}
