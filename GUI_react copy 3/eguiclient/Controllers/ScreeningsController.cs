using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.DTOs;
using CinemaTicketSystem.Models;

namespace CinemaTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreeningsController : ControllerBase
    {
        private readonly CinemaDbContext _context;

        public ScreeningsController(CinemaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScreeningResponseDto>>> GetScreenings()
        {
            var screenings = await _context.Screenings
                .Include(s => s.Cinema)
                .Select(s => new ScreeningResponseDto
                {
                    Id = s.Id,
                    CinemaId = s.CinemaId,
                    CinemaName = s.Cinema.Name,
                    Rows = s.Cinema.Rows,
                    SeatsPerRow = s.Cinema.SeatsPerRow,
                    MovieTitle = s.MovieTitle,
                    StartDateTime = s.StartDateTime
                })
                .ToListAsync();

            return Ok(screenings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScreeningResponseDto>> GetScreening(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null) return NotFound();

            return Ok(new ScreeningResponseDto
            {
                Id = screening.Id,
                CinemaId = screening.CinemaId,
                CinemaName = screening.Cinema.Name,
                Rows = screening.Cinema.Rows,
                SeatsPerRow = screening.Cinema.SeatsPerRow,
                MovieTitle = screening.MovieTitle,
                StartDateTime = screening.StartDateTime
            });
        }

        [HttpPost]
        public async Task<ActionResult<ScreeningResponseDto>> CreateScreening(CreateScreeningDto dto)
        {
            var cinema = await _context.Cinemas.FindAsync(dto.CinemaId);
            if (cinema == null)
            {
                return BadRequest(new { message = "Cinema not found" });
            }

            var screening = new Screening
            {
                CinemaId = dto.CinemaId,
                MovieTitle = dto.MovieTitle,
                StartDateTime = dto.StartDateTime
            };

            _context.Screenings.Add(screening);
            await _context.SaveChangesAsync();

            var response = new ScreeningResponseDto
            {
                Id = screening.Id,
                CinemaId = screening.CinemaId,
                CinemaName = cinema.Name,
                Rows = cinema.Rows,
                SeatsPerRow = cinema.SeatsPerRow,
                MovieTitle = screening.MovieTitle,
                StartDateTime = screening.StartDateTime
            };

            return CreatedAtAction(nameof(GetScreening), new { id = screening.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScreening(int id)
        {
            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null) return NotFound();

            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

