using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myDamco.Areas.MySupplyChainAssistantAdministration.Data;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Controllers
{   
    public class FunctionsController : Controller
    {
        //private mySupplyChainAssistantContext context = new mySupplyChainAssistantContext();

        //
        // GET: /Functions/

        public ViewResult Index()
        {
            IEnumerable<Function> functions = new List<Function>().AsQueryable();
            using (var rep = new SCASqlCERepository())
            {
                functions = rep.GetFunctions();
            }
            return View(functions);
        }

        //
        // GET: /Functions/Create

        public ActionResult Create(int? applicationId)
        {
            using (var rep = new SCASqlCERepository())
            {
                var functions = rep.GetFunctions();
                ViewBag.PossibleApplications = rep.GetApplications();
                ViewBag.PossibleReferences = functions;
                ViewBag.ArgumentIds = functions.SelectMany(f => f.arguments.Select(a => a.id)).ToList();
            }

            // TESTING
            return View();

            if (applicationId != null)
                return View(new Function() { applicationId = (int)applicationId });
            return View();
        } 

        //
        // POST: /Functions/Create

        [HttpPost]
        public ActionResult Create(Function function)
        {
            if (ModelState.IsValid)
            {
                using (var rep = new SCASqlCERepository())
                {
                    rep.CreateFunction(function);
                }
                return RedirectToAction("Index");  
            }

            using (var rep = new SCASqlCERepository())
            {
                var functions = rep.GetFunctions();
                ViewBag.PossibleApplications = rep.GetApplications();
                ViewBag.PossibleReferences = functions;
                ViewBag.ArgumentIds = rep.GetEntityTypes().ToList();
                //new List<string> { "BookingId", "ShippingId", "BillOfLadingNumber", "ContainerId", "PurchaseOrderId" };
            }
            return View(function);
        }
        
        //
        // GET: /Functions/Edit/5
 
        public ActionResult Edit(int id)
        {
            Function function = null;
            using (var rep = new SCASqlCERepository())
            {
                var functions = rep.GetFunctions();
                function = functions.FirstOrDefault(f => f.functionId == id);
                ViewBag.PossibleApplications = rep.GetApplications();
                ViewBag.PossibleReferences = functions;
                ViewBag.ArgumentIds = rep.GetEntityTypes().ToList();
            }
            return View(function);
        }

        //
        // POST: /Functions/Edit/5

        [HttpPost]
        public ActionResult Edit(Function function)
        {
            if (ModelState.IsValid)
            {
                using (var rep = new SCASqlCERepository())
                {
                    rep.UpdateFunction(function);
                }
                return RedirectToAction("Index");
            }
            using (var rep = new SCASqlCERepository())
            {
                ViewBag.PossibleApplications = rep.GetApplications();
                ViewBag.PossibleReferences = rep.GetFunctions();
                ViewBag.ArgumentIds = rep.GetEntityTypes().ToList();
            }
            return View(function);
        }

        //
        // GET: /Functions/Delete/5
 
        public ActionResult Delete(int id)
        {
            Function function = null;
            using (var rep = new SCASqlCERepository())
            {
                function = rep.GetFunctions().First(f => f.functionId == id);
            }
            return View(function);
        }

        //
        // POST: /Functions/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var rep = new SCASqlCERepository())
            {
                rep.DeleteFunction(id);
            }
            return RedirectToAction("Index");
        }
    }
}