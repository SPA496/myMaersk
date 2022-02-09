using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace myDamco
{
    // http://stackoverflow.com/questions/7459560/set-default-for-displayformatattribute-convertemptystringtonull-to-false-across
    // KV: This is to make sure that we can read the difference between Empty String and Null when working between AJAX calls and MVC Actions.
    public class CustomModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            var modelMetadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);
            if (string.IsNullOrEmpty(propertyName)) return modelMetadata;

            if (modelType == typeof(String))
                modelMetadata.ConvertEmptyStringToNull = false;

            return modelMetadata;
        }
    }

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        System.Timers.Timer reloadRouteTimer;

        public static void ReloadRoutes()
        {
            var routes = RouteTable.Routes;
            using (routes.GetWriteLock())
            {
                routes.Clear();
                AreaRegistration.RegisterAllAreas();
                RegisterRoutes(routes);
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            var db = new Database.myDamcoEntities();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "ELearningShowService",
                "Services/ELearningShow/{widgetUID}/{application}/{name}",
                new { controller = "Services", action = "ELearningShow", var = UrlParameter.Optional }
            );

            routes.MapRoute(
                "ELearningLoadFile",
                "Services/ELearningLoadFile/{widgetUID}/{application}/{id}/{*url}",
                new { controller = "Services", action = "ELearningLoadFile", var = UrlParameter.Optional }
            );

            routes.MapRoute(
                "Status",
                "status",
                new { controller = "Status", action = "Status"}
            );

            routes.MapRoute(
                "ErrorService",
                "Services/Error/{statuscode}/{content}/{mimetype}/{wait}",
                new { controller = "Services", action = "Error", mimetype = UrlParameter.Optional, wait = UrlParameter.Optional }
            );

            routes.MapRoute(
                "WidgetLink",
                "WidgetLink/{appUID}/{widgetUID}/{*var}",
                new { controller = "Dashboard", action = "WidgetLink", var = UrlParameter.Optional}
            );

            routes.MapRoute(
                "Services",
                "Services/{action}/{widgetUID}/{selectedFeed}",
                new { controller = "Services", widgetUID = UrlParameter.Optional, selectedFeed = UrlParameter.Optional, waitForNonCached = true }
            );

            routes.MapRoute(
                "Menu",
                "Globalmenu/menuV2.js",
                new { controller = "Navigation", action = "Menu" },
                new string[] { "myDamco.Controllers" }
            );

            routes.MapRoute(
                "Menu-legacy",
                "Dashboard/Menu",
                new { controller = "Navigation", action = "Menu" },
                new string[] { "myDamco.Controllers" }
            );


            // TODO: Needed? Only in case some users has old javascript cached - is not expected to be called by external sites.
            routes.MapRoute(
                "DisabledFunctions-legacy",
                "Dashboard/DisabledFunctions",
                new { controller = "Navigation", action = "DisabledFunctions" },
                new string[] { "myDamco.Controllers" }
            );

            // TODO: Needed? Only in case some users has old javascript cached - is not expected to be called by external sites.
            routes.MapRoute(
                "LoginDummy-legacy",
                "Dashboard/LoginDummy",
                new { controller = "Navigation", action = "LoginDummy" },
                new string[] { "myDamco.Controllers" }
            );

            // TODO: Needed? Only in case some users has old javascript cached - is not expected to be called by external sites.
            routes.MapRoute(
                "LoginDummy2-legacy",
                "Dashboard/LoginDummy2",
                new { controller = "Navigation", action = "LoginDummy2" },
                new string[] { "myDamco.Controllers" }
            );
            
            routes.MapRoute(
                "Portal-legacy",
                "Pages/Portal.aspx",
                new { controller = "StaticContent", action = "RedirectToFrontpage" },
                new string[] { "myDamco.Controllers" }
            );

            // Map all iframe-applications
            try
            {
                foreach (var navigation in db.Navigation.Where(x => x.Target == "Portal"))
                {
                    routes.MapRoute(
                        navigation.UId,
                        "Applications/" + navigation.UId + "/{*vars}", 
                        new { controller = "Dashboard", action = "Application", id = navigation.UId, vars = UrlParameter.Optional }
                    );

                    routes.MapRoute( // KV: Legacy routes
                        navigation.UId + "-legacy",
                        "Applications/" + navigation.UAMApplication + "/{*vars}",
                        new { controller = "Dashboard", action = "Application", id = navigation.UId, vars = UrlParameter.Optional }
                    );
               } 
                // Add routes for static content pages
                foreach (var page in db.Page)
                {
                    routes.MapRoute(
                        page.UId,
                        page.UId,
                        new { controller = "StaticContent", action = "Database", id = page.UId, area = "" },
                        new string[] { "myDamco.Controllers" }
                    );
                }
            }
            catch (EntityException)
            {
                // KV: No database. TODO: what do we do?!
            }
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },// Parameter defaults
                new[] { "myDamco.Controllers" } // This is so that only controllers from this namespace can use this default route (Not the ones in the areas). 
            ).DataTokens["UseNamespaceFallback"] = false; // Don't fall back to accepting controllers from areas if there were no matching controllers in the namespace given above (see: http://suhair.in/Blog/Aspnet-MVC-Areas-in-depth-Part-2 )
        }

        protected void Application_Start()
        {
            ModelMetadataProviders.Current = new CustomModelMetadataProvider();

            // Make sure we get our share of the pie on the shared server :)
            int logicalProcessors = System.Environment.ProcessorCount;
            ThreadPool.SetMaxThreads(512 * logicalProcessors, 512 * logicalProcessors);
            ThreadPool.SetMinThreads(100 * logicalProcessors, 100 * logicalProcessors);
            System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            // We only use razor so clear all other view engines
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            // Registere all routes and filters
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            if (ConfigurationManager.AppSettings["RouteDebugEnabled"] == "true")
                RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);

            int reloadRouteInteval;
            if (int.TryParse(ConfigurationManager.AppSettings["ReloadRouteInterval"], out reloadRouteInteval))
            {
                if (reloadRouteInteval > 0)
                {
                    reloadRouteTimer = new System.Timers.Timer(reloadRouteInteval * 1000);
                    reloadRouteTimer.Elapsed += new System.Timers.ElapsedEventHandler((x, y) => ReloadRoutes());
                    reloadRouteTimer.Enabled = true;
                }
            }
        }

        protected void Application_EndRequest()
        {
            if (Context.IsCustomErrorEnabled)
            {
                string action = "";
                switch (Context.Response.StatusCode)
                {
                    case 404:
                        action = "Error404";
                        break;
                    case 401:
                        action = "Error401";
                        break;
                }

                // User = null, if the path contains e.g. /bin/ or "+", as Application_AuthorizeRequest is not being called. (See 404.8 and 404.11 - the request filtering module denies these requests)
                if (!string.IsNullOrEmpty(action) && User != null && !string.IsNullOrEmpty(User.Identity.Name) && User.Identity.IsAuthenticated) // We can only display error pages when we know the user, as we have navigation in the masterpage.
                {
                    Response.Clear();

                    IController c = new myDamco.Controllers.StaticContentController();

                    RouteData rd = new RouteData();
                    rd.Values["controller"] = "StaticContent";
                    rd.Values["action"] = action;

                    var hcw = new HttpContextWrapper(Context);
                    var rc = new RequestContext(hcw, rd);

                    c.Execute(rc);
                }
            }
        }
    }
}