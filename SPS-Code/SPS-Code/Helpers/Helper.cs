using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SPS_Code.Data.Models;

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
            if (context.Session.GetString(UserCookie) != null)
                id = Guid.Parse(context.Session.GetString(UserCookie));
            else
                id = null;

            return id != null;
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
