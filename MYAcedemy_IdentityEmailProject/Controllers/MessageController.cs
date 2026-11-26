using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MYAcedemy_IdentityEmailProject.Context;
using MYAcedemy_IdentityEmailProject.Entities;

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
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var message = await _context.Messages.FirstOrDefaultAsync(m =>
                m.MessageId == id && m.ReceiverEmail == user.Email);

            if (message == null)
                return NotFound();

            return View(message);
        }

        //  [Authorize(Roles = "Admin")]
        public IActionResult CreateMessage()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        public IActionResult CreateMessage(Message message)
        {
            return RedirectToAction("Sendbox");
        }

        [Authorize]
        public async Task<IActionResult> Sendbox()
        {
            var user = await _userManager.GetUserAsync(User);

            var values = _context.Messages
                .Where(x => x.SenderEmail == user.Email)
                .OrderByDescending(x => x.SendDate)
                .ToList();

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
    }
}
