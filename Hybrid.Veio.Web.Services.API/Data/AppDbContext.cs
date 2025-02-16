using Hybrid.Veio.Names.Models;
using Microsoft.EntityFrameworkCore;
namespace Hybrid.Veio.Web.Services.API.Data
{



    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazioni aggiuntive per le tabelle (opzionale)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
        }
    }
}