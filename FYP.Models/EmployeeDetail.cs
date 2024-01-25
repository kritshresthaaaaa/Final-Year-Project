using System.ComponentModel.DataAnnotations;

namespace Fyp.Models
{
    public class EmployeeDetail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The {0} must be exactly {1} digits.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "The {0} must contain only numbers.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }
        [Required]
        [Display(Name = "Registration Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }
   

        public string? ImageUrl { get; set; }
    }
}
