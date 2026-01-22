namespace CinemaTicketSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        public int ScreeningId { get; set; }
        public Screening? Screening { get; set; }
        public string SeatNumber { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public string ConfirmationNumber { get; set; } = null!;
    }
}