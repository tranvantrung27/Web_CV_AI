using Microsoft.AspNetCore.Mvc;

namespace CV_AI.Controllers
{
    public class CVController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
