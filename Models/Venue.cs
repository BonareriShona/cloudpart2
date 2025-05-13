using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{

    [Table("Venues")]
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string? VenueName { get; set; }

        [Required]
        public string? Location { get; set; }

        [Required]
        public int Capacity { get; set; }



        //image file uploaded by the user
        [NotMapped]
        public IFormFile? ImageFile { get; set; }


        //used to store the URL of the image uploaded 
     
        public string? ImageUrl { get; set; }
        //add this new one
        public ICollection<Booking>? Bookings { get; set; }  

    }

}

