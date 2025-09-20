using Microsoft.AspNetCore.Mvc;

namespace MPLUS_GW_WebCore.Controllers.Admin.Masters
{
    public class MasterJIGsController : Controller
    {
        public MasterJIGsController() { }

        [Route("/admin/master-jigs")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
