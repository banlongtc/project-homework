using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using MPLUS_GW_WebCore.Models;
using Org.BouncyCastle.Crypto.Digests;
using System.Text;
namespace MPLUS_GW_WebCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly MplusGwContext _context;
        public AccountController(MplusGwContext context) 
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] InfoLogin data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Not Found JSON data passed" });
            }
            string passwordString = DecodePassword(data.Password);
            var usernameParam = new SqlParameter("@UserName", data.Username);
            var passwordParam = new SqlParameter("@Password", passwordString);
            var isFirstLoginParam = new SqlParameter
            {
                ParameterName = "@IsFirstLogin",
                SqlDbType = System.Data.SqlDbType.Bit,
                Direction = System.Data.ParameterDirection.Output,
            };
            var mustChangePassword = new SqlParameter
            {
                ParameterName = "@MustChangePassword",
                SqlDbType = System.Data.SqlDbType.Bit,
                Direction = System.Data.ParameterDirection.Output,
            };
            var userId = new SqlParameter
            {
                ParameterName = "@UserId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            var message = new SqlParameter
            {
                ParameterName = "@MessageOutput",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 500,
                Direction = System.Data.ParameterDirection.Output,
            };
            var status = new SqlParameter
            {
                ParameterName = "@Status",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Size = 10,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC LoginUser @UserName, @Password, @IsFirstLogin OUTPUT, @MustChangePassword OUTPUT, @UserId OUTPUT, @MessageOutput OUTPUT, @Status OUTPUT",
                usernameParam, passwordParam, isFirstLoginParam, mustChangePassword, userId, message, status);
            string? statusLogin = status.Value.ToString();
            var getRollName = await (from s in _context.TblUserRoles
                                     join r in _context.TblRoles on s.IdRole equals r.IdRole
                                     join u in _context.TblUsers on s.IdUser equals u.IdUser
                                     where s.IdUser == (int)userId.Value
                                     select new
                                     {
                                         UserID = s.IdUser,
                                         SRoleName = r.RoleName,
                                         SDisplayName = u.DisplayName,
                                         SIdLocation = u.IdLocation,
                                         SEmployee = u.EmployeeNo,
                                     }).ToListAsync();

            var groupRoleById = getRollName.GroupBy(x => x.UserID)
                .Select(x => new
                {
                    UserID = x.Key,
                    RoleName = x.Select(s => s.SRoleName).ToArray(),
                    EmployeeNo = x.First().SEmployee,
                    DisplayName = x.First().SDisplayName,
                    IdLocation = x.First().SIdLocation,
                }).ToList();

            if (statusLogin == "1")
            {
                HttpContext.Session.SetString("User ID", userId.Value.ToString() ?? "");
                if (groupRoleById != null)
                {
                    foreach (var role in groupRoleById)
                    {
                        string displayNamme = role.DisplayName + " " + role.EmployeeNo;
                        var getLocationName = GetLocationUser(role.IdLocation != null ? (int)role.IdLocation : 0);
                        HttpContext.Session.SetString("DisplayName", displayNamme);
                        HttpContext.Session.SetString("RoleName", string.Join(",", role.RoleName));
                    }
                }
            }
            string redirectUrl = string.Empty;
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName != null)
            {
                if (roleName.Contains("Admin") || roleName.Contains("Super Admin"))
                {
                    redirectUrl = Url.Action("Index", "Admin") ?? "".ToString();
                }
                else
                {
                    redirectUrl = Url.Action("Index", "Home") ?? "".ToString();
                }
            }
            return Ok(new { 
                status = statusLogin, 
                message = message.Value,
                redirectUrl = redirectUrl.ToString(),
                firstLogin = isFirstLoginParam.Value,
                changePassword = mustChangePassword.Value,
                userId = userId.Value,
            });
        }

        public async Task<IActionResult> ChangePassword([FromBody] NewPassword newPassword)
        {
            if (newPassword == null) {
                return BadRequest(new { message = "Not Found" });
            }

            string passwordDecode = DecodePassword(newPassword.PasswordUser);
            var userIdParam = new SqlParameter("@UserId", newPassword.IdUser);
            var newPasswordParam = new SqlParameter("@NewPassword", passwordDecode);
            await _context.Database.ExecuteSqlRawAsync("EXEC ChangePassword @UserId, @NewPassword", userIdParam, newPasswordParam);
            return Ok(new { message = "Cập nhật mật khẩu thành công. Đăng nhập lại để bắt đầu làm việc" });
        }

        [HttpPost]
        public IActionResult Ping()
        {
            if (int.TryParse(HttpContext.Session.GetString("User ID"), out int userId) && userId > 0)
            {
                var user = _context.TblUsers.FirstOrDefault(x => x.IdUser == userId);
                if (user != null)
                {
                    user.LastPingAt = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            if (int.TryParse(HttpContext.Session.GetString("User ID"), out int userId) && userId > 0)
            {
                var user = _context.TblUsers.FirstOrDefault(x => x.IdUser == userId);
                if (user != null)
                {
                    user.ActiveUser = false;
                    _context.SaveChanges();
                }
            }
            HttpContext.Session.Clear();
            return Ok(new { redirectUrl = Url.Action("Index", "Home") ?? "".ToString() });
        }

        public async Task<string> GetLocationUser(int idLocation)
        {
            var getLocation = await _context.TblLocations.Where(x => x.IdLocation == idLocation).SingleOrDefaultAsync();
            if(getLocation?.LocationName != null)
                return getLocation.LocationName.ToString();
            return string.Empty;
        }
        private static string DecodePassword(string? password)
        {
            byte[] bytesPassword = Convert.FromBase64String(password ?? "");
            string returnPassword = Encoding.UTF8.GetString(bytesPassword); 
            return returnPassword;
        }
    }

    public class NewPassword
    {
        public int IdUser { get; set; } 
        public string? PasswordUser { get; set; }
    }
    public class InfoLogin
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
