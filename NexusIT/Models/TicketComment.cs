using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class TicketComment
    {
        public int Id { get; set; }


        // =====================================================
        // TICKET
        // =====================================================

        [Required]
        public int SupportTicketId { get; set; }

        public SupportTicket? SupportTicket { get; set; }


        // =====================================================
        // COMMENT AUTHOR
        // =====================================================

        [Required]
        [Display(Name = "Author")]
        public string Author { get; set; } = "IT Support";


        // =====================================================
        // COMMENT
        // =====================================================

        [Required]
        public string Comment { get; set; } = "";


        // =====================================================
        // COMMENT TYPE
        // =====================================================

        [Display(Name = "Internal Note")]
        public bool IsInternal { get; set; }


        // =====================================================
        // CREATED DATE
        // =====================================================

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}