using Microsoft.AspNetCore.Mvc;

namespace MPLUS_GW_WebCore.Controllers.Admin.Masters
{
    public class StandardValueController : Controller
    {
        [Route("/admin/gia-tri-tieu-chuan")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
