using Microsoft.AspNetCore.Mvc;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.RegisterViewComponents
{
    public class _RegisterScriptComponentPartial :ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
