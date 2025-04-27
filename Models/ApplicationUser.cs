using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Models
{
    //this class is for the attributes that arent in the identity user class 

    /*
    identity user used for registration, log in and log out that are made
    through identity scaffolding 
    */ 
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2-100 characters")]
        public string Name { get; set; }
        public string City { get; set; }
        public string? ProfileImage { get; set; }
        [Required]
        [Range(0, 100000, ErrorMessage = "Enter a valid price")]
        public float Price { get; set; }
        [Required(ErrorMessage = "Please select a service.")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

    }
}
