using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.ErrorLayoutViewComponent
{
    public class _ErrorLayoutStyleComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
