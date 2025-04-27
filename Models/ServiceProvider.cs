using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Models
{
    public class ServiceProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceProviderId { get; set; }

        [Required(ErrorMessage = "Name is required\n")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Username is required\n")]
        [StringLength(50, MinimumLength = 3)]
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
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Profile picture is required\n")]
        public string ProfileImage { get; set; }

        [Required(ErrorMessage = "Price is required\n")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number\n")]
        public float Price { get; set; }
        public float AverageRating { get; set; }

        //navigation properties 
        public virtual Service Service { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }

        public string UserId { get; set; }  // Public access

        public void CalculateAverageRating()
        {
            if (Ratings != null && Ratings.Any())
            {
                AverageRating = (float)Ratings.Average(r => (int)r.RatingValue);
            }
            else
            {
                AverageRating = 0; // Default when no ratings
            }
        }
    }
}
