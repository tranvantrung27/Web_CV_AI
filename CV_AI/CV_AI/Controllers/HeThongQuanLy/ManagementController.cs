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
                .Where(jp => jp.ID_Employer == userId)
                .OrderByDescending(jp => jp.PostedDate)
                .ToList();

            ViewBag.JobPosts = jobPosts;
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
    }
}
