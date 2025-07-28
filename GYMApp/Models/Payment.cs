namespace GYMApp.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }         // Primary key
        public int MemberId { get; set; }          // Linked to Members table
        public decimal Amount { get; set; }        // Amount paid
        public DateTime PaymentDate { get; set; }  // Date of payment
        public string Status { get; set; }         // "Paid", "Pending", etc.

        // Optional navigation property
        public Member Member { get; set; }
    }
}
