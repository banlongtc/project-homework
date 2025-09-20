using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Serilog;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MPLUS_GW_WebCore.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly MplusGwContext _context;
        public readonly IWebHostEnvironment _environment;
        public AdminController(MplusGwContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Route("/admin/trang-chu")]
        public IActionResult Index()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //if(HttpContext.Session.GetInt32("User ID") == null)
            //{
            //    return RedirectToAction("Login", "Home");
            //}
            //if (HttpContext.Session.GetString("User Role") == "Users")
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            return View();
        }

        [Route("/admin/manager-products")]
        public IActionResult ManagerProducts()
        {
            var allWorkOrderMes = _context.TblWorkOrderMes.AsEnumerable().ToList();
            var getInfoWO = _context.TblWorkOrderProcessings
                .AsEnumerable()
                .ToList()
                .Select(s => new RenderProductions
                {
                    IdProduction = 0,
                    WorkOrder = s.Woprocessing,
                    ProductCode = s.ProductCode,
                    ProductName = allWorkOrderMes.Where(x => x.WorkOrder == s.Woprocessing).Select(w => w.ItemName).FirstOrDefault(),
                    LotNo = s.LotProcessing,
                    StatusProduction = s.ProcessingStatus,
                    IsStopped = _context.TblChecksheetFormEntries.Where(x => x.WorkOrderCode == s.Woprocessing).Select(e => e.IsStopped).FirstOrDefault(),
                }).ToList();
            ViewData["ListProductProductions"] = getInfoWO;
            return View();
        }

        [Route("/admin/manager-products/edit/{productionWO}")]
        public IActionResult EditProduction(string productionWO)
        {
            var infoEntriesByWO = _context.TblChecksheetFormEntries
                .Where(x => x.WorkOrderCode == productionWO && !string.IsNullOrEmpty(x.TrayNo))
                .GroupBy(x => x.PositionCode)
                .Select(s => new
                {
                    positionCode = s.Key,
                    checksheetVerId = s.Select(cs => cs.ChecksheetVerId).FirstOrDefault(),
                    listTrays = s
                    .GroupBy(g => g.TrayNo)
                    .Select(g => new
                    {
                        trayNo = g.Key,
                        listEntries = g.Select(e => new
                        {
                            e.EntryIndex,
                            e.QtyOfReads,
                            e.QtyProduction,
                            e.QtyOk,
                            e.QtyNg,
                        })
                    }).ToList(),
                }).ToList();
            ViewData["JsonValueEntries"] = JsonConvert.SerializeObject(infoEntriesByWO);
            return View();
        }
        [HttpPost]
        public IActionResult ResetWorkOrder([FromBody] RequestRenderForm requestRenderForm)
        {
            if(requestRenderForm == null)
            {
                return BadRequest(new { message = "Không có yêu cầu được gửi lên" });
            }

            string? workOrder = requestRenderForm.WorkOrder;
            var workOrderProduction = _context.TblWorkOrderProcessings
                .Where(x => x.Woprocessing == workOrder)
                .FirstOrDefault();
            if (workOrderProduction == null) {
                return StatusCode(500, new { message = "Không tìm thấy chỉ thị này" });            
            }
            // Xóa thông tin chỉ thị đã làm trước đấy
            var detailFrequencies = _context.TblDetailWofrequencies.Where(x => x.WoProcessId == workOrderProduction.Id)
                .ToList();
            var workOrderEntries = _context.TblChecksheetFormEntries.Where(x => x.WorkOrderCode == workOrder)
                .ToList();
            var entryValueDelete = new List<TblChecksheetEntryValue>();
            foreach (var item in workOrderEntries)
            {
                var dataEntryValue = _context.TblChecksheetEntryValues.Where(x => x.FormEntryId == item.FormEntryId)
                    .FirstOrDefault();
                if (dataEntryValue != null)
                {
                    entryValueDelete.Add(dataEntryValue);
                }
            }
            if(entryValueDelete.Any())
            {
                _context.TblChecksheetEntryValues.RemoveRange(entryValueDelete);
                _context.TblChecksheetFormEntries.RemoveRange(workOrderEntries);
            }
            _context.TblDetailWofrequencies.RemoveRange(detailFrequencies);
            _context.TblWorkOrderProcessings.RemoveRange(workOrderProduction);
            _context.SaveChanges();
            return Ok(new { message = "Reset chỉ thị thành công. Chỉ thị đã được phép làm tiếp." });
        }

        [Route("/admin/download-checksheet")]
        public IActionResult PrintCheckSheet()
        {
            var getAllWorkOrder = _context.TblWorkOrderProcessings
                .Select(s => new TblWorkOrderProcessing
                {
                    Woprocessing = s.Woprocessing,
                    ProcessingStatus = s.ProcessingStatus,
                    ProductCode = s.ProductCode,
                    LotProcessing = s.LotProcessing,
                    PositionCode = s.PositionCode,
                }).ToList();
            ViewData["AllWorkOrder"] = getAllWorkOrder;
            return View();
        }

        [HttpPost]
        public IActionResult GetChecksheetWithWO([FromBody] RequestDataDownload requestDataDownload)
        {
            if (requestDataDownload == null)
            {
                return BadRequest(new { message = "Request not found" });
            }
            string? workorder = requestDataDownload.WorkOrder;
            var checksheetVerIds = _context.TblChecksheetFormEntries
                .Where(x => x.WorkOrderCode == workorder && x.ProcessStatus != "Exported")
                .Select(s => new
                {
                    s.ChecksheetVerId
                }).Distinct().ToList();
            var infoChecksheets = new List<Dictionary<string, object>>();
            foreach (var item in checksheetVerIds)
            {
                var infoChecksheet = (from s in _context.TblChecksheetsUploads
                                      join csv in _context.TblChecksheetVersions on s.ChecksheetId equals csv.ChecksheetId
                                      where csv.ChecksheetVersionId == item.ChecksheetVerId && s.ChecksheetId == csv.ChecksheetId
                                      select new
                                      {
                                          s.ChecksheetId,
                                          s.ChecksheetCode,
                                          csv.ChecksheetVersionId,
                                          csv.FileName,
                                          csv.FilePath,
                                          csv.VersionNumber,
                                          csv.SheetName,
                                      }).FirstOrDefault();
                infoChecksheets.Add(new Dictionary<string, object>
                {
                    {"checksheetId", infoChecksheet?.ChecksheetId ?? 0 },
                    {"checksheetCode", infoChecksheet?.ChecksheetCode ?? "" },
                    {"checksheetVerId", infoChecksheet?.ChecksheetVersionId ?? 0 },
                    {"fileName", infoChecksheet?.FileName ?? "" },
                    {"filePath", infoChecksheet?.FilePath ?? "" },
                    {"sheetName", infoChecksheet?.SheetName ?? "" },
                    {"versionNumber", infoChecksheet?.VersionNumber ?? 0 },
                });
            }

            return Ok(new { dataChecksheets = infoChecksheets });
        }

        [HttpPost]
        public IActionResult GetCheckSheetPosition([FromBody] RequestDataDownload requestDataDownload)
        {
            if (requestDataDownload == null)
            {
                return BadRequest(new { message = "Request not found" });
            }
            try
            {
                int? checksheetVerId = requestDataDownload.ChecksheetVerId;
                var positionChecksheet = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == checksheetVerId && x.ChecksheetType != "CHECKSHEET_CONDITIONS")
                    .Select(s => (from mp in _context.TblMasterPositions
                                        join lc in _context.TblLocationCs on mp.LocationChildId equals lc.Id
                                        where lc.LocationCodeC == s.PositionWorkingCode && lc.Id == mp.LocationChildId
                                        select new
                                        {
                                            mp.PositionCode,
                                            mp.PositionName,
                                        }).ToList()
                    ).FirstOrDefault();
                return Ok(new { listPositions = positionChecksheet});
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DownloadChecksheet([FromBody] RequestDataDownload requestDataDownload)
        {
            if (requestDataDownload == null)
            {
                return BadRequest(new { message = "Request not found" });
            }
            try
            {
                var userLoggin = HttpContext.Session.GetString("DisplayName")?.ToString();

                int? checksheetVerId = requestDataDownload.ChecksheetVerId;
                string? workOrder = requestDataDownload.WorkOrder;
                string? filePath = requestDataDownload.FilePath;
                string? sheetName = requestDataDownload.SheetName;
                string? positionCode = requestDataDownload.PositionCode;

                // Bắt đầu truy vấn
                var queryAllEntries = _context.TblChecksheetFormEntries
                    .Where(x => x.WorkOrderCode == workOrder && x.ChecksheetVerId == checksheetVerId);

                if(!string.IsNullOrEmpty(positionCode))
                {
                    queryAllEntries = queryAllEntries.Where(x => x.PositionCode == positionCode);
                }

                var existingEntries = queryAllEntries.ToList();

                if(!existingEntries.Any())
                {
                    return StatusCode(500, new { message = "Không có dữ liệu nhập cho Work Order này hoặc vị trí này" });
                }

                string? lineCode = null;

                if (!string.IsNullOrEmpty(positionCode))
                {
                    var idLine = _context.TblMasterPositions
                        .Where(x => x.PositionCode == positionCode)
                        .Select(s => s.IdLine)
                        .FirstOrDefault();

                    if (idLine != null)
                    {
                        lineCode = _context.TblProdLines
                            .Where(x => x.IdLine == idLine)
                            .Select(s => s.LineCode)
                            .FirstOrDefault();
                    }
                }

                // Đọc file template lấy ra và tạo file clone và thêm dữ liệu vào checksheet
                var templatePath = Path.Combine(_environment.ContentRootPath, "templates", filePath?.ToString() ?? "");
                byte[] templateBytes = System.IO.File.ReadAllBytes(templatePath);

                // Tạo file excel trong bộ nhớ và thêm dữ liệu
                var packageBytes = AddDataToExcelFile(templateBytes, sheetName, workOrder, checksheetVerId, _context, positionCode);
                byte[] fileContents = packageBytes;

                var newFileName = Path.GetFileNameWithoutExtension(filePath) + "_" + workOrder;
                if(!string.IsNullOrEmpty(lineCode))
                {
                    newFileName += "_" + lineCode;
                }
                newFileName += ".xlsx";
                if (fileContents.Length > 0)
                {
                    var woprocessing = _context.TblWorkOrderProcessings
                      .Where(x => x.Woprocessing == workOrder && x.PositionCode == positionCode && x.ProcessingStatus == "Production end")
                      .FirstOrDefault();
                    if (woprocessing != null)
                    {
                        woprocessing.ProcessingStatus = "Exported";
                    }

                    foreach (var item in existingEntries)
                    {
                        item.IsExported = true;
                        item.ProcessStatus = woprocessing != null ? "Exported" : item.ProcessStatus;
                        item.ExportedAt = DateTime.Now;
                        item.ExportedBy = userLoggin ?? "Admin";

                        var logEntry = _context.TblChecksheetFormEntryHistories
                            .Where(x => x.OriginalFormEntryId == item.FormEntryId)
                            .FirstOrDefault();

                        if (logEntry != null)
                        {
                            logEntry.Status = woprocessing != null ? "Exported" : logEntry.Status;
                            logEntry.IsExported = true;
                            logEntry.ExportedAt = DateTime.Now;
                            logEntry.ExportedBy = userLoggin ?? "Admin";
                        }
                    }
                    _context.SaveChanges();
                }
                return Ok(new { encodeFileName = Convert.ToBase64String(fileContents), fileDownloadName = newFileName });
            }
            catch (Exception ex)
            {
                Log.Fatal($"ErrorDownloadFile: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        private static byte[] AddDataToExcelFile(byte[] templateBytes, string sheetName, string workOrder, int? checksheetVerId, MplusGwContext _context, string positionCode)
        {
            // Khởi tạo và xử lý ExcelPackage
            using MemoryStream ms = new(templateBytes);
            using ExcelPackage package = new(ms);
            ExcelWorksheet? worksheet = package.Workbook.Worksheets[sheetName];


            if (worksheet == null)
            {
                Log.Fatal("Không tìm thấy Sheet để thêm dữ liệu", "Lỗi thêm dữ liệu");
                throw new InvalidOperationException("Không tìm thấy Sheet để thêm dữ liệu");
            }

            var formDefinitions = _context.TblChecksheetForms
               .Where(x => x.ChecksheetVersionId == checksheetVerId).ToDictionary(f => f.FormId, f => f);

            var formDefIds = _context.TblChecksheetForms
                .Where(x => x.ChecksheetVersionId == checksheetVerId).Select(s => s.FormId).ToList();

            var formJsonFields = _context.TblChecksheetForms
              .Where(x => x.ChecksheetVersionId == checksheetVerId).Select(s => new { s.JsonFormData, s.FormId }).ToList();

            var formMappings = new List<Dictionary<string, object>>();

            foreach (var item in formJsonFields)
            {
                var formSections = JsonConvert.DeserializeObject<List<FormSection>>(item.JsonFormData ?? "");
                if (formSections != null)
                {
                    foreach (var section in formSections)
                    {
                        foreach (var row in section.Rows)
                        {
                            foreach (var col in row.Cols)
                            {
                                foreach (var element in col.Elements)
                                {
                                    var map = new Dictionary<string, object>
                                        {
                                            { "FormId", item.FormId },
                                            { "RowIndex", section.RowCellIndex },
                                            { "FieldName", element.FieldName },
                                            { "ColSpan", element.ColSpan },
                                            { "IsMerged", element.IsMerged },
                                            { "IsTotals", element.IsTotals },
                                            { "StartCell", element.StartCell },
                                            { "RowSpan", element.RowSpan }
                                        };

                                    formMappings.Add(map);
                                }
                            }
                        }
                    }
                }

            }

            var mappingLookup = formMappings
                .Where(m => m.TryGetValue("FormId", out var id) && id is int)
                .ToLookup(m => (int)m["FormId"], m => m);

            var getDataEntries = _context.TblChecksheetFormEntries
                .Where(x => x.ChecksheetVerId == checksheetVerId &&
                x.WorkOrderCode == workOrder &&
                x.PositionCode == positionCode)
                .Select(s => new
                {
                    s.EntryIndex,
                    EntryData = _context.TblChecksheetEntryValues
                    .Where(x => x.FormEntryId == s.FormEntryId)
                    .Select(e => new
                    {
                        e.JsonValue,
                        e.JsonNoteValue,
                    }).ToList()
                })
                .OrderBy(x => x.EntryIndex)
                .ToList();

            if (string.IsNullOrEmpty(positionCode))
            {
                getDataEntries = _context.TblChecksheetFormEntries
                .Where(x => x.ChecksheetVerId == checksheetVerId &&
                x.WorkOrderCode == workOrder)
                .Select(s => new
                {
                    s.EntryIndex,
                    EntryData = _context.TblChecksheetEntryValues
                    .Where(x => x.FormEntryId == s.FormEntryId)
                    .Select(e => new
                    {
                        e.JsonValue,
                        e.JsonNoteValue,
                    }).ToList()
                })
                .OrderBy(x => x.EntryIndex)
                .ToList();
            }


            // 1. Xử lý data json
            var dataJsonFields = new Dictionary<int, List<Dictionary<string, object>>>();
            var dataJsonNoteFields = new List<Dictionary<string, object>>();

            foreach (var entry in getDataEntries)
            {
                var entryValues = entry.EntryData;
                var jsonNoteValue = string.Empty;

                var currentEntryFields = new List<Dictionary<string, object>>();

                foreach (var value in entryValues)
                {
                    jsonNoteValue = value.JsonNoteValue;
                    try
                    {
                        var formDataContainer = JsonConvert.DeserializeObject<FormDataContainer>(value.JsonValue ?? "");

                        if (formDataContainer != null)
                        {
                            foreach (var field in formDataContainer.FormData)
                            {
                                var fieldDict = new Dictionary<string, object>
                                    {
                                        { "formId", field.FormId },
                                        { "fieldName", field.FieldName },
                                        { "value", field.Value },
                                    };
                                currentEntryFields.Add(fieldDict);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Log.Fatal($"Lỗi phân tích JSON: {ex.Message}");
                    }
                }
                dataJsonFields.Add(entry.EntryIndex, currentEntryFields);

                // Xử lý 1 lần json ghi chú khi có dữ liệu
                if (!string.IsNullOrEmpty(jsonNoteValue))
                {
                    try
                    {
                        var formDataContainer = JsonConvert.DeserializeObject<FormDataContainer>(jsonNoteValue);

                        if (formDataContainer != null)
                        {
                            foreach (var field in formDataContainer.FormData)
                            {
                                var fieldDict = new Dictionary<string, object>
                                    {
                                        { "formId", field.FormId },
                                        { "fieldName", field.FieldName },
                                        { "value", field.Value },
                                    };
                                dataJsonNoteFields.Add(fieldDict);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Log.Fatal($"Lỗi phân tích JSON: {ex.Message}");
                    }
                }

            }

            int globalAddedRowsOffset = 0;
            var minEntryIndex = getDataEntries.Any() ? getDataEntries.Min(g => g.EntryIndex) : 0;

            var distinctRepeatableFormRowIndexes = formMappings
                .Where(mapping =>
                    mapping.TryGetValue("FormId", out var formIdObj) &&
                    int.TryParse(formIdObj?.ToString(), out var formId) &&
                    formDefinitions.TryGetValue(formId, out var def) &&
                    def.IsRepeatable == true &&
                    mapping.TryGetValue("RowIndex", out var rowIndexObj) &&
                    int.TryParse(rowIndexObj?.ToString(), out var rowIndex) &&
                    rowIndex > 0)
                .Select(mapping => Convert.ToInt32(mapping["RowIndex"].ToString()))
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            var repeatableBlocks = new List<(int MinRow, int MaxRow, int RowSpan)>();
            if (distinctRepeatableFormRowIndexes.Any())
            {
                int currentMin = distinctRepeatableFormRowIndexes[0];
                int currentMax = distinctRepeatableFormRowIndexes[0];

                for (int i = 1; i < distinctRepeatableFormRowIndexes.Count; i++)
                {
                    if (distinctRepeatableFormRowIndexes[i] == currentMax + 1)
                    {
                        currentMax = distinctRepeatableFormRowIndexes[i];
                    }
                    else
                    {
                        repeatableBlocks.Add((currentMin, currentMax, currentMax - currentMin + 1));
                        currentMin = distinctRepeatableFormRowIndexes[i];
                        currentMax = distinctRepeatableFormRowIndexes[i];
                    }
                }
                repeatableBlocks.Add((currentMin, currentMax, currentMax - currentMin + 1));
            }

            // Thêm dòng theo lần nhập
            foreach (var itemValues in getDataEntries)
            {
                bool isFirstEntry = (itemValues.EntryIndex == minEntryIndex);

                if (!isFirstEntry)
                {
                    // Sắp xếp giảm dần để chèn từ dưới lên tránh làm lệch các hàng đã xử lý
                    foreach (var (MinRow, MaxRow, RowSpan) in repeatableBlocks.OrderByDescending(r => r))
                    {
                        // Vị trí chèn hàng mới
                        int insertionRow = MinRow + globalAddedRowsOffset;
                        worksheet.InsertRow(insertionRow, RowSpan, insertionRow - 1);

                        // Tăng offset vì chúng ta đã thêm một hàng
                        globalAddedRowsOffset += RowSpan;

                        // Đảm bảo chiều cao hàng mới khớp với hàng mẫu
                        for (int i = 0; i < RowSpan; i++)
                        {
                            double previousRowHeight = worksheet.Row(insertionRow - 1 + i).Height;
                            worksheet.Row(insertionRow + i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(insertionRow + i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Row(insertionRow + i).Style.WrapText = true;
                            if (previousRowHeight > 0)
                            {
                                worksheet.Row(insertionRow + i).Height = previousRowHeight;
                            }
                        }
                    }
                }
            }
            if (dataJsonFields != null)
            {
                int maxTargetRow = 0;
                foreach (var entry in getDataEntries)
                {
                    int currentEntryRowOffset = (entry.EntryIndex - minEntryIndex);
                    dataJsonFields.TryGetValue(entry.EntryIndex, out var foundFields);
                    if (foundFields != null)
                    {
                        foreach (var field in foundFields)
                        {
                            //Lấy giá trị của "formId"
                            int formId = Convert.ToInt32(field["formId"].ToString());
                            // Lấy giá trị "fieldName" và "value"
                            string fieldName = field["fieldName"].ToString() ?? "";
                            string value = field["value"].ToString() ?? "";

                            if (formId == 0 || !formDefinitions.TryGetValue(formId, out var formDef)) continue;
                            bool isRepeatable = formDef.IsRepeatable ?? false;

                            var currentFormMappings = mappingLookup[formId];
                            var mappingField = currentFormMappings.FirstOrDefault(x => x.TryGetValue("FieldName", out var name) && name?.ToString() == fieldName);
                            if (mappingField == null) continue;

                            int baseRowIndex = mappingField != null && mappingField.TryGetValue("RowIndex", out var rowIndexObj) && int.TryParse(rowIndexObj?.ToString(), out var rowIndex) && rowIndex > 0 ? rowIndex : -1;
                            int targetRow = baseRowIndex + (isRepeatable ? currentEntryRowOffset * repeatableBlocks.Sum(b => b.RowSpan) : 0);
                            int columnIndex = ColumnLetterToNumber(mappingField?["StartCell"].ToString());

                            string? cellValue = value;
                            if (int.TryParse(cellValue, out int numberValue))
                            {
                                worksheet.Cells[targetRow, columnIndex].Value = numberValue;
                            }
                            else
                            {
                                worksheet.Cells[targetRow, columnIndex].Value = cellValue ?? "";
                            }
                            var cell = worksheet.Cells[targetRow, columnIndex];
                            ApplyCellBorders(cell, string.IsNullOrEmpty(cellValue));

                            bool isMerged = mappingField?.TryGetValue("IsMerged", out var mergedObj) == true && mergedObj is bool b && b;
                            bool isCalcTotal = mappingField?.TryGetValue("IsTotals", out var totalObj) == true && totalObj is bool t && t;

                            int colSpan = mappingField != null && mappingField.TryGetValue("ColSpan", out var colSpanObj) && int.TryParse(colSpanObj?.ToString(), out var colSpanInt) && colSpanInt > 0 ? colSpanInt : -1;
                            if (isMerged && colSpan > 1)
                            {
                                string mergeRange = $"{ColumnNumberToLetter(columnIndex)}{targetRow}:{ColumnNumberToLetter(columnIndex + colSpan - 1)}{targetRow}";
                                worksheet.Cells[mergeRange].Merge = true;
                                worksheet.Cells[mergeRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells[mergeRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            // check field tính tổng và thêm công thức tính tổng vào
                            if (isCalcTotal)
                            {
                                maxTargetRow = Math.Max(maxTargetRow, targetRow);
                                double sum = 0;

                                for (int r = baseRowIndex; r <= maxTargetRow; r++)
                                {
                                    var cellTotal = worksheet.Cells[r, columnIndex];
                                    if (cellTotal.Value != null && double.TryParse(cellTotal.Value.ToString(), out double val))
                                    {
                                        sum += val;
                                    }
                                }
                                int rowTotal = maxTargetRow + 1;
                                worksheet.Cells[rowTotal, columnIndex].Formula = $"SUM({ColumnNumberToLetter(columnIndex)}{baseRowIndex}:{ColumnNumberToLetter(columnIndex)}{maxTargetRow})";
                                
                                if (sum <= 0)
                                {
                                    worksheet.Cells[rowTotal, columnIndex].Value = "";
                                    ApplyCellBorders(worksheet.Cells[rowTotal, columnIndex], true);
                                }
                            }
                            if (isRepeatable && dataJsonNoteFields.Count == 0)
                            {
                                int rowNote = maxTargetRow + 2;
                                cell = worksheet.Cells[rowNote, 2];
                                ApplyCellBorders(cell, true);
                            }
                        }
                    }
                }
            }
            if (dataJsonNoteFields != null)
            {
                foreach (var item in dataJsonNoteFields)
                {
                    //Lấy giá trị của "formId"
                    int formId = Convert.ToInt32(item["formId"].ToString());
                    // Lấy giá trị "fieldName" và "value"
                    string fieldName = item["fieldName"].ToString() ?? "";
                    string value = item["value"].ToString() ?? "";
                    var currentFormMappings = mappingLookup[formId];
                    var mappingField = currentFormMappings.FirstOrDefault(x => x.TryGetValue("FieldName", out var name) && name?.ToString() == fieldName);
                    if (mappingField == null) continue;

                    int baseRowIndex = mappingField != null && mappingField.TryGetValue("RowIndex", out var rowIndexObj) && int.TryParse(rowIndexObj?.ToString(), out var rowIndex) && rowIndex > 0 ? rowIndex : -1;
                    int targetRow = baseRowIndex + globalAddedRowsOffset;
                    int columnIndex = ColumnLetterToNumber(mappingField?["StartCell"].ToString());

                    string? cellValue = value;
                    if (int.TryParse(cellValue, out int numberValue))
                    {
                        worksheet.Cells[targetRow, columnIndex].Value = numberValue;
                    }
                    else
                    {
                        worksheet.Cells[targetRow, columnIndex].Value = cellValue ?? "";
                    }
                    var cell = worksheet.Cells[targetRow, columnIndex];
                    ApplyCellBorders(cell, string.IsNullOrEmpty(cellValue));

                    int colSpan = mappingField != null && mappingField.TryGetValue("ColSpan", out var colSpanObj) && int.TryParse(colSpanObj?.ToString(), out var colSpanInt) && colSpanInt > 0 ? colSpanInt : -1;
                    bool isMerged = mappingField?.TryGetValue("IsMerged", out var mergedObj) == true && mergedObj is bool b && b;

                    if (isMerged && colSpan > 1)
                    {
                        string mergeRange = $"{ColumnNumberToLetter(columnIndex)}{targetRow}:{ColumnNumberToLetter(columnIndex + colSpan - 1)}{targetRow}";
                        worksheet.Cells[mergeRange].Merge = true;
                        worksheet.Cells[mergeRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[mergeRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                }
            }
            worksheet.Protection.SetPassword("123456");
            worksheet.Protection.AllowFormatCells = true;
            worksheet.Protection.AllowFormatColumns = true;
            worksheet.Protection.AllowFormatRows = true;
            package.Save();
            return package.GetAsByteArray();
        }

        // Hàm AddFormulaToExcelCell cần được định nghĩa trong cùng class hoặc accessible scope
        public static void AddFormulaToExcelCell(ExcelWorksheet worksheet, string cellAddress, string formula)
        {
            if (worksheet == null || string.IsNullOrWhiteSpace(cellAddress))
            {
                Console.WriteLine("Worksheet hoặc địa chỉ ô không hợp lệ.");
                return;
            }
            try
            {
                worksheet.Cells[cellAddress].Formula = formula;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm công thức vào ô {cellAddress}: {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Log.Error("Error: {ID}", Activity.Current?.Id);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //protected void CreateFileEx(string newFilePath, FileInfo fileInfo, string? sheetName)
        //{
        //    using ExcelPackage oldFile = new(fileInfo);
        //    using ExcelPackage newFile = new();

        //    ExcelWorksheet oldWorkSheet = oldFile.Workbook.Worksheets[0];
        //    oldWorkSheet.View.ZoomScale = 100;
        //    oldWorkSheet.ConditionalFormatting.RemoveAll();
        //    newFile.Workbook.Worksheets.Add(sheetName);

        //    FileStream objFileStrm = System.IO.File.Create(newFilePath);
        //    objFileStrm.Seek(0, SeekOrigin.Begin);
        //    newFile.SaveAs(objFileStrm);
        //    objFileStrm.Close();
        //    byte[] data = oldFile.GetAsByteArray();
        //    System.IO.File.WriteAllBytes(newFilePath, data);
        //}

        // Hàm hỗ trợ chuyển đổi từ chữ cái cột sang số
        public static int ColumnLetterToNumber(string? columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter)) return 0;

            columnLetter = columnLetter.ToUpperInvariant();
            int sum = 0;
            for (int i = 0; i < columnLetter.Length; i++)
            {
                sum *= 26;
                sum += (columnLetter[i] - 'A' + 1);
            }
            return sum;
        }

        // Hàm hỗ trợ chuyển đổi từ số cột sang chữ cái
        public static string ColumnNumberToLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        // Helper method to apply cell borders
        private static void ApplyCellBorders(ExcelRange cell, bool isEmpty)
        {
            var thinBorderStyle = ExcelBorderStyle.Thin; // Ensure this is accessible or passed as a parameter

            cell.Style.Border.Bottom.Style = thinBorderStyle;
            cell.Style.Border.Right.Style = thinBorderStyle;
            cell.Style.Border.Top.Style = thinBorderStyle;
            cell.Style.Border.Left.Style = thinBorderStyle;

            if (isEmpty)
            {
                cell.Style.Border.Diagonal.Style = thinBorderStyle;
                cell.Style.Border.DiagonalDown = true;
            }
            else
            {
                cell.Style.Border.Diagonal.Style = ExcelBorderStyle.None;
                cell.Style.Border.DiagonalDown = false;
            }
        }
    }

    public class RequestRenderForm
    {
        public int TrayId { get; set; } = 0;
        public string TrayCode { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public string WorkOrder { get; set; } = string.Empty;
        public int EntryIndex { get; set; } = 0;
    }

    public class RequestDataDownload
    {
        public int ChecksheetVerId { get; set; }
        public string WorkOrder { get; set; }
        public string FilePath { get; set; }
        public string SheetName { get; set; }
        public string PositionCode { get; set; }
    }
    public class DataDetailWithField
    {
        public string? FieldName { get; set; }
        public string? Value { get; set; }
        public string? BatchNo { get; set; }
        public int? IdSessions { get; set; }
        public int? IdForm { get; set; }
        public string? FormType { get; set; }
    }

    public class RenderProductions
    {
        public int? IdProduction { get; set; }
        public string? WorkOrder { get; set; }
        public string? ProductCode { get; set; }
        public string? LotNo { get; set; }
        public string? ProductName { get; set; }
        public string? StatusProduction { get; set; }
        public bool? IsStopped { get; set; }
    }

    public class ListPouchesRender
    {
        public int? PouchId { get; set; }
        public int? PouchNo { get; set; }
        public int? QtyPouch { get; set; }
        public double? DiameterPouch { get; set; }
    }

    public class ListErrorsRender
    {
        public int? TrayId { get; set; }
        public int? ErrorId { get; set; }
        public int? QtyError { get; set; }
    }
    public class HtmlRender
    {
        public string? SectionId { get; set; }
        public string? FieldName { get; set; }
        public string? Label { get; set; }
        public string? TypeInput { get; set; }
        public string? ColClass { get; set; }
        public bool? IsHidden { get; set; }
        public string? ElementId { get; set; }
        public string? Value { get; set; }
    }

    public class FormDataEntry
    {
        public string FormId { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
    }

    public class FormDataContainer
    {
        public List<FormDataEntry> FormData { get; set; }
    }


    public class FormSection
    {
        public string SectionId { get; set; }
        public string SectionClass { get; set; }
        public int SectionIndex { get; set; }
        public string RowCellIndex { get; set; }
        public List<Row> Rows { get; set; }
    }

    public class Row
    {
        public string RowClass { get; set; }
        public int RowIndex { get; set; }
        public List<Col> Cols { get; set; }
    }

    public class Col
    {
        public string ColClass { get; set; }
        public int ColIndex { get; set; }
        public List<Element> Elements { get; set; }
    }

    public class Element
    {
        public string FieldName { get; set; }
        public string Label { get; set; }
        public string StartCell { get; set; }
        public string CustomClass { get; set; }
        public string TypeInput { get; set; }
        public int RowSpan { get; set; }
        public int ColSpan { get; set; }
        public bool IsMerged { get; set; }
        public string TypeElement { get; set; }
        public string ElementId { get; set; }
        public string DataSource { get; set; }
        public bool IsTotals { get; set; }
        public List<object> Conditions { get; set; }
        public List<object> Events { get; set; }
        public int TabIndex { get; set; }
    }

}
