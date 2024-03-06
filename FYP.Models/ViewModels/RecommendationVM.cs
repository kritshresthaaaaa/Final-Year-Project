using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class RecommendationVM
    {
        public ProductDetail Product { get; set; }
        public List<ProductDetail> Products { get; set; }
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
        public string SKU { get; set; }
    }
}
