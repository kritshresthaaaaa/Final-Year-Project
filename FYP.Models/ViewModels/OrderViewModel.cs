﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class OrderViewModel
    {
        public OrderHeader OrderHeader { get; set; }
        public string ApplicationUserFullName { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }

}
