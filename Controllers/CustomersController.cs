using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlaElTalab.Controllers
{
    //controls access
    [Authorize(Policy = "CustomerOnly")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();
 
            ViewData["FirstName"] = customer.Name?.Split(' ').FirstOrDefault() ?? "User";
            return View();
        }

        //An asynchronous method can run without blocking the rest of your
        //program while it waits for something slow to finish 
        public async Task<IActionResult> Account() // Views/Customers/Account.cshtml
        {
            //UserManager<TUser> is a built-in class in ASP.NET Core Identity
            //that manages user accounts. It gives you ready-made functions to:
            //Create users, Update users ,Delete users and Get users 

            // GetUserAsync(User) is a method of UserManager that: 
            //Takes the current HTTP User(the one logged in).
            //Fetches their full ApplicationUser object from the database.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)  return NotFound();
            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string username, string email, string phoneNumber, string city, IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null) return NotFound();

            customer.Username = username;
            customer.Email = email;
            customer.PhoneNumber = phoneNumber;
            customer.City = city;

            user.UserName = username;
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.City = city;

            // Handle profile picture
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var ext = Path.GetExtension(profilePicture.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("profilePicture", "Only JPG and PNG files are allowed.");
                    return View(customer); // show validation error
                }

                if (profilePicture.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("profilePicture", "File size must not exceed 5MB.");
                    return View(customer);
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var newFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsPath, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Delete old image if exists and is not default
                if (!string.IsNullOrEmpty(customer.ProfileImage) && customer.ProfileImage != "default-profile.png")
                {
                    var oldFilePath = Path.Combine(uploadsPath, customer.ProfileImage);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                customer.ProfileImage = newFileName;
                user.ProfileImage = newFileName;
            }

            _context.Customers.Update(customer);
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Account");
        }

        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer != null)
            {
                // Delete profile image if exists and is not default
                if (!string.IsNullOrEmpty(customer.ProfileImage) &&
                    customer.ProfileImage != "default-profile.png")
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsPath, customer.ProfileImage);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete related bookings
                var bookings = await _context.Bookings
                    .Where(b => b.CustomerId == customer.CustomerId)
                    .ToListAsync();
                _context.Bookings.RemoveRange(bookings);

                // Delete customer record
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }

            // Delete user account
            await _userManager.DeleteAsync(user);

            // Sign out
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Carpenter() // Views/Customers/Carpenter.cshtml
        {
            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Find customer record to get their city
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            // Get housekeepers (ServiceId = 4) in the same city
            var carp = await _context.ServiceProviders
                .Where(sp => sp.ServiceId == 4 && sp.City == customer.City)
                .Include(sp => sp.Service) // Include service details if needed
                .ToListAsync();

            return View(carp);
        }
  
        public async Task<IActionResult> Cleaning()
        {
            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Find customer record to get their city
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            // Get housekeepers (ServiceId = 1) in the same city
            var housekeepers = await _context.ServiceProviders
                .Where(sp => sp.ServiceId == 1 && sp.City == customer.City)
                .Include(sp => sp.Service) // Include service details if needed
                .ToListAsync();

            return View(housekeepers);
        }
        public async Task<IActionResult> Electrician() //// Views/Customers/Electrician.cshtml
        {
            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Find customer record to get their city
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            // Get housekeepers (ServiceId = 2) in the same city
            var elect = await _context.ServiceProviders
                .Where(sp => sp.ServiceId == 2 && sp.City == customer.City)
                .Include(sp => sp.Service) // Include service details if needed
                .ToListAsync();

            return View(elect);
        }
        
        public async Task<IActionResult> Plumber() // Views/Customers/Plumber.cshtml
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var plumb = await _context.ServiceProviders
                .Where(sp => sp.ServiceId == 3 && sp.City == customer.City)
                .Include(sp => sp.Service) // Include service details
                .ToListAsync();

            return View(plumb);
        }

        public async Task<IActionResult> Book(int serviceProviderId)
        {
            var provider = await _context.ServiceProviders.FindAsync(serviceProviderId);
            if (provider == null)
            {
                return NotFound();
            }

            ViewBag.ServiceProviderId = serviceProviderId;
            ViewBag.ProviderName = provider.Name;
            ViewBag.Price = provider.Price;
            ViewBag.Location = provider.City;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int serviceProviderId, DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "You cannot book appointments in the past. Please select today's date or a future date.";
                return RedirectToAction("Book", new { serviceProviderId });
            }

            var userId = _userManager.GetUserId(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            var booking = new Booking
            {
                CustomerId = customer.CustomerId,
                ServiceProviderId = serviceProviderId,
                DateTime = date,
                Status = Status.Pending,
                PaymentStatus = PaymentStatus.Not_Paid
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Bookings");
        }
        public async Task<IActionResult> Bookings() // Views/Customers/Bookings.cshtml
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null) return NotFound();

            var bookings = await _context.Bookings
                .Include(b => b.ServiceProvider)
                .Include(b => b.Rating) 
                .Where(b => b.CustomerId == customer.CustomerId)
                .OrderByDescending(b => b.DateTime) 
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            if (booking.Status == Status.Pending || booking.Status == Status.Confirmed || booking.Status == Status.InProgress)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Bookings");
        }
    }


}
