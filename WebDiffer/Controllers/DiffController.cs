using DiffPlex;
using DiffPlex.DiffBuilder;
using Microsoft.AspNet.Mvc;
using WebDiffer.ViewModels;

namespace WebDiffer.Controllers
{
	public class DiffController : Controller
	{
		private readonly ISideBySideDiffBuilder diffBuilder;

		public DiffController(ISideBySideDiffBuilder bidiffBuilder)
		{
			diffBuilder = bidiffBuilder;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Diff(DiffViewModel model)
		{
			var diff = diffBuilder.BuildDiffModel(model.OldText ?? string.Empty, model.NewText ?? string.Empty);
			return View(diff);
		}
	}
}