
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Models;
using CinemaTicketSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicketSystem.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Seats)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
            {
                return NotFound();
            }

            var model = new BookingViewModel
            {
                ScreeningId = screening.Id,
                MovieTitle = screening.Movie?.Title ?? "",
                ScreeningTime = screening.ScreeningDateTime,
                TicketPrice = screening.TicketPrice,
                CinemaName = screening.Cinema?.Name ?? "",
                Rows = screening.Cinema?.Rows ?? 0,
                SeatsPerRow = screening.Cinema?.SeatsPerRow ?? 0,
                Seats = screening.Seats
                    .OrderBy(s => s.Row)
                    .ThenBy(s => s.SeatNumber)
                    .Select(s => new SeatViewModel
                    {
                        Id = s.Id,
                        Row = s.Row,
                        SeatNumber = s.SeatNumber,
                        Status = s.Status.ToString()
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            // Validate that at least one seat is selected
            if (model.SelectedSeatIds == null || !model.SelectedSeatIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one seat.");
                return await RedisplayForm(model);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            int newBookingId = 0;

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var screening = await _context.Screenings
                                .Include(s => s.Seats)
                                .FirstOrDefaultAsync(s => s.Id == model.ScreeningId);

                            if (screening == null)
                            {
                                throw new InvalidOperationException("Screening not found.");
                            }

                            // Fetch selected seats and lock them for update
                            var seatsToBook = await _context.Seats
                                .Where(s => model.SelectedSeatIds.Contains(s.Id) && s.ScreeningId == model.ScreeningId)
                                .ToListAsync();

                            // Validate all seats are available
                            if (seatsToBook.Count != model.SelectedSeatIds.Count)
                            {
                                throw new InvalidOperationException("Invalid seat selection.");
                            }

                            if (seatsToBook.Any(s => s.Status != SeatStatus.Available))
                            {
                                throw new DbUpdateConcurrencyException("One or more selected seats are no longer available.");
                            }

                            var user = await _userManager.GetUserAsync(User);
                            if (user == null)
                            {
                                throw new InvalidOperationException("User not found.");
                            }

                            var booking = new Booking
                            {
                                UserId = user.Id,
                                ScreeningId = screening.Id,
                                TotalPrice = seatsToBook.Count * screening.TicketPrice,
                                BookingDate = DateTime.UtcNow,
                                Status = "Confirmed"
                            };

                            foreach (var seat in seatsToBook)
                            {
                                seat.Status = SeatStatus.Booked;
                                booking.Seats.Add(seat);
                            }

                            _context.Bookings.Add(booking);
                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();

                            newBookingId = booking.Id;
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            await transaction.RollbackAsync();
                            throw; // Re-throw to be caught by the outer catch block
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                return RedirectToAction("Success", new { id = newBookingId });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "The seats you selected were just booked by another user. Please select different seats.");
                return await RedisplayForm(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return await RedisplayForm(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while processing your booking. Please try again.");
                return await RedisplayForm(model);
            }
        }

        private async Task<IActionResult> RedisplayForm(BookingViewModel model)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Seats)
                .FirstOrDefaultAsync(s => s.Id == model.ScreeningId);

            if (screening == null)
                return NotFound();

            model.MovieTitle = screening.Movie?.Title ?? "";
            model.ScreeningTime = screening.ScreeningDateTime;
            model.TicketPrice = screening.TicketPrice;
            model.CinemaName = screening.Cinema?.Name ?? "";
            model.Rows = screening.Cinema?.Rows ?? 0;
            model.SeatsPerRow = screening.Cinema?.SeatsPerRow ?? 0;
            model.Seats = screening.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.SeatNumber)
                .Select(s => new SeatViewModel
                {
                    Id = s.Id,
                    Row = s.Row,
                    SeatNumber = s.SeatNumber,
                    Status = s.Status.ToString()
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var booking = await _context.Bookings
                .Include(b => b.Screening)
                .ThenInclude(s => s!.Movie)
                .Include(b => b.Seats)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        [ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, byte[] version)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var booking = await _context.Bookings
                .Include(b => b.Seats)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or you are not authorized to cancel it.";
                return RedirectToAction("Bookings", "Account");
            }

            _context.Entry(booking).Property("Version").OriginalValue = version;

            try
            {
                // Release all booked seats
                foreach (var seat in booking.Seats)
                {
                    seat.Status = SeatStatus.Available;
                    seat.BookingId = null;
                }

                // Remove booking
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking cancelled successfully. Seats have been released.";
                return RedirectToAction("Bookings", "Account");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "The booking has been modified by another user. Please refresh and try again.";
                return RedirectToAction("Bookings", "Account");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling your booking.";
                return RedirectToAction("Index", "Account");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                .ThenInclude(s => s!.Movie)
                .Include(b => b.Seats)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}
