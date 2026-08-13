using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Management)]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // =========================
            // EMPLOYEES
            // =========================

            var totalEmployees =
                await _context.Employees.CountAsync();

            var activeEmployees =
                await _context.Employees
                    .CountAsync(e => e.IsActive);

            var inactiveEmployees =
                totalEmployees - activeEmployees;


            // =========================
            // ASSETS
            // =========================

            var totalAssets =
                await _context.Assets.CountAsync();

            var assignedAssets =
                await _context.Assets
                    .CountAsync(a => a.EmployeeId != null);

            var availableAssets =
                await _context.Assets
                    .CountAsync(a => a.Status == "Available");

            var maintenanceAssets =
                await _context.Assets
                    .CountAsync(a => a.Status == "Maintenance");


            // =========================
            // SUPPORT TICKETS
            // =========================

            var totalTickets =
                await _context.SupportTickets.CountAsync();

            var openTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Status == "Open");

            var inProgressTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Status == "In Progress");

            var waitingTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Status == "Waiting");

            var closedTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Status == "Closed");

            var highPriorityTickets =
                await _context.SupportTickets
                    .CountAsync(t =>
                        (t.Priority == "High" ||
                         t.Priority == "Critical") &&
                        t.Status != "Closed");


            // =========================
            // TICKET PRIORITIES
            // =========================

            var lowPriority =
                await _context.SupportTickets
                    .CountAsync(t => t.Priority == "Low");

            var mediumPriority =
                await _context.SupportTickets
                    .CountAsync(t => t.Priority == "Medium");

            var highPriority =
                await _context.SupportTickets
                    .CountAsync(t => t.Priority == "High");

            var criticalPriority =
                await _context.SupportTickets
                    .CountAsync(t => t.Priority == "Critical");


            // =========================
            // TICKET CATEGORIES
            // =========================

            var hardwareTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Hardware");

            var softwareTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Software");

            var networkTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Network");

            var securityTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Security");

            var emailTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Email");

            var accessTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Access");

            var printerTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "Printer");

            var generalTickets =
                await _context.SupportTickets
                    .CountAsync(t => t.Category == "General");


            // =========================
            // MAINTENANCE
            // =========================

            var totalMaintenance =
                await _context.MaintenanceRecords.CountAsync();

            var scheduledMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "Scheduled");

            var inProgressMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "In Progress");

            var completedMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "Completed");

            var maintenanceCost =
                await _context.MaintenanceRecords
                    .SumAsync(m => (decimal?)m.Cost) ?? 0;


            // =========================
            // RECENT TICKETS
            // =========================

            var recentTickets =
                await _context.SupportTickets
                    .Include(t => t.Employee)
                    .Include(t => t.Asset)
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(8)
                    .ToListAsync();


            // =========================
            // VIEWBAG
            // =========================

            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.ActiveEmployees = activeEmployees;
            ViewBag.InactiveEmployees = inactiveEmployees;

            ViewBag.TotalAssets = totalAssets;
            ViewBag.AssignedAssets = assignedAssets;
            ViewBag.AvailableAssets = availableAssets;
            ViewBag.MaintenanceAssets = maintenanceAssets;

            ViewBag.TotalTickets = totalTickets;
            ViewBag.OpenTickets = openTickets;
            ViewBag.InProgressTickets = inProgressTickets;
            ViewBag.WaitingTickets = waitingTickets;
            ViewBag.ClosedTickets = closedTickets;
            ViewBag.HighPriorityTickets = highPriorityTickets;

            ViewBag.LowPriority = lowPriority;
            ViewBag.MediumPriority = mediumPriority;
            ViewBag.HighPriority = highPriority;
            ViewBag.CriticalPriority = criticalPriority;

            ViewBag.HardwareTickets = hardwareTickets;
            ViewBag.SoftwareTickets = softwareTickets;
            ViewBag.NetworkTickets = networkTickets;
            ViewBag.SecurityTickets = securityTickets;
            ViewBag.EmailTickets = emailTickets;
            ViewBag.AccessTickets = accessTickets;
            ViewBag.PrinterTickets = printerTickets;
            ViewBag.GeneralTickets = generalTickets;

            ViewBag.TotalMaintenance = totalMaintenance;
            ViewBag.ScheduledMaintenance = scheduledMaintenance;
            ViewBag.InProgressMaintenance = inProgressMaintenance;
            ViewBag.CompletedMaintenance = completedMaintenance;
            ViewBag.MaintenanceCost = maintenanceCost;

            ViewBag.RecentTickets = recentTickets;

            return View();
        }
    }
}