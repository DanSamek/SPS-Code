using SPS_Code.Controllers.RequestModels;
using SPS_Code.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SPS_Code.Data.Models
{
    /// <summary>
    /// Base task class
    /// </summary>
    public class TaskModel
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        
        [Required]
        public int MaxPoints { get; set; }

        public DateTime Created = DateTime.Now;


        /// <summary>
        /// Task create
        /// </summary>
        public static string? CreateAndSaveToDb(TaskCreateRequest request, CodeDbContext context, out string taskId)
        {
            taskId = null;
            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";

            var genExt = Path.GetExtension(request.Generator.FileName);
            var valExt = Path.GetExtension(request.Validator.FileName);

            if (genExt != ".exe" || valExt != ".exe") return "Je potřeba, aby generátor a validátor byl .exe soubor!";

            TaskModel task = new()
            {
                Name = request.Name,
                MaxPoints = request.MaxPoints,
            };

            // Save validator files and generator files
            if (!Directory.Exists("./Tasks")) Directory.CreateDirectory("./Tasks");

            if (!Directory.Exists("./wwwroot/Tasks")) Directory.CreateDirectory("./wwwroot/Tasks");

            Directory.CreateDirectory($"./Tasks/{task.Name}");
            Directory.CreateDirectory($"./wwwroot/Tasks/{task.Name}");

            using var generatorFileStream = File.Create($"./Tasks/{task.Name}/genetator.exe");
            request.Generator.CopyTo(generatorFileStream);

            using var validatorFileStream = File.Create($"./Tasks/{task.Name}/validator.exe");
            request.Validator.CopyTo(validatorFileStream);

            using var descriptionFileStream = File.Create($"./wwwroot/Tasks/{task.Name}/{request.Description.FileName}");
            request.Description.CopyTo(descriptionFileStream);

            context.Tasks?.Add(task);
            context.SaveChanges();
            taskId = task.Id.ToString();
            return null;
        }

        /// <summary>
        /// Generate input file for requested task
        /// </summary>
        public static void Generate(TaskModel? task)
        {
            var proc = System.Diagnostics.Process.Start($"./Tasks/{task?.Name}/generator.exe", DateTime.Now.Ticks.ToString());
            proc.Start();
            var data = proc.StandardOutput.ReadToEnd();
            Console.WriteLine(data);
            proc.WaitForExit();
        }
    }
}
