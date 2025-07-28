namespace GYMApp.Models
{
    public class Workout
    {
        public int WorkoutId { get; set; }         // Primary key
        public int MemberId { get; set; }          // Linked to Members table
        public string Description { get; set; }    // Workout routine details
        public DateTime ScheduleDate { get; set; } // When to do the workout

        // Optional navigation property
        public Member Member { get; set; }
    }
}
