using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models;
using NexusIT.Services;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize(Roles = AuthorizationRoles.Management)]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public EmployeesController(
            ApplicationDbContext context,
            ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }


        // =====================================================
        // GET: Employees
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return View(employees);
        }


        // =====================================================
        // GET: Employees/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }


            // Employee assets

            var assets = await _context.Assets
                .Where(a => a.EmployeeId == id)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();


            // Employee support tickets

            var tickets = await _context.SupportTickets
                .Where(t => t.EmployeeId == id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();


            ViewBag.EmployeeAssets = assets;
            ViewBag.EmployeeTickets = tickets;


            return View(employee);
        }


        // =====================================================
        // GET: Employees/Create
        // =====================================================

        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // POST: Employees/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);

                await _context.SaveChangesAsync();


                await _activityLog.LogAsync(
                    "Employee Created",
                    $"Added employee {employee.FirstName} {employee.LastName}.",
                    "Employee",
                    employee.Id);


                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }


        // =====================================================
        // GET: Employees/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // =====================================================
        // POST: Employees/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // FIX:
                    // Previously this appeared twice.
                    _context.Update(employee);

                    await _context.SaveChangesAsync();


                    await _activityLog.LogAsync(
                        "Employee Updated",
                        $"Updated employee {employee.FirstName} {employee.LastName}.",
                        "Employee",
                        employee.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }


        // =====================================================
        // GET: Employees/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);


            if (employee == null)
            {
                return NotFound();
            }


            // Check related records so the Delete page
            // can warn the user.

            var ticketCount = await _context.SupportTickets
                .CountAsync(t => t.EmployeeId == id);

            var assetCount = await _context.Assets
                .CountAsync(a => a.EmployeeId == id);


            ViewBag.TicketCount = ticketCount;
            ViewBag.AssetCount = assetCount;


            return View(employee);
        }


        // =====================================================
        // POST: Employees/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .FindAsync(id);


            if (employee == null)
            {
                return RedirectToAction(nameof(Index));
            }


            // Save name before deleting the employee.

            var employeeName =
                $"{employee.FirstName} {employee.LastName}";


            // =================================================
            // SUPPORT TICKETS
            //
            // We explicitly remove the employee relationship.
            //
            // This means:
            //
            // Employee
            //      ↓
            // Support Ticket
            //
            // becomes:
            //
            // Employee deleted
            //      ↓
            // Support Ticket remains
            // EmployeeId = NULL
            // =================================================

            var tickets = await _context.SupportTickets
                .Where(t => t.EmployeeId == id)
                .ToListAsync();


            foreach (var ticket in tickets)
            {
                ticket.EmployeeId = null;
            }


            // =================================================
            // ASSETS
            //
            // Assets are also kept.
            // They simply become unassigned.
            // =================================================

            var assets = await _context.Assets
                .Where(a => a.EmployeeId == id)
                .ToListAsync();


            foreach (var asset in assets)
            {
                asset.EmployeeId = null;
            }


            // Delete employee

            _context.Employees.Remove(employee);


            await _context.SaveChangesAsync();


            // Activity log

            await _activityLog.LogAsync(
                "Employee Deleted",
                $"Deleted employee {employeeName}. Related tickets and assets were preserved.",
                "Employee",
                id);


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // EMPLOYEE EXISTS
        // =====================================================

        private bool EmployeeExists(int id)
        {
            return _context.Employees
                .Any(e => e.Id == id);
        }
    }
}