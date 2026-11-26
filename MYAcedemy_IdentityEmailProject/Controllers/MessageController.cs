using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MYAcedemy_IdentityEmailProject.Context;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;
using System.Net.Mail;
using MailKit;
using MailKit.Net.Smtp;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;
        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var email = user.Email;

            var messages = await _context.Messages
                .Where(message => message.ReceiverEmail == email)
                .OrderByDescending(message => message.SendDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(category => category.CategoryName).ToListAsync();

            return View(messages);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id, bool sent = false)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.MessageId == id &&(sent ? m.SenderEmail ==user.Email : m.ReceiverEmail == user.Email));

            if (message == null)
                return NotFound();

            ViewData["IsSent"] = sent; 

            return View(message);
        }

        //  [Authorize(Roles = "Admin")]
        public IActionResult CreateMessage()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageViewModel model, [FromServices] IConfiguration configuration)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories.ToList(),
                    "CategoryId",
                    "CategoryName");

                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var message = new Message
            {
                SenderEmail = user.Email,
                ReceiverEmail = model.ReceiverEmail,
                Subject = model.Subject,
                MessageDetail = model.MessageDetail,
                CategoryId = model.CategoryId,
                SendDate = DateTime.Now,
                IsRead = false,
                IsFavorite = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await SendEmailAsync(message.ReceiverEmail, message.Subject, message.MessageDetail, configuration); 

            return RedirectToAction("Sendbox");
        }
        private async Task SendEmailAsync(string receiverEmail, string subject, string text, IConfiguration configuration)
        {
            var senderEmail = configuration["Smtp:Email"]
                ?? throw new InvalidOperationException("SMTP e-posta adresi eksik.");
            var password = configuration["Smtp:Password"]
                ?? throw new InvalidOperationException("SMTP uygulama şifresi eksik.");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Mailora", senderEmail));
            email.To.Add(MailboxAddress.Parse(receiverEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = text };

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(senderEmail, password);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }

        [Authorize]
        public async Task<IActionResult> Sendbox()
        {
            var user = await _userManager.GetUserAsync(User);

            var values = _context.Messages
                .Where(x => x.SenderEmail == user.Email)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            return View(values);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetReadStatus(int[] ids, bool isRead)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var selectedIds = ids.Distinct().ToArray();

            var messages = await _context.Messages
                .Where(m => selectedIds.Contains(m.MessageId) && m.ReceiverEmail == user.Email)
                .ToListAsync();

            if (messages.Count != selectedIds.Length)
                return NotFound();

            foreach (var message in messages)
                message.IsRead = isRead;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFavorite(int id, bool starred)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var message = await _context.Messages.FirstOrDefaultAsync(m =>
                m.MessageId == id && m.ReceiverEmail == user.Email);

            if (message == null)
                return NotFound();

            message.IsFavorite = starred;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSentFavorite(int id, bool starred)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var message = await _context.Messages.FirstOrDefaultAsync(m =>
                m.MessageId == id && m.SenderEmail == user.Email);

            if (message == null)
                return NotFound();

            message.IsSenderFavorite = starred;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
