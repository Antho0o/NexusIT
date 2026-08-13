using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        public Asset? Asset { get; set; }

        [Required]
        [Display(Name = "Maintenance Type")]
        public string MaintenanceType { get; set; } = "Routine";

        [Required]
        public string Status { get; set; } = "Scheduled";

        [Required]
        [Display(Name = "Scheduled Date")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;

        [Display(Name = "Completed Date")]
        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Technician")]
        public string Technician { get; set; } = "";

        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        public string Notes { get; set; } = "";

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}