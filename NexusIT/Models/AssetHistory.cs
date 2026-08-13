using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public Asset? Asset { get; set; }

        [Required]
        [Display(Name = "Action")]
        public string Action { get; set; } = "Updated";

        [Display(Name = "Previous Status")]
        public string? PreviousStatus { get; set; }

        [Display(Name = "New Status")]
        public string? NewStatus { get; set; }

        public int? PreviousEmployeeId { get; set; }
        public Employee? PreviousEmployee { get; set; }

        public int? NewEmployeeId { get; set; }
        public Employee? NewEmployee { get; set; }

        public string Notes { get; set; } = "";

        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = "System";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
