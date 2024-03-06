using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fyp.Models.ViewModels
{
    public class SelectedProductsViewModel
    {
        public List<int> SelectedProductIds { get; set; }
        public string Sku { get; set; }
    }
}
