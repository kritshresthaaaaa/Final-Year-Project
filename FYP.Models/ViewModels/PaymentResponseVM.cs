using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class PaymentResponseVM
    {
        public string Status { get; set; }
        public string Signature { get; set; }
        public string TransactionCode { get; set; }
        public double TotalAmount { get; set; }
        public string TransactionUuid { get; set; }
        public string ProductCode { get; set; }
        public string SuccessUrl { get; set; }
        public string SignedFieldNames { get; set; }
    }
}
