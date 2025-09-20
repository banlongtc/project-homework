using ConnectMES;
using Microsoft.AspNetCore.Mvc;
using MPLUS_GW_WebCore.Models;
using OfficeOpenXml.Table.PivotTable;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace MPLUS_GW_WebCore.Controllers.Stock
{
    public class MasterController : Controller
    {
        private readonly MplusGwContext db;
        private readonly Classa _clst;
        public MasterController(MplusGwContext _db, Classa classa)
        {
            db = _db;
            _clst = classa ?? throw new ArgumentNullException(nameof(classa));
        }
        [Route("/Master-data")]
        public IActionResult MasterTC()
        {

            var listtc = db.TblMasterTcs.ToList();
            ViewData["ListTC"] = listtc;

            return View();
        }
        public IActionResult DetailTC()
        {

            var listtc = db.TblDetailTcs.ToList();
            ViewData["ListdetailTC"] = listtc;
            var listdetc = db.TblMasterTcs.ToList();
            ViewData["Listc"] = listdetc;

            return View();
        }
        public IActionResult ItemNumber()
        {
            var listtc = db.TblMasterProductItems.ToList();
            ViewData["ListItem"] = listtc;

            return View();
        }
        public IActionResult TCthongso()
        {
            var item = db.TblItemValTcs.ToList();
            ViewData["Listitem"] = item;
            var tc = db.TblDetailTcs.ToList();
            ViewData["Listtc"] = tc;
            return View();
        }
        public IActionResult TCmay()
        {
            var may = db.TblMachineValTcs.ToList();
            ViewData["Listmay"] = may;
            var tc = db.TblDetailTcs.ToList();
            ViewData["Listtc"] = tc;
            return View();
        }
        public IActionResult Error()
        {
            var er = db.TblMasterErrors.ToList();
            ViewData["Listerror"] = er;

            return View();
        }
        public IActionResult Location()
        {
            var loc = db.TblLocationCs.ToList();
            ViewData["Listloc"] = loc;

            var loccha = db.TblLocations.ToList();
            ViewData["Listloccha"] = loccha;


            return View();
        }
        public IActionResult Item_Loc()
        {
            var itloc = db.TblItemLocations.ToList();
            ViewData["Listitloc"] = itloc;


            return View();
        }

        //**************************************Chức năng master Error ****************************************************

        [HttpPost]
        public IActionResult SaveError(int tidcha, string tnamev, string tnamej, string tlocation, string tremark, string tid)
        {
            try
            {
                //var error = new TblMasterError { ErrorName = namev };
                if (tid == "new")
                {
                    TblMasterError er = new TblMasterError();
                    er.Idcha = tidcha;
                    er.ErrorName = tnamev;
                    er.NameJp = tnamej;
                    er.Location = tlocation;
                    er.Remarks = tremark;
                    db.TblMasterErrors.Add(er);
                    db.SaveChanges();
                }
                else
                {
                    int id = Convert.ToInt32(tid);
                    var sua = db.TblMasterErrors.Where(s => s.Id == id).FirstOrDefault();
                    if (sua != null)
                    {
                        sua.Idcha = tidcha;
                        sua.ErrorName = tnamev;
                        sua.NameJp = tnamej;
                        sua.Location = tlocation;
                        sua.Remarks = tremark;
                        db.SaveChanges();
                    }
                }

                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DelError(string tid)
        {
            try
            {
                int id = Convert.ToInt32(tid);
                var xoa = db.TblMasterErrors.Where(s => s.Id == id).FirstOrDefault();
                if (xoa != null)
                {
                    db.TblMasterErrors.Remove(xoa);
                    db.SaveChanges();
                }
                return Ok(new { message = "Thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        //************************************************* Chức năng master item number *****************************
        public IActionResult SaveItem(string tid, string titemcode, string titemname, string titemtype, string tunit, string tremark)
        {
            try
            {
                if (tid == "new")
                {
                    TblMasterProductItem it = new TblMasterProductItem();
                    it.ItemCode = titemcode;
                    it.ItemName = titemname;
                    it.ItemType = titemtype;
                    it.Unit = tunit;
                    it.Remarks = tremark;
                    db.TblMasterProductItems.Add(it);
                    db.SaveChanges();
                }
                else
                {
                    int id = Convert.ToInt32(tid);
                    var sua = db.TblMasterProductItems.Where(s => s.IdItem == id).FirstOrDefault();
                    if (sua != null)
                    {
                        sua.ItemCode = titemcode;
                        sua.ItemName = titemname;
                        sua.ItemType = titemtype;
                        sua.Unit = tunit;
                        sua.Remarks = tremark;
                        db.SaveChanges();
                    }
                }
                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DelItem(string tid)
        {
            try
            {
                int id = Convert.ToInt32(tid);
                var xoa = db.TblMasterProductItems.Where(s => s.IdItem == id).FirstOrDefault();
                if (xoa != null)
                {
                    db.TblMasterProductItems.Remove(xoa);
                    db.SaveChanges();
                }
                return Ok(new { message = "Thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        //************************************************* Chức năng master Tiêu chuẩn *****************************
        public IActionResult SaveTC(string tid, string ttccode, string ttcname, string ttcmay, string tremark)
        {
            try
            {
                if (tid == "new")
                {
                    TblMasterTc tc = new TblMasterTc();
                    tc.TcCode = ttccode;
                    tc.TenTieuchuan = ttcname;
                    tc.TcMay = Convert.ToBoolean(ttcmay);
                    tc.Remark = tremark;
                    db.TblMasterTcs.Add(tc);
                    db.SaveChanges();
                }
                else
                {
                    int id = Convert.ToInt32(tid);
                    var sua = db.TblMasterTcs.Where(s => s.IdTc == id).FirstOrDefault();
                    if (sua != null)
                    {
                        sua.TcCode = ttccode;
                        sua.TenTieuchuan = ttcname;
                        sua.TcMay = Convert.ToBoolean(ttcmay);
                        sua.Remark = tremark;
                        db.SaveChanges();
                    }
                }
                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DelTC(string tid)
        {
            try
            {
                int id = Convert.ToInt32(tid);
                var xoa = db.TblMasterTcs.Where(s => s.IdTc == id).FirstOrDefault();
                if (xoa != null)
                {
                    db.TblMasterTcs.Remove(xoa);
                    db.SaveChanges();
                }
                return Ok(new { message = "Thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult GetTC(string idCha)
        {
            int id = Convert.ToInt32(idCha);
            var tc = db.TblDetailTcs.Where(s => s.IdTc == id)
                .Select(s => new { s.IdDetail, s.TenTc, s.ValueText, s.ValueInt, s.ValueDecimal, s.Unit, s.ValueUnit }).ToList();

            return Ok(new { dataDetail = tc });
        }
        //************************************************* Chức năng master Chi tiết Tiêu chuẩn *****************************

        public IActionResult SavedeTC(string tid, string tname, string tmota, string ttext, int tint, decimal tdecimal, string tunit, string tvaunit, int tidtc)
        {
            try
            {
                if (tid == "new")
                {
                    TblDetailTc detc = new TblDetailTc();
                    detc.TenTc = tname;
                    detc.MoTa = tmota;
                    detc.ValueText = ttext;
                    detc.ValueInt = tint;
                    detc.ValueDecimal = tdecimal;
                    detc.Unit = tunit;
                    detc.ValueUnit = tvaunit;
                    detc.IdTc = tidtc;
                    db.TblDetailTcs.Add(detc);
                    db.SaveChanges();
                }
                else
                {
                    int id = Convert.ToInt32(tid);
                    var sua = db.TblDetailTcs.Where(s => s.IdDetail == id).FirstOrDefault();
                    if (sua != null)
                    {
                        sua.TenTc = tname;
                        sua.MoTa = tmota;
                        sua.ValueText = ttext;
                        sua.ValueInt = tint;
                        sua.ValueDecimal = tdecimal;
                        sua.Unit = tunit;
                        sua.ValueUnit = tvaunit;
                        sua.IdTc = tidtc;
                        db.SaveChanges();
                    }
                }
                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DeldeTC(string tid)
        {
            try
            {
                int id = Convert.ToInt32(tid);
                var xoa = db.TblDetailTcs.Where(s => s.IdDetail == id).FirstOrDefault();
                if (xoa != null)
                {
                    db.TblDetailTcs.Remove(xoa);
                    db.SaveChanges();
                }
                return Ok(new { message = "Thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        //************************************************* Chức năng master Location *****************************
        public IActionResult SaveLoc(string tid, string tcode, string tname, int tidcha)
        {
            try
            {
                if (tid == "new")
                {
                    TblLocationC loc = new TblLocationC();
                    loc.LocationCodeC = tcode;
                    loc.LocationNameC = tname;
                    loc.IdChaC = tidcha;
                    db.TblLocationCs.Add(loc);
                    db.SaveChanges();
                }
                else
                {
                    int id = Convert.ToInt32(tid);
                    var sua = db.TblLocationCs.Where(s => s.Id == id).FirstOrDefault();
                    if (sua != null)
                    {
                        sua.LocationCodeC = tcode;
                        sua.LocationNameC = tname;
                        sua.IdChaC = tidcha;
                        db.SaveChanges();
                    }
                }
                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DelLoc(string tid)
        {
            try
            {
                int id = Convert.ToInt32(tid);
                var xoa = db.TblLocationCs.Where(s => s.Id == id).FirstOrDefault();
                if (xoa != null)
                {
                    db.TblLocationCs.Remove(xoa);
                    db.SaveChanges();
                }
                return Ok(new { message = "Thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}