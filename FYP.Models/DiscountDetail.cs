using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fyp.Models
{
    public class DiscountDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, 100.00, ErrorMessage = "Discount percentage must be between 0.01 and 100.00")]
        [Display(Name = "Discount Percentage")]
        public decimal Percentage { get; set; }

 
 

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Discount End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Nullable foreign keys to apply the discount to specific targets
        [ForeignKey("Brand")]
        public int? BrandID { get; set; }
        [ForeignKey("Category")]
        public int? CategoryID { get; set; }

        
        // Navigation properties
        [ValidateNever]
        public virtual BrandDetail? Brand { get; set; }
        [ValidateNever]
        public virtual CategoryDetail? Category { get; set; }

        [ForeignKey("SKU")]
        public int? SKUID { get; set; }

        [ValidateNever]
        public virtual SKUDetail? SKU { get; set; }
     


    }
}
