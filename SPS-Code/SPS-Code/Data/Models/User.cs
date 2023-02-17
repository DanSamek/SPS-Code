using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    public class User 
    {
        [Required]
        [Key]
        public int Id { get; set; }

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

        public virtual List<Task>? Tasks { get; set; } = new List<Task>();
    }
}
