using System.ComponentModel.DataAnnotations;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class CreateMessageViewModel
    {
        [Required(ErrorMessage = "Alıcının e-posta adresini girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string ReceiverEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konu girin.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesajınızı yazın.")]
        public string MessageDetail { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Bir kategori seçin.")]
        public int CategoryId { get; set; }
    }
}