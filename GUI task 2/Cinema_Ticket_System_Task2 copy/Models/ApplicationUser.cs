using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // CORRECT: Use byte[] for SQL Server ROWVERSION/TIMESTAMP
        // or uint for MySQL TIMESTAMP (auto-incrementing version number)
        [Timestamp]
        public byte[] Version { get; set; } = null!;
        
        // Alternative for MySQL: Use an integer-based version
        // Uncomment this if using MySQL and comment out the byte[] above
        // [ConcurrencyCheck]
        // public int Version { get; set; }
    }
}