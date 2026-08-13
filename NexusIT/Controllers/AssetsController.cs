using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models;
using NexusIT.Services;
using NexusIT.ViewModels;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Staff)]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public AssetsController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<IActionResult> Index()
        {
            var assets = await _context.Assets
                .Include(a => a.Employee)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();

            return View(assets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _context.Assets
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null) return NotFound();

            await LoadEmployees();

            var history = await _context.AssetHistories
                .Include(h => h.PreviousEmployee)
                .Include(h => h.NewEmployee)
                .Where(h => h.AssetId == id)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            var vm = new AssetDetailsViewModel
            {
                Asset = asset,
                History = history,
                TicketCount = await _context.SupportTickets.CountAsync(t => t.AssetId == id),
                MaintenanceCount = await _context.MaintenanceRecords.CountAsync(m => m.AssetId == id),
                MaintenanceSpend = await _context.MaintenanceRecords
                    .Where(m => m.AssetId == id && m.Cost.HasValue)
                    .SumAsync(m => m.Cost ?? 0m)
            };

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            await LoadEmployees();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (!ModelState.IsValid)
            {
                await LoadEmployees();
                return View(asset);
            }

            if (asset.Status != "Assigned")
                asset.EmployeeId = null;

            if (asset.Status == "Assigned" && asset.EmployeeId == null)
                ModelState.AddModelError(nameof(asset.EmployeeId), "An assigned asset must have an employee.");

            if (!ModelState.IsValid)
            {
                await LoadEmployees();
                return View(asset);
            }

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            await AddHistory(asset, "Asset Created", null, asset.Status, null, asset.EmployeeId,
                "Asset added to the NexusIT inventory.");
            await _activityLog.LogAsync("Asset Created",
                $"Added asset {asset.AssetTag} — {asset.Brand} {asset.Model}.", "Asset", asset.Id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            await LoadEmployees();
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Asset input)
        {
            if (id != input.Id) return NotFound();

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadEmployees();
                return View(input);
            }

            if (input.Status == "Assigned" && input.EmployeeId == null)
                ModelState.AddModelError(nameof(input.EmployeeId), "An assigned asset must have an employee.");

            if (!ModelState.IsValid)
            {
                await LoadEmployees();
                return View(input);
            }

            var oldStatus = asset.Status;
            var oldEmployeeId = asset.EmployeeId;

            asset.AssetTag = input.AssetTag;
            asset.AssetType = input.AssetType;
            asset.Brand = input.Brand;
            asset.Model = input.Model;
            asset.SerialNumber = input.SerialNumber;
            asset.PurchaseDate = input.PurchaseDate;
            asset.PurchaseCost = input.PurchaseCost;
            asset.WarrantyExpiry = input.WarrantyExpiry;
            asset.Location = input.Location;
            asset.Status = input.Status;
            asset.EmployeeId = input.Status == "Assigned" ? input.EmployeeId : null;
            asset.Notes = input.Notes;

            await _context.SaveChangesAsync();

            if (oldStatus != asset.Status || oldEmployeeId != asset.EmployeeId)
            {
                var action = oldEmployeeId != asset.EmployeeId ? "Assignment Updated" : "Status Updated";
                await AddHistory(asset, action, oldStatus, asset.Status, oldEmployeeId, asset.EmployeeId,
                    "Asset assignment or lifecycle status updated.");
            }

            await _activityLog.LogAsync("Asset Updated",
                $"Updated asset {asset.AssetTag} — {asset.Brand} {asset.Model}.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id = asset.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int employeeId)
        {
            var asset = await _context.Assets.FindAsync(id);
            var employee = await _context.Employees.FindAsync(employeeId);
            if (asset == null || employee == null) return NotFound();
            if (asset.Status == "Retired") return BadRequest("Retired assets cannot be assigned.");

            var oldStatus = asset.Status;
            var oldEmployee = asset.EmployeeId;
            asset.Status = "Assigned";
            asset.EmployeeId = employeeId;

            await _context.SaveChangesAsync();
            await AddHistory(asset, "Asset Assigned", oldStatus, asset.Status, oldEmployee, employeeId,
                $"Assigned to {employee.FirstName} {employee.LastName}.");
            await _activityLog.LogAsync("Asset Assigned",
                $"Assigned {asset.AssetTag} to {employee.FirstName} {employee.LastName}.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            if (asset.Status == "Retired") return BadRequest("Retired assets cannot be returned.");

            var oldStatus = asset.Status;
            var oldEmployee = asset.EmployeeId;
            asset.Status = "Available";
            asset.EmployeeId = null;

            await _context.SaveChangesAsync();
            await AddHistory(asset, "Asset Returned", oldStatus, asset.Status, oldEmployee, null,
                "Asset returned to the available inventory.");
            await _activityLog.LogAsync("Asset Returned",
                $"Returned asset {asset.AssetTag} to available inventory.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToMaintenance(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            if (asset.Status == "Retired") return BadRequest("Retired assets cannot enter maintenance.");

            var oldStatus = asset.Status;
            var oldEmployee = asset.EmployeeId;
            asset.Status = "Maintenance";
            asset.EmployeeId = null;

            await _context.SaveChangesAsync();
            await AddHistory(asset, "Sent To Maintenance", oldStatus, asset.Status, oldEmployee, null,
                "Asset moved into the maintenance lifecycle state.");
            await _activityLog.LogAsync("Asset Sent To Maintenance",
                $"Moved asset {asset.AssetTag} into maintenance.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            if (asset.Status != "Maintenance" && asset.Status != "Retired")
                return BadRequest("Only maintenance or retired assets can be restored.");

            var oldStatus = asset.Status;
            asset.Status = "Available";
            asset.EmployeeId = null;

            await _context.SaveChangesAsync();
            await AddHistory(asset, "Asset Restored", oldStatus, asset.Status, null, null,
                "Asset returned to available inventory.");
            await _activityLog.LogAsync("Asset Restored",
                $"Restored asset {asset.AssetTag} to available inventory.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retire(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            if (asset.Status == "Retired") return RedirectToAction(nameof(Details), new { id });

            var oldStatus = asset.Status;
            var oldEmployee = asset.EmployeeId;
            asset.Status = "Retired";
            asset.EmployeeId = null;

            await _context.SaveChangesAsync();
            await AddHistory(asset, "Asset Retired", oldStatus, asset.Status, oldEmployee, null,
                "Asset removed from active inventory.");
            await _activityLog.LogAsync("Asset Retired",
                $"Retired asset {asset.AssetTag}.", "Asset", asset.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _context.Assets
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null) return NotFound();
            return View(asset);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                var assetTag = asset.AssetTag;
                var brand = asset.Brand;
                var model = asset.Model;
                var assetId = asset.Id;

                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();

                await _activityLog.LogAsync("Asset Deleted",
                    $"Deleted asset {assetTag} — {brand} {model}.", "Asset", assetId);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadEmployees()
        {
            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        private async Task AddHistory(
            Asset asset,
            string action,
            string? previousStatus,
            string? newStatus,
            int? previousEmployeeId,
            int? newEmployeeId,
            string notes)
        {
            var performedBy = User.Identity?.Name ?? "System";

            _context.AssetHistories.Add(new AssetHistory
            {
                AssetId = asset.Id,
                Action = action,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                PreviousEmployeeId = previousEmployeeId,
                NewEmployeeId = newEmployeeId,
                Notes = notes,
                PerformedBy = performedBy,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
