using Microsoft.AspNetCore.Mvc;
using CV_AI.Models;
using CV_AI.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CV_AI.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Feedback/Index - Danh sách feedback của user
        public async Task<IActionResult> Index(string? category = null, string? status = null, string? search = null)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                    : HttpContext.Session.GetString("UserID");

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Vui lòng đăng nhập để xem feedback.";
                    return RedirectToAction("Login", "Account");
                }

                var query = _context.Feedbacks.Where(f => f.UserId == userId);

                // Filter by category
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(f => f.Category == category);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(f => f.Status == status);
                }

                // Search by subject
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(f => f.Subject.Contains(search) || f.Content.Contains(search));
                }

                var feedbacks = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();

                ViewBag.Category = category;
                ViewBag.Status = status;
                ViewBag.Search = search;
                ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "User";

                return View(feedbacks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Feedback/Index: {ex.Message}");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách feedback.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Feedback/Create - Form tạo feedback mới
        public IActionResult Create()
        {
            var userId = User.Identity?.IsAuthenticated == true 
                ? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                : HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Vui lòng đăng nhập để gửi feedback.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Feedback/Create - Xử lý submit feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                    : HttpContext.Session.GetString("UserID");

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                // Check rate limiting (max 5 feedbacks per hour)
                var oneHourAgo = DateTime.Now.AddHours(-1);
                var recentFeedbackCount = await _context.Feedbacks
                    .CountAsync(f => f.UserId == userId && f.CreatedAt >= oneHourAgo);

                if (recentFeedbackCount >= 5)
                {
                    return Json(new { 
                        success = false, 
                        message = "Bạn đã gửi quá nhiều feedback trong giờ qua. Vui lòng thử lại sau." 
                    });
                }

                // Get user info
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                feedback.UserId = userId;
                feedback.UserName = user.FullName ?? user.UserName ?? "Anonymous";
                feedback.UserEmail = user.Email ?? "";
                feedback.Status = "Pending";
                feedback.CreatedAt = DateTime.Now;

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Create notification for admins
                await CreateAdminNotification(feedback);

                return Json(new { 
                    success = true, 
                    message = "Gửi góp ý thành công! Cảm ơn bạn đã đóng góp ý kiến.",
                    feedbackId = feedback.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Feedback/Create: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi feedback." });
            }
        }

        // GET: Feedback/Details/5 - Xem chi tiết feedback
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                    : HttpContext.Session.GetString("UserID");

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var feedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

                if (feedback == null)
                {
                    TempData["Error"] = "Không tìm thấy feedback hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index");
                }

                return View(feedback);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Feedback/Details: {ex.Message}");
                TempData["Error"] = "Đã xảy ra lỗi khi xem feedback.";
                return RedirectToAction("Index");
            }
        }

        // POST: Feedback/Delete/5 - Xóa feedback (chỉ nếu chưa được xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                    : HttpContext.Session.GetString("UserID");

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                var feedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

                if (feedback == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy feedback." });
                }

                // Chỉ cho phép xóa nếu chưa được xử lý
                if (feedback.Status != "Pending")
                {
                    return Json(new { 
                        success = false, 
                        message = "Không thể xóa feedback đã được xử lý." 
                    });
                }

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa feedback thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Feedback/Delete: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa feedback." });
            }
        }

        // Private method: Create notification for admins
        private async Task CreateAdminNotification(Feedback feedback)
        {
            try
            {
                // Find all admin users
                var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();

                foreach (var admin in admins)
                {
                    var notification = new CV_AI.Models.Notification
                    {
                        UserId = admin.Id,
                        Message = $"Góp ý mới từ {feedback.UserName}: {feedback.Subject}",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating admin notification: {ex.Message}");
            }
        }
    }
}

