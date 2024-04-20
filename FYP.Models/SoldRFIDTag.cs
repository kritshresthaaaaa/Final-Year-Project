using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class SoldRFIDTags
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]  // Assuming TagID is assigned manually and is unique
        public string TagID { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }  // The date and time the tag was sold
    }
}
