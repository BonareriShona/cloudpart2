using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using Azure.Storage.Blobs;

using System.IO;
using Azure.Storage.Blobs.Models;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "venueimages"; // your blob container name
        private readonly ApplicationDbContext _context;

        // Constructor to inject ApplicationDbContext
        public VenueController(ApplicationDbContext context , BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Venue (List of venues)
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync(); // Ensure it's "Venues"
            return View(venues);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            // ✅ Check and assign default ImageUrl if not provided
            if (string.IsNullOrEmpty(venue.ImageUrl))
            {
                venue.ImageUrl = "/images/default-placeholder.png"; // or your actual default image path
            }

            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }


        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }


        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            // Check if this venue is used in any bookings
            bool isBooked = await _context.Bookings.AnyAsync(b => b.VenueId == id);
            if (isBooked)
            {
                TempData["Error"] = "Cannot delete this venue because it is associated with existing bookings.";
                return RedirectToAction("Index"); // Or wherever your venue list is
            }


            if (venue != null) 
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index)); // Redirect to the venue list
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue); // Pass the venue object to the Edit view
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl,ImageFile")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            { 
                try
                {
                    if (venue.ImageFile != null)
                    {
                        // Upload new image to blob and update the URL
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);



                        venue.ImageUrl = blobUrl;
                    }

                    // Update the venue in the database
                    _context.Update(venue);
                    await _context.SaveChangesAsync(); // Save changes

                    return RedirectToAction(nameof(Index)); // Redirect to the venue list
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If model state is invalid, return the same view with error messages
            return View(venue);
        }





        private async Task<string?> UploadImageToBlobAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=assignmentsm;AccountKey=...;EndpointSuffix=core.windows.net";
            var containerName = "assignmentsm";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var contentType = imageFile.ContentType;

            // Set correct content type during upload
            var blobHttpHeader = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeader
                });
            }

            return blobClient.Uri.ToString();
        }



        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
    }
