using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using MPLUS_GW_WebCore.Services;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace MPLUS_GW_WebCore.Controllers.Processing
{
    public class PrintLabelsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly ConnectMES.Classa _cl;
        public readonly IWebHostEnvironment _environment;

        public PrintLabelsController(MplusGwContext context, ConnectMES.Classa classa, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("/in-nhan")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "In Nhãn", Url = Url.Action("Index", "PrintLabels"), IsActive = true },
            };

            var currentDate = DateTime.Now;
            var strLast = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            var strNext = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(2).AddDays(-1).ToString("yyyy-MM-dd");
            List<ListWoProduction> listWoProductions = new();

            var locationCode = await (from l in _context.TblLocations
                                     where l.LocationName == "In Nhãn"
                                     select new
                                     {
                                         l.LocationCode,
                                         l.IdLocation
                                     }).FirstOrDefaultAsync();
            if (locationCode != null)
            {
                List<Dictionary<string, string>> workOrdersMes = new();
                var loadData = _cl.WO_status(locationCode.LocationCode, strLast, strNext);
                foreach (DataRow row in loadData.Rows)
                {
                    string workOrder = row["orderno"].ToString() ?? "";
                    string statusName = row["statusname"].ToString() ?? "";
                    if(statusName != "Creating Orders")
                    {
                        workOrdersMes.Add(new Dictionary<string, string> 
                        {
                            { "workOrder", workOrder ?? "" },
                            { "statusname", statusName ?? "" },
                        });
                    }
                }
                var groupWorkOrders = workOrdersMes.DistinctBy(x => x["workOrder"]).ToList();
                foreach (var item in groupWorkOrders)
                {
                    string workOrder = item["workOrder"].ToString() ?? string.Empty;
                    string statusname = item["statusname"].ToString() ?? string.Empty;
                    var getInfoWorkOrder = _context.TblWorkOrderMes
                        .Where(x => x.WorkOrder == workOrder)
                        .FirstOrDefault();
                    if (getInfoWorkOrder != null)
                    {
                        var stdItem = new ListWoProduction
                        {
                            WorkOrderNo = workOrder,
                            ProductCode = getInfoWorkOrder.ItemCode,
                            LotNo = getInfoWorkOrder.LotNo,
                            QtyProd = getInfoWorkOrder.QtyWo.ToString(),
                            Character = getInfoWorkOrder.Character,
                            DateProd = getInfoWorkOrder.DateProd,
                            TimeProd = getInfoWorkOrder.TimeProd?.ToString(@"hh\:mm"),
                            ProcessCode = locationCode.LocationCode,
                            StatusName = statusname,
                        };
                        listWoProductions.Add(stdItem);
                    }
                    await _context.TblWorkOrderMes
                        .Where(x => x.WorkOrder == workOrder)
                        .ForEachAsync(x =>
                        {
                            x.Statusname = statusname;
                            x.ModifyDateUpdate = currentDate.Date;
                        });
                }
                await _context.SaveChangesAsync();
            }
            ViewData["ListWOPrintLabels"] = listWoProductions;
            return View();
        }

        [Route("/in-nhan/chia-nvl-may")]
        public async Task<IActionResult> FlowDivLinePrintLabel()
        {
            string today = DateTime.Now.ToString("dd/MM/yyyy");
            //Lấy toàn bộ workorder đã chia cho máy
            var mcprodData = await _context.TblDivMcprods.ToDictionaryAsync(w => w.WorkOrder ?? "");

            //Lấy chi tiết workorder được thực trong ngày hôm nay
            var rawData = await _context.TblDivLineMcdetails
                .Where(x => x.DateProd == today)
                .ToListAsync();
            //Nhóm dữ liệu theo ngày
            var listItems = rawData.GroupBy(x => x.DateProd)
                .Select(s => new DivMCView
                {
                    DateProd = s.Key,
                    Shifts = s.GroupBy(x => x.ShiftLabel)
                    .Select(shiftGroup => new ShiftRender
                    {
                        ShiftLabel = shiftGroup.Key?.ToLower(),
                        InfoShifts = shiftGroup.GroupBy(x => x.WorkOrder)
                        .Select(x => new InfoShifts
                        {
                            WorkOrder = x.Key,
                            ProductCode = mcprodData.TryGetValue(x.Key ?? "", out var mcprod) ? mcprod.ProductCode : "",
                            LotNo = mcprodData.TryGetValue(x.Key ?? "", out mcprod) ? mcprod.LotNo : "",
                            QtyOrder = mcprodData.TryGetValue(x.Key ?? "", out mcprod) ? mcprod.QtyOrder : 0,
                            StatusWorkOrder = _context.TblWorkOrderMes.Where(wm => wm.WorkOrder == x.Key).Select(st => st.Statusname).FirstOrDefault(),
                            Character = _context.TblWorkOrderMes.Where(wm => wm.WorkOrder == x.Key).Select(st => st.Character).FirstOrDefault(),
                            MachineRows = _context.TblDivLineMcdetails
                            .Where(m => m.WorkOrder == x.Key &&
                            m.ShiftLabel == shiftGroup.Key &&
                            m.DateProd == today)
                            .Select(m => new MachineRows
                            {
                                MachineShift = m.MachineShift,
                                QtyDiv = m.QtyDiv,
                                QtyHasProcessed = 0
                            }).ToList(),
                            TypeLabel = x.FirstOrDefault()?.TypeLabel,
                        }).OrderBy(x => x.WorkOrder).ToList()
                    }).OrderBy(x => x.ShiftLabel).ToList()
                }).ToList();
            ViewData["ListDivMC"] = listItems;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RenderContent()
        {
            try
            {
                var locationCode = await (from l in _context.TblLocations
                                          where l.LocationName == "In Nhãn"
                                          select new
                                          {
                                              l.LocationCode,
                                              l.IdLocation
                                          }).FirstOrDefaultAsync();
                if (locationCode != null)
                {
                    var oldResult = await (from s in _context.TblDivMcprods
                                           join l in _context.TblLocations on s.IdLocation equals l.IdLocation
                                           join w in _context.TblWorkOrderMes on s.WorkOrder equals w.WorkOrder
                                           where s.IdLocation == locationCode.IdLocation && s.WorkOrder == w.WorkOrder
                                           && w.Statusname != "Production end"
                                           select new
                                           {
                                               s.WorkOrder,
                                               s.ProductCode,
                                               s.LotNo,
                                               s.Character,
                                               rows = _context.TblDivLineMcdetails
                                               .Where(x => x.ProdMcid == s.ProdMcid)
                                               .GroupBy(g => new { 
                                                   g.TypeLabel,
                                               })
                                               .Select(x => new
                                               {
                                                   typeLabel = x.Key.TypeLabel,
                                                   dataItems = x.ToList()
                                                   .GroupBy(d => d.DateProd)
                                                   .Select(f => new
                                                   {
                                                       dateProd = f.Key,
                                                       machines = f.Select(m => new
                                                       {
                                                           m.ShiftLabel,
                                                           m.MachineShift,
                                                           m.QtyDiv,
                                                           m.Remarks
                                                       }).ToList()
                                                   })
                                               }).ToList(),
                                           }).ToListAsync();
                    oldResult = oldResult.DistinctBy(x => x.WorkOrder).ToList();
                    var response = new
                    {
                        oldData = oldResult
                    };
                    return Ok(response);
                } else
                {
                    return StatusCode(500, new { message = "Có lỗi xảy ra" });
                }
            }
            catch (Exception ex) { 
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]        
        public async Task<IActionResult> SaveData([FromBody] RequestData requestData)
        {
            if (requestData.StrDataPrintLabels == null)
            {
                return BadRequest("JSON request empty. Please check again");
            }
            try
            {
                int idUser = 1;
                if (HttpContext.Session.GetInt32("User ID") != null)
                {
                    idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
                }
                var idLocation = await (from l in _context.TblLocations
                                  where l.LocationCode == requestData.ProcessCode
                                  select l.IdLocation).SingleOrDefaultAsync();

                List<DataLinePrint>? arrJsonData = JsonConvert.DeserializeObject<List<DataLinePrint>>(requestData.StrDataPrintLabels);
                List<WorkOrderDivDetail>? workOrderDivDetails = new();
                if (arrJsonData != null)
                    foreach (var item in arrJsonData)
                    {
                        try
                        {
                            var existingItem = await (from s in _context.TblDivMcprods
                                                where s.WorkOrder == item.WorkOrder
                                                select s).FirstOrDefaultAsync();    
                            int prodMCId;
                            if (existingItem != null)
                            {
                                // Update existing item
                                existingItem.ProductCode = item.ProductCode;
                                existingItem.LotNo = item.LotNo;
                                prodMCId = existingItem.ProdMcid;
                                if (item.Rows != null)
                                {
                                    foreach (var row in item.Rows)
                                    {
                                        if (row.Shifts != null)
                                        {
                                            foreach (var shift in row.Shifts)
                                            {
                                                if (shift.Machines != null)
                                                {
                                                    foreach (var machine in shift.Machines)
                                                    {
                                                        if (!string.IsNullOrEmpty(machine.Value))
                                                        {
                                                            int.TryParse(machine.Value, out int qtyDiv);
                                                            var existDetails = _context.TblDivLineMcdetails
                                                              .Where(x => x.ProdMcid == prodMCId && x.WorkOrder == item.WorkOrder && x.DateProd == row.DateProd && 
                                                              x.TypeLabel == row.TypeLabel).ToList();
                                                            if (existDetails != null)
                                                            {
                                                                foreach (var detail in existDetails)
                                                                {
                                                                    detail.ShiftLabel = shift.Shift;
                                                                    detail.MachineShift = machine.Machine.ToString();
                                                                    detail.QtyDiv = qtyDiv;
                                                                    detail.DateProd = row.DateProd ?? "".ToString();
                                                                    detail.Remarks = row.Note;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var detail = new TblDivLineMcdetail
                                                                {
                                                                    TypeLabel = row.TypeLabel,
                                                                    DateProd = row.DateProd ?? "".ToString(),
                                                                    WorkOrder = item.WorkOrder,
                                                                    ProdMcid = prodMCId,
                                                                    ShiftLabel = shift.Shift,
                                                                    MachineShift = machine.Machine.ToString(),
                                                                    QtyDiv = qtyDiv,
                                                                    Remarks = row.Note
                                                                };
                                                                _context.TblDivLineMcdetails.Add(detail);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Create new item
                                var std = new TblDivMcprod()
                                {
                                    WorkOrder = item.WorkOrder ?? "".ToString(),
                                    ProductCode = item.ProductCode,
                                    LotNo = item.LotNo, 
                                    QtyOrder = item.QtyUsed,
                                    Character = item.CharacterWo,
                                    IdUser = idUser,
                                    IdLocation = idLocation,
                                };
                                _context.TblDivMcprods.Add(std);
                                await _context.SaveChangesAsync();
                                prodMCId = std.ProdMcid;
                                if (item.Rows != null)
                                {
                                    foreach (var row in item.Rows)
                                    {
                                        if (row.Shifts != null)
                                        {
                                            foreach (var shift in row.Shifts)
                                            {
                                                if (shift.Machines != null)
                                                {
                                                    foreach (var machine in shift.Machines)
                                                    {
                                                        if (!string.IsNullOrEmpty(machine.Value))
                                                        {
                                                            int.TryParse(machine.Value, out int qtyDiv);

                                                            var detail = new TblDivLineMcdetail
                                                            {
                                                                TypeLabel = row.TypeLabel,
                                                                DateProd = row.DateProd ?? "".ToString(),
                                                                WorkOrder = item.WorkOrder,
                                                                ProdMcid = prodMCId,
                                                                ShiftLabel = shift.Shift,
                                                                MachineShift = machine.Machine.ToString(),
                                                                QtyDiv = qtyDiv,
                                                                Remarks = row.Note
                                                            };
                                                            _context.TblDivLineMcdetails.Add(detail);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        var updateDateProd = _context.TblWorkOrderMes
                                            .Where(x => x.WorkOrder == item.WorkOrder && x.DateProd == null && x.TimeProd == null)
                                            .ToList();
                                        DateTime.TryParseExact(row.DateProd, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateProdDiv);
                                        if (updateDateProd != null)
                                        {
                                            foreach (var update in updateDateProd)
                                            {
                                                update.DateProd = dateProdDiv;
                                                update.TimeProd = TimeSpan.Zero;
                                            }
                                        }
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, new { message = ex.Message });
                        }
                    }
                return Ok(new { message = "Lưu thành công" });
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
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
                var getHasDivByWorkOrder = _context.TblDivLineMcdetails.Where(x => x.WorkOrder == workOrder)
                    .Select(s => new
                    {
                        s.WorkOrder,
                        s.ShiftLabel,
                        s.MachineShift,
                        s.QtyDiv,
                        s.TypeLabel,
                    }).ToList();
                var getHasDivMaterial = _context.TblDivMaterialPrintLabels.Where(x => x.WorkOrder == workOrder)
                    .Select(s => new
                    {
                        s.WorkOrder,
                        s.ShiftLabel,
                        s.MachineShift,
                        s.QtyDiv,
                        s.LotMaterial,
                        s.MaterialCode
                    }).ToList();
                return Ok(new { dataLot = renderItemDivLine, oldDataDivByWorkOrder = getHasDivByWorkOrder, oldDivMaterials = getHasDivMaterial });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult SaveDivMaterial([FromBody] RequestData requestData)
        {
            if(requestData.JsonStr == null)
            {
                return BadRequest(new { message = "Not Found data request" });
            }
            try
            {
                List<InfoMaterialHasDiv>? infoMaterialHasDivs = JsonConvert.DeserializeObject<List<InfoMaterialHasDiv>>(requestData.JsonStr);
                if (infoMaterialHasDivs != null)
                {
                    int totalQtyDivMaterial = 0;
                    foreach (var item in infoMaterialHasDivs)
                    {
                        totalQtyDivMaterial += int.Parse(item.QtyDiv);
                        var existItem = _context.TblDivMaterialPrintLabels
                            .Where(x => x.WorkOrder == item.Workorder && x.MaterialCode == item.MaterialCode && x.LotMaterial == item.LotMaterial)
                            .FirstOrDefault();
                        if (existItem != null)
                        {
                            existItem.QtyDiv = int.Parse(item.QtyDiv);
                            existItem.MachineShift = item.MachineShift;
                            existItem.ShiftLabel = item.ShiftLabel;
                        }
                        else
                        {
                            var getProdMcId = _context.TblDivMcprods
                                .Where(x => x.WorkOrder == item.Workorder)
                                .Select(s => s.ProdMcid)
                                .FirstOrDefault();
                            var std = new TblDivMaterialPrintLabel
                            {
                                ProdMcid = getProdMcId,
                                WorkOrder = item.Workorder,
                                MachineShift = item.MachineShift,
                                ShiftLabel = item.ShiftLabel,
                                MaterialCode = item.MaterialCode,
                                LotMaterial = item.LotMaterial,
                                QtyDiv = int.Parse(item.QtyDiv),
                            };

                            _context.TblDivMaterialPrintLabels.Add(std);
                        }
                        var getQtyDivByWorkOrder = _context.TblDivLineMcdetails
                            .Where(x => x.WorkOrder == item.Workorder && x.ShiftLabel == item.ShiftLabel && x.MachineShift == item.MachineShift)
                            .Select(s => s.QtyDiv)
                            .FirstOrDefault();
                        if (getQtyDivByWorkOrder < totalQtyDivMaterial)
                        {
                            return BadRequest(new { message = "Số lượng chia line không khớp với số lượng đã chia " });
                        }
                    }
                    _context.SaveChanges();

                }
                return Ok(new { message = "Lưu thành công" });
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            } 
          
           
        }

    }
    public class Machines
    {
        public int Machine { get; set; }
        public string? Value { get; set; }
    }

    public class DataLinePrint
    {
        public string? WorkOrder { get; set; }
        public string? ProductCode { get; set; }
        public string? LotNo { get; set; }
        public int QtyUsed { get; set; }
        public string? CharacterWo { get; set; }
        public List<Row>? Rows { get; set; }
    }

    public class Row
    {
        public string? DateProd { get; set; }
        public string? TimeProd { get; set; }
        public List<ShiftS>? Shifts { get; set; }
        public string? Note { get; set; }
        public string? TypeLabel { get; set; }
    }

    public class ShiftS
    {
        public string? Shift { get; set; }
        public List<Machines>? Machines { get; set; }
    }

    public class WorkOrderDivDetail
    {
        public string ShiftName { get; set; }
        public string MachineName { get; set; }
        public int QtyDiv { get; set; }
        public string DateProd { get; set; }
        public string Typelabel { get; set; }
    }

    public class DivMCView
    {
        public string? DateProd { get; set; }
        public List<ShiftRender>? Shifts { get; set; }
    }

    public class ShiftRender
    {
        public string? ShiftLabel {  get; set; }
        public List<InfoShifts> InfoShifts { get; set; }
    }
    public class InfoShifts
    {
        public string? WorkOrder { get; set; }
        public string? ProductCode { get; set; }
        public string? LotNo { get; set; }
        public int? QtyOrder { get; set; }
        public string? StatusWorkOrder { get; set; }
        public string? Character {  get; set; }
        public List<MachineRows> MachineRows { get; set; }
        public string? TypeLabel { get; set; }
    }

    public class MachineRows
    {
        public string? MachineShift { get; set; }
        public int? QtyDiv { get; set; }
        public int? QtyHasProcessed { get; set; }
    }


    public class InfoMaterialHasDiv
    {
        public string Workorder { get; set; }
        public string MaterialCode { get; set; }
        public string LotMaterial { get; set; }
        public string ShiftLabel { get; set; }
        public string MachineShift { get; set; }
        public string QtyDiv { get; set; }
    }
}
