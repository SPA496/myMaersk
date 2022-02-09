using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myDamco.Areas.MySupplyChainAssistantAdministration.Data;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Controllers
{   
    public class ApplicationsController : Controller
    {
        //private mySupplyChainAssistantContext context = new mySupplyChainAssistantContext();

        //
        // GET: /Applications/

        public ViewResult Index()
        {
            IEnumerable<Application> apps = new List<Application>().AsQueryable();
            using (var rep = new SCASqlCERepository())
            {
                apps = rep.GetApplications();
            }
            return View(apps);
        }

        //
        // GET: /Applications/Details/5

        public ViewResult Details(int id)
        {
            Application application = null;
            using (var rep = new SCASqlCERepository())
            {
                application = rep.GetApplication(id);
            }
            return View(application);
        }

        //
        // GET: /Applications/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Applications/Create

        [HttpPost]
        public ActionResult Create(Application application)
        {
            if (ModelState.IsValid)
            {
                using (var rep = new SCASqlCERepository())
                {
                    rep.CreateApplication(application);
                }
                return RedirectToAction("Index");  
            }

            return View(application);
        }
        
        //
        // GET: /Applications/Edit/5
 
        public ActionResult Edit(int id)
        {
            Application application = null;
            using (var rep = new SCASqlCERepository())
            {
                application = rep.GetApplication(id);
            }
            return View(application);
        }

        //
        // POST: /Applications/Edit/5

        [HttpPost]
        public ActionResult Edit(Application application)
        {
            if (ModelState.IsValid)
            {
                using (var rep = new SCASqlCERepository())
                {
                    rep.UpdateApplication(application);
                }
                return RedirectToAction("Index");
            }
            return View(application);
        }

        //
        // GET: /Applications/Delete/5
 
        public ActionResult Delete(int id)
        {
            Application application = null;
            using (var rep = new SCASqlCERepository())
            {
                application = rep.GetApplication(id);
            }
            return View(application);
        }

        //
        // POST: /Applications/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var rep = new SCASqlCERepository())
            {
                rep.DeleteApplication(id);
            }
            return RedirectToAction("Index");
        }
    }
}