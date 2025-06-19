using Microsoft.AspNetCore.Mvc;

namespace CV_AI.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
