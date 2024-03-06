using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fyp.Models.ViewModels
{
    public class ProductDetailViewModel
    {
       public ProductDetail Product { get; set; }
        public List<ProductDetail> Products { get; set; }
        // Add SKU property to capture the SKU from the form
        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(30, ErrorMessage = "SKU cannot be longer than 30 characters.")]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "SKU must consist of uppercase letters, numbers, and dashes only.")]
        public string SKU { get; set; }
        public double DiscountPercentage { get; set; } // Nullable to indicate when no discount applies  
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public DiscountDetail? Discount { get; set; }


    }
}
    