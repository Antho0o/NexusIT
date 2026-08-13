using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Administrator)]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var rows = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                rows.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? "Unknown user",
                    Role = roles.FirstOrDefault() ?? AuthorizationRoles.Employee,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            ViewBag.Roles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
                return RedirectToAction(nameof(Index));

            var allowedRoles = new[]
            {
                AuthorizationRoles.Administrator,
                AuthorizationRoles.ITManager,
                AuthorizationRoles.ITTechnician,
                AuthorizationRoles.Employee
            };

            if (!allowedRoles.Contains(role))
            {
                TempData["AdminError"] = "That role is not available.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Prevent an administrator from accidentally removing their own admin access.
            if (user.Id == _userManager.GetUserId(User) && role != AuthorizationRoles.Administrator)
            {
                TempData["AdminError"] = "You cannot remove Administrator access from your own account.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                TempData["AdminError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["AdminSuccess"] = $"{user.Email} is now assigned to {role}.";
            return RedirectToAction(nameof(Index));
        }
    }

    public sealed class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = AuthorizationRoles.Employee;
        public bool EmailConfirmed { get; set; }
    }
}
