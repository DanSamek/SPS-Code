using Microsoft.AspNetCore.Mvc;
using SPS_Code.Controllers.RequestModels;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using SPS_Code.Helpers;

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
            if (!Helper.GetUser(HttpContext,_context, out var user)) { return Redirect("/"); }

            return View(user);
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
            if (errorMessage != null) { TempData[Helper.ErrorToken] = errorMessage; return View("Register", registerRequest.SetError(errorMessage)); }
            TempData[Helper.SuccessToken] = "Registrace proběhla úspěšně!";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("login")]
        public ActionResult LoginPost([FromForm] UserRequest userRequest)
        {
            var errorMessage = UserModel.ValidateAndLogin(userRequest, _context, HttpContext);
            if (errorMessage != null) { TempData[Helper.ErrorToken] = errorMessage; return View("Login", userRequest.SetError(errorMessage)); }
            TempData[Helper.SuccessToken] = "Přihlášení proběhlo úspěšně!";
            return Redirect("/");
        }

        [Route("logout")]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        [HttpPost]
        [Route("edit")]
        public ActionResult EditUser([FromForm] UserEditRequest editRequest)
        {
            if (!Helper.GetUser(HttpContext, _context, out var user)) { return Redirect("/"); }

            var errorMessage = UserModel.ValidateAndEdit(user, editRequest, _context);
            if (errorMessage != null) { TempData[Helper.ErrorToken] = errorMessage; return Redirect("/user"); }

            TempData[Helper.SuccessToken] = "Změna údajů proběhla vpořádku!";
            return Redirect("/user");
        }

        [HttpPost]
        [Route("chpasswd")]
        public ActionResult ChangePassword([FromForm] UserPasswordRequest editRequest)
        {
            if (!Helper.GetUser(HttpContext, _context, out var user)) { return Redirect("/"); }

            var errorMessage = UserModel.ValidateAndChangePassword(user, editRequest, _context);
            if (errorMessage != null) { TempData[Helper.ErrorToken] = errorMessage; return Redirect("/user"); }

            TempData[Helper.SuccessToken] = "Změna hesla proběhla vpořádku!";
            return Redirect("/user");
        }
    }
}
 