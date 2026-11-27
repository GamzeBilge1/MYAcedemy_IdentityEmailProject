using System;
using System.Collections.Generic;
using MYAcedemy_IdentityEmailProject.Entities;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class DashboardViewModel
    {
        public int InboxCount { get; set; }
        public int SentCount { get; set; }
        public int UnreadCount { get; set; }
        public int StarredCount { get; set; }
        public int DraftCount { get; set; }

        public int TodayInboxCount { get; set; }
        public int TodaySentCount { get; set; }

        public string SelectedTab { get; set; } = "all";
        public List<Message> RecentMessages { get; set; } = new();
        public List<CategorySummaryViewModel> CategorySummaries { get; set; } = new();
        public List<FrequentContactViewModel> FrequentContacts { get; set; } = new();

        public List<DailyMessageActivityViewModel> DailyActivity { get; set; } = new();
        public int UnreadTodayCount { get; set; }
        public int UnreadOneToThreeDaysCount { get; set; }
        public int UnreadFourDaysOrMoreCount { get; set; }
    }

    public class CategorySummaryViewModel
    {
        public string CategoryName { get; set; } = "";
        public int MessageCount { get; set; }
    }

    public class FrequentContactViewModel
    {
        public string Email { get; set; } = "";
        public int ReceivedCount { get; set; }
        public int SentCount { get; set; }
    }

    public class DailyMessageActivityViewModel
    {
        public DateTime Date { get; set; }
        public int MessageCount { get; set; }
    }
}