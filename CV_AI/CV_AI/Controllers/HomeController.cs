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

    public IActionResult Index()
    {
        var jobs = _context.JobPosts
            .Include(j => j.Employer)
            .Include(j => j.JobPostCategories)
                .ThenInclude(jpc => jpc.Category)
            .Where(j => j.IsActive && (j.ExpirationDate == null || j.ExpirationDate > DateTime.Now))
            .OrderByDescending(j => j.PostedDate)
            .Take(8)
            .ToList();
        return View(jobs);
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
