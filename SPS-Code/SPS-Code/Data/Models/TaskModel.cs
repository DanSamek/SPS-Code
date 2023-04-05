using Markdig.Extensions.TaskLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SPS_Code.Controllers;
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
        public string Inputs { get; set; }

        [Required]
        public string Outputs { get; set; }

        [Required]
        public int MaxPoints { get; set; }

        [Required]
        public int MaxSubmitTimeMinutes { get; set; }

        [Required]
        public int TestCount { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public bool Visible { get; set; } = true;

        [Required]
        public List<UserCategory>? ViewUserCategories { get; set; }

        /// <summary>
        /// Task create
        /// </summary>
        public static string? CreateAndSaveToDb(TaskCreateRequest request, CodeDbContext context, out string taskId)
        {
            taskId = null;
            if (request.CategoryIDs == null) request.CategoryIDs = new List<int> { 0 };
            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";

            var genExt = Path.GetExtension(request.Generator.FileName);
            var valExt = Path.GetExtension(request.Validator.FileName);

            if (genExt != ".exe" || valExt != ".exe") return "Je potřeba, aby generátor a validátor byl .exe soubor!";

            TaskModel task = new()
            {
                Name = request.Name,
                MaxPoints = request.MaxPoints,
                Description = request.Description,
                Inputs = request.Inputs,
                Outputs = request.Outputs,
                MaxSubmitTimeMinutes = request.MaxSubmitTimeMinutes,
                TestCount = request.TestCount,
                Created = DateTime.Now,
                ViewUserCategories = context.UserCategories.Where(c => request.CategoryIDs.Contains(c.ID)).ToList()
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
            var proc = System.Diagnostics.Process.Start($@"{baseDir}\Tasks\{task?.Name}\generator.exe");
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
        public static int Validate(IFormFile userOutput, TaskModel task, string generatedFilePath)
        {
            if (userOutput == null) return 0;

            using var stream = userOutput.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            var userDatai = streamReader.ReadToEnd();

            using var streamReader2 = new StreamReader(generatedFilePath);
            var inputData = streamReader2.ReadToEnd();

            // Run validation script
            var baseDir = Directory.GetCurrentDirectory();
            var proc = System.Diagnostics.Process.Start($@"{baseDir}\Tasks\{task?.Name}\validator.exe");
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;

            int tRight = 0;

            if (proc.Start())
            {
                proc.StandardInput.WriteLine(inputData);
                string dataI = proc.StandardOutput.ReadToEnd();
                var data = dataI.Split("\n");
                var userData = userDatai.Split("\n");

                // Validate every line, if wrong value, break
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == userData[i]) tRight++;
                    else break;

                    // For tests
                    if (tRight == task.TestCount) break;
                }
            }
            return (int)((double)tRight/task.TestCount * task.MaxPoints);
        }

        public static string? Edit(TaskEditRequest request, CodeDbContext context, int taskId)
        {
            var task = GetTaskModel(context, taskId);

            if (!Helper.CheckAllParams(request)) return "Něco nebylo vyplněno!";
            if(task == null) return "Taková úloha neexistuje!";

            task.Name =  request.Name;
            task.MaxPoints = request.MaxPoints;
            task.Description = request.Description;
            task.Inputs = request.Inputs;    
            task.Outputs = request.Outputs;
            task.MaxSubmitTimeMinutes = request.MaxSubmitTimeMinutes;
            task.TestCount = request.TestCount;
            task.ViewUserCategories.Clear();
            task.ViewUserCategories = context.UserCategories.Where(c => request.CategoryIDs.Contains(c.ID)).ToList();

            if (!Directory.Exists($"./Tasks/{task.Name}")) Directory.CreateDirectory($"./Tasks/{task.Name}");
            if (request.Generator != null)
            {
                string genExt = Path.GetExtension(request.Generator.FileName);
                if (genExt != ".exe") return "Je potřeba, aby generátor byl .exe soubor!";

                using var generatorFileStream = File.Create($"./Tasks/{task.Name}/generator.exe");
                request.Generator.CopyTo(generatorFileStream);
            }
            if(request.Validator != null)
            {
                string valExt = Path.GetExtension(request.Validator.FileName);
                if (valExt != ".exe") return "Je potřeba, aby validátor byl .exe soubor!";

                using var validatorFileStream = File.Create($"./Tasks/{task.Name}/validator.exe");
                request.Validator.CopyTo(validatorFileStream);
            }

            context.Tasks.Update(task);
            context.SaveChanges();
            return null;
        }

        public static void RemoveAllExpiredTasks(Dictionary<string, ActiveTask> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks.ElementAt(i);

                if (task.Value.TimeUntil < DateTime.Now)
                {
                    tasks.Remove(task.Key);
                    i--;
                }
            }
        }

        public static TaskModel? GetTaskModel(CodeDbContext context, int id)
        {
            return context.Tasks?.Include(t => t.ViewUserCategories).FirstOrDefault(t => t.Id == id);
        }


    }
}
