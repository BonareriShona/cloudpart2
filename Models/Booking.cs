using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public required Event? Event { get; set; }

        [Required]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }
        public required Venue? Venue { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }
    }
}
    

