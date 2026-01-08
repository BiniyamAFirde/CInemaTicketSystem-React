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
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(CinemaDbContext context, ILogger<ReservationsController> logger)
        {
            _context = context;
            _logger = logger;
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

        [HttpPost("check-availability")]
        public async Task<ActionResult<SeatAvailabilityDto>> CheckSeatAvailability(CheckSeatDto dto)
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

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => 
                    r.ScreeningId == dto.ScreeningId && 
                    r.Row == dto.Row && 
                    r.Seat == dto.Seat);

            if (existingReservation != null)
            {
                return Ok(new SeatAvailabilityDto
                {
                    IsAvailable = false,
                    Message = "This seat is already reserved",
                    ReservedBy = existingReservation.UserId
                });
            }

            return Ok(new SeatAvailabilityDto
            {
                IsAvailable = true,
                Message = "Seat is available",
                ReservedBy = null
            });
        }

       
        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> CreateReservation(
            CreateReservationDto dto, 
            [FromQuery] int userId)
        {
           
            int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
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

                   
                    var existingReservation = await _context.Reservations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => 
                            r.ScreeningId == dto.ScreeningId && 
                            r.Row == dto.Row && 
                            r.Seat == dto.Seat);

                    if (existingReservation != null)
                    {
                        _logger.LogWarning($"Seat conflict: Screening {dto.ScreeningId}, Row {dto.Row}, Seat {dto.Seat} already reserved by User {existingReservation.UserId}");
                        return Conflict(new 
                        { 
                            message = "⚠️ This seat is already reserved. Please select a different seat.",
                            conflict = true,
                            reservedBy = existingReservation.UserId
                        });
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

                   
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Reservation successful: User {userId} booked Screening {dto.ScreeningId}, Row {dto.Row}, Seat {dto.Seat}");

                    return CreatedAtAction(
                        nameof(GetReservationsByScreening), 
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
                catch (DbUpdateException ex)
                {
                 
                    _context.ChangeTracker.Clear();

                  
                    if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true ||
                        ex.InnerException?.Message.Contains("constraint") == true)
                    {
                        _logger.LogWarning($"Unique constraint violation: User {userId} attempted to book already reserved seat (Screening {dto.ScreeningId}, Row {dto.Row}, Seat {dto.Seat})");
                        
                        return Conflict(new 
                        { 
                            message = "⚠️ CONFLICT: This seat was just reserved by another user. Please select a different seat.",
                            conflict = true
                        });
                    }

                    
                    if (ex.InnerException?.Message.Contains("database is locked") == true ||
                        ex.InnerException?.Message.Contains("locked") == true)
                    {
                        retryCount++;
                        _logger.LogWarning($"Database locked, retry attempt {retryCount}/{maxRetries} for User {userId}");
                        
                        if (retryCount >= maxRetries)
                        {
                            _logger.LogError($"Failed after {maxRetries} retries due to database lock");
                            return StatusCode(500, new { message = "Server is busy. Please try again in a moment." });
                        }

                   
                        await Task.Delay(100 * retryCount);
                        continue; 
                    }

                    _logger.LogError(ex, "Database error during reservation creation");
                    return StatusCode(500, new { message = "Failed to create reservation", error = ex.InnerException?.Message });
                }
                catch (Exception ex)
                {
                    _context.ChangeTracker.Clear();
                    _logger.LogError(ex, "Unexpected error during reservation creation");
                    return StatusCode(500, new { message = "An unexpected error occurred" });
                }
            }

            return StatusCode(500, new { message = "Failed to create reservation after multiple attempts" });
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
        public async Task<IActionResult> CancelReservationBySeat(
            [FromQuery] int screeningId, 
            [FromQuery] int row, 
            [FromQuery] int seat, 
            [FromQuery] int userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => 
                    r.ScreeningId == screeningId && 
                    r.Row == row && 
                    r.Seat == seat && 
                    r.UserId == userId);

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

    public class CheckSeatDto
    {
        public int ScreeningId { get; set; }
        public int Row { get; set; }
        public int Seat { get; set; }
    }

    public class SeatAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public int? ReservedBy { get; set; }
    }
}