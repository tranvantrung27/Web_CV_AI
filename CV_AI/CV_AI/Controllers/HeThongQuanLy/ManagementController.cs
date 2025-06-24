using Microsoft.AspNetCore.Mvc;
using CV_AI.Data;
using System.Linq;
using CV_AI.Models.ViewModels;
using CV_AI.Models;
using Microsoft.EntityFrameworkCore;
namespace CV_AI.Controllers
{
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManagePosts()
        {
            var userId = HttpContext.Session.GetString("UserID");
            // Lấy các tin do nhà tuyển dụng này đăng, kèm thông tin công ty
            var jobPosts = _context.JobPosts
                .Include(jp => jp.Employer)
                .Include(jp => jp.Applications)
                .Where(jp => jp.ID_Employer == userId)
                .OrderByDescending(jp => jp.PostedDate)
                .ToList();

            // Tạo dictionary: JobPostId -> Số lượng ứng viên
            var soLuongUngVienDict = jobPosts.ToDictionary(
                jp => jp.ID_JobPost,
                jp => jp.Applications?.Count ?? 0
            );

            ViewBag.JobPosts = jobPosts;
            ViewBag.SoLuongUngVienDict = soLuongUngVienDict;
            ViewBag.Categories = _context.Categories.ToList();
            return View("QuanLyDangTin/ManagePosts");
        }

        // GET: Job/Create - Tạo tin tuyển dụng mới (chỉ Employer)
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Employer")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Job/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostViewModel model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");

            if (userRole != "Employer" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var jobPost = new JobPost
                {
                    ID_Employer = userId ?? string.Empty,
                    Title = model.Title,
                    Description = model.Description,
                    Requirements = model.Requirements,
                    Location = model.Location,
                    Salary = model.Salary,
                    ExpirationDate = model.ExpirationDate,
                    PostedDate = DateTime.Now,
                    IsActive = true
                };

                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();

                // Add categories
                foreach (var categoryId in model.CategoryIds)
                {
                    var jobPostCategory = new JobPostCategory
                    {
                        ID_JobPost = jobPost.ID_JobPost,
                        ID_Category = categoryId
                    };
                    _context.JobPostCategories.Add(jobPostCategory);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng tin tuyển dụng thành công!";
                return RedirectToAction("ManagePosts");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // Hiển thị danh sách ứng viên ứng tuyển vào một JobPost
        public IActionResult ManageApplicants(int id)
        {
            var jobPost = _context.JobPosts.Include(jp => jp.Employer).FirstOrDefault(jp => jp.ID_JobPost == id);
            if (jobPost == null)
            {
                return NotFound();
            }
            var applications = _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Where(a => a.ID_JobPost == id)
                .ToList();
            ViewBag.JobPost = jobPost;
            return View("QuanLyDangTin/ManageApplicants", applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptApplication(int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application != null)
            {
                application.Status = "Accepted";
                await _context.SaveChangesAsync();
                // TODO: Thêm logic tạo thông báo cho ứng viên
                TempData["SuccessMessage"] = "Đã chấp nhận ứng viên thành công.";
            }
            return RedirectToAction("ManageApplicants", new { id = application?.ID_JobPost });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application != null)
            {
                application.Status = "Rejected";
                await _context.SaveChangesAsync();
                // TODO: Thêm logic tạo thông báo cho ứng viên
                TempData["SuccessMessage"] = "Đã từ chối ứng viên.";
            }
            return RedirectToAction("ManageApplicants", new { id = application?.ID_JobPost });
        }
    }
}
