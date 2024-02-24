using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fyp.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Direct fields for customer information
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

        // Field to track the employee who processed the bill
        [Required]
        public string EmployeeId { get; set; }

        public string TransactionId { get; set; }
        public bool PaymentConfirmed { get; set; }
        public string ApplicationUserId { get; set; }
        // Navigation property for Employee
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public virtual ApplicationUser ApplicationUser { get; set; } // Assuming employees are managed as users

        public virtual ICollection<BillItem> BillItems { get; set; }
    }
}
