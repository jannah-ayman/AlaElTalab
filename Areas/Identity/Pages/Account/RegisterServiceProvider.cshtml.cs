using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

public class RegisterServiceProviderModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RegisterServiceProviderModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public List<SelectListItem> ServiceList { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
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

        [Required]
        [Range(0, 100000, ErrorMessage = "Enter a valid price")]
        public float Price { get; set; }

        [Required(ErrorMessage = "Profile Image is required\n")]
        [Display(Name = "Profile Image")]
        public IFormFile ProfileImageFile { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a service.")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }
    }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ServiceList = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Housekeeping" },
            new SelectListItem { Value = "2", Text = "Electrical Services" },
            new SelectListItem { Value = "3", Text = "Plumbing" },
            new SelectListItem { Value = "4", Text = "Carpentry" },
        };
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        ServiceList = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Housekeeping" },
            new SelectListItem { Value = "2", Text = "Electrical Services" },
            new SelectListItem { Value = "3", Text = "Plumbing" },
            new SelectListItem { Value = "4", Text = "Carpentry" },
        };

        if (!ModelState.IsValid) return Page();

        string imageFileName = null;
        if (Input.ProfileImageFile != null)
        {
            var ext = Path.GetExtension(Input.ProfileImageFile.FileName).ToLower();
            var allowed = new[] { ".jpg", ".png", ".jpeg" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("ProfileImageFile", "Only JPG/PNG are allowed.");
                return Page();
            }

            imageFileName = Guid.NewGuid() + ext;
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);
            var filePath = Path.Combine(uploadsPath, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await Input.ProfileImageFile.CopyToAsync(stream);
        }

        var user = new ApplicationUser
        {
            UserName = Input.Username,
            Email = Input.Email,
            PhoneNumber = Input.PhoneNumber,
            Name = Input.Name,
            City = Input.City,
            ProfileImage = imageFileName,
            EmailConfirmed = true,
            ServiceId = Input.ServiceId
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "ServiceProvider");
            var serviceProvider = new AlaElTalab.Models.ServiceProvider
            {
                UserId = user.Id,
                Name = Input.Name,
                Username = Input.Username,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                City = Input.City,
                ProfileImage = imageFileName,
                AverageRating = 0,
                Price = Input.Price,
                ServiceId = Input.ServiceId 
            };

            _context.ServiceProviders.Add(serviceProvider);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
