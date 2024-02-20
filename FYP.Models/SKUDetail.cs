using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class SKUDetail
    {
        [Key]
        public int SKUID { get; set; } 

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(30, ErrorMessage = "SKU cannot be longer than 30 characters.")]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "SKU must consist of uppercase letters, numbers, and dashes only.")]
        public string SKU { get; set; }

    }
}
