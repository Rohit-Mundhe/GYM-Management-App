namespace GYMApp.Models
{
    public class MemberDashboardViewModel
    {
        public Member Member { get; set; }
        public List<Workout> Workouts { get; set; }
        public List<DietPlan> DietPlans { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
