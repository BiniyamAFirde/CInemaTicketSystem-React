using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.DTOs
{
    public class CreateScreeningDto
    {
        [Required]
        public int CinemaId { get; set; }

        [Required]
        public string MovieTitle { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }
    }

    public class ScreeningResponseDto
    {
        public int Id { get; set; }
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public string MovieTitle { get; set; }
        public DateTime StartDateTime { get; set; }
    }
}
