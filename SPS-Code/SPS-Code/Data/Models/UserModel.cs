using Azure.Core;
using Microsoft.IdentityModel.Tokens;
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
        public string Id { get; set; }

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
        
        [Required]
        public virtual List<UserTaskResult>? Tasks { get; set; } = new();


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
                Id = Guid.NewGuid().ToString()
            };

            context.Users?.Add(user);
            context.SaveChanges();
            return null;
        }

        public static string? ValidateAndLogin(UserRequest request, CodeDbContext context, HttpContext httpcontext)
        {
            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";

            var user = context.Users?.FirstOrDefault(x => x.Email == request.Email);
            if (user == null) return "Tento email není zaregistrovaný :(";

            if (!bcrypt.Verify(request.Password, user.Password)) return "Špatné heslo!";

            httpcontext.Session.SetString(Helper.UserCookie, user.Id);

            if (user.IsAdmin)
                httpcontext.Session.SetInt32(Helper.AdminCheck, 1);

            return null;
        }

        public static string? ValidateAndEdit(UserModel user, UserEditRequest req, CodeDbContext context)
        {
            if (req.FirstName.IsNullOrEmpty() || req.LastName.IsNullOrEmpty()) return "Něco nebylo vyplněno!";
            
            user.FirstName = req.FirstName;
            user.LastName = req.LastName;

            context.SaveChanges();

            return null;
        }

        public static string? ValidateAndChangePassword(UserModel user, UserPasswordRequest req, CodeDbContext context)
        {
            if (req.Password.IsNullOrEmpty() || req.NewPassword.IsNullOrEmpty() || req.NewPasswordCheck.IsNullOrEmpty()) return "Něco nebylo vyplněno!";

            if (!bcrypt.Verify(req.Password, user.Password)) return "Špatné heslo!";

            if (req.NewPassword != req.NewPasswordCheck) return "Nová hesla se neshodují!";

            user.Password = bcrypt.HashPassword(req.NewPassword);

            context.SaveChanges();

            return null;
        }
    }
}
