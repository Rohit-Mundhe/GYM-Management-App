namespace GYMApp.Models
{
    public class User
    {
        public int UserId { get; set; }             // Primary key
        public string Name { get; set; }            // User full name
        public string Email { get; set; }           // Unique email for login
        public string PasswordHash { get; set; }    // Hashed password
        public string Role { get; set; }            // "Admin", "Trainer", "Member"
        public DateTime CreatedDate { get; set; }   // Auto-set by DB
    }
}
