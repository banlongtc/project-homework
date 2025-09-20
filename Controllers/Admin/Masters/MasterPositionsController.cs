using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using System.Data;

namespace MPLUS_GW_WebCore.Controllers.Admin.Masters
{
    public class MasterPositionsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly ExcelData _excelData;
        public MasterPositionsController(MplusGwContext context, ExcelData excelData) 
        { 
            _context = context;
            _excelData = excelData;
        }

        [Route("/admin/master-vi-tri")]
        public async Task<IActionResult> Index()
        {
            var listAllItems = await _context.TblMasterPositions
                .Select(s => new AllPositions
                {
                    IdPosition = s.IdPosition,
                    PositionCode = s.PositionCode,
                    PositionName = s.PositionName,
                    Descriptions = s.PositionDes,
                    LocationName = _context.TblLocations.Where(x => x.IdLocation == s.IdLocation).Select(x => x.LocationName).FirstOrDefault(),
                }).ToListAsync().ConfigureAwait(false);
            ViewData["AllPositions"] = listAllItems;
            return View();
        }

        [Route("/admin/master-vi-tri/them")]
        public async Task<IActionResult> Add()
        {
            var getAllLocations = await _context.TblLocations.ToListAsync().ConfigureAwait(false);
            var getAllLines = await _context.TblProdLines.ToListAsync().ConfigureAwait(false);
            ViewData["AllLocations"] = getAllLocations;
            ViewData["AllProLines"] = getAllLines;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCSV([FromForm] IFormFile file)
        {
            if(file == null || file.Length <= 0)
            {
                return BadRequest(new { message = "Not Found File Upload" });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(400);
            }
            try
            {
                var dataTable = _excelData.ReadExcel(filePath, "MasterOperationPosition");
                UpdateToDatabase(dataTable);
                return Json("Tải lên thành công");

            }
            catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        public void UpdateToDatabase(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row != null)
                {
                    string? positionCode = row["PositionCode"] != null ? row["PositionCode"].ToString() : "";
                    string? positionName = row["PositionName"] != null ? row["PositionName"].ToString() : "";
                    string? locationName = row["Process"] != null ? row["Process"].ToString() : "";
                    string? descriptions = row["Descriptions"] != null ? row["Descriptions"].ToString() : "";
                    var newItem = new TblMasterPosition
                    {
                        PositionCode = positionCode,
                        PositionName = positionName,
                        PositionDes = descriptions,
                        IdLocation = _context.TblLocations.Where(x => x.LocationName == locationName).Select(x => x.IdLocation).FirstOrDefault(),
                    };

                    var existingItem = _context.TblMasterPositions.
                        SingleOrDefault(x => x.PositionCode == positionCode);
                    if (existingItem == null)
                    {
                        _context.TblMasterPositions.Add(newItem);
                    } else
                    {
                        existingItem.PositionCode = positionCode;
                        existingItem.PositionName = positionName;
                        existingItem.PositionDes = descriptions;
                        existingItem.IdLocation = _context.TblLocations.Where(x => x.LocationName == locationName).Select(x => x.IdLocation).FirstOrDefault();
                    }
                }
            }
            _context.SaveChanges();
        }
    }

    public class AllPositions
    {
        public int IdPosition { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public string? Descriptions { get; set; }
        public string? LocationName { get; set; }
        public string? LineName { get; set; }
    }

    public class RequestJson
    { 
        public string file { get; set; }
    }
}
