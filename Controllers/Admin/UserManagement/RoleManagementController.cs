using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;

namespace MPLUS_GW_WebCore.Controllers.Admin.UserManagement
{
    public class RoleManagementController : Controller
    {
        private readonly MplusGwContext _context;
        public RoleManagementController(MplusGwContext context) { 
            _context = context;
        }

        [Route("/admin/tai-khoan/quyen-try-cap")]
        public async Task<IActionResult> Index()
        {
            var listAllRole = await (from s in _context.TblRoles
                                     select new TblRole()
                                     {
                                         IdRole = s.IdRole,
                                         RoleCode = s.RoleCode,
                                         RoleDescriptions = s.RoleDescriptions,
                                         RoleName = s.RoleName,
                                     }).ToListAsync().ConfigureAwait(false);
            ViewData["RoleList"] = listAllRole;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]DataRoleAdd dataRoleAdd)
        {
            if (dataRoleAdd == null) {
                return BadRequest("Not Found!");
            }
            bool checkAdd = true;
            var existRole = await _context.TblRoles.Where(x => x.RoleCode == dataRoleAdd.Code).FirstOrDefaultAsync();
            if (existRole == null)
            {
                var newRole = new TblRole
                {
                    RoleCode = dataRoleAdd.Code,
                    RoleName = dataRoleAdd.Name,
                    RoleDescriptions = dataRoleAdd.Descriptions,
                };
                _context.TblRoles.Add(newRole);
                checkAdd = true;
            }
            else
            {
                checkAdd = false;
            }
            if (checkAdd) {
                _context.SaveChanges();
                return Ok(new { message = "Thêm thành công" });
            } else
            {
                return Ok(new { message = "Đã có role này trước đó. Vui lòng kiểm tra lại!" });
            }
            
        }
    }
    public class DataRoleAdd
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Descriptions { get; set; }
    }
}
