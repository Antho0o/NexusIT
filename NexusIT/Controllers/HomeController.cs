using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models.ViewModels;

namespace NexusIT.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var vm = new DashboardViewModel
            {
                TotalEmployees = await _context.Employees.CountAsync(),
                ActiveEmployees = await _context.Employees.CountAsync(e => e.IsActive),

                TotalAssets = await _context.Assets.CountAsync(),
                AssignedAssets = await _context.Assets.CountAsync(a => a.EmployeeId != null),
                AvailableAssets = await _context.Assets.CountAsync(a => a.Status == "Available" || a.EmployeeId == null),
                MaintenanceAssets = await _context.Assets.CountAsync(a => a.Status == "Maintenance"),

                TotalTickets = await _context.SupportTickets.CountAsync(),
                OpenTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Open"),
                InProgressTickets = await _context.SupportTickets.CountAsync(t => t.Status == "In Progress"),
                WaitingTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Waiting"),
                ResolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved"),
                ClosedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Closed"),
                HighPriorityTickets = await _context.SupportTickets.CountAsync(t =>
                    (t.Priority == "High" || t.Priority == "Critical") &&
                    t.Status != "Closed" && t.Status != "Resolved"),
                CriticalTickets = await _context.SupportTickets.CountAsync(t =>
                    t.Priority == "Critical" &&
                    t.Status != "Closed" && t.Status != "Resolved"),

                TotalMaintenance = await _context.MaintenanceRecords.CountAsync(),
                UpcomingMaintenance = await _context.MaintenanceRecords.CountAsync(m =>
                    m.ScheduledDate >= tomorrow && m.Status != "Completed"),
                DueTodayMaintenance = await _context.MaintenanceRecords.CountAsync(m =>
                    m.ScheduledDate >= today &&
                    m.ScheduledDate < tomorrow &&
                    m.Status != "Completed"),
                OverdueMaintenance = await _context.MaintenanceRecords.CountAsync(m =>
                    m.ScheduledDate < today &&
                    m.Status != "Completed"),
                CompletedMaintenance = await _context.MaintenanceRecords.CountAsync(m => m.Status == "Completed"),
                MonthlyMaintenanceCost = await _context.MaintenanceRecords
                    .Where(m => m.CreatedDate >= monthStart && m.CreatedDate < nextMonth)
                    .SumAsync(m => (decimal?)m.Cost) ?? 0m
            };

            var activeTickets = await _context.SupportTickets
                .Where(t => t.Status != "Closed" && t.Status != "Resolved")
                .Select(t => new
                {
                    t.ResponseDueAt,
                    t.FirstResponseAt,
                    t.ResolutionDueAt
                })
                .ToListAsync();

            vm.ActiveResponseSlaTickets = activeTickets.Count(t =>
                t.ResponseDueAt.HasValue && !t.FirstResponseAt.HasValue);

            vm.ResponseSlaBreachedTickets = activeTickets.Count(t =>
                t.ResponseDueAt.HasValue && !t.FirstResponseAt.HasValue &&
                t.ResponseDueAt.Value < now);

            vm.ResponseSlaAtRiskTickets = activeTickets.Count(t =>
                t.ResponseDueAt.HasValue && !t.FirstResponseAt.HasValue &&
                t.ResponseDueAt.Value >= now &&
                t.ResponseDueAt.Value <= now.AddHours(1));

            vm.ResponseSlaWithinTickets = activeTickets.Count(t =>
                t.ResponseDueAt.HasValue && !t.FirstResponseAt.HasValue &&
                t.ResponseDueAt.Value > now.AddHours(1));

            vm.ResponseSlaMetTickets = await _context.SupportTickets.CountAsync(t =>
                t.FirstResponseAt.HasValue && t.ResponseDueAt.HasValue &&
                t.FirstResponseAt.Value <= t.ResponseDueAt.Value);

            vm.ActiveResolutionSlaTickets = activeTickets.Count(t =>
                t.ResolutionDueAt.HasValue);

            vm.ResolutionSlaBreachedTickets = activeTickets.Count(t =>
                t.ResolutionDueAt.HasValue && t.ResolutionDueAt.Value < now);

            vm.ResolutionSlaAtRiskTickets = activeTickets.Count(t =>
                t.ResolutionDueAt.HasValue &&
                t.ResolutionDueAt.Value >= now &&
                t.ResolutionDueAt.Value <= now.AddHours(2));

            vm.ResolutionSlaWithinTickets = activeTickets.Count(t =>
                t.ResolutionDueAt.HasValue &&
                t.ResolutionDueAt.Value > now.AddHours(2));

            vm.ResolutionSlaMetTickets = await _context.SupportTickets.CountAsync(t =>
                t.ResolvedAt.HasValue && t.ResolutionDueAt.HasValue &&
                t.ResolvedAt.Value <= t.ResolutionDueAt.Value);

            vm.MonitoredSlaTickets = vm.ActiveResolutionSlaTickets;
            vm.SlaBreachedTickets = vm.ResolutionSlaBreachedTickets;
            vm.SlaAtRiskTickets = vm.ResolutionSlaAtRiskTickets;
            vm.SlaWithinTickets = vm.ResolutionSlaWithinTickets;
            vm.SlaHealthPercentage = vm.MonitoredSlaTickets > 0
                ? (int)Math.Round((vm.MonitoredSlaTickets - vm.SlaBreachedTickets) * 100d / vm.MonitoredSlaTickets)
                : 100;

            var resolvedOrClosed = vm.ResolvedTickets + vm.ClosedTickets;
            vm.ResolutionRatePercentage = vm.TotalTickets > 0
                ? Math.Round(resolvedOrClosed * 100d / vm.TotalTickets, 1)
                : 0;

            vm.TicketCategories = await _context.SupportTickets
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            vm.TicketPriorities = await _context.SupportTickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            vm.RecentTickets = await _context.SupportTickets
                .Include(t => t.Employee)
                .Include(t => t.Asset)
                .OrderByDescending(t => t.CreatedDate)
                .Take(6)
                .ToListAsync();

            vm.RecentAssets = await _context.Assets
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Id)
                .Take(6)
                .ToListAsync();

            vm.RecentActivity = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(7)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
