using Hoot.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hoot.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<ClientCorsOrigin> ClientCorsOrigin { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientSecret> ClientSecret { get; set; }
        public DbSet<ClientScope> ClientScope { get; set; }
        public DbSet<ClientPostLogoutRedirectUri> ClientPostLogoutRedirectUri { get; set; }
        public DbSet<ClientRedirectUri> ClientRedirectUri { get; set; }
    }
}
