using Microsoft.AspNetCore.Mvc;
using CV_AI.Models;
using CV_AI.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Xceed.Words.NET;
using System.IO;

namespace CV_AI.Controllers.DichVu
{
    public class EditCVController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        public EditCVController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cvs = _dbContext.SavedCVs.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToList();
                ViewBag.SavedCVs = cvs;
                ViewBag.DebugUserId = userId;
                ViewBag.DebugCVCount = cvs.Count;
            }
            else
            {
                ViewBag.SavedCVs = null;
                ViewBag.DebugUserId = "(not authenticated)";
                ViewBag.DebugCVCount = 0;
            }
            return View();
        }

        [HttpPost]
        public IActionResult SaveEditedCV(int id, string CVContent)
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
                    var cv = _dbContext.SavedCVs.FirstOrDefault(x => x.Id == id && x.UserId == userId);
                    if (cv != null)
                    {
                        string htmlContent = System.Net.WebUtility.HtmlEncode(CVContent);
                        cv.Content = htmlContent;
                        cv.UpdatedAt = DateTime.Now;
                        _dbContext.SaveChanges();
                        TempData["SuccessMessage"] = "Đã lưu CV chỉnh sửa thành công!";
                    }
                    else
                    {
                        TempData["Error"] = "Không tìm thấy CV để chỉnh sửa!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi lưu chỉnh sửa: " + ex.Message;
                }
            }
            return RedirectToAction("Index", "EditCV");
        }

        [HttpPost]
        public IActionResult ExportToWord(string CVContent)
        {
            using (var ms = new MemoryStream())
            {
                using (var doc = DocX.Create(ms))
                {
                    if (!string.IsNullOrEmpty(CVContent))
                    {
                        var lines = CVContent.Replace("\r\n", "\n").Split('\n');
                        foreach (var line in lines)
                        {
                            doc.InsertParagraph(line)
                                .Font("Times New Roman")
                                .FontSize(13)
                                .SpacingAfter(2);
                        }
                    }
                    doc.Save();
                }
                ms.Position = 0;
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "CV.docx");
            }
        }
    }
}
