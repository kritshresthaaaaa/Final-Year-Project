using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Fyp.Models
{
    public class ProductDetail
    {
        public ProductDetail()
        {
            RecommendedProducts = new List<ProductRecommendation>();
        }
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [MinWords(20, ErrorMessage = "Description must contain at least 20 words.")]
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

        [Required]
        [DisplayName("Color")]
        [StringLength(7, ErrorMessage = "Color must be in #RRGGBB format", MinimumLength = 7)]
        [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Color must be in #RRGGBB format")]
        public string ColorCode { get; set; }
        [NotMapped]
        public List<ProductRecommendation> RecommendedProducts { get; set; } // Changed to List<T>


    }
}
