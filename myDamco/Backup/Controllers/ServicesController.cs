using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using Argotic.Syndication;
using myDamco;
using myDamco.Access.Authorization;
using myDamco.Database;
using myDamco.Models;
using myDamco.Utils;
using Newtonsoft.Json;
using ReportingClient = myDamco.Reporting.ReportingWebServicesSoapBindingClient;
using DocManClient = myDamco.DocumentManagement.RecentPouchWidgetWSSoapBindingClient;

namespace myDamco.Controllers
{
    public class ServiceException : Exception {
         public ServiceException(string msg, Exception ie) : base(msg, ie) { }
         public ServiceException(string msg) : base(msg) { }
    }

    // Note: The methods in ServicesController must never set cache headers on the Response, since 1) the ajax calls must never be cached, 2) the headers will leak onto the dashboard page and be sat onto 
    //       the dashboard page itself (resulting in deleted widgets reappearing, newly added widgets disappearing, movement of widgets being undone, announcements not being shown, etc, after a page load).
    public class ServicesController : Controller
    {
        // Do not initialize instance variables with expressions which can cast exceptions here. Do it in OnActionExecuting instead.
        private myDamcoEntities myDamcoDB;
        private Profile profile;
        private string[] roles;
        private bool isAdmin;
        //
        // GET: /Services/ELearning
        private struct swffile { public string file; public int id; public int height; public int width; public string resize; }

        // This method is executed immediately before an action method.
        // You should initialize all instance variables here (not in the constructor or directly by assignment), if your initialization expression can throw exceptions. This reason for this is to ensure 
        // that OnException will be called if an exception is thrown during initialization of the instance variables. OnException is not called, if an exception is thrown during object construction.
        // (Note: If the action-invoking process stops at an earlier point in its lifecycle, before this method is called, (f.e. by an error in OnAuthorization() if UAM is down), these 
        //        instance-variable values will not be set. This will have consequences for e.g. OnDestroy and OnException, so don't rely on instance variables having been initialized in those methods.)
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // initialize instance variables
            this.myDamcoDB = new myDamcoEntities();
            this.profile = myDamco.Profile.GetProfile();
            this.roles = Roles.GetRolesForUser(profile.LoginId);
            this.isAdmin = UAMUtils.UAMHasOperation("MYDAMCO", "ADMINISTRATION", roles);
            
            base.OnActionExecuting(filterContext);
        }

        // TODO: Make it possible to have more than one config per enviroment, fx /Services/ELearning/<config>
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public JsonResult ELearning(string widgetUID, bool waitForNonCached)
        {
            // Get Service configuraiton
            var conftemplate = new { folders = new [] { new { application = "", folder = "", id = 0, include = "", exclude = "", UAMApplication = "", UAMFunction = "", roles = new [] { new { orgName = "", orgType = "", roleName = "" } }, width = 0, height = 0, resize = "", cache = 0 } } };
            var configuration = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);

            // Build list of sfw files in the listed folders sorted by UAMApplication
            var apps = new Dictionary<string, List<swffile>>();
            
            // Becomes false, if at least one folder is not in the cache (used when waitForNonCached = false, to return null response in that case)
            bool allFoldersWereCached = true; 

            foreach(var conf in configuration.folders) {
                if (UAMUtils.UAMHasOperation(conf.UAMApplication, conf.UAMFunction, Roles.GetRolesForUser()) && 
                    (conf.roles == null || conf.roles.Any(
                        role => {
                            if (role.roleName != null)
                            {
                                Regex regex = new Regex(role.roleName.ToLower());
                                if (!regex.IsMatch(Profile.GetProfile().RoleName.ToLower())) return (false);
                            }
                            if (role.orgName != null)
                            {
                                Regex regex = new Regex(role.orgName.ToLower());
                                if (!regex.IsMatch(Profile.GetProfile().RoleOrganization.ToLower())) return (false);
                            }
                            if (role.orgType != null)
                            {
                                Regex regex = new Regex(role.orgType.ToLower());
                                if (!regex.IsMatch(Profile.GetProfile().RoleOrganizationObj.Type.ToLower())) return (false);
                            }
                            return (true);
                        })))
                {
                    // Clear cache if requested by user
                    if (Request.Params["refresh"] != null && Request.Params["refresh"] == "1")
                    {
                        HttpContext.Cache.Remove("elearning:" + conf.folder);
                    }

                    var swffiles = (List<swffile>)HttpRuntime.Cache["elearning:" + conf.folder];
                    if (swffiles == null)
                    {
                        allFoldersWereCached = false;
                        if (waitForNonCached)
                        {
                            if (Directory.Exists(conf.folder))
                            {
                                Regex regexExc;
                                Regex regexInc;
                                if (conf.exclude == null) regexExc = new Regex("");
                                else regexExc = new Regex(conf.exclude.ToLower());
                                if (conf.include == null) regexInc = new Regex("");
                                else regexInc = new Regex(conf.include.ToLower());

                                swffiles = Directory.GetFiles(conf.folder).Where(x => x.ToLower().EndsWith(".swf") && (conf.exclude == null || !regexExc.IsMatch(x.ToLower())) && (conf.include == null || regexInc.IsMatch(x.ToLower())))
                                    // TODO: Auto detect the width and height of the flash file: http://jayakrishnagudla.blogspot.dk/2009/12/how-to-get-width-and-height-of-swf-file.html
                                    .Select(x => new swffile { file = Path.GetFileNameWithoutExtension(x), id = conf.id, width = conf.width == 0 ? 1031 : conf.width, height = conf.height == 0 ? 804 : conf.height, resize = conf.resize })
                                    .ToList();
                                // Add folder to HTTP cache
                                HttpRuntime.Cache.Insert("elearning:" + conf.folder, swffiles, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                            }
                            else
                                throw new ServiceException("E-Learning server not found.");
                        }
                    }

                    if (swffiles != null)
                    {
                        // Add found files to the list under the application
                        if (apps.Keys.Contains(conf.application))
                        {
                            // Multiple folders defined for the same application.
                            apps[conf.application].AddRange(swffiles);
                        }
                        else
                        {
                            // Use cloned list so original object reference inserted into the cache will not be changed by later appends to the same application (see above).
                            List<swffile> tmp = new List<swffile>();
                            tmp.AddRange(swffiles);
                            apps.Add(conf.application, tmp);
                        }
                    }
                }
            }

            if (!allFoldersWereCached && !waitForNonCached)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(apps, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Services/ELearning/<APPLICATION>/<FILENAME>

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult ELearningShow(string widgetUID, string application, string name)
        {
            // Get Service configuraiton
            var conftemplate = new { folders = new[] { new { application = "", folder = "", UAMApplication = "", UAMFunction = "" } } };
            var configuration = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);

            foreach (var conf in configuration.folders.Where(f => f.application == application))
            {
                if (UAMUtils.UAMHasOperation(conf.UAMApplication, conf.UAMFunction, Roles.GetRolesForUser()))
                {
                    var filename = conf.folder + "\\" + name + ".swf";

                    if (System.IO.File.Exists(filename))
                    {
                        Response.ContentType = "application/x-shockwave-flash";
                        Response.WriteFile(filename);

                        Response.Flush();
                        Response.End();
                        return new EmptyResult();
                    }
                }
            }
            
            return Content("Gee, no file found");
        }

        // The below method effectively replaces the above method.
        // The primary aim of the below method was to enable a flash file to link relatively to other flash files. Originally the primary flash file
        // was loaded with the above method. Then a "Base" tag was used to redirect relative links to the below method. However, this did only work then
        // javascripting the link behavior to load the flash file into a dedicated iframe. It failed when the right clicked the link to open the flash
        // in a new window or tab. This was solved by also loading the main flash file with the below method thus automatically making the method base
        // for relative links. Also the method may relatively easy be changed also to load non flash files.
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult ELearningLoadFile(string widgetUID, string application, int id, string url)
        {
            // Get Service configuraiton
            var conftemplate = new { folders = new[] { new { application = "", folder = "", id = 0, UAMApplication = "", UAMFunction = "" } } };
            var configuration = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);

            foreach (var conf in configuration.folders.Where(f => f.application == application && f.id == id))
            {
                if (UAMUtils.UAMHasOperation(conf.UAMApplication, conf.UAMFunction, Roles.GetRolesForUser()))
                {
                    var filename = conf.folder + "\\" + url.Replace("/", "\\");

                    if (System.IO.File.Exists(filename))
                    {
                        Response.BufferOutput = true;
                        Response.AddHeader("Content-Length", new System.IO.FileInfo(filename).Length.ToString());
                        Response.ContentType = "application/x-shockwave-flash";
                        Response.WriteFile(filename);
                        Response.Flush();
                        Response.End();
                        return new EmptyResult();
                    }
                }
            }

            return Content("Gee, no file found");
        }

        //
        // GET: /Services/RecentPouches
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE, UAM:DOCUMENT_MANAGEMENT")]
        public ActionResult RecentPouches(string widgetUID, bool waitForNonCached)
        {
            string cacheKey = String.Format("RecentPouches:{0}:{1}", widgetUID, myDamco.Profile.GetProfile().LoginId + "-" + myDamco.Profile.GetProfile().RoleId);
            string res = HttpRuntime.Cache.Get(cacheKey) as string;

            if (res == null && !waitForNonCached)
                return new EmptyResult();

            if (res == null || Request.Params["refresh"] != null)
            {
                HttpContext.Cache.Remove(cacheKey);

                // Get service configuration
                var conftemplate = new { remoteaddress = "", cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);
                DocManClient docManClient = new DocManClient("RecentPouchWidgetWSSoapBinding", conf.remoteaddress);

                // Make service request
                DateTime startTime = DateTime.UtcNow;
                string response = docManClient.getReleasedPouchData(profile.LoginId, profile.RoleId.ToString());
                DateTime endTime = DateTime.UtcNow;

                XElement xml = XElement.Parse(response);
                PrepareXMLForJSONConversion(xml);
                string resCache = JsonConvert.SerializeObject(xml);
                res = resCache;

                // add the service response-time to the XML, if the user is an admin. (but don't put this altered version into the cache)
                if (xml != null && xml.Name == "widget" && isAdmin)
                {
                    xml.SetAttributeValue("response-time", (endTime - startTime).TotalSeconds.ToString("0.000"));
                    res = JsonConvert.SerializeObject(xml);
                }

                // Cache response
                HttpRuntime.Cache.Insert(cacheKey, resCache, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            // TODO: Use Json result Json()
            return Content(res, "application/json");
        }

        //
        // GET: /Services/RecentSearches
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE, UAM:REPORTING_TRACKTRACE")]
        public ActionResult RecentSearches(string widgetUID, bool waitForNonCached)
        {
            string cacheKey = String.Format("RecentSearches:{0}:{1}", widgetUID, myDamco.Profile.GetProfile().LoginId + "-" + myDamco.Profile.GetProfile().RoleId);
            string res = HttpRuntime.Cache.Get(cacheKey) as string;

            if (res == null && !waitForNonCached)
                return new EmptyResult();

            if (res == null || Request.Params["refresh"] != null)
            {
                HttpContext.Cache.Remove(cacheKey);

                // Get service configuration
                var conftemplate = new { remoteaddress = "", cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);
                var reportingClient = new ReportingClient("ReportingWebServicesSoapBinding", conf.remoteaddress);

                // Make service request
                DateTime startTime = DateTime.UtcNow;
                string response = reportingClient.GetRecentSearches("", profile.LoginId, (int)profile.RoleId);
                DateTime endTime = DateTime.UtcNow;

                XElement xml = XElement.Parse(response);
                PrepareXMLForJSONConversion(xml);
                string resCache = JsonConvert.SerializeObject(xml);
                res = resCache;

                // add the service response-time to the XML, if the user is an admin. (but don't put this altered version into the cache)
                if (xml != null && xml.Name == "widget" && isAdmin)
                {
                    xml.SetAttributeValue("response-time", (endTime - startTime).TotalSeconds.ToString("0.000"));
                    res = JsonConvert.SerializeObject(xml);
                }

                // Cache response
                HttpRuntime.Cache.Insert(cacheKey, resCache, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            // TODO: Use Json result Json()
            return Content(res, "application/json");
        }

        //
        // GET: /Services/RecentReports
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE, UAM:REPORTING")]
        public ActionResult RecentReports(string widgetUID, bool waitForNonCached)
        {
            string cacheKey = String.Format("RecentReports:{0}:{1}", widgetUID, myDamco.Profile.GetProfile().LoginId + "-" + myDamco.Profile.GetProfile().RoleId);
            string res = HttpRuntime.Cache.Get(cacheKey) as string;

            if (res == null && !waitForNonCached)
                return new EmptyResult();

            if (res == null || Request.Params["refresh"] != null)
            {
                HttpContext.Cache.Remove(cacheKey);

                // Get service configuration
                var conftemplate = new { remoteaddress = "", cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);
                var reportingClient = new ReportingClient("ReportingWebServicesSoapBinding", conf.remoteaddress);

                // Make service request
                DateTime startTime = DateTime.UtcNow;
                string response = reportingClient.GetRecentReports("", profile.LoginId);
                DateTime endTime = DateTime.UtcNow;
                
                XElement xml = XElement.Parse(response);
                PrepareXMLForJSONConversion(xml);
                string resCache = JsonConvert.SerializeObject(xml);
                res = resCache;

                // add the service response-time to the XML, if the user is an admin. (but don't put this altered version into the cache)
                if (xml != null && xml.Name == "widget" && isAdmin)
                {
                    xml.SetAttributeValue("response-time", (endTime - startTime).TotalSeconds.ToString("0.000"));
                    res = JsonConvert.SerializeObject(xml);
                }

                // Cache response
                HttpRuntime.Cache.Insert(cacheKey, resCache, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            // TODO: Use Json result Json()
            return Content(res, "application/json");
        }

        //
        // GET: /Services/ScheduleReports
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE, UAM:REPORTING")]
        public ActionResult ScheduledReports(string widgetUID, bool waitForNonCached)
        {
            string cacheKey = String.Format("ScheduledReports:{0}:{1}", widgetUID, myDamco.Profile.GetProfile().LoginId + "-" + myDamco.Profile.GetProfile().RoleId);
            string res = HttpRuntime.Cache.Get(cacheKey) as string;

            if (res == null && !waitForNonCached)
                return new EmptyResult();

            if (res == null || Request.Params["refresh"] != null)
            {
                HttpContext.Cache.Remove(cacheKey);

                // Get service configuration
                var conftemplate = new { remoteaddress = "", cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);
                var reportingClient = new ReportingClient("ReportingWebServicesSoapBinding", conf.remoteaddress);

                // Make service request
                //TODO: Check in old myDamco if there is a cache key and how it is used
                DateTime startTime = DateTime.UtcNow;
                string response = reportingClient.GetScheduleReports("", profile.LoginId);
                DateTime endTime = DateTime.UtcNow;
                
                XElement xml = XElement.Parse(response);
                PrepareXMLForJSONConversion(xml);
                string resCache = JsonConvert.SerializeObject(xml);
                res = resCache;

                // add the service response-time to the XML, if the user is an admin. (but don't put this altered version into the cache)
                if (xml != null && xml.Name == "widget" && isAdmin)
                {
                    xml.SetAttributeValue("response-time", (endTime - startTime).TotalSeconds.ToString("0.000"));
                    res = JsonConvert.SerializeObject(xml);
                }

                // Cache response
                HttpRuntime.Cache.Insert(cacheKey, resCache, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            // TODO: Use Json result Json()
            return Content(res, "application/json");
        }

        //
        // GET: /Services/WhoAmI
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult WhoAmI()
        {
            // TODO: Use Json result Json()
            return Content(JsonConvert.SerializeObject(profile.AsStringMap()), "application/json");
        }


        //
        // GET: /Services/DamcoNews
        //
        // Is only called when a feed has been selected - is not called to get the list of available feeds. The list of available feeds is available to the JavaScript from the Configuration field in the DB.
        // 
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult DamcoNews(string widgetUID, string selectedFeed, bool waitForNonCached)
        {
            // TODO: BUG: DUE TO THIS CACHE KEY NOT BEING PER USER (IT IS FOR ALL USERS), the category.showUAMApplication, category.showUAMFunction attributes does not work! (See GetDamcoNewsFeed())
            // TODO:      Unauthorized users will still get the feed from the cache, which other (authorized) users has inserted into it. 
            // TODO:      Should probably store the rights in the cache too, and check them here in this method instead (at the bottom of the method). Then we don't have to change the cache key.
            string cacheKey = String.Format("damconews:{0}:{1}", widgetUID, selectedFeed); 
            RssFeed feed = HttpRuntime.Cache.Get(cacheKey) as RssFeed;

            if (feed == null && !waitForNonCached)
                return new EmptyResult();

            // If nothing found in cache or if refresh parameter passed
            if (feed == null || Request.Params["refresh"] != null)
            {
                int categoryId; 
                if (!int.TryParse(selectedFeed, out categoryId)) throw new ServiceException("The news category could not be found.");
                
                HttpContext.Cache.Remove(cacheKey);
                DashboardController.ClearNewsPageExternalFeedCache(categoryId, HttpContext); // also clear cache of the news-page, so that the two caches are approximately in sync (maybe they should share cache?)

                var conftemplate = new { itemLimit = 0, cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);

                var limit = conf.itemLimit > 0 ? conf.itemLimit : 50;

                feed = GetDamcoNewsMixedFeed(categoryId, limit);

                // Cache feed
                HttpRuntime.Cache.Insert(cacheKey, feed, null, DateTime.UtcNow.AddSeconds(conf.cache != 0 ? conf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }
           
            // Convert RssFeed to a stream and then to a string
            string outString;
            using (var ms = new MemoryStream())
            {
                feed.Save(ms);
                ms.Position = 0;
                var sr = new StreamReader(ms);
                outString = sr.ReadToEnd();
            }

            return Content(outString, "text/xml");
        }

        //
        // GET: /Services/ExternalNews
        //
        // Is only called when a feed has been selected - is not called to get the list of available feeds. The list of available feeds is available to the JavaScript from the Configuration field in the DB.
        // 
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:USE")]
        public ActionResult ExternalNews(string widgetUID, string selectedFeed, bool waitForNonCached)
        {
            var conftemplate = new { feeds = new[] { new { id = "", url = "", cache = 0, timeout = 0, chartset = "", UAMApplication = "", UAMFunction = "" } } }; // NOTE: UAMApplication/UAMFunction is unused (+ the current cache key does not work for them)
            var configuration = JsonConvert.DeserializeAnonymousType(myDamcoDB.Widget.FirstOrDefault(w => w.UId == widgetUID).ServiceConfiguration, conftemplate);

            // Find the configuration for the feed that matches the feed id, among the feeds defined in the widget's ServiceConfiguration.
            var feedconf = configuration.feeds.FirstOrDefault(f => f.id == selectedFeed);
            if (feedconf != null)
            {
                // Get from cache if we have it
                string cacheKey = String.Format("externalnews:{0}:{1}", widgetUID, selectedFeed);
                var feeddata = (string)HttpRuntime.Cache[cacheKey];

                if (feeddata == null && !waitForNonCached)
                    return new EmptyResult();

                // Contact the external site, if needed
                bool refresh = Request.Params["refresh"] != null;
                if (feeddata == null || refresh)
                {
                    HttpContext.Cache.Remove(cacheKey);

                    feeddata = GetExternalNewsFeedXml(feedconf.url, feedconf.timeout, feedconf.chartset);

                    // Insert into cache. Default cache timeout to 15min.
                    if (feedconf.cache != -1)
                        HttpRuntime.Cache.Insert(cacheKey, feeddata, null, DateTime.UtcNow.AddSeconds(feedconf.cache != 0 ? feedconf.cache : 900), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                }

                // return rss feed data 
                return Content(feeddata, "text/xml");
            }
            else
            {
                throw new ServiceException("The requested feed could not be found on Damco's external feed list.");
            }
        }

        /* Gets both the local and external news-items in the news-category "categoryId", mixes the feeds together, and returns the mixed news-items as an RSSFeed object */
        private RssFeed GetDamcoNewsMixedFeed(int categoryId, int limit)
        {
            // Get the local feed from the DB
            RssFeed feed = GetDamcoNewsLocalFeed(categoryId, limit);

            // Mix external feeds into this local feed, if any external feeds are specified in the Configuration for this news category.
            var categoryConfTemplate = new { externalfeeds = new[] { new { url = "", timeout = 0, chartset = "", itemLimit = 0 } } };
            var categoryConf = JsonConvert.DeserializeAnonymousType(myDamcoDB.NewsCategory.FirstOrDefault(c => c.Id == categoryId).Configuration, categoryConfTemplate);

            var externalFeedsToMix = categoryConf == null ? null : categoryConf.externalfeeds;
            if (externalFeedsToMix != null)
            {
                foreach (var externalFeedInfo in externalFeedsToMix)
                {
                    RssFeed externalFeed = GetExternalNewsFeed(externalFeedInfo.url, externalFeedInfo.timeout, externalFeedInfo.chartset);

                    // Limit the shown items in the feed, if that is configured for the feed
                    if (externalFeedInfo.itemLimit > 0)
                        externalFeed.Channel.Items = externalFeed.Channel.Items.OrderByDescending(rssItem => rssItem.PublicationDate).Take(externalFeedInfo.itemLimit);

                    // Mix the two feeds into a third feed (mix the RssItems), and replace the items in the original feed with the items in this mixed feed
                    var mergedItems = (feed.Channel.Items).Concat(externalFeed.Channel.Items)
                        .OrderByDescending(rssItem => rssItem.PublicationDate)
                        .Take(limit);

                    feed.Channel.Items = mergedItems;
                }
            }

            return feed;
        }

        /* Gets the local news-items in the news-category "categoryId" from the DB, and returns these news-items as an RSSFeed object */
        private RssFeed GetDamcoNewsLocalFeed(int categoryId, int limit)
        {
            // Validate id and retrieve news category
            NewsCategory category = myDamcoDB.NewsCategory.FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                // Check authorization
                string requiredRole = String.Format("UAM:{0}:{1}", category.showUAMApplication, category.showUAMFunction);
                string[] roles = Roles.GetRolesForUser(myDamco.Profile.GetProfile().UserName);
                if (!roles.Any(r => r == requiredRole))
                {
                    throw new ServiceException("You do not have the required rights to view this news category.");
                }
            }
            else
            {
                throw new ServiceException("The news category could not be found.");
            }

            // Load news items from database
            IEnumerable<NewsItem> newsItems = myDamcoDB.NewsItem
                .Where(n => n.From < DateTime.UtcNow && (n.To > DateTime.UtcNow || n.To == null) && n.NewsCategory_Id == categoryId)
                .OrderByDescending(x => x.From)
                .Take(limit);

            // Get the myDamco application url
            Uri appUrl = new Uri(ControllerUtil.GetSitePathPath(false)); //Uri appUrl = new Uri(String.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Request.ApplicationPath));

            // Compose RSS feed
            RssFeed feed = new RssFeed();
            feed.Channel.Link = new Uri("http://localhost");
            feed.Channel.Title = category.Name;
            feed.Channel.Description = category.Description;
            foreach (NewsItem newsItem in newsItems)
            {
                RssItem rssItem = new RssItem();
                rssItem.Title = newsItem.Title;
                rssItem.Link = new Uri(String.Format("{0}/{1}", appUrl, "Dashboard/News/" + newsItem.Id));
                rssItem.Description = newsItem.Description;
                rssItem.PublicationDate = DateTime.SpecifyKind(newsItem.From, DateTimeKind.Utc); // The dates in the DB is in UTC, so we just have to set the Kind of the DateTimes to reflect this.
                feed.Channel.AddItem(rssItem);
            }

            return feed;
        }

        /* Fetches an RSS-feed from an external site and returns the feed as an RssFeed object */
        [NonAction]
        public static RssFeed GetExternalNewsFeed(string url, int timeout, string charset)
        {
            string feedXml = GetExternalNewsFeedXml(url, timeout, charset);

            try
            {
                using (Stream feedXmlStream = new MemoryStream(Encoding.UTF8.GetBytes(feedXml)))
                {
                    RssFeed feed = new RssFeed();
                    feed.Load(feedXmlStream);

                    // Convert the dates of all items from Local-Time to UTC, since RssFeed.Load() seems to convert them to Local-Time.
                    feed.Channel.Items.All(x => { x.PublicationDate = x.PublicationDate.ToUniversalTime(); return true; }); // .All() is used as .ForEach() here

                    return feed;
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException("The external feed did not respond correctly.", ex);
            }
        }

        /* Fetches an RSS-feed from an external site and returns the raw XML as a string */
        private static string GetExternalNewsFeedXml(string url, int timeout, string charset)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = timeout != 0 ? timeout * 1000 : 10000; // 10 sec timeout for getting the resource

            // Do request via proxy server (When we are inside Damco's network, we can only connect to the outside through their proxy-server)
            if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["RSSNewsProxy"]))
            {
                var proxy = new WebProxy(ConfigurationManager.AppSettings["RSSNewsProxy"]);
                request.Proxy = proxy;
                //request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            }

            // Support https
            if ("https" == new Uri(url).Scheme)
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((x, y, z, xyz) => true);
                request.PreAuthenticate = true;
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.AllowAutoRedirect = true;
            }

            // Get remote resource
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (Stream s = response.GetResponseStream())
                {
                    // Decode the stream with the charset defined in the feed or response charset
                    using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding(charset ?? response.CharacterSet)))
                    {
                        string feeddata = Regex.Replace(sr.ReadToEnd(), @"^\s*<\?xml.*\?>", "<?xml version=\"1.0\" encoding=\"utf-8\" ?>", RegexOptions.Singleline);
                        return feeddata;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException("The external feed did not respond correctly.", ex);
            }
        }

        /**
         * This method adresses a problem when converting from XML to JSON using JsonConvert.SerializeObject(...), for RecentPouchers, RecentSearches, RecentReports 
         * and ScheduledReports. 
         * 
         * In case there is only one <tr> in the xml, the converted JSON will be in a vastly different format than if there are multiple <tr> elements in the xml. 
         * In case of multiple <tr> elements, the generated tr-element in the JSON will be an array of <td> elements, but in case of 1 tr-element, there will be no array. 
         * The javascript code receiving the JSON does not expect this inconsistency. 
         * The exact same problem exists for <td> elements, in case there is only one <td> element in a row.
         * 
         * To solve the problem, attributes are added to the xml, before converting it to JSON, to force <tr> and <td> elements to always be converted into arrays, even 
         * if there is only one of them.
         * (see http://james.newtonking.com/projects/json/help/index.html?topic=html/ConvertingJSONandXML.htm )
         */
        private void PrepareXMLForJSONConversion(XElement xmlRoot)
        {
            if (xmlRoot == null) return; 

            XNamespace jsonNamespace = "http://james.newtonking.com/projects/json";
            xmlRoot.Add(new XAttribute(XNamespace.Xmlns + "json", jsonNamespace.NamespaceName)); // add namespace declaration to root XElement: http://msdn.microsoft.com/en-us/library/bb387075.aspx

            foreach (XElement trNode in xmlRoot.Descendants("tr").Union(xmlRoot.Descendants("td"))) // add attributes to each "tr" and "td" element
                trNode.Add(new XAttribute(jsonNamespace + "Array", "true"));
        }

        private DateTime GetCacheUtcExpiryDateTime(string cacheKey)
        {
            object cacheEntry = HttpRuntime.Cache.GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(HttpRuntime.Cache, new object[] { cacheKey, 1 });
            PropertyInfo utcExpiresProperty = cacheEntry.GetType().GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
            DateTime utcExpiresValue = (DateTime)utcExpiresProperty.GetValue(cacheEntry, null);

            return utcExpiresValue;
        }

        public String Error(int statuscode, string content, string mimetype = "text/plain", int wait = 0)
        {
            Thread.Sleep(wait * 1000);
            Response.StatusCode = statuscode;
            Response.ContentType = mimetype;
            return content;
        }

        // Note: Instance fields might not have been initialized yet when this method is called.
        protected override void OnException(ExceptionContext filterContext)
        {
            // KV: Child actions are invocations of Html.Action from elsewhere, and not real requests.
            // We rethrow the exception in those cases, to allow the same handling of exceptions for them irregardles of the value of customerrors.
            if (filterContext.IsChildAction)
                throw filterContext.Exception;

            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);

                string eType = exception.GetType().ToString();
                ErrorModel result = null;

                // If the exception is wrapped in a provider exception, find its inner exception and use that instead
                if (eType == "System.Configuration.Provider.ProviderException" && exception.InnerException != null)
                {
                    eType = exception.InnerException.GetType().ToString();
                    exception = exception.InnerException;
                }

                // Action specific errors
                string action = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
                switch (action)
                {
                    // Document Management
                    case "RecentPouches":
                        if (eType == "System.Xml.XmlException" || eType == "System.ServiceModel.FaultException" || eType == "System.ServiceModel.EndpointNotFoundException")
                        {
                            result = new ErrorModel("Internal Error (Document Management)", "Document Management did not respond correctly.");
                        }
                        break;
                    // Reporting
                    case "ScheduledReports":
                    case "RecentReports":
                    case "RecentSearches":
                        if (eType == "System.Xml.XmlException" || eType == "System.ServiceModel.FaultException" || eType == "System.ServiceModel.EndpointNotFoundException")
                        {
                            result = new ErrorModel("Internal Error (Reporting)", "Reporting did not respond correctly.");
                        }
                        break;
                }

                // Exception type specific errors
                switch (eType)
                {
                    case "Newtonsoft.Json.JsonReaderException":
                        result = new ErrorModel("Configuration error", "Configuration could not be parsed.", exception.Message);
                        break;
                    case "UAMSharp.UAMTimeoutException" :
                        result = new ErrorModel("Internal Error (UAM)", "UAM did not respond within the timeout limit.");
                        break;
                    case "UAMSharp.UAMNotFoundException" :
                        result = new ErrorModel("Internal Error (UAM)", "Connection refused by UAM.");
                        break;
                    case "UAMSharp.UAMResponseTooLargeException":
                        result = new ErrorModel("Internal Error (UAM)", "The response from UAM was larger than expected.");
                        break;
                    case "UAMSharp.UAMUserDoesNotExistException":
                        result = new ErrorModel("Internal Error (UAM)", "You have not yet been created in UAM. Please contact Damco to correct the problem.", exception.Message);
                        break;
                    case "UAMSharp.UAMRoleNotAssignedToUser":
                        result = new ErrorModel("Internal Error (UAM)", "You have changed to a role which you are not associated with in UAM.");
                        break;
                    case "UAMSharp.UAMApplicationNotAssignedToUser":
                        result = new ErrorModel("Internal Error (UAM)", "You tried accessing an application which you do not yet have access to in UAM.");
                        break;
                    case "myDamco.Controllers.ServiceException" :
                        result = new ErrorModel("Service failed", exception.Message, exception.InnerException != null ? exception.InnerException.Message : "");
                        break;
                    case "System.Data.EntityException":
                        result = new ErrorModel("Internal Error (Database)", "Error communicating with the database.");
                        break;
                    case "System.ServiceModel.CommunicationException":  
                    {
                        QuotaExceededException inner = exception.InnerException as QuotaExceededException; // Assumes if this happens, it is due to a service, since the case for UAM is handled by UAMResponseTooLargeException
                        if (inner != null)
                            result = new ErrorModel("Service error", "The response from the external service was larger than expected.", inner.Message);
                        break;
                    }
                }

                // Default
                if (result == null)
                {
                    result = new ErrorModel("Internal Error", "Sorry, an unxpected error occurred while processing your request.", exception.Message);
                }
                
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();

                // Include exception if admininistrator
                var detailedmessage = "";

                try
                {
                    var operations = Roles.GetRolesForUser(); // Get UAM operations APPLICATION:FUNCTION in users current role (does not get the list of roles!)
                    if (operations.Contains("UAM:MYDAMCO:ADMINISTRATION"))
                    {
                        detailedmessage = result.ErrorDescription ?? "";
                    }
                }
                catch (Exception)
                {
                    // ignore UAM exception (and assume user is not administrator)
                }

                var JsonRes = new { title = result.Title, description = result.Description, detailedmessage = detailedmessage };
                this.Json(JsonRes, JsonRequestBehavior.AllowGet).ExecuteResult(this.ControllerContext);
            }
        }

        // Note: Instance fields might not have been initialized yet when this method is called.
        protected override void Dispose(bool disposing)
        {
            if (myDamcoDB != null) // in case an error happened in the action-invoker process, before the instance variables got initialized. (f.e. in OnAuthorization)
                myDamcoDB.Dispose();
            base.Dispose(disposing);
        }
    }
}
