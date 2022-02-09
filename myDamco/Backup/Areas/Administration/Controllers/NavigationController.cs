using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web.Mvc;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class NavigationController : BaseAdminController
    {
        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/Navigation/
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            ViewBag.Count = db.Navigation.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Administration/Navigation/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Navigation.Count().ToString());
            ObjectQuery<Navigation> navigation = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Navigation>();

            return PartialView(navigation.OrderBy(x => x.Priority));
        }

        //
        // GET: /Default5/RowData/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult RowData(int id)
        {
            Navigation navigation = db.Navigation.Find(id);
            return PartialView("GridData", new Navigation[] { navigation });
        }

        //
        // GET: /Administration/Navigation/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create()
        {
            return PartialView("Edit");
        }

        //
        // POST: /Administration/Navigation/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Create(Navigation navigation)
        {
            if (ModelState.IsValid)
            {
                db.Navigation.Add(navigation);
                db.SaveChanges();

                MvcApplication.ReloadRoutes();

                return PartialView("GridData", new Navigation[] { navigation });
            }

            return PartialView("Edit", navigation);
        }

        //
        // GET: /Administration/Navigation/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(int id)
        {
            Navigation navigation = db.Navigation.Find(id);
            return PartialView(navigation);
        }

        //
        // POST: /Administration/Navigation/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Edit(Navigation navigation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(navigation).State = EntityState.Modified;
                db.SaveChanges();

                MvcApplication.ReloadRoutes();

                return PartialView("GridData", new Navigation[] { navigation });
            }

            return PartialView(navigation);
        }

        //
        // POST: /Administration/Navigation/Delete/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public void Delete(int id)
        {
            Navigation navigation = db.Navigation.Find(id);
            db.Navigation.Remove(navigation);
            db.SaveChanges();

            MvcApplication.ReloadRoutes();
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public void Reorder(List<int> ids)
        {
            int cnt = 0;
            foreach(int id in ids) {
                db.Navigation.Single(x => x.Id == id).Priority = (byte)cnt++;
            }
            
            db.SaveChanges();
            MvcApplication.ReloadRoutes();

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
