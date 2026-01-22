using System.ComponentModel.DataAnnotations;
using CinemaTicketSystem.Models;

namespace CinemaTicketSystem.ViewModels
{
    public class BookingViewModel
    {
        public int ScreeningId { get; set; }

        [Display(Name = "Number of Tickets")]
        public int NumberOfTickets { get; set; }

        public string? MovieTitle { get; set; }
        public DateTime ScreeningTime { get; set; }
        public decimal TicketPrice { get; set; }

        // Seat information
        public string? CinemaName { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public List<SeatViewModel> Seats { get; set; } = new List<SeatViewModel>();

        // Selected seat IDs for booking
        public List<int> SelectedSeatIds { get; set; } = new List<int>();
    }

    public class SeatViewModel
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; } = "Available";
    }
}
