using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Transactions;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Argotic.Syndication;
using LDAPUtils;
using myDamco.Access.Authorization;
using myDamco.Config;
using myDamco.Database;
using myDamco.Models;
using myDamco.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAMSharp;
using myDamco;
//using System.IO;

/* TODO: Enable static and dynamic compression of pages */
/* TODO: Caching  : http://www.dotnetperls.com/cache */

/* Links
 * RAW SQL from EntityFramwork: http://www.nakov.com/blog/2011/01/12/native-sql-queries-in-entity-framework/
 * 
http://www.vbforums.com/showthread.php?t=568034
*/

// TODO: Gracefully handle when we get a ADFS redirect from a $.getJSON call http://stackoverflow.com/questions/1462919/form-that-makes-browser-redirect-when-accessed-by-either-a-regular-form-submit-o/1463206#1463206

// TODO: Version widget and service configuration 
// TODO: Find out how we authenticated, fx if we used the debug module http://stackoverflow.com/questions/91831/detecting-web-config-authentication-mode
// TODO: Look into how we store sessions, we need to consider that the site will be load balanced so we need to support the session on two servers at once
// TODO: Make pretty access denied page when if the user does not have fx. UAM:MYDAMCO:USE
// TODO: Look at caching http://leoncullens.nl/post/2011/12/24/Essential-guide-to-ASPNET-MVC3-performance.aspx

// TODO: Log java script errors: http://stackoverflow.com/questions/779644/how-to-catch-javascript-exceptions-errors-to-log-them-on-server

namespace myDamco.Controllers
{
    public class DashboardController : Controller
    {
        private myDamcoEntities myDamcoDB = new myDamcoEntities();
        private Profile profile = myDamco.Profile.GetProfile();

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Application(string id, string vars)
        {
            var navigation = myDamcoDB.Navigation.SingleOrDefault(x => x.UId == id);

            if (navigation == null)
            {
                Response.StatusCode = 404;
                return Content("");
            }

            if (!string.IsNullOrWhiteSpace(vars))
                ViewBag.OverrideURL = navigation.Url + vars + Request.Url.Query;

            return View(navigation);
        }


        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult WidgetLink(string appUID, string widgetUID, string var)
        {

            var app = myDamcoDB.Navigation.Single(x => x.UId == appUID);
            var widget = myDamcoDB.Widget.Single(x => x.UId == widgetUID);

            dynamic settings = JObject.Parse(widget.Configuration);
            var targetUrl = (string)settings.targeturl;
            var requesturl = var + Request.Url.Query;

            var fullurl = targetUrl + requesturl;

            if (app.Target == "External")
            {
                return Redirect(fullurl);
            }
            else
            {
                ViewBag.OverrideURL = fullurl;
                return View("Application", app);
            }
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult News(int id, bool showArchived = false)
        {
            var item = myDamcoDB.NewsItem.Single(x => x.Id == id);
            var items = GetNewsPageItems(item.NewsCategory.Id, showArchived);

            return View(new NewsPageModel { menuNewsItems = items.ToList(), newsItem = new NewsPageItem(item, Request), newsCategory = item.NewsCategory, showArchivedItemsInMenu = showArchived });
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")] // Show the news page, without going to a specific news item, but instead only going to a specific news category
        public ActionResult NewsFeed(int id, bool showArchived = false)
        {
            int categoryId = id;
            var items = GetNewsPageItems(categoryId, showArchived);
            var category = myDamcoDB.NewsCategory.Single(x => x.Id == categoryId);

            return View("News", new NewsPageModel { menuNewsItems = items.ToList(), newsItem = null, newsCategory = category, showArchivedItemsInMenu = showArchived });
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult NewsExternal(string id, bool showArchived = false)
        {
            // TODO: Handle exceptions (show a blank page)? (for substring, etc) Also, if the feed is no longer in the rss-feed.
            // decode id
            Tuple<int, DateTime, string> decodedId = NewsPageItem.DecodeExternalNewsItemId(id);
            int categoryId = decodedId.Item1;
            DateTime time = decodedId.Item2;
            string title = decodedId.Item3;


            var items = GetNewsPageItems(categoryId, showArchived);
            var category = myDamcoDB.NewsCategory.Single(x => x.Id == categoryId);

            var newsItem = items.FirstOrDefault(x => x.Title == title && x.From == time && x.External); // items.SingleOrDefault(x => x.Title == title && x.From == time && x.External);

            return View("News", new NewsPageModel { menuNewsItems = items.ToList(), newsItem = newsItem, newsCategory = category, showArchivedItemsInMenu = showArchived });
        }

        // This is to update the newspage cache at the same time as when the cache of the news widget is updated, so that the two will be in sync (approximately).
        [NonAction]
        public static void ClearNewsPageExternalFeedCache(int categoryId, HttpContextBase httpContext)
        {
            string cacheKey1 = String.Format("getnewspageitemsExternal:{0}:{1}", categoryId, true);
            string cacheKey2 = String.Format("getnewspageitemsExternal:{0}:{1}", categoryId, false);
            httpContext.Cache.Remove(cacheKey1);
            httpContext.Cache.Remove(cacheKey2);
        }

        // Gets local feeds from the DB as well as any external feeds associated with the given categoryId in that widget's ServiceConfiguration.
        // TODO: Refactor. Merge this with the mixing code in ServicesController.DamcoNews/GetDamcoNewsMixedFeed, so that this mixing is only done in one place. (Challenge: Must support archive + send extra info (Body + External (bool) + To (+Id?)) along - can f.e. add an "extension" to the RssItems or a subclass). Also merge caching mechanism (so they're always in sync)?
        private IList<NewsPageItem> GetNewsPageItems(int categoryId, bool showArchived)
        {
            // Get NewsItems in this NewsCategory from the DB 
            var items = myDamcoDB.NewsItem.Where(x => x.NewsCategory_Id == categoryId).AsQueryable();
            if (!showArchived)
                items = items.Where(x => (x.To == null || x.To > DateTime.UtcNow) && x.From < DateTime.UtcNow);
            items = items.OrderByDescending(x => x.From);

            var itemsList = items.ToList().Select(x => new NewsPageItem(x, Request)).ToList();

            // Get any external feeds associated with this NewsCategory as well. (Cached to avoid sending requests to external site on each newspage reload + sync'ed with widget cache) 
            string cacheKey = String.Format("getnewspageitemsExternal:{0}:{1}", categoryId, showArchived);
            var externalItems = HttpRuntime.Cache.Get(cacheKey) as List<NewsPageItem>;
            if (externalItems == null)
            {
                externalItems = new List<NewsPageItem>();

                var categoryConfTemplate = new { cache = 0, externalfeeds = new[] { new { url = "", timeout = 0, chartset = "", itemLimit = 0 } } };
                var categoryConf = JsonConvert.DeserializeAnonymousType(myDamcoDB.NewsCategory.FirstOrDefault(c => c.Id == categoryId).Configuration, categoryConfTemplate);

                // Mix external feeds into this local feed, if any external feeds are specified in the ServiceConfiguration for this news category.
                var externalFeedsToMix = categoryConf == null ? null : categoryConf.externalfeeds;
                if (externalFeedsToMix != null)
                {
                    foreach (var externalFeedInfo in externalFeedsToMix)
                    {
                        RssFeed externalFeed = ServicesController.GetExternalNewsFeed(externalFeedInfo.url, externalFeedInfo.timeout, externalFeedInfo.chartset);

                        // Convert from RssItem into NewsPageItems
                        IEnumerable<NewsPageItem> feedItems = externalFeed.Channel.Items
                            .Select(rssItem => new NewsPageItem(rssItem, categoryId, Request))
                            .OrderByDescending(x => x.From);
                        if (!showArchived && externalFeedInfo.itemLimit > 0) feedItems = feedItems.Take(externalFeedInfo.itemLimit); // Or limit by date?
                        externalItems.AddRange(feedItems);
                    }
                }

                HttpRuntime.Cache.Insert(cacheKey, externalItems, null, DateTime.UtcNow.AddSeconds(categoryConf != null && categoryConf.cache != 0 ? categoryConf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            itemsList.AddRange(externalItems);
            itemsList = itemsList.OrderByDescending(x => x.From).ToList(); // reorder after mixing the feeds

            return itemsList;
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Index()
        {
            //string strFilePath = @"C:\\log\CS_Log.txt";
            //using (StreamWriter sw = new StreamWriter(strFilePath, false))
            //{
            //    sw.WriteLine("_______________Dashboard Controller page__________________________");
            //    // sw.WriteLine("Roles : " + operations.Length);
            //    sw.WriteLine("profile details: loginID " + profile.LoginId);
            //    sw.WriteLine("profile details: RoleId " + profile.RoleId);
            //    sw.WriteLine("profile details: UserName " + profile.UserName);
            //    sw.WriteLine("Roles.Application : " + String.Join(",", Roles.GetRolesForUser(profile.UserName)));
            //    sw.WriteLine("_______________Dashboard Controller end__________________________");
            //}
            var model = new DashboardModel();

            // TODO: Add role information from ADFS or session?
            //changes done by Santhi

            var operations = Roles.GetRolesForUser(profile.UserName);
            //var operations = Roles.GetRolesForUser(); // Get UAM operations APPLICATION:FUNCTION in users current role (does not get the list of roles!)

            // Get widgets as dictionary with id as key
            model.Widgets = myDamcoDB.Widget.ToList()
                .Where(w => UAMUtils.UAMHasOperation(w.UAMApplication, w.UAMFunction, operations) && !w.Disabled)
                .AsEnumerable()
                .ToDictionary(w => w.Id, w => w);

            // Get widget instances sorted by DashboardColumn and DashboardPriority
            var widgetids = model.Widgets.Keys.ToList<int>(); // Give a List<int> so linq can convert to IN (...)
            model.WidgetInstances = myDamcoDB.WidgetInstance
                .Where(wi => wi.Login == profile.LoginId && wi.Role == (int)profile.RoleId && widgetids.Contains(wi.Widget_Id))
                .OrderBy(w => w.DashboardColumn)
                .ThenBy(w => w.DashboardPriority)
                .ToList();

            // Add a Welcome widget if the user don't have a widget
            if (model.WidgetInstances.Count == 0)
            {
                var templateWidgetInstances = AddDashboardTemplateWidgetsToDashboardIfExists().OrderBy(w => w.DashboardColumn).ThenBy(w => w.DashboardPriority).ToList();
                if (templateWidgetInstances.Count != 0)
                    model.WidgetInstances.AddRange(templateWidgetInstances);
                else
                {
                    var welcomeInstance = AddWelcomeWidgetToDashboard();
                    model.WidgetInstances.Add(welcomeInstance);
                }
            }

            // Handle back button //TODO: document
            if (Request.Cookies["invalidatebfcache"] != null)
            {
                var c = new HttpCookie("invalidatebfcache");
                c.Expires = new DateTime(1970, 1, 1, 0, 0, 1); // Set to 1
                Response.Cookies.Add(c);
            }

            var announcement = myDamcoDB.NewsItem
                .Where(x => x.NewsCategory_Id == 1 && x.From <= DateTime.UtcNow && (x.To == null || DateTime.UtcNow <= x.To))
                .OrderByDescending(x => x.From)
                .FirstOrDefault();

            model.announcement = announcement;

            // Put the list of widget-instances into the ViewBag so that _Piwik.cshtml has access to them (avoids having to read them again from the database + duplicate code). 
            ViewBag.WidgetInstances = model.WidgetInstances;

            return View(model);
        }

        private WidgetInstance AddWelcomeWidgetToDashboard()
        {
            var welcomeWidget = myDamcoDB.Widget.Single(x => x.Title == "Welcome");

            var welcomeInstance = new WidgetInstance()
            {
                Login = profile.LoginId,
                Role = (int)profile.RoleId,
                Widget_Id = welcomeWidget.Id,
                Configuration = "{}"
            };

            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                myDamcoDB.WidgetInstance.Add(welcomeInstance);
                myDamcoDB.SaveChanges(); // to generate an Id for the WidgetInstance

                var welcomeInstanceHistory = CreateWidgetInstanceHistoryObject(welcomeInstance);
                myDamcoDB.WidgetInstanceHistory.Add(welcomeInstanceHistory);
                myDamcoDB.SaveChanges();

                transaction.Complete();
            }

            return welcomeInstance;
        }

        public const string DASHBOARD_TEMPLATE_USERNAME = "/TemplateUser/"; // TODO: Some name impossible to get from UAM. TODO: Move this constant somewhere else.

        // Adds the widgets from a dashboard template to this users dashboard, if a dashboard template exists for the users current role. 
        // Returns the list of widgets added. Returns empty list if none added and if there is no dashboard template for this role.
        private List<WidgetInstance> AddDashboardTemplateWidgetsToDashboardIfExists()
        {
            // get dashboard-template widgets for this role, if any
            var templateWidgetInstances = myDamcoDB.WidgetInstance.Where(x => x.Login == DASHBOARD_TEMPLATE_USERNAME && x.Role == profile.RoleId).ToList();
            if (templateWidgetInstances.Count == 0) return templateWidgetInstances;

            // Create copies of these widget-instance objects which belongs the current user instead of to the template user
            var widgetInstances = new List<WidgetInstance>();
            foreach (var widgetInstance in templateWidgetInstances)
            {
                widgetInstances.Add(new WidgetInstance()
                {
                    Login = profile.LoginId,
                    Role = (int)profile.RoleId,
                    Widget_Id = widgetInstance.Widget_Id,
                    Configuration = widgetInstance.Configuration,
                    DashboardColumn = widgetInstance.DashboardColumn,
                    DashboardPriority = widgetInstance.DashboardPriority,
                    Title = widgetInstance.Title
                });
            }

            // Insert into the database and in the history table (And create their Ids).
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                foreach (var widgetInstance in widgetInstances)
                {
                    myDamcoDB.WidgetInstance.Add(widgetInstance);
                    myDamcoDB.SaveChanges(); // to generate an Id for the WidgetInstance

                    var widgetInstanceHistory = CreateWidgetInstanceHistoryObject(widgetInstance);
                    myDamcoDB.WidgetInstanceHistory.Add(widgetInstanceHistory);
                    myDamcoDB.SaveChanges();
                }
                transaction.Complete();
            }

            return widgetInstances;
        }

        //TODO: Make this work with ADFS logout
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            // TODO: Do something diffrent when it's for ADFS where we redirect back to ADFS server to invalidate our session there
            return RedirectToAction("Index", new { uamrole = Request.Params["uamrole"] });
        }

        [Authorize(Roles = "DONTHAVETHISROLE")]
        public ActionResult NoAccess()
        {
            return Content(
               "Will never be shown"
            );
        }


        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Account()
        {
            return View(new ChangePasswordModel());
        }

        //
        // GET: /Services/ChangeRole
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult UAMChangeRole(int id)
        {
            UAMClient uam = new UAMClient();
            uam.EnableWebCaching(HttpContext.Cache);
            uam.ChangeRole(profile.LoginId, id);
            return Content("window.location.reload(true);", "text/javascript"); // KV: Important that valid Javascript is returned by this service. It should reload the window.
        }

        //
        // GET: /Account/ChangePassword
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult Account(ChangePasswordModel model)
        {
            ViewBag.ChangePasswordSuccess = false;
            var ADAMUrl = WebConfigurationManager.ConnectionStrings["ADAM"].ConnectionString;
            if (ModelState.IsValid)
            {
                try
                {
                    if (LDAPAccount.ChangePassword(ADAMUrl, profile.LoginId, model.OldPassword, model.NewPassword))
                    {
                        ViewBag.Message = " Your password has been changed successfully";
                        ViewBag.ChangePasswordSuccess = true;
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ModelState.AddModelError("", "URL:" + ADAMUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public JsonResult AddWidget(int widgetId, int column)
        {
            var widget = myDamcoDB.Widget.Single(x => x.Id == widgetId);

            // Check if user actually has access to this widget
            var operations = Roles.GetRolesForUser(profile.UserName);
            // var operations = Roles.GetRolesForUser(); // Get UAM operations APPLICATION:FUNCTION in users current role (does not get the list of roles!) TODO: Hvad var det nu med GetRolesForUser()?
            if (!UAMUtils.UAMHasOperation(widget.UAMApplication, widget.UAMFunction, operations))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException("User tried adding a widget (" + widgetId + " = " + widget.Title + ") to his dashboard which he does not have access to.")); // log the error (it is unexpected, unless a user calls the action method manually)
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Response.TrySkipIisCustomErrors = true;
                return Json(new ErrorModel("Widget could not be added", "Your do not have access to this widget in your current role. You probably changed your role in another window."), JsonRequestBehavior.AllowGet);
            }

            // Add widget instance to DB
            var widgetInstance = new WidgetInstance()
            {
                Login = profile.LoginId,
                Role = (int)profile.RoleId,
                Widget_Id = widgetId,
                DashboardColumn = (byte)column,
                Configuration = "{}"
            };

            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                myDamcoDB.WidgetInstance.Add(widgetInstance);
                myDamcoDB.SaveChanges(); // to generate an Id for the WidgetInstance

                // Add widget instance history to DB
                var widgetInstanceHistory = CreateWidgetInstanceHistoryObject(widgetInstance);
                myDamcoDB.WidgetInstanceHistory.Add(widgetInstanceHistory);
                myDamcoDB.SaveChanges();

                transaction.Complete();
            }

            var model = new WidgetModel(widget, widgetInstance);

            var viewdata = ControllerUtil.RenderRazorViewToString("_WidgetInstance", model, this.ControllerContext, this.ViewData, this.TempData);

            var returndata = new { id = widgetInstance.Id, widgetTemplateName = widget.Template, data = viewdata };

            return Json(returndata, JsonRequestBehavior.AllowGet);
        }

        // Create a history object for a WidgetInstance object. The given WidgetInstance must have a valid "Id" (an Id which corresponds to the database)
        private WidgetInstanceHistory CreateWidgetInstanceHistoryObject(WidgetInstance widgetInstance)
        {
            if (widgetInstance.Id == 0) throw new ArgumentException("The WidgetInstance had Id = 0. It must have the proper Id from the database.");

            return new WidgetInstanceHistory()
            {
                WidgetInstance_Id = widgetInstance.Id, // must be set to its value in the database
                Login = widgetInstance.Login,
                Role = widgetInstance.Role,
                Widget_Id = widgetInstance.Widget_Id,
                AddTime = DateTime.UtcNow,
                DeleteTime = null
            };
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult RemoveWidgetInstance(int id)
        {
            // KV: TODO: Add some failure detection.
            // Remove from widget instance table
            var widget = myDamcoDB.WidgetInstance.SingleOrDefault(x => x.Id == id);

            if (widget == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Response.TrySkipIisCustomErrors = true;
                return Json(new ErrorModel("Widget could not be deleted", "Widget does not exists. You probably already deleted it in another window."), JsonRequestBehavior.AllowGet);
            }

            myDamcoDB.WidgetInstance.Remove(widget);

            // Set the deleteTime in the history table (might not exist there, if it is an old widget made before we added the history table)
            var widgetHistory = myDamcoDB.WidgetInstanceHistory.SingleOrDefault(x => x.WidgetInstance_Id == id);
            if (widgetHistory != null)
                widgetHistory.DeleteTime = DateTime.UtcNow;

            myDamcoDB.SaveChanges();

            return Content("Success");
        }

        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult SetWidgetOrder(byte dashboardColumn, List<int> widgetList)
        {
            var wis = myDamcoDB.WidgetInstance
                .Where(w =>
                    (widgetList.Contains(w.Id) || w.DashboardColumn == dashboardColumn)
                    && w.Login == profile.LoginId
                    && w.Role == profile.RoleId)
                .ToDictionary(w => w.Id);

            for (int i = 0; i < widgetList.Count; i++)
            {
                int id = widgetList[i];
                if (wis.ContainsKey(id) && (wis[id].DashboardColumn != dashboardColumn || i != wis[id].DashboardPriority))
                {
                    wis[id].DashboardColumn = dashboardColumn;
                    wis[id].DashboardPriority = (byte)i;
                }
            }

            myDamcoDB.SaveChanges();
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult SetWidgetTitle(int id, string title)
        {
            var wi = myDamcoDB.WidgetInstance.First(w => w.Id == id && w.Login == profile.LoginId && w.Role == profile.RoleId);
            if (wi != null)
            {
                // Empty string or just spaces resets to default Title inherited from widget
                if (String.IsNullOrEmpty(title))
                {
                    title = null;
                }

                wi.Title = title;
                myDamcoDB.SaveChanges();
            }
            else
            {
                throw new Exception("Id was not found");
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult SetWidgetConfiguration(int id, string configuration)
        {
            if (configuration != null)
            {
                // Parse configuration throw exception if that fails
                JsonConvert.DeserializeObject(configuration);

                var wi = myDamcoDB.WidgetInstance.First(w => w.Id == id && w.Login == profile.LoginId && w.Role == (int)profile.RoleId);
                if (wi != null)
                {
                    wi.Configuration = configuration;
                    myDamcoDB.SaveChanges();
                }
                else
                {
                    throw new Exception("Id was not found");
                }
            }
            else
            {
                throw new Exception("Configuration was not set");
            }

            return null;
        }

        [HttpPost]
        [ValidateInput(false)] // allows < > characters in eg. the stacktrace (and any html tags, such as <script>)
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult LogClientException(string msg, string url, int? line, int? column, string stacktrace)
        {
            if (Settings.Logging.ClientLoggingEnabled)
            {
                var exception = new JavaScriptException("\nmsg: " + msg + "\nurl: " + url + "\nline: " + line + "\ncolumn: " + column + "\nstacktrace:\n" + stacktrace);
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
            }
            return null;
        }

        private class JavaScriptException : Exception { public JavaScriptException(string message) : base(message) { } }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);

                bool page = false;
                bool simple;

                string action = filterContext.RouteData.Values["action"].ToString();
                switch (action)
                {
                    case "Index":
                    case "News":
                    case "NewsFeed":
                    case "NewsExternal":
                    case "Status":
                    case "Account":
                        page = true;
                        break;
                }

                // If the exception is wrapped in a provider exception, find its inner exception and use that instead
                if (exception.GetType().ToString() == "System.Configuration.Provider.ProviderException" && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }

                // Exception type specific errors
                ErrorModel result = ConvertExceptionToErrorModel(exception, out simple);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();

                if (page)
                {
                    IController c = new StaticContentController();
                    RouteData rd = new RouteData();
                    rd.Values["controller"] = "StaticContent";
                    rd.Values["action"] = "CustomError";
                    rd.Values["model"] = result;
                    rd.Values["simple"] = simple;
                    RequestContext rc = new RequestContext(filterContext.HttpContext, rd);

                    try
                    {
                        c.Execute(rc);
                    }
                    catch (Exception) // Down-grade to generic static page if something goes wrong
                    {
                        rd.Values["action"] = "ErrorSimple";
                        c = new StaticContentController();
                        c.Execute(rc);
                    }
                }
                else
                {
                    this.Json(result, JsonRequestBehavior.AllowGet).ExecuteResult(this.ControllerContext);
                }
            }
        }

        // Converts an Exception to an ErrorModel for use in OnException-methods. (Some types of exceptions are given a specific custom message in the ErrorModel)
        // This method is also called from NavigationController, at the time of writing, since they both need to (more or less) handle the same errors.
        [NonAction]
        public static ErrorModel ConvertExceptionToErrorModel(Exception exception, out bool simple)
        {
            ErrorModel result;
            simple = false;

            // Exception type specific errors
            string eType = exception.GetType().ToString();
            switch (eType)
            {
                case "UAMSharp.UAMNotFoundException":
                    result = new ErrorModel("Internal Error (UAM)", "UAM did not respond.");
                    simple = true;
                    break;
                case "UAMSharp.UAMTimeoutException":
                    result = new ErrorModel("Internal Error (UAM)", "UAM did not respond within the timeout limit.");
                    break;
                case "UAMSharp.UAMConnectionReset":
                    result = new ErrorModel("Internal Error (UAM)", "Connection refused by UAM.");
                    break;
                case "UAMSharp.UAMResponseTooLargeException":
                    result = new ErrorModel("Internal Error (UAM)", "The response from UAM was larger than expected.");
                    simple = true;
                    break;
                case "UAMSharp.UAMUserDoesNotExistException":
                    result = new ErrorModel("Internal Error (UAM)", "You have not yet been created in UAM. Please contact Damco to correct the problem.");
                    simple = true;
                    break;
                case "UAMSharp.UAMRoleNotAssignedToUser":
                    result = new ErrorModel("Internal Error (UAM)", "You have changed to a role which you are not associated with in UAM."); // TODO: Can we recover and change back to a legal role? (should not be done here - do in change role code)
                    simple = true;
                    break;
                case "UAMSharp.UAMApplicationNotAssignedToUser":
                    result = new ErrorModel("Internal Error (UAM)", "You tried accessing an application which you do not yet have access to in UAM."); // TODO: Write application name?
                    simple = true;
                    break;
                case "myDamco.Controllers.ServiceException":
                    result = new ErrorModel("Service failed", exception.Message, exception.InnerException != null ? exception.InnerException.Message : "");
                    break;
                case "System.Data.EntityException":
                    result = new ErrorModel("Internal Error (Database)", "Error communicating with the database.");
                    simple = true;
                    break;
                case "System.ServiceModel.CommunicationException":
                    {
                        QuotaExceededException inner = exception.InnerException as QuotaExceededException;
                        if (inner != null)
                        {
                            result = new ErrorModel("Service error", "The returned response was larger than expected.", inner.Message);
                            simple = true;
                            break;
                        }
                        goto default;
                    }
                default:
                    result = new ErrorModel("Internal Error", "An unexpected error occured.");
                    break;
            }

            return result;
        }

        [NonAction]
        public static ErrorModel ConvertExceptionToErrorModel(Exception exception)
        {
            bool ignore;
            return ConvertExceptionToErrorModel(exception, out ignore);
        }

        protected override void Dispose(bool disposing)
        {
            myDamcoDB.Dispose();
            base.Dispose(disposing);
        }
    }
}
