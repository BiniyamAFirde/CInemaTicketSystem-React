using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }

        // 🔄 ATOMIC: Optimistic Concurrency Control Token
        // This is automatically managed by EF Core
        // Updated on every change to enable optimistic locking
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Alias for views that expect "Version"
        [NotMapped]
        public byte[]? Version => RowVersion;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("ScreeningId")]
        public virtual Screening? Screening { get; set; }

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}