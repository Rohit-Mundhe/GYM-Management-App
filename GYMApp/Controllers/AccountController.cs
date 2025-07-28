using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GymManagementApp.Models;
using GymManagementApp.Filters;

namespace GymManagementApp.Controllers
{

    public class AccountController : Controller
    {
        private readonly DBHelper _db;

        public AccountController(DBHelper db)
        {
            _db = db;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string name, string email, string password, string role)
        {

            int result;
            var exists = _db.ExecuteQuery(
    "SELECT COUNT(*) AS Cnt FROM Users WHERE Email=@E",
    new SqlParameter("@E", email));

            if (Convert.ToInt32(exists.Rows[0]["Cnt"]) > 0)
            {
                ViewBag.Error = "Email already registered!";
                return View(); // Or redirect with error
            }
            else
            {
               result = _db.ExecuteNonQuery(@"
        INSERT INTO Users (Name, Email, Password, Role)
        VALUES (@N, @E, @P, @R)",
                    new SqlParameter("@N", name),
                    new SqlParameter("@E", email),
                    new SqlParameter("@P", password),
                    new SqlParameter("@R", role));
            }

            if (result > 0)
                return RedirectToAction("Login");
            ViewBag.Error = "Registration failed.";
            return View();
        }

        public IActionResult Login() => View();

        //[HttpPost]
        //public IActionResult Login(string email, string password)
        //{
        //    string query = "SELECT * FROM Users WHERE Email=@Email";
        //    var dt = _db.ExecuteQuery(query, new SqlParameter("@Email", email));

        //    if (dt.Rows.Count > 0)
        //    {
        //        string hash = dt.Rows[0]["PasswordHash"].ToString();
        //        if (string.Equals(password, hash))
        //        {
        //            string role = dt.Rows[0]["Role"].ToString();
        //            HttpContext.Session.SetString("UserId", dt.Rows[0]["UserId"].ToString());
        //            HttpContext.Session.SetString("Role", role);

        //            return role switch
        //            {
        //                "Admin" => RedirectToAction("Dashboard", "Admin"),
        //                "Trainer" => RedirectToAction("Dashboard", "Trainer"),
        //                _ => RedirectToAction("Dashboard", "Member")
        //            };
        //        }
        //    }

        //    ViewBag.Error = "Invalid credentials.";
        //    return View();
        //}
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var dt = _db.ExecuteQuery(@"
                SELECT UserId, Role
                FROM Users
                WHERE Email = @E AND PasswordHash = @P",
                new SqlParameter("@E", email),
                new SqlParameter("@P", password));

            if (dt.Rows.Count == 0)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            int userId = Convert.ToInt32(dt.Rows[0]["UserId"]);
            string role = dt.Rows[0]["Role"].ToString();

            // Set session
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("Role", role);

            // If Member, fetch MemberId
            if (role == "Member")
            {
                var memberDt = _db.ExecuteQuery(@"
                    SELECT MemberId FROM Members WHERE UserId = @U",
                    new SqlParameter("@U", userId));

                if (memberDt.Rows.Count > 0)
                {
                    HttpContext.Session.SetInt32("MemberId", Convert.ToInt32(memberDt.Rows[0]["MemberId"]));
                }

                return RedirectToAction("Dashboard", "Member");
            }
            else if (role == "Trainer")
            {
                return RedirectToAction("Dashboard", "Trainer");
            }
            else if (role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Login");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult Unauthorized()
        {
            return View();
        }
    }
}
