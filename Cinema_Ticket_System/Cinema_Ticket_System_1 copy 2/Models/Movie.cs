using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [StringLength(100)]
        public string? Genre { get; set; }

        public int? DurationMinutes { get; set; }

        [StringLength(100)]
        public string? Director { get; set; }

        [Range(0, 10)]
        public decimal? Rating { get; set; }

        [StringLength(500)]
        public string? PosterUrl { get; set; }

        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}