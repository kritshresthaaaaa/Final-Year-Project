using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class Summary
    {
        public List<CartConfimation> Products { get; set; }
        public double Subtotal { get; set; }
    }
}
