using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using System.Text;

namespace MPLUS_GW_WebCore.Controllers.Admin.UserManagement
{
    public class UserManagementController : Controller
    {
        private readonly MplusGwContext _context;
        public readonly IWebHostEnvironment _environment;
        public UserManagementController(MplusGwContext context, IWebHostEnvironment hostEnvironment) 
        {
            _context = context;
            _environment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [Route("/admin/tai-khoan")]
        public async Task<IActionResult> Index()
        {
            var getAllUsers = await (from s in _context.TblUsers
                                     select s).ToListAsync();
            var listUsers = new List<GetAllUsers>();
            if(getAllUsers.Count > 0)
            {
                foreach (var user in getAllUsers)
                {
                    string locationName = string.Empty;
                    string sectionName = string.Empty;
                    if (user.IdLocation != null || user.IdSection != null)
                    {
                        var getLocationName = _context.TblLocations.Where(x => x.IdLocation == user.IdLocation).FirstOrDefault();
                        var getSectionName = _context.TblSections.Where(x => x.IdSection == user.IdSection).FirstOrDefault();
                        if (getLocationName != null)
                        {
                            locationName = getLocationName.LocationName ?? "".ToString();
                        }
                        if (getSectionName != null)
                        {
                            sectionName = getSectionName.SectionName ?? "".ToString();
                        }
                    }
                    string roleName = string.Empty;
                    var getRoleName = await (from s in _context.TblUserRoles
                                             join u in _context.TblUsers on s.IdUser equals u.IdUser
                                             join r in _context.TblRoles on s.IdRole equals r.IdRole
                                             where s.IdUser == user.IdUser
                                             select new
                                             {
                                                 RoleName = r.RoleName ?? "".ToString(),
                                             }).ToListAsync();
                    if (roleName != null) { 
                        roleName = string.Join(",", getRoleName.Select(x => x.RoleName).ToArray());
                    }
                    var allUsers = new GetAllUsers
                    {
                        IdUser = user.IdUser,
                        UserName = user.Username,
                        FullName = user.DisplayName,
                        EmployeeNo = user.EmployeeNo,
                        Email = user.Email,
                        Role = roleName,
                        Location = locationName,
                        Section = sectionName,
                        Status = user.ActiveUser.ToString(),
                        Deactivation = user.DeactivationUser.ToString(),
                    };
                    listUsers.Add(allUsers);
                }
            }
            ViewData["ListUsers"] = listUsers;
            return View();
        }

        [Route("/admin/tai-khoan/them")]
        public async Task<IActionResult> AddUser() {
            var listAllRole = await (from s in _context.TblRoles
                                     select new TblRole()
                                     {
                                         IdRole = s.IdRole,
                                         RoleName = s.RoleName,
                                     }).ToListAsync().ConfigureAwait(false);
            var listAllSection = await (from s in _context.TblSections
                                        select new TblSection()
                                        {
                                            IdSection = s.IdSection,
                                            SectionName = s.SectionName,
                                        }).ToListAsync().ConfigureAwait(false);
            var listAllProcess = await (from s in _context.TblLocations
                                        select new TblLocation()
                                        {
                                            IdLocation = s.IdLocation,
                                            LocationName = s.LocationName,
                                            LocationCode = s.LocationCode,
                                        }).ToListAsync().ConfigureAwait(false);
            ViewData["RoleList"] = listAllRole;
            ViewData["SectionList"] = listAllSection;
            ViewData["ProcessList"] = listAllProcess;
            return View();
        }

        [Route("/admin/tai-khoan/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var getUserById = await _context.TblUsers.FindAsync(id);
            var listAllRole = await (from s in _context.TblRoles
                                     select new TblRole()
                                     {
                                         IdRole = s.IdRole,
                                         RoleName = s.RoleName,
                                     }).ToListAsync().ConfigureAwait(false);
            var listAllSection = await (from s in _context.TblSections
                                        select new TblSection()
                                        {
                                            IdSection = s.IdSection,
                                            SectionName = s.SectionName,
                                        }).ToListAsync().ConfigureAwait(false);
            var listAllProcess = await (from s in _context.TblLocations
                                        select new TblLocation()
                                        {
                                            IdLocation = s.IdLocation,
                                            LocationName = s.LocationName,
                                            LocationCode = s.LocationCode,
                                        }).ToListAsync().ConfigureAwait(false);
            ViewData["RoleList"] = listAllRole;
            ViewData["SectionList"] = listAllSection;
            ViewData["ProcessList"] = listAllProcess;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewUser([FromBody]NewUser newUser)
        {
            if (newUser == null) {
                return BadRequest("Not Found");
            }
            var existsUser = await _context.TblUsers.Where(x => x.Username == newUser.Username).FirstOrDefaultAsync();
            if (existsUser == null)
            {
                var usernameParam = new SqlParameter { 
                    ParameterName = "@UserName",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 50,
                    Value = newUser.Username
                };
                var passwordParam = new SqlParameter
                {
                    ParameterName = "@Password",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 50,
                    Value = newUser.PasswordHash
                };
                var displayNameParam = new SqlParameter {
                    ParameterName = "@DisplayName",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 200,
                    Value = newUser.FullName
                };
                var employeeParam = new SqlParameter
                {
                    ParameterName = "@EmployeeNo",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 50,
                    Value = newUser.EmployeeNo
                };
                var emailParam = new SqlParameter
                {
                    ParameterName = "@Email",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 50,
                    Value = newUser.Email
                };
                var idSection = new SqlParameter("@IdSection", newUser.SectionID);
                var idLocation = new SqlParameter("@IdLocation", newUser.LocationID);
                var idRole = new SqlParameter("@IdRole", newUser.RoleID);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CreateUser @UserName, @Password, @DisplayName, @EmployeeNo, @Email, @IdSection, @IdLocation, @IdRole",
                    usernameParam, passwordParam, displayNameParam, employeeParam, emailParam, idSection, idLocation, idRole);
                return Ok(new { message = "Thêm thành công" });
            }
            else { 
                return BadRequest(new {message = $"Đã tồn tại tài khoản này: {existsUser.Username}. Vui lòng thử lại!" });
            }
            
        }

    }

    public class NewUser
    {
        public string Email { get; set; }
        public string EmployeeNo { get; set; }
        public string FullName { get; set; }
        public string LocationID { get; set; }
        public string PasswordHash { get; set; }
        public string RoleID { get; set; }
        public string SectionID { get; set; }
        public string Username { get; set; }
    }
}