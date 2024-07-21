using Microsoft.AspNetCore.Mvc;

namespace WebApplication6.Controllers
{
    public class FilterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FilterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult GetOperations()
        {
            var operations = _context.Operations.Select(o => o.Action).Distinct().ToList();
            return Json(operations);
        }

        [HttpGet]
        public JsonResult GetObjectLevels()
        {
            var objectLevels = _context.ObjectLevels.Select(o => o.Name).Distinct().ToList();
            return Json(objectLevels);
        }
    }
}
