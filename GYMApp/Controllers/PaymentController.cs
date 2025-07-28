using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GymManagementApp.Models;
using System.Data;
using GymManagementApp.Filters;

namespace GymManagementApp.Controllers
{
    [AuthorizeRole("Member")]
    public class PaymentController : Controller
    {
        private readonly DBHelper _db;

        public PaymentController(DBHelper db)
        {
            _db = db;
        }

        public IActionResult History(int memberId)
        {
            var dt = _db.ExecuteQuery(@"SELECT PaymentId, Amount, PaymentDate, Status 
                                        FROM Payments WHERE MemberId=@Id",
                                        new SqlParameter("@Id", memberId));
            return View(dt);
        }

        [HttpPost]
        public IActionResult AddPayment(int memberId, decimal amount)
        {
            string query = "INSERT INTO Payments (MemberId, Amount, PaymentDate, Status) VALUES (@Id,@Amt,GETDATE(),'Paid')";
            _db.ExecuteNonQuery(query, new SqlParameter("@Id", memberId), new SqlParameter("@Amt", amount));
            return RedirectToAction("Members", "Admin");
        }
    }
}
