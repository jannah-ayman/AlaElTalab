using AlaElTalab.Data;
using AlaElTalab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlaElTalab.Controllers
{
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int bookingId, RatingValue ratingValue)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.ServiceProvider)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.Customer.UserId == user.Id);

            if (booking == null) return NotFound();

            if (booking.Status != Status.Completed)
            {
                return BadRequest("You can only rate completed bookings");
            }

            // Check if already rated
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);

            if (existingRating != null)
            {
                existingRating.RatingValue = ratingValue;
                _context.Update(existingRating);
            }
            else
            {
                var rating = new Rating
                {
                    BookingId = bookingId,
                    RatingValue = ratingValue,
                    ServiceProviderId = booking.ServiceProviderId
                };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();

            await UpdateServiceProviderRating(booking.ServiceProviderId);

            TempData["SuccessMessage"] = "Thank you for your rating!";
            return RedirectToAction("Bookings", "Customers");
        }

        private async Task UpdateServiceProviderRating(int serviceProviderId)
        {
            var serviceProvider = await _context.ServiceProviders
                .Include(sp => sp.Ratings)
                .FirstOrDefaultAsync(sp => sp.ServiceProviderId == serviceProviderId);

            if (serviceProvider != null)
            {
                serviceProvider.CalculateAverageRating();
                _context.Update(serviceProvider);
                await _context.SaveChangesAsync();
            }
        }
    }
}