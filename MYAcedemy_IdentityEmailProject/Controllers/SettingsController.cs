using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;


        public SettingsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            FillAccountInfo(user);

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            FillAccountInfo(user);

            if (!ModelState.IsValid)
                return View(model);

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError(
                            nameof(model.CurrentPassword), "Mevcut şifreniz yanlış.");
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordChanged"] = "Şifreniz başarıyla değiştirildi.";

            return RedirectToAction(nameof(ChangePassword));
        }

        private void FillAccountInfo(AppUser user)
        {
            ViewBag.FullName = $"{user.Name} {user.SurName}".Trim();
            ViewBag.UserName = user.UserName;
            ViewBag.Email = user.Email;
            ViewBag.EmailConfirmed = user.EmailConfirmed;
        }
    }
}
