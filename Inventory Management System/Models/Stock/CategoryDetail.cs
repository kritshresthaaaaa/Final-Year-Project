﻿using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.Stock
{
    public class CategoryDetail
    {
        [Key]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
   

    }
}
