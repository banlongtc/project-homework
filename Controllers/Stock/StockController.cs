using Azure;
using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using Serilog;
using System;
using System.Data;
using System.Globalization;
using System.Net.Security;
using System.Reflection.Metadata;
using System.Security.Policy;


namespace MPLUS_GW_WebCore.Controllers.Stock
{
    public class StockController : Controller
    {
        private readonly MplusGwContext db;
        private readonly EslsystemContext db_Eink;
        private readonly Classa _clst;
        string result;       
        private readonly string apiKey = "CCAE6917-323B-4B5F-A62F-56910FA3F8CF";

        private readonly HttpClient _client;

        public StockController(MplusGwContext _db, Classa classa, EslsystemContext _dbeink)
        {
            db = _db;
            db_Eink = _dbeink;
            _clst = classa ?? throw new ArgumentNullException(nameof(classa)); 
        }
        [Route("/QLstock")]
        public IActionResult Show(string txtmasp, string txtsolot, string result)
        {
            
             ViewBag.Breadcrumbs = new List<Breadcrumb>
            {
                new() {Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false},
                new() {Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
                new() {Title = "Quản lý tồn kho", Url = Url.Action("Show", "Stock"), IsActive = false }
            };

            var loadcd = db.TblLocations.Select(s => s.LocationCode).ToList();
            if (result == null)
            {
                result = string.Join(", ", loadcd);
            }

            #region truy vấn bảng
            var listTonQuery = db_Eink.TblProducts.AsQueryable();

            listTonQuery = listTonQuery.Where(s => s.MoTa == "Eink");

            if (!string.IsNullOrWhiteSpace(txtmasp))
            {
                listTonQuery = listTonQuery.Where(s => s.ItemCode == txtmasp);
            }

            if (!string.IsNullOrWhiteSpace(txtsolot))
            {
                listTonQuery = listTonQuery.Where(s => s.LotNo == txtsolot);
            }

            if (!string.IsNullOrWhiteSpace(txtmasp) && !string.IsNullOrWhiteSpace(txtsolot) && !string.IsNullOrWhiteSpace(result))
            {
                listTonQuery = listTonQuery.Where(s => s.MaCd == result);
            }

            // Gán kết quả vào ViewData
            ViewData["Listtonkho"] = listTonQuery.OrderBy(x => x.MaCd).ToList();
            #endregion

            var loadcd2 = db.TblLocations.ToList();
            ViewData["Listcd"] = loadcd2;

            //var all_eink = db_Eink.Labelstatuses.Where(s => s.Id == null || s.Id == "").ToList();
            //ViewData["ListEink"] = all_eink;

            var linkeink = db_Eink.Links.ToList();
            ViewData["CheckEink"] = linkeink;


            return View();

        }

        #region code show trực tiếp từ Mes
        //public IActionResult Show(string txtmasp, string txtsolot, string result)
        //{
        //    ViewBag.Breadcrumbs = new List<Breadcrumb>
        //    {
        //        new() {Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false},
        //        new() {Title = "Nguyên Vật Liệu", Url = Url.Action("Index", "Materials"), IsActive = false },
        //        new() {Title = "Quản lý tồn kho", Url = Url.Action("Show", "Stock"), IsActive = false }
        //    };
        //    // var loadhsd = db.TblImportedItems.ToList();

        //    var loadcd = db.TblLocations.Select(s => s.LocationCode).ToList();
        //    if (result == null)
        //    {
        //        result = string.Join(", ", loadcd);
        //    }

        //    var loadstock = _clst.QL_Stock(txtmasp, txtsolot, result);

        //    List<Liststo> listTon = new();
        //    listTon.Clear();
        //    foreach (DataRow row in loadstock.Rows)
        //    {
        //        string Item = row["ARTICLECODE"].ToString() ?? "";
        //        string Name = row["ARTICLESHORTNAME"].ToString() ?? "";
        //        string Lot = row["LOTNO"].ToString() ?? "";
        //        string CD = row["PLACEDETAILCODE"].ToString() ?? "";
        //        string CDName = row["PLACEDETAILNAME"].ToString() ?? "";

        //        var loadhsd = db.TblImportedItems.Where(s => s.ItemCode == Item && s.LotNo == Lot).Select(s => s.TimeSterilization).FirstOrDefault();
        //        DateTime? Hansd = string.IsNullOrEmpty(loadhsd) ? (DateTime?)null : DateTime.ParseExact(loadhsd, "yyMMdd", CultureInfo.InvariantCulture);

        //        if (Hansd.HasValue)
        //        {
        //            TimeSpan diff = Hansd.Value - DateTime.Now;
        //            int remain = Math.Abs(diff.Days);

        //            listTon.Add(new Liststo(
        //                Item,
        //                Lot,
        //                Name,
        //                Convert.ToDecimal(row["TOTALQTY"].ToString()),
        //                Convert.ToDecimal(row["RESERVED"].ToString()),
        //                Convert.ToDecimal(row["ACTUALQTY"].ToString()),
        //                CD,
        //                CDName, Hansd, remain));
        //        }
        //        else
        //        {
        //            listTon.Add(new Liststo(
        //                Item,
        //                Lot,
        //                Name,
        //                Convert.ToDecimal(row["TOTALQTY"].ToString()),
        //                Convert.ToDecimal(row["RESERVED"].ToString()),
        //                Convert.ToDecimal(row["ACTUALQTY"].ToString()),
        //                CD,
        //                CDName, Hansd, 0));
        //        }

        //    }

        //    #region code join
        //    //var listton1 = (from s in loadhsd
        //    //                join b in listTon
        //    //                on s.ItemCode equals b.Item
        //    //                select new
        //    //                {
        //    //                    b.Item,
        //    //                    b.Name,
        //    //                    b.Lot,
        //    //                    b.Tongton,
        //    //                    b.Tondd,
        //    //                    b.Toncsd,
        //    //                    b.Macd,
        //    //                    b.Tencd,
        //    //                    s.TimeSterilization
        //    //                }).ToList();
        //    #endregion


        //    ViewData["Listtonkho"] = listTon;

        //    var loadcd2 = db.TblLocations.ToList();
        //    ViewData["Listcd"] = loadcd2;

        //    var all_eink = db_Eink.Labelstatuses.Where(s=>s.Id == null || s.Id == "").ToList();
        //    ViewData["ListEink"] = all_eink;

        //    return View();


        //}
        #endregion

        [HttpPost]
        public IActionResult Search(string txtmasp, string txtsolot, string result)
        {
            if (!String.IsNullOrEmpty(txtmasp) && !String.IsNullOrEmpty(txtsolot))
            {
                return RedirectToAction("Show", "Stock", new { txtmasp = txtmasp, txtsolot = txtsolot, result = result });
            }
            else
            {
                return View();
            }
        }

        public IActionResult Export()
        {
            #region list tồn kho
            var loadcd = db.TblLocations.Select(s => s.LocationCode).ToList();
            string result = string.Join(", ", loadcd);
            var loadstock = _clst.QL_Stock("", "", result);
            List<Liststo> listTon = new();
            listTon.Clear();
            foreach (DataRow row in loadstock.Rows)
            {
                string Item = row["ARTICLECODE"].ToString() ?? "";
                string Name = row["ARTICLESHORTNAME"].ToString() ?? "";
                string Lot = row["LOTNO"].ToString() ?? "";
                string CD = row["PLACEDETAILCODE"].ToString() ?? "";
                string CDName = row["PLACEDETAILNAME"].ToString() ?? "";

                var loadhsd = db.TblImportedItems.Where(s => s.ItemCode == Item && s.LotNo == Lot).Select(s => s.TimeSterilization).FirstOrDefault();
                DateTime? Hansd = string.IsNullOrEmpty(loadhsd) ? (DateTime?)null : DateTime.ParseExact(loadhsd, "yyMMdd", CultureInfo.InvariantCulture);

                if (Hansd.HasValue)
                {
                    TimeSpan diff = Hansd.Value - DateTime.Now;
                    int remain = Math.Abs(diff.Days);

                    listTon.Add(new Liststo(
                        Item,
                        Lot,
                        Name,
                        Convert.ToDecimal(row["TOTALQTY"].ToString()),
                        Convert.ToDecimal(row["RESERVED"].ToString()),
                        Convert.ToDecimal(row["ACTUALQTY"].ToString()),
                        CD,
                        CDName, Hansd, remain));
                }
                else
                {
                    listTon.Add(new Liststo(
                        Item,
                        Lot,
                        Name,
                        Convert.ToDecimal(row["TOTALQTY"].ToString()),
                        Convert.ToDecimal(row["RESERVED"].ToString()),
                        Convert.ToDecimal(row["ACTUALQTY"].ToString()),
                        CD,
                        CDName, Hansd, 0));
                }
            }
            #endregion

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Sheet1");

                #region tiêu đề
                ws.Cells[1, 1].Value = "Mã sản phẩm";
                ws.Cells[1, 2].Value = "Số lot";
                ws.Cells[1, 3].Value = "Tên sản phẩm";
                ws.Cells[1, 4].Value = "Hạn sử dụng";
                ws.Cells[1, 5].Value = "Tổng tồn";
                ws.Cells[1, 6].Value = "Tồn dở dang";
                ws.Cells[1, 7].Value = "Tồn chưa sử dụng";
                ws.Cells[1, 8].Value = "Mã công đoạn";
                ws.Cells[1, 9].Value = "Tên công đoạn";
                #endregion

                int row = 2;
                foreach (var item in listTon)
                {
                    ws.Cells[row, 1].Value = item.Item;
                    ws.Cells[row, 2].Value = item.Lot;
                    ws.Cells[row, 3].Value = item.Name;
                    ws.Cells[row, 4].Value = item.Hansd;
                    ws.Cells[row, 5].Value = item.Tongton;
                    ws.Cells[row, 6].Value = item.Tondd;
                    ws.Cells[row, 7].Value = item.Toncsd;
                    ws.Cells[row, 8].Value = item.Macd;
                    ws.Cells[row, 9].Value = item.Tencd;

                    row++;
                }
                ws.Cells.AutoFitColumns();

                byte[] fileContents = package.GetAsByteArray();

                return File(
                    fileContents: fileContents,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: "data.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eink(string endpoint)
        {
            try
            {
                var response = await PostLinkESL(endpoint);
                if(response)
                {
                    return Ok(new { message = "Link ESL thành công." });
                } else
                {
                    return StatusCode(500, new { message = "Link dữ liệu đến ESL bị lỗi." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private Task<bool> PostLinkESL(string endpoint)
        {
            return PostLinkESL(endpoint, _client);
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

        public IActionResult Eink_code(string id)
        {          
            var data = db_Eink.Links.Where(s => s.Id == id).Select(s=>s.Mac).ToList();           
            string list_eink = string.Join(", ", data);                

            return Json(list_eink);
        }

        public async Task StockMes()
        {
            var loadcd = db.TblLocations.Select(s => s.LocationCode).ToList();
            if (result == null)
            {
                result = string.Join(", ", loadcd);
            }
            var loadstock = _clst.QL_Stock("", "", result);
            foreach (DataRow row in loadstock.Rows)
            {
                string Item = row["ARTICLECODE"].ToString() ?? "";
                string Name = row["ARTICLESHORTNAME"].ToString() ?? "";
                string Lot = row["LOTNO"].ToString() ?? "";
                string CD = row["PLACEDETAILCODE"].ToString() ?? "";
                string CDName = row["PLACEDETAILNAME"].ToString() ?? "";
                var loadhsd = db.TblImportedItems.Where(s => s.ItemCode == Item && s.LotNo == Lot
                && s.TimeSterilization != "").Select(s => s.TimeSterilization).FirstOrDefault();

                //Ghi dữ liệu vào bảng E-ink
                var eink = db_Eink.TblProducts.Where(s => s.ItemCode == Item && s.LotNo == Lot && s.MaCd == CD).FirstOrDefault();
                if (eink != null)
                {
                    eink.TenSp = Name;
                    eink.Qty = Convert.ToInt32(row["TOTALQTY"].ToString());
                    eink.QtyPlan = Convert.ToInt32(row["RESERVED"].ToString());
                    eink.QtyOk = Convert.ToInt32(row["ACTUALQTY"].ToString());
                    eink.TenCd = CDName;
                    eink.HeThong = "M+ GW";
                    eink.MoTa = "Eink";
                    if (!string.IsNullOrEmpty(loadhsd))
                    {
                        DateTime? Hansd = DateTime.ParseExact(loadhsd, "yyMMdd", CultureInfo.InvariantCulture);
                        if (Hansd.HasValue)
                        {
                            TimeSpan diff = Hansd.Value - DateTime.Now;
                            int days = diff.Days;

                            eink.HanSuDung = Hansd.Value.ToString("dd/MM/yyyy");
                            eink.RDatetime1 = Hansd;
                            eink.RInt2 = days;
                        }
                    }
                    await db_Eink.SaveChangesAsync();


                }
                else
                {
                    TblProduct pro = new TblProduct();
                    pro.ItemCode = Item;
                    pro.LotNo = Lot;
                    pro.MaCd = CD;
                    pro.TenSp = Name;
                    pro.Qty = Convert.ToInt32(row["TOTALQTY"].ToString());
                    pro.QtyPlan = Convert.ToInt32(row["RESERVED"].ToString());


                    pro.QtyOk = Convert.ToInt32(row["ACTUALQTY"].ToString());
                    pro.TenCd = CDName;
                    if (!string.IsNullOrEmpty(loadhsd))
                    {
                        DateTime? Hansd = DateTime.ParseExact(loadhsd, "yyMMdd", CultureInfo.InvariantCulture);
                        if (Hansd.HasValue)
                        {
                            TimeSpan diff = Hansd.Value - DateTime.Now;
                            int days = diff.Days;
                            pro.HanSuDung = Hansd.Value.ToString("dd/MM/yyyy");
                            pro.RDatetime1 = Hansd;
                            pro.RInt2 = days;
                        }
                    }
                    pro.HeThong = "M+ GW";
                    pro.MoTa = "Eink";
                    db_Eink.TblProducts.Add(pro);
                    await db_Eink.SaveChangesAsync();
                }
            }
            //Xóa các tồn kho = 0 của các thẻ E-ink
            //tồn = 0 hoặc tồn kho = tồn đã sử dụng hết

            var xoaeink = db_Eink.TblProducts.Where(s => s.MoTa == "Eink").ToList();
            foreach (var it in xoaeink)
            {
                var mes = loadstock.Select($"ARTICLECODE = '{it.ItemCode}' AND LOTNO = '{it.LotNo}' AND PLACEDETAILCODE = '{it.MaCd}'").ToList();
                if (mes.Count() == 0)
                {
                    //Xóa liên kết Eink những thẻ ko còn tồn
                    var xoalink = db_Eink.Links.Where(s => s.Id == it.Iditem.ToString()).ToList();
                    db_Eink.Links.RemoveRange(xoalink);
                    db_Eink.SaveChanges();
                    //Xóa bảng Product
                    db_Eink.TblProducts.Remove(it);
                    db_Eink.SaveChanges();

                }
            }


        }
    }
}

public class Liststo
{
    public string Item { get; set; }
    public string Name { get; set; }
    public string Lot { get; set; }
    public decimal Tongton { get; set; }
    public decimal Tondd { get; set; }
    public decimal Toncsd { get; set; }
    public string Macd { get; set; }
    public string Tencd { get; set; }

    public DateTime? Hansd { get; set; }

    public int? Ngaysd { get; set; }
    public Liststo(string _Item, string _Lot, string _Name, decimal _Tongton, decimal _Tondd, decimal _Toncsd, string _Macd, string _Tencd, DateTime? _Hansd, int _ngaysd)
    {
        Item = _Item;
        Lot = _Lot;
        Tongton = _Tongton;
        Tondd = _Tondd;
        Toncsd = _Toncsd;
        Macd = _Macd;
        Tencd = _Tencd;
        Name = _Name;
        Hansd = _Hansd;
        Ngaysd = _ngaysd;
    }
}

