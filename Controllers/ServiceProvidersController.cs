using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlaElTalab.Controllers
{
    [Authorize(Policy = "ServiceProviderOnly")]
    public class ServiceProvidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ServiceProvidersController(
      UserManager<ApplicationUser> userManager,
      ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

            if (serviceProvider == null) return NotFound();

            ViewData["FirstName"] = serviceProvider.Name?.Split(' ').FirstOrDefault() ?? "Professional";
            return View();
        }

        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var serviceProvider = await _context.ServiceProviders
                .Include(sp => sp.Service) 
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            return View(serviceProvider);
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (serviceProvider == null) return NotFound();

            return View(serviceProvider);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string username, string email, string phoneNumber, 
            string city, float price, IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (serviceProvider == null) return NotFound();

            // Update fields

            serviceProvider.Username = username;
            serviceProvider.Email = email;
            serviceProvider.PhoneNumber = phoneNumber;
            serviceProvider.City = city;
            serviceProvider.Price = price;

            // Update Identity User fields
            user.UserName = username;
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.City = city; // If you have the City in your ApplicationUser

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var ext = Path.GetExtension(profilePicture.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("profilePicture", "Only JPG and PNG files are allowed.");
                    return View(serviceProvider);
                }

                if (profilePicture.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("profilePicture", "File size must not exceed 5MB.");
                    return View(serviceProvider);
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
                if (!string.IsNullOrEmpty(serviceProvider.ProfileImage) &&
                    serviceProvider.ProfileImage != "default-profile.png")
                {
                    var oldFilePath = Path.Combine(uploadsPath, serviceProvider.ProfileImage);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                serviceProvider.ProfileImage = newFileName;
                user.ProfileImage = newFileName;
            }

            _context.ServiceProviders.Update(serviceProvider);
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Account");
        }

        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            // 1. Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Find service provider record
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

            if (serviceProvider != null)
            {
                // 3. Delete profile image (if custom)
                if (!string.IsNullOrEmpty(serviceProvider.ProfileImage) &&
                    serviceProvider.ProfileImage != "default-profile.png")
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsPath, serviceProvider.ProfileImage);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // 4. Delete related bookings
                var bookings = await _context.Bookings
                    .Where(b => b.ServiceProviderId == serviceProvider.ServiceProviderId)
                    .ToListAsync();
                _context.Bookings.RemoveRange(bookings);

                // 5. Delete service provider record
                _context.ServiceProviders.Remove(serviceProvider);
                await _context.SaveChangesAsync();
            }

            // 6. Delete user account
            await _userManager.DeleteAsync(user);

            // 7. Sign out
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Booking()
        {
            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Find the service provider associated with this user
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

            if (serviceProvider == null)
            {
                return NotFound("Service provider not found");
            }

            // Get all bookings for this service provider with customer details, excluding cancelled and rejected bookings
            var bookings = await _context.Bookings
                .Where(b => b.ServiceProviderId == serviceProvider.ServiceProviderId &&
                           b.Status != Status.Cancelled &&
                           b.Status != Status.Rejected) // Exclude cancelled and rejected bookings
                .Include(b => b.Customer) // Include customer details
                .Include(b => b.ServiceProvider) // Include service provider details
                .OrderByDescending(b => b.DateTime) // Sort by most recent
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = Status.Confirmed;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Booking");
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = Status.Rejected;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Booking");
        }

        [HttpPost]
        public async Task<IActionResult> StartProgress(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = Status.InProgress;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Booking");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = Status.Completed;
            booking.PaymentStatus = PaymentStatus.Not_Paid;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Booking");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.PaymentStatus = PaymentStatus.Paid;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Booking");
        }
    }
}