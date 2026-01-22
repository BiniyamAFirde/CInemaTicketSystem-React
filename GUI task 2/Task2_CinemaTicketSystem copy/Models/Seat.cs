using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public enum SeatStatus
    {
        Available,
        Booked,
        Locked // Temporarily locked during booking process
    }

    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScreeningId { get; set; }

        [ForeignKey("ScreeningId")]
        public Screening? Screening { get; set; }

        [Required]
        public int Row { get; set; }

        [Required]
        public int SeatNumber { get; set; }

        [Required]
        public SeatStatus Status { get; set; }

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
