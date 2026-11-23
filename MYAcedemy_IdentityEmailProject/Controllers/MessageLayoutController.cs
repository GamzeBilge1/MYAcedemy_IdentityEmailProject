using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    public class MessageLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
