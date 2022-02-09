using System.Collections.Generic;
using System.Web.Mvc;
using myDamco.Areas.MySupplyChainAssistantAdministration.Data;
using myDamco.Areas.MySupplyChainAssistantAdministration.Models;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Controllers
{
    public class FunctionModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            var form = controllerContext.HttpContext.Request.Form;
            if (propertyDescriptor.Name.Equals("references") && form["references"] != null)
            {
                using (var context = new SCASqlCERepository())
                {
                    List<int> fids = new List<int>();
                    foreach (string val in form["references"].Split(','))
                    {
                        int iVal = -1;
                        if (int.TryParse(val, out iVal)) fids.Add(iVal);
                    }
                    propertyDescriptor.SetValue(bindingContext.Model, fids);
                }
                return;
            }
            else if (propertyDescriptor.Name.Equals("arguments"))
            {
                List<Argument> args = new List<Argument>();
                if (form["arguments.index"] != null)
                {
                    foreach (string val in form["arguments.index"].Split(','))
                    {
                        int iVal = -1;
                        if (!int.TryParse(val, out iVal)) continue;
                        if ((string.IsNullOrWhiteSpace(form["arguments[" + iVal + "].id"]) || form["arguments[" + iVal + "].id"].ToLower().Equals("choose...")) && string.IsNullOrWhiteSpace(form["arguments[" + iVal + "].alias"]) && string.IsNullOrWhiteSpace(form["arguments[" + iVal + "].matcher"]))
                            continue;
                        string id = form["arguments[" + iVal + "].id"].ToLower().Equals("other") ? form["arguments[" + iVal + "].text"] : form["arguments[" + iVal + "].id"];
                        args.Add(new Argument { id = id, alias = form["arguments[" + iVal + "].alias"], matcher = form["arguments[" + iVal + "].matcher"] });
                    }
                }
                else
                {
                    int index = 0;
                    while (form["arguments[" + index + "].id"] != null)
                    {
                        if ((string.IsNullOrWhiteSpace(form["arguments[" + index + "].id"]) || form["arguments[" + index + "].id"].ToLower().Equals("choose...")) && string.IsNullOrWhiteSpace(form["arguments[" + index + "].alias"]) && string.IsNullOrWhiteSpace(form["arguments[" + index + "].matcher"]))
                            continue;
                        string id = form["arguments[" + index + "].id"].ToLower().Equals("other") ? form["arguments[" + index + "].text"] : form["arguments[" + index + "].id"];
                        args.Add(new Argument { id = id, alias = form["arguments[" + index + "].alias"], matcher = form["arguments[" + index + "].matcher"] });
                    }
                }
                propertyDescriptor.SetValue(bindingContext.Model, args);
                return;
            }
            else if (propertyDescriptor.Name.Equals("entityIdentifiers"))
            {
                List<EntityIdentifier> ents = new List<EntityIdentifier>();
                if (form["entityIdentifiers.index"] != null)
                {
                    foreach (string val in form["entityIdentifiers.index"].Split(','))
                    {
                        int iVal = -1;
                        if (!int.TryParse(val, out iVal)) continue;
                        if ((string.IsNullOrWhiteSpace(form["entityIdentifiers[" + iVal + "].entityId"]) || form["entityIdentifiers[" + iVal + "].entityId"].ToLower().Equals("choose...")) && string.IsNullOrWhiteSpace(form["entityIdentifiers[" + iVal + "].selector"]))
                            continue;
                        string id = form["entityIdentifiers[" + iVal + "].entityId"].ToLower().Equals("other") ? form["entityIdentifiers[" + iVal + "].text"] : form["entityIdentifiers[" + iVal + "].entityId"];
                        ents.Add(new EntityIdentifier { entityId = id, selector = form["entityIdentifiers[" + iVal + "].selector"] });
                    }
                }
                else
                {
                    int index = 0;
                    while (form["entityIdentifiers[" + index + "].entityId"] != null)
                    {
                        if ((string.IsNullOrWhiteSpace(form["entityIdentifiers[" + index + "].entityId"]) || form["entityIdentifiers[" + index + "].entityId"].ToLower().Equals("choose...")) && string.IsNullOrWhiteSpace(form["entityIdentifiers[" + index + "].selector"]))
                            continue;
                        string id = form["entityIdentifiers[" + index + "].entityId"].ToLower().Equals("other") ? form["entityIdentifiers[" + index + "].text"] : form["entityIdentifiers[" + index + "].entityId"];
                        ents.Add(new EntityIdentifier { entityId = id, selector = form["entityIdentifiers[" + index + "].selector"] });
                    }
                }
                propertyDescriptor.SetValue(bindingContext.Model, ents);
                return;
            }
            else if (propertyDescriptor.Name.Equals("hooks"))
            {
                List<EventHook> hooks = new List<EventHook>();
                if (form["hooks.index"] != null)
                {
                    foreach (string val in form["hooks.index"].Split(','))
                    {
                        int iVal = -1;
                        if (!int.TryParse(val, out iVal)) continue;
                        if (string.IsNullOrWhiteSpace(form["hooks[" + iVal + "].title"]) && string.IsNullOrWhiteSpace(form["hooks[" + iVal + "].selector"]) && string.IsNullOrWhiteSpace(form["hooks[" + iVal + "].hook"]))
                            continue;
                        hooks.Add(new EventHook { title = form["hooks[" + iVal + "].title"], selector = form["hooks[" + iVal + "].selector"], hook = form["hooks[" + iVal + "].hook"] });
                    }
                }
                else
                {
                    int index = 0;
                    while (form["hooks[" + index + "].entityId"] != null)
                    {
                        if (string.IsNullOrWhiteSpace(form["hooks[" + index + "].title"]) && string.IsNullOrWhiteSpace(form["hooks[" + index + "].selector"]) && string.IsNullOrWhiteSpace(form["hooks[" + index + "].hook"]))
                            continue;
                        hooks.Add(new EventHook { title = form["hooks[" + index + "].title"], selector = form["hooks[" + index + "].selector"], hook = form["hooks[" + index + "].hook"] });
                    }
                }
                propertyDescriptor.SetValue(bindingContext.Model, hooks);
                return;
            }
            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }
}