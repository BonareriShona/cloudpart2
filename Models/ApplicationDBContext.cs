using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Data  // 👀 Make sure this matches your project's namespace
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { 
        }

        public DbSet<Venue> Venues { get; set; }  // Ensure it's plural!


        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        }
}
