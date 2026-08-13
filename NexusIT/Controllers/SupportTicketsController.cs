using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusIT.Data;
using NexusIT.Models;
using NexusIT.Services;
using NexusIT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace NexusIT.Controllers
{
    [Authorize]
    public class SupportTicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public SupportTicketsController(
            ApplicationDbContext context,
            ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }


        // =====================================================
        // GET: SupportTickets
        // =====================================================

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? priority,
            string? category)
        {
            var query = _context.SupportTickets
                .Include(t => t.Employee)
                .Include(t => t.Asset)
                .AsQueryable();

            // Employees only see their own support requests.
            if (User.IsInRole(AuthorizationRoles.Employee))
            {
                var email = User.Identity?.Name;
                query = query.Where(t => t.Employee != null && t.Employee.Email == email);
            }


            // =================================================
            // SEARCH
            // =================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Description.Contains(search) ||
                    (t.Employee != null &&
                     (t.Employee.FirstName + " " +
                      t.Employee.LastName)
                        .Contains(search)));
            }


            // =================================================
            // STATUS FILTER
            // =================================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t =>
                    t.Status == status);
            }


            // =================================================
            // PRIORITY FILTER
            // =================================================

            if (!string.IsNullOrWhiteSpace(priority))
            {
                query = query.Where(t =>
                    t.Priority == priority);
            }


            // =================================================
            // CATEGORY FILTER
            // =================================================

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t =>
                    t.Category == category);
            }


            var tickets = await query
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Priority = priority;
            ViewBag.Category = category;


            return View(tickets);
        }


        // =====================================================
        // GET: SupportTickets/Details/5
        // =====================================================

        // =====================================================
        // GET: SupportTickets/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.SupportTickets
                .Include(t => t.Employee)
                .Include(t => t.Asset)
                .Include(t => t.Comments
                    .OrderByDescending(c => c.CreatedDate))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (User.IsInRole(AuthorizationRoles.Employee) &&
                (ticket.Employee == null || ticket.Employee.Email != User.Identity?.Name))
            {
                return Forbid();
            }


            // =================================================
            // SLA INFORMATION
            // =================================================

            ViewBag.ResponseSlaStatus =
                GetSlaStatus(
                    ticket.ResponseDueAt,
                    ticket.FirstResponseAt);

            ViewBag.ResolutionSlaStatus =
                GetSlaStatus(
                    ticket.ResolutionDueAt,
                    ticket.ResolvedAt);

            ViewBag.ResponseTimeRemaining =
                GetTimeRemaining(
                    ticket.ResponseDueAt,
                    ticket.FirstResponseAt);

            ViewBag.ResolutionTimeRemaining =
                GetTimeRemaining(
                    ticket.ResolutionDueAt,
                    ticket.ResolvedAt);


            // =================================================
            // BUILD ACTIVITY TIMELINE
            // =================================================

            // ActivityLog is the authoritative audit trail for ticket actions.
            // Human/system comments remain in the communication panel below.
            var ticketLogs = await _context.ActivityLogs
                .Where(a => a.EntityType == "Support Ticket" && a.EntityId == ticket.Id)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            var activities = new List<TicketActivityViewModel>
            {
                new TicketActivityViewModel
                {
                    Type = "Created",
                    Title = "Ticket Created",
                    Description = $"Ticket #{ticket.Id:D4} was created.",
                    Author = ticket.Employee != null
                        ? $"{ticket.Employee.FirstName} {ticket.Employee.LastName}"
                        : "NexusIT",
                    Date = ticket.CreatedDate,
                    Icon = "+",
                    CssClass = "activity-created"
                }
            };

            foreach (var log in ticketLogs)
            {
                var (type, title, icon, cssClass) = MapTicketActivity(log.Action);

                activities.Add(new TicketActivityViewModel
                {
                    Type = type,
                    Title = title,
                    Description = log.Description,
                    Author = string.IsNullOrWhiteSpace(log.PerformedBy)
                        ? "NexusIT"
                        : log.PerformedBy,
                    Date = log.CreatedDate,
                    Icon = icon,
                    CssClass = cssClass
                });
            }

            // Older tickets may pre-date the activity log implementation.
            // Add lifecycle milestones as fallbacks when no matching audit event exists.
            if (ticket.FirstResponseAt.HasValue &&
                !ticketLogs.Any(a => a.Action == "First Response"))
            {
                activities.Add(new TicketActivityViewModel
                {
                    Type = "Response",
                    Title = "First Response",
                    Description = "The first response was recorded for this ticket.",
                    Author = "IT Support",
                    Date = ticket.FirstResponseAt.Value,
                    Icon = "↗",
                    CssClass = "activity-response"
                });
            }

            if (ticket.ResolvedAt.HasValue &&
                !ticketLogs.Any(a => a.Action == "Ticket Resolved"))
            {
                activities.Add(new TicketActivityViewModel
                {
                    Type = "Resolved",
                    Title = "Ticket Resolved",
                    Description = "The ticket was marked as resolved.",
                    Author = "IT Support",
                    Date = ticket.ResolvedAt.Value,
                    Icon = "✓",
                    CssClass = "activity-resolved"
                });
            }

            if (ticket.ClosedAt.HasValue &&
                !ticketLogs.Any(a => a.Action == "Ticket Closed"))
            {
                activities.Add(new TicketActivityViewModel
                {
                    Type = "Closed",
                    Title = "Ticket Closed",
                    Description = "The ticket was closed.",
                    Author = "IT Support",
                    Date = ticket.ClosedAt.Value,
                    Icon = "×",
                    CssClass = "activity-closed"
                });
            }

            ViewBag.TicketActivities = activities
                .OrderByDescending(a => a.Date)
                .ToList();

            return View(ticket);
        }


        // =====================================================
        // GET: SupportTickets/Create
        // =====================================================

        public async Task<IActionResult> Create()
        {
            await LoadFormData();


            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();


            ViewBag.DefaultPriority =
                settings?.DefaultTicketPriority ?? "Medium";


            ViewBag.DefaultStatus =
                settings?.DefaultTicketStatus ?? "Open";


            return View();
        }


        // =====================================================
        // POST: SupportTickets/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SupportTicket ticket)
        {
            if (User.IsInRole(AuthorizationRoles.Employee))
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == User.Identity!.Name);

                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Your NexusIT account is not linked to an employee profile yet. Please contact IT support.");
                }
                else
                {
                    ticket.EmployeeId = employee.Id;
                    ticket.AssignedTo = null;

                    if (ticket.AssetId.HasValue)
                    {
                        var assetBelongsToEmployee = await _context.Assets
                            .AnyAsync(a => a.Id == ticket.AssetId.Value && a.EmployeeId == employee.Id);

                        if (!assetBelongsToEmployee)
                        {
                            ModelState.AddModelError(nameof(ticket.AssetId),
                                "You can only attach an asset assigned to you.");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var settings = await _context.SystemSettings
                    .FirstOrDefaultAsync();


                // =================================================
                // DEFAULT SETTINGS
                // =================================================

                ticket.Priority =
                    settings?.DefaultTicketPriority ?? "Medium";

                ticket.Status =
                    settings?.DefaultTicketStatus ?? "Open";


                ticket.CreatedDate = DateTime.Now;


                // =================================================
                // SLA
                // =================================================

                SetSlaDates(ticket);


                // =================================================
                // SAVE TICKET
                // =================================================

                _context.SupportTickets.Add(ticket);

                await _context.SaveChangesAsync();


                // =================================================
                // ACTIVITY LOG
                // =================================================

                await _activityLog.LogAsync(
                    "Ticket Created",
                    $"Created support ticket #{ticket.Id}: {ticket.Title}.",
                    "Support Ticket",
                    ticket.Id);


                // =================================================
                // INITIAL SYSTEM COMMENT
                // =================================================

                await AddSystemComment(
                    ticket.Id,
                    "Ticket created and SLA timer started.");


                return RedirectToAction(
                    nameof(Details),
                    new { id = ticket.Id });
            }


            await LoadFormData();

            return View(ticket);
        }


        // =====================================================
        // GET: SupportTickets/Edit/5
        // =====================================================

        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t =>
                    t.Id == id);


            if (ticket == null)
            {
                return NotFound();
            }


            await LoadFormData();


            return View(ticket);
        }


        // =====================================================
        // POST: SupportTickets/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SupportTicket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var existingTicket =
                        await _context.SupportTickets
                            .FirstOrDefaultAsync(t =>
                                t.Id == id);


                    if (existingTicket == null)
                    {
                        return NotFound();
                    }


                    var oldStatus =
                        existingTicket.Status;

                    var oldPriority =
                        existingTicket.Priority;

                    var oldAssignedTo =
                        existingTicket.AssignedTo;

                    if (oldStatus != ticket.Status &&
                        !CanTransition(oldStatus, ticket.Status))
                    {
                        ModelState.AddModelError(
                            nameof(ticket.Status),
                            $"Invalid workflow transition: {oldStatus} → {ticket.Status}.");

                        await LoadFormData();
                        return View(ticket);
                    }


                    // =================================================
                    // UPDATE VALUES
                    // =================================================

                    existingTicket.Title =
                        ticket.Title;

                    existingTicket.Description =
                        ticket.Description;

                    existingTicket.Category =
                        ticket.Category;

                    existingTicket.Priority =
                        ticket.Priority;

                    existingTicket.Status =
                        ticket.Status;

                    existingTicket.EmployeeId =
                        ticket.EmployeeId;

                    existingTicket.AssetId =
                        ticket.AssetId;

                    existingTicket.AssignedTo =
                        ticket.AssignedTo;

                    existingTicket.UpdatedDate =
                        DateTime.Now;


                    // =================================================
                    // PRIORITY CHANGED
                    // Recalculate SLA if the priority changed
                    // =================================================

                    if (oldPriority != ticket.Priority)
                    {
                        RecalculateSla(existingTicket);

                        await AddSystemComment(
                            existingTicket.Id,
                            $"Priority changed from {oldPriority} to {ticket.Priority}.");
                    }


                    // =================================================
                    // ASSIGNMENT CHANGED
                    // =================================================

                    if (oldAssignedTo != ticket.AssignedTo)
                    {
                        var assignment =
                            string.IsNullOrWhiteSpace(ticket.AssignedTo)
                                ? "Ticket unassigned."
                                : $"Ticket assigned to {ticket.AssignedTo}.";

                        await AddSystemComment(
                            existingTicket.Id,
                            assignment);
                    }


                    // =================================================
                    // STATUS CHANGED
                    // =================================================

                    if (oldStatus != ticket.Status)
                    {
                        await ProcessStatusChange(
                            existingTicket,
                            oldStatus,
                            ticket.Status);
                    }


                    await _context.SaveChangesAsync();


                    await _activityLog.LogAsync(
                        "Ticket Updated",
                        $"Updated support ticket #{existingTicket.Id}: {existingTicket.Title}.",
                        "Support Ticket",
                        existingTicket.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }


                return RedirectToAction(
                    nameof(Details),
                    new { id = ticket.Id });
            }


            await LoadFormData();

            return View(ticket);
        }


        // =====================================================
        // POST: Add Comment
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            int id,
            string comment,
            bool isInternal = false)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            var ticket =
                await _context.SupportTickets
                    .Include(t => t.Employee)
                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (ticket == null)
            {
                return NotFound();
            }

            if (User.IsInRole(AuthorizationRoles.Employee) &&
                (ticket.Employee == null || ticket.Employee.Email != User.Identity?.Name))
            {
                return Forbid();
            }

            if (User.IsInRole(AuthorizationRoles.Employee) && isInternal)
            {
                return Forbid();
            }


            var newComment = new TicketComment
            {
                SupportTicketId = ticket.Id,
                Author = User.Identity?.Name
                    ?? "IT Support",
                Comment = comment.Trim(),
                IsInternal = isInternal,
                CreatedDate = DateTime.Now
            };


            _context.TicketComments.Add(newComment);


            // =================================================
            // FIRST RESPONSE
            // =================================================

            if (!ticket.FirstResponseAt.HasValue)
            {
                ticket.FirstResponseAt =
                    DateTime.Now;

                ticket.UpdatedDate =
                    DateTime.Now;


                if (ticket.Status == "Open")
                {
                    ticket.Status =
                        "In Progress";
                }


                await _activityLog.LogAsync(
                    "First Response",
                    $"First response added to ticket #{ticket.Id}: {ticket.Title}.",
                    "Support Ticket",
                    ticket.Id);
            }


            ticket.UpdatedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            await _activityLog.LogAsync(
                "Ticket Comment",
                $"Added a comment to ticket #{ticket.Id}: {ticket.Title}.",
                "Support Ticket",
                ticket.Id);


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =====================================================
        // POST: Resolve/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> Resolve(
            int? id,
            string? resolutionNote)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status == "Resolved")
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ticket.Status != "In Progress" && ticket.Status != "Waiting")
            {
                TempData["TicketError"] =
                    "A ticket must be In Progress or Waiting before it can be resolved.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var now = DateTime.Now;
            ticket.Status = "Resolved";
            ticket.ResolvedAt = now;
            ticket.FirstResponseAt ??= now;
            ticket.UpdatedDate = now;

            var note = string.IsNullOrWhiteSpace(resolutionNote)
                ? "Ticket marked as resolved."
                : $"Resolution: {resolutionNote.Trim()}";

            await AddSystemComment(ticket.Id, note);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Ticket Resolved",
                $"Resolved support ticket #{ticket.Id}: {ticket.Title}. Resolution: {resolutionNote?.Trim() ?? "Ticket marked as resolved."}",
                "Support Ticket",
                ticket.Id);

            return RedirectToAction(nameof(Details), new { id });
        }


        // =====================================================
        // POST: ChangeStatus/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> ChangeStatus(
            int? id,
            string newStatus)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            newStatus = (newStatus ?? string.Empty).Trim();

            if (!IsValidStatus(newStatus))
            {
                TempData["TicketError"] = "That ticket status is not valid.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ticket.Status == newStatus)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!CanTransition(ticket.Status, newStatus))
            {
                TempData["TicketError"] =
                    $"A ticket cannot move directly from {ticket.Status} to {newStatus}.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var oldStatus = ticket.Status;
            await ProcessStatusChange(ticket, oldStatus, newStatus);

            if (newStatus == "In Progress" && !ticket.FirstResponseAt.HasValue)
            {
                ticket.FirstResponseAt = DateTime.Now;
            }

            if (newStatus == "Waiting")
            {
                await AddSystemComment(ticket.Id, "Ticket moved to Waiting for customer or external input.");
            }

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Ticket Status Changed",
                $"Changed ticket #{ticket.Id} from {oldStatus} to {newStatus}.",
                "Support Ticket",
                ticket.Id);

            return RedirectToAction(nameof(Details), new { id });
        }


        // =====================================================
        // POST: Close/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status == "Closed")
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ticket.Status != "Resolved")
            {
                TempData["TicketError"] =
                    "A ticket must be Resolved before it can be closed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            await ProcessStatusChange(ticket, ticket.Status, "Closed");
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Ticket Closed",
                $"Closed support ticket #{ticket.Id}: {ticket.Title}.",
                "Support Ticket",
                ticket.Id);

            return RedirectToAction(nameof(Details), new { id });
        }


        // =====================================================
        // POST: Reopen/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> Reopen(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != "Closed" && ticket.Status != "Resolved")
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var oldStatus = ticket.Status;
            ticket.Status = "Open";
            ticket.UpdatedDate = DateTime.Now;
            ticket.ClosedAt = null;
            ticket.ResolvedAt = null;
            ticket.FirstResponseAt = null;

            RecalculateSla(ticket);

            await AddSystemComment(ticket.Id, $"Ticket reopened from {oldStatus} and returned to Open.");
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Ticket Reopened",
                $"Reopened support ticket #{ticket.Id}: {ticket.Title}.",
                "Support Ticket",
                ticket.Id);

            return RedirectToAction(nameof(Details), new { id });
        }


        // =====================================================
        // GET: Delete/5
        // =====================================================

        [Authorize(Roles = AuthorizationRoles.Staff)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var ticket =
                await _context.SupportTickets
                    .Include(t => t.Employee)
                    .Include(t => t.Asset)
                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (ticket == null)
            {
                return NotFound();
            }


            return View(ticket);
        }


        // =====================================================
        // POST: Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var ticket =
                await _context.SupportTickets
                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (ticket != null)
            {
                var ticketId =
                    ticket.Id;

                var ticketTitle =
                    ticket.Title;


                _context.SupportTickets.Remove(ticket);


                await _context.SaveChangesAsync();


                await _activityLog.LogAsync(
                    "Ticket Deleted",
                    $"Deleted support ticket #{ticketId}: {ticketTitle}.",
                    "Support Ticket",
                    ticketId);
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // WORKFLOW RULES
        // =====================================================

        private static bool IsValidStatus(string status)
        {
            return status is
                "Open" or
                "In Progress" or
                "Waiting" or
                "Resolved" or
                "Closed";
        }

        private static bool CanTransition(string currentStatus, string newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                ("Open", "In Progress") => true,
                ("In Progress", "Waiting") => true,
                ("In Progress", "Resolved") => true,
                ("Waiting", "In Progress") => true,
                ("Waiting", "Resolved") => true,
                ("Resolved", "Closed") => true,
                _ => false
            };
        }


        private static (string Type, string Title, string Icon, string CssClass) MapTicketActivity(string action)
        {
            return action switch
            {
                "Ticket Status Changed" => ("Status", "Status Updated", "↻", "activity-status"),
                "Ticket Assigned" => ("Assignment", "Ticket Assigned", "↗", "activity-assignment"),
                "First Response" => ("Response", "First Response", "↗", "activity-response"),
                "Ticket Resolved" => ("Resolved", "Ticket Resolved", "✓", "activity-resolved"),
                "Ticket Closed" => ("Closed", "Ticket Closed", "×", "activity-closed"),
                "Ticket Reopened" => ("Reopened", "Ticket Reopened", "↺", "activity-reopened"),
                "Ticket Created" => ("Created", "Ticket Created", "+", "activity-created"),
                _ => ("Activity", action, "•", "activity-default")
            };
        }


        // =====================================================
        // SLA CALCULATION
        // =====================================================

        private void SetSlaDates(
            SupportTicket ticket)
        {
            var created =
                ticket.CreatedDate;


            switch (ticket.Priority)
            {
                case "Critical":

                    ticket.ResponseDueAt =
                        created.AddMinutes(15);

                    ticket.ResolutionDueAt =
                        created.AddHours(4);

                    break;


                case "High":

                    ticket.ResponseDueAt =
                        created.AddMinutes(30);

                    ticket.ResolutionDueAt =
                        created.AddHours(8);

                    break;


                case "Medium":

                    ticket.ResponseDueAt =
                        created.AddHours(2);

                    ticket.ResolutionDueAt =
                        created.AddHours(24);

                    break;


                case "Low":

                    ticket.ResponseDueAt =
                        created.AddHours(8);

                    ticket.ResolutionDueAt =
                        created.AddHours(72);

                    break;


                default:

                    ticket.ResponseDueAt =
                        created.AddHours(2);

                    ticket.ResolutionDueAt =
                        created.AddHours(24);

                    break;
            }
        }


        // =====================================================
        // RECALCULATE SLA
        // =====================================================

        private void RecalculateSla(
            SupportTicket ticket)
        {
            var now =
                DateTime.Now;


            ticket.ResponseDueAt =
                null;

            ticket.ResolutionDueAt =
                null;


            switch (ticket.Priority)
            {
                case "Critical":

                    ticket.ResponseDueAt =
                        now.AddMinutes(15);

                    ticket.ResolutionDueAt =
                        now.AddHours(4);

                    break;


                case "High":

                    ticket.ResponseDueAt =
                        now.AddMinutes(30);

                    ticket.ResolutionDueAt =
                        now.AddHours(8);

                    break;


                case "Medium":

                    ticket.ResponseDueAt =
                        now.AddHours(2);

                    ticket.ResolutionDueAt =
                        now.AddHours(24);

                    break;


                case "Low":

                    ticket.ResponseDueAt =
                        now.AddHours(8);

                    ticket.ResolutionDueAt =
                        now.AddHours(72);

                    break;
            }
        }


        // =====================================================
        // PROCESS STATUS CHANGE
        // =====================================================

        private async Task ProcessStatusChange(
            SupportTicket ticket,
            string oldStatus,
            string newStatus)
        {
            ticket.UpdatedDate =
                DateTime.Now;


            if (newStatus == "In Progress" && !ticket.FirstResponseAt.HasValue)
            {
                ticket.FirstResponseAt = DateTime.Now;
            }


            if (newStatus == "Resolved")
            {
                ticket.ResolvedAt =
                    DateTime.Now;
            }


            if (newStatus != "Resolved" && newStatus != "Closed")
            {
                ticket.ResolvedAt = null;
            }


            if (newStatus == "Closed")
            {
                ticket.ClosedAt =
                    DateTime.Now;

                if (!ticket.ResolvedAt.HasValue)
                {
                    ticket.ResolvedAt =
                        DateTime.Now;
                }
            }


            if (newStatus != "Closed")
            {
                ticket.ClosedAt =
                    null;
            }


            await AddSystemComment(
                ticket.Id,
                $"Status changed from {oldStatus} to {newStatus}.");
        }


        // =====================================================
        // ADD SYSTEM COMMENT
        // =====================================================

        private async Task AddSystemComment(
            int ticketId,
            string message)
        {
            var comment = new TicketComment
            {
                SupportTicketId = ticketId,
                Author = "NexusIT System",
                Comment = message,
                IsInternal = true,
                CreatedDate = DateTime.Now
            };


            _context.TicketComments.Add(comment);


            await Task.CompletedTask;
        }


        // =====================================================
        // SLA STATUS
        // =====================================================

        private string GetSlaStatus(
            DateTime? dueDate,
            DateTime? completedDate)
        {
            if (!dueDate.HasValue)
            {
                return "Not Set";
            }


            if (completedDate.HasValue)
            {
                if (completedDate.Value <= dueDate.Value)
                {
                    return "Met";
                }

                return "Breached";
            }


            if (DateTime.Now > dueDate.Value)
            {
                return "Breached";
            }


            var remaining =
                dueDate.Value - DateTime.Now;


            if (remaining.TotalMinutes <= 60)
            {
                return "At Risk";
            }


            return "Within SLA";
        }


        // =====================================================
        // SLA TIME REMAINING
        // =====================================================

        private string GetTimeRemaining(
            DateTime? dueDate,
            DateTime? completedDate)
        {
            if (!dueDate.HasValue)
            {
                return "Not set";
            }


            if (completedDate.HasValue)
            {
                if (completedDate.Value <= dueDate.Value)
                {
                    var elapsed =
                        dueDate.Value -
                        completedDate.Value;


                    return
                        $"Met · {FormatDuration(elapsed)} early";
                }


                var breached =
                    completedDate.Value -
                    dueDate.Value;


                return
                    $"Breached · {FormatDuration(breached)}";
            }


            var remaining =
                dueDate.Value -
                DateTime.Now;


            if (remaining.TotalSeconds <= 0)
            {
                return
                    $"Breached · {FormatDuration(remaining.Duration())}";
            }


            return
                $"{FormatDuration(remaining)} remaining";
        }


        // =====================================================
        // FORMAT DURATION
        // =====================================================

        private string FormatDuration(
            TimeSpan duration)
        {
            duration =
                duration.Duration();


            if (duration.TotalDays >= 1)
            {
                return
                    $"{(int)duration.TotalDays}d " +
                    $"{duration.Hours}h";
            }


            if (duration.TotalHours >= 1)
            {
                return
                    $"{(int)duration.TotalHours}h " +
                    $"{duration.Minutes}m";
            }


            return
                $"{duration.Minutes}m";
        }


        // =====================================================
        // LOAD FORM DATA
        // =====================================================

        private async Task LoadFormData()
        {
            if (User.IsInRole(AuthorizationRoles.Employee))
            {
                var email = User.Identity?.Name;
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == email);

                ViewBag.Employees = employee == null
                    ? new List<NexusIT.Models.Employee>()
                    : new List<NexusIT.Models.Employee> { employee };

                ViewBag.Assets = employee == null
                    ? new List<NexusIT.Models.Asset>()
                    : await _context.Assets
                        .Where(a => a.EmployeeId == employee.Id)
                        .OrderBy(a => a.AssetTag)
                        .ToListAsync();

                return;
            }

            ViewBag.Employees =
                await _context.Employees
                    .OrderBy(e => e.FirstName)
                    .ToListAsync();

            ViewBag.Assets =
                await _context.Assets
                    .OrderBy(a => a.AssetTag)
                    .ToListAsync();
        }


        // =====================================================
        // CHECK IF TICKET EXISTS
        // =====================================================

        private bool TicketExists(int id)
        {
            return _context.SupportTickets
                .Any(t => t.Id == id);
        }
    }
}