using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponent
{
    public class _MessageLayoutScriptComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
