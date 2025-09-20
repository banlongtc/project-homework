using ConnectMES;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPLUS_GW_WebCore.Controllers.Materials;
using MPLUS_GW_WebCore.Controllers.Processing;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;

namespace MPLUS_GW_WebCore.Services
{
    public class ScheduledTaskService
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        private readonly IHubContext<MaterialTaskHub> _hubContext;
        private readonly EslsystemContext _dbEink;
        private readonly MaterialsController _materialsController;
        private string result;
        private readonly HttpClient httpClient;
        private readonly string apiKey = "CCAE6917-323B-4B5F-A62F-56910FA3F8CF";
        private readonly string einkUrl = "https://10.239.4.40/api/esl";

        public ScheduledTaskService(MplusGwContext context, IHubContext<MaterialTaskHub> hubContext, Classa cl, EslsystemContext dbEink, MaterialsController materialsController)
        {
            _context = context;
            _hubContext = hubContext;
            _cl = cl;
            _dbEink = dbEink;
            _materialsController = materialsController;
        }

        /// <summary>
        /// Updated số lượng các nguyên liệu phụ không có trong MES
        /// </summary>
        /// <returns></returns>
        public async Task UpdateListSubmaterials()
        {
            Log.Information("Update list submaterial at: {Timer}", DateTime.Now);
            var getHistoryItem = _context.TblSubMaterials.Select(x => new
            {
                x.ProductCode,
                x.QtyProdPerDay,
                x.QtyPrintedPerRoll,
                x.InventoryPre,
                x.QtyCanInput,
                x.SafeInventory,
                x.Inventory
            }).ToList();
            if (getHistoryItem != null)
            {
                foreach (var item in getHistoryItem)
                {
                    decimal qtyRollUsed = 0;
                    if (item.ProductCode == "YYMF11354V")
                    {
                        qtyRollUsed = Math.Ceiling((item.QtyProdPerDay > 0 ? (decimal)item.QtyProdPerDay : 0) / (item.QtyPrintedPerRoll > 0 ? (decimal)item.QtyPrintedPerRoll : 0));
                    }
                    if (item.ProductCode == "YY00XB110A30")
                    {
                        qtyRollUsed = Math.Ceiling((item.QtyProdPerDay > 0 ? ((decimal)item.QtyProdPerDay / 5) : 0) / (item.QtyPrintedPerRoll > 0 ? (decimal)item.QtyPrintedPerRoll : 0));
                    }
                    if (item.ProductCode == "YY00XB110A110")
                    {
                        qtyRollUsed = Math.Ceiling((item.QtyProdPerDay > 0 ? ((decimal)item.QtyProdPerDay / 25) : 0) / (item.QtyPrintedPerRoll > 0 ? (decimal)item.QtyPrintedPerRoll : 0));
                    }
                    if (item.ProductCode == "YY00XB110A70")
                    {
                        decimal qtyRollUsed1 = Math.Ceiling((item.QtyProdPerDay > 0 ? ((decimal)item.QtyProdPerDay) : 0) / (item.QtyPrintedPerRoll > 0 ? (decimal)item.QtyPrintedPerRoll : 0));
                        decimal qtyRollUsed2 = Math.Ceiling((item.QtyProdPerDay > 0 ? ((decimal)item.QtyProdPerDay) / 5 : 0) / (item.QtyPrintedPerRoll > 0 ? (decimal)item.QtyPrintedPerRoll : 0));
                        qtyRollUsed = qtyRollUsed1 + qtyRollUsed2;
                    }
                    int newInventoryPre = int.Parse(item.Inventory.ToString() ?? "0");
                    int newInventoryAfter = newInventoryPre - (int)qtyRollUsed;
                    if (newInventoryAfter < 0)
                    {
                        newInventoryAfter = 0;
                    }
                    int qtyCanInput = int.Parse(item.SafeInventory.ToString() ?? "0") - newInventoryAfter;
                    if (qtyCanInput < 0)
                    {
                        qtyCanInput = 0;
                    }
                    var itemUpdate = _context.TblSubMaterials.FirstOrDefault(x => x.ProductCode == item.ProductCode);
                    if (itemUpdate != null)
                    {
                        itemUpdate.InventoryPre = newInventoryPre;
                        itemUpdate.Inventory = newInventoryAfter > 0 ? newInventoryAfter : 0;
                        itemUpdate.QtyCanInput = qtyCanInput;
                    }
                    _context.SaveChanges();
                }
               

                var getContentAfter = await _context.TblSubMaterials.Select(x => new
                {
                    x.ProductCode,
                    x.QtyProdPerDay,
                    x.QtyPrintedPerRoll,
                    x.InventoryPre,
                    x.QtyCanInput,
                    x.SafeInventory,
                    x.Inventory
                }).ToListAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveSubMaterials", JsonConvert.SerializeObject(getContentAfter));
            }
        }

        public async Task UpdateItemInRecevingPLMES()
        {
            var getAllLocations = await _context.TblLocations
                .Select(x => new
                {
                    x.LocationCode
                }).ToListAsync();
            DateTime dateTimeNow = DateTime.Now;
            if (getAllLocations != null)
            {
                foreach (var item in getAllLocations)
                {
                    var getAllItems = _cl.Receiving_Plan(item.LocationCode);
                    if (getAllItems.Rows.Count > 0)
                    {
                        foreach (DataRow row in getAllItems.Rows)
                        {
                            string itemMaterialCode = row["cd_itm"].ToString() ?? "";
                            string lotNo = row["no_lot"].ToString() ?? "";
                            int qtyNet = int.Parse(row["net_qty"].ToString() ?? "0");
                            string locationCode = row["cd_prc"].ToString() ?? "";
                            string locationName = row["PLACEDETAILNAME"].ToString() ?? "";
                            string orderShipment = row["NO_OUTREG_ORDER"].ToString() ?? "";
                            var existItem = _context.TblRecevingPlmes.Where(x => x.ItemCode == itemMaterialCode &&
                            x.LotNo == lotNo && x.OrderShipment == orderShipment).FirstOrDefault();
                            if(existItem != null)
                            {
                                existItem.Qty = qtyNet;
                            } else
                            {
                                var std = new TblRecevingPlme
                                {
                                    ItemCode = itemMaterialCode,
                                    LotNo = lotNo,
                                    Qty = qtyNet,
                                    LocationCode = locationCode,
                                    LocationName = locationName,
                                    ModifyUpdate = dateTimeNow,
                                    OrderShipment = orderShipment,
                                };
                                _context.TblRecevingPlmes.Add(std);
                            }
                            await _context.SaveChangesAsync();
                        }
                    } else
                    {
                        var latestRecord = _context.TblRecevingPlmes.OrderByDescending(x => x.ModifyUpdate).FirstOrDefault();
                        if (latestRecord == null || (latestRecord.ModifyUpdate != null && latestRecord.ModifyUpdate.Value.Date < DateTime.Now.Date))
                        {
                            // Xóa dữ liệu trước ngày hiện tại
                            _context.TblRecevingPlmes.RemoveRange(_context.TblRecevingPlmes);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        public async Task StockMes()
        {
            // Khởi tạo dữ liệu tất cả công đoạn
            if (string.IsNullOrEmpty(result))
            {
                var loadcd = await _context.TblLocations.Select(s => s.LocationCode).ToListAsync();
                result = string.Join(", ", loadcd);
            }

            // Lấy data trên MES
            DataTable loadstock = _cl.QL_Stock("", "", result);
            // Chuyển dữ liệu lấy trên MES thành dạng list
            var stockItems = loadstock.AsEnumerable().Select(row => new
            {
                Item = row.Field<string>("ARTICLECODE") ?? "",
                Name = row.Field<string>("ARTICLESHORTNAME") ?? "",
                Lot = row.Field<string>("LOTNO") ?? "",
                CD = row.Field<string>("PLACEDETAILCODE") ?? "",
                CDName = row.Field<string>("PLACEDETAILNAME") ?? "",
                TotalQty = Convert.ToInt32(row.Field<decimal>("TOTALQTY")),
                Reserved = Convert.ToInt32(row.Field<decimal>("RESERVED")),
                ActualQty = Convert.ToInt32(row.Field<decimal>("ACTUALQTY"))
            }).ToList();

            // Lấy dữ liệu đã imports trước đó trên M+
            var itemLots = stockItems.Select(si => new {si.Item, si.Lot}).Distinct().ToList();
            var importedItemsDict = await _context.TblImportedItems
                .Where(s => 
                itemLots.Select(il => il.Item).Contains(s.ItemCode) && 
                itemLots.Select(il => il.Lot).Contains(s.LotNo) && 
                s.TimeSterilization != "")
                .GroupBy(s => new { s.ItemCode, s.LotNo })
                .ToDictionaryAsync(
                    g => (g.Key.ItemCode, g.Key.LotNo),
                    g => g.Select(x => x.TimeSterilization).First() // or some aggregation
                );
            var allProductEinks = await _dbEink.TblProducts.ToListAsync();

            var productsToAdd = new List<TblProduct>();

            foreach (var stockItem in stockItems)
            {
                var exsitProduct = allProductEinks.FirstOrDefault(s => s.ItemCode == stockItem.Item &&
                                                                     s.LotNo == stockItem.Lot &&
                                                                     s.MaCd == stockItem.CD && 
                                                                     s.HeThong == "M+ GW" && 
                                                                     s.MoTa == "Eink");

                string loadhsd = string.Empty;
                if (importedItemsDict.TryGetValue((stockItem.Item, stockItem.Lot), out var hsdValue))
                {
                    loadhsd = hsdValue ?? "";
                }
                Guid productId = Guid.NewGuid();
                if (exsitProduct != null)
                {
                    // Cập nhật sản phẩm hiện có
                    exsitProduct.Qty = stockItem.TotalQty;
                    exsitProduct.QtyPlan = stockItem.Reserved;
                    exsitProduct.QtyOk = stockItem.ActualQty;
                    ApplyHsdData(exsitProduct, loadhsd);
                }
                else
                {
                    // Thêm sản phẩm mới
                    var newProduct = new TblProduct
                    {
                        Iditem = productId,
                        ItemCode = stockItem.Item,
                        LotNo = stockItem.Lot,
                        MaCd = stockItem.CD,
                        TenSp = stockItem.Name,
                        Qty = stockItem.TotalQty,
                        QtyPlan = stockItem.Reserved,
                        QtyOk = stockItem.ActualQty,
                        TenCd = stockItem.CDName,
                        HeThong = "M+ GW",
                        MoTa = "Eink"
                    };
                    ApplyHsdData(newProduct, loadhsd);
                    productsToAdd.Add(newProduct);
                }
            }

            if (productsToAdd.Any())
            {
                _dbEink.TblProducts.AddRange(productsToAdd);
            }
            await _dbEink.SaveChangesAsync();

            // Xóa các tồn kho = 0 của các thẻ E-ink
            var totalQtyProducts = await _dbEink.TblProducts.Where(s => s.HeThong == "M+ GW" && s.MoTa == "Eink" && (s.Qty == 0 || s.QtyOk == 0)).ToListAsync();

            var productsToDelete = new List<TblProduct>();

            foreach (var it in totalQtyProducts)
            {
                // Sử dụng LINQ to Objects trên các stockItems đã được tải trước
                bool existsInStock = stockItems.Any(si => si.Item == it.ItemCode &&
                                                           si.Lot == it.LotNo &&
                                                           si.CD == it.MaCd);

                if (!existsInStock)
                {
                    // Logic xóa
                    var xoalink = await _dbEink.Links.Where(s => s.Id == it.Iditem.ToString()).ToListAsync();
                    if (xoalink.Any())
                    {
                        foreach (var item in xoalink)
                        {
                            string endpoint = $"{einkUrl}/{item.Mac}/unlink/";
                            await PostLinkESL(endpoint, httpClient);
                        }
                        
                    }
                    productsToDelete.Add(it);
                }
            }

            if (productsToDelete.Any())
            {
                _dbEink.TblProducts.RemoveRange(productsToDelete);
            }
            await _dbEink.SaveChangesAsync();
        }

        private async Task PostLinkESL(string endpoint, HttpClient _client)
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

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Log.Fatal($"Error: {response.StatusCode} - {content}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Fatal($"HTTP error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                Log.Fatal("Request timed out.");
            }
        }

        /// <summary>
        /// Áp dụng dữ liệu hạn sử dụng (HSD) vào một đối tượng TblProduct.
        /// </summary>
        /// <param name="product">Đối tượng TblProduct cần cập nhật.</param>
        /// <param name="loadhsd">Chuỗi HSD theo định dạng "yyMMdd".</param>
        private static void ApplyHsdData(TblProduct product, string loadhsd)
        {
            if (!string.IsNullOrEmpty(loadhsd))
            {
                if (DateTime.TryParseExact(loadhsd, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedHansd))
                {
                    TimeSpan diff = parsedHansd - DateTime.Now;
                    int days = (int)Math.Floor(diff.TotalDays); // Sử dụng Math.Floor để lấy số ngày nguyên còn lại

                    product.HanSuDung = parsedHansd.ToString("dd/MM/yyyy");
                    product.RDatetime1 = parsedHansd;
                    product.RInt2 = days;
                }
                else
                {
                    // Ghi log hoặc xử lý lỗi phân tích cú pháp nếu cần
                    product.HanSuDung = null;
                    product.RDatetime1 = null;
                    product.RInt2 = null;
                }
            }
            else
            {
                product.HanSuDung = null;
                product.RDatetime1 = null;
                product.RInt2 = null;
            }
        }

        public async Task GetWorkOrdersMES()
        {
            var loadcd = _context.TblLocations.Select(s => s.LocationCode).ToList();
            //Update workorder trên MES và tồn kho theo workorder
            List<ListStockMaterials> listStockMaterials = new();
            List<ListGetWorkOrderMES> listGetWorkOrderMES = new();
            var currentDate = DateTime.Now;
            var currentDay = currentDate.Date;
            var strLast = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            var strNext = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(2).AddDays(-1).ToString("yyyy-MM-dd");
            foreach (var itemLocation in loadcd)
            {
                // Lấy các workorder theo loaction trong vòng 3 tháng
                var loadData = _cl.WO_status(itemLocation, strLast, strNext);
                foreach (DataRow row in loadData.Rows)
                {
                    string workOrder = row["orderno"].ToString() ?? "";
                    string productCode = row["productcode"].ToString() ?? "";
                    string productName = row["name"].ToString() ?? "";
                    string lotNo = row["lotno"].ToString() ?? "";
                    string startDate = row["startdate"].ToString() ?? "";
                    string endDate = row["enddate"].ToString() ?? "";
                    string createdDate = row["createddate"].ToString() ?? "";
                    string statusName = row["statusname"].ToString() ?? "";
                    string inputGoodsCodeForWO = row["inputgoodscode"].ToString() ?? "";
                    string nameMaterial = row["inputgoodsshortname"].ToString() ?? "";
                    decimal orderQty = 0;
                    decimal standardValue = 0;

                    if (row["OrderQty"] != DBNull.Value)
                    {
                        _ = decimal.TryParse(row["OrderQty"].ToString(), out orderQty);
                    }
                    if (row["standardvalue"] != DBNull.Value)
                    {
                        _ = decimal.TryParse(row["standardvalue"].ToString(), out standardValue);
                    }

                    decimal qtyUnused = 0;
                    decimal? mutilOrdervalue = orderQty * standardValue;
                    int totalOrder = mutilOrdervalue != null ? (int)mutilOrdervalue : 0;   
                    int reservedQty = _materialsController.ReservedValue(workOrder, inputGoodsCodeForWO);

                    if((totalOrder - reservedQty) > 0)
                    {
                        qtyUnused = (totalOrder - reservedQty);
                    } else
                    {
                        qtyUnused = 0;
                    }
                    listGetWorkOrderMES.Add(new ListGetWorkOrderMES
                    {
                        WorkOrder = workOrder,
                        ProgressOrder = itemLocation,
                        ItemCode = productCode,
                        ItemName = productName,
                        LotNo = lotNo,
                        QtyWo = orderQty,
                        TimeStart = Convert.ToDateTime(startDate),
                        TimeEnd = Convert.ToDateTime(endDate),
                        TimeCreate = Convert.ToDateTime(createdDate),
                        QtyUnused = qtyUnused > 0 ? qtyUnused : 0,
                        Statusname = statusName,
                        ModifyDateUpdate = currentDay,
                        InputGoodsCodeMes = inputGoodsCodeForWO,
                        InputGoodsCodeSeq = standardValue
                    });

                    // Lấy thông tin tồn kho mã NVL theo từng order
                    int qtyTotal = 0;
                    var stockQtyMES = _cl.InventoryQty(itemLocation, inputGoodsCodeForWO);
                    foreach (DataRow stockRow in stockQtyMES.Rows)
                    {
                        string materialCode = stockRow["PRODUCT"].ToString() ?? "";
                        string lotMaterial = stockRow["LOTNO"].ToString() ?? "";
                        if (stockRow["Type"].ToString() == "Inventory")
                        {
                            if (materialCode == inputGoodsCodeForWO)
                            {
                                qtyTotal += Convert.ToInt32(stockRow["QTY"].ToString());
                                var checkAbnomal = CheckAbnormal(materialCode, lotMaterial);
                                if (checkAbnomal != "1")
                                {
                                    int qtyAbnormal = Convert.ToInt32(stockRow["QTY"].ToString());
                                    //qtyTotal -= qtyAbnormal;
                                }
                            }
                        }
                    }
                    listStockMaterials.Add(new ListStockMaterials
                    {
                        ItemMaterial = inputGoodsCodeForWO,
                        MaterialName = nameMaterial,
                        LocationCode = itemLocation,
                        Qty = qtyTotal,
                    });
                }
            }
            if(listGetWorkOrderMES.Count > 0)
            {
                foreach(var itemWO in listGetWorkOrderMES)
                {
                   
                    //Kiểm tra và thêm dữ liệu workorder vào M+
                    var existItem = await _context.TblWorkOrderMes
                   .Where(x => x.WorkOrder == itemWO.WorkOrder)
                   .ToListAsync();
                    if (existItem.Count > 0)
                    {
                        foreach (var item in existItem)
                        {
                            if(item.InputGoodsCodeMes == itemWO.InputGoodsCodeMes)
                            {
                                item.Statusname = itemWO.Statusname;
                                item.ModifyDateUpdate = currentDay;
                                item.QtyUnused = itemWO.QtyUnused != null ? (int)itemWO.QtyUnused : 0;
                            }
                        }
                    }
                    else
                    {
                        var std = new TblWorkOrderMe()
                        {
                            WorkOrder = itemWO.WorkOrder,
                            ProgressOrder = itemWO.ProgressOrder,
                            ItemCode = itemWO.ItemCode,
                            ItemName = itemWO.ItemName,
                            LotNo = itemWO.LotNo,
                            QtyWo = itemWO.QtyWo != null ? (int)itemWO.QtyWo : 0,
                            TimeStart = itemWO.TimeStart,
                            TimeEnd = itemWO.TimeEnd,
                            TimeCreate = itemWO.TimeCreate,
                            QtyUnused = itemWO.QtyUnused != null ? (int)itemWO.QtyUnused : 0,
                            Statusname = itemWO.Statusname,
                            ModifyDateUpdate = currentDay,
                            InputGoodsCodeMes = itemWO.InputGoodsCodeMes,
                            InputGoodsCodeSeq = itemWO.InputGoodsCodeSeq,
                        };
                        _context.TblWorkOrderMes.Add(std);
                    }
                }
               
            }
            // Xử lý để lưu lại thông tin tồn kho của mã NVL theo từng loại NVL
            var groupByName = listStockMaterials.GroupBy(x => x.ItemMaterial)
                .Select(s => new
                {
                    ItemCode = s.Key,
                    ItemName = s.FirstOrDefault()?.MaterialName,
                    TotalQty = s.LastOrDefault()?.Qty,
                    Location = s.LastOrDefault()?.LocationCode,
                }).ToList();
            foreach (var group in groupByName)
            {
                var existItem = _context.TblInventoryMes.Where(x => x.ItemCode == group.ItemCode).FirstOrDefault();
                if (existItem != null)
                {
                    existItem.Qty = group.TotalQty ?? 0;
                    existItem.ItemName = group.ItemName;
                    existItem.LocationCode = group.Location;
                }
                else
                {
                    _context.TblInventoryMes.Add(new TblInventoryMe
                    {
                        ItemCode = group.ItemCode,
                        ItemName = group.ItemName,
                        Qty = group.TotalQty ?? 0,
                        LocationCode = group.Location
                    });
                }
            }
            await _context.SaveChangesAsync();
            var listAllWorkOrders = await _context.TblWorkOrderMes.ToListAsync();
            foreach (var item in listAllWorkOrders)
            {
                if(item.ModifyDateUpdate != null && item.ModifyDateUpdate < currentDay)
                {
                    item.QtyUnused = 0;
                    item.Statusname = "Production end";
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task FetchDataPrintLabelMCDiv()
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
                .Select(s => new Dictionary<string, object>
                {
                    { "dateProd", s.Key ?? "" },
                    { "shifts", s.GroupBy(x => x.ShiftLabel)
                    .Select(shiftGroup => new
                    {
                        shiftLabel = shiftGroup.Key?.ToLower(),
                        infoShifts = shiftGroup.GroupBy(x => x.WorkOrder)
                        .Select(x => new
                        {
                            workOrder = x.Key,
                            productCode = mcprodData.TryGetValue(x.Key ?? "", out var mcprod) ? mcprod.ProductCode : "",
                            lotNo = mcprodData.TryGetValue(x.Key ?? "", out mcprod) ? mcprod.LotNo : "",
                            qtyOrder = mcprodData.TryGetValue(x.Key ?? "", out mcprod) ? mcprod.QtyOrder : 0,
                            statusWorkOrder = _context.TblWorkOrderMes.Where(wm => wm.WorkOrder == x.Key).Select(st => st.Statusname).FirstOrDefault(),
                            character = _context.TblWorkOrderMes.Where(wm => wm.WorkOrder == x.Key).Select(st => st.Character).FirstOrDefault(),
                            machineShift = _context.TblDivLineMcdetails
                            .Where(m => m.WorkOrder == x.Key &&
                            m.ShiftLabel == shiftGroup.Key &&
                            m.DateProd == today)
                            .Select(m => new
                            {
                                machineShift = m.MachineShift,
                                qtyDiv = m.QtyDiv,
                                qtyHasProcessed = 0
                            }).ToList(),
                            typeLabel = x.FirstOrDefault()?.TypeLabel,
                        }).DistinctBy(x => x.workOrder).ToList()
                    }).OrderBy(x => x.shiftLabel).ToList()
                    },
                }).ToList();

            await _hubContext.Clients.All.SendAsync("ReceiveDataMC", JsonConvert.SerializeObject(listItems) ?? "");
        }
        public string CheckAbnormal(string inputGoodsCode, string lotno)
        {
            var checkItem = "";
            var loadData = _cl.CheckAbnormal(inputGoodsCode, lotno);
            List<ItemAbnormal> statusCheck = new();
            foreach (DataRow row in loadData.Rows)
            {
                string issusable = string.Empty;
                if (row["isusable"] != null)
                {
                    issusable = row["isusable"].ToString() ?? "";
                }
                statusCheck.Add(new ItemAbnormal(issusable));
            }
            var items = (from tbl in statusCheck select tbl).ToList();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    checkItem = item.Isusable;
                }
            }
            return checkItem;
        }

        public async Task InActiveUser()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(2);
            var threshold = DateTime.Now - timeout;

            var inactiveUsers = await _context.TblUsers
            .Where(u => u.ActiveUser == true && u.LastPingAt < threshold)
            .ToListAsync();
            foreach (var user in inactiveUsers)
            {
                user.ActiveUser = false;
            }
            _context.SaveChanges();
        }
    }

    public class ListStockMaterials
    {
        public string? ItemMaterial { get; set; }
        public string? MaterialName { get; set; }
        public int? Qty { get; set; }
        public string? LocationCode { get; set; }
    }

    public class ListGetWorkOrderMES
    {
        public string? WorkOrder { get; set; }
        public string? ProgressOrder { get;set; }
        public string? ItemCode { get;set; }
        public string? ItemName { get;set; }
        public string? LotNo { get;set; }
        public string? Statusname { get;set; }
        public string? InputGoodsCodeMes { get;set; }
        public decimal? InputGoodsCodeSeq { get;set; }
        public decimal? QtyWo { get;set; }
        public decimal? QtyUnused { get;set; }
        public DateTime? TimeStart { get;set; }
        public DateTime? TimeEnd { get;set; }
        public DateTime? TimeCreate { get;set; }
        public DateTime? ModifyDateUpdate { get; set; }
    }
}
