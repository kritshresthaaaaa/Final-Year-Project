using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }
        public double Price { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string StockStatus { get; set; }
    }
}
