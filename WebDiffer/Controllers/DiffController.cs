using DiffPlex.DiffBuilder;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
	public class DiffController : Controller
	{
		private readonly ISideBySideDiffBuilder _diffBuilder;

		public DiffController(ISideBySideDiffBuilder bidiffBuilder)
		{
			_diffBuilder = bidiffBuilder;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Diff(DiffViewModel model)
		{
			var diff = _diffBuilder.BuildDiffModel(model.OldText ?? string.Empty, model.NewText ?? string.Empty);
			return View(diff);
		}
	}
}