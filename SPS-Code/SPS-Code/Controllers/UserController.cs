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
            if (HttpContext.Session.GetString(Helper.UserCookie) != null) Redirect("/");
            return View(new UserRequest());
        }

        [Route("register")]
        public ActionResult Register()
        {
            if (HttpContext.Session.GetString(Helper.UserCookie) != null) Redirect("/");
            return View(new RegisterRequest());
        }

        [HttpPost]
        [Route("register")]
        public ActionResult RegisterPost([FromForm] RegisterRequest registerRequest)
        {
            var errorMessage = UserModel.CreateAndSaveToDb(registerRequest, _context);
            if (errorMessage != null) return View("Register", registerRequest.SetError(errorMessage));
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("login")]
        public ActionResult LoginPost([FromForm] UserRequest userRequest)
        {
            var errorMessage = UserModel.ValidateAndLogin(userRequest, _context, HttpContext);
            if (errorMessage != null) return View("Login", userRequest.SetError(errorMessage));
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
 