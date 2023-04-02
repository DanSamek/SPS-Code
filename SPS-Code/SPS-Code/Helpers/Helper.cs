using Microsoft.EntityFrameworkCore;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using System.Reflection;

namespace SPS_Code.Helpers
{
    static public class Helper
    {
        /// <summary>
        /// Název cookie pro přihlášené uživatele
        /// </summary>
        public static string UserCookie => "cookie";

        /// <summary>
        /// Klíč do tempDat kvůli úspěšnému přihlášení
        /// </summary>
        public static string LoginSuccessful => "loginSuccessful";

        /// <summary>
        /// Název cookie pro kontrolu zda je uživatel admin -> kvůli stylům
        /// </summary>
        public static string AdminCheck => "is-admin";

        /// <summary>
        /// Počet minut, po kterých se vygenerovaný vstup smaže
        /// </summary>
        public static int MinutesToDelete => 15;

        /// <summary>
        /// Pokud něco bude prázdné v objektu, vrátí null
        /// Vynechá to prvky, které budou mít <see cref="NotRequired"/> 
        /// </summary>
        /// <param name="obj">Object</param>
        public static bool CheckAllParams(object obj)
        {
            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(w => w.CanRead && w.CanWrite)
            .Where(w => w.GetGetMethod(true).IsPublic)
            .Where(w => w.GetSetMethod(true).IsPublic);
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes().ToList();
                if (attrs.Any(x => x.GetType().Name == "NotRequired")) continue;
                var propValue = (type.GetProperty(prop.Name).GetValue(obj, null) ?? string.Empty).ToString();
                if (string.IsNullOrEmpty(propValue)) return false;
            }
            return true;
        }

        public static bool GetLoggedUser(HttpContext context, out Guid? id)
        {
            var cookie = context.Session.GetString(UserCookie);
            if (cookie != null) id = Guid.Parse(cookie);
            else id = null;
            
            return id != null;
        }

        public static bool CheckIfAdmin(HttpContext context)
        {
            if (context.Session.GetInt32(AdminCheck) == 1) return true;
            else return false;
        }

        public static bool GetUser(HttpContext httpContext, CodeDbContext dbContext, out UserModel user, bool adminCheck = false)
        {
            user = null;
            if(!GetLoggedUser(httpContext, out Guid? id)) return false;
            user = dbContext.Users.Include(x => x.Tasks).ThenInclude(x => x.Task).FirstOrDefault(u => u.Id == id.Value.ToString());

            if (user == null) return false;
            if (!user.IsAdmin && adminCheck) return false;
            return true;
        }
    }
    public static class TmpFolder
    {
        /// <summary>
        /// Deletes all files in tmp older, then 15 minutes
        /// </summary>
        public static void DeleteOldFiles(object state)
        {
            if (!Directory.Exists("./Tmp")) return;

            var files = Directory.GetFiles("./Tmp");

            DateTime dateTime = DateTime.Now.AddMinutes(-Helper.MinutesToDelete);

            int fileDeleted = 0;
            foreach (var f in files)
            {
                var fct = System.IO.File.GetCreationTime(f);
                if (fct < dateTime)
                {
                    System.IO.File.Delete(f);
                    fileDeleted++;
                }
            }
            if (fileDeleted > 0) Console.WriteLine($"{DateTime.Now}: Tmp files deleted: {fileDeleted}");
            
        }
    }
}
