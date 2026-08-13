using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "System Name")]
        public string SystemName { get; set; } = "NexusIT";

        [Display(Name = "Organisation Name")]
        public string OrganisationName { get; set; } = "";

        [Display(Name = "Default Currency")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "dd MMM yyyy";

        [Display(Name = "Default Ticket Priority")]
        public string DefaultTicketPriority { get; set; } = "Medium";

        [Display(Name = "Default Ticket Status")]
        public string DefaultTicketStatus { get; set; } = "Open";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}