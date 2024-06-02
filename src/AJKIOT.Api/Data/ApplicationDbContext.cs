using AJKIOT.Api.Models;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AJKIOT.Api.Data
{
    public class ApplicationDbContext : IdentityUserContext<ApplicationUser>
    {
        public DbSet<IotDevice> IotDevices { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
