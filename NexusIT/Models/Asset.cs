using System.ComponentModel.DataAnnotations;

namespace NexusIT.Models
{
    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Asset Tag")]
        public string AssetTag { get; set; } = "";

        [Required]
        [Display(Name = "Asset Type")]
        public string AssetType { get; set; } = "";

        public string Brand { get; set; } = "";

        public string Model { get; set; } = "";

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; } = "";

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        [Display(Name = "Purchase Cost")]
        [DataType(DataType.Currency)]
        public decimal? PurchaseCost { get; set; }

        [Display(Name = "Warranty Expiry")]
        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiry { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; } = "";

        public string Notes { get; set; } = "";

        // Employee relationship

        [Display(Name = "Assigned Employee")]
        public int? EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public ICollection<AssetHistory> History { get; set; } = new List<AssetHistory>();
    }
}