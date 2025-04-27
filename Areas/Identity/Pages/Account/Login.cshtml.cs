using AlaElTalab.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Handles all sign-in operations (login/logout)
        private readonly SignInManager<ApplicationUser> _signInManager;
        //For logging authentication events
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        //Page Load
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                    Response.Redirect("/Customers/Index");
                else if (User.IsInRole("ServiceProvider"))
                    Response.Redirect("/ServiceProviders/Index");
                else
                    Response.Redirect("/");
            }

            ReturnUrl = returnUrl;
        }

        //Form Submission
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Prevent already logged in users from submitting the form
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                    return LocalRedirect("/Customers/Index");
                else if (User.IsInRole("ServiceProvider"))
                    return LocalRedirect("/ServiceProviders/Index");
                return LocalRedirect(returnUrl);
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Username, Input.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _signInManager.UserManager.FindByNameAsync(Input.Username);

                    if (await _signInManager.UserManager.IsInRoleAsync(user, "Customer"))
                        return LocalRedirect("/Customers/Index");
                    else if (await _signInManager.UserManager.IsInRoleAsync(user, "ServiceProvider"))
                        return LocalRedirect("/ServiceProviders/Index");

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }
}