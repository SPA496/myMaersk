using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myDamco.Database;

namespace myDamco.Areas.Administration.Controllers
{
    public class SettingsController : BaseAdminController
    {

        private myDamcoEntities myDamcoDB = new myDamcoEntities();

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Index()
        {
            List<Setting> settings = myDamcoDB.Setting.OrderBy(s => s.Name).ToList();
            return View(settings);
        }

        // Rerturns the html for a single non-editable row for the table in Index.cshtml
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Row(int id)
        {
            Setting setting = myDamcoDB.Setting.Single(s => s.Id == id);
            return PartialView(setting);
        }

        // Rerturns the html for a single editable row for the table in Index.cshtml
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult RowEditable(int id)
        {
            Setting setting = myDamcoDB.Setting.Single(s => s.Id == id);
            return PartialView(setting);
        }

        // Save a setting
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost]
        public ActionResult Save(int id, string value)
        {           
            // TODO: Check if old value was modified in the meantime, by also sending the old value along? (probably unlikely)
            Setting setting = myDamcoDB.Setting.Single(s => s.Id == id); // throws exception if not found
            setting.Value = value;
            myDamcoDB.SaveChanges();
            return PartialView("Row", setting);
        }

        protected override void Dispose(bool disposing)
        {
            if (myDamcoDB != null)
                myDamcoDB.Dispose();
            base.Dispose(disposing);
        }

    }
}
