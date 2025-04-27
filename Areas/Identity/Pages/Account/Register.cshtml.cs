// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AlaElTalab.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

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


            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "City is required\n")]
            [StringLength(50)]
            public string City { get; set; }

            [Display(Name = "Profile Image")]
            public IFormFile ProfileImageFile { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

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

                if (Input.ProfileImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfileImageFile", "File size must not exceed 5MB.");
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
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                var customer = new Customer
                {
                    UserId = user.Id,
                    Name = Input.Name,
                    Username = Input.Username,
                    Email = Input.Email,
                    City = Input.City,
                    PhoneNumber = Input.PhoneNumber,
                    ProfileImage = imageFileName
                };

                _context.Customers.Add(customer);
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
}