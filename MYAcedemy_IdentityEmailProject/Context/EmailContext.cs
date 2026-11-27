using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MYAcedemy_IdentityEmailProject.Entities;

namespace MYAcedemy_IdentityEmailProject.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-VK9TPT1\\SQLEXPRESS; initial Catalog=IdentityEmailDb; integrated Security=true; trust server certificate=true");
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MessageDraft> MessageDrafts { get; set; }
    }
}
