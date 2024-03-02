using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class OrderHeader
    {
        public Guid Id { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string CustomerEmail { get; set; }
        [Required]
        public string CustomerPhone { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }


    }
}
