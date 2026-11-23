using Microsoft.AspNetCore.Identity;

namespace MYAcedemy_IdentityEmailProject.Entities
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? SurName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? ValidCode { get; set; }
    }
}
