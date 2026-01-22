// Models/User.cs - COMPLETE REPLACEMENT
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
        
        // ✅ For SQLite, we'll use a timestamp as a string
        [ConcurrencyCheck] // This tells EF Core to check this field for concurrency
        public string RowVersion { get; set; }
        
        public virtual ICollection<Reservation> Reservations { get; set; }
        
        public User()
        {
            Reservations = new List<Reservation>();
            RowVersion = Guid.NewGuid().ToString(); // ✅ Generate unique version
        }
    }
}