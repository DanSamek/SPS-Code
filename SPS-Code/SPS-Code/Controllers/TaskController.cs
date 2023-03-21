using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPS_Code.Controllers.RequestModels;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using SPS_Code.Helpers;

namespace SPS_Code.Controllers
{
    [Route("task")]
    public class TaskController : Controller
    {
        // User id : ActiveTask
        static Dictionary<string, ActiveTask> ActiveTasks = new();
        private CodeDbContext _context;
        public TaskController(CodeDbContext context)
        {
            _context = context;
        }

        [HttpGet("/task/{id}")]
        public ActionResult Task(int id)
        {
            var task = _context.Tasks?.FirstOrDefault(x => x.Id == id);
            if (task == null) return Redirect("/");

            ResponseTask rt = new();
            rt.MaxPoints = task.MaxPoints;
            rt.Created = task.Created;
            rt.Name = task.Name;
            rt.Id = task.Id;
            rt.Description = task.Description;
            rt.Inputs = task.Inputs;
            rt.Outputs = task.Outputs;
            rt.Visible = task.Visible;
            ActiveTask at = new();
            var cookie = HttpContext.Session.GetString(Helper.UserCookie);
            if (cookie == null) return View(rt);
            if (ActiveTasks.ContainsKey(cookie)) at = ActiveTasks[cookie];
            if (at.TaskId == id) rt.ActiveTask = at;

            var user = _context.Users.Include(x => x.Tasks).FirstOrDefault(x => x.Id == cookie);
            var taskResult = user.Tasks.FirstOrDefault(x => x.Task.Id == id);
            rt.UserTaskResult = taskResult;
            return View(rt);
        }

        [Route("show")]
        public ActionResult Show()
        {
            var tasks = _context.Tasks?.ToList();
            return View(tasks);
        }

        [Route("create")]
        public ActionResult Create()
        {
            return View(new TaskCreateRequest());
        }

        [Route("edit")]
        public ActionResult Edit()
        {
            return View();
        }

        [Route("delete")]
        public ActionResult Delete() => Redirect("/");

        [Route("hide")]
        public ActionResult Hide() => Redirect("/");

        [HttpGet("/task/downloadInput/{taskId}")]
        public ActionResult Download(int taskId)
        {
            var cookie = HttpContext.Session.GetString(Helper.UserCookie);

            if(cookie == null || !ActiveTasks.ContainsKey(cookie)) return Redirect("/");
           
            var at = ActiveTasks[cookie];

            if (at.TaskId != taskId) return Redirect($"/task/{taskId}");

            byte[] fileData = System.IO.File.ReadAllBytes(at.Uri);

            return File(fileData, "application/force-download", "input.txt");
        }

        [HttpGet("/task/generate/{taskId}")]
        public ActionResult Generate(int taskId)
        {
            var cookie = HttpContext.Session.GetString(Helper.UserCookie);

            if (cookie == null) return Redirect("/");

            if (ActiveTasks.ContainsKey(cookie)) return Redirect($"/task/{taskId}");
           
            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Redirect("/error");

            var path = TaskModel.Generate(task);

            ActiveTask at = new()
            {
                TaskId = taskId,
                Uri = path,
                TimeUntil = DateTime.Now.AddMinutes(task.MaxSubmitTimeMinutes)
            };

            ActiveTasks.Add(cookie, at);

            return Redirect($"/task/{taskId}");
        }

        [HttpPost("/task/validateInput/{taskId}")]
        public ActionResult ValidateInput(int taskId, [FromForm] IFormFile UserFile)
        {
            var cookie = HttpContext.Session.GetString(Helper.UserCookie);
            if (cookie == null) return Redirect("/");

            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null || !ActiveTasks.ContainsKey(cookie)) return Redirect("/");

            var at = ActiveTasks[cookie];

            var points = TaskModel.Validate(UserFile, task, at.Uri);

            var user = _context.Users.Include(x => x.Tasks).FirstOrDefault(x => x.Id == cookie);
            

            var taskResult = user.Tasks.FirstOrDefault(x => x.Task.Id == taskId);

            if (taskResult == null)
            {
                user.Tasks.Add(new UserTaskResult()
                {
                    AttemptsCount = 1,
                    LastAttemptTime = DateTime.Now,
                    MaxPointsObtained = points,
                    Task = task,
                    User = user
                });
            }
            else
            {
                taskResult.AttemptsCount++;
                taskResult.LastAttemptTime = DateTime.Now;
                taskResult.MaxPointsObtained = Math.Max(taskResult.MaxPointsObtained, points);
            }

            ActiveTasks.Remove(cookie);
            _context.Users.Update(user);
            return Redirect($"/task/{task.Id}");
        }


        [HttpPost]
        [Route("create")]
        public ActionResult CreatePost([FromForm] TaskCreateRequest taskCreateRequest)
        {
            var errorMessage = TaskModel.CreateAndSaveToDb(taskCreateRequest, _context, out var taskId);
            if (errorMessage != null) return View("Create", taskCreateRequest.SetError(errorMessage));

            return Redirect($"/task/{taskId}");
        }
    }

    public class ActiveTask
    { 
        public int TaskId { get; set; }
        public string Uri { get; set; }
        public DateTime TimeUntil { get; set; }
    }

    public class ResponseTask : TaskModel
    {
        public ActiveTask ActiveTask { get; set; }

        public UserTaskResult UserTaskResult { get; set; }
    }
}
