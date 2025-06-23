using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CV_AI.Data;
using CV_AI.Models;
using CV_AI.Models.ViewModels;

namespace CV_AI.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Job/Index - Danh sách tin tuyển dụng
        public async Task<IActionResult> Index(string? searchTerm, string? location, int? categoryId, int page = 1)
        {
            var query = _context.JobPosts
                .Include(jp => jp.Employer)
                .Include(jp => jp.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
                .Where(jp => jp.IsActive && (jp.ExpirationDate == null || jp.ExpirationDate > DateTime.Now));

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(jp => jp.Title.Contains(searchTerm) || jp.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(jp => jp.Location != null && jp.Location.Contains(location));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(jp => jp.JobPostCategories.Any(jpc => jpc.ID_Category == categoryId));
            }

            // Pagination
            int pageSize = 10;
            var totalJobs = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalJobs / pageSize);

            var jobs = await query
                .OrderByDescending(jp => jp.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Location = location;
            ViewBag.CategoryId = categoryId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = categories;

            return View(jobs);
        }

        // GET: Job/Details/5 - Chi tiết tin tuyển dụng
        public async Task<IActionResult> Details(int id)
        {
            var jobPost = await _context.JobPosts
                .Include(jp => jp.Employer)
                .Include(jp => jp.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
                .FirstOrDefaultAsync(jp => jp.ID_JobPost == id && jp.IsActive);

            if (jobPost == null)
            {
                return NotFound();
            }

            // Check if user has applied for this job
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(userId) && userRole == "Candidate")
            {
                var hasApplied = await _context.Applications
                    .AnyAsync(a => a.ID_JobPost == id && a.ID_Candidate == userId);
                ViewBag.HasApplied = hasApplied;
            }

            return View(jobPost);
        }

        // GET: Job/Create - Tạo tin tuyển dụng mới (chỉ Employer)
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId) || !string.Equals(userRole, "Employer", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Debug"] = $"UserID: {userId}, UserRole: {userRole}";
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
            if (string.IsNullOrEmpty(userId) || !string.Equals(userRole, "Employer", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Debug"] = $"UserID: {userId}, UserRole: {userRole}";
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
                return RedirectToAction("MyJobs");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // GET: Job/MyJobs - Tin tuyển dụng của Employer
        public async Task<IActionResult> MyJobs()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            
            if (userRole != "Employer" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var jobs = await _context.JobPosts
                .Include(jp => jp.Applications)
.Where(jp => jp.Employer.ID_Employer == userId)
                .OrderByDescending(jp => jp.PostedDate)
                .ToListAsync();

            return View(jobs);
        }

        // POST: Job/Apply/5 - Ứng tuyển công việc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            
            if (userRole != "Candidate" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if already applied
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.ID_JobPost == id && a.ID_Candidate == userId);

            if (existingApplication != null)
            {
                TempData["ErrorMessage"] = "Bạn đã ứng tuyển công việc này rồi!";
                return RedirectToAction("Details", new { id });
            }

            var application = new Application
            {
                ID_JobPost = id,
                ID_Candidate = userId ?? string.Empty,
                AppliedDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ứng tuyển thành công!";
            return RedirectToAction("Details", new { id });
        }

        // GET: Job/MyApplications - Đơn ứng tuyển của Candidate
        public async Task<IActionResult> MyApplications()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            
            if (userRole != "Candidate" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var applications = await _context.Applications
                .Include(a => a.JobPost)
                .ThenInclude(jp => jp.Employer)
                .Where(a => a.ID_Candidate == userId)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

        public IActionResult ManagePosts()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Job/Delete/5 - Xóa tin tuyển dụng (chỉ Employer là chủ sở hữu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId) || !string.Equals(userRole, "Employer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            var jobPost = await _context.JobPosts.FindAsync(id);
            if (jobPost == null)
            {
                return NotFound();
            }
            if (jobPost.ID_Employer != userId)
            {
                return Forbid(); // Không cho xóa nếu không phải chủ sở hữu
            }
            _context.JobPosts.Remove(jobPost);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa tin tuyển dụng thành công!";
            return RedirectToAction("MyJobs");
        }

        // GET: Job/Edit/5 - Sửa tin tuyển dụng (chỉ Employer là chủ sở hữu)
        public async Task<IActionResult> Edit(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId) || !string.Equals(userRole, "Employer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            var jobPost = await _context.JobPosts
                .Include(jp => jp.JobPostCategories)
                .FirstOrDefaultAsync(jp => jp.ID_JobPost == id);
            if (jobPost == null)
            {
                return NotFound();
            }
            if (jobPost.ID_Employer != userId)
            {
                return Forbid();
            }
            var model = new JobPostViewModel
            {
                ID_JobPost = jobPost.ID_JobPost,
                Title = jobPost.Title,
                Description = jobPost.Description,
                Requirements = jobPost.Requirements,
                Location = jobPost.Location,
                Salary = jobPost.Salary,
                ExpirationDate = jobPost.ExpirationDate,
                CategoryIds = jobPost.JobPostCategories.Select(jpc => jpc.ID_Category).ToList()
            };
            ViewBag.Categories = _context.Categories.ToList();
            return View("~/Views/Management/QuanLyDangTin/Create.cshtml", model);
        }

        // POST: Job/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(JobPostViewModel model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId) || !string.Equals(userRole, "Employer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }
            var jobPost = await _context.JobPosts
                .Include(jp => jp.JobPostCategories)
                .FirstOrDefaultAsync(jp => jp.ID_JobPost == model.ID_JobPost);
            if (jobPost == null)
            {
                return NotFound();
            }
            if (jobPost.ID_Employer != userId)
            {
                return Forbid();
            }
            // Cập nhật thông tin
            jobPost.Title = model.Title;
            jobPost.Description = model.Description;
            jobPost.Requirements = model.Requirements;
            jobPost.Location = model.Location;
            jobPost.Salary = model.Salary;
            jobPost.ExpirationDate = model.ExpirationDate;
            // Cập nhật ngành nghề
            jobPost.JobPostCategories.Clear();
            foreach (var categoryId in model.CategoryIds)
            {
                jobPost.JobPostCategories.Add(new JobPostCategory
                {
                    ID_JobPost = jobPost.ID_JobPost,
                    ID_Category = categoryId
                });
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
            return RedirectToAction("Details", new { id = jobPost.ID_JobPost });
        }
    }
} 