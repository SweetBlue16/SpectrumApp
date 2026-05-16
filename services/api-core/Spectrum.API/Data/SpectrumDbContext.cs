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
        /// Gets or sets the data set for the gaming platforms available.
        /// </summary>
        public DbSet<Platform> Platforms { get; set; }

        /// <summary>
        /// Gets or sets the data set for user-uploaded video clips.
        /// </summary>
        public DbSet<GameClip> GameClips { get; set; }

        /// <summary>
        /// Configures the relational database schema, entity relationships, and global query filters
        /// using the Fluent API. These configurations override data annotation attributes.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the database schema model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasQueryFilter(user => !user.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(review => !review.IsDeleted);

            modelBuilder.Entity<AdminDetail>()
                .HasOne(adminDetail => adminDetail.User)
                .WithOne(user => user.AdminDetail)
                .HasForeignKey<AdminDetail>(adminDetail => adminDetail.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasOne(review => review.User)
                .WithMany()
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .Property(review => review.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            modelBuilder.Entity<Review>()
                .Property(review => review.UpdatedAt)
                .HasColumnName("updated_at");

            modelBuilder.Entity<Review>()
                .Property(review => review.LikesCount)
                .HasColumnName("likes_count");

            modelBuilder.Entity<Review>()
                .Property(review => review.DislikesCount)
                .HasColumnName("dislikes_count");

            modelBuilder.Entity<Platform>().HasData(
                new Platform { Id = 1, Name = "PC" },
                new Platform { Id = 2, Name = "PlayStation" },
                new Platform { Id = 3, Name = "Xbox" },
                new Platform { Id = 4, Name = "Nintendo" },
                new Platform { Id = 5, Name = "Phone" }
            );

            modelBuilder.Entity<GameClip>()
                .HasOne(gameClip => gameClip.User)
                .WithMany()
                .HasForeignKey(gameClip => gameClip.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClip>()
                .HasOne(gameClip => gameClip.Game)
                .WithMany()
                .HasForeignKey(gameClip => gameClip.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClip>()
                .Property(gameClip => gameClip.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            modelBuilder.Entity<GameClip>()
                .Property(gameClip => gameClip.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            modelBuilder.Entity<GameClip>()
                .Property(gameClip => gameClip.CreatedAt)
                .HasColumnName("created_at");

            modelBuilder.Entity<GameClip>()
                .Property(gameClip => gameClip.Title)
                .HasColumnName("title")
                .IsRequired();

            modelBuilder.Entity<GameClip>()
                .Property(gameClip => gameClip.Description)
                .HasColumnName("description");
        }
    }
}