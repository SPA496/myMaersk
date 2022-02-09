using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Mvc;

namespace myDamco.Utils
{
    /// <summary>
    /// Misc methods useful in controllers
    /// </summary>
    public class ControllerUtil
    {
        
        // KV: http://stackoverflow.com/questions/483091/render-a-view-as-a-string/2759898#2759898
        public static string RenderRazorViewToString(string viewName, object model, ControllerContext controllerContext, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            viewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        // Get the "sitepath" app-configuration from web.config, if exists, otherwise fake it from the current Request (won't work correctly behind loadbalancer (due to https/http problem), but works fine on dev).
        // The returned string does not end with "/".
        public static string GetSitePath()
        {
            string sitepath = WebConfigurationManager.AppSettings["SitePath"] ?? HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return sitepath;
        }

        // As GetSitePath, but only returns "http" or "https" (the scheme (=protocol)). Use this instead of Request.Url.Scheme, as that won't work correctly behind loadbalancer.
        public static string GetSiteScheme()
        {
            string sitepath = GetSitePath();
            return new Uri(sitepath).Scheme;
        }

        // As GetSitePath, but also includes the base path to myDamco. (such as /debug/, etc) TODO: Rename "sitepath" here and in web.config and everywhere else to "sitebase" or something else, since it doesn't contain the path of the site
        // The returned string ends with "/" by default. Can set the param to false to make it not end with "/".
        public static string GetSitePathPath(bool endWithSlash = true)
        {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext); // http://stackoverflow.com/questions/2031995/call-urlhelper-in-models-in-asp-net-mvc
            string sitepath = GetSitePath();
            string sitepathpath = sitepath + urlHelper.Content("~/");
            if (!endWithSlash && sitepathpath.EndsWith("/"))
                sitepathpath = sitepathpath.Substring(0, sitepathpath.Length - 1);
            return sitepathpath;
        }


        // Gets the build time of the assembly containing this class in UTC (= gets the time at which this assembly was linked)
        // 
        // This gets the date+time of when the assembly, that this class is contained within, was linked. The approach is a bit hacky (reads the assembly dll-file and extracts a timestamp from its header), 
        // but seems to be one of the best ways to do it. The reason for doing it, is so that we can easily check after deployment whether the wrong (old) version has been deployed (we don't have automatic 
        // version numbers).
        // See: http://stackoverflow.com/questions/267421/how-to-get-a-build-date-for-an-asp-net-application, http://www.codinghorror.com/blog/archives/000264.html, etc etc.
        // PE-header spec (layout of assembly dll-file header): http://msdn.microsoft.com/en-us/windows/hardware/gg463119.aspx. Easier info: https://www.simple-talk.com/blogs/2011/03/15/anatomy-of-a-net-assembly-pe-headers/.
        //
        // NOTE: The timestamp of course won't change if for example javascript, css or cshtml files are changed, since they are not part of the assembly.
        public static DateTime GetBuildTimeUtc()
        {
            // Ok to cache the buildtime value forever, since a deployment to the server will clear these caches. 
            // (Since the value almost never changes, it it is cached as an optimization, since it's currently used in NavigationController.Menu() before the e-tag check - so it should be as fast as possible)
            string cacheKey = "ControllerUtil.GetBuildTimeUtc";
            DateTime? cachedValue = HttpRuntime.Cache.Get(cacheKey) as DateTime?;
            if (cachedValue != null)
                return cachedValue.GetValueOrDefault();

            // Find the timestamp
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string filepath = assembly.Location;

            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;

            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null) s.Close();
            }

            int i = BitConverter.ToInt32(b, peHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + linkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);

            // Add to cache
            HttpRuntime.Cache.Insert(cacheKey, dt, null, DateTime.UtcNow.AddSeconds(900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

            return dt;
        }


    }
}