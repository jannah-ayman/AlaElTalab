using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Models
{
    public class Customer
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2-100 characters\n")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Username is required\n")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3-50 characters\n")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required\n")]
        [EmailAddress(ErrorMessage = "Invalid email format\n")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required\n")]
        [Phone(ErrorMessage = "Invalid phone number format\n")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "City is required\n")]
        [StringLength(50)]
        public string City { get; set; }
        public string? ProfileImage { get; set; } 


        //navigation properties 
        public virtual ICollection<Booking> Bookings { get; set; }
        public string UserId { get; set; }


    }
}
