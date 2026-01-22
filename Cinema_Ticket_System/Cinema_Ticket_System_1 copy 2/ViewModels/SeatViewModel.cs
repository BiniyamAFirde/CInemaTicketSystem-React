using System;

namespace CinemaTicketSystem.ViewModels
{
    public class SeatViewModel
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public string SeatNumber { get; set; } = string.Empty;  // FIXED: Changed from int to string
        public string Status { get; set; } = "Available";

        // 🔄 ATOMIC: Store RowVersion for seat concurrency control
        public byte[]? RowVersion { get; set; }
        
        // Property to track if seat is selected by user in the UI
        public bool IsSelected { get; set; }
    }
}