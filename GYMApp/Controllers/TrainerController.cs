using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GymManagementApp.Models;
using System.Data;
using GYMApp.Models;

namespace GymManagementApp.Controllers
{
    public class TrainerController : Controller
    {
        private readonly DBHelper _db;

        public TrainerController(DBHelper db)
        {
            _db = db;
        }

        public IActionResult Dashboard()
        {
            // List all members assigned to this trainer (simplified)
            var dt = _db.ExecuteQuery(@"SELECT m.MemberId, u.Name, u.Email 
                                        FROM Members m 
                                        JOIN Users u ON m.UserId=u.UserId");
            return View(dt);
        }

        [HttpGet]
        public IActionResult AssignWorkout(int memberId) => View(new Workout { MemberId = memberId });

        [HttpPost]
        public IActionResult AssignWorkout(Workout model)
        {
            string query = "INSERT INTO Workouts (MemberId, Description, ScheduleDate) VALUES (@MemberId,@Desc,@Date)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@MemberId", model.MemberId),
                new SqlParameter("@Desc", model.Description),
                new SqlParameter("@Date", model.ScheduleDate)
            };

            _db.ExecuteNonQuery(query, parameters);
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult AssignDiet(int memberId) => View(new DietPlan { MemberId = memberId });

        [HttpPost]
        public IActionResult AssignDiet(DietPlan model)
        {
            string query = "INSERT INTO DietPlans (MemberId, DietDetails, StartDate, EndDate) VALUES (@MemberId,@Diet,@Start,@End)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@MemberId", model.MemberId),
                new SqlParameter("@Diet", model.DietDetails),
                new SqlParameter("@Start", model.StartDate),
                new SqlParameter("@End", model.EndDate)
            };

            _db.ExecuteNonQuery(query, parameters);
            return RedirectToAction("Dashboard");
        }
    }
}
