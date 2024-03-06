using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class ProductRecommendation
    {
        [Key]
        public int Id { get; set; }  
        public int ProductId { get; set; }
        // Navigation property to the ProductDetail of the product making the recommendation
        public ProductDetail Product { get; set; }

        // The ID of the product being recommended
        public int RecommendedProductId { get; set; }
        // Navigation property to the ProductDetail of the recommended product
        public ProductDetail RecommendedProduct { get; set; }
    }

}
