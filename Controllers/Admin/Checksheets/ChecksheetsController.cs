using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.Linq;

namespace MPLUS_GW_WebCore.Controllers.Admin.Checksheets
{
    public class ChecksheetsController : Controller
    {
        private readonly MplusGwContext _context;
        public ChecksheetsController(MplusGwContext context)
        {
            _context = context;
        }
        [Route("/admin/checksheets")]
        public async Task<IActionResult> Index()
        {
            var listChecksheets = await _context.TblChecksheetVersions
                .Select(s => new RenderChecksheetVersion
                {
                    ChecksheetVersionId = s.ChecksheetVersionId,
                    ChecksheetId = s.ChecksheetId,
                    VersionChecksheet = s.VersionNumber,
                    Statusname = s.Statusname,
                    CreatedTime = s.CreatedAt,
                    EffectiveDate = s.EffectiveDate,
                    ExpiryDate = s.ExpiryDate,
                    ChecksheetCode = _context.TblChecksheetsUploads.Where(x => x.ChecksheetId == s.ChecksheetId).Select(c => c.ChecksheetCode).FirstOrDefault(),
                    ChecksheetName = s.FileName,
                }).ToListAsync();
            ViewData["ListChecksheets"] = listChecksheets;
            return View();
        }

        [Route("/admin/checksheets/them")]
        public IActionResult Add()
        {
            var getAllLocations = _context.TblLocations
                .Select(x => new TblLocation
                {
                    IdLocation = x.IdLocation,
                    LocationCode = x.LocationCode,
                    LocationName = x.LocationName,
                }).ToList();
            var getAllLocationChilds = _context.TblLocationCs
                .Select(x => new TblLocationC
                {
                    Id = x.Id,
                    LocationCodeC = x.LocationCodeC,
                    LocationNameC = x.LocationNameC
                })
                .ToList();
            ViewData["AllLocations"] = getAllLocations;
            ViewData["ListAllChilds"] = getAllLocationChilds;
            return View();
        }

        [Route("/admin/checksheets/update/{id}")]
        public IActionResult Update(int id)
        {
            var getAllLocationChilds = _context.TblLocationCs
                .Select(x => new TblLocationC
                {
                    Id = x.Id,
                    LocationCodeC = x.LocationCodeC,
                    LocationNameC = x.LocationNameC
                })
                .ToList();
            ViewData["ListAllChilds"] = getAllLocationChilds;
            ViewData["ChecksheetVerId"] = id;
            return View();
        }

        [HttpPost]
        public IActionResult GetLocationChild([FromBody] RequestLocationChild requestLocationChild)
        {
            if (requestLocationChild == null)
            {
                return BadRequest(new { message = "Not Found!" });
            }
            var listLocationChild = _context.TblLocationCs
                .Where(x => x.IdChaC == requestLocationChild.IdLocation)
                .ToList();
            return Ok(new { dataRender = listLocationChild });
        }


        [HttpPost]
        public IActionResult GetInfoByPosition([FromBody] RequestLocationChild requestLocationChild)
        {
            if (requestLocationChild == null)
            {
                return BadRequest(new { message = "Dữ liệu gửi lên không tìm thấy. Vui lòng kiểm tra lại" });
            }
            int locationChildId = requestLocationChild.LocationChildCode;
            var locationChildCode = _context.TblLocationCs
                .Where(x => x.Id == locationChildId)
                .Select(s => s.LocationCodeC).FirstOrDefault();
            var getLineByChildId = _context.TblMasterPositions
                .Where(x => x.LocationChildId == locationChildId)
                .Select(s => new
                {
                    s.IdLine,
                    s.PositionCode,
                    s.IdPosition,
                    s.LocationChildId,
                    LineName = _context.TblProdLines.Where(x => x.IdLine == s.IdLine).Select(l => l.LineName).FirstOrDefault(),
                }).ToList();
            return Ok(new { lines = getLineByChildId });
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel([FromForm] IFormFile file, string checksheetCode, int locationId, int locationChildId, string lineChecksheet, bool isChange, string checksheetType)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest(new { message = "Not Found JSON Passing" });
            }
            try
            {
                string displayName = "Admin";
                if (HttpContext.Session.GetString("DisplayName") != null)
                {
                    displayName = HttpContext.Session.GetString("DisplayName")?.ToString() ?? "Admin";
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "templates", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var fileInfo = new FileInfo(filePath);

                using (var package = new ExcelPackage(fileInfo))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.Contains("JCQ"));
                    if (worksheet == null)
                    {
                        return NotFound("Worksheet containing 'JCQ' not found in the Excel file.");
                    }
                    // Tạo và update checksheets
                    TblChecksheetsUpload targetChecksheets;
                    var checksheet = await _context.TblChecksheetsUploads
                        .Include(cs => cs.TblChecksheetVersions)
                        .FirstOrDefaultAsync(cs => cs.ChecksheetCode == checksheetCode);
                    if (checksheet == null)
                    {
                        targetChecksheets = new TblChecksheetsUpload
                        {
                            ChecksheetCode = checksheetCode,
                            CreatedAt = DateTime.Now,
                            IdLocation = locationId,
                        };
                        _context.TblChecksheetsUploads.Add(targetChecksheets);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        targetChecksheets = checksheet;
                        checksheet.CreatedAt = DateTime.Now;
                    }

                    // Thêm version mới cho checksheet
                    decimal newVersionNum = 1.0m;
                    if (targetChecksheets.TblChecksheetVersions != null && targetChecksheets.TblChecksheetVersions.Any())
                    {
                        newVersionNum = targetChecksheets.TblChecksheetVersions.Max(v => v.VersionNumber) + 1.0m;
                    }

                    string? positionWorkCode;

                    if (locationChildId == 0)
                    {
                        positionWorkCode = string.Join(", ", await _context.TblLocationCs
                            .Where(x => x.IdChaC == locationId)
                            .Select(s => s.LocationCodeC)
                            .ToArrayAsync()).ToString();
                    }
                    else
                    {
                        positionWorkCode = await _context.TblLocationCs
                            .Where(x => x.Id == locationChildId)
                            .Select(s => s.LocationCodeC)
                            .FirstOrDefaultAsync();
                    }

                    var newVersionCS = new TblChecksheetVersion
                    {
                        ChecksheetId = targetChecksheets.ChecksheetId,
                        VersionNumber = newVersionNum,
                        CreatedBy = displayName,
                        CreatedAt = DateTime.Now,
                        Statusname = "Draft",
                        IsChangeForm = isChange,
                        EffectiveDate = DateTime.Now.Date,
                        ExpiryDate = DateTime.MaxValue,
                        PositionWorkingCode = positionWorkCode,
                        FilePath = file.FileName,
                        FileName = Path.GetFileNameWithoutExtension(fileInfo.Name),
                        SheetName = worksheet.Name,
                        ChecksheetType = checksheetType
                    };

                    _context.TblChecksheetVersions.Add(newVersionCS);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                    {
                        Console.WriteLine($"DbUpdateException (ChecksheetVersion): {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }
                        return StatusCode(500, $"Lỗi khi lưu thông tin Phiên bản Checksheet: {ex.InnerException?.Message ?? ex.Message}");
                    }

                    targetChecksheets.CurrentVersionId = newVersionCS.ChecksheetVersionId;

                    // Extend thêm vào các bảng chi tiết khác
                    List<int>? lines = JsonConvert.DeserializeObject<List<int>>(lineChecksheet) ?? await _context.TblProdLines.Select(s => s.IdLine).ToListAsync();
                    if(lines.Any())
                    {
                        var relevantMasterPositions = new Dictionary<int, List<int>>();
                        if (locationChildId == 0)
                        {
                            // Fetch all positions for the main location and selected lines
                            relevantMasterPositions = await _context.TblMasterPositions
                                .Where(x => x.IdLocation == locationId && lines.Contains(x.IdLine ?? 0))
                                .GroupBy(x => x.IdLine ?? 0)
                                .ToDictionaryAsync(
                                    x => x.Key, 
                                    x => x.Select(s => s.IdPosition).ToList()
                                );
                        }
                        else
                        {
                            // Fetch positions for the specific child location and selected lines
                            relevantMasterPositions = await _context.TblMasterPositions
                                .Where(x => x.LocationChildId == locationChildId && lines.Contains(x.IdLine ?? 0))
                                 .GroupBy(x => x.IdLine ?? 0)
                                .ToDictionaryAsync(
                                    x => x.Key,
                                    x => x.Select(s => s.IdPosition).ToList()
                                );
                        }
                        var existingAssignments = _context.TblChecksheetWorkstationAssignments
                            .Where(x => x.ChecksheetId == targetChecksheets.ChecksheetId)
                            .ToDictionary(x => x.WorkstationId);

                        var assignmentsToAdd = new List<TblChecksheetWorkstationAssignment>();
                        var assignmentsToUpdate = new List<TblChecksheetWorkstationAssignment>();

                        foreach (var line in lines)
                        {
                            if (relevantMasterPositions.TryGetValue(line, out List<int>? workstationIdsForLine) && workstationIdsForLine.Any())
                            {
                                foreach (var getWorkStationId in workstationIdsForLine)
                                {
                                    if (getWorkStationId != 0)
                                    {
                                        if(existingAssignments.TryGetValue(getWorkStationId, out var existingAssignment))
                                        {
                                            if (existingAssignment.LastUsedChecksheetVersionId != newVersionCS.ChecksheetVersionId)
                                            {
                                                existingAssignment.LastUsedChecksheetVersionId = newVersionCS.ChecksheetVersionId;
                                                existingAssignment.AssignmentDate = DateTime.Now;
                                                assignmentsToUpdate.Add(existingAssignment);
                                            }
                                        }
                                        else
                                        {
                                            var workstationAssignment = new TblChecksheetWorkstationAssignment
                                            {
                                                WorkstationId = getWorkStationId,
                                                ChecksheetId = targetChecksheets.ChecksheetId,
                                                LastUsedChecksheetVersionId = newVersionCS.ChecksheetVersionId,
                                                AssignmentDate = DateTime.Now,
                                                IsChecksheetCondition = checksheetType == "CHECKSHEET_CONDITIONS",
                                            };
                                            assignmentsToAdd.Add(workstationAssignment);
                                        }
                                       
                                    }
                                }
                            }
                        }

                        if (assignmentsToAdd.Any())
                        {
                            _context.TblChecksheetWorkstationAssignments.AddRange(assignmentsToAdd);
                        }
                        if (assignmentsToUpdate.Any())
                        {
                            _context.TblChecksheetWorkstationAssignments.UpdateRange(assignmentsToUpdate);
                        }
                    }
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                    {
                        Console.WriteLine($"DbUpdateException (ChecksheetVersion): {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }
                        return StatusCode(500, $"Lỗi khi lưu thông tin Phiên bản Checksheet: {ex.InnerException?.Message ?? ex.Message}");
                    }
                    // Clone form nhập dữ liệu
                    var checkForms = await _context.TblChecksheetForms
                        .Where(x => x.ChecksheetVersionId == newVersionCS.ChecksheetVersionId)
                        .ToListAsync();
                    if(checkForms.Any())
                    {
                        var newVerChecksheetId = new SqlParameter("@NewVersionChecksheetId", newVersionCS.ChecksheetVersionId);
                        await _context.Database.ExecuteSqlRawAsync("EXEC CloneFormsWhenNewVersion @NewVersionChecksheetId", newVerChecksheetId);
                    }
                }
                return Ok(new { message = "Thêm thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVersionChecksheet([FromBody] RequestUpdateChecksheets requestUpdateChecksheets)
        {
            if(requestUpdateChecksheets == null)
            {
                return BadRequest(new { message = "Yêu cầu gửi lên không có dữ liệu. Vui lòng kiểm tra lại!" });
            }
            int checksheetVerId = requestUpdateChecksheets.ChecksheetVerId;
            string dateTimeEffective = requestUpdateChecksheets.DateTimeEffective;
            string notes = requestUpdateChecksheets.Note;
            string dateFormat = "dd/MM/yyyy HH:mm";
            try
            {
                DateTime.TryParseExact(dateTimeEffective, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeEff);

                var checksheetVersion = await _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == checksheetVerId)
                    .FirstOrDefaultAsync();

                var checksheetId = await _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == checksheetVerId)
                    .Select(x => x.ChecksheetId)
                    .FirstOrDefaultAsync();

                if (checksheetVersion != null)
                {
                    checksheetVersion.EffectiveDate = dateTimeEff;
                    checksheetVersion.ApprovalDate = DateTime.Now;
                    checksheetVersion.ApprovedBy = "Admin";
                    checksheetVersion.Statusname = "Approved";
                    checksheetVersion.Notes = notes;
                    await _context.SaveChangesAsync();

                    var lastVersion = await _context.TblChecksheetVersions
                        .Where(x => x.ChecksheetId == checksheetId &&
                                    x.ChecksheetVersionId != checksheetVerId)
                        .OrderByDescending(x => x.VersionNumber) // đảm bảo lấy version cao nhất
                        .FirstOrDefaultAsync();

                    if (lastVersion != null)
                    {
                        lastVersion.ExpiryDate = dateTimeEff;
                        lastVersion.Statusname = "Expired";
                        await _context.SaveChangesAsync();
                    }

                    if(requestUpdateChecksheets.InfoProducts != null)
                    {
                        List<InfoProductsVer>? infoProducsWithVer = JsonConvert.DeserializeObject<List<InfoProductsVer>>(requestUpdateChecksheets.InfoProducts) ?? new List<InfoProductsVer>();
                        foreach (var item in infoProducsWithVer)
                        {
                            var checksheetItemForVer = new TblChecksheetItemAssignment
                            {
                                ChecksheetId = checksheetId,
                                LastUsedChecksheetVersionId = checksheetVerId,
                                IsLocked = true,
                                AssignmentDate = dateTimeEff,
                                ProductItem = item.ProductCode,
                                ProductLot = item.ProductLot,
                                IsChecksheetCondition = checksheetVersion.ChecksheetType == "CHECKSHEET_CONDITIONS",
                            };
                            _context.TblChecksheetItemAssignments.Add(checksheetItemForVer);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(new { message = "Cập nhật thành công!" });
            } catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RenderChecksheetVersion
    {
        public int? ChecksheetVersionId { get; set; }
        public int? ChecksheetId { get; set; }
        public string? ChecksheetCode { get; set; }
        public string? ChecksheetName { get; set; }
        public decimal? VersionChecksheet { get; set; }
        public string? Statusname { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class RequestLocationChild
    {
        public int LocationChildCode { get; set; }
        public int IdLocation { get; set; }
    }

    public class RequestChecksheetVers
    {
        public string ChecksheetVerId { get; set; }
    }

    public class RequestUpdateChecksheets
    {
        public int ChecksheetVerId { get; set; }
        public string DateTimeEffective { get; set; }
        public string Note { get; set; }
        public string InfoProducts { get; set; }
    }

    public class InfoProductsVer
    {
        public string ProductCode { get; set; }
        public string ProductLot { get; set; }
    }
}
