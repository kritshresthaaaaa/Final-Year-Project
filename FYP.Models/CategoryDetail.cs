using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Fyp.Models
{
    public class CategoryDetail
    {
        [Key]
        public int CategoryID { get; set; }
        [Required]
        [StringLength(255)]
        public string CategoryName { get; set; }
        [ValidateNever]
        public ICollection<DiscountDetail>? Discounts { get; set; }


    }
}
