using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponent
{
    public class _MessageLayoutStyleComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
