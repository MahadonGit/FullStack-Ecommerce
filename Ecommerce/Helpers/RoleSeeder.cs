using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Infrastructure.Constants;
namespace Ecommerce.Helpers
{
    public static class RoleSeeder
    {


        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            //Go into the service provider and get the role manager
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //these are our roles that we want to create in the database if they don't exist
            string[] roles = { Roles.Customer, Roles.Manager, Roles.Admin };

            //loop logic is if role does not wxist create it

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }

            }



        }


    }
}
