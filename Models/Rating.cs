using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Models
{
    public enum RatingValue
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5
    }
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }
        public int BookingId { get; set; }
        public RatingValue RatingValue { get; set; }

        //navigation properties 
        public virtual Booking Booking { get; set; }
        public int ServiceProviderId { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }

    }
}
