using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using Serilog;

namespace MPLUS_GW_WebCore.Controllers.Eink
{
    public class EinkController : Controller
    {
        private readonly EslsystemContext db_Eink;
        private readonly string apiKey = "CCAE6917-323B-4B5F-A62F-56910FA3F8CF";
        private readonly HttpClient httpClient;
        public EinkController(EslsystemContext _db)
        {
            db_Eink = _db;
        }
        public IActionResult Show()
        {
            ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() {Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false},
                new() {Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() {Title = "Quản lý E-ink", Url = Url.Action("Show", "Eink"), IsActive = false }
            };

            var all_eink = (from s in db_Eink.Labelstatuses
                           join l in db_Eink.Links on s.Mac equals l.Mac
                           where s.Mac == l.Mac
                           select new InfoEinks
                           {
                               IdProduct = l.Id,
                               EinkMac = l.Mac,
                               Variant = l.Variant,
                               Description = s.Description,
                               Group = db_Eink.TblProducts.Where(x => x.Iditem.ToString() == l.Id).Select(s => s.HeThong).FirstOrDefault(),
                               ImageFile = s.ImageFile,
                               PollInterval = s.PollInterval,
                               PollTimeout = s.PollTimeout,
                               ScanInterval = s.ScanInterval,
                               BatteryStatus = s.BatteryStatus,
                               BatteryVoltage = s.BatteryVoltage,
                               LastPoll = s.LastPoll,
                               LastInfo = s.LastInfo,
                               LastImage = s.LastImage,
                               BaseStation = s.BaseStation,
                           }).ToList();
            ViewData["ListEink"] = all_eink.Where(x => x.Group == "M+ GW").ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Remove(string endpoint, string productId)
        {
            try
            {
                if (!string.IsNullOrEmpty(productId))
                {
                    Guid productIdGuid = Guid.Parse(productId);
                    var existingProductESL = db_Eink.TblProducts.Where(x => x.Iditem == productIdGuid && 
                    x.MoTa == "Eink Máng" && 
                    x.HeThong == "M+ GW").FirstOrDefault();
                    if (existingProductESL != null)
                    {
                        db_Eink.TblProducts.RemoveRange(existingProductESL);
                    }
                    db_Eink.SaveChanges();
                }
                var response = await PostLinkESL(endpoint, httpClient);
                if (response)
                {
                    return Ok(new { message = "Unlink ESL thành công." });
                }
                else
                {
                    return StatusCode(500, new { message = "Unlink ESL có lỗi. Vui lòng liên hệ Admin để xử lý." });
                }
             
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }

        }
        private async Task<bool> PostLinkESL(string endpoint, HttpClient _client)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, SslPolicyErrors) => true
            };
            _client = new HttpClient(handler);

            _client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            try
            {
                var response = await _client.PostAsync(endpoint, null).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Log.Fatal($"Error: {response.StatusCode} - {content}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Fatal($"HTTP error: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                Log.Fatal("Request timed out.");
                return false;
            }
        }
    }

    public class InfoEinks
    {
        public string? IdProduct { get; set; }
        public string? EinkMac { get; set; }
        public string? Variant { get; set; }
        public string? Description { get; set; }
        public string? Group { get; set; }
        public string? ImageFile { get; set; }
        public int? PollInterval { get; set; }
        public int? PollTimeout { get; set; }
        public int? ScanInterval { get; set; }
        public int? BatteryStatus { get; set; }
        public decimal? BatteryVoltage { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? FirmwareSubversion { get; set; }
        public int? ImageId { get; set; }
        public int? ImageIdLocal { get; set; }
        public int? Backlight { get; set; }
        public int? DisplayOptions { get; set; }
        public int? Lqi { get; set; }
        public int? LqiRx { get; set; }
        public DateTime? LastPoll { get; set; }
        public DateTime? LastInfo { get; set; }
        public DateTime? LastImage { get; set; }
        public string? BaseStation { get; set; }
        public int? ScanChannels { get; set; }
        public int? FirmwareStatus { get; set; }
        public int? ImageStatus { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? ImgIx { get; set; }
        public int? ImgFormat { get; set; }
        public int? BootCount { get; set; }
        public int? Temperature { get; set; }
        public string? Lanid { get; set; }
        public string? Panid { get; set; }
        public int? LedOptions { get; set; }
        public int? NfcOptions { get; set; }
    }
}
