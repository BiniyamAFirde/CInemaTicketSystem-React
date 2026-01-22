using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [Range(1, 50)]
        public int Rows { get; set; }

        [Required]
        [Range(1, 50)]
        public int SeatsPerRow { get; set; }

        public virtual ICollection<Screening> Screenings { get; set; }

        public Cinema()
        {
            Screenings = new List<Screening>();
        }
    }
}