namespace MYAcedemy_IdentityEmailProject.Entities
{
    public class MessageDraft
    {
        public int MessageDraftId { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string? ReceiverEmail { get; set; }
        public string? Subject { get; set; }
        public string? MessageDetail { get; set; }
        public int? CategoryId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}