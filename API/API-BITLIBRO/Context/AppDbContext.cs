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
    public DbSet<Book> Books { get; set; }
    public DbSet<BookGenre> BookGenres { get; set; }
    public DbSet<Image> Images { get; set; }
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

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.Name).IsUnique();
            entity.HasIndex(b => b.ISBN).IsUnique();
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(b => b.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
        modelBuilder.Entity<BookGenre>()
            .HasKey(bg => new { bg.BookId, bg.GenreId });
        modelBuilder.Entity<BookGenre>()
            .HasOne(bg => bg.Book)
            .WithMany(b => b.BookGenres)
            .HasForeignKey(bg => bg.BookId);
        modelBuilder.Entity<BookGenre>()
            .HasOne(bg => bg.Genre)
            .WithMany(g => g.BookGenres)
            .HasForeignKey(bg => bg.GenreId);

        modelBuilder.Entity<Image>()
            .HasIndex(i => i.ImageUuid)
            .IsUnique();
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Book)
            .WithMany(b => b.Images)
            .HasForeignKey(i => i.BookId);

    }
}
