using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CV_AI.Models;
using Microsoft.EntityFrameworkCore;
using CV_AI.Data;

namespace CV_AI.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index(int? categoryId, string? keyword, string? location)
    {
        var jobs = _context.JobPosts
            .Include(j => j.Employer)
            .Include(j => j.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
            .Where(j => j.IsActive && (j.ExpirationDate == null || j.ExpirationDate > DateTime.Now));

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            jobs = jobs.Where(j => j.JobPostCategories.Any(jpc => jpc.ID_Category == categoryId.Value));
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            jobs = jobs.Where(j =>
                j.Title.ToLower().Contains(lowerKeyword) ||
                j.Description.ToLower().Contains(lowerKeyword) ||
                j.Employer.CompanyName.ToLower().Contains(lowerKeyword) ||
                j.JobPostCategories.Any(jpc => jpc.Category.Name.ToLower().Contains(lowerKeyword))
            );
        }
        if (!string.IsNullOrWhiteSpace(location))
        {
            jobs = jobs.Where(j => j.Location.ToLower().Contains(location.ToLower()));
        }

        var jobList = jobs.OrderByDescending(j => j.PostedDate).Take(8).ToList();

        // Lấy danh sách categories để hiển thị trong dropdown tìm kiếm
        var categories = _context.Categories
            .OrderBy(c => c.Name)
            .ToList();

        ViewData["Categories"] = categories;
        return View(jobList);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
