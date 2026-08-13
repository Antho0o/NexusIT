using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Management)]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActivityLogs
        public async Task<IActionResult> Index(
            string? search,
            string? actionType,
            string? entityType)
        {
            var query = _context.ActivityLogs
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Description.Contains(search) ||
                    a.Action.Contains(search) ||
                    a.PerformedBy.Contains(search) ||
                    a.EntityType.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                query = query.Where(a =>
                    a.Action == actionType);
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a =>
                    a.EntityType == entityType);
            }

            var logs = await query
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            // Filter values
            ViewBag.Search = search;
            ViewBag.ActionType = actionType;
            ViewBag.EntityType = entityType;

            // Available action filters
            ViewBag.ActionTypes = await _context.ActivityLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            // Available entity filters
            ViewBag.EntityTypes = await _context.ActivityLogs
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // Dashboard statistics
            ViewBag.TotalActivities =
                await _context.ActivityLogs.CountAsync();

            ViewBag.TodayActivities =
                await _context.ActivityLogs
                    .CountAsync(a =>
                        a.CreatedDate.Date == DateTime.Now.Date);

            ViewBag.TotalEntityTypes =
                await _context.ActivityLogs
                    .Select(a => a.EntityType)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .CountAsync();

            return View(logs);
        }

        // GET: ActivityLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.ActivityLogs
                .FirstOrDefaultAsync(a => a.Id == id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
    }
}