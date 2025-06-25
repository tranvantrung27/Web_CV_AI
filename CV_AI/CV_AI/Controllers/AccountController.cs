using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CV_AI.Data;
using CV_AI.Models;
using CV_AI.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CV_AI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == hashedPassword && u.IsActive);

                if (user != null)
                {
                    // Set session
                    HttpContext.Session.SetString("UserID", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    // Mặc dù dùng session, vẫn tạo cookie xác thực để [Authorize] hoạt động
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                }
            }
            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    Role = model.Role,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    FullName = model.FullName
                };

                _context.Users.Add(user);

                // Create role-specific record
                if (model.Role == "Candidate")
                {
                    var candidate = new Candidate
                    {
                        ID_Candidate = user.Id,
                        Phone = model.Phone
                    };
                    _context.Candidates.Add(candidate);
                }
                else if (model.Role == "Employer")
                {
                    var employer = new Employer
                    {
                        ID_Employer = user.Id,
                        CompanyName = model.CompanyName ?? "",
                        CompanyWebsite = model.CompanyWebsite,
                        CompanyAddress = model.CompanyAddress,
                        Description = model.Description
                    };
                    _context.Employers.Add(employer);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi xác thực: {remoteError}");
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Đăng nhập nếu đã có tài khoản liên kết
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.Role))
                    {
                        user.Role = "Candidate";
                        await _userManager.UpdateAsync(user);
                    }
                    HttpContext.Session.SetString("UserID", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Nếu chưa có user, tạo mới user ở đây
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = name ?? email,
                        Role = "Candidate",
                        IsActive = true
                    };
                    var identityResult = await _userManager.CreateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản Google.");
                        return RedirectToAction("Login");
                    }
                }
                else if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "Candidate";
                    await _userManager.UpdateAsync(user);
                }

                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Set session
                HttpContext.Session.SetString("UserID", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserName", user.FullName);

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }
            var user = await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Employer)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Employer)
                .FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.FullName = model.FullName;
            user.Email = model.Email;
            // Cập nhật thông tin ứng viên
            if (user.Role == "Candidate" && user.Candidate != null)
            {
                user.Candidate.Phone = model.Candidate?.Phone;
                user.Candidate.CV_Link = model.Candidate?.CV_Link;
            }
            // Cập nhật thông tin nhà tuyển dụng
            if (user.Role == "Employer" && user.Employer != null)
            {
                user.Employer.CompanyName = model.Employer?.CompanyName;
                user.Employer.CompanyWebsite = model.Employer?.CompanyWebsite;
                user.Employer.CompanyAddress = model.Employer?.CompanyAddress;
                user.Employer.Description = model.Employer?.Description;
            }
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 