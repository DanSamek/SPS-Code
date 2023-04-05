using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    public class UserCategory
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
        public UserCategory() { }

        public UserCategory(string name)  => Name = name;
        public static void AddDefaultCategories(CodeDbContext context)
        {
            if (context.UserCategories?.Count() > 0) return;

            context.UserCategories.Add(new("None"));
            context.UserCategories.Add(new("1. EP"));
            context.UserCategories.Add(new("2. EP"));
            context.UserCategories.Add(new("3. EP"));
            context.UserCategories.Add(new("4. EP"));
            context.UserCategories.Add(new("Absolvent"));

            context.SaveChanges();
        }
    }
}
