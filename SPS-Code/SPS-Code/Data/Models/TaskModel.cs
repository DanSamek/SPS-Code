using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        // In MD
        [Required]
        public string Description { get; set; }

        [Required]
        public int MaxPoints { get; set; }

        [Required]
        public int MaxSubmitTimeMinutes { get; set; }

        [Required]
        public int TestCount { get; set; }

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
                Description = request.Description,
                MaxSubmitTimeMinutes = request.MaxSubmitTimeMinutes,
                TestCount = request.TestCount
            };

            // Save validator files and generator files
            if (!Directory.Exists("./Tasks")) Directory.CreateDirectory("./Tasks");

            Directory.CreateDirectory($"./Tasks/{task.Name}");

            using var generatorFileStream = File.Create($"./Tasks/{task.Name}/generator.exe");
            request.Generator.CopyTo(generatorFileStream);

            using var validatorFileStream = File.Create($"./Tasks/{task.Name}/validator.exe");
            request.Validator.CopyTo(validatorFileStream);

            context.Tasks?.Add(task);
            context.SaveChanges();
            taskId = task.Id.ToString();
            return null;
        }

        /// <summary>
        /// Generate input file for requested task
        /// </summary>
        public static string Generate(TaskModel? task)
        {
            string path = string.Empty;
            var baseDir = Directory.GetCurrentDirectory();
            var proc = System.Diagnostics.Process.Start($@"{baseDir}\Tasks\{task?.Name}\generator.exe", DateTime.Now.Ticks.ToString());
            proc.StartInfo.RedirectStandardOutput = true;
            if (proc.Start())
            {                 
                string data = proc.StandardOutput.ReadToEnd();
                if (!Directory.Exists($@"{baseDir}\Tmp\")) Directory.CreateDirectory($@"{baseDir}\Tmp");
                path = $@"{baseDir}\Tmp\{Guid.NewGuid().ToString()}.txt";
                using var fileStream = new FileStream(path, FileMode.Create);
                using var writer = new StreamWriter(fileStream);
                writer.WriteLine(data);
                proc.WaitForExit();
            }

            return path;
        }

        /// <summary>
        /// Validation of user input, when mistake is done by user output, it will calculate total points for user
        /// </summary>
        public static int Validate(IFormFile userOutput, TaskModel task, string generatedFileInput)
        {
            using var stream = userOutput.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            var userDatai = streamReader.ReadToEnd();

            // Run validation script
            var baseDir = Directory.GetCurrentDirectory();
            var proc = System.Diagnostics.Process.Start($@"{baseDir}\Tasks\{task?.Name}\validator.exe", generatedFileInput);
            proc.StartInfo.RedirectStandardOutput = true;

            int tRight = 0;

            if (proc.Start())
            {
                string dataI = proc.StandardOutput.ReadToEnd();
                var data = dataI.Split("\n");
                var userData = userDatai.Split("\n");

                // Validate every line, if wrong value, break
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == userData[i]) tRight++;
                    else break;
                }
            }
            return task.TestCount/tRight;
        }
    }
}
