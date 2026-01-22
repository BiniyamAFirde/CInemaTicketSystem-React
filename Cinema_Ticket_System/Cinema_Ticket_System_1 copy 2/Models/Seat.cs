using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public int Row { get; set; }  // ← Must be int

        [Required]
        [StringLength(2)]
        public string SeatNumber { get; set; } = string.Empty;  // ← Must be string

        [Required]
        public SeatStatus Status { get; set; } = SeatStatus.Available;

        public int? BookingId { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        [ForeignKey("ScreeningId")]
        public virtual Screening? Screening { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }
    }

    public enum SeatStatus
    {
        Available,
        Booked,
        Blocked
    }
}