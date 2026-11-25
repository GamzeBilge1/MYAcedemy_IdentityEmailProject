using System.ComponentModel.DataAnnotations;

namespace MYAcedemy_IdentityEmailProject.Models
{
    public class ForgotPasswordViewModel
    {

        [Required(ErrorMessage = "E-posta adresinizi girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; } = string.Empty;
    }
}
