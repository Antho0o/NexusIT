using NexusIT.Models;

namespace NexusIT.ViewModels
{
    public class AssetDetailsViewModel
    {
        public Asset Asset { get; set; } = null!;
        public List<AssetHistory> History { get; set; } = new();
        public int TicketCount { get; set; }
        public int MaintenanceCount { get; set; }
        public decimal MaintenanceSpend { get; set; }
        public bool WarrantyActive => Asset.WarrantyExpiry.HasValue && Asset.WarrantyExpiry.Value.Date >= DateTime.Today;
        public bool WarrantyExpired => Asset.WarrantyExpiry.HasValue && Asset.WarrantyExpiry.Value.Date < DateTime.Today;
    }
}
