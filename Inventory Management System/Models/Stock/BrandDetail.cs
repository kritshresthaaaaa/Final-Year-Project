using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.Stock
{
    public class BrandDetail
    {
        [Key]
        public int BrandID { get; set; }
        [Required]
        [StringLength(255)]
        public string BrandName { get; set; }
        [Display(Name = "Creation Date")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

    }
}
