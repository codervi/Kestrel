using Kestrel.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        b.Entity<Project>(e =>
        {
            e.Property(p => p.Title).HasMaxLength(120);
            e.Property(p => p.Location).HasMaxLength(100);
            e.Property(p => p.ClientName).HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(3000);
            e.Property(p => p.CoverImage).HasMaxLength(100);
            // don't let someone delete a category that still has projects
            e.HasOne(p => p.Category).WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.CategoryId, p.Year });
        });

        b.Entity<QuoteRequest>(e =>
        {
            e.Property(q => q.FullName).HasMaxLength(100);
            e.Property(q => q.Email).HasMaxLength(120);
            e.Property(q => q.Phone).HasMaxLength(30);
            e.Property(q => q.Company).HasMaxLength(100);
            e.Property(q => q.Budget).HasMaxLength(40);
            e.Property(q => q.Message).HasMaxLength(2000);
            e.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(q => q.Category).WithMany()
                .HasForeignKey(q => q.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
