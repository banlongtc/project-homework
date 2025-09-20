using Microsoft.AspNetCore.Mvc.Filters;

namespace MPLUS_GW_WebCore.Models
{
    public class Breadcrumb
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
