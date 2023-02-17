using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    public class Task
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public int MaxPoints { get; set; }

        [Required]
        public int? UserPoint { get; set; }

        public DateTime Created = DateTime.Now;

        public DateTime ToClose { get; set; } = DateTime.Now.AddYears(9999);

        public virtual List<User>? User { get; set; } = new List<User>();
    }
}
