using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.DTOs;
using CinemaTicketSystem.Models;

namespace CinemaTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly CinemaDbContext _context;

        public ReservationsController(CinemaDbContext context)
        {
            _context = context;
        }

        [HttpGet("screening/{screeningId}")]
        public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetReservationsByScreening(int screeningId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.ScreeningId == screeningId)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    ScreeningId = r.ScreeningId,
                    UserId = r.UserId,
                    Row = r.Row,
                    Seat = r.Seat,
                    ReservedAt = r.ReservedAt
                })
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("screening/{screeningId}/seatmap")]
        public async Task<ActionResult<IEnumerable<SeatMapDto>>> GetSeatMap(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null) return NotFound();

            var reservations = await _context.Reservations
                .Where(r => r.ScreeningId == screeningId)
                .ToListAsync();

            var seatMap = new List<SeatMapDto>();

            for (int row = 0; row < screening.Cinema.Rows; row++)
            {
                for (int seat = 0; seat < screening.Cinema.SeatsPerRow; seat++)
                {
                    var reservation = reservations.FirstOrDefault(r => r.Row == row && r.Seat == seat);
                    seatMap.Add(new SeatMapDto
                    {
                        Row = row,
                        Seat = seat,
                        IsReserved = reservation != null,
                        UserId = reservation?.UserId
                    });
                }
            }

            return Ok(seatMap);
        }

        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> CreateReservation(CreateReservationDto dto, [FromQuery] int userId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == dto.ScreeningId);

            if (screening == null)
            {
                return BadRequest(new { message = "Screening not found" });
            }

            if (dto.Row < 0 || dto.Row >= screening.Cinema.Rows ||
                dto.Seat < 0 || dto.Seat >= screening.Cinema.SeatsPerRow)
            {
                return BadRequest(new { message = "Invalid seat position" });
            }

            var reservation = new Reservation
            {
                ScreeningId = dto.ScreeningId,
                UserId = userId,
                Row = dto.Row,
                Seat = dto.Seat,
                ReservedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This seat is already reserved. Please select another seat." });
            }

            return CreatedAtAction(nameof(GetReservationsByScreening), 
                new { screeningId = reservation.ScreeningId }, 
                new ReservationResponseDto
                {
                    Id = reservation.Id,
                    ScreeningId = reservation.ScreeningId,
                    UserId = reservation.UserId,
                    Row = reservation.Row,
                    Seat = reservation.Seat,
                    ReservedAt = reservation.ReservedAt
                });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id, [FromQuery] int userId)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            
            if (reservation == null) return NotFound();

            if (reservation.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Reservation was modified by another process." });
            }

            return NoContent();
        }

        [HttpDelete("seat")]
        public async Task<IActionResult> CancelReservationBySeat([FromQuery] int screeningId, [FromQuery] int row, [FromQuery] int seat, [FromQuery] int userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ScreeningId == screeningId && r.Row == row && r.Seat == seat && r.UserId == userId);

            if (reservation == null) return NotFound();

            try
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Reservation was modified by another process." });
            }

            return NoContent();
        }
    }
}
