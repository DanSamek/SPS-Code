using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    public class UserTaskResult
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public TaskModel Task { get; set; }

        [Required]
        public UserModel User { get; set; }
    
        [Required]
        public int MaxPointsObtained { get; set; }

        [Required]
        public int AttemptsCount { get; set; }

        public DateTime LastAttemptTime { get; set; }
    }
}
