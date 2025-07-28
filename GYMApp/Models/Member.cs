namespace GYMApp.Models
{
    public class Member
    {
        public int MemberId { get; set; }             // Primary key
        public int UserId { get; set; }               // Linked to Users table
        public DateTime JoinDate { get; set; }        // Date of joining
        public DateTime SubscriptionEndDate { get; set; } // End of subscription

        // Navigation property (optional, not DB-mapped directly)
        public User User { get; set; }                // For displaying user info
    }
}
