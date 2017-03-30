using Microsoft.AspNetCore.Mvc;
using DiffPlex.DiffBuilder;

namespace WebDiffer.Controllers
{
    public class DiffController : Controller
    {
        private readonly ISideBySideDiffBuilder diffBuilder;

        public DiffController(ISideBySideDiffBuilder bidiffBuilder)
        {
            diffBuilder = bidiffBuilder;
        }


        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult Diff(string oldText, string newText)
        {
            var model = diffBuilder.BuildDiffModel(oldText ?? string.Empty, newText ?? string.Empty);

            return View(model);
        }
    }
}
