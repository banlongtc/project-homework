using ConnectMES;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Controllers.Processing;
using MPLUS_GW_WebCore.Models;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

namespace MPLUS_GW_WebCore.Controllers.Stock
{
    public class ImportController : Controller
    {
        private readonly MplusGwContext db;
        private readonly EslsystemContext db_Eink;
        private readonly Classa _clst;
        private readonly ExportDataQadContext dbl;
        private readonly IWebHostEnvironment _environment;
        string start, location = "";
        public ImportController(MplusGwContext _db, Classa classa, EslsystemContext _dbeink, ExportDataQadContext _dbl, IWebHostEnvironment environment)
        {
            db = _db;
            db_Eink = _dbeink;
            _clst = classa ?? throw new ArgumentNullException(nameof(classa));
            dbl = _dbl;
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        [Route("/Import-data")]
        public IActionResult Show()
        {
            var loadcd2 = db.TblMasterProductItems.ToList();
            ViewData["Listitem"] = loadcd2;

            var loadcd = db.TblLocations.ToList();
            ViewData["Listcd"] = loadcd;

            return View();

        }
        public IActionResult Month()
        {
            var loadcd2 = db.TblMasterProductItems.ToList();
            ViewData["Listitem"] = loadcd2;

            var loadcd = db.TblLocations.ToList();
            ViewData["Listcd"] = loadcd;

            return View();

        }

        // Import Item Master
        [HttpPost]
        public async Task<IActionResult> Btnimpo_Click(IFormFile FileUpload1, ImportViewModel model)
        {
            try
            {
                var idchon = model.SelectedValue;

                if (FileUpload1 != null && FileUpload1.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await FileUpload1.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            if (idchon == "1")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Item_Master"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        var ItemCode = ws.Cells[i, 1].Value.ToString();
                                        var Name = ws.Cells[i, 2].Value.ToString();
                                        var Type = ws.Cells[i, 3].Value.ToString();
                                        var Unit = ws.Cells[i, 4].Value.ToString();

                                        var check = db.TblMasterProductItems.Where(s => s.ItemCode == ItemCode).FirstOrDefault();
                                        if (check != null)
                                        {
                                            check.ItemName = Name;
                                            check.ItemType = Type;
                                            check.Unit = Unit;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var it = new TblMasterProductItem();
                                            it.ItemCode = ItemCode;
                                            it.ItemType = Type;
                                            it.Unit = Unit;
                                            it.ItemName = Name;
                                            db.TblMasterProductItems.Add(it);
                                            db.SaveChanges();
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "2")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Item_Location"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        for (int c = 2; c <= ws.Dimension.End.Column; c++)
                                        {
                                            var item = ws.Cells[i, 1].Value == null ? "" : ws.Cells[i, 1].Value.ToString();
                                            var location = ws.Cells[1, c].Value == null ? "" : ws.Cells[1, c].Value.ToString();

                                            var check1 = ws.Cells[i, c].Value == null ? "" : ws.Cells[i, c].Value.ToString();
                                            if (check1 == "1")
                                            {
                                                var check = db.TblItemLocations.Where(s => s.ItemCode == item && s.LocationCode == location).ToList();
                                                if (check.Count == 0)
                                                {
                                                    var it = new TblItemLocation();
                                                    it.ItemCode = item;
                                                    it.LocationCode = location;
                                                    db.TblItemLocations.Add(it);
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "3")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Master_TC"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        var id = ws.Cells[i, 1].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 1].Value);
                                        if (id != 0)
                                        {
                                            var sua = db.TblMasterTcs.Where(s => s.IdTc == id).FirstOrDefault();
                                            if (sua != null)
                                            {
                                                sua.TenTieuchuan = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                                sua.Remark = ws.Cells[i, 3].Value == null ? "" : ws.Cells[i, 3].Value.ToString();
                                                sua.TcMay = ws.Cells[i, 4].Value == null ? false : Convert.ToBoolean(ws.Cells[i, 4].Value);
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            var it = new TblMasterTc();
                                            it.TenTieuchuan = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                            it.Remark = ws.Cells[i, 3].Value == null ? "" : ws.Cells[i, 3].Value.ToString();
                                            it.TcMay = ws.Cells[i, 4].Value == null ? false : Convert.ToBoolean(ws.Cells[i, 4].Value);
                                            db.TblMasterTcs.Add(it);
                                            db.SaveChanges();
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "4")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["TC_detail"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        var id = ws.Cells[i, 1].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 1].Value);
                                        if (id != 0)
                                        {
                                            var sua = db.TblDetailTcs.Where(s => s.IdDetail == id).FirstOrDefault();
                                            if (sua != null)
                                            {
                                                var tcthongso = ws.Cells[i, 3].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 3].Value);
                                                var check = db.TblMasterTcs.Where(s => s.IdTc == tcthongso).ToList();
                                                if (check.Count > 0)
                                                {
                                                    sua.TenTc = ws.Cells[i, 1].Value == null ? "" : ws.Cells[i, 1].Value.ToString();
                                                    sua.MoTa = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                                    sua.IdTc = tcthongso;
                                                    sua.ValueText = ws.Cells[i, 5].Value == null ? "" : ws.Cells[i, 5].Value.ToString();
                                                    sua.ValueInt = ws.Cells[i, 6].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 6].Value);
                                                    sua.ValueDecimal = ws.Cells[i, 7].Value == null ? 0 : Convert.ToDecimal(ws.Cells[i, 7].Value);
                                                    sua.Unit = ws.Cells[i, 8].Value == null ? "" : ws.Cells[i, 8].Value.ToString();
                                                    sua.ValueUnit = ws.Cells[i, 9].Value == null ? "" : ws.Cells[i, 9].Value.ToString();

                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var tcthongso = ws.Cells[i, 3].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 3].Value);
                                            var check = db.TblMasterTcs.Where(s => s.IdTc == tcthongso).ToList();
                                            if (check.Count > 0)
                                            {
                                                var it = new TblDetailTc();
                                                it.TenTc = ws.Cells[i, 1].Value == null ? "" : ws.Cells[i, 1].Value.ToString();
                                                it.MoTa = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                                it.IdTc = tcthongso;
                                                it.ValueText = ws.Cells[i, 5].Value == null ? "" : ws.Cells[i, 5].Value.ToString();
                                                it.ValueInt = ws.Cells[i, 6].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 6].Value);
                                                it.ValueDecimal = ws.Cells[i, 7].Value == null ? 0 : Convert.ToDecimal(ws.Cells[i, 7].Value);
                                                it.Unit = ws.Cells[i, 8].Value == null ? "" : ws.Cells[i, 8].Value.ToString();
                                                it.ValueUnit = ws.Cells[i, 9].Value == null ? "" : ws.Cells[i, 9].Value.ToString();
                                                db.TblDetailTcs.Add(it);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "5")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Item_TC_ThongSo"];
                                if (ws != null)
                                {
                                    for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                    {
                                        if (ws.Cells[i, 1].Value != null)
                                        {
                                            for (int col = 2; col <= ws.Dimension.End.Column; col++)
                                            {
                                                if (ws.Cells[i, col].Value != null)
                                                {
                                                    if (ws.Cells[i, col].Value.ToString() == "1")
                                                    {
                                                        var item = ws.Cells[i, 1].Value.ToString();
                                                        var nhom = ws.Cells[2, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[2, col].Value);
                                                        if (nhom != 0)
                                                        {
                                                            var check = db.TblItemValTcs.Where(s => s.ItemCode == item && s.IdNhomTc == nhom).FirstOrDefault();
                                                            if (check != null)
                                                            {
                                                                check.IdValTc = ws.Cells[3, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[3, col].Value);
                                                                db.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                var it = new TblItemValTc();
                                                                it.ItemCode = item;
                                                                it.IdNhomTc = nhom;
                                                                it.IdValTc = ws.Cells[3, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[3, col].Value);
                                                                db.TblItemValTcs.Add(it);
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "6")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Master_May"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        var maycode = ws.Cells[i, 1].Value == null ? "" : ws.Cells[i, 1].Value.ToString();
                                        var tenmay = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                        var location = ws.Cells[i, 3].Value == null ? "" : ws.Cells[i, 3].Value.ToString();

                                        var check = db.TblMasterMachines.Where(s => s.MachineCode == maycode).FirstOrDefault();
                                        if (check != null)
                                        {
                                            check.MachineName = tenmay;
                                            check.LocationCode = location;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var it = new TblMasterMachine();
                                            it.MachineCode = maycode;
                                            it.MachineName = tenmay;
                                            it.LocationCode = location;
                                            db.TblMasterMachines.Add(it);
                                            db.SaveChanges();
                                        }

                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "7")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Item_TC_May"];
                                if (ws != null)
                                {
                                    for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                    {
                                        if (ws.Cells[i, 1].Value != null)
                                        {
                                            for (int col = 2; col <= ws.Dimension.End.Column; col++)
                                            {
                                                if (ws.Cells[i, col].Value != null)
                                                {
                                                    if (ws.Cells[i, col].Value.ToString() == "1")
                                                    {
                                                        var may = ws.Cells[i, 1].Value.ToString();
                                                        var nhom = ws.Cells[2, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[2, col].Value);
                                                        if (nhom != 0)
                                                        {
                                                            var check = db.TblMachineValTcs.Where(s => s.MachineCode == may && s.IdNhomTc == nhom).FirstOrDefault();

                                                            if (check != null)
                                                            {
                                                                check.IdValTc = ws.Cells[3, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[3, col].Value);
                                                                db.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                var it = new TblMachineValTc();
                                                                it.MachineCode = may?.Trim() ?? "";
                                                                it.IdNhomTc = nhom;
                                                                it.IdValTc = ws.Cells[3, col].Value == null ? 0 : Convert.ToInt32(ws.Cells[3, col].Value);
                                                                db.TblMachineValTcs.Add(it);
                                                                db.SaveChanges();

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "8")
                            {
                                ExcelWorksheet ws = package.Workbook.Worksheets["Master_Error"];
                                if (ws != null)
                                {
                                    for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                    {
                                        var id = ws.Cells[i, 1].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 1].Value);
                                        var Location = ws.Cells[i, 2].Value == null ? "" : ws.Cells[i, 2].Value.ToString();
                                        var Tenloi = ws.Cells[i, 3].Value == null ? "" : ws.Cells[i, 3].Value.ToString();
                                        var idcha = ws.Cells[i, 4].Value == null ? 0 : Convert.ToInt32(ws.Cells[i, 4].Value);
                                        var namejp = ws.Cells[i, 5].Value == null ? "" : ws.Cells[i, 5].Value.ToString();

                                        var check = db.TblMasterErrors.Where(s => s.Id == id).FirstOrDefault();
                                        if (check != null)
                                        {
                                            check.Location = Location;
                                            check.ErrorName = Tenloi.Trim();
                                            check.Idcha = idcha;
                                            check.NameJp = namejp.Trim();
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var it = new TblMasterError();
                                            //  it.Id = id;
                                            it.Location = Location;
                                            it.ErrorName = Tenloi.Trim();
                                            it.Idcha = idcha;
                                            it.NameJp = namejp.Trim();
                                            db.TblMasterErrors.Add(it);
                                            db.SaveChanges();

                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }
                            else if (idchon == "9")
                            {
                                ExcelWorksheet ws9 = package.Workbook.Worksheets["Tansuat"];
                                if (ws9 != null)
                                {
                                    for (int r = 2; r <= ws9.Dimension.End.Row; r++)
                                    {
                                        for (int c = 2; c <= ws9.Dimension.End.Column; c++)
                                        {
                                            var check = ws9.Cells[r, c].Value == null ? "" : ws9.Cells[r, c].Value.ToString();
                                            if (check == "1")
                                            {
                                                int idlocation = ws9.Cells[r, 1].Value == null ? 0 : Convert.ToInt32(ws9.Cells[r, 1].Value);
                                                int idtansuat = ws9.Cells[1, c].Value == null ? 0 : Convert.ToInt32(ws9.Cells[1, c].Value);

                                                var checkex = db.TblLocationTansuats.Where(s => s.IdLocationc == idlocation && s.IdTansuat == idtansuat).FirstOrDefault();
                                                if (checkex == null)
                                                {
                                                    TblLocationTansuat ts = new TblLocationTansuat();
                                                    ts.IdLocationc = idlocation;


                                                    ts.IdTansuat = idtansuat;

                                                    db.TblLocationTansuats.Add(ts);
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                    TempData["SuccessMess"] = "Hệ thống import thành công!";
                                }
                            }







                        }
                    }
                }
                else
                {
                    TempData["ErrorMess"] = "Chưa chọn File Import!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMess"] = "Chi tiết lỗi: " + ex.Message;
            }
            return RedirectToAction("Show");
        }

        //Export template import Item - Tiêu chuẩn ********************************************************
        [HttpPost]
        public IActionResult Export(ImportViewModel model)
        {
            try
            {
                var idchon = model.SelectedTem;
                using (var package = new ExcelPackage())
                {
                    if (idchon == "1")
                    {
                        //Master Item
                        var ws5 = package.Workbook.Worksheets.Add("Item_Master");
                        ws5.Cells[1, 1].Value = "Item Code";
                        ws5.Cells[1, 2].Value = "Name";
                        ws5.Cells[1, 3].Value = "Type";
                        ws5.Cells[1, 4].Value = "Unit";
                        ws5.Cells.AutoFitColumns();

                    }
                    else if (idchon == "2")
                    {
                        //Master Item Location
                        var ws5 = package.Workbook.Worksheets.Add("Item_Location");
                        ws5.Cells[1, 1].Value = "ID";
                        ws5.Cells[1, 2].Value = "Item Code";
                        ws5.Cells[1, 3].Value = "Mã Location";
                        ws5.Cells.AutoFitColumns();

                    }
                    else if (idchon == "3")
                    {
                        #region Master TC
                        var ws1 = package.Workbook.Worksheets.Add("Master_TC");

                        ws1.Cells[1, 1].Value = "ID";
                        ws1.Cells[1, 2].Value = "Tên tiêu chuẩn";
                        ws1.Cells[1, 3].Value = "Remark";
                        ws1.Cells[1, 4].Value = "TC Máy";

                        var load = db.TblMasterTcs.ToList();
                        int row1 = 2;
                        foreach (var it in load)
                        {
                            ws1.Cells[row1, 1].Value = it.IdTc;
                            ws1.Cells[row1, 2].Value = it.TenTieuchuan;
                            ws1.Cells[row1, 3].Value = it.Remark;
                            ws1.Cells[row1, 4].Value = it.TcMay;
                            row1++;
                        }
                        ws1.Cells.AutoFitColumns();
                        #endregion
                    }
                    else if (idchon == "4")
                    {
                        #region Detail tc
                        var ws2 = package.Workbook.Worksheets.Add("TC_detail");

                        ws2.Cells[1, 1].Value = "ID";
                        ws2.Cells[1, 2].Value = "Tên tiêu chuẩn";
                        ws2.Cells[1, 3].Value = "Mô tả";
                        ws2.Cells[1, 4].Value = "IDtc";
                        ws2.Cells[1, 5].Value = "Giá trị (text)";
                        ws2.Cells[1, 6].Value = "Giá trị (int)";
                        ws2.Cells[1, 7].Value = "Giá trị (decimal)";
                        ws2.Cells[1, 8].Value = "Unit";
                        ws2.Cells[1, 9].Value = "Giá trị";

                        int row2 = 2;
                        var load1 = db.TblDetailTcs.ToList();
                        foreach (var it in load1)
                        {
                            ws2.Cells[row2, 1].Value = it.IdDetail;
                            ws2.Cells[row2, 2].Value = it.TenTc;
                            ws2.Cells[row2, 3].Value = it.MoTa;
                            ws2.Cells[row2, 4].Value = it.IdTc;
                            ws2.Cells[row2, 5].Value = it.ValueText;
                            ws2.Cells[row2, 6].Value = it.ValueInt;
                            ws2.Cells[row2, 7].Value = it.ValueDecimal;
                            ws2.Cells[row2, 8].Value = it.Unit;
                            ws2.Cells[row2, 9].Value = it.ValueUnit;
                            row2++;
                        }
                        ws2.Cells.AutoFitColumns();
                        #endregion
                    }
                    else if (idchon == "5")
                    {
                        // Tiêu chuẩn thông số
                        #region Detail item - tc
                        var ws3 = package.Workbook.Worksheets.Add("Item_TC_Tem");

                        ws3.Cells[1, 1].Value = "ID";
                        ws3.Cells[1, 2].Value = "Item Code";
                        ws3.Cells[1, 3].Value = "ID tiêu chuẩn";
                        ws3.Cells[1, 4].Value = "Tên tiêu chuẩn";
                        ws3.Cells[1, 5].Value = "Mô tả";
                        ws3.Cells[1, 6].Value = "TC cha";
                        ws3.Cells[1, 7].Value = "Giá trị (text)";
                        ws3.Cells[1, 8].Value = "Giá trị (int)";
                        ws3.Cells[1, 9].Value = "Giá trị (decimal)";
                        ws3.Cells[1, 10].Value = "Unit";

                        int row3 = 2;
                        var load3 = (from s in db.TblItemValTcs
                                     join d in db.TblDetailTcs on s.IdValTc equals d.IdDetail
                                     select new
                                     {
                                         s.Id,
                                         s.ItemCode,
                                         s.IdValTc,
                                         d.TenTc,
                                         d.MoTa,
                                         d.IdTc,
                                         d.ValueText,
                                         d.ValueInt,
                                         d.ValueDecimal,
                                         d.Unit
                                     }).ToList();
                        foreach (var it in load3)
                        {
                            ws3.Cells[row3, 1].Value = it.Id;
                            ws3.Cells[row3, 2].Value = it.ItemCode;
                            ws3.Cells[row3, 3].Value = it.IdValTc;
                            ws3.Cells[row3, 4].Value = it.TenTc;
                            ws3.Cells[row3, 5].Value = it.MoTa;

                            ws3.Cells[row3, 6].Value = it.IdTc;
                            ws3.Cells[row3, 7].Value = it.ValueText;
                            ws3.Cells[row3, 8].Value = it.ValueInt;
                            ws3.Cells[row3, 9].Value = it.ValueDecimal;
                            ws3.Cells[row3, 10].Value = it.Unit;
                            row3++;
                        }
                        ws3.Cells.AutoFitColumns();
                        #endregion

                        #region Template export item - tc
                        var ws = package.Workbook.Worksheets.Add("Item_TC_ThongSo");
                        ws.Row(2).Hidden = true;
                        ws.Row(3).Hidden = true;
                        ws.Row(1).Height = 30;
                        ws.Row(4).Height = 25;
                        // Tiêu đề
                        ws.Cells[1, 1, 4, 1].Merge = true;
                        ws.Cells[1, 1].Value = "Mã Item";
                        ws.Cells[1, 1].Style.Font.Bold = true;
                        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        //var listtc = db.TblLocationTcs.Where(s => s.Location == "01050").ToList();
                        var listtc = db.TblMasterTcs.Where(s => s.TcMay == false).ToList();
                        int cot = 2;
                        foreach (var tc in listtc)
                        {
                            var tcdetail = db.TblDetailTcs.Where(s => s.IdTc == tc.IdTc).ToList();
                            var tentc = db.TblMasterTcs.Where(s => s.IdTc == tc.IdTc).FirstOrDefault();
                            if (tentc != null)
                            {
                                int cotend = cot + (tcdetail.Count - 1);
                                if (cot <= cotend)
                                {
                                    ws.Cells[1, cot, 1, cotend].Merge = true;
                                    ws.Cells[1, cot].Value = tentc.TenTieuchuan;
                                    ws.Cells[1, cot].Style.Font.Bold = true;
                                    ws.Cells[1, cot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[1, cot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    ws.Cells[1, cot].Style.WrapText = true;
                                    ws.Cells[1, cot].AutoFitColumns();
                                }
                            }
                            foreach (var de in tcdetail)
                            {
                                if (de.IdTc != null)
                                {
                                    ws.Cells[2, cot].Value = de.IdTc;
                                }
                                else
                                {
                                    ws.Cells[2, cot].Value = 0;
                                }
                                ws.Cells[3, cot].Value = de.IdDetail;
                                ws.Cells[4, cot].Value = de.ValueText;
                                ws.Cells[4, cot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[4, cot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                if (de.ValueInt != 0)
                                {
                                    ws.Cells[4, cot].Value += de.ValueInt.ToString();
                                }
                                if (de.ValueDecimal > 0)
                                {
                                    ws.Cells[4, cot].Value += Convert.ToDecimal(de.ValueDecimal).ToString("G29");
                                }
                                ws.Cells[4, cot].Value += de.ValueUnit;
                                ws.Cells[4, cot].Value += de.Unit;

                                cot++;
                            }
                            for (int col = 1; col <= ws.Dimension.End.Column; col++)
                            {
                                for (int row = 1; row <= 4; row++)
                                {
                                    var cell = ws.Cells[row, col];
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        ws.Columns.Width = 7;
                        ws.Column(1).Width = 11;
                        // ws.Cells.AutoFitColumns();
                        #endregion
                    }
                    else if (idchon == "6")
                    {
                        //Master Item Location
                        var ws5 = package.Workbook.Worksheets.Add("Master_May");
                        ws5.Cells[1, 1].Value = "Mã máy";
                        ws5.Cells[1, 2].Value = "Tên Máy";
                        ws5.Cells[1, 3].Value = "Mã Location";
                        ws5.Cells.AutoFitColumns();
                    }
                    else if (idchon == "7")
                    {
                        //Tiêu chuẩn máy
                        #region Template export item - tc - Máy
                        var ws = package.Workbook.Worksheets.Add("Item_TC_May");
                        ws.Row(2).Hidden = true;
                        ws.Row(3).Hidden = true;
                        ws.Row(1).Height = 30;
                        ws.Row(4).Height = 25;
                        // Tiêu đề
                        ws.Cells[1, 1, 4, 1].Merge = true;
                        ws.Cells[1, 1].Value = "Mã Máy";
                        ws.Cells[1, 1].Style.Font.Bold = true;
                        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        //var listtc = db.TblLocationTcs.Where(s => s.Location == "01050").ToList();
                        var listtc = db.TblMasterTcs.Where(s => s.TcMay == true).ToList();
                        int cot = 2;
                        foreach (var tc in listtc)
                        {
                            var tcdetail = db.TblDetailTcs.Where(s => s.IdTc == tc.IdTc).ToList();
                            var tentc = db.TblMasterTcs.Where(s => s.IdTc == tc.IdTc).FirstOrDefault();
                            if (tentc != null)
                            {
                                int cotend = cot + (tcdetail.Count - 1);
                                if (cot <= cotend)
                                {
                                    ws.Cells[1, cot, 1, cotend].Merge = true;
                                    ws.Cells[1, cot].Value = tentc.TenTieuchuan;
                                    ws.Cells[1, cot].Style.Font.Bold = true;


                                    ws.Cells[1, cot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[1, cot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    ws.Cells[1, cot].Style.WrapText = true;
                                    ws.Cells[1, cot].AutoFitColumns();
                                }
                            }
                            foreach (var de in tcdetail)
                            {
                                if (de.IdTc != null)
                                {
                                    ws.Cells[2, cot].Value = de.IdTc;
                                }
                                else
                                {
                                    ws.Cells[2, cot].Value = 0;
                                }
                                ws.Cells[3, cot].Value = de.IdDetail;
                                ws.Cells[4, cot].Value = de.ValueText;
                                ws.Cells[4, cot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[4, cot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                if (de.ValueInt != 0)
                                {
                                    ws.Cells[4, cot].Value += de.ValueInt.ToString();
                                }
                                if (de.ValueDecimal > 0)
                                {
                                    ws.Cells[4, cot].Value += Convert.ToDecimal(de.ValueDecimal).ToString("G29");
                                }
                                ws.Cells[4, cot].Value += de.ValueUnit;
                                ws.Cells[4, cot].Value += de.Unit;

                                cot++;
                            }
                            for (int col = 1; col <= ws.Dimension.End.Column; col++)
                            {
                                for (int row = 1; row <= 4; row++)
                                {
                                    var cell = ws.Cells[row, col];
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        ws.Columns.Width = 7;
                        ws.Column(1).Width = 11;
                        // ws.Cells.AutoFitColumns();
                        #endregion
                    }
                    else if (idchon == "8")
                    {
                        var ws5 = package.Workbook.Worksheets.Add("Master_Error");
                        ws5.Cells[1, 1].Value = "ID";
                        ws5.Cells[1, 2].Value = "Location";
                        ws5.Cells[1, 3].Value = "Tên lỗi";
                        ws5.Cells[1, 4].Value = "ID cha";
                        ws5.Cells[1, 5].Value = "Tên tiếng nhật";
                        ws5.Cells.AutoFitColumns();
                    }

                    byte[] fileContents = package.GetAsByteArray();
                    return File(
                        fileContents: fileContents,
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: "data.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        //--------------------------------------------------- Chức năng Báo cáo sản lượng theo tháng-----------------------------------------------
        [HttpPost]
        public IActionResult BC_month(DateTime tdate)
        {
            try
            {
                DateTime start = new DateTime(tdate.Year, tdate.Month, 1);
                var filePath = Path.Combine(_environment.ContentRootPath, "OtherTemplates", "Baocao.xlsx");
                var fileInfo = new FileInfo(filePath);

                using (var package = new ExcelPackage(fileInfo))
                {
                    //var ws = package.Workbook.Worksheets.Add("Baocao_Month");
                    var ws = package.Workbook.Worksheets["Export"];
                    ws.Cells["C4"].Value = start.ToString("MM");
                    ws.Cells["E4"].Value = start.ToString("yyyy");

                    DateTime end = start.AddMonths(1).AddDays(-1);
                    var loadstock = _clst.Output_WO_loc(start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), "01080");

                    int row1 = 10;
                    int stt = 1;
                    foreach (DataRow row in loadstock.Rows)
                    {
                        string loc = row["process"].ToString() ?? "";
                        string item = row["productcode"].ToString() ?? "";
                        string lot = row["lotno"].ToString() ?? "";
                        int qty_order = Convert.ToInt32(row["OrderQty"] ?? 0);
                        int output = Convert.ToInt32(row["outputqty"] ?? 0);

                        if (loc == "01075")
                        {
                            ws.Cells[row1, 4].Value = "K";
                        }
                        ws.Cells[row1, 1].Value = stt;
                        ws.Cells[row1, 2].Value = item;
                        ws.Cells[row1, 5].Value = qty_order;
                        ws.Cells[row1, 6].Value = lot;
                        ws.Cells[row1, 7].Value = output;

                        stt++;
                        row1++;
                    }

                    //ws.Cells.AutoFitColumns();
                    byte[] fileContents = package.GetAsByteArray();
                    return Ok(new { filePath = fileContents, excelName = "BC_SanluongTH.xlsx" });

                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public IActionResult BC_nvl(DateTime tdate, bool cbsave)
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "OtherTemplates", "hsd.xlsx");

                var fileInfo = new FileInfo(filePath);

                using (var package = new ExcelPackage(fileInfo))
                {
                    var ws = package.Workbook.Worksheets["NVL"];

                    DateTime start = new DateTime(tdate.Year, tdate.Month, 1);
                    DateTime end = tdate.AddMonths(1).AddDays(-1);
                    DateTime startqk = tdate.AddMonths(-1);
                    DateTime endqk = end.AddMonths(-1);

                    for (int i = 4; i <= ws.Dimension.Rows; i += 1)
                    {
                        var itemcode = ws.Cells[i, 1].Value;
                        if (itemcode != null)
                        {
                            //xác định số lượng dòng cần thêm
                            var stock = _clst.InventoryQty_bct(itemcode.ToString());
                            var stockqk = db.TblStocks.Where(s => s.ItemCode == itemcode.ToString() && s.Date >= startqk && s.Date <= endqk).ToList();
                            var trans = dbl.TransGws.Where(s => s.ItemNumber == itemcode.ToString() && s.TranType == "RCT-TR" && s.Effdate >= start && s.Effdate <= end).OrderBy(s => s.Effdate).ToList();
                            int countStock = stock.Rows.Count; //stock != null ? 1 : 0;
                            int countStockqk = stockqk.Count;
                            int countTrans = trans.Count;

                            int maxCount = Math.Max(countStockqk, Math.Max(countTrans, countStock));
                            if (maxCount > 1)
                            {
                                ws.InsertRow(i, maxCount - 1);
                                int them = maxCount - 1;
                                ws.Cells[i, 1].Value = ws.Cells[i + them, 1].Value;
                                ws.Cells[i, 2].Value = ws.Cells[i + them, 2].Value;
                                ws.Cells[i, 3].Value = ws.Cells[i + them, 3].Value;
                            }
                            //Lấy stock cuối tháng này
                            if (stock != null)
                            {
                                int s1 = i;
                                foreach (DataRow row in stock.Rows)
                                {
                                    string lotno = row["LOTNO"].ToString() ?? "";
                                    int qtystock = Convert.ToInt32(row["QTY"] ?? 0);
                                    ws.Cells[s1, 21].Value = lotno;
                                    ws.Cells[s1, 22].Value = qtystock;
                                    if (cbsave == true)
                                    {
                                        var sua = db.TblStocks.Where(s => s.ItemCode == itemcode.ToString()
                                        && s.Lotno == lotno && s.Date == start).FirstOrDefault();
                                        if (sua != null)
                                        {
                                            sua.QtyStock = qtystock;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            TblStock st = new TblStock();
                                            st.ItemCode = itemcode.ToString();
                                            st.Lotno = lotno;
                                            st.QtyStock = qtystock;
                                            st.Date = start;
                                            db.TblStocks.Add(st);
                                            db.SaveChanges();
                                        }
                                    }
                                    s1++;
                                }
                            }

                            //ghi thông tin sử dụng trong tháng
                            int themdong = 0;
                            for (int sd = i; sd <= ws.Dimension.Rows; sd += 1)
                            {
                                if (ws.Cells[sd, 21].Text == "*")
                                    break;
                                int xh = sd;
                                int xk = sd;
                                var lot = ws.Cells[sd, 21].Value;
                                if (lot != null)
                                {
                                    var layloc = _clst.Bang_qlsl(itemcode.ToString(), lot.ToString(), start.ToString("dd/MM/yyyy HH:mm:ss"));
                                    foreach (DataRow row in layloc.Rows)
                                    {
                                        string loc = row["Location"].ToString() ?? "";
                                        var layxuatdung = _clst.Xuat_dung(itemcode.ToString(), lot.ToString(), loc, start.ToString("dd/MM/yyyy HH:mm:ss"), end.ToString("dd/MM/yyyy HH:mm:ss"));
                                        //thêm dòng
                                        int countxuatdung = layxuatdung?.Rows.Count ?? 1;
                                        if (countxuatdung > 1)
                                        {
                                            ws.InsertRow(xh + 1, countxuatdung);
                                            themdong = themdong + (countxuatdung);
                                        }
                                        foreach (DataRow row2 in layxuatdung.Rows)
                                        {
                                            string Thoi_Gian = row2["Thoi_Gian"].ToString() ?? "";
                                            string lotnvl = row2["LO_NVL"].ToString() ?? "";
                                            string lotbtp = row2["LO_BTP"].ToString() ?? "";
                                            string mabtp = row2["MA_BTP"].ToString() ?? "";
                                            int luongsd = Convert.ToInt32(row2["LUONG_SU_DUNG"] ?? 0);
                                            int luongloi = Convert.ToInt32(row2["LUONG_LOI"] ?? 0);

                                            int spaceIndex = Thoi_Gian.IndexOf(' ');
                                            ws.Cells[xh, 10].Value = spaceIndex != -1 ? Thoi_Gian.Substring(0, spaceIndex) : Thoi_Gian;
                                            ws.Cells[xh, 12].Value = lotnvl;
                                            ws.Cells[xh, 13].Value = lotbtp;
                                            ws.Cells[xh, 14].Value = mabtp;
                                            ws.Cells[xh, 15].Value = luongsd;
                                            ws.Cells[xh, 16].Value = luongsd - luongloi;
                                            ws.Cells[xh, 17].Value = luongloi;

                                            xh++;
                                        }
                                        //Lấy xuất khác
                                        var layxuatkhac = _clst.Xuat_khac(itemcode.ToString(), lot.ToString(), loc, start.ToString("dd/MM/yyyy HH:mm:ss"), end.ToString("dd/MM/yyyy HH:mm:ss"));
                                        foreach (DataRow row3 in layxuatkhac.Rows)
                                        {
                                            string locxk = row3["Loc"].ToString() ?? "";
                                            var checkloc = db.TblLocations.Where(s => s.LocationCode == locxk && s.XuatKhac == true).FirstOrDefault();
                                            if (checkloc == null)
                                            {
                                                int luongsd = Convert.ToInt32(row3["So_Luong"] ?? 0);
                                                ws.Cells[xk, 19].Value = luongsd;
                                                ws.Cells[xk, 20].Value = locxk;
                                                xk++;
                                            }
                                        }
                                    }
                                }
                            }
                            //ghi tồn tháng trước
                            int qk = i;
                            foreach (var a in stockqk)
                            {
                                ws.Cells[qk, 4].Value = a.Lotno;
                                ws.Cells[qk, 5].Value = a.QtyStock;
                                qk++;
                            }

                            //Ghi lượng nhập trong tháng                                                      
                            if (trans != null)
                            {
                                int tran = i;
                                foreach (var it in trans)
                                {
                                    ws.Cells[tran, 6].Value = it.Effdate;
                                    ws.Cells[tran, 7].Value = it.Order;
                                    ws.Cells[tran, 8].Value = it.Lot;
                                    ws.Cells[tran, 9].Value = it.ChangeQty;
                                    tran++;
                                }
                            }
                            ////Gộp ô item 
                            int rowgop = (i + maxCount + themdong) - 1;
                            if (rowgop > i)
                            {
                                ws.Cells[i, 1, rowgop, 1].Merge = true;
                                ws.Cells[i, 2, rowgop, 2].Merge = true;
                                ws.Cells[i, 3, rowgop, 3].Merge = true;
                            }
                            i = i + maxCount + themdong;
                        }

                    }



                    byte[] fileContents = package.GetAsByteArray();
                    return Ok(new { filePath = fileContents, excelName = "Baocao_THSD.xlsx" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]

        public IActionResult BC_stock(IFormFile FileUpload1)
        {
            try
            {
                // var filePath = Path.Combine(_environment.ContentRootPath, "OtherTemplates", "Tyledat.xlsx");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new MemoryStream())
                {
                    if (FileUpload1 != null && FileUpload1.Length > 0)
                    {
                        FileUpload1.CopyTo(stream);
                    }
                    else
                    {
                        var filePath = Path.Combine(_environment.ContentRootPath, "OtherTemplates", "Tyledat.xlsx");
                        stream.Write(System.IO.File.ReadAllBytes(filePath), 0, (int)new FileInfo(filePath).Length);
                    }
                    stream.Position = 0;

                    // var fileInfo = new FileInfo(filePath);
                    using (var package = new ExcelPackage(stream))
                    {
                        var ws1 = package.Workbook.Worksheets["2.Số lượng BTP"];
                        if (ws1 == null)
                        {
                            for (int c = 4; c < 17; c++)
                            {
                                var startte = ws1.Cells[3, c].Value;
                                start = (new DateTime(1900, 1, 1).AddDays(Convert.ToUInt32(startte) - 2)).ToString("yyyy-MM-dd");
                                DateTime end = Convert.ToDateTime(start);

                                for (int r = 5; r <= ws1.Dimension.Rows; r++)
                                {
                                    string item = ws1.Cells[r, 2].Text;
                                    var stockbtp = db.TblStocks.Where(s => s.ItemCode == item && s.Date == end).FirstOrDefault();
                                    if (stockbtp != null)
                                    {
                                        ws1.Cells[r, c].Value = stockbtp.QtyStock;
                                    }
                                }
                            }
                        }
                        //Sheet 2
                        var ws2 = package.Workbook.Worksheets["4. Số lượng TP"];
                        for (int r = 5; r <= ws2.Dimension.Rows; r++)
                        {
                            string textloc = ws2.Cells[r, 3].Text;
                            if (location.Contains(textloc))
                            { }
                            else
                            {
                                location += "'" + textloc + "',";
                            }
                        }

                        for (int c = 4; c < 17; c++)
                        {
                            var startte = ws2.Cells[3, c].Value;
                            start = (new DateTime(1900, 1, 1).AddDays(Convert.ToUInt32(startte) - 2)).ToString("yyyy-MM-dd");
                            string end = (Convert.ToDateTime(start).AddMonths(1).AddDays(-1)).ToString("yyyy-MM-dd");

                            var loadstock = _clst.Output_WO_Sumloc(start, end, location.TrimEnd(','));
                            if (loadstock.Rows.Count > 0)
                            {
                                foreach (DataRow row in loadstock.Rows)
                                {
                                    for (int r = 5; r <= ws2.Dimension.Rows; r++)
                                    {
                                        string item = row["item"].ToString() ?? "";
                                        string loc = row["loc"].ToString() ?? "";

                                        if (item == ws2.Cells[r, 2].Value.ToString() && loc == ws2.Cells[r, 3].Value.ToString())
                                        {
                                            int qty = Convert.ToInt32(row["output"] ?? 0);
                                            ws2.Cells[r, c].Value = qty;
                                        }
                                    }
                                }
                            }
                        }

                        //Sheet 3
                        var ws3 = package.Workbook.Worksheets["8.NVL tồn cuối kỳ"];
                        for (int c = 4; c < 17; c++)
                        {
                            var startte = ws3.Cells[3, c].Value;
                            start = (new DateTime(1900, 1, 1).AddDays(Convert.ToUInt32(startte) - 2)).ToString("yyyy-MM-dd");
                            DateTime end = Convert.ToDateTime(start);

                            for (int r = 4; r <= ws3.Dimension.Rows; r++)
                            {
                                string item = ws3.Cells[r, 1].Text;
                                var stockbtp = db.TblStocks.Where(s => s.ItemCode == item && s.Date == end).FirstOrDefault();
                                if (stockbtp != null && stockbtp.QtyStock != 0)
                                {
                                    ws3.Cells[r, c].Value = stockbtp.QtyStock;
                                }
                            }
                        }

                        //Sheet NVL
                        var ws4 = package.Workbook.Worksheets["9.NVL nhập từ kho"];
                        for (int c = 4; c < 16; c++)
                        {
                            var startte = ws4.Cells[3, c].Value;
                            start = (new DateTime(1900, 1, 1).AddDays(Convert.ToUInt32(startte) - 2)).ToString("yyyy-MM-dd");
                            DateTime start1 = Convert.ToDateTime(start);
                            DateTime end = (Convert.ToDateTime(start).AddMonths(1).AddDays(-1));

                            for (int r = 4; r <= ws4.Dimension.Rows; r++)
                            {
                                string item = ws4.Cells[r, 1].Text;
                                var trans = dbl.TransGws.Where(s => s.ItemNumber == item
                                && s.TranType == "RCT-TR" && s.Effdate >= start1 && s.Effdate <= end).ToList().Sum(s => s.ChangeQty ?? 0);
                                if (trans != 0)
                                {
                                    ws4.Cells[r, c].Value = trans;
                                }
                            }
                        }

                        byte[] fileContents = package.GetAsByteArray();
                        return Ok(new { filePath = fileContents, excelName = "Baocao_Tyledat.xlsx" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        public IActionResult Save_st(string titem, string tlot, decimal tqty, string tdate)
        {
            try
            {
                DateTime datechon = Convert.ToDateTime(tdate);
                //DateTime datect = new DateTime(datect.Year, datect.Month, DateTime.DaysInMonth(datect.Year, datect.Month));
                DateTime datedt = new DateTime(datechon.Year, datechon.Month, 1);

                var sua = db.TblStocks.Where(s => s.ItemCode == titem && s.Lotno == tlot && s.Date == datedt).FirstOrDefault();
                if (sua != null)
                {
                    sua.QtyStock = tqty;
                    db.SaveChanges();
                }
                else
                {
                    TblStock st = new TblStock();
                    st.ItemCode = titem;
                    st.Lotno = tlot;
                    st.QtyStock = tqty;
                    st.Date = datedt;
                    db.TblStocks.Add(st);
                    db.SaveChanges();
                }
                return Ok(new { message = "Hệ thống lưu thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        public IActionResult Del_st(string titem, string tlot, string tdate)
        {
            try
            {
                DateTime datechon = Convert.ToDateTime(tdate);
                DateTime datedt = new DateTime(datechon.Year, datechon.Month, 1);
                var xoa = db.TblStocks.Where(s => s.ItemCode == titem && s.Lotno == tlot && s.Date == datedt).ToList();
                if (xoa.Count > 0)
                {
                    db.TblStocks.RemoveRange(xoa);
                    db.SaveChanges();
                }
                else
                {
                    return Ok(new { message = "Không có dữ liệu tồn kho" });
                }
                return Ok(new { message = "Hệ thống xóa thành công" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
