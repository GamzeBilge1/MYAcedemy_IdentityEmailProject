using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponent
{
    public class _MessageLayoutNavbarComponentPartial : ViewComponent
    { 
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
