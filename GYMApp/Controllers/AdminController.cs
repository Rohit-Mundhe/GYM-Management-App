using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GymManagementApp.Models;
using System.Collections.Generic;
using System.Data;
using GymManagementApp.Filters;

namespace GymManagementApp.Controllers
{

    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly DBHelper _db;

        public AdminController(DBHelper db)
        {
            _db = db;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalMembers = _db.ExecuteQuery("SELECT COUNT(*) FROM Members");
            ViewBag.TotalTrainers = _db.ExecuteQuery("SELECT COUNT(*) FROM Trainers");
            return View();
        }

        public IActionResult Members()
        {
            var dt = _db.ExecuteQuery(@"SELECT m.MemberId, u.Name, u.Email, m.JoinDate, m.SubscriptionEndDate 
                                        FROM Members m 
                                        JOIN Users u ON m.UserId=u.UserId");
            return View(dt);
        }
        public IActionResult Trainers()
        {
            var dt = _db.ExecuteQuery(@"
                SELECT t.TrainerId, u.Name, u.Email, t.Specialty
                FROM Trainers t
                JOIN Users u ON t.UserId = u.UserId");
            return View(dt);
        }

        [HttpGet]
        public IActionResult AddTrainer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddTrainer(string name, string email, string specialty)
        {
            var userId = _db.ExecuteScalar("SELECT UserId FROM Users WHERE Email=@Email",
                        new SqlParameter("@Email", email));

            if (userId == null)
            {
                string hashed = "Trainer@123";
                _db.ExecuteNonQuery(@"
                    INSERT INTO Users(Name, Email, PasswordHash, Role) 
                    VALUES(@Name, @Email, @Pass, 'Trainer')",
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Pass", hashed));

                userId = _db.ExecuteScalar("SELECT SCOPE_IDENTITY()");
            }

            var exists = _db.ExecuteScalar("SELECT COUNT(*) FROM Trainers WHERE UserId=@Uid",
                        new SqlParameter("@Uid", userId));
            if (Convert.ToInt32(exists) > 0)
            {
                TempData["Error"] = "This user is already a trainer.";
                return RedirectToAction("Trainers");
            }

            _db.ExecuteNonQuery(@"
                INSERT INTO Trainers(UserId, Specialty) VALUES(@Uid,@Spec)",
                new SqlParameter("@Uid", userId),
                new SqlParameter("@Spec", specialty));

            TempData["Message"] = "Trainer added successfully!";
            return RedirectToAction("Trainers");
        }

        [HttpGet]
        public IActionResult EditTrainer(int id)
        {
            var dt = _db.ExecuteQuery(@"
                SELECT t.TrainerId, u.Name, u.Email, t.Specialty
                FROM Trainers t
                JOIN Users u ON t.UserId = u.UserId
                WHERE t.TrainerId=@Id", new SqlParameter("@Id", id));

            if (dt.Rows.Count == 0) return RedirectToAction("Trainers");
            return View(dt.Rows[0]);
        }

        [HttpPost]
        public IActionResult EditTrainer(int id, string name, string email, string specialty)
        {
            var userId = _db.ExecuteScalar("SELECT UserId FROM Trainers WHERE TrainerId=@Tid",
                         new SqlParameter("@Tid", id));

            _db.ExecuteNonQuery(@"
                UPDATE Users SET Name=@Name, Email=@Email WHERE UserId=@Uid",
                new SqlParameter("@Name", name),
                new SqlParameter("@Email", email),
                new SqlParameter("@Uid", userId));

            _db.ExecuteNonQuery(@"
                UPDATE Trainers SET Specialty=@Spec WHERE TrainerId=@Tid",
                new SqlParameter("@Spec", specialty),
                new SqlParameter("@Tid", id));

            TempData["Message"] = "Trainer updated successfully!";
            return RedirectToAction("Trainers");
        }

        [HttpGet]
        public IActionResult DeleteTrainer(int id)
        {
            _db.ExecuteNonQuery("DELETE FROM Trainers WHERE TrainerId=@Tid", new SqlParameter("@Tid", id));
            TempData["Message"] = "Trainer deleted!";
            return RedirectToAction("Trainers");
        }

        public IActionResult Payments()
        {
            var dt = _db.ExecuteQuery(@"
        SELECT p.PaymentId, u.Name AS MemberName, p.Amount, p.PaymentDate, p.Status 
        FROM Payments p
        JOIN Members m ON p.MemberId = m.MemberId
        JOIN Users u ON m.UserId = u.UserId
        ORDER BY p.PaymentDate DESC");
            return View(dt);
        }

        // GET: Add a payment manually
        [HttpGet]
        public IActionResult AddPayment()
        {
            // Get members list for dropdown
            var members = _db.ExecuteQuery(@"
        SELECT m.MemberId, u.Name FROM Members m
        JOIN Users u ON m.UserId = u.UserId");
            ViewBag.Members = members;
            return View();
        }

        // POST: Save manual payment
        [HttpPost]
        public IActionResult AddPayment(int memberId, decimal amount, string status)
        {
            _db.ExecuteNonQuery(@"
        INSERT INTO Payments(MemberId, Amount, PaymentDate, Status)
        VALUES(@Mid, @Amt, GETDATE(), @Status)",
                new SqlParameter("@Mid", memberId),
                new SqlParameter("@Amt", amount),
                new SqlParameter("@Status", status));

            TempData["Message"] = "Payment recorded successfully!";
            return RedirectToAction("Payments");
        }

        // GET: Update payment status only
        [HttpGet]
        public IActionResult UpdatePaymentStatus(int id)
        {
            var dt = _db.ExecuteQuery(@"
        SELECT p.PaymentId, u.Name AS MemberName, p.Amount, p.PaymentDate, p.Status
        FROM Payments p
        JOIN Members m ON p.MemberId = m.MemberId
        JOIN Users u ON m.UserId = u.UserId
        WHERE p.PaymentId=@Pid",
                new SqlParameter("@Pid", id));

            if (dt.Rows.Count == 0) return RedirectToAction("Payments");
            return View(dt.Rows[0]);
        }

        // POST: Save updated status
        [HttpPost]
        public IActionResult UpdatePaymentStatus(int id, string status)
        {
            _db.ExecuteNonQuery(@"
        UPDATE Payments SET Status=@Status WHERE PaymentId=@Pid",
                new SqlParameter("@Status", status),
                new SqlParameter("@Pid", id));

            TempData["Message"] = "Payment status updated!";
            return RedirectToAction("Payments");
        }
        public IActionResult AddMember()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddMember(string name, string email, DateTime joinDate, DateTime subscriptionEndDate)
        {
            // Check if user exists
            var userId = _db.ExecuteScalar("SELECT UserId FROM Users WHERE Email=@Email",
                        new SqlParameter("@Email", email));

            if (userId == null) // Create new user
            {
                string hashed = "Member@123";
                _db.ExecuteNonQuery(@"
                    INSERT INTO Users(Name, Email, PasswordHash, Role) 
                    VALUES(@Name, @Email, @Pass, 'Member')",
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Pass", hashed));

                userId = _db.ExecuteScalar("SELECT SCOPE_IDENTITY()");
            }

            // Check if already a member
            var exists = _db.ExecuteScalar("SELECT COUNT(*) FROM Members WHERE UserId=@Uid",
                        new SqlParameter("@Uid", userId));
            if (Convert.ToInt32(exists) > 0)
            {
                TempData["Error"] = "This user is already a member.";
                return RedirectToAction("Members");
            }

            // Insert Member record
            _db.ExecuteNonQuery(@"
                INSERT INTO Members(UserId, JoinDate, SubscriptionEndDate)
                VALUES(@Uid, @Join, @End)",
                new SqlParameter("@Uid", userId),
                new SqlParameter("@Join", joinDate),
                new SqlParameter("@End", subscriptionEndDate));

            TempData["Message"] = "Member added successfully!";
            return RedirectToAction("Members");
        }

        [HttpGet]
        public IActionResult EditMember(int id)
        {
            var dt = _db.ExecuteQuery(@"
                SELECT m.MemberId, u.Name, u.Email, m.JoinDate, m.SubscriptionEndDate
                FROM Members m
                JOIN Users u ON m.UserId = u.UserId
                WHERE m.MemberId=@Id", new SqlParameter("@Id", id));

            if (dt.Rows.Count == 0) return RedirectToAction("Members");
            return View(dt.Rows[0]);
        }

        [HttpPost]
        public IActionResult EditMember(int id, string name, string email, DateTime joinDate, DateTime subscriptionEndDate)
        {
            var userId = _db.ExecuteScalar("SELECT UserId FROM Members WHERE MemberId=@Mid",
                         new SqlParameter("@Mid", id));

            _db.ExecuteNonQuery(@"
                UPDATE Users SET Name=@Name, Email=@Email WHERE UserId=@Uid",
                new SqlParameter("@Name", name),
                new SqlParameter("@Email", email),
                new SqlParameter("@Uid", userId));

            _db.ExecuteNonQuery(@"
                UPDATE Members SET JoinDate=@Join, SubscriptionEndDate=@End WHERE MemberId=@Mid",
                new SqlParameter("@Join", joinDate),
                new SqlParameter("@End", subscriptionEndDate),
                new SqlParameter("@Mid", id));

            TempData["Message"] = "Member updated successfully!";
            return RedirectToAction("Members");
        }

        [HttpGet]
        public IActionResult DeleteMember(int id)
        {
            // Delete member record only (keep user for history)
            _db.ExecuteNonQuery("DELETE FROM Members WHERE MemberId=@Mid", new SqlParameter("@Mid", id));
            TempData["Message"] = "Member deleted!";
            return RedirectToAction("Members");
        }

    }
}
