using Microsoft.AspNetCore.Mvc;
using CV_AI.Services; // namespace của GeminiService
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CV_AI.Models;
using CV_AI.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CV_AI.Controllers.DichVu
{
    public class CreateCVWithAI : Controller
    {
        private readonly IGeminiService _geminiService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public CreateCVWithAI(IGeminiService geminiService, ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _geminiService = geminiService;
            _dbContext = dbContext;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string FullName, string Position, string Skills, string Experience, string Education, string Objective, string Phone, string Email, string Address, string Projects, string Certificates, string Languages, string Interests)
        {
            var prompt = $@"
Hãy tạo một bản SƠ YẾU LÝ LỊCH (CV) chuyên nghiệp, trình bày rõ ràng, KHÔNG dùng icon, KHÔNG markdown, KHÔNG ký tự đặc biệt (kể cả **, #, -, *, ...), các mục chính viết hoa, mỗi mục trên 1 dòng, gạch đầu dòng dùng dấu •,và trình bày đẹp mắt xuống dòng căn dòng không trình bày lộn xộn,thiết kế cv chuẩn giúp mìnhmình không giải thích gì thêm, chỉ trả về nội dung CV.

Nếu trường nào đã có dữ liệu thì giữ nguyên, nếu trường nào trống thì hãy tự động sinh ra một giá trị mẫu hợp lý (ví dụ: số điện thoại, email, địa chỉ, ...).
Yêu cầu:

Thiết kế rõ ràng, đẹp, dễ đọc

Trình bày chuyên nghiệp có thể dùng đi phỏng vấn ngay

Sử dụng icon cho từng mục (ví dụ: 🎯 cho Mục tiêu, 💼 cho Kinh nghiệm, 📞 cho Liên hệ...)

Có định dạng dễ copy sang Word hoặc xuất file PDF

Sắp xếp các mục theo thứ tự: Họ tên, Thông tin liên hệ, Vị trí ứng tuyển, Tóm tắt nghề nghiệp, Mục tiêu, Kỹ năng, Kinh nghiệm, Học vấn, Chứng chỉ, Dự án, Ngoại ngữ, Soft skills
Dữ liệu:
HỌ VÀ TÊN: {(string.IsNullOrWhiteSpace(FullName) ? "" : FullName)}
SỐ ĐIỆN THOẠI: {(string.IsNullOrWhiteSpace(Phone) ? "" : Phone)}
EMAIL: {(string.IsNullOrWhiteSpace(Email) ? "" : Email)}
ĐỊA CHỈ: {(string.IsNullOrWhiteSpace(Address) ? "" : Address)}
MỤC TIÊU NGHỀ NGHIỆP: {(string.IsNullOrWhiteSpace(Objective) ? "" : Objective)}
KINH NGHIỆM LÀM VIỆC: {(string.IsNullOrWhiteSpace(Experience) ? "" : Experience)}
HỌC VẤN: {(string.IsNullOrWhiteSpace(Education) ? "" : Education)}
KỸ NĂNG: {(string.IsNullOrWhiteSpace(Skills) ? "" : Skills)}
DỰ ÁN TIÊU BIỂU: {(string.IsNullOrWhiteSpace(Projects) ? "" : Projects)}
CHỨNG CHỈ: {(string.IsNullOrWhiteSpace(Certificates) ? "" : Certificates)}
NGÔN NGỮ: {(string.IsNullOrWhiteSpace(Languages) ? "" : Languages)}
SỞ THÍCH: {(string.IsNullOrWhiteSpace(Interests) ? "" : Interests)}
";
            var result = await _geminiService.GenerateCVAsync(prompt);

            ViewBag.CVResult = result ?? "Không thể sinh CV, vui lòng thử lại!";
            return View();
        }

        [HttpPost]
        public IActionResult SaveCV(string CVContent)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "Không xác định được UserId. Vui lòng đăng nhập lại.";
                        return RedirectToAction("Index", "EditCV");
                    }
                    if (string.IsNullOrWhiteSpace(CVContent))
                    {
                        TempData["Error"] = "Nội dung CV rỗng, không thể lưu.";
                        return RedirectToAction("Index", "EditCV");
                    }
                    string htmlContent = "<pre style='font-family:inherit;font-size:1.08rem;'>" + System.Net.WebUtility.HtmlEncode(CVContent) + "</pre>";
                    var savedCV = new SavedCV
                    {
                        UserId = userId,
                        Content = htmlContent,
                        CreatedAt = DateTime.Now,
                        Title = "CV " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                    };
                    _dbContext.SavedCVs.Add(savedCV);
                    int affected = _dbContext.SaveChanges();
                    TempData["SuccessMessage"] = $"Lưu CV thành công! Đã lưu {affected} bản ghi.";
                    TempData["EditCVContent"] = htmlContent;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi lưu CV: " + ex.Message;
                }
                return RedirectToAction("Index", "EditCV");
            }
            else
            {
                var list = HttpContext.Session.GetObjectFromJson<List<string>>("SavedCVs") ?? new List<string>();
                list.Add(CVContent);
                HttpContext.Session.SetObjectAsJson("SavedCVs", list);
                TempData["SuccessMessage"] = "Lưu CV thành công!";
                TempData["EditCVContent"] = CVContent;
                return RedirectToAction("Index", "EditCV");
            }
        }

        [HttpPost]
        public IActionResult EditCV(string CVContent)
        {
            // Lưu tạm CVContent vào TempData hoặc Session để chuyển sang trang chỉnh sửa
            TempData["EditCVContent"] = CVContent;
            return RedirectToAction("Index", "EditCV");
        }

        [HttpGet]
        public IActionResult TestInsert()
        {
            try
            {
                var savedCV = new SavedCV
                {
                    UserId = "test-user",
                    Content = "<pre>Test content</pre>",
                    CreatedAt = DateTime.Now,
                    Title = "Test CV"
                };
                _dbContext.SavedCVs.Add(savedCV);
                int affected = _dbContext.SaveChanges();
                return Content($"Inserted rows: {affected}");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }
    }
}
