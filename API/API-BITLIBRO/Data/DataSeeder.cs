using System;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;

namespace API_BITLIBRO.Data;

public static class DataSeeder
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        //crear roles
        string[] roles = { "Admin", "Employee", "Client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        //crear admin si no existe
        var adminUser = new User
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            Name = "Admin",
            LastName = "System",
            Ci = "1311220211",
            EmailConfirmed = true
        };
        string adminPassword = "Admin123!";
        var user = await userManager.FindByEmailAsync(adminUser.Email);
        if (user == null)
        {
            var createResult= await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception("Error creating admin user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

    }
}
