using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Action")]
        public string Action { get; set; } = "";

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = "";

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; } = "";

        [Display(Name = "Entity ID")]
        public int? EntityId { get; set; }

        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = "";

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }
    }
}