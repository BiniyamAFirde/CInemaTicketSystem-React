using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystem.Models
{
    [Index(nameof(ScreeningId), nameof(Row), nameof(Seat), IsUnique = true)]
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0, 49)]
        public int Row { get; set; }

        [Required]
        [Range(0, 49)]
        public int Seat { get; set; }

        [Required]
        public DateTime ReservedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Screening Screening { get; set; }
        public virtual User User { get; set; }
    }
}