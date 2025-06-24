using Microsoft.AspNetCore.Mvc;
using CV_AI.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CV_AI.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                    return StatusCode(401, "Session UserID is null or empty. Please login again.");

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var result = notifications.Select(n => new {
                    n.Message,
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList();

                return Json(new { count = result.Count, notifications = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            return $"{(int)span.TotalDays} ngày trước";
        }
    }
} 