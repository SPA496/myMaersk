using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web.Mvc;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class NewsCategoryController : BaseAdminController
    {
        private const string ViewPath = "../News/NewsCategory/";

        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/NewsCategory/
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ViewResult Index()
        {
            return View(ViewPath + "Index");
        }
        
        //
        // GET: /Administration/NewsCategory/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.NewsCategory.Count().ToString());
            ObjectQuery<NewsCategory> newscategories = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<NewsCategory>();
            newscategories = newscategories.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(ViewPath + "GridData", newscategories.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult RowData(int id)
        {
            NewsCategory newscategories = db.NewsCategory.Find(id);
            return PartialView(ViewPath + "GridData", new NewsCategory[] { newscategories });
        }

        //
        // GET: /Administration/NewsCategory/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create()
        {
            return PartialView(ViewPath + "Edit");
        }

        //
        // POST: /Administration/NewsCategory/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Create(NewsCategory newscategories)
        {
            if (ModelState.IsValid)
            {
                db.NewsCategory.Add(newscategories);
                db.SaveChanges();
                return PartialView(ViewPath + "GridData", new NewsCategory[] { newscategories });
            }

            return PartialView(ViewPath + "Edit", newscategories);
        }

        //
        // GET: /Administration/NewsCategory/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(int id)
        {
            NewsCategory newscategories = db.NewsCategory.Find(id);
            return PartialView(ViewPath + "Edit", newscategories);
        }

        //
        // POST: /Administration/NewsCategory/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Edit(NewsCategory newscategories)
        {
            if (ModelState.IsValid)
            {
                db.Entry(newscategories).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView(ViewPath + "GridData", new NewsCategory[] { newscategories });
            }

            return PartialView(ViewPath + "Edit", newscategories);
        }

        //
        // POST: /Administration/NewsCategory/Delete/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public void Delete(int id)
        {
            NewsCategory newscategories = db.NewsCategory.Find(id);
            db.NewsCategory.Remove(newscategories);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
