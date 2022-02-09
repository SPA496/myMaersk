using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myDamco.Database;
using Newtonsoft.Json.Linq;

// TODO: Look into using http://jsonwidget.org/wiki/Jsonwidget for editing the configurations files
// or https://github.com/wjosdejong/jsoneditoronline

namespace myDamco.Areas.Administration.Controllers
{
    public class WidgetsController : BaseAdminController
    {
        private myDamcoEntities db = new myDamcoEntities();

        //
        // GET: /Administration/Widgets/
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ViewResult Index()
        {
            return View(db.Widget.ToList());
        }

        //
        // GET: /Administration/Widgets/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Administration/Widgets/Create
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        [HttpPost, ValidateInput(false)]
        public ActionResult Create(Widget widgets)
        {
            if (ModelState.IsValid)
            {
                db.Widget.Add(widgets);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(widgets);
        }
        
        //
        // GET: /Administration/Widgets/Edit/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(int id)
        {
            Widget widget = db.Widget.Find(id);
            return View(widget);
        }

        //
        // POST: /Administration/Widgets/Edit/5

        [HttpPost, ValidateInput(false)]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Edit(Widget widget)
        {
            // Validate the JSON schema if we have it
            var validSchema = true;
            if (widget.ConfigurationSchema != null && widget.Configuration != null)
            {
                //JsonSchema schema = JsonSchema.Parse(widget.ConfigurationSchema);
                JObject config = JObject.Parse(widget.Configuration);
                //validSchema = config.IsValid(schema);
                // TODO: Notify the user that the configuration did not validate
                //ModelState.AddModelError("", "Schema failed to validate, at line ...");
            }

            if (ModelState.IsValid && validSchema)
            {
                db.Entry(widget).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(widget);
        }

        //
        // GET: /Administration/Widgets/Delete/5
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Delete(int id)
        {
            Widget widgets = db.Widget.Find(id);
            return View(widgets);
        }

        //
        // POST: /Administration/Widgets/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult DeleteConfirmed(int id)
        {
            Widget widgets = db.Widget.Find(id);
            foreach (var wiHistory in widgets.WidgetInstanceHistory.ToList())
            {
                db.WidgetInstanceHistory.Remove(wiHistory);
            }
            db.Widget.Remove(widgets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpGet, ActionName("Export")]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public JsonResult Export(int id)
        {
            var widget = db.Widget.Single(x => x.Id == id);

            var jsonSerializableWidget = new
            {
                UId = widget.UId,
                Title = widget.Title,
                Description = widget.Description,
                Category = widget.Category,
                Icon = widget.Icon,
                Template = widget.Template,
                TemplateHTML = widget.TemplateHTML,
                TemplateInstanceHTML = widget.TemplateInstanceHTML,
                TemplateCSS = widget.TemplateCSS,
                TemplateJS = widget.TemplateJS,
                Configuration = widget.Configuration,
                ServiceConfiguration = widget.ServiceConfiguration,
                ConfigurationSchema = widget.ConfigurationSchema,
                ServiceConfigurationSchema = widget.ServiceConfigurationSchema,
                ServiceURL = widget.ServiceURL,
                InstanceConfigurationSchema = widget.InstanceConfigurationSchema,
                Editable = widget.Editable,
                Disabled = widget.Disabled,
                UAMApplication = widget.UAMApplication,
                UAMFunction = widget.UAMFunction
            };

            HttpContext.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.json", widget.UId));
            return Json(jsonSerializableWidget, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var widgetFromFile = Newtonsoft.Json.JsonConvert.DeserializeObject<Widget>(new StreamReader(file.InputStream).ReadToEnd());
                var widgetInDatabase = db.Widget.SingleOrDefault(x => x.UId == widgetFromFile.UId);
                if (widgetInDatabase == null)
                {
                    db.Widget.Add(widgetFromFile);
                    db.SaveChanges();
                    return Content("Inserted");

                }
                else
                {
                    widgetInDatabase.UId = widgetFromFile.UId;
                    widgetInDatabase.Title = widgetFromFile.Title;
                    widgetInDatabase.Description = widgetFromFile.Description;
                    widgetInDatabase.Category = widgetFromFile.Category;
                    widgetInDatabase.Icon = widgetFromFile.Icon;
                    widgetInDatabase.Template = widgetFromFile.Template;
                    widgetInDatabase.TemplateHTML = widgetFromFile.TemplateHTML;
                    widgetInDatabase.TemplateInstanceHTML = widgetFromFile.TemplateInstanceHTML;
                    widgetInDatabase.TemplateCSS = widgetFromFile.TemplateCSS;
                    widgetInDatabase.TemplateJS = widgetFromFile.TemplateJS;
                    widgetInDatabase.Configuration = widgetFromFile.Configuration;
                    widgetInDatabase.ServiceConfiguration = widgetFromFile.ServiceConfiguration;
                    widgetInDatabase.ConfigurationSchema = widgetFromFile.ConfigurationSchema;
                    widgetInDatabase.ServiceConfigurationSchema = widgetFromFile.ServiceConfigurationSchema;
                    widgetInDatabase.ServiceURL = widgetFromFile.ServiceURL;
                    widgetInDatabase.InstanceConfigurationSchema = widgetFromFile.InstanceConfigurationSchema;
                    widgetInDatabase.Editable = widgetFromFile.Editable;
                    widgetInDatabase.Disabled = widgetFromFile.Disabled;
                    widgetInDatabase.UAMApplication = widgetFromFile.UAMApplication;
                    widgetInDatabase.UAMFunction = widgetFromFile.UAMFunction;

                    db.SaveChanges();
                    return Content("Updated");
                }

            }
            return Content("No file");
        }

        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}