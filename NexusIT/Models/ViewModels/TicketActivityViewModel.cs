namespace NexusIT.Models.ViewModels
{
    public class TicketActivityViewModel
    {
        public string Type { get; set; } = "";

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string Author { get; set; } = "";

        public DateTime Date { get; set; }

        public bool IsInternal { get; set; }

        public string Icon { get; set; } = "•";

        public string CssClass { get; set; } = "activity-default";
    }
}