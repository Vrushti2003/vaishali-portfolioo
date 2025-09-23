using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VaishaliPortfolio.Models;

namespace VaishaliPortfolio.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactInquiry> ContactInquiries { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ContactInquiry table
            modelBuilder.Entity<ContactInquiry>(entity =>
            {
                entity.ToTable("ContactInquiries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(15).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // Configure BlogPost table
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable("BlogPosts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Summary).HasMaxLength(500);
                entity.Property(e => e.FeaturedImageUrl).HasMaxLength(255);
                entity.Property(e => e.AuthorId).IsRequired();
                entity.Property(e => e.AuthorName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
            });
        }
    }
}

       

    
