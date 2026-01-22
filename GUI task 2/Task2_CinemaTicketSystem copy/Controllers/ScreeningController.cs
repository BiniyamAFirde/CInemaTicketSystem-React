using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Models;
using CinemaTicketSystem.ViewModels;

namespace CinemaTicketSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ScreeningController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScreeningController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? movieId)
        {
            var screeningsQuery = _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Seats)
                .AsQueryable();

            if (movieId.HasValue)
            {
                screeningsQuery = screeningsQuery.Where(s => s.MovieId == movieId.Value);
            }

            var screenings = await screeningsQuery.ToListAsync();

            return View(screenings);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var movies = await _context.Movies.ToListAsync();
            var cinemas = await _context.Cinemas.ToListAsync();
            var model = new ScreeningCreateViewModel
            {
                Movies = movies,
                Cinemas = cinemas
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScreeningCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Movies = await _context.Movies.ToListAsync();
                model.Cinemas = await _context.Cinemas.ToListAsync();
                return View(model);
            }

            // Get the cinema to determine seat layout
            var cinema = await _context.Cinemas.FindAsync(model.CinemaId);
            if (cinema == null)
            {
                ModelState.AddModelError("", "Selected cinema not found.");
                model.Movies = await _context.Movies.ToListAsync();
                model.Cinemas = await _context.Cinemas.ToListAsync();
                return View(model);
            }

            var screening = new Screening
            {
                MovieId = model.MovieId,
                CinemaId = model.CinemaId,
                ScreeningDateTime = model.ScreeningDateTime,
                TicketPrice = model.TicketPrice
            };

            // Create seats based on cinema's row and seat layout
            for (int row = 1; row <= cinema.Rows; row++)
            {
                for (int seatNumber = 1; seatNumber <= cinema.SeatsPerRow; seatNumber++)
                {
                    screening.Seats.Add(new Seat
                    {
                        Row = row,
                        SeatNumber = seatNumber,
                        Status = SeatStatus.Available
                    });
                }
            }

            _context.Screenings.Add(screening);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Screening created successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
                return NotFound();

            return View(screening);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var screening = await _context.Screenings.FindAsync(id);

            if (screening == null)
                return NotFound();

            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Screening deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}