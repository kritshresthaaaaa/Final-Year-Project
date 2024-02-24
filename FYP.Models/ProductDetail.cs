using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fyp.Models
{
    public class ProductDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        [Required]
        public double Price { get; set; }
  
        public double DiscountedPrice { get; set; }

        [Required]

        public int BrandID { get; set; }

        [Required]
        public int CategoryID { get; set; }
        [Required]
        public int SKUID { get; set; }

        [ValidateNever]
        public virtual SKUDetail SKU { get; set; }
        [ValidateNever]
        public virtual BrandDetail Brand { get; set; }
        [ValidateNever]
        public virtual CategoryDetail Category { get; set; }


        public string RFIDTag { get; set; } // RFID Tag     

        public string Sizes { get; set; }
      

        public string? ImageUrl { get; set; }
     


    }
}
