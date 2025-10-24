using Microsoft.AspNetCore.Mvc;
using CV_AI.Models;
using CV_AI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CV_AI.Controllers.Admin
{
    public class AdminFeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminFeedback
        public async Task<IActionResult> Index(string? category = null, string? status = null, string? search = null)
        {
            // Kiểm tra quyền Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Feedbacks.AsQueryable();

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

            // Search by subject or content
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Subject.Contains(search) || 
                                         f.Content.Contains(search) || 
                                         f.UserName.Contains(search) || 
                                         f.UserEmail.Contains(search));
            }

            var feedbacks = await query
                .OrderByDescending(f => f.Status == "Pending") // Pending first
                .ThenByDescending(f => f.Status == "In Progress") // Then In Progress
                .ThenByDescending(f => f.Priority == "Urgent") // Then by Priority
                .ThenByDescending(f => f.CreatedAt) // Then by date
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.PendingCount = await _context.Feedbacks.CountAsync(f => f.Status == "Pending");
            ViewBag.UrgentCount = await _context.Feedbacks.CountAsync(f => f.Priority == "Urgent" && f.Status != "Resolved" && f.Status != "Closed");

            return View(feedbacks);
        }

        // GET: Admin/AdminFeedback/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra quyền Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Admin/AdminFeedback/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminResponse = null, string? priority = null)
        {
            // Kiểm tra quyền Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền thực hiện." });
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return Json(new { success = false, message = "Không tìm thấy feedback." });
            }

            // Cập nhật trạng thái
            feedback.Status = status;
            
            // Cập nhật phản hồi của admin nếu có
            if (!string.IsNullOrEmpty(adminResponse))
            {
                feedback.AdminResponse = adminResponse;
            }
            
            // Cập nhật mức độ ưu tiên nếu có
            if (!string.IsNullOrEmpty(priority))
            {
                feedback.Priority = priority;
            }
            
            // Cập nhật thời gian
            feedback.UpdatedAt = DateTime.Now;
            
            // Nếu đã giải quyết hoặc đóng, cập nhật thời gian giải quyết
            if (status == "Resolved" || status == "Closed")
            {
                feedback.ResolvedAt = DateTime.Now;
                
                // Tạo thông báo cho người dùng
                await CreateUserNotification(feedback);
            }

            _context.Update(feedback);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // POST: Admin/AdminFeedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra quyền Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền thực hiện." });
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return Json(new { success = false, message = "Không tìm thấy feedback." });
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Xóa feedback thành công!" });
        }

        // Private method: Create notification for user when feedback is resolved
        private async Task CreateUserNotification(Feedback feedback)
        {
            var notification = new Notification
            {
                UserId = feedback.UserId,
                Message = $"Góp ý của bạn '{feedback.Subject}' đã được phản hồi.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}

