using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using myDamco.Database;
using ResourceHelper;

namespace myDamco.Areas.Administration.Controllers
{
    public class ServerManagementController : BaseAdminController
    {
        private myDamcoEntities myDamcoDB = new myDamcoEntities();

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Index()
        {
            ViewBag.Message = TempData["message"] as string;
            ViewBag.Success = TempData["success"] as bool? ?? false;
            return View();
        }

        // Delete the entire Elmah log from the database
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult DeleteElmahLog()
        {
            // Seems LINQ 2 SQL's approach executes one delete statement per row (slow), so we're executing the "raw" sql-delete statement instead. (also note that "truncate table" is much faster than "delete from" - "delete from" deletes one row at a time, which becomes too slow on very large tables)
            myDamcoDB.Database.ExecuteSqlCommand("TRUNCATE TABLE ELMAH_Error");

            TempData["message"] = "All ELMAH log entries have been deleted";
            TempData["success"] = true; 

            return RedirectToAction("Index");
        }

        // Clear the ResourceHelper's Javascript/CSS/Image cache
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult DeleteResourceHelperCache()
        {
            try
            {
                HtmlHelperExtensions.ClearCacheFolder(HttpContext.Server, new HtmlResources());
                TempData["message"] = "The cache has been cleared.";
                TempData["success"] = true; 
            }
            catch (Exception e)
            {
                TempData["message"] = "An error occurred while clearing the cache: "+e.Message+" ("+e.GetType()+")";
                TempData["success"] = false;
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
            return RedirectToAction("Index");
        }

        // Remove html tags and html encoded characters in newsItem-descriptions (copies the body text to the description text and removes tags).
        // This action method is intended to be run by manually entering the URL for it in the browser.
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Clean()
        {
            List<NewsItem> newsItems = myDamcoDB.NewsItem.ToList();

            foreach (NewsItem newsItem in newsItems)
            {
                newsItem.Description = HttpUtility.HtmlDecode(Regex.Replace(newsItem.Body, "<.*?>", string.Empty));
            }
            myDamcoDB.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (myDamcoDB != null)
                myDamcoDB.Dispose();
            base.Dispose(disposing);
        }
    }
}
