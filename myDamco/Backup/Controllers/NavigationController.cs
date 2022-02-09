using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using myDamco.Config;
using myDamco.Database;
using myDamco.Models;
using myDamco.Utils;
using Newtonsoft.Json;
using UAMSharp;

namespace myDamco.Controllers
{
    /// <summary>
    /// Controller for the navigation menu.
    /// </summary>
    public class NavigationController : Controller
    {
        private myDamcoEntities myDamcoDB = new myDamcoEntities();

        private NavigationModel getNavigationModel()
        {
            var operations = Roles.GetRolesForUser(); // Get UAM operations APPLICATION:FUNCTION in users current role (does not get the list of roles!)

            var applications = myDamcoDB.Navigation // Get applications that the user has permission for.
                .Where(x => operations.Contains("UAM:" + x.UAMApplication + (String.IsNullOrEmpty(x.UAMFunction) ? "" : ":" + x.UAMFunction)))
                .OrderBy(x => x.Priority).ToList();

            Navigation active = null;
            string selected = "";
            var path = HttpContext.Request.AppRelativeCurrentExecutionFilePath;
            if (HttpContext.Request.QueryString["menu"] != null)
            {
                active = applications.SingleOrDefault(x => x.UId == HttpContext.Request.QueryString["menu"]);
                selected = HttpContext.Request.QueryString["menu"];
            }
            else if (path.StartsWith("~/Administration") && applications.Exists(x => x.Target == "Admin"))
            {
                active = applications.First(x => x.Target == "Admin");
            }
            else if (path.StartsWith("~/Applications/"))
            {
                var uid = path.Replace("~/Applications/", "");
                var index = uid.IndexOf("/");
                if (index != -1)
                    uid = uid.Substring(0, index);

                active = applications.SingleOrDefault(x => x.UId == uid);
                if (active == null)
                    active = applications.SingleOrDefault(x => x.UAMApplication == uid);
            }
            else if (path.StartsWith("~/WidgetLink/"))
            {
                var uid = path.Replace("~/WidgetLink/", "");
                var index = uid.IndexOf("/");
                if (index != -1)
                    uid = uid.Substring(0, index);

                active = applications.SingleOrDefault(x => x.UId == uid);
            }

            var model = new NavigationModel { MenuItems = applications, activeItem = active, qsMenu = selected };
            return model;
        }

        // KV: Used by the "make sure client is logged before we start loading menu"-hack
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult LoginDummy()
        {
            var sitepath = ControllerUtil.GetSitePathPath(false);
            return Content("<html><body onload='redirect()'><script>/*function redirect()*/ {window.location.href = '" + sitepath + "/Navigation/LoginDummy2';}</script></body></html>");
        }

        public ActionResult LoginDummy2()
        {
            return Content("Hello world");
        }

        public ActionResult Menu()
        {
            var allowedRoles = new List<string> { "UAM:MYDAMCO:ADMINISTRATION", "UAM:MYDAMCO:USE" };

            // The url for the CSS file.
            string sitepath = ControllerUtil.GetSitePathPath();

            bool compressJavaScript = Settings.Navigation.CompressExternalMenuJavaScript;

            Profile profile = myDamco.Profile.GetProfile();
            if (profile == null || !Roles.GetRolesForUser(profile.LoginId).Any(x => allowedRoles.Contains(x))) // <- Calls Roles.GetRolesForUser with argument, instead of with no args, in order to get a proper exception 
            {                                                                                                  //    message in case of error. The no-arg version throws NullReferenceException (!) in case UAM is down, which
                                                                                                               //    would result in the generic "An unexpected error occurred" message in OnException below. 

                // Max-age is set to 0 here, otherwise the whole login procedure (Menu, logindummy, logindummy2, Menu) will be cached by IE and be performed on every following page-load (slightly poor page-load performance, due to higher number of requests).
                Response.Cache.SetMaxAge(new TimeSpan(0, 0, 0)); // "Cache-Control: private, max-age=..."
                
                // The javascript that is needed for the menu generally
                string mydamcologinscript = GetMenuJavaScript(Server.MapPath("~/Scripts/menuloadinglogin.js"), compressJavaScript);

                return new JavaScriptResult()
                {
                    Script = "var basepathMyDamcoTopBar = \"" + sitepath + "\";\r\n" +
                             mydamcologinscript
                };
            }

            // This is to prevent the browser from caching this response, without checking the e-tag (IE caches it otherwise, if we don't set max-age). 
            // To improve performance (when browsing quickly on reporting), i set max-age a bit above 0. This improves the user experience a lot - but is at the expense of that the e-tag is not checked until this response expires in the browser cache (so the max-age should not be sat too high).
            // ("Change role" forces a reload of the menu, which overrides the cache. So it will still work correctly immediately when changing role, even with max-age > 0.)
            Response.Cache.SetMaxAge(new TimeSpan(0, 0, 120)); // "Cache-Control: private, max-age=..."
            //Response.Cache.SetMaxAge(new TimeSpan(0, 0, 0)); // "Cache-Control: private, max-age=..."

            // Including the build-time in the e-tag, is a quick way to make sure that when a new version of myDamco is deployed, users won't cache the old menu javascript-code forever (or until they press reload), due to their e-tag not changing. 
            // (A more fool-proof way would be to check the max of the timestamps of all js/css/cshtml files used by the menu, but that gets ugly and interdependant quickly (+slower). This is the simple way.)
            DateTime buildTime = ControllerUtil.GetBuildTimeUtc();

            // Check E-Tag
            var eTag = profile.RoleId + "-" + profile.LoginId + "-" + buildTime.Ticks;
            var req = Request.Headers["If-None-Match"];
            if (req == eTag)
            {
                Response.Clear();
                Response.StatusCode = 304;
                Response.SuppressContent = true;
                return null;
            }

            // The HTML
            var applications = getNavigationModel();
            string viewdata = ControllerUtil.RenderRazorViewToString("Navigation", applications, this.ControllerContext, this.ViewData, this.TempData);
            viewdata = JsonConvert.SerializeObject(viewdata); // This escapes since viewdata is simply a string - it does not create a "full" JSON object 

            DateTime lastCSSChange = System.IO.File.GetLastWriteTime(Server.MapPath("~/Content/menu.css"));
            DateTime lastMtCSSChange = System.IO.File.GetLastWriteTime(Server.MapPath("~/Content/maersk-theme.css"));
            DateTime lastJSLibChange = GetLastFileChangeOfExternalMenuJS();

            // The javascript that is needed for the menu generally
            string menuscript = GetMenuJavaScript(Server.MapPath("~/Scripts/menu.js"), compressJavaScript);

            // The javascript that is needed to actually embed
            string menuloadingscript = GetMenuJavaScript(Server.MapPath("~/Scripts/menuloading.js"), compressJavaScript);

            Response.ContentType = "text/javascript";
            Response.AppendHeader("ETag", eTag);

            var result = new JavaScriptResult();
            result.Script = "var myDamcoTopbarHTML = " + viewdata + ";\r\n" +
                   "var basepathMyDamcoTopBar = \"" + sitepath + "\";\r\n" +
                   "var lastCSSChange = \"" + lastCSSChange.Ticks + "\";\r\n" +
                   "var lastMtCSSChange = \"" + lastMtCSSChange.Ticks + "\";\r\n" +
                   "var myDamcoLastJSChange = \"" + lastJSLibChange.Ticks + "\";\r\n";
            
            // Append variable telling whether logo is white labelled.
            UAMOrganization org = profile.RoleOrganizationObj;
            if (org.Logo == null)
            {
                result.Script += "var MyDamco_isLogo = false;\r\n";
            }
            else
            {
                result.Script += "var MyDamco_isLogo = true;\r\n";
            }

            result.Script += menuloadingscript + "\r\n" +
                   "function menuscriptMyDamcoTopBar($,moment) {" + menuscript + "\r\n}"; // KV: important to send in menuscript as the last, since menuloading will asume presence of DOM elements and the basepath variable defined in the HTML

            return result;
        }

        // Load and (optionally) compress javascript for the external menu. Caches the (possibly) compressed result in memory for optimal performance (so that it does not have to load and compress on each pageview).
        private string GetMenuJavaScript(string filename, bool minimize)
        {
            // check if we have a cached version of the minimized files and if it is as recent as the file on disk
            DateTime lastFileChange = System.IO.File.GetLastWriteTime(filename);
            string cacheKey = "DashboardController.cs:GetMenuJavaScript:" + filename;
            var cacheValue = HttpRuntime.Cache.Get(cacheKey) as MenuJavaScriptCacheValue;

            // if we have a cache version of the javascript and it is not older than the file we have on disk. (and also the minification of the cached version must fit with the value of the "minimize" variable)
            if (cacheValue != null && cacheValue.lastFileChange == lastFileChange && cacheValue.minimized == minimize)
            {
                return cacheValue.javascript;
            }
            else // otherwise load the javascript from disk and (optionally) minimize it + put it into the cache.
            {
                string javascript = System.IO.File.ReadAllText(filename); // Concurrency note: No problem if the file changed between getting the file-timestamp and here, as it will just be reloaded next time this method is called (the file-timestamp will be newer next time).
                if (minimize) javascript = Yahoo.Yui.Compressor.JavaScriptCompressor.Compress(javascript);
                var newCacheValue = new MenuJavaScriptCacheValue { lastFileChange = lastFileChange, javascript = javascript, minimized = minimize };
                HttpRuntime.Cache.Insert(cacheKey, newCacheValue, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                return javascript;
            }
        }

        private class MenuJavaScriptCacheValue { public DateTime lastFileChange; public string javascript; public bool minimized;}

        /* Called by the external menu. This action method returns the library files which is needed by the external menu (JQuery, JQuery UI, moment, etc) merged into 1 file. 
         * 
         * Reason: To shield our library files from reporting, so that they don't clash with reportings library files (e.g. JQuery) - no matter how the javascript is schedules. 
         *         (Since javascript is single-threaded, it is impossible for any reporting javascript code to execute while our library javascript is executing. For this to work, 
         *          this returned file must also contain ALL the logic for shielding our libraries from theirs - it cannot be deferred to later, since that would make it possible 
         *          for some or reportings code to execute before we shield them from our libraries.)
         *          
         * (No authorization attribute - our js files can already be loaded without authorization)
         */
        [OutputCache(Location = OutputCacheLocation.Client, Duration = 3600)] // Maybe "Client" is too extreme?
        public ActionResult ExternalMenuJS()
        {
            string[] filenames = GetExternalMenuJSFilenames();

            // get the timestamp of the newest of the files we wanna load
            DateTime lastFileChange = GetLastFileChangeOfExternalMenuJS();

            // check if we have a cached version of the boundled files and if it is as recent as the newest file on disk
            string cacheKey = "DashboardController.cs:ExternalMenuJS:";
            var cacheValue = HttpRuntime.Cache.Get(cacheKey) as MenuJavaScriptCacheValue;

            // if we have a cache version of the javascript and it is not older than the newest of the files we have on disk.
            if (cacheValue != null && cacheValue.lastFileChange == lastFileChange)
            {
                return Content(cacheValue.javascript, "text/javascript");
            }
            else // otherwise load the javascript from disk + put it into the cache.
            {
                string mergedjs = "";
                foreach (string filename in filenames)
                    mergedjs += System.IO.File.ReadAllText(filename) + "\r\n"; // Concurrency note: No problem if the file changed between getting the file-timestamp and here, as it will just be reloaded next time this method is called (the file-timestamp will be newer next time).

                string javascript = "function mydamcoScriptsInit() {\r\n"+
                                    "    var oldMomentBinding = window.moment;\r\n" + 
                                    mergedjs +
                                    "    var myDamcoJQuery = $.noConflict(true);\r\n" + // Remove JQuery/$ from the global namespace again (restore the previous bindings)
                                    "    var myDamcoMoment = moment;\r\n" +
                                    "    window.moment = oldMomentBinding;\r\n" + // Remove moment from the global namespace again (restore the previous binding)
                                    "    return {jquery: myDamcoJQuery, moment: myDamcoMoment};\r\n" +
                                    "}\r\n";

                var newCacheValue = new MenuJavaScriptCacheValue { lastFileChange = lastFileChange, javascript = javascript };
                HttpRuntime.Cache.Insert(cacheKey, newCacheValue, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                return Content(javascript, "text/javascript");
            }
        }

        // Get the timestamp of the newest of the JS files which are merged together in ExternalMenuJS
        private DateTime GetLastFileChangeOfExternalMenuJS()
        {
            string[] filenames = GetExternalMenuJSFilenames();
            return filenames.Select(filename => System.IO.File.GetLastWriteTime(filename)).Max();
        }

        // These are the javascript library files to be loaded in ExternalMenuJS()
        private string[] GetExternalMenuJSFilenames()
        {
            // order is important (the order here is the output order in the merged javascript file)
            string[] filenames = { Server.MapPath("~/Scripts/jquery-1.7.2.min.js"), Server.MapPath("~/Scripts/jquery-ui-1.8.20.min.js"), Server.MapPath("~/Scripts/moment.js") }; 
            return filenames;
        }

        // Returns logo from UAM profile.
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult GetLogo(string Organization)
        {
            Profile profile = myDamco.Profile.GetProfile();
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.Cache.SetMaxAge(new TimeSpan(1, 0, 0));
            UAMOrganization org = profile.RoleOrganizationObj;
            if (org == null || org.Logo == null)
            {
                string path = Request.PhysicalApplicationPath;
                Response.ContentType = "image/jpeg";
                Response.WriteFile(path+"Content\\images\\myDamco-logo.png");
            }
            else
            {
                Response.ContentType = org.LogoMimeType;
                Response.BinaryWrite(org.Logo);
                Response.Write("Bytes: "+org.Logo.Length);
            }
            Response.Flush();
            Response.End();
            return new EmptyResult();
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Navigation()
        {
            var applications = getNavigationModel();
            return PartialView(applications);
        }

        // Returns a JSON-list of all currently disabled UAM-functions. (CORS is enabled on this type of response to support cross-domain AJAX for the external menu, if the browser supports it)
        // Optionally it returns JSONP, if "callback" is not null. This reason this was made, is to get around the same-origin policy (for ajax), on browsers which does not support CORS (IE6-7 (8?)).
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult DisabledFunctions(string callback = null)
        {
            var now = DateTime.UtcNow;
            var list = myDamcoDB.Downtime
                        .Where(x => x.From <= now && now <= x.To)
                        .ToList()
                        .Select(x => new { App = x.UAMApplication, Func = x.UAMFunction, From = DateTime.SpecifyKind(x.From, DateTimeKind.Utc), To = DateTime.SpecifyKind(x.To, DateTimeKind.Utc), Message = x.Message });

            if (callback == null) // return normal JSON (but with CORS headers to allow for cross-domain AJAX calls to this action-method)
            {
                // These headers allows this action-method to be called by cross-domain ajax requests (CORS). Note: "Allow-Origin=*" does not work with "Allow-Credentials=true". That's the reason we write the URL manually for "Allow-Origin" (unless NULL). 
                Response.AppendHeader("Access-Control-Allow-Origin", Request.UrlReferrer != null ? Request.UrlReferrer.GetLeftPart(UriPartial.Authority) : "*"); // Enable CORS to allow AJAX requests to this action-method from other domains (if the browser supports it)
                Response.AppendHeader("Access-Control-Allow-Credentials", "true"); // Allow ajax requests to send credentials (cookies) along with ajax requests. (see: http://stackoverflow.com/questions/2870371/why-is-jquerys-ajax-method-not-sending-my-session-cookie)
                Response.AppendHeader("Access-Control-Allow-Headers", "X-Requested-With");

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else // return JSONP
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat; // Since this is the format Json() uses and our javascript code currently relies on that. Should perhaps change to ISO (the default).
                return Content(callback + "(" + JsonConvert.SerializeObject(list, jsonSerializerSettings) + ");", "application/javascript");
            }
        }


        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);

                bool returnPartialPage = false;
                bool returnJavascript = false;

                string action = filterContext.RouteData.Values["action"].ToString();
                switch (action)
                {
                    case "Navigation":
                        returnPartialPage = true;
                        break;
                    case "Menu":
                        returnJavascript = true;
                        break;
                }

                // If the exception is wrapped in a provider exception, find its inner exception and use that instead
                if (exception.GetType().ToString() == "System.Configuration.Provider.ProviderException" && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }

                // Exception type specific errors
                ErrorModel result = DashboardController.ConvertExceptionToErrorModel(exception);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();

                if (returnPartialPage)
                {
                    IController c = new StaticContentController();
                    RouteData rd = new RouteData();
                    rd.Values["controller"] = "StaticContent";
                    rd.Values["action"] = "ErrorNavigation";
                    rd.Values["model"] = result;    // <- param for the action method
                    RequestContext rc = new RequestContext(filterContext.HttpContext, rd);

                    try
                    {
                        c.Execute(rc);
                    }
                    catch (Exception) // Down-grade to simple content if something goes wrong
                    {
                        this.Content("Error: " + result.Title + " - " + result.Description).ExecuteResult(this.ControllerContext);
                    }
                }
                else if (returnJavascript)
                {
                    try
                    {
                        // This might be a slight hack - would be better to call the controllers action-method (instead of calling the view directly) and turn that into a string
                        StaticContentController c = new StaticContentController();
                        RouteData rd = new RouteData();
                        rd.Values["controller"] = "StaticContent";
                        rd.Values["action"] = "ErrorNavigation";
                        c.ControllerContext = new ControllerContext(HttpContext, rd, c);
                        string viewdata = ControllerUtil.RenderRazorViewToString("_ErrorNavigation", result, c.ControllerContext, this.ViewData, this.TempData);
                        viewdata = JsonConvert.SerializeObject(viewdata); // This escapes since viewdata is simply a string - it does not create a "full" JSON object                   

                        // Return the script (This is similar to the Menu action method)
                        string sitepath = ControllerUtil.GetSitePathPath();

                        DateTime lastCSSChange = System.IO.File.GetLastWriteTime(Server.MapPath("~/Content/menu.css"));
                        DateTime lastMtCSSChange = System.IO.File.GetLastWriteTime(Server.MapPath("~/Content/maersk-theme.css"));
                        DateTime lastJSLibChange = GetLastFileChangeOfExternalMenuJS();
                        string menuloadingscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/menuloading.js"));

                        string javascript = "var myDamcoTopbarHTML = " + viewdata + ";\r\n" +
                                            "var basepathMyDamcoTopBar = \"" + sitepath + "\";\r\n" +
                                            "var lastCSSChange = \"" + lastCSSChange.Ticks + "\";\r\n" +
                                            "var lastMtCSSChange = \"" + lastMtCSSChange.Ticks + "\";\r\n" +
                                            "var myDamcoLastJSChange = \"" + lastJSLibChange.Ticks + "\";\r\n" +
                                            menuloadingscript + "\r\n" +
                                            "function menuscriptMyDamcoTopBar($) {}\r\n";
                        filterContext.HttpContext.Response.StatusCode = 200; // the javascript won't load in <script> tags if the status code is 500. Therefore, setting it back to 200.
                        this.JavaScript(javascript).ExecuteResult(this.ControllerContext);
                    }
                    catch (Exception) // Down-grade to empty content if something goes wrong
                    {
                        this.Content("").ExecuteResult(this.ControllerContext);
                    }
                }
                else
                {
                    this.Json(result, JsonRequestBehavior.AllowGet).ExecuteResult(this.ControllerContext);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            myDamcoDB.Dispose();
            base.Dispose(disposing);
        }
    }
}
