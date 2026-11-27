using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcedemy_IdentityEmailProject.Context;
using MYAcedemy_IdentityEmailProject.Entities;
using MYAcedemy_IdentityEmailProject.Models;

namespace MYAcedemy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(
            EmailContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tab)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return Challenge();

            var email = user.Email;
            var messages = _context.Messages.AsNoTracking();

            var inbox = messages.Where(m =>
                m.ReceiverEmail == email
                && !m.IsTrashedByReceiver
                && !m.IsDeletedByReceiver);

            var sent = messages.Where(m =>
                m.SenderEmail == email
                && !m.IsTrashedBySender
                && !m.IsDeletedBySender);

            var unread = inbox.Where(m => !m.IsRead);

            var activeMessageIds = inbox.Select(m => m.MessageId)
                .Union(sent.Select(m => m.MessageId));

            var activeMessages = messages.Where(m =>
                activeMessageIds.Contains(m.MessageId));

            var selectedTab = tab is "inbox" or "sent" ? tab : "all";

            var recentQuery = selectedTab switch
            {
                "inbox" => inbox,
                "sent" => sent,
                _ => activeMessages
            };

            const int activityDays = 28;
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var threeDaysAgo = today.AddDays(-3);
            var activityStart = today.AddDays(1 - activityDays);

            var activityCounts = await activeMessages
                .Where(m => m.SendDate >= activityStart && m.SendDate < tomorrow)
                .GroupBy(m => m.SendDate.Date)
                .Select(day => new
                {
                    Date = day.Key,
                    Count = day.Count()
                })
                .ToDictionaryAsync(day => day.Date, day => day.Count);

            var dailyActivity = Enumerable.Range(0, activityDays)
                .Select(offset => activityStart.AddDays(offset))
                .Select(date => new DailyMessageActivityViewModel
                {
                    Date = date,
                    MessageCount = activityCounts.GetValueOrDefault(date)
                })
                .ToList();

            var model = new DashboardViewModel
            {
                InboxCount = await inbox.CountAsync(),
                SentCount = await sent.CountAsync(),
                UnreadCount = await unread.CountAsync(),

                StarredCount = await inbox
                    .Where(m => m.IsFavorite)
                    .Select(m => m.MessageId)
                    .Union(sent.Where(m => m.IsSenderFavorite)
                        .Select(m => m.MessageId))
                    .CountAsync(),

                DraftCount = await _context.MessageDrafts
                    .CountAsync(d => d.OwnerId == user.Id),

                SelectedTab = selectedTab,

                RecentMessages = await recentQuery
                    .OrderByDescending(m => m.SendDate)
                    .ThenByDescending(m => m.MessageId)
                    .Take(5)
                    .ToListAsync(),

                CategorySummaries = await _context.Categories
                    .Select(category => new CategorySummaryViewModel
                    {
                        CategoryName = category.CategoryName,
                        MessageCount = activeMessages.Count(m =>
                            m.CategoryId == category.CategoryId)
                    })
                    .OrderByDescending(category => category.MessageCount)
                    .ThenBy(category => category.CategoryName)
                    .ToListAsync(),

                DailyActivity = dailyActivity,

                UnreadTodayCount = await unread.CountAsync(m =>
                    m.SendDate >= today && m.SendDate < tomorrow),

                UnreadOneToThreeDaysCount = await unread.CountAsync(m =>
                    m.SendDate >= threeDaysAgo && m.SendDate < today),

                UnreadFourDaysOrMoreCount = await unread.CountAsync(m =>
                    m.SendDate < threeDaysAgo)
            };

            ViewData["UserEmail"] = email;
            return View(model);
        }
    }
}