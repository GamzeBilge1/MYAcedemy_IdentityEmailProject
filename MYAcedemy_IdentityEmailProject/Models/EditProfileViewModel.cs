using System.ComponentModel.DataAnnotations;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Adınızı girin.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Soyadınızı girin.")]
        public string SurName { get; set; } = "";

        [Required(ErrorMessage = "Kullanıcı adınızı girin.")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "E-posta adresinizi girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; } = "";
        public string? ImageUrl { get; set; }
        public IFormFile? ProfilePhoto { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        public string? PhoneNumber { get; set; }

        [StringLength(250, ErrorMessage = "Hakkımda alanı en fazla 250 karakter olabilir.")]
        public string? Description { get; set; }
    }
}
