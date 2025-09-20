using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;

namespace MPLUS_GW_WebCore
{
    public class ExcelData
    {
        public DataTable ReadExcel(string filePath, string worksheetName)
        {
            // Kiểm tra xem tệp có tồn tại hay không
            if (!File.Exists(filePath)) 
            { 
                throw new FileNotFoundException("Tệp Excel không tồn tại."); 
            } 

          
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[worksheetName];

            if (worksheet.Dimension == null) 
            { 
                throw new Exception("Worksheet không chứa dữ liệu."); 
            }

            var dataTable = new DataTable();
            foreach (var headerCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                dataTable.Columns.Add(headerCell.Text);
            }
            for (var rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var row = dataTable.NewRow();
                for (var colNum = 1; colNum <= worksheet.Dimension.End.Column; colNum++)
                {
                    row[colNum - 1] = worksheet.Cells[rowNum, colNum].Text;
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}
