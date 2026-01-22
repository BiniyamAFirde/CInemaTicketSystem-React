using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Cinema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int Rows { get; set; }

        [Required]
        public int SeatsPerRow { get; set; }

        // Computed property for views - not stored in database
        [NotMapped]
        public int TotalSeats => Rows * SeatsPerRow;

        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}