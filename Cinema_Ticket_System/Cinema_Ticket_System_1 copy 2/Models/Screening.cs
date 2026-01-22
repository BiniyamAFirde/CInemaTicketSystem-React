using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Screening
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        public DateTime ScreeningDateTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TicketPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [ForeignKey("MovieId")]
        public virtual Movie? Movie { get; set; }

        [ForeignKey("CinemaId")]
        public virtual Cinema? Cinema { get; set; }

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}