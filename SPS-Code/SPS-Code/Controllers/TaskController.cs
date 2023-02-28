using Microsoft.AspNetCore.Mvc;
using SPS_Code.Controllers.RequestModels;
using SPS_Code.Data;
using SPS_Code.Data.Models;

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
            ActiveTask at = new();
            if(ActiveTasks.ContainsKey("Test")) at = ActiveTasks["Test"];
            if (at.TaskId  == id) rt.ActiveTask = at;
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
        public ActionResult Delete()
        {
            return View();
        }


        [HttpGet("/task/download/{taskId}")]
        public ActionResult DownloadInput(int taskId)
        {
            //if (HttpContext.Session.GetString(Helper.UserCookie) == null) Redirect("/");
            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Redirect("/error");


            // if(ActiveTasks.ContainsKey(HttpContext.Session.GetString(Helper.UserCookie)));
            if (!ActiveTasks.ContainsKey("Test")) return Redirect($"/task/{taskId}");

            ActiveTasks.TryGetValue("Test", out var at);
            if (at.TaskId != taskId) return Redirect($"/task/{taskId}");

            return File(at.Uri, "text/plain");
        }

        [HttpGet("/task/generate/{taskId}")]
        public ActionResult Generate(int taskId)
        {
            //if (HttpContext.Session.GetString(Helper.UserCookie) == null) Redirect("/");
            //if (ActiveTasks.ContainsKey(HttpContext.Session.GetString(Helper.UserCookie))) return Redirect($"/task/{taskId}");

            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Redirect("/error");

            var path = TaskModel.Generate(task);

            ActiveTask at = new()
            {
                TaskId = taskId,
                Uri = path,
            };

            //ActiveTasks.Add(HttpContext.Session.GetString(Helper.UserCookie), at);
            ActiveTasks.Add("Test", at);
            return Redirect($"/task/{taskId}");
        }


        [HttpPost("/validate/:taskId")]
        public ActionResult ValidateResults(int taskId)
        {

            return Json("ok");
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

        public DateTime GeneratedTime = DateTime.Now.AddMinutes(5);
    }

    public class ResponseTask : TaskModel
    {
        public ActiveTask ActiveTask { get; set; }
    }
}
