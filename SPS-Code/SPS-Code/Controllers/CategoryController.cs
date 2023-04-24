using Microsoft.AspNetCore.Mvc;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using SPS_Code.Helpers;

namespace SPS_Code.Controllers
{
    [Route("cat")]
    public class CategoryController : Controller
    {
        private CodeDbContext _context;

        public CategoryController(CodeDbContext context) => _context = context;
        
        [Route("/cat")]
        public ActionResult Index()
        {
            if (!Helper.GetUser(HttpContext, _context, out _, true)) return Redirect("/404");

            return View(_context.UserCategories.ToList());
        }

        [Route("/cat/{id}")]
        public ActionResult Index(int id)
        {
            if (!Helper.GetUser(HttpContext, _context, out _, true)) return Redirect("/404");

            ViewBag.Cat = _context.UserCategories.FirstOrDefault(c => c.ID == id);

            return Index();
        }

        [HttpPost]
        [Route("edit")]
        public ActionResult Add(string name)
        {
            if (!Helper.GetUser(HttpContext, _context, out _, true)) return Redirect("/404");

            if (string.IsNullOrEmpty(name)) 
            {
                TempData[Helper.ErrorToken] = "Jméno je prázdné!";
                return RedirectToAction("Index"); 
            }

            if (_context.UserCategories.FirstOrDefault(c => c.Name == name) != null)
            {
                TempData[Helper.ErrorToken] = "Kategorie s tímto jménem již existuje!";
                return RedirectToAction("Index");
            }

            _context.UserCategories.Add(new UserCategory(name));
            _context.SaveChanges();

            TempData[Helper.SuccessToken] = "Přidání proběhlo v pořádku!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("edit/{id}")]
        public ActionResult Edit(string name, int id)
        {
            if (!Helper.GetUser(HttpContext, _context, out _, true)) return Redirect("/404");

            if (string.IsNullOrEmpty(name))  
            {
                TempData[Helper.ErrorToken] = "Jméno je prázdné!";
                return RedirectToAction("Index");
            }

            if (_context.UserCategories.FirstOrDefault(c => c.Name == name) != null)
            {
                TempData[Helper.ErrorToken] = "Kategorie s tímto jménem již existuje!";
                return RedirectToAction("Index");
            }

            var cat = _context.UserCategories.FirstOrDefault(c => c.ID == id);

            if (cat == null)
            {
                TempData[Helper.ErrorToken] = "Kategorie s tímto ID neexistuje!";
                return RedirectToAction("Index");
            }

            cat.Name = name;
            _context.SaveChanges();

            TempData[Helper.SuccessToken] = "Úprava proběhla v pořádku!";

            return RedirectToAction("Index");
        }

        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            if (!Helper.GetUser(HttpContext, _context, out _, true)) return Redirect("/404");

            var cat = _context.UserCategories.FirstOrDefault(c => c.ID == id);

            if (cat == null)
            {
                TempData[Helper.ErrorToken] = "Kategorie s tímto ID neexistuje!";
                return RedirectToAction("Index");
            }

            foreach (var user in _context.Users.Where(u => u.UserCategory.ID == id))
            {
                user.UserCategory = _context.UserCategories.FirstOrDefault();
            }

            _context.UserCategories.Remove(cat);
            _context.SaveChanges();

            TempData[Helper.SuccessToken] = "Odstanění proběhlo v pořádku!";

            return RedirectToAction("Index");
        }
    }
}
