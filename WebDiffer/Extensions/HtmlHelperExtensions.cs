using System.Web.Mvc;

namespace WebDiffer.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string GetBrowserCssClass(this HtmlHelper htmlHelper)
        {
            string userAgent = htmlHelper.ViewContext.HttpContext.Request.UserAgent;
            if (userAgent != null)
            {
                if (userAgent.Contains("MSIE 8"))
                    return "IE IE8";

                if (userAgent.Contains("MSIE 7"))
                    return "IE IE7";

                if (userAgent.Contains("Chrome"))
                    return "Chrome";

                if (userAgent.Contains("Firefox/3"))
                    return "FF FF3";

                if (userAgent.Contains("Firefox"))
                    return "FF";

                if (userAgent.Contains("MSIE 6"))
                    return "IE IE6";

                if (userAgent.Contains("Safari"))
                    return "Safari";

                if (userAgent.Contains("Opera"))
                    return "Opera";
            }

            return "";
        }
    }
}