using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
        {
            Random random = new Random();
            int validCode = random.Next(100000, 1000000);
            AppUser appUser = new AppUser()
            {
                Name = userRegisterViewModel.Name,
                SurName = userRegisterViewModel.Surname,
                Email=userRegisterViewModel.Email,
                UserName=userRegisterViewModel.Username,
                ValidCode= validCode,
                ValidCodeExpiresAt = DateTime.UtcNow.AddMinutes(1)
            };
            var result = await _userManager.CreateAsync(appUser, userRegisterViewModel.Password);

            if (result.Succeeded)
            {

                SendVerificationCode(userRegisterViewModel.Email, validCode);
                TempData["VerificationEmail"] = appUser.Email;
                return RedirectToAction("VerifyEmail");
            }
            else
            {
                return View();
            }
            
        }
        private void SendVerificationCode(string email, int validCode)
        {
            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom = new MailboxAddress("Mailora Yönetim Paneli", "bilgegamze01@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User", email);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = "Email Doğrulama Kodu";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Sisteme giriş yapabilmek için gerekli olan doğrulama kodu: " + validCode;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 587, false);
            smtpClient.Authenticate("bilgegamze01@gmail.com", "tetp ehzx jecs ywjx");
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCode()
        {
            var email = TempData.Peek("VerificationEmail") as string;

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("SignUp");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
                return RedirectToAction("SignIn", "Login");

            if (user.ValidCodeExpiresAt > DateTime.UtcNow)
                return RedirectToAction("VerifyEmail");

            int newCode = System.Security.Cryptography.RandomNumberGenerator
                .GetInt32(100000, 1000000);

            user.ValidCode = newCode;
            user.ValidCodeExpiresAt = DateTime.UtcNow.AddMinutes(1);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                SendVerificationCode(email, newCode);

            return RedirectToAction("VerifyEmail");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail()
        {
            var email = TempData.Peek("VerificationEmail") as string;

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("SignUp");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
                return RedirectToAction("SignIn", "Login");


            return View(new VerifyEmailViewModel { Email = email, ValidCodeExpiresAt = user.ValidCodeExpiresAt });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            var email = TempData.Peek("VerificationEmail") as string;

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("SignUp");

            model.Email = email;
            ModelState.Remove(nameof(model.Email));

            var user = await _userManager.FindByEmailAsync(email);
            model.ValidCodeExpiresAt = user?.ValidCodeExpiresAt;

            if (!ModelState.IsValid)
                return View(model);


            if (user == null ||
                user.ValidCode?.ToString() != model.Code ||
                user.ValidCodeExpiresAt == null ||
                user.ValidCodeExpiresAt <= DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(model.Code), "Kod yanlış veya süresi dolmuş.");
                return View(model);
            }

            user.EmailConfirmed = true;
            user.ValidCode = null;
            user.ValidCodeExpiresAt = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Doğrulama kaydedilemedi. Tekrar deneyin.");
                return View(model);
            }

            TempData.Remove("VerificationEmail");
            return RedirectToAction("SignIn", "Login");
        }
    }
}
