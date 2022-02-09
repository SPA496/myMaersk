using System.Data;
using System.Linq;
using System.Web.Mvc;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class StaticContentController : BaseAdminController
    {
        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/StaticContent/
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ViewResult Index()
        {
            return View(db.Page.ToList());
        }

        //
        // GET: /Administration/StaticContent/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Administration/StaticContent/Create
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [ValidateInput(false)] // KV: TODO: Figure out how to do this only for a single parameter
        public ActionResult Create(Page page)
        {
            if (ModelState.IsValid)
            {
                db.Page.Add(page);
                db.SaveChanges();

                MvcApplication.ReloadRoutes();
                return RedirectToAction("Index");  
            }

            return View(page);
        }
        
        //
        // GET: /Administration/StaticContent/Edit/5
        [ValidateInput(false)]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(int id)
        {
            Page page = db.Page.Find(id);
            return View(page);
        }

        //
        // POST: /Administration/StaticContent/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(Page page)
        {
            if (ModelState.IsValid)
            {
                db.Entry(page).State = EntityState.Modified;
                db.SaveChanges();
                MvcApplication.ReloadRoutes();
                return RedirectToAction("Index");
            }
            return View(page);
        }

        //
        // GET: /Administration/StaticContent/Delete/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Delete(int id)
        {
            Page page = db.Page.Find(id);
            return View(page);
        }

        //
        // POST: /Administration/StaticContent/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Page page = db.Page.Find(id);
            db.Page.Remove(page);
            db.SaveChanges();
            MvcApplication.ReloadRoutes();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}