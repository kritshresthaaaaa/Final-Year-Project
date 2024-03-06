using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class MapVM
    {
        public IEnumerable<RecommendationVM> SkuProducts { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<ProductDetailViewModel> AllProducts { get; set; } // Assuming ProductDetailVM is a ViewModel for your products
    }


}