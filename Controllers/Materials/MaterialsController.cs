using Azure;
using ConnectMES;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;
using System.IO;
using OfficeOpenXml.Style;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.WebSockets;
using Hangfire;
using MPLUS_GW_WebCore.Controllers.Processing;

namespace MPLUS_GW_WebCore.Controllers.Materials
{
    public class MaterialsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public readonly IWebHostEnvironment _environment;
        public readonly static List<TblSubMaterial> _tblSubMaterials = new();
        public MaterialsController(MplusGwContext context, Classa classa, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("nguyen-vat-lieu")]
        public IActionResult Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = true },
            };
            return View();
        }

        [Route("nguyen-vat-lieu/chuan-bi")]
        public async Task<IActionResult> PrepareForMaterials()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Chuẩn bị", Url = Url.Action("PrepareForMaterials", "Materials"), IsActive = true },
            };
            var processProd = await _context.TblLocations
                .Select(s => new
                {
                    IdProcess = s.IdLocation.ToString(),
                    ProcessCode = s.LocationCode,
                    ProcessName = s.LocationName,
                }).ToListAsync();
            var groupProcess = processProd.GroupBy(s => s.ProcessName)
                .Select(x => new CustomRenderProcess
                {
                    ProcessName = x.Key,
                    IdProcess = string.Join(",", x.Select(s => s.IdProcess).ToArray()),
                    ProcessCode = string.Join(",", x.Select(s => s.ProcessCode).ToArray()),
                }).ToList();
            return View(groupProcess);
        }

        [HttpPost]
        public async Task<IActionResult> GetWorkOrder([FromBody] RequestData processRequest)
        {
            if(processRequest.ProcessCode == null)
            {
                return BadRequest("Not Found");
            }

            var arrProcessCode = processRequest.ProcessCode.Contains(',') ? processRequest.ProcessCode.Split(',') : new string[] { processRequest.ProcessCode };


            var listWorkOrder = _context.TblWorkOrderMes
                .Where(x => x.Statusname == "Order Approval End"
                && arrProcessCode.Contains(x.ProgressOrder))
                .GroupBy(x => x.WorkOrder)
                .Select(x => new
                {
                    workOrderNo = x.Key,
                    processCode = x.First().ProgressOrder,
                    productCode = x.First().ItemCode,
                    productName = x.First().ItemName,
                    lotNo = x.First().LotNo,
                    orderedValue = x.First().QtyWo,
                    character = x.First().Character ?? "",
                    dateProd = x.First().DateProd,
                    timeProd = x.First().TimeProd,
                }).ToList();

            foreach (var processItem in arrProcessCode)
            {
                try
                {
                    await SaveWOWithCharacterAsync(processItem);
                }
                catch (Exception ex)
                {
                    var errorMessage = new
                    {
                        message = ex.Message,
                    };
                    return StatusCode(500, errorMessage);
                }

            }

            var locationId = await _context.TblLocations
                .Where(x => arrProcessCode.Contains(x.LocationCode))
                .Select(x => x.IdLocation.ToString())
                .ToListAsync();

            var getWOAdded = await (from s in _context.TblPreImportItems
                                    join c in _context.TblDetailsPreMaterials on s.Id equals c.IdItemImport
                                    where locationId.Contains(s.IdLocation.ToString() ?? "") && s.WorkOrder == c.WorkOrder
                                    select new
                                    {
                                        s.WorkOrder,
                                        s.ItemCode,
                                        s.LotNo,
                                        s.CharacterAlp,
                                        s.DateProd,
                                        s.TimeProd,
                                        c.DateImport,
                                        c.TimeImport,
                                        c.QtyImport,
                                        s.ProgressMes,
                                    }).ToListAsync();

            var filterOldData = (from s in listWorkOrder
                                 join l in getWOAdded on s.workOrderNo equals l.WorkOrder
                                 select new
                                 {
                                     l.WorkOrder,
                                     l.ItemCode,
                                     l.LotNo,
                                     s.character,
                                     s.dateProd,
                                     s.timeProd,
                                     l.ProgressMes,
                                     l.DateImport,
                                     l.QtyImport,
                                     l.TimeImport
                                 }).ToList();
            var groupOldData = filterOldData.GroupBy(x => x.WorkOrder)
                .Select(x => new
                {
                    WorkOrder = x.Key,
                    x.FirstOrDefault()?.ItemCode,
                    x.FirstOrDefault()?.LotNo,
                    x.FirstOrDefault()?.character,
                    x.FirstOrDefault()?.dateProd,
                    x.FirstOrDefault()?.timeProd,
                    x.FirstOrDefault()?.ProgressMes,
                    rows = x.Select(s => new
                    {
                        s.WorkOrder,
                        s.QtyImport,
                        s.DateImport,
                        s.TimeImport
                    }).Distinct().OrderBy(x => x.DateImport).ToList()
                }).ToList();
            var response = new
            {
                workOrder = listWorkOrder,
                oldData = groupOldData,
            };
            
            return Ok(response);

        }

        [HttpPost]
        public async Task<IActionResult> SaveDataInDb([FromBody] RequestData request)
        {
            if (string.IsNullOrWhiteSpace(request.JsonStr))
            {
                return BadRequest(new { message = "Input JSON string cannot be null or empty." });
            }

            ListItemAdd[]? listItemAdds = JsonConvert.DeserializeObject<ListItemAdd[]>(request.JsonStr);
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }
                if (listItemAdds?.Any() == true)
                {
                    foreach (var item in listItemAdds)
                    {
                        DateTime.TryParseExact(item.DateIntended, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateIntended);
                        TimeSpan timeIntended = TimeSpan.Parse(item.TimeIntended??"".ToString());
                        var existsWO = await _context.TblPreImportItems
                            .Where(x => x.WorkOrder == item.WorkOrder)
                            .FirstOrDefaultAsync();
                        var checkWOInput = await _context.TblWorkOrderMes
                           .Where(s => s.WorkOrder == item.WorkOrder)
                           .ToListAsync();
                        var hasItemDivLine = await _context.TblDivLineProds
                                             .Where(s => s.WorkOrder == item.WorkOrder)
                                             .FirstOrDefaultAsync();

                        if (item.ProcessCode == "01065")
                        {
                            var existCharacterDivMC = _context.TblDivMcprods
                                .Where(x => x.WorkOrder == item.WorkOrder &&
                                x.ProductCode == item.ItemCode &&
                                x.LotNo == item.Lotno)
                                .FirstOrDefault();
                            if(string.IsNullOrEmpty(existCharacterDivMC?.Character))
                            {
                                if (existCharacterDivMC != null)
                                {
                                    existCharacterDivMC.Character = item.Character;
                                }
                            }
                        }
                        if (existsWO != null)
                        {
                            UpdateExistingWorkOrderPreItem(existsWO, item, dateIntended, timeIntended);

                            //Cập nhật thông tin khi thêm dữ liệu chuẩn bị NVL
                            await ProcessItemRows(item.Rows, existsWO.Id);
                            // Đóng cập nhật update chia line cho đóng gói khi lắp ráp nhập thông tin chia line
                            //if (item.ProcessCode == "01070" || item.ProcessCode == "01075" || item.ProcessCode == "01074")
                            //{
                            //    await ProcessDivLine(item, hasItemDivLine, idUser, dateIntended, timeIntended);
                            //}
                        }
                        else
                        {
                            var newItem = CreateNewItemPre(item, idUser, dateIntended, timeIntended);
                            _context.TblPreImportItems.Add(newItem);
                            await _context.SaveChangesAsync();

                            //Cập nhật thông tin khi thêm dữ liệu chuẩn bị NVL
                            await ProcessItemRows(item.Rows, newItem.Id);
                            // Đóng cập nhật update chia line cho đóng gói khi lắp ráp nhập thông tin chia line
                            //if (item.ProcessCode == "01070" || item.ProcessCode == "01075" || item.ProcessCode == "01074")
                            //{
                            //    await ProcessDivLine(item, hasItemDivLine, idUser, dateIntended, timeIntended);
                            //}
                            if (checkWOInput != null)
                            {
                                UpdateWorkOrderInputs(checkWOInput, item, dateIntended, timeIntended);
                            }
                        }
                    }
                }
                try
                {
                    await _context.SaveChangesAsync();
                    var response = new
                    {
                        message = "Lưu thành công"
                    };
                    return Ok(response);
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, new { message = ex.InnerException?.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        /// <summary>
        /// Thêm dữ liệu thực hiện chuẩn bị NVL
        /// </summary>
        /// <param name="item"></param>
        /// <param name="idUser"></param>
        /// <param name="dateProd"></param>
        /// <param name="timeProd"></param>
        /// <returns></returns>
        private TblPreImportItem CreateNewItemPre(ListItemAdd item, int idUser, DateTime dateProd, TimeSpan timeProd)
        {
            return new TblPreImportItem
            {
                WorkOrder = item.WorkOrder,
                ItemCode = item.ItemCode,
                LotNo = item.Lotno,
                Qty = int.Parse(item.Qty??"".ToString()),
                CharacterAlp = item.Character??"".ToUpper(),
                DateImport = DateTime.Now.Date,
                DateProd = dateProd,
                TimeProd = timeProd,
                ProgressMes = item.ProcessCode,
                IdUser = idUser,
                IdLocation = GetIDProcess(item.ProcessCode??""),
            };
        }

        /// <summary>
        /// Cập nhật thông tin workorder đã nhập ngày nhập nguyên vật liệu tại chuẩn bị nvl
        /// </summary>
        /// <param name="workOrder"></param>
        /// <param name="item"></param>
        /// <param name="dateProd"></param>
        /// <param name="timeProd"></param>
        private static void UpdateExistingWorkOrderPreItem(TblPreImportItem workOrder, ListItemAdd item, DateTime dateProd, TimeSpan timeProd)
        {
            workOrder.CharacterAlp = item.Character??"".ToUpper();
            workOrder.DateProd = dateProd;
            workOrder.TimeProd = timeProd;
            workOrder.DateImport = DateTime.Now.Date;
        }

        /// <summary>
        /// Xử lý item trong rows khi thực hiện nhập ngày xuất NVL tại chuẩn bị nvl
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private async Task ProcessItemRows(List<RowAddItem> rows, int itemId)
        {
            foreach (var row in rows.Where(r => !string.IsNullOrEmpty(r.DateInput) && !string.IsNullOrEmpty(r.TimeInput) && !string.IsNullOrEmpty(r.QtyImport)))
            {
                DateTime.TryParseExact(row.DateInput, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateImportItem);
                TimeSpan.TryParse(row.TimeInput, out TimeSpan timeImportItem);

                var existingItems = await _context.TblDetailsPreMaterials
                    .Where(x => x.WorkOrder == row.WorkOrder && x.DateImport.HasValue && x.DateImport.Value.Date == dateImportItem.Date && x.TimeImport == timeImportItem)
                    .ToListAsync();

                if (existingItems.Any())
                {
                    foreach (var existingItem in existingItems)
                    {
                        existingItem.QtyImport = int.Parse(row.QtyImport);
                        existingItem.DateImport = dateImportItem;
                        existingItem.TimeImport = timeImportItem;
                    }
                }
                else
                {
                    var materialCodes = await _context.TblWorkOrderMes
                        .Where(x => x.WorkOrder == row.WorkOrder)
                        .Select(x => x.InputGoodsCodeMes)
                        .ToListAsync();

                    foreach (var materialCode in materialCodes)
                    {
                        var newDetail = new TblDetailsPreMaterial
                        {
                            WorkOrder = row.WorkOrder,
                            DateImport = dateImportItem,
                            TimeImport = timeImportItem,
                            QtyImport = int.Parse(row.QtyImport),
                            StatusExported = "Checked",
                            IdItemImport = itemId,
                            MaterialCode = materialCode
                        };
                        _context.TblDetailsPreMaterials.Add(newDetail);
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý chia line cho các nguyên vật liệu tại đóng gói khi thực hiện chuẩn bị NVL
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="existingDivLine"></param>
        /// <param name="idUser"></param>
        /// <param name="dateIntended"></param>
        /// <param name="timeIntended"></param>
        /// <returns></returns>
        private async Task ProcessDivLine(ListItemAdd item, TblDivLineProd? existingDivLine, int idUser, DateTime dateIntended, TimeSpan timeIntended)
        {
            string itemCodeWithCut = item.ItemCode != null && item.ItemCode.Length >= 11 ? item.ItemCode.Substring(4, 7) : "";
            string lotNoSearch = item.Lotno != null && item.Lotno.Length >= 5 ? item.Lotno.Substring(0, 6) : "";

            int idLocationCheck = _context.TblLocations.Where(x => x.LocationCode == "01060").Select(x => x.IdLocation).FirstOrDefault();

            var divLine = await _context.TblDivLineProds
                .FirstOrDefaultAsync(x => x.ItemCode != null && x.LotNo != null && x.ItemCode.Contains(itemCodeWithCut) && x.LotNo.Contains(lotNoSearch) && x.IdLocation == idLocationCheck);

            if (existingDivLine == null)
            {
                var newDivLine = new TblDivLineProd
                {
                    WorkOrder = item.WorkOrder,
                    ItemCode = item.ItemCode,
                    LotNo = item.Lotno,
                    QtyUsed = int.Parse(item.Qty??""),
                    Character = item.Character??"".ToUpper(),
                    DateProd = dateIntended,
                    TimeProd = timeIntended,
                    Line1 = divLine?.Line1 ?? 0,
                    Line2 = divLine?.Line2 ?? 0,
                    Line3 = divLine?.Line3 ?? 0,
                    Line4 = divLine?.Line4 ?? 0,
                    IdUser = idUser,
                    IdLocation = GetIDProcess(item.ProcessCode??"")
                };
                _context.TblDivLineProds.Add(newDivLine);
            }
        }

        /// <summary>
        /// Cập nhật lại thông tin cho bảng WorkOrderMes khi có chữ cái và ngày sản xuất
        /// 
        /// </summary>
        /// <param name="workOrders"></param>
        /// <param name="item"></param>
        /// <param name="dateProd"></param>
        /// <param name="timeProd"></param>
        private static void UpdateWorkOrderInputs(List<TblWorkOrderMe> workOrders, ListItemAdd item, DateTime dateProd, TimeSpan timeProd)
        {
            foreach (var workOrder in workOrders)
            {
                workOrder.DateProd = dateProd;
                workOrder.TimeProd = timeProd;
                workOrder.Character = item.Character;
            }
        }

        /// <summary>
        /// lấy id công đoạn bằng code công đoạn
        /// </summary>
        /// <param name="processCode"></param>
        /// <returns></returns>
        public int GetIDProcess(string processCode)
        {
            int id = _context.TblLocations
                .Where(x => x.LocationCode == processCode)
                .Select(x => x.IdLocation)
                .FirstOrDefault();
            return id;
        }

        [HttpPost]
        public async Task<IActionResult> SeeInventory([FromBody] RequestData processRequest)
        {
            var currentDate = DateTime.Now;
            var currentDay = currentDate.Date;
            List<ListItemFlInventory> listItems = new();
            try
            {
                if(processRequest.ProcessCode == null)
                {
                    return BadRequest("Not Found");
                }
                var arrProcessCode = processRequest.ProcessCode.Contains(',') ? processRequest.ProcessCode.Split(',') : new string[] { processRequest.ProcessCode };
                var returnAllRM = await _context.TblInventoryMes
                    .Where(x => arrProcessCode.Contains(x.LocationCode))
                    .Select(x => new
                    {
                        InputGoodsCode = x.ItemCode,
                        InputGoodsName = x.ItemName,
                        Inventory = x.Qty,
                        OrderList = x.Orderno,
                    }).ToListAsync();
                foreach (var processCode in arrProcessCode)
                {
                    var getWoCurrent = await (from s in _context.TblWorkOrderMes
                                              where s.ProgressOrder == processCode
                                              && s.ModifyDateUpdate == currentDay
                                              && s.Statusname != "Creating Orders"
                                              select new
                                              {
                                                  workOrder = s.WorkOrder,
                                                  productCode = s.ItemCode,
                                                  lotNo = s.LotNo,
                                                  qtyUsed = s.QtyWo,
                                                  qtyUnused = s.QtyUnused,
                                                  dateProd = s.DateProd.HasValue && s.TimeProd.HasValue ? s.DateProd.Value.Add(s.TimeProd.Value) : (DateTime?)null,
                                                  character = s.Character,
                                                  processCode = s.ProgressOrder,
                                                  statusname = s.Statusname,
                                                  inputGoodsCode = s.InputGoodsCodeMes,
                                                  inputGoodsCodeSeq = s.InputGoodsCodeSeq,
                                              }).ToListAsync();
                    if (getWoCurrent.Count > 0)
                    {
                        foreach (var itemCurrent in getWoCurrent)
                        {
                            listItems.Add(new ListItemFlInventory(
                            itemCurrent.workOrder,
                            itemCurrent.productCode,
                            itemCurrent.lotNo,
                            itemCurrent.character,
                            itemCurrent.qtyUsed.HasValue ? (int)itemCurrent.qtyUsed : 0,
                            itemCurrent.qtyUnused.HasValue ? (int)itemCurrent.qtyUnused : 0,
                            (DateTime?)itemCurrent.dateProd,
                            itemCurrent.processCode,
                            itemCurrent.statusname,
                            itemCurrent.inputGoodsCode,
                            itemCurrent.inputGoodsCodeSeq.HasValue ? (decimal)itemCurrent.inputGoodsCodeSeq : 0
                            ));
                        }
                    }
                }
                listItems = listItems
                    .GroupBy(x => new
                    {
                        x.WorkOrder,
                        x.ProductCode,
                        x.LotNo,
                        x.Character,
                        x.QtyUsed,
                        x.QtyUnused,
                        x.DateProd,
                        x.ProcessCode,
                        x.Statusname,
                        x.InputGoodsCode,
                        x.InputGoodsSeq,
                    })
                    .Select(x => x.First())
                    .ToList();
                var sortedListByDateProd = listItems
                          .OrderBy(s => s.DateProd == null && string.IsNullOrEmpty(s.Character) ? 1 : 0)
                          .ThenBy(s => s.DateProd == null && !string.IsNullOrEmpty(s.Character) ? 1 : 0)
                          .ThenBy(s => s.DateProd)
                          .ThenBy(s => string.IsNullOrEmpty(s.Character) ? 1 : (s.Character.EndsWith("'")) ? 0 : 1)
                          .ThenBy(s => s.Character)
                          .ThenBy(s => s.WorkOrder)
                          .ToList();
                var groupListWO = sortedListByDateProd
                    .GroupBy(x => new
                    {
                        x.WorkOrder
                    })
                    .Select(x => new
                    {
                        workOrder = x.Key.WorkOrder,
                        productCode = x.First().ProductCode,
                        lotNo = x.First().LotNo,
                        qtyUsed = x.First().QtyUsed,
                        qtyUnused = x.Sum(i => i.QtyUnused),
                        listWorkOrderItems = x.Select(s => new
                        {
                            qtyUnusedForWo = s.QtyUnused,
                            qtyUserForWo = s.QtyUsed,
                            workOrderItem = s.WorkOrder,
                            inputGoodsCode = s.InputGoodsCode,
                        }).GroupBy(s => s.inputGoodsCode).ToList(),

                    });
                var response = new
                {
                    dataAllWO = groupListWO,
                    dataAllRM = returnAllRM
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// Phiếu xuất kho
        /// </summary>
        /// <returns></returns>
        [Route("nguyen-vat-lieu/chuan-bi/phieu-xuat-kho")]
        public IActionResult WriteDeliveryNote()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên vật liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Chuẩn bị", Url = Url.Action("PrepareForMaterials", "Materials"), IsActive = false },
                new() { Title = "Phiếu xuất kho", Url = Url.Action("WriteDeliveryNote", "Materials"), IsActive = true },
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RenderDeliveryItem()
        {
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }

                var getItemsO = await (from s in _context.TblDetailsPreMaterials
                                      join l in _context.TblPreImportItems on s.IdItemImport equals l.Id
                                      where l.IdUser == idUser && l.DateImport == DateTime.Now.Date
                                      && s.StatusExported == "Checked"
                                      select new
                                      {
                                          s.WorkOrder,
                                          s.DateImport,
                                          s.TimeImport,
                                          s.QtyImport,
                                          s.MaterialCode,
                                          ItemCode = _context.TblWorkOrderMes
                                          .Where(x => x.WorkOrder == s.WorkOrder)
                                          .Select(x => x.ItemCode)
                                          .FirstOrDefault() ?? "",
                                          LotNo = _context.TblWorkOrderMes
                                          .Where(x => x.WorkOrder == s.WorkOrder)
                                          .Select(x => x.LotNo)
                                          .FirstOrDefault() ?? "",
                                          ProgressOrder = _context.TblWorkOrderMes
                                          .Where(x => x.WorkOrder == s.WorkOrder)
                                          .Select(x => x.ProgressOrder)
                                          .FirstOrDefault() ?? "",
                                      }).ToListAsync();
                List<RenderItemDelivery> listItemsRender = new();
                var groupByItems = getItemsO.GroupBy(x => new
                {
                    x.DateImport,
                    x.TimeImport
                }).Select(x => new
                {
                    x.Key.DateImport,
                    x.Key.TimeImport,
                    dataItems = x.GroupBy(y => y.MaterialCode)
                        .Select(yg => new
                        {
                            InputGoodsCode = yg.Key,
                            ItemCode = String.Join(",", yg.Select(s => s.ItemCode).Distinct().ToArray()),
                            lotNo = yg.FirstOrDefault()?.LotNo,
                            dateImport = yg.FirstOrDefault()?.DateImport,
                            timeImport = yg.FirstOrDefault()?.TimeImport,
                            qtyImport = yg.FirstOrDefault()?.QtyImport,
                            qtyUnused = _context.TblWorkOrderMes
                            .Where(x => x.InputGoodsCodeMes == yg.Key)
                            .FirstOrDefault()?.QtyUnused,
                            processCode = yg.LastOrDefault()?.ProgressOrder,
                            workOrder = yg.Select(s => s.WorkOrder).Distinct().ToList(),
                            maxWorkOrder = _context.TblWorkOrderMes
                            .Where(x => x.InputGoodsCodeMes == yg.Key)
                            .AsEnumerable()
                            .Select(s => new
                            {
                                s.WorkOrder,
                                DateProd = (s.DateProd != null && s.TimeProd != null) ? s.DateProd.Value.Add(s.TimeProd.Value) : DateTime.MinValue,
                            })
                            .OrderBy(x => x.DateProd).LastOrDefault()?.WorkOrder,
                        }),
                }).ToList();
                foreach (var item in groupByItems)
                {
                    foreach (var item1 in item.dataItems)
                    {
                        var listWOImports = item1.workOrder;
                        int? inventory = await _context.TblInventoryMes
                           .Where(x => x.ItemCode == item1.InputGoodsCode)
                           .Select(x => x.Qty)
                           .FirstOrDefaultAsync();

                        // Lấy số lượng đã xuất phiếu trong ngày hôm nay
                        int? qtyWOHasExported = _context.TblExportWhs
                                    .Where(x => x.ItemCode == item1.InputGoodsCode &&
                                    x.DateImport == DateTime.Now.Date && x.Progress == "Exported")
                                    .Sum(x => x.QtyEx) ?? 0;

                        //Lấy receving pl trong ngày để tính lượng tồn còn lại
                        var getItemWithReciving = _context.TblRecevingPlmes
                            .Where(x => x.ItemCode == item1.InputGoodsCode
                            && x.ModifyUpdate != null && x.ModifyUpdate.Value.Date == DateTime.Now.Date)
                            .ToList();

                        int? qtyHasReceving = 0;
                        if (getItemWithReciving.Count > 0)
                        {
                            qtyHasReceving = getItemWithReciving.GroupBy(x => x.ItemCode)
                                 .Select(x => x.Sum(s => s.Qty))
                                 .FirstOrDefault();
                        }
                        int? qtyMM = (qtyWOHasExported - qtyHasReceving);
                        inventory += (qtyWOHasExported - qtyHasReceving - qtyMM);

                        //Lấy số lượng đã viết từ ngày hiện tại đến ngày dự định xuất NVL
                        int? qtyTotalExportedDateImport = _context.TblExportWhs
                                    .Where(x => x.ItemCode == item1.InputGoodsCode
                                    && x.DateImport.HasValue && x.TimeImport.HasValue && x.Progress == "Exported")
                                    .AsEnumerable()
                                    .Where(x => x.DateImport != null &&
                                    x.TimeImport != null &&
                                    item.DateImport != null &&
                                    item.TimeImport != null && x.DateImport >= DateTime.Now.Date &&
                                    x.DateImport.Value.Add(x.TimeImport.Value) < item.DateImport.Value.Add(item.TimeImport.Value))
                                    .Sum(x => x.QtyEx) ?? 0;
                        //Lấy receiving hiện tại
                        var getItemReciving = _cl.Receiving_Plan(item1.processCode);
                        if (getItemReciving.Rows.Count > 0)
                        {
                            foreach (DataRow row in getItemReciving.Rows)
                            {
                                if (item1.InputGoodsCode == row["CD_ITM"].ToString())
                                {
                                    inventory += int.Parse(row["NET_QTY"].ToString() ?? "0");
                                }
                            }
                        }

                        if (qtyTotalExportedDateImport > 0)
                        {
                            qtyTotalExportedDateImport -= qtyHasReceving;
                            inventory += qtyTotalExportedDateImport;
                        }

                        if (inventory < 0)
                        {
                            inventory = 0;
                        }

                        // Lấy dateprod để lấy số lượng còn lại chưa dùng nhỏ hơn dateprod
                        // Tính số lượng chưa sử dụng
                        var getDateTimeProd = _context.TblWorkOrderMes
                            .Where(x => x.WorkOrder == item1.maxWorkOrder)
                            .Select(s => new
                            {
                                DateProd = s.DateProd.HasValue && s.TimeProd.HasValue ? s.DateProd.Value.Add(s.TimeProd.Value) : (DateTime?)null,
                            })
                            .FirstOrDefault();
                        var listQtyUnused = await (from s in _context.TblWorkOrderMes
                                                   where s.InputGoodsCodeMes == item1.InputGoodsCode
                                                   && s.DateProd != null && s.TimeProd != null && s.QtyUnused > 0
                                                   && s.WorkOrder != null && !listWOImports.Contains(s.WorkOrder) &&
                                                   s.Statusname != "Creating Orders"
                                                   select new
                                                   {
                                                       s.WorkOrder,
                                                       s.ProgressOrder,
                                                       s.QtyUnused,
                                                       DateProd = s.DateProd.HasValue && s.TimeProd.HasValue ? s.DateProd.Value.Add(s.TimeProd.Value) : (DateTime?)null,
                                                   }).ToListAsync();
                        listQtyUnused = listQtyUnused
                            .Where(x => x.DateProd < getDateTimeProd?.DateProd)
                            .ToList();
                        int qtyUnsed = 0;
                        if (listQtyUnused.Count > 0)
                        {
                            foreach (var itemUnused in listQtyUnused)
                            {
                                qtyUnsed += itemUnused.QtyUnused ?? 0;
                            }

                        }

                        // Số lượng dự định xuất của các workorder dự định xuất kho
                        decimal qtyIntendedNow = 0;
                        decimal qtyImportedBefore = 0;
                        foreach (var wo in listWOImports)
                        {
                            // Lấy giá trị BOM theo mã NVL của các WO dự định nhập
                            var standardValue = _context.TblWorkOrderMes
                                .Where(x => x.WorkOrder == wo && x.InputGoodsCodeMes == item1.InputGoodsCode)
                                .Select(s => s.InputGoodsCodeSeq).FirstOrDefault() ?? 0;
                            qtyIntendedNow += (_context.TblDetailsPreMaterials
                            .Where(x => x.WorkOrder == wo && x.MaterialCode == item1.InputGoodsCode && x.StatusExported == "Checked")
                            .Sum(s => s.QtyImport != null ? (decimal)s.QtyImport : 0) * standardValue);

                            // Lấy số lượng đã viết trong ngày hôm nay
                            qtyImportedBefore += (int)(_context.TblDetailsPreMaterials
                                        .Where(x => x.WorkOrder == wo && x.MaterialCode == item1.InputGoodsCode
                                        && x.DateImport != null && x.DateImport.Value.Date == DateTime.Now.Date
                                        && x.StatusExported == "Exported")
                                        .Sum(x => x.QtyImport != null ? (decimal)x.QtyImport : 0) * standardValue);
                        }
                        int qtyTotalUsed = qtyUnsed + (int)qtyImportedBefore + (int)qtyIntendedNow;    

                        listItemsRender.Add(new RenderItemDelivery
                        {
                            WorkOrder = String.Join(',', item1.workOrder),
                            Lotno = item1.lotNo,
                            ItemCode = item1.ItemCode,
                            DateImport = item.DateImport?.ToString("dd/MM/yyyy") ?? "",
                            TimeImport = item.TimeImport?.ToString(@"hh\:mm") ?? "",
                            QtyImport = _context.TblDetailsPreMaterials
                            .Where(x => x.DateImport != null && item.DateImport != null && x.WorkOrder == item1.maxWorkOrder && x.DateImport.Value.Date == item.DateImport.Value.Date
                            && x.TimeImport == item.TimeImport && x.MaterialCode == item1.InputGoodsCode).Select(s => s.QtyImport).FirstOrDefault() ?? 0,
                            InputGoodsCode = item1.InputGoodsCode ?? "",
                            QtyUnused = qtyTotalUsed,
                            IvtQty = inventory ?? 0,
                            ProcessCode = item1.processCode ?? "",
                            TotalExported = qtyTotalExportedDateImport ?? 0
                        });
                    }
                }

                var listRG90Items = listItemsRender.Where(x => x.InputGoodsCode != null
                && x.InputGoodsCode.StartsWith("RG90")).ToList();
                var listOtherItems = listItemsRender.Where(x => !x.InputGoodsCode.StartsWith("RG90")).ToList();

                var groupItems = listOtherItems
                    .GroupBy(x => new
                    {
                        x.DateImport,
                        x.TimeImport,
                    })
                    .Select(x => new
                    {
                        dateImport = x.Key.DateImport,
                        timeImport = x.Key.TimeImport,
                        dataItems = x.GroupBy(y => y.InputGoodsCode)
                        .Select(yg => new
                        {
                            InputGoodsCode = yg.Key,
                            itemCode = yg.LastOrDefault()?.ItemCode,
                            dateImport = yg.LastOrDefault()?.DateImport,
                            timeImport = yg.LastOrDefault()?.TimeImport,
                            qtyImport = yg.LastOrDefault()?.QtyImport,
                            ivtQty = yg.LastOrDefault()?.IvtQty,
                            qtyUnused = yg.LastOrDefault()?.QtyUnused,
                            processCode = yg.LastOrDefault()?.ProcessCode,
                            workOrder = yg.LastOrDefault()?.WorkOrder,
                            inventoryReal = _context.TblInventoryMes
                           .Where(x => x.ItemCode == yg.Key)
                           .Select(x => x.Qty).FirstOrDefault() ?? 0,
                            totalExported = yg.LastOrDefault()?.TotalExported
                        }).ToList()
                    });
                var groupRG90Items = listRG90Items
                  .GroupBy(x => new
                  {
                      x.DateImport,
                      x.TimeImport,
                  })
                  .Select(x => new
                  {
                      dateImport = x.Key.DateImport,
                      timeImport = x.Key.TimeImport,
                      dataItems = x.GroupBy(y => y.InputGoodsCode)
                      .Select(yg => new
                      {
                          InputGoodsCode = yg.Key,
                          itemCode = yg.LastOrDefault()?.ItemCode,
                          dateImport = yg.LastOrDefault()?.DateImport,
                          timeImport = yg.LastOrDefault()?.TimeImport,
                          qtyImport = yg.LastOrDefault()?.QtyImport,
                          ivtQty = yg.LastOrDefault()?.IvtQty,
                          qtyUnused = yg.LastOrDefault()?.QtyUnused,
                          processCode = yg.LastOrDefault()?.ProcessCode,
                          workOrder = yg.LastOrDefault()?.WorkOrder,
                          inventoryReal = _context.TblInventoryMes
                           .Where(x => x.ItemCode == yg.Key)
                           .Select(x => x.Qty).FirstOrDefault() ?? 0,
                          totalExported = yg.LastOrDefault()?.TotalExported
                      }).ToList()
                  });
                var response = new
                {
                    deliveryItems = groupItems,
                    rg90Items = groupRG90Items,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public async Task<int> TotalQtyExported(string itemCode, string processCode)
        {
            int qty = 0;
            var dateCurrent = DateTime.Now;
            var convertDateCurrent = new DateTime(dateCurrent.Year, dateCurrent.Month, dateCurrent.Day, 0, 0, 0);
            var dateAdd = new DateTime(dateCurrent.Year, dateCurrent.Month, dateCurrent.Day, 0, 0, 0).AddDays(3);
            var loadItem = await (from s in _context.TblExportWhs
                                  where s.ItemCode == itemCode
                                  && s.Whlocation == processCode
                                  && s.Progress == "Exported"
                                  select s).ToListAsync();
            if (loadItem.Count > 0)
            {
                var groupItem = loadItem.GroupBy(s => s.ItemCode).Select(x => new
                {
                    itemCode = x.Key,
                    totalQty = x.Sum(i => i.QtyEx ?? 0)
                }).ToList();
                foreach (var item in groupItem)
                {
                    qty = item.totalQty;
                }
            }
            else
            {
                qty = 0;
            }
            return qty;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Log.Error("Error: {ID}", Activity.Current?.Id);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task SaveWOWithCharacterAsync(string location)
        {
            var currentDate = DateTime.Now;

            var characterProcessing = await _context.TblPreImportItems
                .Where(s => s.ProgressMes == "01050")
                .Select(s => new TblPreImportItem
                {
                    WorkOrder = s.WorkOrder,
                    ItemCode = s.ItemCode,
                    LotNo = s.LotNo,
                    CharacterAlp = s.CharacterAlp,
                    DateProd = s.DateProd,
                    TimeProd = s.TimeProd
                }).ToListAsync();

            var allWorkOrders = await _context.TblWorkOrderMes
            .Where(x => x.ModifyDateUpdate != null && x.ProgressOrder == location)
            .ToListAsync();

            var todayWorkOrders = allWorkOrders.Where(x => x.ModifyDateUpdate?.Date == currentDate).ToList();
            var pastWorkOrders = allWorkOrders.Where(x => x.ModifyDateUpdate?.Date < currentDate).ToList();

            foreach (var item in todayWorkOrders)
            {
                if (item.ProgressOrder != "01050")
                {
                    item.Character = GetCharacter(item.ProgressOrder ?? "", item.ItemCode ?? "", item.LotNo ?? "", characterProcessing);
                }
            }

            foreach (var item in pastWorkOrders)
            {
                item.QtyUnused = 0;
                item.Statusname = "Production end";
            }
            var existDateProd = await (from s in _context.TblPreImportItems
                                       join c in _context.TblWorkOrderMes on s.WorkOrder equals c.WorkOrder
                                       where s.ProgressMes == location && c.ModifyDateUpdate != null && c.ModifyDateUpdate.Value.Date == currentDate
                                       select new
                                       {
                                           s.WorkOrder,
                                           s.ItemCode,
                                           s.LotNo,
                                           s.CharacterAlp,
                                           s.DateProd,
                                           s.TimeProd,
                                           c.Statusname,
                                           c.ProgressOrder,
                                           c.InputGoodsCodeMes
                                       }).ToListAsync();

            foreach (var item in existDateProd)
            {
                var matched = allWorkOrders.Where(x =>
                    x.WorkOrder == item.WorkOrder &&
                    x.ProgressOrder == item.ProgressOrder &&
                    x.InputGoodsCodeMes == item.InputGoodsCodeMes).ToList();

                foreach (var wo in matched)
                {
                    wo.DateProd = item.DateProd;
                    wo.TimeProd = item.TimeProd ?? TimeSpan.Zero;
                    wo.Character = item.CharacterAlp ?? "";
                    wo.Statusname = item.Statusname;
                }
            }

            await _context.SaveChangesAsync();
        }
        private static string GetCharacter(string progressOrder, string itemCode, string lotNo, List<TblPreImportItem> characterProcessing)
        {
            foreach (var itemProcessing in characterProcessing)
            {
                if (progressOrder == "01055" && lotNo == itemProcessing.LotNo)
                {
                    string charGripItem = itemProcessing.ItemCode?.Length >= 10 ? itemProcessing.ItemCode.Substring(5, 6) : string.Empty;
                    string subPrintGripItemCode = itemProcessing.ItemCode?.Length >= 16 ? string.Concat(itemProcessing.ItemCode.AsSpan(6, 10), charGripItem) : string.Empty;
                    if (!string.IsNullOrEmpty(subPrintGripItemCode) && itemCode != null && Regex.IsMatch(itemCode, subPrintGripItemCode))
                        return itemProcessing.CharacterAlp ?? "";
                } else if (progressOrder == "01060" && lotNo == itemProcessing.LotNo)
                {
                    string subProductCode = itemProcessing.ItemCode?.Length >= 10 ? itemProcessing.ItemCode.Substring(4, 7) : string.Empty;
                    if (!string.IsNullOrEmpty(subProductCode) && itemCode != null && Regex.IsMatch(itemCode, subProductCode))
                        return itemProcessing.CharacterAlp ?? "";
                } else if (new[] { "01065", "01070", "01075", "01074", "01069" }.Contains(progressOrder))
                {
                    string subProductCode = itemProcessing.ItemCode?.Length >= 10 ? itemProcessing.ItemCode.Substring(4, 7) : string.Empty;
                    string subLotNo = itemProcessing.LotNo?.Length >= 5 ? itemProcessing.LotNo.Substring(0, 6) : string.Empty;
                    if (!string.IsNullOrEmpty(subProductCode) && !string.IsNullOrEmpty(subLotNo) &&
                        itemCode != null && lotNo != null &&
                        Regex.IsMatch(itemCode, subProductCode) && Regex.IsMatch(lotNo, subLotNo))
                        return itemProcessing.CharacterAlp ?? "";
                }
            }
            return "";
        }
        public int ReservedValue(string orderno, string itemcode)
        {
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
        public async Task<IActionResult> CreatingDeliveryExcel([FromBody] RequestData requestData)
        {
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }
                string? stringDateImport = requestData.StringDate;
                string? stringTimeImport = requestData.StringTime;
                DateTime.TryParseExact(stringDateImport, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateImport);
                TimeSpan timeFormat = TimeSpan.Parse(stringTimeImport ?? "");

                string jsonStr = @"" + requestData.DataImport + "";
                var itemDataJson = JsonConvert.DeserializeObject<JsonConvertDataEX[]>(jsonStr);

                string stringFileName = requestData.CheckItemRG90 == true
                    ? "CQ20000-5-Rev6 PHIẾU YÊU CẦU XUẤT KHO KIÊM BẢN KIỂM TRA XUẤT KHO (xuất từ sản xuất).xlsx"
                    : "CQ20000-4 Rev6 PHIẾU YÊU CẦU XUẤT KHO KIÊM BẢN KIỂM TRA XUẤT KHO(xuất từ kho).xlsx";

                var templatePath = Path.Combine(_environment.ContentRootPath, "templates", stringFileName);
                byte[] templateBytes = System.IO.File.ReadAllBytes(templatePath);

                using (MemoryStream memoryStream = new(templateBytes))
                using (ExcelPackage package = new(memoryStream))
                {
                    var newFileName = Path.GetFileNameWithoutExtension(stringFileName) + "_" + requestData.NewFileName + ".xlsx";

                    List<ItemAddForTblExport> listItemExport = new();

                    if (itemDataJson != null)
                        foreach (var item in itemDataJson)
                        {
                            var title = item.Title;
                            if (item.Value != null)
                            {
                                if (title == "Phiếu 1")
                                {
                                    AddData(package, item.Value, title, stringDateImport ?? "", stringTimeImport ?? "");
                                }
                                else
                                {
                                    CreateNewSheet(package, title ?? "");
                                    AddData(package, item.Value, title ?? "", stringDateImport ?? "", stringTimeImport ?? "");
                                }
                                foreach (var itemData in item.Value)
                                {
                                    listItemExport.Add(new ItemAddForTblExport
                                    {
                                        ItemCode = itemData.ItemCode,
                                        LotNo = itemData.LotNO,
                                        Unit = itemData.UnitCode,
                                        QtyExported = itemData.Qty,
                                        WHLocation = itemData.PositionWH,
                                        Note = itemData.Remarks
                                    });

                                    if (item.ProductCode != null)
                                    {
                                        foreach (var v in item.ProductCode)
                                        {
                                            var workOrderItem = !string.IsNullOrEmpty(v.WorkOrder) ? v.WorkOrder.Split(',') : Array.Empty<string>();
                                            if (workOrderItem.Length > 0)
                                            {
                                                foreach (var itemWorkOrder in workOrderItem)
                                                {
                                                    var strOrder = itemWorkOrder.ToString();
                                                    await (from s in _context.TblDetailsPreMaterials
                                                           where s.WorkOrder == strOrder && s.DateImport == dateImport && s.TimeImport == timeFormat
                                                           && s.MaterialCode == itemData.ItemCode
                                                           select s).ForEachAsync(x => x.StatusExported = "Exported");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    var groupItems = listItemExport
                        .GroupBy(x => x.ItemCode)
                        .Select(x => new
                        {
                            itemCodeEx = x.Key?.ToString(),
                            lotNoEx = x.First().LotNo,
                            unitCode = x.First().Unit,
                            positionWH = x.First().WHLocation,
                            qtyEx = x.Sum(i => i.QtyExported),
                            remarksEx = x.First().Note,
                        })
                        .ToList();
                    foreach (var item in groupItems)
                    {
                        var exItem = new TblExportWh()
                        {
                            ItemCode = item.itemCodeEx,
                            LotNo = item.lotNoEx,
                            Unit = item.unitCode,
                            QtyEx = item.qtyEx,
                            Whlocation = item.positionWH == "01070,01075" ? "01075" : item.positionWH,
                            Progress = "Exported",
                            IdUser = idUser,
                            Note1 = item.remarksEx,
                            DateImport = dateImport,
                            TimeImport = timeFormat
                        };
                        _context.TblExportWhs.Add(exItem);
                        await (from s in _context.TblSubMaterials
                               where s.ProductCode == item.itemCodeEx
                               select s).ForEachAsync(x => x.SafeInventory += item.qtyEx);
                    }
                    _context.SaveChanges();
                    byte[] fileBytes = package.GetAsByteArray();
                    string base64String = Convert.ToBase64String(fileBytes);
                    var response = new
                    {
                        fileDownload = base64String,
                        excelName = newFileName,
                        message = "Tạo file thành công vui lòng tải file để sử dụng"
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        protected void CreateNewSheet(ExcelPackage package, string title)
        {
            ExcelWorksheet templateSheet = package.Workbook.Worksheets[0];
            templateSheet.ConditionalFormatting.RemoveAll();
            templateSheet.View.ZoomScale = 100;
            package.Workbook.Worksheets.Add(title, templateSheet);
        }

        protected void AddData(ExcelPackage excelPackage, List<ItemValue> dataMaterialsExported, string title, string date, string time)
        {
            TimeSpan timeFormat = TimeSpan.Parse(time);
            var minutes = timeFormat.Minutes.ToString("00");
            var hours = timeFormat.Hours.ToString("00");

            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[title];
            int rows = worksheet.Dimension.Rows;
            int columns = worksheet.Dimension.Columns;
            int targetRow = -1;
            int targetStartColumn = -1;
            int targetEndColumn = -1;
            int mergeColumns = -1;
            worksheet.View.ZoomScale = 100;
            for (int row = 1; row < rows; row++)
            {
                for (int col = 1; col < columns; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value?.ToString();
                    if (cellValue != null)
                    {
                        if (cellValue.Contains("Thời gian yêu cầu giao nhận"))
                        {
                            worksheet.Cells[row, col].Value = $"Thời gian yêu cầu giao nhận/引渡希望時間: {hours}:{minutes}h, {date}";
                        }
                        if (cellValue.Contains("Ghi chép của bộ phận yêu cầu"))
                        {
                            targetStartColumn = col; // Xác định cột đầu tiên
                            var mergedRange = worksheet.MergedCells[row, col]; // Lấy thông tin vùng merge
                            var address = new ExcelAddress(mergedRange);
                            mergeColumns = address.End.Column - address.Start.Column + 1; // Tính số cột merge
                            targetEndColumn = col + mergeColumns;
                            targetRow = row;
                            break;
                        }
                    }
                }
            }
            int i = 1;
            if (targetRow != -1 && targetStartColumn != -1 && targetEndColumn != -1)
            {
                for (int col = targetStartColumn; col <= targetEndColumn; col++)
                {
                    int rowCheck = targetRow + 1;
                    var cellValue = worksheet.Cells[rowCheck, col].Value?.ToString();
                    if (cellValue != null)
                    {
                        if (cellValue.Contains("Số TT") || cellValue.Contains("Mã") || cellValue.Contains("Số lô") ||
                          cellValue.Contains("Đơn vị") || cellValue.Contains("Số lượng yêu cầu xuất") || cellValue.Contains("Vị trí kho") ||
                          cellValue.Contains("Ghi chú"))
                        {
                            int rowAddData = rowCheck + 1;
                            foreach (var item in dataMaterialsExported)
                            {
                                var cellMerge = worksheet.Cells[rowAddData - 1, col];
                                if (cellMerge.Merge)
                                {
                                    var mergedRange = worksheet.MergedCells[cellMerge.Start.Row, cellMerge.Start.Column];
                                    var mergeAddress = new ExcelAddress(mergedRange);
                                    worksheet.Cells[rowAddData, mergeAddress.Start.Column, rowAddData, mergeAddress.End.Column].Merge = true;
                                }
                                if (worksheet.Cells[rowAddData, col].Value != null)
                                {
                                    worksheet.InsertRow(rowAddData, 1);
                                    //Lấy style từ dòng bên trên khi thêm dòng mới
                                    for (int colStyle = 1; colStyle <= columns; colStyle++)
                                    {
                                        worksheet.Cells[rowAddData, colStyle].StyleID = worksheet.Cells[rowAddData - 1, colStyle].StyleID;
                                    }
                                }
                                if (cellValue.Contains("Số TT"))
                                {
                                    worksheet.Cells[rowAddData, col].Value = i.ToString();
                                    i++;
                                }
                                else if (cellValue.Contains("Mã"))
                                {
                                    worksheet.Cells[rowAddData, col].Value = item.ItemCode ?? "";
                                }
                                else if (cellValue.Contains("Số lô"))
                                {
                                    if (string.IsNullOrEmpty(item.LotNO))
                                    {
                                        worksheet.Cells[rowAddData, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.DiagonalDown = true;
                                    }
                                    worksheet.Cells[rowAddData, col].Value = item.LotNO ?? "";
                                }
                                else if (cellValue.Contains("Đơn vị"))
                                {
                                    worksheet.Cells[rowAddData, col].Value = item.UnitCode ?? "";
                                }
                                else if (cellValue.Contains("Số lượng yêu cầu xuất"))
                                {
                                    worksheet.Cells[rowAddData, col].Value = item.Qty.ToString();
                                }
                                else if (cellValue.Contains("Vị trí kho"))
                                {
                                    worksheet.Cells[rowAddData, col].Value = item.PositionWH ?? "";
                                }
                                else if (cellValue.Contains("Ghi chú"))
                                {
                                    if (string.IsNullOrEmpty(item.Remarks))
                                    {
                                        worksheet.Cells[rowAddData, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                                        worksheet.Cells[rowAddData, col].Style.Border.DiagonalDown = true;
                                    }
                                    worksheet.Cells[rowAddData, col].Value = item.Remarks ?? "";
                                }
                                rowAddData++;
                            }
                        }
                    }
                }
            }
            excelPackage.Save();
        }
        [Route("/nguyen-vat-lieu/phu")]
        public async Task<IActionResult> SubMaterials()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu Phụ", Url = Url.Action("SubMaterials", "Materials"), IsActive = true },
            };
            if (_tblSubMaterials.Count > 0)
                _tblSubMaterials.Clear();
            var getHistoryItem = await (from s in _context.TblSubMaterials
                                        select s).ToArrayAsync();
            if (getHistoryItem.Length > 0)
            {
                foreach (var subMaterial in getHistoryItem)
                {
                    var submaterialData = new TblSubMaterial
                    {
                        ProductCode = subMaterial.ProductCode,
                        QtyProdPerDay = subMaterial.QtyProdPerDay,
                        QtyPrintedPerRoll = subMaterial.QtyPrintedPerRoll,
                        QtyCanInput = subMaterial.QtyCanInput,
                        Inventory = subMaterial.Inventory,
                        InventoryPre = subMaterial.InventoryPre,
                        SafeInventory = subMaterial.SafeInventory,
                    };
                    _tblSubMaterials.Add(submaterialData);
                }
            }
            ViewData["SubMaterialsList"] = _tblSubMaterials;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubMaterials([FromBody] RequestData requestData)
        {
            if (requestData.DataSave == null)
            {
                return BadRequest("Request data is empty. Can be check again.");
            }

            try
            {
                SubMaterialData[]? subMaterialList = JsonConvert.DeserializeObject<SubMaterialData[]>(requestData.DataSave);
                if (subMaterialList != null && subMaterialList.Length > 0)
                    foreach (var subMaterial in subMaterialList)
                    {
                        var existItem = await (from s in _context.TblSubMaterials
                                               where s.ProductCode == subMaterial.ProductCode
                                               select s
                                               ).FirstOrDefaultAsync();
                        if (existItem != null)
                        {
                            existItem.Inventory = subMaterial.Inventory;
                            existItem.InventoryPre = subMaterial.InventoryPre;
                            existItem.QtyProdPerDay = subMaterial.QtyProd;
                            existItem.QtyPrintedPerRoll = subMaterial.QtyPrinted;
                            existItem.QtyCanInput = subMaterial.QtyCanInput;
                            existItem.SafeInventory = subMaterial.SafeInventory;
                        }
                        else
                        {
                            var std = new TblSubMaterial()
                            {
                                ProductCode = subMaterial.ProductCode,
                                ProductName = subMaterial.ProductName,
                                Inventory = subMaterial.Inventory,
                                SafeInventory = subMaterial.SafeInventory,
                                QtyProdPerDay = subMaterial.QtyProd,
                                QtyPrintedPerRoll = subMaterial.QtyPrinted,
                                QtyCanInput = subMaterial.QtyCanInput,
                                InventoryPre = subMaterial.InventoryPre
                            };
                            _context.TblSubMaterials.Add(std);
                        }
                    }
                _context.SaveChanges();

                return Ok(new { message = "Lưu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [Route("/nguyen-vat-lieu/phu/xuat-kho")]
        public async Task<IActionResult> WriteDeliverySubMaterials()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu Phụ", Url = Url.Action("SubMaterials", "Materials"), IsActive = false },
                new() { Title = "Xuất kho NVL Phụ", Url = Url.Action("WriteDeliverySubMaterials", "Materials"), IsActive = true },
            };
            // Lấy thông tin NVL Phụ
            var dataSubMaterial = await (from s in _context.TblSubMaterials
                                         select new
                                         {
                                             s.ProductCode,
                                             s.ProductName,
                                             s.QtyCanInput,
                                         }).ToListAsync();
            var oldMaterialExported = await (from s in _context.TblExportWhs
                                             where s.Progress != null && !s.Progress.Equals("Exported")
                                             select new
                                             {
                                                 s.ItemCode,
                                                 s.DateImport,
                                                 s.TimeImport,
                                                 s.QtyEx
                                             }).ToListAsync();
            var defaultSubMaterial = new List<TblSubMaterial>();
            var oldSubMaterialsList = new List<SubMaterialHolding>();
            if (dataSubMaterial != null)
            {
                foreach (var subMaterial in dataSubMaterial)
                {
                    var item = new TblSubMaterial
                    {
                        ProductCode = subMaterial.ProductCode,
                        ProductName = subMaterial.ProductName,
                        QtyCanInput = subMaterial.QtyCanInput,
                    };
                    defaultSubMaterial.Add(item);
                }
            }

            if (oldMaterialExported != null)
            {
                foreach (var subMaterial in oldMaterialExported)
                {
                    var item = new SubMaterialHolding
                    {
                        ItemCode = subMaterial.ItemCode,
                        DateImport = subMaterial.DateImport,
                        TimeImport = subMaterial.TimeImport,
                        QtyEx = subMaterial.QtyEx > 0 ? (int)subMaterial.QtyEx : 0,
                    };
                    oldSubMaterialsList.Add(item);
                }
            }
            ViewData["SubMaterialsList"] = defaultSubMaterial;
            ViewData["OldSubMaterialsList"] = oldSubMaterialsList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SavedSubMaterialExported([FromBody] RequestData requestData)
        {
            if (requestData.StrDataSub == null)
            {
                return BadRequest("Request data is empty. Can be check again.");
            }
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }
                List<Dictionary<string, object>>? listItem = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(requestData.StrDataSub);
                if (listItem != null)
                {
                    foreach (var item in listItem)
                    {
                        string productCode = item["productCode"].ToString() ?? "";
                        string dateImportStr = item["dateImportSub"].ToString() ?? "";
                        string timeImportStr = item["timeImportSub"].ToString() ?? "";
                        string unitCode = item["unitCode"].ToString() ?? "";
                        int qtyTicket = int.Parse(item["qtyTicket"].ToString() ?? "0");
                        string processProd = item["processProd"].ToString() ?? "";
                        DateTime.TryParseExact(dateImportStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateImport);
                        TimeSpan timeFormat = TimeSpan.Parse(timeImportStr);
                        var existItem = await (from s in _context.TblExportWhs
                                               where s.ItemCode == productCode
                                               && s.DateImport == dateImport
                                               select s).FirstOrDefaultAsync();
                        if (existItem == null)
                        {
                            var std = new TblExportWh()
                            {
                                ItemCode = productCode,
                                LotNo = "",
                                Unit = unitCode,
                                QtyEx = qtyTicket,
                                Whlocation = processProd,
                                Progress = "Holding",
                                IdUser = idUser,
                                Note1 = "",
                                DateImport = dateImport,
                                TimeImport = timeFormat
                            };
                            _context.TblExportWhs.Add(std);
                        }
                    }
                }
                _context.SaveChanges();
                return Ok(new { message = "Lưu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // View Lịch sử xuất phiếu
        [Route("/nguyen-vat-lieu/lich-su-xuat-kho")]
        public IActionResult HistoryPrinted()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Lịch sử", Url = Url.Action("HistoryPrinted", "Materials"), IsActive = true },
            };
            var getCurrentExported = _context.TblExportWhs
                .Where(x => x.DateImport != null && x.DateImport.Value.Date == DateTime.Now.Date)
                .Select(s => new RenderDataSearch
                {
                    ItemCode = s.ItemCode,
                    DateImport = s.DateImport,
                    QtyEx = s.QtyEx.ToString(),
                    WhPosition = s.Whlocation,
                    UserEx = _context.TblUsers.Where(x => x.IdUser == s.IdUser).Select(u => u.DisplayName).FirstOrDefault(),
                })
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchHistory([FromBody] DataSearch dataSearch)
        {
            if(dataSearch.DateImport == null && dataSearch.ItemCode == null && dataSearch.UserId == null && dataSearch.WhPosition == null)
            {
                return BadRequest(new { message = "Not Found Data" });
            }
            var filteredProducts = _context.TblExportWhs.AsQueryable();

            if (!string.IsNullOrEmpty(dataSearch.ItemCode))
            {
                filteredProducts = filteredProducts.Where(p => p.ItemCode == dataSearch.ItemCode);
            }
            if (!string.IsNullOrEmpty(dataSearch.DateImport))
            {
                DateTime.TryParseExact(dataSearch.DateImport, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateImport);
                filteredProducts = filteredProducts.Where(p => p.DateImport == dateImport.Date);
            }
            if (!string.IsNullOrEmpty(dataSearch.WhPosition))
            {
                filteredProducts = filteredProducts.Where(p => p.Whlocation == dataSearch.WhPosition);
            }
            if (!string.IsNullOrEmpty(dataSearch.UserId))
            {
                var getUserId = await _context.TblUsers.Where(x => x.DisplayName == dataSearch.UserId).Select(s => s.IdUser).FirstOrDefaultAsync();
                filteredProducts = filteredProducts.Where(p => p.IdUser == getUserId);
            }

            var result = await filteredProducts.Select(x => new
            {
                itemCode = x.ItemCode,
                dateImport = x.DateImport,
                timeImport = x.TimeImport,
                qtyExport = x.QtyEx,
                whPosition = x.Whlocation,
                userExport = _context.TblUsers.Where(s => s.IdUser == x.IdUser).Select(s => s.DisplayName).FirstOrDefault(),    
            }).OrderBy(x => x.dateImport).ToListAsync();

            return Ok(new { resultSearch = result });
        }
    }

    public class ListItemAdd
    {
        public string? WorkOrder { get; set; }
        public string? ItemCode { get; set; }
        public string? Lotno { get; set; }
        public string? Qty { get; set; }
        public string? Character { get; set; }
        public string? DateIntended { get; set; }
        public string? TimeIntended { get; set; }
        public string? ProcessCode { get; set; }
        public List<RowAddItem> Rows { get; set; }
    }

    public class RowAddItem
    {
        public string? WorkOrder { get; set; }
        public string? ItemCode { get; set; }
        public string? Lotno { get; set; }
        public string DateInput { get; set; }
        public string TimeInput { get; set; }
        public string QtyImport { get; set; }
    }

    public class RenderItemDelivery
    {
        public string? WorkOrder { get; set; }
        public string? ItemCode { get; set; }
        public string? Lotno { get; set; }
        public string DateImport { get; set; }
        public string TimeImport { get; set; }
        public int QtyImport { get; set; }
        public string InputGoodsCode { get; set; }
        public string ProcessCode { get; set; }
        public int IvtQty {  get; set; }
        public int QtyUnused {  get; set; }
        public string StatusExported {  get; set; }
        public int TotalExported {  get; set; }
    }
    public class ItemAddForTblExport
    {
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int QtyExported { get; set; }
        public string? WHLocation { get; set; }
        public string? Note { get; set; }
    }
    public class ListItemReceving
    {
        public string ItemMaterial { get; set; }
        public int QtyRev { get; set; }
    }
    public class SubMaterialHolding
    {
        public string? ItemCode { get; set; }
        public DateTime? DateImport { get; set; }
        public TimeSpan? TimeImport { get; set; }
        public int QtyEx { get; set; }
    }
    public class DataSearch
    {
        public string? ItemCode { get; set; }
        public string? DateImport { get; set; }
        public string? WhPosition {  get; set; }
        public string? UserId { get; set; }
    }
    public class RenderDataSearch
    {
        public string? ItemCode { get; set; }
        public DateTime? DateImport { get; set; }
        public string? WhPosition { get; set; }
        public string? QtyEx { get; set; }
        public string? UserEx { get; set; }
    }
}

