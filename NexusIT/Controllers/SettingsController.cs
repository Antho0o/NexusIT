using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Administrator)]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting
                {
                    SystemName = "NexusIT",
                    OrganisationName = "",
                    Currency = "USD",
                    DateFormat = "dd MMM yyyy",
                    DefaultTicketPriority = "Medium",
                    DefaultTicketStatus = "Open"
                };

                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        // POST: Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemSetting settings)
        {
            if (!ModelState.IsValid)
            {
                return View(settings);
            }

            var existingSettings = await _context.SystemSettings
                .FirstOrDefaultAsync();

            if (existingSettings == null)
            {
                settings.UpdatedDate = DateTime.Now;

                _context.SystemSettings.Add(settings);
            }
            else
            {
                existingSettings.SystemName = settings.SystemName;
                existingSettings.OrganisationName = settings.OrganisationName;
                existingSettings.Currency = settings.Currency;
                existingSettings.DateFormat = settings.DateFormat;
                existingSettings.DefaultTicketPriority =
                    settings.DefaultTicketPriority;
                existingSettings.DefaultTicketStatus =
                    settings.DefaultTicketStatus;
                existingSettings.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "System settings updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}