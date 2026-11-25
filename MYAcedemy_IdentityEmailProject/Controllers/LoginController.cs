using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;
using NuGet.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MailKit;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel)
        {
            var result = await _signInManager.PasswordSignInAsync(userLoginViewModel.UserName, userLoginViewModel.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Inbox", "Message");
            }
            
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, [FromServices] IConfiguration configuration)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && user.EmailConfirmed)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPath = Url.Action("ResetPassword", "Login", new
                {
                    email = user.Email,
                    token = token
                });
                var baseUrl = configuration["AppBaseUrl"];

                if (!string.IsNullOrWhiteSpace(baseUrl) && resetPath != null)
                {
                    var resetLink = $"{baseUrl.TrimEnd('/')}{resetPath}";
                    SendPasswordResetEmail(model.Email, resetLink);
                }
            }
            ViewData["ResetMessage"] =
            "Şifre yenileme bağlantısı gönderildi.";
            return View(model);
        }


        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Şifre yenileme bağlantısı geçersiz.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user, model.Token, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    var message = error.Code == "InvalidToken"
                        ? "Bu bağlantı geçersiz veya süresi dolmuş. Şifremi unuttum ekranından yeni bağlantı isteyin."
                        : error.Description;

                    ModelState.AddModelError("", message);
                }

                return View(model);
            }
            TempData["PasswordResetSuccess"] = "Şifreniz başarıyla değiştirildi.";
            return RedirectToAction("SignIn");
        }
        private void SendPasswordResetEmail(string email, string resetLink)
        {
            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom =
                new MailboxAddress("Mailora Yönetim Paneli", "bilgegamze01@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User", email);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = "Şifre Yenileme";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody =
                "Şifrenizi yenilemek için aşağıdaki bağlantıyı açın:\n" + resetLink;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 587, false);
            smtpClient.Authenticate("bilgegamze01@gmail.com", "tetp ehzx jecs ywjx");
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);
        }
    }
}
