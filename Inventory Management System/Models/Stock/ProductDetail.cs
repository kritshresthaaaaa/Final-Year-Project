using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Models.Stock
{
    public class ProductDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        [Required]
        public double Price { get; set; }
        [Required]
     
        public int BrandID { get; set; }

        [Required]
        public int CategoryID { get; set; }


        public virtual BrandDetail Brand { get; set; }
        public virtual CategoryDetail Category { get; set; }


        public string RFIDTag { get; set; } // RFID Tag     

        public string Sizes { get; set; }
    }
}
