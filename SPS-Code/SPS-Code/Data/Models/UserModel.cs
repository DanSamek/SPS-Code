using SPS_Code.Controllers.RequestModels;
using SPS_Code.Helpers;
using System.ComponentModel.DataAnnotations;
using bcrypt = BCrypt.Net.BCrypt;

namespace SPS_Code.Data.Models
{
    public class UserModel 
    {
        [Required]
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public bool IsAdmin { get; set; } = false;

        public virtual List<TaskModel>? Tasks { get; set; } = new List<TaskModel>();
        public static string? CreateAndSaveToDb(RegisterRequest request, CodeDbContext context)
        {
            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";
            if (request.Password != request.PasswordCheck) return "Hesla se neshodují!";

            var anyUser = context.Users?.Any(x => x.Email == request.Email);
            if (anyUser != null && anyUser.Value) return "Zadaný email byl již použit!";

            if (!request.Email.Contains("spstrutnov")) return "Je potřeba použít školní email!";

            UserModel user = new()
            {
                IsAdmin = false,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,    
                Password = bcrypt.HashPassword(request.Password),
            };

            context.Users?.Add(user);
            context.SaveChanges();
            return null;
        }

        public static string? ValidateAndLogin(UserRequest request, CodeDbContext context, HttpContext httpcontext )
        {
            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";

            var user = context.Users?.FirstOrDefault(x => x.Email == request.Email);
            if (user == null) return "Tento email není zaregistrovaný :(";

            if (!bcrypt.Verify(request.Password, user.Password)) return "Špatné heslo!";

            httpcontext.Session.SetString(Helper.UserCookie, user.Id);
            return null;
        }
    }
}
