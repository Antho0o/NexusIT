using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NexusIT.Data
{
    public static class RoleSeeder
    {
        private static readonly string[] Roles =
        {
            AuthorizationRoles.Administrator,
            AuthorizationRoles.ITManager,
            AuthorizationRoles.ITTechnician,
            AuthorizationRoles.Employee
        };

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to create NexusIT role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Existing accounts that have never been assigned a role become Employees.
            // Administrators can promote them from the NexusIT User Management page.
            var users = await userManager.Users
                .OrderBy(u => u.Id)
                .ToListAsync();

            // Ensure an existing development database always has an administrator.
            // The first registered account becomes the initial administrator only when
            // no administrator exists yet. Subsequent unassigned accounts are Employees.
            var hasAdministrator = false;
            foreach (var user in users)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                if (userRoles.Contains(AuthorizationRoles.Administrator))
                {
                    hasAdministrator = true;
                    break;
                }
            }

            foreach (var user in users)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                if (userRoles.Count > 0)
                    continue;

                var role = !hasAdministrator
                    ? AuthorizationRoles.Administrator
                    : AuthorizationRoles.Employee;

                await userManager.AddToRoleAsync(user, role);
                if (role == AuthorizationRoles.Administrator)
                    hasAdministrator = true;
            }
        }
    }
}
