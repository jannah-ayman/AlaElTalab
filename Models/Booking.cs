using System.ComponentModel.DataAnnotations;

namespace AlaElTalab.Models
{
    public enum Status
    {
        Confirmed,
        Rejected,
        Pending,
        InProgress, 
        Completed,
        Cancelled
    }
    public enum PaymentStatus
    {
        Paid, Not_Paid
    }
    public class Booking
    {
        //model for Bookings
        [Key]
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int ServiceProviderId { get; set; }
        public int? RatingId { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        public Status Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        //navigation properties 
        public virtual ServiceProvider ServiceProvider { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Rating Rating { get; set; }
    }
}
