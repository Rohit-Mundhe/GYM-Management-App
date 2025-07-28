using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GymManagementApp.Models;
using System.Collections.Generic;
using System.Data;
using GYMApp.Models;
using GymManagementApp.Filters;

namespace GymManagementApp.Controllers
{
    [AuthorizeRole("Member")]
    public class MemberController : Controller
    {
        private readonly DBHelper _db;

        public MemberController(DBHelper db)
        {
            _db = db;
        }

        public IActionResult Dashboard()
        {
            int? memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null || memberId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var ds = new DataSet();

            // Member info
            ds.Tables.Add(_db.ExecuteQuery(@"
        SELECT u.Name
        FROM Members m
        JOIN Users u ON m.UserId = u.UserId
        WHERE m.MemberId = @Mid",
                new SqlParameter("@Mid", memberId)));

            // Workouts
            ds.Tables.Add(_db.ExecuteQuery(@"
        SELECT ScheduleDate, Description
        FROM Workouts
        WHERE MemberId=@Mid
        ORDER BY ScheduleDate",
                new SqlParameter("@Mid", memberId)));

            // Diet Plans
            ds.Tables.Add(_db.ExecuteQuery(@"
        SELECT StartDate, EndDate, DietDetails
        FROM DietPlans
        WHERE MemberId=@Mid
        ORDER BY StartDate",
                new SqlParameter("@Mid", memberId)));

            // Attendance
            ds.Tables.Add(_db.ExecuteQuery(@"
        SELECT AttendanceDate, Status
        FROM Attendance
        WHERE MemberId=@Mid
        ORDER BY AttendanceDate",
                new SqlParameter("@Mid", memberId)));

            return View(ds);
        }

    }
}
