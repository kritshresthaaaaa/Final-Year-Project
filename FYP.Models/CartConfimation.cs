using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class CartConfimation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public double ProductPrice { get; set; }
        public string ProductBrand { get; set; }
        public string ProductSize { get; set; }
        public string ProductColor { get; set; }

        public int ProductQuantity { get; set; }
        public string ImageUrl { get; set; }
   
        public string ProductRFID { get; set; }
        public double OrderTotal { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountedPrice { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public DiscountDetail? Discount { get; set; }
        public OrderHeader? OrderHeader { get; set; }
    }
}
