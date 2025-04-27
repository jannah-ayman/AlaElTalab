using Microsoft.EntityFrameworkCore;
using AlaElTalab.Data;
using AlaElTalab.Models;

namespace AlaElTalab.Services
{
    public class RatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRating(int bookingId, RatingValue ratingValue, string comment)
        {
            var booking = await _context.Bookings
                .Include(b => b.ServiceProvider)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) throw new Exception("Booking not found");
            if (booking.Status != Status.Completed) throw new Exception("Only completed bookings can be rated");

            var rating = new Rating
            {
                BookingId = bookingId,
                ServiceProviderId = booking.ServiceProviderId,
                RatingValue = ratingValue
            };

            _context.Ratings.Add(rating);
            booking.ServiceProvider.CalculateAverageRating();

            await _context.SaveChangesAsync();
        }
    }
}
