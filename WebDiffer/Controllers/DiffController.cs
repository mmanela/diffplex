using System.Web.Mvc;
using DiffPlex;
using DiffPlex.TextDiffer;

namespace WebDiffer.Controllers
{
    [HandleError]
    public class DiffController : Controller
    {
        private TextDiffBuilder diffBuilder;

        public DiffController(TextDiffBuilder bidiffBuilder)
        {
            diffBuilder = bidiffBuilder;
        }

        public DiffController()
        {
            diffBuilder = new TextDiffBuilder(new Differ());
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