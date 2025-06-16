using System;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Context;

public class AppDbContext:IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aquí puedes agregar configuraciones adicionales para tus entidades
        // Por ejemplo, si tienes una entidad llamada "Product":
        // modelBuilder.Entity<Product>().ToTable("Products");
    }
}
