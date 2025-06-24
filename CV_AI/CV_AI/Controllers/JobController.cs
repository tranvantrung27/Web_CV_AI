using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CV_AI.Data;
using CV_AI.Models;
using CV_AI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CV_AI.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (User.Identity.IsAuthenticated && User.IsInRole("Candidate"))
            {
                var candidateId = HttpContext.Session.GetString("UserID");
                if (!string.IsNullOrEmpty(candidateId))
                {
                    var savedJobCount = _context.SavedJobs.Count(sj => sj.ID_Candidate == candidateId);
                    ViewBag.SavedJobCount = savedJobCount;
                }
            }
        }

        // GET: Job/Details/5 - Chi tiết tin tuyển dụng
        public async Task<IActionResult> Details(int id)
        {
            var jobPost = await _context.JobPosts
                .Include(j => j.Employer)
                .Include(j => j.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
                .FirstOrDefaultAsync(m => m.ID_JobPost == id);
    
            if (jobPost == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole == "Candidate" && !string.IsNullOrEmpty(userId))
            {
                ViewBag.HasApplied = await _context.Applications.AnyAsync(a => a.ID_JobPost == id && a.ID_Candidate == userId);
                ViewBag.IsSaved = await _context.SavedJobs.AnyAsync(sj => sj.ID_JobPost == id && sj.ID_Candidate == userId);
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

        [HttpPost]
        public async Task<IActionResult> ToggleSaveJob(int id)
        {
            var candidateId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Candidate" || string.IsNullOrEmpty(candidateId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập với tư cách ứng viên.", redirectTo = Url.Action("Login", "Account") });
            }

            var savedJob = await _context.SavedJobs
                .FirstOrDefaultAsync(sj => sj.ID_JobPost == id && sj.ID_Candidate == candidateId);

            bool isSaved;
            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                isSaved = false;
            }
            else
            {
                var newSavedJob = new SavedJob
                {
                    ID_JobPost = id,
                    ID_Candidate = candidateId,
                };
                _context.SavedJobs.Add(newSavedJob);
                isSaved = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, saved = isSaved });
        }

        public async Task<IActionResult> SavedJobs()
        {
            var candidateId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Candidate" || string.IsNullOrEmpty(candidateId))
            {
                return RedirectToAction("Login", "Account");
            }

            var savedJobPosts = await _context.SavedJobs
                .Where(sj => sj.ID_Candidate == candidateId)
                .Include(sj => sj.JobPost)
                    .ThenInclude(jp => jp.Employer)
                .Select(sj => sj.JobPost)
                .ToListAsync();
            
            ViewBag.SavedJobIds = savedJobPosts.Select(jp => jp.ID_JobPost).ToHashSet();

            return View(savedJobPosts);
        }
    }
} 