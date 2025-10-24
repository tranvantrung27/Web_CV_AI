using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CV_AI.Data;
using CV_AI.Models;
using Microsoft.AspNetCore.Authorization;

namespace CV_AI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Middleware: Check if user is Admin
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == "Admin";
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Thống kê tổng quan
            var totalUsers = await _context.Users.CountAsync();
            var totalCandidates = await _context.Candidates.CountAsync();
            var totalEmployers = await _context.Employers.CountAsync();
            var totalJobPosts = await _context.JobPosts.CountAsync();
            var totalApplications = await _context.Applications.CountAsync();
            var activeJobPosts = await _context.JobPosts.CountAsync(j => j.IsActive);
            var pendingApplications = await _context.Applications.CountAsync(a => a.Status == "Pending");

            // Người dùng mới trong 30 ngày
            var newUsersLast30Days = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync();

            // Tin tuyển dụng mới trong 7 ngày
            var newJobPostsLast7Days = await _context.JobPosts
                .Where(j => j.PostedDate >= DateTime.Now.AddDays(-7))
                .CountAsync();

            // Top 5 công ty có nhiều tin tuyển dụng nhất
            var topEmployers = await _context.JobPosts
                .Include(j => j.Employer)
                    .ThenInclude(e => e != null ? e.User : null)
                .GroupBy(j => j.ID_Employer)
                .Select(g => new
                {
                    EmployerId = g.Key,
                    CompanyName = g.First().Employer != null ? g.First().Employer.CompanyName : "N/A",
                    JobCount = g.Count()
                })
                .OrderByDescending(x => x.JobCount)
                .Take(5)
                .ToListAsync();

            // Recent activities (latest 10 applications)
            var recentApplications = await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c != null ? c.User : null)
                .Include(a => a.JobPost)
                .OrderByDescending(a => a.AppliedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCandidates = totalCandidates;
            ViewBag.TotalEmployers = totalEmployers;
            ViewBag.TotalJobPosts = totalJobPosts;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.ActiveJobPosts = activeJobPosts;
            ViewBag.PendingApplications = pendingApplications;
            ViewBag.NewUsersLast30Days = newUsersLast30Days;
            ViewBag.NewJobPostsLast7Days = newJobPostsLast7Days;
            ViewBag.TopEmployers = topEmployers;
            ViewBag.RecentApplications = recentApplications;

            return View();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string? searchTerm, string? roleFilter)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var usersQuery = _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Employer)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    (u.Email != null && u.Email.Contains(searchTerm))
                );
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All")
            {
                usersQuery = usersQuery.Where(u => u.Role == roleFilter);
            }

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;

            return View(users);
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền!" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = user.IsActive });
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền!" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });
            }

            if (user.Role == "Admin")
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản Admin!" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa người dùng thành công!" });
        }

        // GET: Admin/JobPosts
        public async Task<IActionResult> JobPosts(string? searchTerm, bool? isActive)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var jobPostsQuery = _context.JobPosts
                .Include(j => j.Employer)
                    .ThenInclude(e => e != null ? e.User : null)
                .Include(j => j.JobPostCategories)
                    .ThenInclude(jpc => jpc.Category)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                jobPostsQuery = jobPostsQuery.Where(j =>
                    j.Title.Contains(searchTerm) ||
                    (j.Employer != null && j.Employer.CompanyName.Contains(searchTerm))
                );
            }

            // Filter by status
            if (isActive.HasValue)
            {
                jobPostsQuery = jobPostsQuery.Where(j => j.IsActive == isActive.Value);
            }

            var jobPosts = await jobPostsQuery
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.IsActive = isActive;

            return View(jobPosts);
        }

        // POST: Admin/ToggleJobPostStatus
        [HttpPost]
        public async Task<IActionResult> ToggleJobPostStatus(int jobPostId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền!" });
            }

            var jobPost = await _context.JobPosts.FindAsync(jobPostId);
            if (jobPost == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tin tuyển dụng!" });
            }

            jobPost.IsActive = !jobPost.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = jobPost.IsActive });
        }

        // POST: Admin/DeleteJobPost
        [HttpPost]
        public async Task<IActionResult> DeleteJobPost(int jobPostId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền!" });
            }

            var jobPost = await _context.JobPosts.FindAsync(jobPostId);
            if (jobPost == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tin tuyển dụng!" });
            }

            _context.JobPosts.Remove(jobPost);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa tin tuyển dụng thành công!" });
        }

        // GET: Admin/Applications
        public async Task<IActionResult> Applications(string? status)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var applicationsQuery = _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c != null ? c.User : null)
                .Include(a => a.JobPost)
                    .ThenInclude(j => j != null ? j.Employer : null)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                applicationsQuery = applicationsQuery.Where(a => a.Status == status);
            }

            var applications = await applicationsQuery
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            ViewBag.Status = status;

            return View(applications);
        }
    }
}

