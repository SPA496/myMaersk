using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using myDamco.Areas.MySupplyChainAssistantAdministration.Data;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;
using Newtonsoft.Json;

namespace myDamco.Controllers
{
    public class MySupplyChainAssistantController : Controller
    {

        public ActionResult JavaScript()
        {
            IEnumerable<Function> funcs = null;
            using (var rep = new SCASqlCERepository())
            {
                funcs = rep.GetFunctions();
            }
            Response.ContentType = "application/x-javascript";
            String mappings = JsonConvert.SerializeObject(funcs, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return View(model: mappings);
        }

        // TODO: Update path wherever this is used
        public JsonResult LogFunction(string log)
        {
            var decoded = HttpUtility.UrlDecode(log);
            var haps = JsonConvert.DeserializeObject<LogEntry>(decoded);
            // TODO: Perists log data to db.
            return Json(null, JsonRequestBehavior.AllowGet);
        }

    }
}
