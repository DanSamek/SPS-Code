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

        // for multiple requests
        private readonly object _lock = new object();
        private readonly object _lock2 = new object();

        public TaskController(CodeDbContext context)
        {
            _context = context;
        }

        [HttpGet("/task/{id}")]
        public ActionResult Task(int id)
        {
            var task = _context.Tasks?.FirstOrDefault(x => x.Id == id);
            if (task == null) return Redirect("/404");

            if(!task.Visible) return Redirect("/404");

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

            Helper.GetUser(HttpContext, _context, out var user);
            ViewData["admin"] = user?.IsAdmin;
            return View(tasks);
        }

        [Route("create")]
        public ActionResult Create()
        {
            if (!Helper.GetUser(HttpContext, _context, out var user, true)) return Redirect("/404");
            return View(new TaskCreateRequest());
        }

        [Route("edit")]
        public ActionResult Edit()
        {
            return View();
        }

        [Route("delete")]
        public ActionResult Delete() => Redirect("/");

        [HttpGet("/task/unhide/{taskId}")]
        [HttpGet("/task/hide/{taskId}")]
        public ActionResult Hide(int taskId)
        {
            var task = _context.Tasks?.FirstOrDefault(t => t.Id == taskId);
            if(task == null) return Redirect("/404");
            if(!Helper.GetUser(HttpContext, _context, out var user, true)) return Redirect("/404");

            task.Visible = !task.Visible;
            _context.Tasks.Update(task);
            _context.SaveChanges();
            return Redirect("/task/show");
        }

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

            if (cookie == null) return Redirect("/404");

            if (ActiveTasks.ContainsKey(cookie)) return Redirect($"/task/{taskId}");
           
            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Redirect("/404");

            // For multiple requests
            string path;
            lock (_lock)
            {
                path = TaskModel.Generate(task);
            }

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
            if (cookie == null) return Redirect("/404");

            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null || !ActiveTasks.ContainsKey(cookie)) return Redirect("/404");

            var at = ActiveTasks[cookie];

            int points;
            lock(_lock2) 
            {
                points = TaskModel.Validate(UserFile, task, at.Uri);
            }

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
            if(!Helper.GetUser(HttpContext, _context, out var user, true)) return Redirect("/404");

            var errorMessage = TaskModel.CreateAndSaveToDb(taskCreateRequest, _context, out var taskId);
            if (errorMessage != null) return View("Create", taskCreateRequest.SetError(errorMessage));

            return Redirect($"/task/{taskId}");
        }


        [HttpGet("/task/edit/{taskId}")]
        public ActionResult Edit(int taskId)
        {
            // If no admin, redirect
            if (!Helper.GetUser(HttpContext, _context, out var user, true)) return Redirect("/404");

            var data = _context.Tasks.FirstOrDefault(t => t.Id == taskId);
            if(data == null) return Redirect("/404");

            var ter = new TaskEditRequest()
            {
                Description = data.Description,
                Outputs = data.Outputs,
                Inputs = data.Inputs,
                MaxSubmitTimeMinutes = data.MaxSubmitTimeMinutes,
                Name = data.Name,
                MaxPoints = data.MaxPoints,
                TestCount = data.TestCount,
                Id = data.Id
            };
          
            return View("Edit", ter);
        }

        [HttpPost("/task/edit/{taskId}")]
        public ActionResult EditPost(int taskId, [FromForm] TaskEditRequest editRequest)
        {
            if (!Helper.GetUser(HttpContext, _context, out var user, true)) return Redirect("/404");

            var succeed = TaskModel.Edit(editRequest, _context, taskId);
            if (succeed != null) return View("Edit", editRequest.SetError("Nastala neočekávaná chyba!"));
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
