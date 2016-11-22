using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebDiffer
{
    public class DiffPlexWebsite : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            //routes.MapRoute(
            //    "Default",
            //    "{controller}/{action}",
            //    new { controller = "diff", action = "index" }
            //);
            routes.MapRoute(
                "Home",
                "",
                new {controller = "diff", action = "index"}
                );
            routes.MapRoute(
                "Diff",
                "diff",
                new {controller = "diff", action = "diff"}
                );
        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
        }
    }
}