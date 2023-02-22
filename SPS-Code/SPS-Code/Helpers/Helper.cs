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


        public static bool GetLoggedUser(HttpContext context, out int? id)
        {
            id = context.Session.GetInt32(UserCookie);
            return id != null;
        }
    }
}
