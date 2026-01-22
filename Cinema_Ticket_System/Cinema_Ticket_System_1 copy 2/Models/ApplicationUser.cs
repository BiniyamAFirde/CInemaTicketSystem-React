using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        // 🔄 ATOMIC: ConcurrencyStamp is inherited from IdentityUser
        // It's automatically managed by AspNetCore.Identity
        // Used for optimistic concurrency control in user management

        // Alias for views that expect "Version" instead of "ConcurrencyStamp"
        [NotMapped]
        public string? Version => ConcurrencyStamp;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}