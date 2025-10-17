using System.Text;

namespace Rentrey.Maui
{
    // Simple helper class for consistent (simulated) password hashing across the app
    public static class PasswordHelper
    {
        // NOTE: In a production app, use a strong library like BCrypt.
        private const string Salt = "rentreysalt";

        public static string CreateSimpleHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            // Combine password with a simple salt and return the hash code
            return (password + Salt).GetHashCode().ToString();
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return CreateSimpleHash(enteredPassword) == storedHash;
        }
    }
}