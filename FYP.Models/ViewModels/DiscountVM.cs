using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class DiscountVM
    {
        public DiscountDetail Discount { get; set; }
        [Required]
        public string DiscountFor { get; set; } // "Categories", "Brands", "Products"
        public int? SelectedCategoryId { get; set; }
        public int? SelectedBrandId { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> BrandList { get; set; }
        // Add this for SKU
        [ValidateNever]
        public IEnumerable<SelectListItem> SKUList { get; set; }
        public int? SelectedSKUID { get; set; }

    }
}
