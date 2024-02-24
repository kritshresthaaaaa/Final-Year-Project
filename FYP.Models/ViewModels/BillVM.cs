using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fyp.Models.ViewModels
{
    public class BillViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime BillingDate { get; set; } = DateTime.Now;
        public double TotalAmount { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; } // Assuming you want to display the employee's name who is processing the bill

        // Add additional fields as necessary
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        // Add additional fields as necessary
    }
}
