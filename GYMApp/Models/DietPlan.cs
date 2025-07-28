namespace GYMApp.Models
{
    public class DietPlan
    {
        public int DietId { get; set; }            // Primary key
        public int MemberId { get; set; }          // Linked to Members table
        public string DietDetails { get; set; }    // Diet info
        public DateTime StartDate { get; set; }    // Plan start
        public DateTime EndDate { get; set; }      // Plan end

        // Optional navigation property
        public Member Member { get; set; }
    }
}
