using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.ViewModels
{
    public class ProfileViewModel
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Concurrency token - stores the ConcurrencyStamp from the database
        public string ConcurrencyStamp { get; set; } = string.Empty;
    }
}