using NexusIT.Models;

namespace NexusIT.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }

        public int TotalAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int MaintenanceAssets { get; set; }

        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int WaitingTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int CriticalTickets { get; set; }

        public int MonitoredSlaTickets { get; set; }
        public int SlaBreachedTickets { get; set; }
        public int SlaAtRiskTickets { get; set; }
        public int SlaWithinTickets { get; set; }
        public int SlaHealthPercentage { get; set; }

        public int ActiveResponseSlaTickets { get; set; }
        public int ResponseSlaBreachedTickets { get; set; }
        public int ResponseSlaAtRiskTickets { get; set; }
        public int ResponseSlaWithinTickets { get; set; }
        public int ResponseSlaMetTickets { get; set; }

        public int ActiveResolutionSlaTickets { get; set; }
        public int ResolutionSlaBreachedTickets { get; set; }
        public int ResolutionSlaAtRiskTickets { get; set; }
        public int ResolutionSlaWithinTickets { get; set; }
        public int ResolutionSlaMetTickets { get; set; }

        public int TotalSlaBreaches => ResponseSlaBreachedTickets + ResolutionSlaBreachedTickets;
        public int TotalSlaAtRisk => ResponseSlaAtRiskTickets + ResolutionSlaAtRiskTickets;

        public int TotalMaintenance { get; set; }
        public int UpcomingMaintenance { get; set; }
        public int DueTodayMaintenance { get; set; }
        public int OverdueMaintenance { get; set; }
        public int CompletedMaintenance { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }

        public double ResolutionRatePercentage { get; set; }

        public List<SupportTicket> RecentTickets { get; set; } = new();
        public List<Asset> RecentAssets { get; set; } = new();
        public List<ActivityLog> RecentActivity { get; set; } = new();

        public Dictionary<string, int> TicketCategories { get; set; } = new();
        public Dictionary<string, int> TicketPriorities { get; set; } = new();

        public int OpenPercentage => Percentage(OpenTickets, TotalTickets);
        public int InProgressPercentage => Percentage(InProgressTickets, TotalTickets);
        public int WaitingPercentage => Percentage(WaitingTickets, TotalTickets);
        public int ResolvedPercentage => Percentage(ResolvedTickets, TotalTickets);
        public int ClosedPercentage => Percentage(ClosedTickets, TotalTickets);

        private static int Percentage(int value, int total)
            => total > 0 ? (int)Math.Round(value * 100d / total) : 0;
    }
}
