using System;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Context;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<Genre> Genres { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Esto es CRUCIAL para Identity
        modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasIndex(g => g.Name).IsUnique();
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(g => g.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
    }
}
