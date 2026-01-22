using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Genre { get; set; } = string.Empty;

        [Required]
        [Range(1, 500)]
        public int DurationMinutes { get; set; } // in minutes

        [StringLength(50)]
        public string? Rating { get; set; } // e.g., PG-13, R, etc.

        public string? ImageUrl { get; set; }
        
        public string? PosterUrl { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}