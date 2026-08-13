using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexusIT.Models;

namespace NexusIT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =====================================================
        // DATABASE TABLES
        // =====================================================

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Asset> Assets { get; set; }

        public DbSet<AssetHistory> AssetHistories { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }

        public DbSet<TicketComment> TicketComments { get; set; }

        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }


        // =====================================================
        // DATABASE CONFIGURATION
        // =====================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // MAINTENANCE COST
            // =================================================

            modelBuilder.Entity<MaintenanceRecord>()
                .Property(m => m.Cost)
                .HasPrecision(18, 2);



            // =================================================
            // ASSET → HISTORY
            // =================================================

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.Asset)
                .WithMany(a => a.History)
                .HasForeignKey(h => h.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.PreviousEmployee)
                .WithMany()
                .HasForeignKey(h => h.PreviousEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.NewEmployee)
                .WithMany()
                .HasForeignKey(h => h.NewEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Asset>()
                .Property(a => a.PurchaseCost)
                .HasPrecision(18, 2);

            // =================================================
            // SUPPORT TICKET → EMPLOYEE
            //
            // When an employee is deleted:
            // - Keep the support ticket
            // - Set EmployeeId to NULL
            // =================================================

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);


            // =================================================
            // SUPPORT TICKET → ASSET
            //
            // Keep tickets if an asset is deleted.
            // AssetId becomes NULL.
            // =================================================

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Asset)
                .WithMany()
                .HasForeignKey(t => t.AssetId)
                .OnDelete(DeleteBehavior.SetNull);


            // =================================================
            // SUPPORT TICKET → COMMENTS
            //
            // Deleting a ticket deletes its comments.
            // =================================================

            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.SupportTicket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}