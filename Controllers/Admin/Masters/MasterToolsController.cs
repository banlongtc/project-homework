using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using System.Data;
using System.Reflection.Metadata;

namespace MPLUS_GW_WebCore.Controllers.Admin.Masters
{
    public class MasterToolsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly ExcelData _excelData;
        public MasterToolsController(MplusGwContext context, ExcelData excelData)
        {
            _context = context;
            _excelData = excelData;
        }

        [Route("/admin/master-dung-cu")]
        public async Task<IActionResult> Index()
        {
            var listAllItems = await (from s in _context.TblMasterTools
                                      select new TblMasterTool()
                                      {
                                          Id = s.Id,
                                          ToolCode = s.ToolCode,
                                          ToolName = s.ToolName,
                                          Descriptions = s.Descriptions,
                                      }).ToListAsync().ConfigureAwait(false);
            ViewData["AllItems"] = listAllItems;
            return View();
        }

        [Route("/admin/master-dung-cu/them")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCSV([FromForm] IFormFile file)
        {
            if (file == null || file.Length <= 0)
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
                var dataTable = _excelData.ReadExcel(filePath, "MasterTools");
                UpdateToDatabase(dataTable);
                return Ok(new { message = "Tải lên thành công", href = Url.Action("Index", "MasterTools") });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        public void UpdateToDatabase(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row != null)
                {
                    string? toolCode = row["ToolCode"] != null ? row["ToolCode"].ToString() : "";
                    string? toolName = row["ToolName"] != null ? row["ToolName"].ToString() : "";
                    string? descriptions = row["Descriptions"] != null ? row["Descriptions"].ToString() : "";
                    var newItem = new TblMasterTool
                    {
                        ToolCode = toolCode,
                        ToolName = toolName,
                        Descriptions = descriptions,
                        ModifyUpdate = DateTime.Now,
                    };

                    var existingItem = _context.TblMasterTools.
                        SingleOrDefault(x => x.ToolCode == toolCode);
                    if (existingItem == null)
                    {
                        _context.TblMasterTools.Add(newItem);
                    }
                    else
                    {
                        existingItem.ToolCode = toolCode;
                        existingItem.ToolName = toolName;
                        existingItem.Descriptions = descriptions;
                        existingItem.ModifyUpdate = DateTime.Now;
                    }
                }
            }
            _context.SaveChanges();
        }
    }
}
