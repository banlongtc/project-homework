using Microsoft.AspNetCore.Mvc;
using MPLUS_GW_WebCore.Models;

namespace MPLUS_GW_WebCore.Views.Components
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Breadcrumb> breadcrumbs)
        {
            return View(breadcrumbs);
        }
    }
}
