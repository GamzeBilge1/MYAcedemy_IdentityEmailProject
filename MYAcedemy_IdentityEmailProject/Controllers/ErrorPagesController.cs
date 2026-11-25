using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    public class ErrorPagesController : Controller
    {
        public IActionResult Page401()
        {
            Response.StatusCode = 401;
            return View();
        }
        public IActionResult Page403()
        {
            Response.StatusCode = 403;
            return View();
        }
        public IActionResult Page404()
        {
            return View();
        }
    }
}
