using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcedemy_IdentityEmailProject.Context;
using MYAcedemy_IdentityEmailProject.Entities;

namespace MYAcedemy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponent
{
    public class _MessageLayoutSidebarComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutSidebarComponentPartial(
            EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            var email = user?.Email;

            ViewBag.InboxStarredCount = string.IsNullOrWhiteSpace(email)
       ? 0
       : await _context.Messages.CountAsync(m =>
           m.ReceiverEmail == email
           && m.IsFavorite
           && !m.IsTrashedByReceiver
           && !m.IsDeletedByReceiver);

            ViewBag.SentStarredCount = string.IsNullOrWhiteSpace(email)
                ? 0
                : await _context.Messages.CountAsync(m =>
                    m.SenderEmail == email
                    && m.IsSenderFavorite
                    && !m.IsTrashedBySender
                    && !m.IsDeletedBySender);
            ViewBag.DraftCount = user == null ? 0 : await _context.MessageDrafts.CountAsync(d => d.OwnerId == user.Id);
            return View();
        }
    }
}