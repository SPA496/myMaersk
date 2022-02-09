using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web.Mvc;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class NewsItemScaffoldingController : BaseAdminController
    {
        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/NewsItemScaffolding/
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            ViewBag.Count = db.NewsItem.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Administration/NewsItemScaffolding/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.NewsItem.Count().ToString());
            ObjectQuery<NewsItem> newsitems = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<NewsItem>();
            var newsitems2 = newsitems.Include(n => n.NewsCategory).OrderBy(x => x.Id);

            return PartialView(newsitems2.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult RowData(int id)
        {
            NewsItem newsitems = db.NewsItem.Find(id);
            return PartialView("GridData", new NewsItem[] { newsitems });
        }

        //
        // GET: /Administration/NewsItemScaffolding/Create
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create()
        {
            ViewBag.NewsCategory_Id = new SelectList(db.NewsCategory, "Id", "Name");
            return PartialView("Edit");
        }

        //
        // POST: /Administration/NewsItemScaffolding/Create
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create(NewsItem newsitems)
        {
            if (ModelState.IsValid)
            {
                db.NewsItem.Add(newsitems);
                db.SaveChanges();
                return PartialView("GridData", new NewsItem[] { newsitems });
            }

            ViewBag.NewsCategory_Id = new SelectList(db.NewsCategory, "Id", "Name", newsitems.NewsCategory_Id);
            return PartialView("Edit", newsitems);
        }

        //
        // GET: /Administration/NewsItemScaffolding/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(int id)
        {
            NewsItem newsitems = db.NewsItem.Find(id);
            ViewBag.NewsCategory_Id = new SelectList(db.NewsCategory, "Id", "Name", newsitems.NewsCategory_Id);
            return PartialView(newsitems);
        }

        //
        // POST: /Administration/NewsItemScaffolding/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Edit(NewsItem newsitems)
        {
            if (ModelState.IsValid)
            {
                db.Entry(newsitems).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new NewsItem[] { newsitems });
            }

            ViewBag.NewsCategory_Id = new SelectList(db.NewsCategory, "Id", "Name", newsitems.NewsCategory_Id);
            return PartialView(newsitems);
        }

        //
        // POST: /Administration/NewsItemScaffolding/Delete/5
        [Authorize(Roles = "UAM:MYDAMCO:EDITOR, UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public void Delete(int id)
        {
            NewsItem newsitems = db.NewsItem.Find(id);
            db.NewsItem.Remove(newsitems);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
