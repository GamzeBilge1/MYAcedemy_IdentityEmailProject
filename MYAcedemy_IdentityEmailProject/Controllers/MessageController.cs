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


            var messages = await _context.Messages
               .Where(m => m.ReceiverEmail == user.Email && !m.IsTrashedByReceiver&& !m.IsDeletedByReceiver)
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
        public async Task<IActionResult> CreateMessage(CreateMessageViewModel model, int? draftId,[FromServices] IConfiguration configuration)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();
            MessageDraft? draft = null;

            if (draftId.HasValue)
            {
                draft = await _context.MessageDrafts.FirstOrDefaultAsync(d =>
                    d.MessageDraftId == draftId.Value && d.OwnerId == user.Id);

                if (draft == null)
                    return NotFound();

                ViewBag.DraftId = draft.MessageDraftId;
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories.ToList(),
                    "CategoryId",
                    "CategoryName");

                return View(model);
            }
         
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

            if (draft != null)
            {
                _context.MessageDrafts.Remove(draft);
                await _context.SaveChangesAsync();
            }

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
              .Where(m => m.SenderEmail == user.Email && !m.IsTrashedBySender && !m.IsDeletedBySender)
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
        [HttpGet]
        public async Task<IActionResult> Starred()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var messages = await _context.Messages
               .Where(m => (m.ReceiverEmail == user.Email && m.IsFavorite && !m.IsTrashedByReceiver && !m.IsDeletedByReceiver) || (m.SenderEmail == user.Email && m.IsSenderFavorite && !m.IsTrashedBySender && !m.IsDeletedBySender))
                .OrderByDescending(m => m.SendDate)
                .ToListAsync();

            ViewData["UserEmail"] = user.Email;
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            return View(messages);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(CreateMessageViewModel model, int? draftId)
        {
            var ownerId = _userManager.GetUserId(User);

            if (ownerId == null)
                return Challenge();

            if (draftId.HasValue)
            {
                var existingDraft = await _context.MessageDrafts.FirstOrDefaultAsync(d =>
                    d.MessageDraftId == draftId.Value && d.OwnerId == ownerId);

                if (existingDraft == null)
                    return NotFound();

                existingDraft.ReceiverEmail = model.ReceiverEmail;
                existingDraft.Subject = model.Subject;
                existingDraft.MessageDetail = model.MessageDetail;
                existingDraft.CategoryId = model.CategoryId > 0 ? model.CategoryId : null;
                existingDraft.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["DraftSaved"] = "Taslak başarıyla güncellendi.";
                return RedirectToAction("CreateMessage");
            }

            var draft = new MessageDraft { 
                OwnerId =  ownerId, 
                ReceiverEmail = model.ReceiverEmail,
                Subject=model.Subject,
                MessageDetail = model.MessageDetail,
                CategoryId = model.CategoryId > 0 ? model.CategoryId : null,
                UpdatedAt= DateTime.UtcNow,
            };

            _context.MessageDrafts.Add(draft);
            await _context.SaveChangesAsync();
            
            TempData["DraftSaved"] = "Taslak başarıyla kaydedildi.";

            return RedirectToAction("CreateMessage");
        }
        [HttpGet]
        public async Task<IActionResult> Drafts()
        {
            var ownerId = _userManager.GetUserId(User);

            if (ownerId == null)
                return Challenge();

            var drafts = await _context.MessageDrafts
                .Where(draft => draft.OwnerId == ownerId)
                .OrderByDescending(draft => draft.UpdatedAt)
                .ToListAsync();

            return View(drafts);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditDraft(int id)
        {
            var ownerId = _userManager.GetUserId(User);

            var draft = await _context.MessageDrafts.FirstOrDefaultAsync(d =>
                d.MessageDraftId == id && d.OwnerId == ownerId);

            if (draft == null)
                return NotFound();

            var model = new CreateMessageViewModel
            {
                ReceiverEmail = draft.ReceiverEmail ?? "",
                Subject = draft.Subject ?? "",
                MessageDetail = draft.MessageDetail ?? "",
                CategoryId = draft.CategoryId ?? 0
            };

            ViewBag.Categories = new SelectList(
                _context.Categories.ToList(), "CategoryId", "CategoryName");

            ViewBag.DraftId = draft.MessageDraftId;

            return View("CreateMessage", model);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDrafts(int[] ids)
        {
            var ownerId = _userManager.GetUserId(User);

            if (ownerId == null)
                return Challenge();

            var drafts = await _context.MessageDrafts
                .Where(d => d.OwnerId == ownerId && ids.Contains(d.MessageDraftId))
                .ToListAsync();

            _context.MessageDrafts.RemoveRange(drafts);
            await _context.SaveChangesAsync();

            return RedirectToAction("Drafts");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveInboxToTrash(int[] ids)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var messages = await _context.Messages
                .Where(m => ids.Contains(m.MessageId)
                    && m.ReceiverEmail == user.Email
                    && !m.IsTrashedByReceiver
                    && !m.IsDeletedByReceiver)
                .ToListAsync();

            foreach (var message in messages)
                message.IsTrashedByReceiver = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSentToTrash(int[] ids)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var messages = await _context.Messages
                .Where(m => ids.Contains(m.MessageId)
                    && m.SenderEmail == user.Email
                    && !m.IsTrashedBySender
                    && !m.IsDeletedBySender)
                .ToListAsync();

            foreach (var message in messages)
                message.IsTrashedBySender = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var messages = await _context.Messages
                .Where(m =>
                    (m.ReceiverEmail == user.Email
                        && m.IsTrashedByReceiver
                        && !m.IsDeletedByReceiver)
                    ||
                    (m.SenderEmail == user.Email
                        && m.IsTrashedBySender
                        && !m.IsDeletedBySender))
                .OrderByDescending(m => m.SendDate)
                .ToListAsync();

            return View(messages);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RestoreTrashMessages(int[] ids)
        {
            return UpdateTrashMessages(ids, delete: false);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DeleteTrashMessages(int[] ids)
        {
            return UpdateTrashMessages(ids, delete: true);
        }

        private async Task<IActionResult> UpdateTrashMessages(int[] ids, bool delete)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            if (ids == null || ids.Length == 0)
                return RedirectToAction("Trash");

            var email = user.Email;
            var messages = await _context.Messages
                .Where(m => ids.Contains(m.MessageId) &&
                    ((m.ReceiverEmail == email && m.IsTrashedByReceiver && !m.IsDeletedByReceiver)
                    || (m.SenderEmail == email && m.IsTrashedBySender && !m.IsDeletedBySender)))
                .ToListAsync();

            foreach (var message in messages)
            {
                if (message.ReceiverEmail == email && message.IsTrashedByReceiver && !message.IsDeletedByReceiver)
                {
                    message.IsTrashedByReceiver = false;
                    message.IsDeletedByReceiver = delete;
                }

                if (message.SenderEmail == email && message.IsTrashedBySender && !message.IsDeletedBySender)
                {
                    message.IsTrashedBySender = false;
                    message.IsDeletedBySender = delete;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Trash");
        }

    }

}
