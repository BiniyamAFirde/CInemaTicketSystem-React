using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.Models
{
    public class Screening
    {
        public int Id { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        [MaxLength(300)]
        public string MovieTitle { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        public virtual Cinema Cinema { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }

        public Screening()
        {
            Reservations = new List<Reservation>();
        }
    }
}