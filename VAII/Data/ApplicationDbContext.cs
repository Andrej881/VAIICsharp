using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VAII.Models.Entities;

namespace VAII.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GameTag> GameTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameTag>()
                .HasKey(gt => new { gt.GameID, gt.TagID });

            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Game)
                .WithMany(g => g.GameTags)
                .HasForeignKey(gt => gt.GameID);

            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Tag)
                .WithMany(t => t.GameTags)
                .HasForeignKey(gt => gt.TagID);
        }
    }
}
