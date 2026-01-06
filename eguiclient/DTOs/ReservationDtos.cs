using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.DTOs
{
    public class CreateReservationDto
    {
        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public int Row { get; set; }

        [Required]
        public int Seat { get; set; }
    }

    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int ScreeningId { get; set; }
        public int UserId { get; set; }
        public int Row { get; set; }
        public int Seat { get; set; }
        public DateTime ReservedAt { get; set; }
    }

    public class SeatMapDto
    {
        public int Row { get; set; }
        public int Seat { get; set; }
        public bool IsReserved { get; set; }
        public int? UserId { get; set; }
    }
}