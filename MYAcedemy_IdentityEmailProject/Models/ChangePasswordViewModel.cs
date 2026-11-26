using System.ComponentModel.DataAnnotations;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi girin.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrenizi girin.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrenizi tekrar girin.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
