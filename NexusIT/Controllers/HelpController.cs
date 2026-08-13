using Microsoft.AspNetCore.Mvc;

namespace NexusIT.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}