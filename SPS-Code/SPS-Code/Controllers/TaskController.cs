using Microsoft.AspNetCore.Mvc;
using SPS_Code.Data;

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
            return View();
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
    }
}
