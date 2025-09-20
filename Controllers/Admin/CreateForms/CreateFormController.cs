using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPLUS_GW_WebCore.Controllers.Materials;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using System.Data;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace MPLUS_GW_WebCore.Controllers.Admin.CreateForms
{
    public class CreateFormController : Controller
    {
        private readonly MplusGwContext _context;
        public CreateFormController(MplusGwContext context)
        {
            _context = context;
        }
        [Route("/admin/templates")]
        public IActionResult Index()
        {
            var getForms = _context.TblChecksheetForms
                .Select(s => new
                {
                    s.FormId,
                    s.FormName,
                    s.FormOrder,
                    s.ChecksheetVersionId,
                })
                .OrderBy(x => x.ChecksheetVersionId)
                .ThenBy(x => x.FormOrder)
                .ToList();
            List<RenderAllFormData> renderAllFormData = new();
            foreach (var item in getForms)
            {
                var checksheetName = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == item.ChecksheetVersionId)
                    .Select(s => s.FileName)
                    .FirstOrDefault();
                var versionChecksheet = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == item.ChecksheetVersionId)
                    .Select(s => s.VersionNumber)
                    .FirstOrDefault();
                var positionWorkingCode = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == item.ChecksheetVersionId)
                    .Select(s => s.PositionWorkingCode)
                    .FirstOrDefault();
                var positionWorkName = _context.TblLocationCs
                    .Where(x => x.LocationCodeC == positionWorkingCode)
                    .Select(s => s.LocationNameC)
                    .FirstOrDefault();
                renderAllFormData.Add(new RenderAllFormData
                {
                    FormName = item.FormName,
                    IdForm = item.FormId,
                    ChecksheetCode = checksheetName,
                    VersionNumber = versionChecksheet.ToString(),
                    PositionWorkName = positionWorkName
                });
            }
            ViewData["ListAllFormData"] = renderAllFormData.ToList();
            return View();
        }

        [Route("/admin/templates/add")]
        public IActionResult Add()
        {
            var checksheetsWithoutForms = _context.TblChecksheetsUploads
                .Select(s => new ListAllCheckSheets
                {
                    IdCheckSheet = s.ChecksheetId,
                    ChecksheetVerId = s.CurrentVersionId,
                    ChecksheetCode = s.ChecksheetCode,
                    VersionNumber = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == s.CurrentVersionId)
                    .Select(s => s.VersionNumber)
                    .FirstOrDefault().ToString(),
                }).ToList();
            ViewData["AllCheckSheets"] = checksheetsWithoutForms;
            return View();
        }

        [Route("/admin/templates/edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewData["FormId"] = id;

            var getChecksheetVerId = _context.TblChecksheetForms
                .Where(x => x.FormId == id)
                .Select(s => s.ChecksheetVersionId)
                .FirstOrDefault();
            ViewData["ChecksheetVerId"] = getChecksheetVerId;

            var checksheetsWithoutForms = _context.TblChecksheetsUploads   
                .Select(s => new ListAllCheckSheets
                {
                    IdCheckSheet = s.ChecksheetId,
                    ChecksheetVerId = s.CurrentVersionId,
                    ChecksheetCode = s.ChecksheetCode,
                    VersionNumber = _context.TblChecksheetVersions
                    .Where(x => x.ChecksheetVersionId == s.CurrentVersionId)
                    .Select(s => s.VersionNumber)
                    .FirstOrDefault().ToString(),
                }).ToList();
            ViewData["AllCheckSheets"] = checksheetsWithoutForms;

            var positionWorkName = _context.TblChecksheetForms
                .Where(x => x.ChecksheetVersionId == getChecksheetVerId)
                .Select(s => s.FormPosition)
                .FirstOrDefault();
            ViewData["PositionWorkName"] = positionWorkName;

            var getFormName = _context.TblChecksheetForms
                .Where(x => x.FormId == id)
                .Select(s => s.FormName).FirstOrDefault();
            ViewData["FormName"] = getFormName;

            var getFormOrder = _context.TblChecksheetForms
                .Where(x => x.FormId == id)
                .Select(s => s.FormOrder).FirstOrDefault();
            ViewData["FormOrder"] = getFormOrder;

            var getFormType = _context.TblChecksheetForms
                .Where(x => x.FormId == id)
                .Select(s => s.IsRepeatable).FirstOrDefault();
            ViewData["IsRepeatable"] = getFormType;

            var formDataCreated = _context.TblChecksheetForms
                .Where(x => x.FormId == id)
                .Select(s => new
                {
                    s.JsonFormData,
                    s.FormType,
                }).FirstOrDefault();
            ViewData["FormDataCreated"] = JsonConvert.SerializeObject(formDataCreated);

            var getFormDataMapping = _context.TblChecksheetFormFields
                .Where(x => x.FormId == id)
                .OrderBy(x => x.SectionIndex)
                .Select(s => new
                {
                    s.FieldName,
                    s.LabelText,
                    s.SectionId,
                    s.ColClass,
                    s.ColIndex,
                    s.ColSpan,
                    s.RowIndex,
                    s.RowSpan,
                    s.StartCell,
                    s.IsHidden,
                    s.IsMerged,
                    s.InputType,
                    s.ElementType,
                    s.ElementId,
                    s.SectionIndex,
                    s.DataSource,
                    s.IsTotals,
                }).ToArray();

            ViewData["FormDataMapping"] = JsonConvert.SerializeObject(getFormDataMapping.GroupBy(x => x.SectionId)
                .Select(s => new
                {
                    sectionId = s.Key,
                    colInRow = 12 / GetLastNumberFromColClass(s.FirstOrDefault()?.ColClass ?? ""),
                    formMapping = s.ToList()
                }).ToList());

            return View();
        }

        [HttpPost]
        public IActionResult Delete([FromBody] RequestDeleteForm requestDeleteForm)
        {
            var getForm = _context.TblChecksheetForms.Where(x => x.FormId == requestDeleteForm.FormId).FirstOrDefault();
            if (getForm == null)
            {
                return StatusCode(500, new { message = "Không có form nhập này. Vui lòng kiểm tra lại" });
            } else
            {
                _context.TblChecksheetForms.Remove(getForm);
            } 
            _context.SaveChanges();
            return Ok(new { message = "Xóa thành công" });
        }

        public static int? GetLastNumberFromColClass(string colClass)
        {
            if (string.IsNullOrEmpty(colClass))
            {
                return null;
            }
 
            int lastHyphenIndex = colClass.LastIndexOf('-');
            if (lastHyphenIndex != -1 && lastHyphenIndex < colClass.Length - 1)
            {
                string numberString = colClass.Substring(lastHyphenIndex + 1);
                if (int.TryParse(numberString, out int number))
                {
                    return number;
                }
            }
            return null;
        }

        [HttpPost]
        public IActionResult GetPositionChecksheet([FromBody] RequestDataSelect dataSelect)
        {
            if(dataSelect == null)
            {
                return BadRequest(new { message = "Không có request được gửi lên" });
            }

            var positionWorkCode = _context.TblChecksheetVersions
                .Where(x => x.ChecksheetVersionId == dataSelect.ChecksheetVerId)
                .Select(s => s.PositionWorkingCode).FirstOrDefault() ?? "";
            List<string> listPositionCode = positionWorkCode
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            string? positionName = string.Empty;
            if(!positionWorkCode.Contains(", "))
            {
                positionName = _context.TblLocationCs
                .Where(x => x.LocationCodeC == positionWorkCode)
                .Select(s => s.LocationNameC)
                .FirstOrDefault();
            } else
            {
                positionName = string.Join(",", _context.TblLocationCs
                     .Where(x => listPositionCode.Contains(x.LocationCodeC ?? ""))
                     .Select(s => s.LocationNameC)
                     .ToArray());
            }
            return Ok(new { postionNameWorking = positionName });
        }

        [HttpPost]
        public IActionResult SaveFormTemplate([FromBody] RequestFormData requestFormData)
        {
            if (requestFormData.DataFormCreated == null)
            {
                return BadRequest(new { message = "Not Found Request" });
            }
            try
            {
                FormDataSave? formDataSave = JsonConvert.DeserializeObject<FormDataSave>(requestFormData.DataFormCreated);
                if (formDataSave != null)
                {
                    TblChecksheetForm form;
                    var listFormFields = formDataSave.FormFields;
                    var formFieldData = formDataSave.FormDataCreated;
                    var existingForm = _context.TblChecksheetForms.Where(x => x.FormId == formDataSave.FormId).FirstOrDefault();
                    if (existingForm != null)
                    {
                        existingForm.FormName = formDataSave.FormName ?? "";
                        existingForm.FormOrder = formDataSave.OrderForm;
                        existingForm.IsRepeatable = formDataSave.FormType;
                        existingForm.JsonFormData = formFieldData;

                        var existingFields = _context.TblChecksheetFormFields.Where(x => x.FormId == existingForm.FormId).ToList();
                        if(existingFields.Any())
                        {
                            _context.TblChecksheetFormFields.RemoveRange(existingFields);
                        }
                        form = existingForm;
                    } else
                    {
                        var formDataCreated = new TblChecksheetForm
                        {
                            FormName = formDataSave.FormName ?? "",
                            FormOrder = formDataSave.OrderForm,
                            IsRepeatable = formDataSave.FormType,
                            ChecksheetVersionId = formDataSave.ChecksheetVerId ?? 0,
                            IsActive = true,
                            FormPosition = formDataSave.FormPosition,
                            FormType = formDataSave.FormMode,
                            JsonFormData = formFieldData,
                        };
                        _context.TblChecksheetForms.Add(formDataCreated);
                        _context.SaveChanges();
                        form = formDataCreated;
                    }

                    //if (listFormFields != null)
                    //{
                    //    foreach (var field in listFormFields)
                    //    {

                    //        string dataSource = string.Empty;
                    //        if(field.DataSource == "")
                    //        {
                    //            dataSource = string.Empty;
                    //        } else if(field.DataSource != "" && field.DataSource.Contains('.'))
                    //        {
                    //            dataSource = field.DataSource;
                    //        } else if(field.DataSource != "" && !field.DataSource.Contains('.'))
                    //        {
                    //            dataSource = field.DataSource + "." + field.FieldName;
                    //        }
                    //        var newFieldMapping = new TblChecksheetFormField
                    //        {
                    //            FieldName = field.FieldName,
                    //            LabelText = field.Label,
                    //            StartCell = field.StartCell,
                    //            RowIndex = field.RowIndex > 0 ? field.RowIndex : 0,
                    //            ColIndex = 0,
                    //            IsMerged = field.IsMerged,
                    //            RowSpan = field.RowSpan > 0 ? field.RowSpan : 0,
                    //            ColSpan = field.ColSpan > 0 ? field.ColSpan : 0,
                    //            InputType = field.TypeInput,
                    //            IsHidden = field.IsHidden,
                    //            FormId = form.FormId,
                    //            SectionId = field.SectionId,
                    //            ColClass = field.ColClass,
                    //            ElementType = field.TypeElement,
                    //            ElementId = field.ElementId,
                    //            SectionIndex = field.SectionIndex,
                    //            DataSource = dataSource,
                    //            IsTotals = field.IsTotals
                    //        };
                    //        _context.TblChecksheetFormFields.Add(newFieldMapping);
                    //    }
                    //}
                }
                _context.SaveChanges();
                return Ok(new { message = "Lưu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }
    }

    public class ListAllTemplates
    {
        public int IdTemplate { get; set; }
        public string TemplateName { get; set; }
        public string ChecksheetCode { get; set; }
    }
    public class ListAllCheckSheets
    {
        public int IdCheckSheet { get; set; }
        public int? ChecksheetVerId { get; set; }
        public string? ChecksheetCode { get; set; }
        public string? VersionNumber { get; set; }
    }

    public class RequestDataSelect
    {
        public string JsonSelect { get; set; }
        public int JsonVersion { get; set; }
        public int ChecksheetVerId { get; set; }
    }

    public class RequestFormData
    {
        public string DataFormCreated { get; set; }
    }

    public class FormDataSave
    {
        public int? ChecksheetVerId { get; set; }
        public string? FormName { get; set; }
        public string? FormPosition { get; set; }
        public string? FormMode { get; set; }
        public bool FormType { get; set; } = false;
        public int OrderForm { get; set; }
        public int FormId { get; set; }
        public string? FormDataCreated { get; set; }
        public List<FormFieldMapping> FormFields { get; set; }
    }

    public class FormFieldMapping
    {
        public string FieldName { get; set; }
        public string Label { get; set; }
        public string StartCell { get; set; }
        public int RowIndex { get; set; } = 0;
        public int ColIndex { get; set; } = 0;
        public string TypeInput { get; set; }
        public int RowSpan { get; set; } = 0;
        public int ColSpan { get; set; } = 0;
        public bool IsMerged { get; set; }
        public bool IsHidden { get; set; }
        public bool IsTotals { get; set; }
        public string SectionId { get; set; }
        public string ColClass { get; set; }
        public string TypeElement { get; set; }
        public string ElementId { get; set; }
        public string DataSource { get; set; }
        public int SectionIndex { get; set; }
    }

    public class RequestDeleteForm
    {
        public int FormId { get; set; }
    }

    public class SectionViewModel
    {
        public string SectionId { get; set; }
        public string SectionClass { get; set; }
        public string SectionStyle { get; set; }
        public List<RowViewModel> Rows { get; set; }
    }

    public class RowViewModel
    {
        public string RowClass { get; set; }
        public string RowId { get; set; }
        public string RowStyles { get; set; }
        public List<ColumnViewModel> Columns { get; set; }
    }

    public class ColumnViewModel
    {
        public string ColId { get; set; }
        public string ColClass { get; set; }
        public string ColClassParent { get; set; }
        public string ColStyles { get; set; }
        public List<ElementSectionViewModel> Elements { get; set; }
    }

    public class ElementSectionViewModel
    {
        public string ElementType { get; set; }
        public string Label { get; set; }
        public string InputName { get; set; }
        public string InputId { get; set; }
        public string InputClass { get; set; }
        public string DataRowIndex { get; set; }
        public string DataColumnStart { get; set; }
        public string DataColumnEnd { get; set; }
        public bool DataCheckMerge { get; set; }
        public string TextContent { get; set; }
        public string TagName { get; set; }
        public bool DataCheckDisplay { get; set; }
        public string ElementStyles { get; set; }
    }

    public class TemplateViewModel
    {
        public List<SectionViewModel> SectionsView { get; set; }
    }

    public class ListAllVersionChecksheet
    {
        public int VersionChecksheet { get; set; }
        public int IdChecksheet { get; set; }
    }

    public class RenderAllFormData
    {
        public string? FormName { get; set; }
        public string? ChecksheetCode { get; set; }
        public int? IdForm { get; set; }
        public string? VersionNumber { get; set; }
        public string? PositionWorkName { get; set; }
    }
}
