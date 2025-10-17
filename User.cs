using SQLite;

namespace RentreyApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ⭐ NEW: Fields for Authentication
        [Unique] // Ensures no two users share the same email
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Store hash, not plain password
        public string PhoneNumber { get; set; } // ⭐ New field for Create Account page

        // Existing Profile Fields (for Create Account page)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string TaxFileNumber { get; set; }

        // Gamification Field
        public int Points { get; set; } = 0;
    }
}