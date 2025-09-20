using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace MPLUS_GW_WebCore.Controllers.Materials
{
    public class FlowTimeLimitMaterialsController : Controller
    {
        private readonly MplusGwContext _context;
        private readonly Classa _cl;
        public FlowTimeLimitMaterialsController(MplusGwContext context, Classa cl) 
        { 
            _context = context;
            _cl = cl;
        }

        [Route("/nguyen-vat-lieu/time-limit")]
        public IActionResult Index()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false },
                new() { Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() { Title = "Thời Hạn NVL", Url = Url.Action("Index", "FlowTimeLimitMaterials"), IsActive = true },
            };
            DateTime today = DateTime.Now;

            var getAllMaterial = _context.TblImportedItems
                .Where(x => x.ItemCode != null && !string.IsNullOrEmpty(x.TimeSterilization) && x.TimeImport != null && x.TimeImport.Value.Date == today.Date)
                .ToList();
            List<ListMaterialTimeLimit> listMaterialTimeLimits = new();
            foreach (var item in getAllMaterial)
            {
                string stringFormatTime = string.Empty;    
                if (!string.IsNullOrEmpty(item.TimeSterilization))
                {
                    if (item.TimeSterilization.Length > 6)
                    {
                        stringFormatTime = "yyyyMMdd";
                    }
                    else
                    {
                        stringFormatTime = "yyMMdd";
                    }
                }
                string formattedDate = today.ToString(stringFormatTime);
                DateTime.TryParseExact(item.TimeSterilization, stringFormatTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeLimit);
                int.TryParse(item.TimeSterilization, out int timeLimitImported);
                int timeNow = int.Parse(formattedDate);
                string timeRemaining = string.Empty;
                string classBg = string.Empty;

                if ((timeLimitImported - timeNow) > 7)
                {
                    timeRemaining = (timeLimitImported - timeNow).ToString();
                    classBg = "";
                }
                else if (2 < (timeLimitImported - timeNow) && (timeLimitImported - timeNow) < 7)
                {
                    timeRemaining = (timeLimitImported - timeNow).ToString();
                    classBg = "bg-warning";
                }
                else if ((timeLimitImported - timeNow) < 2)
                {
                    timeRemaining = (timeLimitImported - timeNow).ToString();
                    classBg = "bg-danger";
                }

                if ((timeLimitImported - timeNow) < 0)
                {
                    timeRemaining = "0";
                }

                var std = new ListMaterialTimeLimit
                {
                    ProductCode = item.ItemCode ?? "",
                    LotNo = item.LotNo ?? "",
                    TimeLimit = timeLimit.ToString("dd/MM/yyyy") ?? "",
                    ClassBg = classBg,
                    TimeRemaining = timeRemaining,
                    PersonWorking = _context.TblUsers.Where(x => x.IdUser == item.IdUser).FirstOrDefault()?.DisplayName ?? "",
                    DateTimeImport = item.TimeImport != null ? item.TimeImport.Value.ToString("dd/MM/yyyy") : ""
                };
                listMaterialTimeLimits.Add(std);
            }
            var searchResults = TempData["SearchResult"] as string;
            ViewData["ValueMaterialSearched"] = TempData["ValueMaterialSearched"];
            ViewData["ValueDateFrom"] = TempData["ValueDateFrom"];
            ViewData["ValueDateTo"] = TempData["ValueDateTo"];
            ViewData["ValueLocationSearched"] = TempData["ValueLocationSearched"];
            ViewData["ValueUserSearched"] = TempData["ValueUserSearched"];
            ViewData["ErrorMessage"] = TempData["ErrorMessage"] as string;
            ViewData["SearchResults"] = JsonConvert.DeserializeObject<List<ListMaterialTimeLimit>>(searchResults ?? "");
            ViewData["ListItems"] = listMaterialTimeLimits;
            return View();
        }

        [HttpPost]
        public IActionResult SearchData(string searchMaterials, string searchDateFrom, string searchDateTo, string searchLocation, string searchUser)
        {
            DateTime today = DateTime.Now;
            var querySearchData = _context.TblImportedItems.AsQueryable();
            List<ListMaterialTimeLimit> searchResults = new();
            var locations = _context.TblLocations.ToList();
            var users = _context.TblUsers.ToList();
            if (!string.IsNullOrEmpty(searchMaterials))
            {
                querySearchData = querySearchData.Where(p => p.ItemCode == searchMaterials);
            }
            if (!string.IsNullOrEmpty(searchDateFrom) && !string.IsNullOrEmpty(searchDateTo))
            {
                DateTime.TryParseExact(searchDateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeSearchFrom);
                DateTime.TryParseExact(searchDateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeSearchTo);
                querySearchData = querySearchData.Where(p => p.TimeImport != null && p.TimeImport.Value.Date >= timeSearchFrom.Date && p.TimeImport.Value.Date <= timeSearchTo.Date);
            }
            if (!string.IsNullOrEmpty(searchLocation))
            {
                var idLocation = locations.FirstOrDefault(s => s.LocationCode == searchLocation)?.IdLocation;
                querySearchData = querySearchData.Where(p => idLocation != null && p.IdLocation == idLocation);
            }
            if (!string.IsNullOrEmpty(searchUser))
            {
                var userId = users.FirstOrDefault(s => s.DisplayName != null && s.DisplayName.Contains(searchUser))?.IdUser;
                querySearchData = querySearchData.Where(p => userId != null && p.IdUser == userId);
            }
            foreach (var item in querySearchData)
            {
                string stringFormatTime = string.Empty;
                if (!string.IsNullOrEmpty(item.TimeSterilization))
                {
                    if (item.TimeSterilization.Length > 6)
                    {
                        stringFormatTime = "yyyyMMdd";
                    }
                    else
                    {
                        stringFormatTime = "yyMMdd";
                    }
                }
                string formattedDate = today.ToString(stringFormatTime);

                DateTime? timeLimit = null;
                if (!string.IsNullOrEmpty(item.TimeSterilization))
                {
                    if (DateTime.TryParseExact(item.TimeSterilization, stringFormatTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        timeLimit = parsedDate;
                    }
                    int.TryParse(item.TimeSterilization, out int timeLimitImported);
                    int timeNow = int.Parse(formattedDate);
                    string timeRemaining = string.Empty;
                    string classBg = string.Empty;

                    if ((timeLimitImported - timeNow) > 7)
                    {
                        timeRemaining = (timeLimitImported - timeNow).ToString();
                        classBg = "";
                    }
                    else if (2 < (timeLimitImported - timeNow) && (timeLimitImported - timeNow) < 7)
                    {
                        timeRemaining = (timeLimitImported - timeNow).ToString();
                        classBg = "bg-warning";
                    }
                    else if ((timeLimitImported - timeNow) < 2)
                    {
                        timeRemaining = (timeLimitImported - timeNow).ToString();
                        classBg = "bg-danger";
                    }

                    if ((timeLimitImported - timeNow) < 0)
                    {
                        timeRemaining = "0";
                    }

                    var std = new ListMaterialTimeLimit
                    {
                        ProductCode = item.ItemCode ?? "",
                        LotNo = item.LotNo ?? "",
                        TimeLimit = timeLimit?.ToString("dd/MM/yyyy") ?? "N/A",
                        ClassBg = classBg,
                        TimeRemaining = timeRemaining,
                        PersonWorking = users.FirstOrDefault(s => s.IdUser == item.IdUser)?.DisplayName ?? "",
                        DateTimeImport = item.TimeImport != null ? item.TimeImport.Value.ToString("dd/MM/yyyy") : ""
                    };
                    searchResults.Add(std);
                }

            }
            if (searchResults.Count <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy dữ liệu";
            }
            TempData["ValueMaterialSearched"] = searchMaterials;
            TempData["ValueDateFrom"] = searchDateFrom;
            TempData["ValueDateTo"] = searchDateTo;
            TempData["ValueLocationSearched"] = searchLocation;
            TempData["ValueUserSearched"] = searchUser;
            TempData["SearchResult"] = JsonConvert.SerializeObject(searchResults);
            
            return RedirectToAction("Index", "FlowTimeLimitMaterials");
        }
        
    }

    public class ListMaterialTimeLimit
    {
        public string ProductCode { get; set; }
        public string LotNo { get; set; }
        public string TimeLimit { get; set; }
        public string TimeRemaining { get; set; }
        public string ClassBg { get; set; }
        public string PersonWorking {  get; set; }
        public string DateTimeImport { get; set; }
    }
}
