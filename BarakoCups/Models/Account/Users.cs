using BarakoCups.Models.UserRole;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarakoCups.Models.Account
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First Name is required")]

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "FirsName name must contain only letters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "LastName name must contain only letters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; } 
        }

        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?![a-zA-Z]+$)(?!\d+$).+", ErrorMessage = "Password must contain at least one letter and one number, and cannot consist solely of letters or numbers.")]
        public string Password { get; set; } = string.Empty;



        [ForeignKey("Role")]
        public int RoleId { get; set; }
        // Navigation property to relate users to roles
        public Role? Roles { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }


    }
}
