using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Add this line
using CinemaTicketSystem.Models;
using CinemaTicketSystem.Validation;

namespace CinemaTicketSystem.ViewModels
{
    public class ScreeningCreateViewModel
    {
        [Required(ErrorMessage = "Movie is required")]
        [Display(Name = "Movie")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Cinema is required")]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [Required(ErrorMessage = "Screening date and time is required")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Screening date and time must be in the future.")]
        [Display(Name = "Screening Date & Time")]
        public DateTime ScreeningDateTime { get; set; }

        [Required(ErrorMessage = "Ticket price is required")]
        [Range(0.01, 50, ErrorMessage = "Ticket price must be between 0.01 and 50")]
        [Display(Name = "Ticket Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TicketPrice { get; set; }

        public List<Movie> Movies { get; set; } = new List<Movie>();
        public List<Cinema> Cinemas { get; set; } = new List<Cinema>();
    }
}
