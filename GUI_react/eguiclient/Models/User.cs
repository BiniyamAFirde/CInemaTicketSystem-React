using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace CinemaTicketSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }
        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public byte[] RowVersion { get; set; }  // ← Removed [Timestamp]
        public virtual ICollection<Reservation> Reservations { get; set; }
        public User()
        {
            Reservations = new List<Reservation>();
            RowVersion = Array.Empty<byte>();  // ← Initialize here
        }
    }
}