using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 50)]
        public int Rows { get; set; } // n rows

        [Required]
        [Range(1, 50)]
        public int SeatsPerRow { get; set; } // m seats per row

        public int TotalSeats => Rows * SeatsPerRow;

        // Navigation property
        public ICollection<Screening>? Screenings { get; set; }
    }
}
