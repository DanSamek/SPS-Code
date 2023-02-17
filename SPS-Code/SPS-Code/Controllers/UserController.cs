using Microsoft.AspNetCore.Mvc;
using SPS_Code.Controllers.RequestModels;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using SPS_Code.Helpers;
using bcrypt = BCrypt.Net.BCrypt;

namespace SPS_Code.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private CodeDbContext _context;

        public UserController(CodeDbContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Route("login")]
        public ActionResult Login()
        {
            if (HttpContext.Session.GetInt32(Helper.UserCookie) != null) Redirect("/");
            return View(new UserRequest());
        }

        [Route("register")]
        public ActionResult Register()
        {
            if (HttpContext.Session.GetInt32(Helper.UserCookie) != null) Redirect("/");
            return View(new RegisterRequest());
        }

        [HttpPost]
        [Route("register")]
        public ActionResult RegisterPost([FromForm] RegisterRequest registerRequest)
        {
            if (!Helper.CheckAllParams(registerRequest)) return View("Register", registerRequest.SetError("Něco nebylo vyplněno!"));

            if(registerRequest.Password != registerRequest.PasswordCheck) return View("Register", registerRequest.SetError("Hesla se neshodují!"));

            var anyUser = _context.Users?.Any(x => x.Email == registerRequest.Email);

            if (anyUser != null && anyUser.Value) return View("Register", registerRequest.SetError("Zadaný email byl již použit!"));

            if (!registerRequest.Email.Contains("spstrutnov")) return View("Register", registerRequest.SetError("Je potřeba použít školní email!"));

            User newUser = new()
            {
                IsAdmin = false,
                Email = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Password = bcrypt.HashPassword(registerRequest.Password),
            };

            _context.Users?.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("login")]
        public ActionResult LoginPost([FromForm] UserRequest userRequest)
        {
            if (!Helper.CheckAllParams(userRequest)) return View("Login", userRequest.SetError("Něco nebylo vyplněno!"));

            var user = _context.Users?.FirstOrDefault(x => x.Email == userRequest.Email);
            if (user == null) return View("Login", userRequest.SetError("Tento email není zaregistrovaný :("));

            if (!bcrypt.Verify(userRequest.Password, user.Password)) return View("Login", userRequest.SetError("Špatné heslo!"));

            HttpContext.Session.SetInt32(Helper.UserCookie, user.Id);
            TempData[Helper.LoginSuccessful] = true;
            return Redirect("/");
        }

        [Route("logout")]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }
    }
}
 