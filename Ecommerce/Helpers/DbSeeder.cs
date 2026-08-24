using Ecommerce.Models;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var adminUser =
                await userManager.FindByEmailAsync("admin@gmail.com");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    FirstName = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    adminUser,
                    "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        adminUser,
                        Roles.Admin);
                }
            }
        }
    }
}