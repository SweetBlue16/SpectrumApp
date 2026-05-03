using Microsoft.EntityFrameworkCore;
using Spectrum.API.Models;

namespace Spectrum.API.Data
{
    /// <summary>
    /// Represents the primary Entity Framework Core session with the relational database.
    /// Responsible for coordinating the Unit of Work, managing entity states, and 
    /// enforcing referential integrity and database-level constraints.
    /// </summary>
    public class SpectrumDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumDbContext"/> class 
        /// with the specified configuration options.
        /// </summary>
        /// <param name="options">The configuration options for this context instance, such as the database provider.</param>
        public SpectrumDbContext(DbContextOptions<SpectrumDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the data set for the system's registered users.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the data set for the locally tracked video game catalog.
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <summary>
        /// Gets or sets the data set for user-generated game reviews.
        /// </summary>
        public DbSet<Review> Reviews { get; set; }

        /// <summary>
        /// Gets or sets the data set containing sensitive, extended profile data for administrators.
        /// </summary>
        public DbSet<AdminDetail> AdminDetails { get; set; }

        /// <summary>
        /// Configures the relational database schema, entity relationships, and global query filters 
        /// using the Fluent API. These configurations override data annotation attributes.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the database schema model.</param>
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
