using System.ComponentModel.DataAnnotations;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class VerifyEmailViewModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Kod 6 rakam olmalı.")]
        public string Code { get; set; } = string.Empty;
        public DateTime? ValidCodeExpiresAt { get; set; }
    }
}
