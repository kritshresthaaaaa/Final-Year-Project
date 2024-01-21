using System.ComponentModel.DataAnnotations;

namespace Fyp.Models
{
    public class CategoryDetail
    {
        [Key]
        public int CategoryID { get; set; }
        [Required]
        [StringLength(255)]
        public string CategoryName { get; set; }


    }
}
