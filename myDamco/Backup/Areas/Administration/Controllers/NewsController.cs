using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using myDamco.Areas.Administration.Models;
using myDamco.Code.Utils;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class NewsController : BaseAdminController
    {
        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/News/
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:EDITOR")]
        public ActionResult Index()
        {
            var UAMApps =   db.Downtime.Select(x => x.UAMApplication).Union(
                            db.Navigation.Select(x => x.UAMApplication)).Union(
                            db.Widget.Select(x => x.UAMApplication))
                            .AsEnumerable();

            var UAMFuncs = db.Downtime.Select(x => x.UAMFunction).Union(
                            db.Navigation.Select(x => x.UAMFunction)).Union(
                            db.Widget.Select(x => x.UAMFunction))
                            .AsEnumerable();

            // The categories which the user is authorized to edit
            string[] operations = Roles.GetRolesForUser(); // Get UAM operations APPLICATION:FUNCTION in users current role (does not get the list of roles!)
            var categories = db.NewsCategory.Where(
                nc => operations.Contains("UAM:" + nc.editUAMApplication + ":" + nc.editUAMFunction)) // TODO: Do more effeciently, note that you can't use string.Format
                .AsEnumerable();

            ViewBag.IsAdmin = operations.Contains("UAM:MYDAMCO:ADMINISTRATION");

            return View(new NewsModel() { UAMApps = UAMApps, UAMFuncs = UAMFuncs, Categories = categories});
        }

        [HttpPost]
        [ValidateInput(false)] // KV: TODO: Figure out how to do this only for a single parameter
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:EDITOR")]
        public JsonResult UpdateNewsItem(NewsItem item)
        {
            var dbItem = db.NewsItem.Single(x => x.Id == item.Id);

            // Check authorization
            string reqRole = string.Format("UAM:{0}:{1}", dbItem.NewsCategory.editUAMApplication, dbItem.NewsCategory.editUAMFunction);
            if (!Roles.GetRolesForUser().Contains(reqRole)) throw new UnauthorizedAccessException("Current user and/or role lacks authorization to do this");

            dbItem.Title = item.Title;
            dbItem.From = item.From;
            dbItem.To = item.To;
            dbItem.Description = item.Description;
            dbItem.Body = item.Body;
            dbItem.UpdatedAt = DateTime.UtcNow;
            dbItem.UpdatedBy = myDamco.Profile.GetProfile().LoginId; // (ok to crash if getprofile returns null)


            foreach (var it in dbItem.Downtime.ToList()) // KV: Important to enumerate before we start iterating and deleting. Do no delete "toList()"
            {
                db.Entry(it).State = EntityState.Deleted;
            }

            foreach (var it in item.Downtime)
            {
                it.NewsItem_Id = dbItem.Id;
                it.UAMApplication = it.UAMApplication ?? "";
                it.UAMFunction = it.UAMFunction ?? "";
                it.Message = it.Message ?? "";
                db.Downtime.Add(it);
            }

            db.SaveChanges();

            var json = new { updatedBy = dbItem.UpdatedBy, updatedAt = TimeUtil.DateTimeToUnixTimestampMiliseconds(dbItem.UpdatedAt) };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)] // KV: TODO: Figure out how to do this only for a single parameter
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:EDITOR")]
        public JsonResult InsertNewsItem(NewsItem newItem)
        {
            var item = new NewsItem();

            // Check authorization
            var category = db.NewsCategory.Single(nc => nc.Id == newItem.NewsCategory_Id);
            string reqRole = string.Format("UAM:{0}:{1}", category.editUAMApplication, category.editUAMFunction);
            if (!Roles.GetRolesForUser().Contains(reqRole)) throw new UnauthorizedAccessException("Current user and/or role lacks authorization to do this");

            item.NewsCategory_Id = newItem.NewsCategory_Id;
            item.Title = newItem.Title;
            item.From = newItem.From;
            item.To = newItem.To;
            item.Description = newItem.Description;
            item.Body = newItem.Body;
            item.CreatedAt = DateTime.UtcNow;
            item.CreatedBy = myDamco.Profile.GetProfile().LoginId; // (ok to crash if getprofile returns null)

            db.NewsItem.Add(item);

            foreach (var it in newItem.Downtime)
            {
                it.NewsItem_Id = item.Id;
                it.UAMApplication = it.UAMApplication ?? "";
                it.UAMFunction = it.UAMFunction ?? "";
                it.Message = it.Message ?? "";
                db.Downtime.Add(it);
            }

            db.SaveChanges();

            var json = new { id = item.Id, createdBy = item.CreatedBy, createdAt = TimeUtil.DateTimeToUnixTimestampMiliseconds(item.CreatedAt) };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION, UAM:MYDAMCO:EDITOR")]
        public bool DeleteNewsItem(int id)
        {
            NewsItem item = db.NewsItem.Single(x => x.Id == id);

            // Check authorization
            string reqRole = string.Format("UAM:{0}:{1}", item.NewsCategory.editUAMApplication, item.NewsCategory.editUAMFunction);
            if (!Roles.GetRolesForUser().Contains(reqRole)) throw new UnauthorizedAccessException("Current user and/or role lacks authorization to do this");

            foreach (var dt in item.Downtime.ToList())
            {
                db.Downtime.Remove(dt);
            }
            db.NewsItem.Remove(item);
            db.SaveChanges();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (db != null) db.Dispose();
            base.Dispose(disposing);
        }

    }
}
