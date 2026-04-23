using Microsoft.EntityFrameworkCore;
using Spectrum.API.Models;

namespace Spectrum.API.Data
{
    public class SpectrumDbContext : DbContext
    {
        public SpectrumDbContext(DbContextOptions<SpectrumDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AdminDetail> AdminDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<AdminDetail>()
                .HasOne(ad => ad.User)
                .WithOne(u => u.AdminDetail)
                .HasForeignKey<AdminDetail>(ad => ad.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
