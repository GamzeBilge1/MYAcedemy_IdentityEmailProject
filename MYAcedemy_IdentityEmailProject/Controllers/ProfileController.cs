using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var model = new EditProfileViewModel
            {
                Name = user.Name ?? "",
                SurName = user.SurName ?? "",
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                Description = user.Description,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProfileViewModel model,
            [FromServices] SignInManager<AppUser> signInManager,
            [FromServices] IWebHostEnvironment environment)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            model.ImageUrl = user.ImageUrl;

            if (!ModelState.IsValid)
                return View(model);

            string? uploadedPath = null;

            if (model.ProfilePhoto is { } photo)
            {
                const int maxPhotoSize = 2 * 1024 * 1024;

                if (photo.Length == 0 || photo.Length > maxPhotoSize)
                {
                    ModelState.AddModelError(nameof(model.ProfilePhoto),
                        "En fazla 2 MB boyutunda bir fotoğraf seçin.");
                    return View(model);
                }

                await using var input = photo.OpenReadStream();
                var header = new byte[8];
                var bytesRead = await input.ReadAtLeastAsync(
                    header, header.Length, throwOnEndOfStream: false);

                var isJpeg = bytesRead >= 3
                    && header[0] == 0xFF
                    && header[1] == 0xD8
                    && header[2] == 0xFF;

                byte[] pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
                var isPng = bytesRead == header.Length && header.SequenceEqual(pngSignature);

                if (!isJpeg && !isPng)
                {
                    ModelState.AddModelError(nameof(model.ProfilePhoto),
                        "JPG veya PNG biçiminde bir fotoğraf seçin.");
                    return View(model);
                }

                var folder = Path.Combine(environment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(folder);

                var extension = isJpeg ? ".jpg" : ".png";
                var fileName = $"{Guid.NewGuid():N}{extension}";
                uploadedPath = Path.Combine(folder, fileName);

                await using (var output = System.IO.File.Create(uploadedPath))
                {
                    await output.WriteAsync(header.AsMemory(0, bytesRead));
                    await input.CopyToAsync(output);
                }

                user.ImageUrl = $"~/uploads/profiles/{fileName}";
            }

            user.Name = model.Name.Trim();
            user.SurName = model.SurName.Trim();
            user.UserName = model.UserName.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)  ? null : model.PhoneNumber.Trim();

            if (user.PhoneNumber != phoneNumber)
            {
                user.PhoneNumber = phoneNumber;
                user.PhoneNumberConfirmed = false;
            }

            user.Description = model.Description?.Trim();
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                if (uploadedPath != null)
                    System.IO.File.Delete(uploadedPath);

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Edit");
        }
    }
}