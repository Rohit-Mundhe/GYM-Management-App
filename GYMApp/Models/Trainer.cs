namespace GYMApp.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }         // Primary key
        public int UserId { get; set; }            // Linked to Users table
        public string Specialization { get; set; } // e.g., Weight Training, Cardio

        // Optional navigation property
        public User User { get; set; }
    }
}
