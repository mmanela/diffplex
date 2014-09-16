using System.Web.Mvc;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Moq;
using WebDiffer.Controllers;
using Xunit;

namespace Facts.WebDiffer
{
    public class DiffControllerFacts
    {
        public class Diff
        {
            [Fact]
            public void Will_call_BidiffBuild_with_given_text_and_return_model_to_view()
            {
                var controller = TestableDiffController.Create();
                string oldText = "a";
                string newText = "b";
                var model = new SideBySideDiffModel();
                controller.MockDiffBuilder.Setup(x => x.BuildDiffModel(oldText, newText)).Returns(model);


                var result = controller.Diff(oldText, newText);

                Assert.IsType<ViewResult>(result);
                var viewResult = (ViewResult) result;
                Assert.Equal(model, viewResult.ViewData.Model);
            }

            [Fact]
            public void Will_change_null_inputs_into_emtpty_strings()
            {
                var controller = TestableDiffController.Create();
                string oldText = null;
                string newText = null;
                var model = new SideBySideDiffModel();
                controller.MockDiffBuilder.Setup(x => x.BuildDiffModel(string.Empty, string.Empty)).Returns(model);

                var result = controller.Diff(oldText, newText);

                Assert.IsType<ViewResult>(result);
                var viewResult = (ViewResult) result;
                Assert.Equal(model, viewResult.ViewData.Model);
            }
        }


        public class TestableDiffController : DiffController
        {
            public Mock<ISideBySideDiffBuilder> MockDiffBuilder;

            private TestableDiffController(Mock<ISideBySideDiffBuilder> diffBuilder)
                : base(diffBuilder.Object)
            {
                MockDiffBuilder = diffBuilder;
            }

            public static TestableDiffController Create()
            {
                var differ = new Mock<IDiffer>();
                return new TestableDiffController(new Mock<ISideBySideDiffBuilder>());
            }
        }
    }
}