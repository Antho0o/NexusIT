using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        // =====================================================
        // BASIC TICKET INFORMATION
        // =====================================================

        [Required]
        [Display(Name = "Ticket Title")]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Priority { get; set; } = "Medium";

        [Required]
        public string Status { get; set; } = "Open";

        [Required]
        public string Category { get; set; } = "General";


        // =====================================================
        // DATES
        // =====================================================

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }


        // =====================================================
        // SLA
        // =====================================================

        [Display(Name = "Response Due")]
        public DateTime? ResponseDueAt { get; set; }

        [Display(Name = "Resolution Due")]
        public DateTime? ResolutionDueAt { get; set; }

        [Display(Name = "First Response")]
        public DateTime? FirstResponseAt { get; set; }

        [Display(Name = "Resolved Date")]
        public DateTime? ResolvedAt { get; set; }

        [Display(Name = "Closed Date")]
        public DateTime? ClosedAt { get; set; }


        // =====================================================
        // EMPLOYEE WHO REPORTED THE ISSUE
        // =====================================================

        [Display(Name = "Employee")]
        public int? EmployeeId { get; set; }

        public Employee? Employee { get; set; }


        // =====================================================
        // ASSET INVOLVED IN THE ISSUE
        // =====================================================

        [Display(Name = "Asset")]
        public int? AssetId { get; set; }

        public Asset? Asset { get; set; }


        // =====================================================
        // IT TECHNICIAN HANDLING THE TICKET
        // =====================================================

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }


        // =====================================================
        // TICKET COMMENTS
        // =====================================================

        public ICollection<TicketComment> Comments { get; set; }
            = new List<TicketComment>();
    }
}