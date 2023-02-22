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
        private CodeDbContext _context;
        public TaskController(CodeDbContext context)
        {
            _context = context;
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

        [HttpPost("/generate/:taskId")]
        public ActionResult GenerateFile(int taskId)
        {
            return Json("OK");
        }


        public ActionResult ValidateResults()
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
}
