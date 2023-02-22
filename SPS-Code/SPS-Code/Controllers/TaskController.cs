using Microsoft.AspNetCore.Mvc;
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

            return View(task);
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

        [HttpGet("/task/generate/{taskId}")]
        public ActionResult Generate(int taskId)
        {

            
            //if (ActiveTasks.ContainsKey(HttpContext.Session.GetString(Helper.UserCookie))) return Redirect($"/task/{taskId}");
            
            //if (HttpContext.Session.GetString(Helper.UserCookie) == null) Redirect("/");

            var task = _context.Tasks?.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Redirect("/error");

            TaskModel.Generate(task);

            ActiveTask at = new()
            {
                TaskId = taskId
            };

            ActiveTasks.Add(HttpContext.Session.GetString(Helper.UserCookie), at);

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

    class ActiveTask
    { 
        public int TaskId { get; set; }
        public DateTime GeneratedTime = DateTime.Now;
    }
}
