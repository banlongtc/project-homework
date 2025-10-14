# project-homework
Learning C#


Chuyển đến nội dung
Lịch sử đoạn chat

Bạn đã nói:
SQLQuery1.sql
Tệp
Đọc qua cơ sở dữ liệu này để xem nó có phù hợp với với vấn đề khi thực hiện sản xuất một lô trên một vị trí thao tác thuộc một công đoạn mà vị trí thao tác đó sẽ phải có từng bước thực hiện, trong đó sẽ có bước kiểm tra điều kiện. Trong kiểm tra điều kiện sẽ có các tần suất kiểm tra riêng cho từng vị trí tại bước đó để khi sản xuất sẽ phải thực hiện kiểm tra tại vị trí đó đã có điều kiện chưa hoặc khi bắt đầu ca  đã có điều kiện đầu ca chưa... nhiều loại điều kiện khác nữa. Mục đích là sẽ phải biết đã có điều kiện thiết bị tại bước đó của vị trí đó chưa thì mới được thao tác tiếp. Còn tiếp theo là xử lý bất thường ở đây sẽ có dừng toàn lô NVL hoặc dừng 1 máng của lô NVL đang thao tác hoặc dừng toàn chỉ thị thao tác và sẽ cho xử lý bất thường để quay lại sản xuất tiếp tục. Các dữ liệu sẽ được lưu lại để truy tìm nguồn gốc sau này, và lưu dữ liệu đã nhập của các form để thực hiện mapping vào file checksheet tương ứng với các form và cách form sẽ phải theo từng bước để gọi ra cho công nhân nhập dữ liệu. Các master sẽ được liên kết tương ứng với các Item và các tiêu chuẩn để khi gọi các item ra sẽ lấy được thông số cần thiết cho item đó dựa vào đó làm tiêu chuẩn so sánh với dữ liệu nhập vào. Ngoài ra trong các form đang có những trường bị ẩn và sẽ tự động thêm dữ liệu dựa trên item được đọc vào khi sản xuất
ChatGPT đã nói:
Mình đã đọc toàn bộ file cơ sở dữ liệu của bạn — cấu trúc này rất mạnh và khá đầy đủ để phục vụ hệ thống sản xuất nhiều bước, nhiều vị trí thao tác, có kiểm tra điều kiện, xử lý bất thường, và quản lý form nhập liệu — nhưng cần một số mở rộng nhỏ để đạt đúng yêu cầu bạn mô tả.

Dưới đây là phân tích chi tiết theo yêu cầu thực tế sản xuất bạn nói đến 👇

⚙️ 1. Cấu trúc hiện tại đã bao quát tốt
CSDL của bạn đã có:

Mục đích	Bảng liên quan	Nhận xét
Quản lý lô sản xuất, chia line, chia máng	tbl_DivLineProd, tbl_DivLineForLot, tbl_DivLineMCDetails, tbl_DivMaterialPrintLabels, tbl_DivMCProd	✔ Đủ để quản lý chia lô và máng NVL.
Quản lý chỉ thị và WorkOrder	tbl_WorkOrderMES, tbl_WorkOrderProcessing	✔ Có thể quản lý luồng chỉ thị đến từng vị trí.
Quản lý vị trí thao tác, máy, line, ca	tbl_MasterPosition, tbl_Location, tbl_MasterMachines, tbl_Shift, tbl_ShiftSchedules	✔ Đầy đủ để xác định công đoạn – vị trí – ca làm việc.
Quản lý form nhập liệu & version checksheet	tbl_ChecksheetForms, tbl_ChecksheetFormFields, tbl_ChecksheetVersions, tbl_ChecksheetFormEntry, tbl_ChecksheetEntryValues	✔ Phần này chính là nền tảng mapping dữ liệu nhập ra file Excel.
Lưu lịch sử và nguồn gốc dữ liệu nhập	tbl_ChecksheetEntryValueHistory, tbl_ChecksheetFormEntryHistory	✔ Rất tốt – đã hỗ trợ truy xuất nguồn gốc.
Quản lý tần suất kiểm tra điều kiện	tbl_Tansuat, tbl_Location_Tansuat, tbl_DetailWOFrequency	✔ Đã có cấu trúc cho tần suất kiểm tra từng vị trí.
Tiêu chuẩn và giá trị chuẩn của item/machine	tbl_MasterTC, tbl_Detail_TC, tbl_Detail_TCMay, tbl_Item_ValTC, tbl_Machine_ValTC	✔ Cho phép ánh xạ tiêu chuẩn sản phẩm hoặc máy để so sánh dữ liệu nhập.
⚠️ 2. Các điểm chưa đủ / cần mở rộng
Để đáp ứng yêu cầu sản xuất phức tạp bạn nói (kiểm tra điều kiện, dừng lô, xử lý bất thường...), cần thêm vài bảng và cột như sau:

🔹 (1) Điều kiện trước thao tác (Pre-Condition)
Hiện có cờ IsChecksheetCondition trong tbl_ChecksheetItemAssignments, nhưng chưa đủ để thể hiện:

Loại điều kiện (điều kiện đầu ca, điều kiện thiết bị, điều kiện môi trường, v.v.)

Trạng thái đã được kiểm tra hay chưa tại vị trí – bước – ca

👉 Gợi ý thêm bảng:

CREATE TABLE tbl_ProcessConditionStatus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkOrderCode NVARCHAR(50),
    StepCode NVARCHAR(50),
    PositionCode NVARCHAR(50),
    ConditionType NVARCHAR(50), -- ví dụ: 'Đầu ca', 'Thiết bị', 'Môi trường'
    CheckedBy NVARCHAR(100),
    CheckedAt DATETIME,
    IsPassed BIT,
    Remarks NVARCHAR(500)
);
🔹 (2) Quản lý xử lý bất thường / dừng lô
tbl_ChecksheetFormEntry đã có các cột như IsAbnormal, IsStopped, StopReason, ... → rất tốt.
Nhưng nếu bạn cần dừng 1 máng / toàn lô / toàn chỉ thị, nên có bảng trung gian quản lý trạng thái dừng:

CREATE TABLE tbl_AbnormalHandling (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkOrderCode NVARCHAR(50),
    LotNo NVARCHAR(50),
    TrayNo NVARCHAR(50),
    PositionCode NVARCHAR(50),
    AbnormalType NVARCHAR(50), -- Toàn lô / 1 máng / chỉ thị
    Reason NVARCHAR(500),
    ReportedBy NVARCHAR(100),
    ReportedAt DATETIME,
    ApprovedBy NVARCHAR(100),
    ApprovedAt DATETIME,
    ResumeAllowed BIT
);
🔹 (3) Liên kết form theo từng bước
Hiện tbl_ChecksheetForms có FormOrder và FormPosition → nhưng chưa có Step cụ thể cho từng vị trí.

👉 Gợi ý:
Thêm cột StepCode hoặc ProcessStep vào tbl_ChecksheetForms để biết form này thuộc bước nào trong quy trình.
Điều này giúp hệ thống:

Hiển thị đúng form tại đúng bước thao tác.

Kiểm tra điều kiện trước khi gọi form nhập dữ liệu.

🔹 (4) Theo dõi form ẩn / tự động sinh dữ liệu
Bạn đã có IsHidden trong tbl_ChecksheetFormFields → rất tốt.
Nhưng nếu muốn auto-fill theo item thì nên lưu biểu thức hoặc công thức liên kết:

ALTER TABLE tbl_ChecksheetFormFields ADD AutoFillExpression NVARCHAR(500) NULL;
Ví dụ: "AutoFillExpression = SELECT Value_text FROM tbl_Detail_TC WHERE ID_tc = {Item.TC_ID}"

🔹 (5) Bảng điều kiện theo tần suất
Bạn có tbl_Location_Tansuat và tbl_Tansuat – nhưng nếu muốn tự động kiểm tra “đã đủ điều kiện đầu ca chưa”, nên lưu:

Lần cuối kiểm tra điều kiện của vị trí đó.

Tần suất (ví dụ: mỗi 2h, mỗi lô, mỗi ngày, đầu ca...).

Gợi ý mở rộng:

CREATE TABLE tbl_PositionConditionTracking (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PositionCode NVARCHAR(50),
    ConditionType NVARCHAR(50),
    LastCheckedAt DATETIME,
    FrequencyType NVARCHAR(50), -- 'Mỗi ca', 'Mỗi lô', 'Mỗi 2h'
    NextCheckDue DATETIME
);
✅ 3. Đánh giá tổng thể
Tiêu chí	Trạng thái
Quản lý version form & dữ liệu nhập	✔ Hoàn chỉnh
Truy xuất nguồn gốc dữ liệu	✔ Có bảng lịch sử
Ánh xạ form ↔ Excel	✔ Có cấu trúc mapping
Điều kiện thao tác theo bước	⚠ Cần mở rộng thêm bảng điều kiện
Tần suất kiểm tra	⚠ Có dữ liệu cơ bản, cần tracking thời điểm kiểm tra
Dừng lô / xử lý bất thường	⚠ Có trường cơ bản, nên tách bảng chi tiết riêng
Auto-fill dữ liệu ẩn	⚠ Có cờ IsHidden, nên thêm AutoFillExpression
Quản lý bước thao tác (process step)	⚠ Cần thêm liên kết giữa step và form
Nếu bạn muốn, mình có thể vẽ sơ đồ quan hệ ERD tổng quát (hiện + phần đề xuất) để bạn nhìn rõ cách mở rộng mà không phá cấu trúc cũ.
👉 Bạn có muốn mình vẽ sơ đồ đó không?

Bạn đã nói:
Theo dõi form ẩn / tự động sinh dữ liệu cái này sẽ như thế nào khi viết AutoFillExpression = SELECT Value_text FROM tbl_Detail_TC WHERE ID_tc = {Item.TC_ID} sẽ phải thực hiện trong c# mvc như thế nào
ChatGPT đã nói:
Rất hay — đây chính là phần “tự động sinh dữ liệu ẩn” (auto-fill logic) khi render form nhập liệu trong C# MVC.
Mình sẽ hướng dẫn cách triển khai thực tế trong hệ thống bạn đang xây dựng, dựa trên tbl_ChecksheetFormFields và cơ chế AutoFillExpression mà bạn nói đến.

🎯 Mục tiêu
Một số field trong form (IsHidden = 1) sẽ không hiển thị với công nhân.

Nhưng hệ thống sẽ tự động lấy giá trị từ dữ liệu khác (ví dụ: tiêu chuẩn của item, máy, v.v.)

Khi render form hoặc khi đọc mã Item, C# MVC sẽ đánh giá (evaluate) biểu thức trong AutoFillExpression để sinh ra giá trị.

🧩 1. Cấu trúc database mở rộng
Bảng tbl_ChecksheetFormFields nên có thêm cột:

ALTER TABLE tbl_ChecksheetFormFields
ADD AutoFillExpression NVARCHAR(500) NULL;
Ví dụ dữ liệu mẫu:

FieldId	FieldName	IsHidden	AutoFillExpression
15	"TieuChuanDo"	1	SELECT Value_text FROM tbl_Detail_TC WHERE ID_tc = {Item.TC_ID}
🧠 2. Cách C# MVC xử lý AutoFillExpression
Quy trình chung:

Khi render form nhập liệu, hệ thống sẽ lấy danh sách các field từ tbl_ChecksheetFormFields.

Với field nào có IsHidden = true và AutoFillExpression không null → hệ thống thực thi truy vấn trong AutoFillExpression.

Hệ thống thay thế biến placeholder (ví dụ {Item.TC_ID}) bằng giá trị thực của Item đang nhập.

Kết quả query sẽ là giá trị auto-fill cho field đó.

💻 3. Ví dụ mã C# thực hiện
Giả sử bạn đang trong Controller ChecksheetController → action RenderForm.

public async Task<IActionResult> RenderForm(int formId, string itemCode)
{
    // 1. Lấy thông tin form và các field
    var form = await _context.tbl_ChecksheetForms.FindAsync(formId);
    var fields = await _context.tbl_ChecksheetFormFields
        .Where(f => f.FormId == formId)
        .ToListAsync();

    // 2. Lấy thông tin item (để thay thế placeholder)
    var item = await _context.tbl_MasterProductItem
        .FirstOrDefaultAsync(i => i.ItemCode == itemCode);

    var result = new Dictionary<string, object>();

    foreach (var field in fields)
    {
        if (field.IsHidden == true && !string.IsNullOrEmpty(field.AutoFillExpression))
        {
            // 3. Thay thế biến trong biểu thức
            string query = field.AutoFillExpression
                .Replace("{Item.TC_ID}", item.ID_Item.ToString());

            // 4. Thực thi query SQL động để lấy giá trị
            var value = await _context.Database.SqlQueryRaw<string>(query).FirstOrDefaultAsync();

            result[field.FieldName] = value;
        }
        else
        {
            result[field.FieldName] = null; // Chờ người dùng nhập
        }
    }

    return Json(result);
}
⚠️ Lưu ý:

SqlQueryRaw trong EF Core 8 cho phép bạn chạy truy vấn trả về kiểu đơn giản (string, int, …).

Nếu dùng EF Core 6 trở xuống, bạn có thể dùng FromSqlRaw kết hợp với model tạm.

🧱 4. Cách xử lý an toàn hơn (tránh SQL injection)
Để an toàn, bạn không nên lưu câu lệnh SQL nguyên.
Thay vào đó, bạn lưu theo dạng token logic, ví dụ:

FieldName	AutoFillExpression
"TieuChuanDo"	"tbl_Detail_TC.Value_text:ID_tc={Item.TC_ID}"
Khi đó bạn sẽ parse chuỗi này thành câu truy vấn an toàn trong C# như sau:

if (field.AutoFillExpression.Contains(":"))
{
    var parts = field.AutoFillExpression.Split(':');
    var tableAndColumn = parts[0]; // tbl_Detail_TC.Value_text
    var condition = parts[1];      // ID_tc={Item.TC_ID}

    var table = tableAndColumn.Split('.')[0];
    var column = tableAndColumn.Split('.')[1];

    var idTc = item.ID_Item; // Lấy từ item

    // Thực thi truy vấn an toàn
    string sql = $"SELECT {column} FROM {table} WHERE ID_tc = @p0";
    var value = await _context.Database
        .SqlQueryRaw<string>(sql, idTc)
        .FirstOrDefaultAsync();

    result[field.FieldName] = value;
}
Cách này vẫn động nhưng không nguy hiểm vì bạn chỉ cho phép cấu trúc cố định.

🧩 5. Trường hợp dữ liệu auto-fill phức tạp hơn
Nếu bạn cần logic kiểu:

Lấy thông số từ tiêu chuẩn của máy, hoặc

Tính toán giá trị dựa trên nhiều bảng

thì bạn nên viết một service AutoFillEvaluator.

Ví dụ:

public interface IAutoFillEvaluator
{
    Task<object> EvaluateAsync(string expression, object contextData);
}

public class AutoFillEvaluator : IAutoFillEvaluator
{
    private readonly AppDbContext _context;

    public AutoFillEvaluator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> EvaluateAsync(string expression, object contextData)
    {
        // Giải mã expression, thay thế biến trong { }
        var context = (dynamic)contextData;
        string sql = expression.Replace("{Item.TC_ID}", context.Item.TC_ID.ToString());
        return await _context.Database.SqlQueryRaw<object>(sql).FirstOrDefaultAsync();
    }
}
Rồi inject IAutoFillEvaluator vào Controller để gọi linh hoạt.

✅ 6. Kết luận
Thành phần	Mục đích
IsHidden	Ẩn field khỏi UI
AutoFillExpression	Quy tắc tự động lấy dữ liệu
AutoFillEvaluator (C# Service)	Thực thi expression và trả giá trị
RenderForm()	Khi gọi form → chạy AutoFill cho các field ẩn
Nếu bạn muốn, mình có thể giúp bạn viết luôn một class AutoFillEvaluator hoàn chỉnh, hỗ trợ nhiều kiểu biến (Item, Machine, User, Position...) — để dùng luôn trong hệ thống bạn đang làm.
👉 Bạn có muốn mình viết luôn đoạn code đó không?

Bạn đã nói:
nếu đang dùng linq thì sao
ChatGPT đã nói:
Câu hỏi rất hay 👍 — và đúng hướng thực tế, vì trong hệ thống MVC hiện tại của bạn đang dùng Entity Framework LINQ, không nên dùng SqlQueryRaw trực tiếp (vừa rủi ro vừa khó kiểm soát).
Mình sẽ hướng dẫn cách thực hiện AutoFillExpression bằng LINQ an toàn và linh hoạt — vẫn giữ ý tưởng “tự động sinh dữ liệu ẩn”.

🎯 Mục tiêu
Giống như trước:

Các field có IsHidden = true và AutoFillExpression sẽ được tự động lấy giá trị.

Không dùng SQL thô (SqlQueryRaw), mà dùng LINQ + switch logic để truy xuất dữ liệu.

🧩 1. Mở rộng cột trong bảng tbl_ChecksheetFormFields
ALTER TABLE tbl_ChecksheetFormFields
ADD AutoFillExpression NVARCHAR(200) NULL;
Ví dụ giá trị:

FieldName	AutoFillExpression
"TieuChuanDo"	"Detail_TC.Value_text:ID_tc={Item.TC_ID}"
"TenMay"	"MasterMachines.MachineName:MachineCode={Position.MachineCode}"
⚙️ 2. Ý tưởng triển khai trong C#
Ta sẽ phân tích (parse) chuỗi AutoFillExpression và chuyển nó thành logic LINQ tương ứng.

🔹 Ví dụ:
AutoFillExpression = "Detail_TC.Value_text:ID_tc={Item.TC_ID}"
→ nghĩa là: Lấy Value_text từ bảng tbl_Detail_TC nơi ID_tc = Item.TC_ID.

💻 3. Code mẫu LINQ AutoFill trong Controller
public async Task<IActionResult> RenderForm(int formId, string itemCode)
{
    var fields = await _context.tbl_ChecksheetFormFields
        .Where(f => f.FormId == formId)
        .ToListAsync();

    var item = await _context.tbl_MasterProductItem
        .FirstOrDefaultAsync(i => i.ItemCode == itemCode);

    var result = new Dictionary<string, object>();

    foreach (var field in fields)
    {
        if (field.IsHidden == true && !string.IsNullOrEmpty(field.AutoFillExpression))
        {
            var value = await EvaluateAutoFillExpression(field.AutoFillExpression, item);
            result[field.FieldName] = value;
        }
        else
        {
            result[field.FieldName] = null;
        }
    }

    return Json(result);
}
💡 4. Hàm EvaluateAutoFillExpression() — xử lý bằng LINQ
private async Task<object> EvaluateAutoFillExpression(string expression, tbl_MasterProductItem item)
{
    // Tách cấu trúc "Bảng.Cột:ĐiềuKiện"
    var parts = expression.Split(':');
    if (parts.Length != 2)
        return null;

    var tableAndColumn = parts[0]; // Ví dụ: "Detail_TC.Value_text"
    var condition = parts[1];      // Ví dụ: "ID_tc={Item.TC_ID}"

    var table = tableAndColumn.Split('.')[0];
    var column = tableAndColumn.Split('.')[1];

    // Lấy giá trị biến trong { } ra
    var regex = new System.Text.RegularExpressions.Regex(@"\{(.*?)\}");
    var match = regex.Match(condition);
    if (!match.Success) return null;

    var variable = match.Groups[1].Value; // "Item.TC_ID"

    // Thay thế biến trong điều kiện bằng giá trị thực
    string fieldName = condition.Split('=')[0]; // ID_tc
    string value = "";
    if (variable == "Item.TC_ID") value = item.ID_Item.ToString();

    // Dựa vào bảng → gọi LINQ tương ứng
    switch (table)
    {
        case "Detail_TC":
            {
                var id = int.Parse(value);
                var record = await _context.tbl_Detail_TC
                    .FirstOrDefaultAsync(x => x.ID_tc == id);
                return record?.Value_text;
            }
        case "MasterMachines":
            {
                var code = value;
                var record = await _context.tbl_MasterMachines
                    .FirstOrDefaultAsync(x => x.MachineCode == code);
                return record?.MachineName;
            }
        case "MasterProductItem":
            {
                var code = value;
                var record = await _context.tbl_MasterProductItem
                    .FirstOrDefaultAsync(x => x.ItemCode == code);
                return record?.ItemName;
            }
        default:
            return null;
    }
}
✅ 5. Ưu điểm của cách dùng LINQ
Ưu điểm	Giải thích
🔒 An toàn tuyệt đối	Không dùng SQL động → tránh SQL Injection.
⚙️ Tích hợp chặt với EF Core	Tận dụng được caching, async query, tracking.
🧩 Dễ mở rộng	Chỉ cần thêm case cho các bảng khác.
🔍 Dễ debug / kiểm soát	Không lo lỗi SQL hoặc sai cú pháp.
🧱 6. Khi cần nâng cao
Nếu bạn có nhiều biểu thức khác nhau (hàng chục bảng auto-fill khác nhau), bạn có thể trừu tượng hóa nó thành một service riêng, ví dụ:

public interface IAutoFillService
{
    Task<object> EvaluateAsync(string expression, object context);
}

public class AutoFillService : IAutoFillService
{
    private readonly AppDbContext _context;
    public AutoFillService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> EvaluateAsync(string expression, object context)
    {
        // Giống như EvaluateAutoFillExpression() ở trên
        // Có thể parse {Item}, {Machine}, {User}, {Position}, ...
        // Dễ mở rộng khi thêm loại điều kiện mới
    }
}
Sau đó Controller chỉ cần:

var value = await _autoFillService.EvaluateAsync(field.AutoFillExpression, new { Item = item });
🧩 7. Kết hợp thực tế khi render form
Khi render giao diện form builder hoặc form nhập liệu:

Các field có IsHidden = true sẽ không render input HTML.

Nhưng trước khi gửi form ra view, controller đã gán giá trị auto-fill → nên khi submit dữ liệu, giá trị này vẫn được lưu trong JsonValue.

🔚 Tổng kết
Thành phần	Vai trò
AutoFillExpression	Định nghĩa cách lấy dữ liệu tự động
EvaluateAutoFillExpression()	Phân tích biểu thức & truy xuất bằng LINQ
IAutoFillService	Giúp gom logic auto-fill và tái sử dụng
IsHidden = true	Không hiển thị nhưng vẫn có giá trị
Nếu bạn muốn, mình có thể giúp bạn viết luôn version nâng cao của AutoFillService hỗ trợ:

Biểu thức có nhiều biến {Item}, {Machine}, {Position}, {User}

Cả phép toán đơn giản (VD: {Item.Weight} * 2)

Cache kết quả để tránh truy vấn lại

👉 Bạn có muốn mình viết luôn bản đầy đủ đó không?

Bạn đã nói:
Đầy đi
ChatGPT đã nói:
Rất tuyệt 😎 — vậy mình sẽ viết bản đầy đủ của AutoFillService dùng LINQ, an toàn, mở rộng được, và tương thích hoàn toàn với mô hình bạn đang xây dựng (EF Core trong C# MVC).

Service này có thể:

Tự động đọc các field ẩn (IsHidden) trong form.

Thực thi các biểu thức động như {Item.FieldName}, {Machine.Code}, {Position.LocationCode}.

Hỗ trợ lấy dữ liệu từ các bảng khác nhau bằng LINQ (không SQL thô).

Hỗ trợ các phép toán cơ bản (nhân, chia, cộng, trừ).

Cho phép cache kết quả trong 1 lần render form để tăng tốc.

🧱 1️⃣ Tạo file service AutoFillService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Services
{
    public interface IAutoFillService
    {
        Task<object> EvaluateAsync(string expression, object context);
    }

    public class AutoFillService : IAutoFillService
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<string, object> _cache = new(); // cache cục bộ cho 1 lần render form

        public AutoFillService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> EvaluateAsync(string expression, object context)
        {
            if (string.IsNullOrWhiteSpace(expression)) return null;

            // 1. Nếu đã có trong cache thì dùng lại
            if (_cache.ContainsKey(expression))
                return _cache[expression];

            // 2. Xử lý biểu thức trong { }
            string resolvedExpr = await ResolveVariablesAsync(expression, context);

            // 3. Nếu chỉ là phép toán, tính luôn (vd: "5*2")
            if (IsSimpleMathExpression(resolvedExpr))
            {
                var mathResult = EvaluateMathExpression(resolvedExpr);
                _cache[expression] = mathResult;
                return mathResult;
            }

            // 4. Nếu là truy vấn dữ liệu
            object result = await ExecuteDataQueryAsync(resolvedExpr, context);
            _cache[expression] = result;
            return result;
        }

        // ------------------- HÀM PHỤ TRỢ -------------------

        private async Task<string> ResolveVariablesAsync(string expr, object context)
        {
            // tìm tất cả {Variable.Path}
            var regex = new Regex(@"\{(.*?)\}");
            var matches = regex.Matches(expr);

            string result = expr;

            foreach (Match match in matches)
            {
                string fullPath = match.Groups[1].Value; // ví dụ: "Item.ItemCode"
                string[] parts = fullPath.Split('.');

                if (parts.Length < 2) continue;

                string entityName = parts[0];
                string propertyName = parts[1];
                object value = null;

                // lấy giá trị context động
                switch (entityName)
                {
                    case "Item":
                        var item = GetPropValue(context, "Item");
                        value = GetPropValue(item, propertyName);
                        break;
                    case "Machine":
                        var machine = GetPropValue(context, "Machine");
                        value = GetPropValue(machine, propertyName);
                        break;
                    case "User":
                        var user = GetPropValue(context, "User");
                        value = GetPropValue(user, propertyName);
                        break;
                    case "Position":
                        var pos = GetPropValue(context, "Position");
                        value = GetPropValue(pos, propertyName);
                        break;
                }

                result = result.Replace(match.Value, value?.ToString() ?? "");
            }

            return result;
        }

        private static object GetPropValue(object obj, string name)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(name);
            return prop?.GetValue(obj, null);
        }

        private static bool IsSimpleMathExpression(string expr)
        {
            return Regex.IsMatch(expr, @"^[0-9\.\+\-\*/\(\) ]+$");
        }

        private static object EvaluateMathExpression(string expr)
        {
            // cách nhanh & an toàn: dùng DataTable.Compute
            try
            {
                var dt = new System.Data.DataTable();
                return dt.Compute(expr, "");
            }
            catch
            {
                return null;
            }
        }

        private async Task<object> ExecuteDataQueryAsync(string expr, object context)
        {
            // Ví dụ biểu thức:
            // "tbl_Detail_TC.Value_text:ID_tc=123"
            if (!expr.Contains(":")) return expr;

            var parts = expr.Split(':');
            var tableAndColumn = parts[0];
            var condition = parts[1];

            var table = tableAndColumn.Split('.')[0];
            var column = tableAndColumn.Split('.')[1];

            string[] condParts = condition.Split('=');
            if (condParts.Length != 2) return null;

            string condField = condParts[0];
            string condValue = condParts[1];

            switch (table)
            {
                case "tbl_Detail_TC":
                    if (int.TryParse(condValue, out int idTc))
                    {
                        var record = await _context.tbl_Detail_TC
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.ID_tc == idTc);
                        return GetPropValue(record, column);
                    }
                    break;

                case "tbl_MasterMachines":
                    var machine = await _context.tbl_MasterMachines
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MachineCode == condValue);
                    return GetPropValue(machine, column);

                case "tbl_MasterProductItem":
                    var item = await _context.tbl_MasterProductItem
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ItemCode == condValue);
                    return GetPropValue(item, column);
            }

            return null;
        }
    }
}
🧩 2️⃣ Sử dụng trong Controller
Ví dụ ChecksheetController:

using YourNamespace.Services;

public class ChecksheetController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAutoFillService _autoFillService;

    public ChecksheetController(AppDbContext context, IAutoFillService autoFillService)
    {
        _context = context;
        _autoFillService = autoFillService;
    }

    public async Task<IActionResult> RenderForm(int formId, string itemCode)
    {
        var fields = await _context.tbl_ChecksheetFormFields
            .Where(f => f.FormId == formId)
            .ToListAsync();

        var item = await _context.tbl_MasterProductItem
            .FirstOrDefaultAsync(i => i.ItemCode == itemCode);

        var contextData = new
        {
            Item = item,
            User = new { Username = User.Identity.Name },
            Machine = await _context.tbl_MasterMachines.FirstOrDefaultAsync(),
            Position = await _context.tbl_MasterPosition.FirstOrDefaultAsync()
        };

        var result = new Dictionary<string, object>();

        foreach (var field in fields)
        {
            if (field.IsHidden == true && !string.IsNullOrEmpty(field.AutoFillExpression))
            {
                var value = await _autoFillService.EvaluateAsync(field.AutoFillExpression, contextData);
                result[field.FieldName] = value;
            }
            else
            {
                result[field.FieldName] = null;
            }
        }

        return Json(result);
    }
}
⚙️ 3️⃣ Cách đăng ký Service trong Program.cs
builder.Services.AddScoped<IAutoFillService, AutoFillService>();
🧠 4️⃣ Ví dụ biểu thức bạn có thể dùng trong DB
AutoFillExpression	Ý nghĩa
tbl_Detail_TC.Value_text:ID_tc={Item.TC_ID}	Lấy giá trị tiêu chuẩn của sản phẩm hiện tại
tbl_MasterMachines.MachineName:MachineCode={Position.MachineCode}	Lấy tên máy theo vị trí đang thao tác
{Item.Weight} * 2	Tính giá trị nhân đôi từ thuộc tính item
{User.Username}	Lấy tên người thao tác hiện tại
tbl_MasterProductItem.ItemName:ItemCode={Item.ItemCode}	Lấy tên sản phẩm hiện tại
✅ 5️⃣ Ưu điểm của bản đầy đủ này
Tính năng	Mô tả
🧩 Đa biến	Hỗ trợ {Item}, {Machine}, {User}, {Position}
🧠 Tự động tính toán	Hỗ trợ phép toán cơ bản (nhân, chia, cộng, trừ)
🔒 An toàn tuyệt đối	Không SQL thô, dùng LINQ + EF
⚙️ Tích hợp sẵn caching	Giảm truy vấn lặp khi render form nhiều field
🧱 Dễ mở rộng	Chỉ cần thêm case cho bảng mới
Nếu bạn muốn mình mở rộng thêm tính năng:

Cho phép chuỗi biểu thức phức tạp nhiều tầng (ví dụ: tbl_Detail_TC.Value_text:ID_tc={Item.TC_ID} + 1)

Hoặc liên kết nhiều bảng trong một expression
thì mình có thể viết thêm lớp ExpressionParser nâng cao để phân tích cú pháp dạng mini-language.

👉 Bạn có muốn mình mở rộng tiếp theo hướng đó không (có hỗ trợ parse nhiều điều kiện / join dữ liệu giữa các bảng)?






Không có tệp nào được chọnKhông có tệp nào được chọn
ChatGPT có thể mắc lỗi. Hãy kiểm tra các thông tin quan trọng.


