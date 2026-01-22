using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.ViewModels
{
    public class BookingCancelViewModel
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ScreeningTime { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> Seats { get; set; } = new List<string>();

        // 🔄 ATOMIC: Store RowVersion for booking concurrency control
        [Display(Name = "Row Version")]
        public byte[]? RowVersion { get; set; }
    }
}