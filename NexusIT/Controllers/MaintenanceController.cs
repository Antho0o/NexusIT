using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models;
using NexusIT.Services;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Staff)]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public MaintenanceController(
            ApplicationDbContext context,
            ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // =====================================================
        // GET: Maintenance
        // =====================================================

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? type)
        {
            var query = _context.MaintenanceRecords
                .Include(m => m.Asset)
                .AsQueryable();

            // Search

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                    m.MaintenanceType.Contains(search) ||
                    m.Technician.Contains(search) ||
                    m.Notes.Contains(search) ||
                    (m.Asset != null &&
                     (m.Asset.AssetTag.Contains(search) ||
                      m.Asset.Brand.Contains(search) ||
                      m.Asset.Model.Contains(search))));
            }

            // Status filter

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(m =>
                    m.Status == status);
            }

            // Maintenance type filter

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(m =>
                    m.MaintenanceType == type);
            }

            var records = await query
                .OrderByDescending(m => m.ScheduledDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Type = type;

            // Dashboard statistics

            ViewBag.TotalMaintenance =
                await _context.MaintenanceRecords.CountAsync();

            ViewBag.ScheduledMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "Scheduled");

            ViewBag.InProgressMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "In Progress");

            ViewBag.CompletedMaintenance =
                await _context.MaintenanceRecords
                    .CountAsync(m => m.Status == "Completed");

            return View(records);
        }

        // =====================================================
        // GET: Maintenance/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.MaintenanceRecords
                .Include(m => m.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        // =====================================================
        // GET: Maintenance/Create
        // =====================================================

        public async Task<IActionResult> Create()
        {
            await LoadAssets();

            return View();
        }

        // =====================================================
        // POST: Maintenance/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MaintenanceRecord record)
        {
            if (ModelState.IsValid)
            {
                record.CreatedDate = DateTime.Now;

                _context.MaintenanceRecords.Add(record);

                await _context.SaveChangesAsync();

                await _activityLog.LogAsync(
                    "Maintenance Scheduled",
                    $"Scheduled {record.MaintenanceType} maintenance.",
                    "Maintenance",
                    record.Id);

                return RedirectToAction(nameof(Index));
            }

            await LoadAssets();

            return View(record);
        }

        // =====================================================
        // GET: Maintenance/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record =
                await _context.MaintenanceRecords.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            await LoadAssets();

            return View(record);
        }

        // =====================================================
        // POST: Maintenance/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MaintenanceRecord record)
        {
            if (id != record.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(record);

                    await _context.SaveChangesAsync();

                    await _activityLog.LogAsync(
                        "Maintenance Updated",
                        $"Updated maintenance record #{record.Id}.",
                        "Maintenance",
                        record.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceExists(record.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadAssets();

            return View(record);
        }

        // =====================================================
        // GET: Maintenance/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.MaintenanceRecords
                .Include(m => m.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        // =====================================================
        // POST: Maintenance/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var record =
                await _context.MaintenanceRecords.FindAsync(id);

            if (record != null)
            {
                var recordId = record.Id;
                var maintenanceType = record.MaintenanceType;

                _context.MaintenanceRecords.Remove(record);

                await _context.SaveChangesAsync();

                await _activityLog.LogAsync(
                    "Maintenance Deleted",
                    $"Deleted maintenance record #{recordId} ({maintenanceType}).",
                    "Maintenance",
                    recordId);
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // POST: Maintenance/Complete/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record =
                await _context.MaintenanceRecords.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            // Don't complete an already completed record.
            if (record.Status == "Completed")
            {
                return RedirectToAction(nameof(Index));
            }

            record.Status = "Completed";
            record.CompletedDate = DateTime.Today;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Maintenance Completed",
                $"Completed maintenance record #{record.Id}.",
                "Maintenance",
                record.Id);

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // POST: Maintenance/Start/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record =
                await _context.MaintenanceRecords.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            // Only scheduled maintenance can be started.
            if (record.Status != "Scheduled")
            {
                return RedirectToAction(nameof(Index));
            }

            record.Status = "In Progress";

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Maintenance Started",
                $"Started maintenance record #{record.Id}.",
                "Maintenance",
                record.Id);

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LOAD ASSETS
        // =====================================================

        private async Task LoadAssets()
        {
            ViewBag.Assets = await _context.Assets
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        // =====================================================
        // CHECK IF RECORD EXISTS
        // =====================================================

        private bool MaintenanceExists(int id)
        {
            return _context.MaintenanceRecords
                .Any(m => m.Id == id);
        }
    }
}