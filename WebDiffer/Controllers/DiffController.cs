using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiffPlex.DiffBuilder;
using DiffPlex;

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
