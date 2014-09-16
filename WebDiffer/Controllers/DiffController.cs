using System.Web.Mvc;
using DiffPlex;
using DiffPlex.DiffBuilder;

namespace WebDiffer.Controllers
{
    [HandleError]
    public class DiffController : Controller
    {
        private readonly ISideBySideDiffBuilder diffBuilder;

        public DiffController(ISideBySideDiffBuilder bidiffBuilder)
        {
            diffBuilder = bidiffBuilder;
        }

        public DiffController()
        {
            diffBuilder = new SideBySideDiffBuilder(new Differ());
        }


        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult Diff(string oldText, string newText)
        {
            var model = diffBuilder.BuildDiffModel(oldText ?? string.Empty, newText ?? string.Empty);

            return View(model);
        }
    }
}