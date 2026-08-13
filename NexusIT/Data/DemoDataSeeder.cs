using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusIT.Models;

namespace NexusIT.Data
{
    /// <summary>
    /// Creates a safe, repeatable demo dataset for a fresh NexusIT installation.
    /// Existing non-empty application data is left untouched.
    /// </summary>
    public static class DemoDataSeeder
    {
        private const string DemoPassword = "NexusIT123!";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            await EnsureDemoUsersAsync(userManager);

            if (await context.Employees.AnyAsync() || await context.Assets.AnyAsync() || await context.SupportTickets.AnyAsync())
            {
                await EnsureSettingsAsync(context);
                return;
            }

            var now = DateTime.Now;

            var employees = new List<Employee>
            {
                new() { FirstName = "Joshua", LastName = "Barnes", Email = "employee@nexusit.local", Department = "Operations", JobTitle = "Operations User", PhoneNumber = "+27 41 555 0101", Location = "Port Elizabeth", DateJoined = now.AddMonths(-18), IsActive = true },
                new() { FirstName = "Sarah", LastName = "Mokoena", Email = "sarah.mokoena@nexusit.local", Department = "Finance", JobTitle = "Finance Analyst", PhoneNumber = "+27 41 555 0102", Location = "Cape Town", DateJoined = now.AddMonths(-30), IsActive = true },
                new() { FirstName = "Daniel", LastName = "Naidoo", Email = "daniel.naidoo@nexusit.local", Department = "Human Resources", JobTitle = "HR Specialist", PhoneNumber = "+27 41 555 0103", Location = "Johannesburg", DateJoined = now.AddMonths(-12), IsActive = true },
                new() { FirstName = "Aisha", LastName = "Pillay", Email = "aisha.pillay@nexusit.local", Department = "Sales", JobTitle = "Account Executive", PhoneNumber = "+27 41 555 0104", Location = "Durban", DateJoined = now.AddMonths(-8), IsActive = true },
                new() { FirstName = "Michael", LastName = "Smith", Email = "michael.smith@nexusit.local", Department = "Operations", JobTitle = "Operations Manager", PhoneNumber = "+27 41 555 0105", Location = "Port Elizabeth", DateJoined = now.AddMonths(-42), IsActive = true },
                new() { FirstName = "Thandi", LastName = "Dlamini", Email = "thandi.dlamini@nexusit.local", Department = "Marketing", JobTitle = "Marketing Coordinator", PhoneNumber = "+27 41 555 0106", Location = "Cape Town", DateJoined = now.AddMonths(-20), IsActive = true }
            };

            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();

            var assets = new List<Asset>
            {
                new() { AssetTag = "NXS-LT-001", AssetType = "Laptop", Brand = "Dell", Model = "Latitude 5540", SerialNumber = "DL5540NXS001", PurchaseDate = now.AddMonths(-10), PurchaseCost = 24500, WarrantyExpiry = now.AddMonths(26), Location = "Port Elizabeth", Status = "Assigned", EmployeeId = employees[0].Id, Notes = "Primary operations laptop." },
                new() { AssetTag = "NXS-LT-002", AssetType = "Laptop", Brand = "Lenovo", Model = "ThinkPad T14", SerialNumber = "LNT14NXS002", PurchaseDate = now.AddMonths(-16), PurchaseCost = 27900, WarrantyExpiry = now.AddMonths(20), Location = "Cape Town", Status = "Assigned", EmployeeId = employees[1].Id, Notes = "Finance workstation." },
                new() { AssetTag = "NXS-DT-003", AssetType = "Desktop", Brand = "HP", Model = "ProDesk 600 G6", SerialNumber = "HPP600NXS003", PurchaseDate = now.AddMonths(-28), PurchaseCost = 18800, WarrantyExpiry = now.AddMonths(8), Location = "Johannesburg", Status = "Assigned", EmployeeId = employees[2].Id, Notes = "HR desktop workstation." },
                new() { AssetTag = "NXS-MN-004", AssetType = "Monitor", Brand = "Dell", Model = "P2422H", SerialNumber = "DLP2422NXS004", PurchaseDate = now.AddMonths(-9), PurchaseCost = 4900, WarrantyExpiry = now.AddMonths(27), Location = "Durban", Status = "Available", Notes = "Spare monitor ready for deployment." },
                new() { AssetTag = "NXS-NW-005", AssetType = "Network", Brand = "Ubiquiti", Model = "UniFi AP AC Pro", SerialNumber = "UAPNXS005", PurchaseDate = now.AddMonths(-22), PurchaseCost = 3600, WarrantyExpiry = now.AddMonths(2), Location = "Port Elizabeth", Status = "Maintenance", Notes = "Intermittent wireless connectivity under investigation." },
                new() { AssetTag = "NXS-LT-006", AssetType = "Laptop", Brand = "HP", Model = "EliteBook 840 G8", SerialNumber = "HPE840NXS006", PurchaseDate = now.AddMonths(-36), PurchaseCost = 22100, WarrantyExpiry = now.AddMonths(-12), Location = "Cape Town", Status = "Retired", Notes = "End-of-life device." }
            };

            context.Assets.AddRange(assets);
            await context.SaveChangesAsync();

            context.AssetHistories.AddRange(
                new AssetHistory { AssetId = assets[0].Id, Action = "Asset Created", NewStatus = "Assigned", NewEmployeeId = employees[0].Id, Notes = "Initial demo inventory record.", PerformedBy = "System", CreatedAt = now.AddDays(-40) },
                new AssetHistory { AssetId = assets[0].Id, Action = "Asset Assigned", PreviousStatus = "Available", NewStatus = "Assigned", NewEmployeeId = employees[0].Id, Notes = "Assigned to operations user.", PerformedBy = "IT Support", CreatedAt = now.AddDays(-35) },
                new AssetHistory { AssetId = assets[4].Id, Action = "Sent To Maintenance", PreviousStatus = "Available", NewStatus = "Maintenance", Notes = "Wireless connectivity issue reported.", PerformedBy = "IT Technician", CreatedAt = now.AddDays(-3) },
                new AssetHistory { AssetId = assets[5].Id, Action = "Asset Retired", PreviousStatus = "Assigned", NewStatus = "Retired", PreviousEmployeeId = employees[3].Id, Notes = "Device reached end of lifecycle.", PerformedBy = "IT Manager", CreatedAt = now.AddDays(-20) }
            );
            await context.SaveChangesAsync();

            var tickets = new List<SupportTicket>
            {
                new() { Title = "Laptop cannot connect to corporate Wi-Fi", Description = "User reports intermittent Wi-Fi disconnects after returning from a meeting room.", Category = "Network", Priority = "High", Status = "In Progress", CreatedDate = now.AddHours(-5), UpdatedDate = now.AddHours(-1), ResponseDueAt = now.AddHours(-3), FirstResponseAt = now.AddHours(-4), ResolutionDueAt = now.AddHours(3), EmployeeId = employees[0].Id, AssetId = assets[0].Id, AssignedTo = "IT Technician" },
                new() { Title = "Finance printer is producing faded pages", Description = "The finance department printer requires inspection and a likely toner replacement.", Category = "Printer", Priority = "Medium", Status = "Open", CreatedDate = now.AddHours(-2), ResponseDueAt = now.AddHours(2), ResolutionDueAt = now.AddDays(1), EmployeeId = employees[1].Id, AssignedTo = "IT Technician" },
                new() { Title = "Request access to shared Finance folder", Description = "User needs access to the month-end reporting folder on the finance file share.", Category = "Access", Priority = "Low", Status = "Waiting", CreatedDate = now.AddDays(-2), UpdatedDate = now.AddDays(-1), ResponseDueAt = now.AddDays(-1), FirstResponseAt = now.AddDays(-2).AddHours(3), ResolutionDueAt = now.AddDays(2), EmployeeId = employees[1].Id, AssignedTo = "IT Manager" },
                new() { Title = "Microsoft 365 sign-in prompt repeats", Description = "Outlook and Teams repeatedly request authentication despite successful sign-in.", Category = "Software", Priority = "Critical", Status = "Resolved", CreatedDate = now.AddDays(-3), UpdatedDate = now.AddDays(-2), ResponseDueAt = now.AddDays(-3).AddHours(2), FirstResponseAt = now.AddDays(-3).AddHours(1), ResolutionDueAt = now.AddDays(-2), ResolvedAt = now.AddDays(-2).AddHours(-2), EmployeeId = employees[2].Id, AssetId = assets[2].Id, AssignedTo = "IT Technician" },
                new() { Title = "New starter laptop setup", Description = "Prepare a laptop, standard software, Microsoft 365 profile and security baseline for a new starter.", Category = "Hardware", Priority = "Medium", Status = "Closed", CreatedDate = now.AddDays(-9), UpdatedDate = now.AddDays(-7), ResponseDueAt = now.AddDays(-9).AddHours(4), FirstResponseAt = now.AddDays(-9).AddHours(1), ResolutionDueAt = now.AddDays(-7), ResolvedAt = now.AddDays(-7).AddHours(-1), ClosedAt = now.AddDays(-7), EmployeeId = employees[3].Id, AssetId = assets[3].Id, AssignedTo = "IT Technician" },
                new() { Title = "Suspicious email reported", Description = "User forwarded a suspicious message for review and requested confirmation that the message is malicious.", Category = "Security", Priority = "High", Status = "Open", CreatedDate = now.AddHours(-8), ResponseDueAt = now.AddHours(-4), ResolutionDueAt = now.AddHours(4), EmployeeId = employees[4].Id, AssignedTo = "IT Manager" }
            };

            context.SupportTickets.AddRange(tickets);
            await context.SaveChangesAsync();

            context.TicketComments.AddRange(
                new TicketComment { SupportTicketId = tickets[0].Id, Author = "IT Technician", Comment = "Initial diagnostics completed. Testing the access point configuration and driver state.", IsInternal = false, CreatedDate = now.AddHours(-2) },
                new TicketComment { SupportTicketId = tickets[0].Id, Author = "IT Technician", Comment = "Check AP-PE-01 logs before replacing the endpoint adapter.", IsInternal = true, CreatedDate = now.AddHours(-1) },
                new TicketComment { SupportTicketId = tickets[3].Id, Author = "IT Support", Comment = "Cleared cached credentials and repaired the Microsoft 365 profile. User confirmed the issue is resolved.", IsInternal = false, CreatedDate = now.AddDays(-2).AddHours(-1) },
                new TicketComment { SupportTicketId = tickets[4].Id, Author = "IT Support", Comment = "Laptop prepared, patched, encrypted and handed over to the employee.", IsInternal = false, CreatedDate = now.AddDays(-7) }
            );
            await context.SaveChangesAsync();

            context.MaintenanceRecords.AddRange(
                new MaintenanceRecord { AssetId = assets[4].Id, MaintenanceType = "Diagnostic", Status = "In Progress", ScheduledDate = now.Date.AddDays(-1), Technician = "IT Technician", Cost = 650, Notes = "Investigating intermittent wireless disconnects.", CreatedDate = now.AddDays(-2) },
                new MaintenanceRecord { AssetId = assets[1].Id, MaintenanceType = "Routine", Status = "Scheduled", ScheduledDate = now.Date.AddDays(2), Technician = "IT Technician", Cost = 450, Notes = "Routine inspection and endpoint health check.", CreatedDate = now.AddDays(-1) },
                new MaintenanceRecord { AssetId = assets[2].Id, MaintenanceType = "Software", Status = "Completed", ScheduledDate = now.Date.AddDays(-6), CompletedDate = now.Date.AddDays(-5), Technician = "IT Technician", Cost = 300, Notes = "Operating system updates and application patching.", CreatedDate = now.AddDays(-8) }
            );
            await context.SaveChangesAsync();

            context.ActivityLogs.AddRange(
                new ActivityLog { Action = "Ticket Created", Description = $"Created support ticket #{tickets[0].Id}: {tickets[0].Title}.", EntityType = "Support Ticket", EntityId = tickets[0].Id, PerformedBy = "employee@nexusit.local", CreatedDate = now.AddHours(-5), IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "Ticket Assigned", Description = $"Assigned ticket #{tickets[0].Id} to IT Technician.", EntityType = "Support Ticket", EntityId = tickets[0].Id, PerformedBy = "manager@nexusit.local", CreatedDate = now.AddHours(-4), IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "First Response", Description = $"First response recorded for ticket #{tickets[0].Id}.", EntityType = "Support Ticket", EntityId = tickets[0].Id, PerformedBy = "technician@nexusit.local", CreatedDate = now.AddHours(-4), IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "Asset Assigned", Description = $"Assigned {assets[0].AssetTag} to Joshua Barnes.", EntityType = "Asset", EntityId = assets[0].Id, PerformedBy = "manager@nexusit.local", CreatedDate = now.AddDays(-35), IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "Maintenance Started", Description = $"Started maintenance on {assets[4].AssetTag}.", EntityType = "Asset", EntityId = assets[4].Id, PerformedBy = "technician@nexusit.local", CreatedDate = now.AddDays(-1), IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "Ticket Resolved", Description = $"Resolved support ticket #{tickets[3].Id}.", EntityType = "Support Ticket", EntityId = tickets[3].Id, PerformedBy = "technician@nexusit.local", CreatedDate = tickets[3].ResolvedAt!.Value, IpAddress = "127.0.0.1" },
                new ActivityLog { Action = "Ticket Closed", Description = $"Closed support ticket #{tickets[4].Id}.", EntityType = "Support Ticket", EntityId = tickets[4].Id, PerformedBy = "manager@nexusit.local", CreatedDate = tickets[4].ClosedAt!.Value, IpAddress = "127.0.0.1" }
            );
            await context.SaveChangesAsync();

            await EnsureSettingsAsync(context);
        }

        private static async Task EnsureDemoUsersAsync(UserManager<IdentityUser> userManager)
        {
            var users = new (string Email, string Role)[]
            {
                ("admin@nexusit.local", AuthorizationRoles.Administrator),
                ("manager@nexusit.local", AuthorizationRoles.ITManager),
                ("technician@nexusit.local", AuthorizationRoles.ITTechnician),
                ("employee@nexusit.local", AuthorizationRoles.Employee)
            };

            foreach (var item in users)
            {
                var user = await userManager.FindByEmailAsync(item.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, DemoPassword);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to create demo account '{item.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, item.Role))
                {
                    var result = await userManager.AddToRoleAsync(user, item.Role);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to assign role '{item.Role}' to '{item.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task EnsureSettingsAsync(ApplicationDbContext context)
        {
            if (await context.SystemSettings.AnyAsync())
                return;

            context.SystemSettings.Add(new SystemSetting
            {
                SystemName = "NexusIT",
                OrganisationName = "NexusIT Demo Organisation",
                Currency = "ZAR",
                DateFormat = "dd MMM yyyy",
                DefaultTicketPriority = "Medium",
                DefaultTicketStatus = "Open",
                UpdatedDate = DateTime.Now
            });

            await context.SaveChangesAsync();
        }
    }
}
