using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace MPLUS_GW_WebCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly MplusGwContext _context;

        public HomeController(MplusGwContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = true },
            };
            ViewBag.UserID = HttpContext.Session.GetString("UserID");
            ViewBag.UserRole = HttpContext.Session.GetString("RoleName");
            ViewBag.Displayname = HttpContext.Session.GetString("DisplayName");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Log.Error("Error: {ID}", Activity.Current?.Id);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/dang-xuat")]
        public IActionResult Logout()
        {
            int userId = int.Parse(HttpContext.Session.GetString("User ID") ?? "0");
            if (userId > 0)
            {
                var checkUser = _context.TblUsers.Where(x => x.IdUser == userId).FirstOrDefault();
                if (checkUser != null) {
                    checkUser.ActiveUser = false;
                }
                _context.SaveChanges();
            }
            HttpContext.Session.Clear();
            
            return RedirectToAction("Index", "Home");
        }
    }
}
