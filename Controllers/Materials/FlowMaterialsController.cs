using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Globalization;

namespace MPLUS_GW_WebCore.Controllers.Materials
{
    public class FlowMaterialsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public readonly IWebHostEnvironment _environment;
        public FlowMaterialsController(MplusGwContext context, Classa classa, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _cl = classa ?? throw new ArgumentNullException(nameof(classa));
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("/nguyen-vat-lieu/ti-le-ton-kho")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên vật liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Theo dõi tỷ lệ tồn kho", Url = Url.Action("Index", "FlowMaterials"), IsActive = true },
            };
            var processProd = await _context.TblLocations
                 .Select(s => s.LocationCode).ToListAsync();
            string allLocation = string.Join(',', processProd);
            var loadAllMaterials = _cl.QL_Stock("", "", allLocation);
            List<ListAllMaterials> allMaterials = new();
            List<ListAllWIP> allWIPS = new();

            foreach (DataRow row in loadAllMaterials.Rows)
            {
                string itemCode = row["ARTICLECODE"].ToString() ?? "";
                string lotNo = row["LOTNO"].ToString() ?? "";
                string processCode = row["PLACEDETAILCODE"].ToString() ?? "";
                int totalQty = Convert.ToInt32(row["TOTALQTY"].ToString());
                int reserved = Convert.ToInt32(row["RESERVED"].ToString());
                if (row["ARTICLETYPECODE"].ToString() == "1")
                {
                    string typeMaterial = string.Empty;
                    if (processCode == "01050")
                    {
                        if (itemCode.Contains('Y'))
                        {
                            typeMaterial = "Dân dẫn TYC";
                        }
                        else
                        {
                            typeMaterial = "Dân dẫn Ashitaka";
                        }
                    }
                    if (processCode == "01055")
                    {
                        typeMaterial = "NVL chuôi";
                    }
                    if (processCode == "01060")
                    {
                        if (itemCode.Contains("RG00X100V"))
                        {
                            typeMaterial = "Vật dẫn";
                        }
                        else if (itemCode.EndsWith('B'))
                        {
                            typeMaterial = "Ống bọc ngoài xanh";
                        }
                        else
                        {
                            typeMaterial = "Ống bọc ngoài trắng";
                        }
                    }
                    if (processCode == "01065")
                    {
                        if (itemCode.Contains("RG37X"))
                        {
                            typeMaterial = "Thùng xuất xưởng";
                        }
                        else if (itemCode.Contains("RG36M"))
                        {
                            typeMaterial = "Hộp đơn vị";
                        }
                        else if (itemCode.Contains("RG26X004V"))
                        {
                            typeMaterial = "Nhãn thùng";
                        }
                        else if (itemCode.Contains("-01"))
                        {
                            typeMaterial = "Nhãn nắp";
                        }
                        else
                        {
                            typeMaterial = "Nhãn túi hộp";
                        }
                    }
                    if (processCode == "01070" || processCode == "01075")
                    {
                        if (itemCode.Contains("RG00X"))
                        {
                            typeMaterial = "Kẹp ống bọc ngoài";
                        }
                        else if (itemCode.Contains("RG30M"))
                        {
                            typeMaterial = "Túi đóng gói";
                        }
                        else
                        {
                            typeMaterial = "Hướng dẫn sử dụng";
                        }
                    }
                    allMaterials.Add(new ListAllMaterials
                    {
                        ItemCode = itemCode,
                        LotNo = lotNo,
                        ProcessCode = processCode,
                        ProcessName = _context.TblLocations.Where(x => x.LocationCode == processCode).Select(s => s.LocationName).FirstOrDefault(),
                        TotalQty = totalQty,
                        StockNotUsed = reserved,
                        TypeMaterial = typeMaterial,
                    });
                }
                else
                {
                    string typeWips = string.Empty;
                    if(processCode == "01060")
                    {
                        if(itemCode.StartsWith("RG80"))
                        {
                            if(itemCode.EndsWith("Y"))
                            {
                                typeWips = "Bán thành phẩm gia công TYC";
                            } else
                            {
                                typeWips = "Bàn thành phẩm gia công";
                            }
                        } else
                        {
                            typeWips = "Chuỗi đã in";
                        }
                    } else
                    {
                        if(itemCode.EndsWith("M.E"))
                        {
                            typeWips = "Hộp đơn vị đã dán nhãn";
                        } 
                        if(itemCode.EndsWith("M.F"))
                        {
                            typeWips = "Thùng xuất xưởng đã dán nhãn";
                        }
                        if (itemCode.EndsWith("M.G"))
                        {
                            typeWips = "Nhãn TĐG đã in";
                        }
                        if (itemCode.StartsWith("RG85"))
                        {
                            if(itemCode.EndsWith("Y"))
                            {
                                typeWips = "Đã luồn dây dẫn TYC";
                            } else
                            {
                                typeWips = "Đã luồn dây dẫn";
                            }
                        }
                    }
                    allWIPS.Add(new ListAllWIP
                    {
                        ItemCode = itemCode,
                        ItemName = row["ARTICLESHORTNAME"].ToString() ?? "",
                        LotNo = lotNo,
                        ProcessCode = processCode,
                        ProcessName = _context.TblLocations.Where(x => x.LocationCode == processCode).Select(s => s.LocationName).FirstOrDefault(),
                        TotalQty = totalQty,
                        StockNotUsed = reserved,
                        TypeMaterial = typeWips,
                    });
                }
            }
            var groupMaterialsLocation = allMaterials.GroupBy(x => x.ItemCode)
                .Select(s => new ListAllMaterials
                {
                    ItemCode = s.Key,
                    LotNo = s.FirstOrDefault()?.LotNo,
                    ProcessCode = s.FirstOrDefault()?.ProcessCode,
                    ProcessName = s.FirstOrDefault()?.ProcessName,
                    TotalQty = s.Sum(x => x.TotalQty),
                    StockNotUsed = s.Sum(x => x.StockNotUsed),
                    TypeMaterial = s.FirstOrDefault()?.TypeMaterial,
                }).ToList();
            var groupWips = allWIPS.GroupBy(x => x.ItemCode)
                .Select(s => new ListAllWIP
                {
                    ItemCode = s.Key,
                    LotNo = s.FirstOrDefault()?.LotNo,
                    ItemName = s.FirstOrDefault()?.ItemName,
                    ProcessCode = s.FirstOrDefault()?.ProcessCode,
                    ProcessName = s.FirstOrDefault()?.ProcessName,
                    TotalQty = s.Sum(x => x.TotalQty),
                    StockNotUsed = s.Sum(x => x.StockNotUsed),
                    TypeMaterial = s.FirstOrDefault()?.TypeMaterial,
                }).ToList();
            List<ListAllMaterials> resultItems = new();
            List<ListAllWIP> resultWips = new();
            foreach (var item in groupMaterialsLocation)
            {
                int? stockNoPlanUsed = 0;
                if (item.StockNotUsed == 0)
                {
                    stockNoPlanUsed = item.TotalQty;
                    var existDateProd = _context.TblWorkOrderMes
                        .Where(x => x.InputGoodsCodeMes == item.ItemCode && x.DateProd == DateTime.Now.Date)
                        .FirstOrDefault();
                    if (existDateProd != null)
                    {
                        stockNoPlanUsed -= item.TotalQty;
                    }
                }
                resultItems.Add(new ListAllMaterials
                {
                    ItemCode = item.ItemCode,
                    LotNo = item.LotNo,
                    ProcessCode = item.ProcessCode,
                    ProcessName = item.ProcessName,
                    TotalQty = item.TotalQty,
                    StockNotUsed = stockNoPlanUsed,
                    TypeMaterial = item.TypeMaterial,
                });
            }
            foreach (var item in groupWips)
            {
                int? stockNoPlanUsed = 0;
                if (item.StockNotUsed == 0)
                {
                    stockNoPlanUsed = item.TotalQty;
                    var existDateProd = _context.TblWorkOrderMes
                        .Where(x => x.InputGoodsCodeMes == item.ItemCode && x.DateProd == DateTime.Now.Date)
                        .FirstOrDefault();
                    if (existDateProd != null)
                    {
                        stockNoPlanUsed -= item.TotalQty;
                    }
                }
                resultWips.Add(new ListAllWIP
                {
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    LotNo = item.LotNo,
                    ProcessCode = item.ProcessCode,
                    ProcessName = item.ProcessName,
                    TotalQty = item.TotalQty,
                    StockNotUsed = stockNoPlanUsed,
                    TypeMaterial = item.TypeMaterial,
                });
            }
            var groupMaterialsType = resultItems.GroupBy(x => x.TypeMaterial)
                .Select(s => new ListAllMaterials
                {
                    TypeMaterial = s.Key,
                    ItemCode = string.Join(',', s.Select(x => x.ItemCode).ToList()),
                    LotNo = string.Join(',', s.Select(x => x.LotNo).ToList()),
                    ProcessCode = s.FirstOrDefault()?.ProcessCode,
                    ProcessName = s.FirstOrDefault()?.ProcessName,
                    TotalQty = s.Sum(x => x.TotalQty),
                    StockNotUsed = s.Sum(x => x.StockNotUsed),
                }).ToList();
            var groupWipsType = resultWips.GroupBy(x => x.TypeMaterial)
                  .Select(s => new ListAllWIP
                  {
                      TypeMaterial = s.Key,
                      ItemCode = string.Join(',', s.Select(x => x.ItemCode).ToList()),
                      ItemName = string.Join(',', s.Select(x => x.ItemName).ToList()),
                      LotNo = string.Join(',', s.Select(x => x.LotNo).ToList()),
                      ProcessCode = s.FirstOrDefault()?.ProcessCode,
                      ProcessName = s.FirstOrDefault()?.ProcessName,
                      TotalQty = s.Sum(x => x.TotalQty),
                      StockNotUsed = s.Sum(x => x.StockNotUsed),
                  }).ToList();

            ViewData["ListAllMaterials"] = groupMaterialsType;
            ViewData["ListAllWips"] = groupWipsType;
            return View();
        }

        [Route("/nguyen-vat-lieu/so-quan-ly")]
        public IActionResult ViewMaterialInBook()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên vật liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Sổ quản lý", Url = Url.Action("ViewMaterialInBook", "FlowMaterials"), IsActive = true },
            };
            List<ListAllMaterials> allMaterials = new();
            var listAllMaterials = _context.TblInventoryMes
                .OrderBy(x => x.LocationCode)
                .ToList();
            foreach (var item in listAllMaterials)
            {
                allMaterials.Add(new ListAllMaterials
                {
                    ItemCode = item.ItemCode,
                });
            }
            ViewData["AllMaterials"] = allMaterials;
            return View();
        }

        [HttpPost]
        public IActionResult GetContentMaterials([FromForm] string selectMaterials, string selectMonth)
        {
            try
            {
                DateTime.TryParseExact(selectMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthValue);
                var getInfoMaterial = _context.TblInventoryMes
                        .Where(x => x.ItemCode == selectMaterials)
                        .Select(s => new
                        {
                            materialCode = s.ItemCode ?? "",
                            materialName = s.ItemName ?? "",
                        }).ToList();
                var results = _context.TblImportedItems
                 .Where(x => x.ItemCode == selectMaterials && x.TimeImport != null &&
                 x.TimeImport.Value.Month == monthValue.Month &&
                 x.TimeImport.Value.Year == monthValue.Year)
                 .GroupBy(x => x.LotNo)
                 .Select(lot => new
                 {
                     LotNo = lot.Key,
                 }).ToList();

                return Ok(new { infoMaterials = getInfoMaterial, infoLotMaterials = results });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetContentByLot([FromForm] string selectMaterial, string lotMaterial)
        {
            try
            {
                // Lấy thông tin NVL được nhập theo số lô
                var resultImportedItems = _context.TblImportedItems
                    .Where(x => x.ItemCode == selectMaterial && x.LotNo == lotMaterial)
                    .Select(s => new InfoMaterialImported
                    {
                        DateImported = s.TimeImport != null ? s.TimeImport.Value.ToString("dd/MM") : "",
                        RequestNo = s.RequestNo,
                        LotNo = s.LotNo,
                        Qty = s.Qty.ToString(),
                        UserImported = _context.TblUsers
                            .Where(x => x.IdUser == s.IdUser)
                            .Select(u => u.EmployeeNo + " " + u.DisplayName).FirstOrDefault(),
                    })
                    .OrderBy(x => x.RequestNo)
                    .ToList();

                resultImportedItems = resultImportedItems
                    .GroupBy(x => new { x.DateImported, x.RequestNo })
                    .Select(s => new InfoMaterialImported
                    {
                        DateImported = s.Key.DateImported,
                        RequestNo = s.Key.RequestNo,
                        Qty = s.Sum(x => int.Parse(x.Qty ?? "0")).ToString(),
                        UserImported = s.FirstOrDefault()?.UserImported,
                        Note = s.FirstOrDefault()?.Note,
                        LotNo = s.FirstOrDefault()?.LotNo
                    }).ToList();


                // Lấy thông tin NVL được Reserved MES
                var getInfoMaterialImported = _context.TblImportedItems
                   .Where(x => x.ItemCode == selectMaterial &&
                   x.LotNo == lotMaterial)
                   .Select(s => new InfoMaterialImported
                   {
                       DateImported = s.TimeImport != null ? s.TimeImport.Value.ToString("dd/MM") : "",
                       RequestNo = s.RequestNo,
                       LotNo = s.LotNo,
                       Qty = s.Qty.ToString(),
                       UserImported = _context.TblUsers
                       .Where(x => x.IdUser == s.IdUser)
                       .Select(u => u.DisplayName).FirstOrDefault(),
                       Note = ""
                   }).OrderBy(x => x.RequestNo).ToList();
                var getLocationGWs = _context.TblLocations
                    .Select(s => s.LocationCode).ToList();
                var getInfoHistoryReserved = _cl.History_Reserved(selectMaterial, lotMaterial);
                var infoMaterialPickings = new List<InfoMaterialPicking>();
                var processedEntriesMaterials = new List<ProcessedEntriesMaterial>();
                foreach (DataRow rowInfo in getInfoHistoryReserved.Rows)
                {
                    if (getLocationGWs.Contains(rowInfo["WORKCODE"].ToString()))
                    {
                        DateTime dateExported = rowInfo["CONFIRMEDDATE"] != DBNull.Value
                           ? Convert.ToDateTime(rowInfo["CONFIRMEDDATE"])
                           : DateTime.MinValue;

                        if (dateExported != DateTime.MinValue)
                        {
                            var productCode = rowInfo["SEMIPRODUCTCODE"].ToString() ?? "";
                            var receivedQty = int.Parse(rowInfo["RECEIVEVALUE"].ToString() ?? "0");
                            var lotProduction = rowInfo["LOTFG"].ToString();
                            var lotMaterialUsed = rowInfo["LOTNO"].ToString() ?? "";
                            var userExported = rowInfo["CONFIRMERCODE"].ToString() + " " + rowInfo["CONFIRMERNAME"].ToString();
                            var inputGoodCode = rowInfo["INPUTGOODSCODE"].ToString();

                            infoMaterialPickings.Add(new InfoMaterialPicking
                            {
                                DateUsed = dateExported,
                                ProductCode = productCode,
                                LotProduction = lotProduction,
                                RequestNo = "",
                                LotMaterial = lotMaterialUsed,
                                QtyUsed = receivedQty,
                                UserExported = userExported.ToString(),
                            });
                        }
                    }
                }
                infoMaterialPickings = infoMaterialPickings.OrderBy(x => x.DateUsed).ThenBy(x => x.LotProduction).ToList();
                getInfoMaterialImported = getInfoMaterialImported
                    .GroupBy(x => new { x.DateImported, x.RequestNo })
                    .Select(s => new InfoMaterialImported
                    {
                        DateImported = s.Key.DateImported,
                        RequestNo = s.Key.RequestNo,
                        Qty = s.Sum(x => int.Parse(x.Qty ?? "0")).ToString(),
                        UserImported = s.FirstOrDefault()?.UserImported,
                        Note = s.FirstOrDefault()?.Note,
                        LotNo = s.FirstOrDefault()?.LotNo
                    }).ToList();
                int importedIndex = 0; // Index của danh sách NVL đã nhập
                foreach (var itemPicking in infoMaterialPickings)
                {
                    int qtyToSubtract = itemPicking.QtyUsed ?? 0; // Lấy số cần trừ

                    // Lặp khi trừ hết của một số request sẽ sang số tiếp theo
                    while (qtyToSubtract > 0 && importedIndex < getInfoMaterialImported.Count)
                    {
                        var currentImported = getInfoMaterialImported[importedIndex]; // lấy số request hiện tại
                        int availableQty = int.Parse(currentImported.Qty ?? "0"); // Lấy số lượng đã nhập của số request hiện tại
                        int amountToSubtract = (qtyToSubtract >= availableQty) ? availableQty : qtyToSubtract; // So sánh lấy số lượng nhỏ nhất giữa hai số trừ và số bị trừ hiện tại

                        qtyToSubtract -= amountToSubtract; // Cập nhật lại số cần trừ với số lượng so sánh nhỏ nhất
                        int newStock = availableQty - amountToSubtract;

                        getInfoMaterialImported[importedIndex].Qty = newStock.ToString(); // Trừ để cập nhật lại số lượng bị trừ của số request hiện tại

                        processedEntriesMaterials.Add(new ProcessedEntriesMaterial
                        {
                            DateUsed = itemPicking.DateUsed,
                            ProductCode = itemPicking.ProductCode,
                            LotProduction = itemPicking.LotProduction,
                            RequestNo = currentImported.RequestNo,
                            LotMaterial = itemPicking.LotMaterial,
                            QtyPicking = itemPicking.QtyUsed.ToString(),
                            QtyUsed = amountToSubtract.ToString(),
                            QtyDifference = "",
                            InventoryUsed = getInfoMaterialImported[importedIndex].Qty,
                            UserExported = itemPicking.UserExported,
                            NoteExported = "",
                        });

                        // Chuyển tiếp sang số request sau nếu số lượng trừ nhỏ hơn hoặc bằng 0
                        if (newStock == 0)
                        {
                            importedIndex++;
                        }
                    }
                }

                return Ok(new { infoImported = resultImportedItems, infoMaterialUsed = processedEntriesMaterials.OrderBy(x => x.DateUsed).ThenBy(x => x.RequestNo).ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// API tải file sổ quản lý NVL
        /// </summary>
        /// <param name="selectMaterials"></param>
        /// <param name="infoMaterialImported"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportData([FromForm] string selectMaterials, string lotMaterial, string infoMaterialImported, string infoMaterialUsed)
        {
            try
            {
                var getInfoMaterial = _context.TblInventoryMes
                     .Where(x => x.ItemCode == selectMaterials)
                     .Select(s => new Dictionary<string, string>
                     {
                            { "MaterialCode", s.ItemCode??"".ToString() },
                            { "MaterialName", s.ItemName??"".ToString() },
                            { "Unit", "pcs"},
                            { "Year", DateTime.Now.Year.ToString() }
                     }).ToList();

                List<InfoMaterialImported>? infoMaterialImportedDeserialize = JsonConvert.DeserializeObject<List<InfoMaterialImported>>(infoMaterialImported);
                List<InfoMaterialPickingExported>? infoMaterialUseds = JsonConvert.DeserializeObject<List<InfoMaterialPickingExported>>(infoMaterialUsed);

                var templatePath = Path.Combine(_environment.ContentRootPath, "templates", "JCQ10-GW001-2-Rev Sổ quản lý GW.xlsx");
                var fileInfo = new FileInfo(templatePath);
                var oldName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var newFileName = oldName + "_" + selectMaterials + "_" + lotMaterial + ".xlsx";

                string savedFolderPath = Path.Combine(_environment.ContentRootPath, "SavedFiles", newFileName);
                if (System.IO.File.Exists(savedFolderPath))
                    System.IO.File.Delete(savedFolderPath);
                // Tạo file
                CreateFileManageBook(savedFolderPath, fileInfo);
                // Thêm dữ liệu đã nhập vào file
                AddDataToExcel(savedFolderPath, getInfoMaterial, infoMaterialImportedDeserialize);
                // Xử lý thêm dữ liệu nvl sử dụng
                AddInfoMaterialUsed(savedFolderPath, infoMaterialUseds);

                // Xử lý để tải file về
                var newFileInfo = new FileInfo(savedFolderPath);
                ExcelPackage fileDownload = new(newFileInfo);
                byte[] fileContents = fileDownload.GetAsByteArray();
                return Ok(new { encodeFileName = Convert.ToBase64String(fileContents), fileDownloadName = newFileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo file sổ quản lý
        /// </summary>
        /// <param name="newFilePath"></param>
        /// <param name="fileInfo"></param>
        private static void CreateFileManageBook(string newFilePath, FileInfo fileInfo)
        {
            ExcelPackage oldFile = new(fileInfo);
            ExcelPackage newFile = new();

            ExcelWorksheet oldWorkSheet = oldFile.Workbook.Worksheets[0];
            oldWorkSheet.View.ZoomScale = 100;
            oldWorkSheet.ConditionalFormatting.RemoveAll();
            newFile.Workbook.Worksheets.Add(oldWorkSheet.Name);

            FileStream objFileStrm = System.IO.File.Create(newFilePath);
            objFileStrm.Seek(0, SeekOrigin.Begin);
            newFile.SaveAs(objFileStrm);
            objFileStrm.Close();
            byte[] data = oldFile.GetAsByteArray();
            System.IO.File.WriteAllBytes(newFilePath, data);
        }

        /// <summary>
        /// Thêm dữ liệu vào file sổ quản lý
        /// </summary>
        /// <param name="newFileInfo">File name excel mới</param>
        /// <param name="infoMaterial">Thông tin nguyên vật liệu</param>
        /// <param name="infoImportedMaterials">Thông tin các nvl đã nhập trong tháng</param>
        private static void AddDataToExcel(string newFileInfo, List<Dictionary<string, string>> infoMaterial, List<InfoMaterialImported>? infoImportedMaterials)
        {
            var fileInfo = new FileInfo(newFileInfo);
            ExcelPackage package = new(fileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            worksheet.View.ZoomScale = 100;
            foreach (var item in infoMaterial)
            {
                foreach (var value in item)
                {
                    string defineName = value.Key;
                    string valueAdd = value.Value;

                    var namedRange = package.Workbook.Names[defineName];
                    if (namedRange != null)
                    {
                        var cell = namedRange.Worksheet.Cells[namedRange.Address];
                        cell.Value = valueAdd;
                    }
                }
            }
            var defineNameImportMaterials = package.Workbook.Names["ImportMaterials"];
            if (defineNameImportMaterials != null && infoImportedMaterials != null)
            {
                var worksheetDefineName = defineNameImportMaterials.Worksheet;
                int titleRow = defineNameImportMaterials.End.Row;
                int templateRow = titleRow + 1;
                int colStart = defineNameImportMaterials.Start.Column;
                int colEnd = defineNameImportMaterials.End.Column;

                int insertAtRow = templateRow;
                foreach (var item in infoImportedMaterials)
                {
                    int currentCol = colStart;
                    if (worksheetDefineName.Cells[templateRow, currentCol].Value != null)
                    {
                        worksheetDefineName.InsertRow(insertAtRow, 1, templateRow);
                        for (int col = colStart; col <= colEnd; col++)
                        {
                            var templateCell = worksheetDefineName.Cells[templateRow, col];
                            var newCell = worksheetDefineName.Cells[insertAtRow, col];

                            newCell.StyleID = templateCell.StyleID;

                            if (templateCell.Merge)
                            {
                                var mergedRange = worksheetDefineName.MergedCells[templateCell.Start.Row, templateCell.Start.Column];
                                if (mergedRange != null)
                                {
                                    var mergeAddress = new ExcelAddress(mergedRange);
                                    worksheetDefineName.Cells[insertAtRow, mergeAddress.Start.Column, insertAtRow, mergeAddress.End.Column].Merge = true;
                                }
                            }
                        }
                    }
                    foreach (var value in new[] {
                        item.DateImported,
                        item.RequestNo,
                        item.LotNo,
                        item.Qty,
                        item.UserImported,
                        item.Note,
                    })
                    {
                        var cellAdd = worksheetDefineName.Cells[insertAtRow, currentCol];
                        if (cellAdd.Merge)
                        {
                            var mergedRange = worksheetDefineName.MergedCells[insertAtRow, currentCol];
                            var mergeAddress = new ExcelAddress(mergedRange);

                            cellAdd = worksheetDefineName.Cells[insertAtRow, mergeAddress.Start.Column];
                            currentCol = mergeAddress.End.Column + 1;
                        }
                        if(!string.IsNullOrEmpty(value?.ToString()))
                        {
                            cellAdd.Value = value;
                        } else
                        {
                            cellAdd.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.DiagonalDown = true;
                        }
                        currentCol++;
                    }
                    insertAtRow++;
                }
            }
            package.Save();
        }

        private static void AddInfoMaterialUsed(string newFileInfo, List<InfoMaterialPickingExported>? infoMaterialUseds)
        {
            var fileInfo = new FileInfo(newFileInfo);
            ExcelPackage package = new(fileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            worksheet.View.ZoomScale = 100;

            var defineNameImportMaterials = package.Workbook.Names["UsedMaterials"];
            if (defineNameImportMaterials != null && infoMaterialUseds != null)
            {
                var worksheetDefineName = defineNameImportMaterials.Worksheet;
                int titleRow = defineNameImportMaterials.End.Row;
                int templateRow = titleRow + 1;
                int colStart = defineNameImportMaterials.Start.Column;
                int colEnd = defineNameImportMaterials.End.Column;

                int insertAtRow = templateRow;
                foreach (var item in infoMaterialUseds)
                {
                    int currentCol = colStart;
                    if (worksheetDefineName.Cells[templateRow, currentCol].Value != null)
                    {
                        worksheetDefineName.InsertRow(insertAtRow, 1, templateRow);
                        for (int col = colStart; col <= colEnd; col++)
                        {
                            var templateCell = worksheetDefineName.Cells[templateRow, col];
                            var newCell = worksheetDefineName.Cells[insertAtRow, col];

                            newCell.StyleID = templateCell.StyleID;

                            if (templateCell.Merge)
                            {
                                var mergedRange = worksheetDefineName.MergedCells[templateCell.Start.Row, templateCell.Start.Column];
                                if (mergedRange != null)
                                {
                                    var mergeAddress = new ExcelAddress(mergedRange);
                                    worksheetDefineName.Cells[insertAtRow, mergeAddress.Start.Column, insertAtRow, mergeAddress.End.Column].Merge = true;
                                }
                            }
                        }
                    }
                    foreach (var value in new[] {
                        item.DateUsed,
                        item.ProductCode,
                        item.LotProduction,
                        item.RequestNo,
                        item.LotMaterial,
                        item.QtyPicking,
                        item.QtyUsed,
                        item.QtyDifference,
                        item.InventoryUsed,
                        item.UserExported,
                        item.NoteExported,
                    })
                    {
                        var cellAdd = worksheetDefineName.Cells[insertAtRow, currentCol];
                        if (cellAdd.Merge)
                        {
                            var mergedRange = worksheetDefineName.MergedCells[insertAtRow, currentCol];
                            var mergeAddress = new ExcelAddress(mergedRange);

                            cellAdd = worksheetDefineName.Cells[insertAtRow, mergeAddress.Start.Column];
                            currentCol = mergeAddress.End.Column + 1;
                        }
                        if(string.IsNullOrEmpty(value?.ToString()))
                        {
                            cellAdd.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                            cellAdd.Style.Border.DiagonalDown = true;
                        } else
                        {
                            cellAdd.Value = value;
                        }
                        currentCol++;
                    }
                    insertAtRow++;
                }
            }
            package.Save();
        }
    }

    public class ListAllMaterials
    {
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public int? TotalQty { get; set; }
        public int? StockNotUsed { get; set; }
        public string? TypeMaterial { get; set; }
        public string? ProcessCode { get; set; }
        public string? ProcessName { get; set; }
    }

    public class ListAllWIP
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? LotNo { get; set; }
        public int? TotalQty { get; set; }
        public int? StockNotUsed { get; set; }
        public string? TypeMaterial { get; set; }
        public string? ProcessCode { get; set; }
        public string? ProcessName { get; set; }
    }

    public class InfoMaterialImported
    {
        public string? DateImported { get; set; }
        public string? RequestNo { get; set; }
        public string? LotNo { get; set; }
        public string? Qty { get; set; }
        public string? UserImported { get; set; }
        public string? Note { get; set; }
    }

    public class InfoMaterialPicking
    {
        public DateTime? DateUsed { get; set; }
        public string? ProductCode { get; set; }
        public string? LotProduction { get; set; }
        public string? RequestNo { get; set; }
        public string? LotMaterial { get; set; }
        public int? QtyUsed { get; set; }
        public string? UserExported { get; set; }
    }

    public class InfoMaterialPickingExported
    {
        public string? DateUsed { get; set; }
        public string? ProductCode { get; set; }
        public string? LotProduction { get; set; }
        public string? RequestNo { get; set; }
        public string? LotMaterial { get; set; }
        public string? QtyPicking { get; set; }
        public string? QtyUsed { get; set; }
        public string? QtyDifference { get; set; }
        public string? InventoryUsed { get; set; }
        public string? UserExported { get; set; }
        public string? NoteExported { get; set; }
    }

    public class ProcessedEntriesMaterial
    {
        public DateTime? DateUsed { get; set; }
        public string? ProductCode { get; set; }
        public string? LotProduction { get; set; }
        public string? RequestNo { get; set; }
        public string? LotMaterial { get; set; }
        public string? QtyPicking { get; set; }
        public string? QtyUsed { get; set; }
        public string? QtyDifference { get; set; }
        public string? InventoryUsed { get; set; }
        public string? UserExported { get; set; }
        public string? NoteExported { get; set; }
    }
}
