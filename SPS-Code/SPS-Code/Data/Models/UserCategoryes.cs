using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    public class UserCategory
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        public UserCategory() { }

        public UserCategory(string name, string? description = null) 
        { 
            Name = name;
            Description = description;
        }

        public void AddDefaultCategoryes(CodeDbContext context)
        {
            if (context.UserCategoryes.Count() > 0) return;

            context.UserCategoryes.Add(new("None", "Žák, nebo registrovaný uživatel, který nemá přiřazenou třídu."));
            context.UserCategoryes.Add(new("1. EP"));
            context.UserCategoryes.Add(new("2. EP"));
            context.UserCategoryes.Add(new("3. EP"));
            context.UserCategoryes.Add(new("4. EP"));
            context.UserCategoryes.Add(new("Absolvent", "Žák, který úspěšně odmaturoval."));

            context.SaveChanges();
        }
    }
}
