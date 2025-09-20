using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace MPLUS_GW_WebCore.Controllers.Materials
{
    public class ImportMaterialsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly ConnectMES.Classa _cl;
        public readonly IWebHostEnvironment _environment;
        private readonly ExcelData _excelData;
        public readonly static List<TblSubMaterial> _tblSubMaterials = new();
        public ImportMaterialsController(MplusGwContext context, ConnectMES.Classa classa, IWebHostEnvironment hostEnvironment, ExcelData excelData)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _excelData = excelData;
        }
        [Route("/nguyen-vat-lieu/nhap")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên vật liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Nhập", Url = Url.Action("Index", "ImportMaterials"), IsActive = true },
            };

            int idUser = 1;
            if (HttpContext.Session.GetInt32("User ID") != null)
            {
                idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
            }

            var processProd = await _context.TblLocations
                .Where(s => s.LocationCode != "01050" && s.LocationCode != "01070")
                .Select(s => new CustomRenderProcess
                {
                    IdProcess = s.IdLocation.ToString(),
                    ProcessCode = s.LocationCode,
                    ProcessName = s.LocationName,
                }).ToListAsync();

            // Lấy dữ liệu receiving plan list trên MES trong ngày
            var getDataReceivingPlanListLeadWires = _context.TblRecevingPlmes
                .Where(x => x.ModifyUpdate != null && 
                x.LocationCode == "01050" &&
                x.ModifyUpdate.Value.Date == DateTime.Now.Date)
                .Select(s => new
                {
                    Id = s.NewId,
                    MaterialCode = s.ItemCode,
                    LotMaterial = s.LotNo,
                    QtyImported = s.Qty,
                    DateCreated = s.ModifyUpdate,
                }).ToList();

            // Thêm dữ liệu dây dẫn chưa nhập về
            var listLeadWires = new List<ListItemReceivingNow>();
            foreach (var item in getDataReceivingPlanListLeadWires)
            {
                // Lấy ordershipment cuối cùng khi cùng mã cùng lô
                var getLastOrdershipment = _context.TblRecevingPlmes
                    .Where(x => x.ItemCode == item.MaterialCode && x.LotNo == item.LotMaterial && x.NewId == item.Id)
                    .Select(s => s.OrderShipment)
                    .FirstOrDefault();
                // Kiểm tra bảng nhập xem ordershipment đó đã nhập chưa
                var existItem = _context.TblImportedItems
                    .FirstOrDefault(x => x.ItemCode == item.MaterialCode &&
                    x.LotNo == item.LotMaterial &&
                    x.TimeImport != null &&
                    item.DateCreated != null &&
                    x.TimeImport.Value.Date == item.DateCreated.Value.Date &&
                    x.OrderShipment == getLastOrdershipment &&
                    x.Status == "Imported");
                if (existItem == null)
                {
                    listLeadWires.Add(new ListItemReceivingNow
                    {
                        IdItem = item.Id,
                        MaterialCode = item.MaterialCode,
                        LotMaterial = item.LotMaterial,
                        QtyReceiving = item.QtyImported,
                        LocationCode = "01050"
                    });
                }
            }

            // Update trans
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "TransQAD.xlsx");
            //var dataTable = _excelData.ReadExcel(filePath, "TransQAD");
            //if(dataTable.Rows.Count > 0)
            //{
            //    foreach (DataRow row in dataTable.Rows)
            //    {
            //        if (row != null && row["ItemCode"].ToString() != "")
            //        {
            //            string? itemCode = row["ItemCode"] != null ? row["ItemCode"].ToString() : "";
            //            string? lotNo = row["Lot"] != null ? row["Lot"].ToString() : "";
            //            string? qtyImported = row["change_qty"] != null ? row["change_qty"].ToString() : "";
            //            string? requestNo = row["RequestNo"] != null ? row["RequestNo"].ToString() : "";
            //            string? effdate = row["effdate"] != null ? row["effdate"].ToString() : "";
            //            string? locationCode = row["Location"] != null ? "0" + row["Location"].ToString() : "";

            //            requestNo = requestNo?.ToUpper();

            //            var getIdLocation = _context.TblLocations.Where(x => x.LocationCode == locationCode)
            //                .Select(s => s.IdLocation).FirstOrDefault();
            //            var existingItem = _context.TblImportedItems
            //                .FirstOrDefault(x => x.ItemCode == itemCode &&
            //                x.LotNo == lotNo && x.RequestNo == requestNo);

            //            string? typeItem = string.Empty;
            //            string? timeSterilization = string.Empty;
            //            if (locationCode == "01050")
            //            {
            //                timeSterilization = "250630";
            //                if (itemCode?.EndsWith("Y") == true)
            //                {
            //                    typeItem = "Dây dẫn TYC";
            //                }
            //                else
            //                {
            //                    typeItem = "Dây dẫn thường";
            //                }
            //            }
            //            else
            //            {
            //                typeItem = "Nguyên vật liệu khác";
            //            }

            //            // Format timespan
            //            DateTime dateImported = DateTime.ParseExact(effdate ?? "", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                      
            //            int qty;
            //            int.TryParse(qtyImported, out qty);
            //            var newItem = new TblImportedItem
            //            {
            //                ItemCode = itemCode,
            //                LotNo = lotNo,
            //                RequestNo = requestNo,
            //                Qty = qty,
            //                TimeImport = dateImported,
            //                ItemType = typeItem,
            //                IdUser = idUser,
            //                IdLocation = getIdLocation,
            //                Status = "Imported",
            //                OrderShipment = "",
            //                TimeSterilization = timeSterilization,
            //            };

            //            if (existingItem == null)
            //            {

            //                _context.TblImportedItems.Add(newItem);
            //            } else
            //            {
            //                existingItem.TimeSterilization = timeSterilization;
            //            }
            //        }
            //    }
            //    _context.SaveChanges();
            //} 

            ViewData["ListItemRecv"] = listLeadWires;
            return View(processProd);
        }

        [HttpPost]
        public async Task<IActionResult> GetMaterialOthers([FromBody] RequestData requestData)
        {
            if (requestData.ProcessCode == null)
            {
                return BadRequest(new { message = "Request empty. Please check again" });
            }

            // Thêm dữ liệu dây dẫn chưa nhập về
            var listOthersMaterial = new List<ListItemReceivingNow>();

            List<ListOldImported> listImported = new();

            List<string>? arrProcessCode = JsonConvert.DeserializeObject<List<string>>(requestData.ProcessCode);

            if (arrProcessCode != null)
            {
                foreach (var itemProcesscode in arrProcessCode)
                {
                    // Lấy dữ liệu receiving plan list trên MES trong ngày
                    var getDataReceivingPlanListLeadWires = _context.TblRecevingPlmes
                        .Where(x => x.ModifyUpdate != null &&
                        x.LocationCode == itemProcesscode &&
                        x.ModifyUpdate.Value.Date == DateTime.Now.Date)
                        .Select(s => new
                        {
                            Id = s.NewId,
                            MaterialCode = s.ItemCode,
                            LotMaterial = s.LotNo,
                            QtyImported = s.Qty,
                            DateCreated = s.ModifyUpdate,
                            orderShipment = s.OrderShipment,
                        }).ToList();
                    foreach (var item in getDataReceivingPlanListLeadWires)
                    {
                        // Kiểm tra xem đã nhập nvl với ordershipment chưa
                        var existItem = _context.TblImportedItems
                            .FirstOrDefault(x => x.ItemCode == item.MaterialCode &&
                            x.LotNo == item.LotMaterial &&
                            x.TimeImport != null &&
                            item.DateCreated != null &&
                            x.TimeImport.Value.Date == item.DateCreated.Value.Date &&
                            x.Qty == item.QtyImported &&
                            x.OrderShipment != null &&
                            x.OrderShipment == item.orderShipment &&
                            x.Status == "Imported");
                        if (existItem == null)
                        {
                            listOthersMaterial.Add(new ListItemReceivingNow
                            {
                                IdItem = item.Id,
                                MaterialCode = item.MaterialCode,
                                LotMaterial = item.LotMaterial,
                                QtyReceiving = item.QtyImported,
                                LocationCode = itemProcesscode,
                            });
                        }
                    }

                    // Lấy NVL đã nhập trong ngày
                    var getIdProcessCode = await _context.TblLocations.Where(x => x.LocationCode == itemProcesscode).FirstOrDefaultAsync();
                    int idLocation = getIdProcessCode != null ? getIdProcessCode.IdLocation : 0;
                    var oldDataImported = await _context.TblImportedItems
                        .Where(x => x.IdLocation == idLocation &&
                        x.TimeImport != null &&
                        x.TimeImport.Value.Date == DateTime.Now.Date).ToListAsync();
                    foreach (var item in oldDataImported)
                    {
                        var itemOld = new ListOldImported
                        {
                            ItemCode = item.ItemCode,
                            LotNo = item.LotNo,
                            QtyImported = item.Qty,
                            IDRecev = _context.TblRecevingPlmes
                            .Where(x => x.ItemCode == item.ItemCode && x.LotNo == item.LotNo && x.OrderShipment == item.OrderShipment)
                            .Select(s => s.NewId)
                            .FirstOrDefault()
                        };
                        listImported.Add(itemOld);
                    }
                }
            }
            var groupItem = listOthersMaterial.GroupBy(x => x.LocationCode)
                .Select(x => new
                {
                    processCode = x.Key,
                    processName = _context.TblLocations
                    .Where(s => s.LocationCode == x.Key)
                    .Select(s => s.LocationName)
                    .FirstOrDefault(),
                    listItems = x.ToList()
                }).ToList();
            return Ok(new { items = groupItem, oldItems = listImported });
        }

        [HttpPost]
        public async Task<IActionResult> CheckQtyOnMes([FromBody] RequestData requestData)
        {
            if (requestData.StrDataCheck == null)
            {
                return BadRequest(new { message = "Request empty. Please check again" });
            }
            CheckImportMaterial? importMaterials = JsonConvert.DeserializeObject<CheckImportMaterial>(requestData.StrDataCheck);

            if (importMaterials == null)
            {
                return BadRequest(new { message = "Request empty. Please check again" });
            }

            var qtyRecevingPl = await _context.TblRecevingPlmes
                .Where(x => x.ItemCode == importMaterials.ProductCode && 
                x.LotNo == importMaterials.LotNo &&
                x.Qty == importMaterials.Qty && 
                x.ModifyUpdate != null && x.ModifyUpdate.Value.Date == DateTime.Now.Date)
                .Select(s => s.Qty).FirstOrDefaultAsync() ?? 0;

            int qtyMaterialOld = await _context.TblImportedItems
                .Where(x => x.ItemCode == importMaterials.ProductCode 
                && x.LotNo == importMaterials.LotNo 
                && x.Status == "Imported")
                .SumAsync(x => x.Qty != null ? (int)x.Qty : 0);

            if(qtyRecevingPl > 0 && importMaterials.Qty <= qtyRecevingPl)
            {
                var responseTrue = new
                {
                    message = "Số lượng khớp",
                    qty = qtyRecevingPl,
                    productCode = importMaterials.ProductCode,
                };
                return Ok(responseTrue);
            }
            else
            {
                var response = new
                {
                    message = "Thông tin nguyên vật liệu không khớp. Vui lòng kiểm tra lại",
                };
                return StatusCode(500, response);
            } 
        }

        [HttpPost]
        public async Task<IActionResult> SaveMaterialImport([FromBody] RequestData requestData)
        {
            int idUser = 1;
            if (HttpContext.Session.GetInt32("User ID") != null)
            {
                idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
            }
            if (requestData.DataSave == null)
            {
                return BadRequest(new { message = "Request empty, please check again" });
            }
            CheckImportMaterial? importMaterials = JsonConvert.DeserializeObject<CheckImportMaterial>(requestData.DataSave);
            if (importMaterials != null)
            {

                var getProcessCode = await (from s in _context.TblInventoryMes
                                            where s.ItemCode == importMaterials.ProductCode
                                            select new
                                            {
                                                s.LocationCode
                                            }).FirstOrDefaultAsync();
                var arrProcessCode = getProcessCode != null && getProcessCode.LocationCode.Contains(',') ? getProcessCode.LocationCode.Split(',') : new string[] { getProcessCode != null ? getProcessCode.LocationCode.ToString() : "" };
                var getIdProcess = await (from s in _context.TblLocations
                                          where s.LocationCode == "01050"
                                          select new
                                          {
                                              idProcessCode = s.IdLocation
                                          }).FirstOrDefaultAsync();
                var getOrderShipment = _context.TblRecevingPlmes
                    .Where(x => x.ItemCode == importMaterials.ProductCode && 
                    x.LotNo == importMaterials.LotNo &&
                    x.NewId == importMaterials.IdRecev)
                    .Select(s => s.OrderShipment)
                    .FirstOrDefault();
                var existItem = await (from s in _context.TblImportedItems
                                       where s.ItemCode == importMaterials.ProductCode
                                       && s.LotNo == importMaterials.LotNo && s.RequestNo == importMaterials.RequestNo
                                       && s.OrderShipment == getOrderShipment
                                       select s).FirstOrDefaultAsync();

                if (existItem != null)
                {
                    existItem.Qty = importMaterials.Qty;
                    existItem.RequestNo = importMaterials.RequestNo;
                    existItem.Status = importMaterials.PauseStatus;
                    existItem.TimeSterilization = importMaterials.TimeLimit;
                    existItem.IdLocation = getIdProcess?.idProcessCode;
                }
                else
                {
                    var std = new TblImportedItem
                    {
                        ItemCode = importMaterials.ProductCode,
                        Qty = importMaterials.Qty,
                        LotNo = importMaterials.LotNo,
                        RequestNo = importMaterials.RequestNo,
                        TimeImport = DateTime.Now,
                        ItemType = importMaterials.TypeMaterial,
                        Status = importMaterials.PauseStatus,
                        OrderShipment = getOrderShipment,
                        TimeSterilization = importMaterials.TimeLimit,
                        IdUser = idUser,
                        IdLocation = getIdProcess?.idProcessCode,
                    };
                    _context.TblImportedItems.Add(std);
                }
            }
            _context.SaveChanges();
            if (requestData.Status == "paused")
            {
                var response = new
                {
                    message = requestData.Status,
                };
                return Ok(response);
            } else
            {
                var response = new
                {
                    message = requestData.Status,
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOtherMaterialsImport([FromBody] RequestData requestData)
        {
            int idUser = 1;
            if (HttpContext.Session.GetInt32("User ID") != null)
            {
                idUser = int.Parse(HttpContext.Session.GetInt32("User ID").ToString() ?? "1");
            }
            if (requestData.DataSave == null)
            {
                return BadRequest(new { message = "Request empty, please check again" });
            }
            CheckImportMaterial[]? importMaterials = JsonConvert.DeserializeObject<CheckImportMaterial[]>(requestData.DataSave);
            if (importMaterials != null)
            {
                foreach (var item in importMaterials)
                {
                    var getProcessCode = await (from s in _context.TblInventoryMes
                                                where s.ItemCode == item.ProductCode
                                                select new
                                                {
                                                    s.LocationCode
                                                }).FirstOrDefaultAsync();
                    var arrProcessCode = getProcessCode != null && getProcessCode.LocationCode.Contains(',') ? getProcessCode.LocationCode.Split(',') : new string[] { getProcessCode != null ? getProcessCode.LocationCode.ToString() : "" };
                    var getIdProcess = await (from s in _context.TblLocations
                                              where arrProcessCode.Contains(s.LocationCode)
                                              select new
                                              {
                                                  idProcessCode = s.IdLocation
                                              }).FirstOrDefaultAsync();
                    var getOrderShipment = _context.TblRecevingPlmes
                          .Where(x => x.ItemCode == item.ProductCode && x.LotNo == item.LotNo && x.NewId == item.IdRecev)
                          .Select(s => s.OrderShipment).FirstOrDefault();
                    var existItem = await (from s in _context.TblImportedItems
                                           where s.ItemCode == item.ProductCode
                                           && s.LotNo == item.LotNo && s.TimeImport != null &&
                                           s.TimeImport.Value.Date == DateTime.Now.Date &&
                                           s.OrderShipment == getOrderShipment
                                           select s).FirstOrDefaultAsync();
                    if (existItem != null)
                    {
                        existItem.Qty = item.Qty;
                        existItem.RequestNo = item.RequestNo;
                        existItem.Status = item.PauseStatus;
                        existItem.TimeSterilization = item.TimeLimit;
                    }
                    else
                    {
                        var std = new TblImportedItem
                        {
                            ItemCode = item.ProductCode,
                            Qty = item.Qty,
                            LotNo = item.LotNo,
                            RequestNo = item.RequestNo,
                            TimeImport = DateTime.Now,
                            ItemType = item.TypeMaterial,
                            Status = item.PauseStatus,
                            OrderShipment = getOrderShipment,
                            TimeSterilization = item.TimeLimit,
                            IdUser = idUser,
                            IdLocation = getIdProcess?.idProcessCode,
                        };
                        _context.TblImportedItems.Add(std);
                    }
                }
            }
            _context.SaveChanges();
            var response = new
            {
                message = requestData.Status,
            };
            return Ok(response);
        }

        [HttpPost]
        public IActionResult GetOldImported()
        {
            var getAllItems = _context.TblImportedItems
                .ToList();
            return Ok(new {dataOld = getAllItems });
        }
    }

    public class ListItemReceivingNow
    {
        public int? IdItem { get; set; }
        public string? MaterialCode { get; set; }
        public string? LotMaterial { get; set; }
        public int? QtyReceiving { get; set; }
        public string? LocationCode { get; set; }
    }

    public class ListOldImported
    {
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public int? QtyImported { get; set; }
        public int? IDRecev { get; set; }
    }
}
