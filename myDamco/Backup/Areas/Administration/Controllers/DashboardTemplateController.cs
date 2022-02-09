using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using myDamco.Controllers;
using myDamco.Database;
using myDamco.Models;
using Newtonsoft.Json;
using UAMSharp;

namespace myDamco.Areas.Administration.Controllers
{
    public class DashboardTemplateController : Controller
    {
        private myDamcoEntities myDamcoDB = new myDamcoEntities();

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult TabsAndTables()
        {
            // Get all existing dashboard templates from the database, and put it into a dictionary with OrganizationId as key
            var dashboardTemplateMap = myDamcoDB.DashboardTemplate.GroupBy(x => x.CachedOrganizationId).ToDictionary(g => g.Key, g => g.ToList());

            return View(dashboardTemplateMap);
        }

        // Shows the edit dialog
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult EditDialog(int id)
        {
            var dashboardTemplate = myDamcoDB.DashboardTemplate.Single(x => x.Id == id);
            return View("Edit", dashboardTemplate);
        }

        // Shows the create dialog (= the edit-dialog, but with model = null)
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult CreateDialog()
        {
            return View("Edit", null);            
        }

        // Delete a DashboardTemplate from the DB.
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Delete(int id)
        {
            // Delete the DashboardTemplate entry itself
            DashboardTemplate item = myDamcoDB.DashboardTemplate.Single(x => x.Id == id); // Crash if id does not exist
            myDamcoDB.DashboardTemplate.Remove(item);

            // Delete all widget instances connected to the DashboardTemplate
            var itemsToDelete = myDamcoDB.WidgetInstance.Where(x => x.Login == DashboardController.DASHBOARD_TEMPLATE_USERNAME && x.Role == item.Role).ToList();
            foreach (WidgetInstance itemToDelete in itemsToDelete)
                myDamcoDB.WidgetInstance.Remove(itemToDelete);

            myDamcoDB.SaveChanges();

            return new EmptyResult();
        }

        // Updates an existing DashboardTemplate in the DB. Called from the edit/create dialog.
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Update(int id, string newLoginId, int newRoleId, string description)
        {
            return CreateOrUpdateDashboardTemplate(id, newLoginId, newRoleId, description);
        }

        // Creates a new DashboardTemplate in the DB. Called from the edit/create dialog.
        [HttpPost]
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult Create(string loginId, int roleId, string description)
        {
            return CreateOrUpdateDashboardTemplate(0, loginId, roleId, description);
        }

        // Updates an existing DashboardTemplate in the DB if id>0. If id=0 it creates a new DashboardTemplate in the DB.
        // Returns JSON on exception (see OnException), so that the client can write the error message out.
        private ActionResult CreateOrUpdateDashboardTemplate(int id, string newLoginId, int newRoleId, string description)
        {
            bool createNewTemplate = id == 0;

            Profile adminProfile = myDamco.Profile.GetProfile();

            // Get roles for the user. Fail if the user does not exist.
            UAMClient uamClient = new UAMClient(); // TODO: Cache + reuse instance, etc? Maybe ok for now, since this is on an admin page => not as performance critical.
            UAMRole[] roles = uamClient.GetAllUserRoles(newLoginId); // Crashes if user does not exist in UAM (See UAMExceptionProxy) (expected to happen). So we do not have to check for that explicitly. TODO: Mockservices always returns the same roles... But for the real UAM, UAMExceptionProxy throws an exception if the user does not exist.       

            // Get UAMRole object for the specific role. Fail if the loginId does not have the specified role.
            UAMRole newRole = roles.Single(role => role.Id == newRoleId); // Crashes if newLoginId does not have the selected roleId. So we do not have to check for that explicitly.


            // Check if there already exists a template for the new role (apart from this dashboard template which we are now trying to update)
            bool templateAlreadyExistsForRole = myDamcoDB.DashboardTemplate.Any(x => x.Role == newRoleId && x.Id != id); // (note: if creating a new template, then id=0, so x.Id is never equal to id => it works)
            if (templateAlreadyExistsForRole) throw new DashboardTemplateException("Cannot "+(createNewTemplate?"create":"edit")+" dashboard template.", "A Dashboard Template already exists for this role.");

            // Check if id exists in the DB and update it, if it does. Otherwise fail.
            DashboardTemplate item = createNewTemplate 
                                   ? new DashboardTemplate()
                                   : myDamcoDB.DashboardTemplate.Single(x => x.Id == id); // Crashes if id does not exist in the DB. So we do not have to check for that explicitly.
            int oldRoleId = item.Role; // only used when editing an existing template
            item.LoginCopiedFrom = newLoginId;
            item.RoleCopiedFrom = newRoleId;
            item.Role = newRoleId;
            item.Description = description;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = adminProfile.LoginId;

            // Update cached names too
            item.CachedRoleName = newRole.Name;
            item.CachedOrganizationId = (int)newRole.Organization.Id;
            item.CachedOrganizationName = newRole.Organization.Name;

            if (createNewTemplate)
                myDamcoDB.DashboardTemplate.Add(item);  // If creating a new template, add the new object to the Entity Framework context, so that it will be persisted to the DB when calling SaveChanges().

            // Delete all widgets from the from the previous version of this template (only if editing an existing template)
            if (!createNewTemplate) { 
                var itemsToDelete = myDamcoDB.WidgetInstance.Where(x => x.Login == DashboardController.DASHBOARD_TEMPLATE_USERNAME && x.Role == oldRoleId).ToList();
                foreach (WidgetInstance itemToDelete in itemsToDelete)
                    myDamcoDB.WidgetInstance.Remove(itemToDelete);
            }

            // Insert the new widgets, by copying from the given user and role, and inserting them again in the DB, but for the "template-username" this time. 
            // (Fails if there are no widgets on the source dashboard, since it does not make sense to create a dashboard template with no widgets (not even a welcome widget))
            var widgetsToCopy = myDamcoDB.WidgetInstance.Where(x => x.Login == newLoginId && x.Role == newRoleId).ToList();
            if (widgetsToCopy.Count() == 0) throw new DashboardTemplateException("Error", "Cannot create a dashboard template with no widgets");
            var copiedWidgets = widgetsToCopy.Select(widgetInstance => new WidgetInstance() // TODO: Can this copying be made prettier? Same kind of code is in DashboardController. A util method?
            {
                Login = DashboardController.DASHBOARD_TEMPLATE_USERNAME,
                Role = newRoleId,
                Widget_Id = widgetInstance.Widget_Id,
                Configuration = widgetInstance.Configuration,
                DashboardColumn = widgetInstance.DashboardColumn,
                DashboardPriority = widgetInstance.DashboardPriority,
                Title = widgetInstance.Title
            }).ToList();

            foreach (var widgetInstance in copiedWidgets)
            {
                myDamcoDB.WidgetInstance.Add(widgetInstance);
            }

            // Execute the updates/inserts/deletions within a transaction (The reads above have naturally already been executed, so this corresponds to the "Read Comitted" isolation level)
            myDamcoDB.SaveChanges();

            // Return something?
            return new EmptyResult();
        }


        // Shows the edit dialog
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult GetRolesForUser(string loginId)
        {
            UAMClient uamClient = new UAMClient(); // TODO: Cache + reuse instance, etc?
            UAMRole[] roles = uamClient.GetAllUserRoles(loginId);

            // Group the roles of "LoginId" by Organization. Each of these organization-groups contains the list of roles for that organization.
            var output = roles.GroupBy(role => role.Organization, (org, rolesInOrg) => new {
                orgId = org.Id, orgName = org.Name, roles = rolesInOrg.Select(role => new {
                    roleId = role.Id, roleName = role.Name
                })
            }, new OrganizationEqualityComparer()).ToList();

            string json = JsonConvert.SerializeObject(output, Formatting.Indented);

            return Content(json, "application/json");
        }

        private class OrganizationEqualityComparer : IEqualityComparer<UAMOrganization>
        {
            public bool Equals(UAMOrganization org1, UAMOrganization org2) {return org1.Id == org2.Id;}
            public int GetHashCode(UAMOrganization org) {return org.Id.GetHashCode();}
        }


        // Returns a preview of a dashboard template
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult DashboardTemplatePreview(int id)
        {
            var dashboardTemplate = myDamcoDB.DashboardTemplate.Single(x => x.Id == id);
            var widgetInstances = myDamcoDB.WidgetInstance
                .Where(wi => wi.Login == DashboardController.DASHBOARD_TEMPLATE_USERNAME && wi.Role == dashboardTemplate.Role)
                .OrderBy(w => w.DashboardColumn)
                .ThenBy(w => w.DashboardPriority)
                .ToList();
            return View("DashboardPreview", widgetInstances);
        }

        // Returns a preview of a users dashboard
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult UserDashboardPreview(string loginId, int roleId)
        {
            var widgetInstances = myDamcoDB.WidgetInstance
                .Where(wi => wi.Login == loginId && wi.Role == roleId)
                .OrderBy(w => w.DashboardColumn)
                .ThenBy(w => w.DashboardPriority)
                .ToList();
            return View("DashboardPreview", widgetInstances);
        }

        // Updates all cached organization names and role names from UAM, as well as the cached organization-id for all the roles.
        // This is a relatively expensive operation, due to the hops and hoops it has to jump through in order to get this info from UAM. Which is why the values are cached in the first place :)
        // (There is no UAM operation "getRole(roleId)" unfortunately)
        // To test this: Change the cached values in the DB to something random, run this and check that the cached values revert to their correct states.
        [Authorize(Roles = "UAM:MYDAMCO:ADMINISTRATION")]
        public ActionResult RefreshCachedNamesFromUAM()
        {
            UAMClient uamClient = GetUAMClientAllowingLargeResponseFromUAM();

            // Get all dashboard templates from the database - these are the ones we will update the cached values of.
            IList<DashboardTemplate> dashboardTemplates = myDamcoDB.DashboardTemplate.ToList();

            // For each role, get the role-name, organization-name and organization-id from UAM. (There can only be 1 dashboard template for each role, so fine to loop through all dashboard-templates)
            // 
            // This is done the following way: 
            // For each role, find the users in that role, by calling getUsersByRole(roleId). For the first user in that role, call getAllUserRoles(loginId), and find the new role-name and organization name/id there. 
            // (With this approach there is the possibility of not finding the role, if no users have it - but in that case the role is useless wrt. dashboard templates, so that is not a real problem)
            IList<int> failedRoles = new List<int>(); // <- failed roles
            foreach (DashboardTemplate dashboardTemplate in dashboardTemplates)
            {
                int roleId = dashboardTemplate.Role;

                // Part1: Call getUsersByRole(roleId), so that we will get a loginId for a user which has that role. This is used in part 2 to get info about that role.
                UAMUserSimple userWithRole = uamClient.GetUsersByRole(roleId).FirstOrDefault();
                if (userWithRole == null)
                {
                    failedRoles.Add(roleId);
                    continue;
                }

                // Part2: For this user, call getAllUserRoles(loginId). This will give us the role-name, org-id and org-name.
                UAMRole role = uamClient.GetAllUserRoles(userWithRole.LoginId).FirstOrDefault(r => r.Id == roleId);
                if (role == null)
                {
                    failedRoles.Add(roleId);
                    continue;
                }

                // Update the cached values in the dashboard-template
                dashboardTemplate.CachedRoleName = role.Name;
                dashboardTemplate.CachedOrganizationId = (int)role.Organization.Id;
                dashboardTemplate.CachedOrganizationName = role.Organization.Name;
            }

            // Get list of modified entities, so user can se if something happened (http://stackoverflow.com/questions/17463415/how-to-get-list-of-modified-objects-in-entity-framework-5)
            List<DashboardTemplate> modifiedEntities = myDamcoDB.ChangeTracker.Entries()
                .Where(x => x.State == System.Data.EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<DashboardTemplate>()
                .ToList();

            // Save changes to DB (If we reached here without exception, we assume all is fine)
            myDamcoDB.SaveChanges();


            // Output to javascript
            string responseText = "";
            if (failedRoles.Any())
            {
                string msg = "Refresh completed, but was unable to update the names for the roles with the following IDs from UAM: " + string.Join(", ", failedRoles);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(msg));
                responseText += "<p>" + msg + "</p>";
            }
            if (modifiedEntities.Any())
            {
                responseText += "<div>Updated names for the following roles: <ul>";
                foreach (DashboardTemplate dt in modifiedEntities)
                    responseText += "<li>" + dt.CachedRoleName + "</li>";
                responseText += "</ul></div>";
            }
            return Content(responseText);
        }

        // Used for UpdateCachedNamesFromUAM, so that we can call UAMClient.getOrganizations() without crashing, due to the response being too large.
        private UAMClient GetUAMClientAllowingLargeResponseFromUAM()
        {
            // We have to increase the maximum allowed response site, to a value larger than that from web.config, since getOrganizations() returns ALL organizations, which results in
            // a very large response (more than 2MB at the time of writing).
            System.ServiceModel.BasicHttpBinding uamBinding = new System.ServiceModel.BasicHttpBinding("UAMWebservicePortV1Binding"); // This constructor reads values from web.config
            uamBinding.MaxBufferSize = 10 * 1024 * 1024; // 10 MB - arbitrarily chosen. If the UAM-response becomes larger than this, just increase it.
            uamBinding.MaxReceivedMessageSize = 10 * 1024 * 1024;

            // Get endpoint from web.config as well.
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~");
            ServiceModelSectionGroup section = rootWebConfig.GetSectionGroup("system.serviceModel") as ServiceModelSectionGroup;
            ChannelEndpointElementCollection endpoints = section.Client.Endpoints;
            ChannelEndpointElement uamEndpoint = endpoints.Cast<ChannelEndpointElement>().Single<ChannelEndpointElement>(endpoint => endpoint.Name == "UAMWebservicePortV1Binding");

            return new UAMClient(uamEndpoint.Address.ToString(), uamBinding);
        }

        private class DashboardTemplateException : Exception
        {
            public readonly string Title;
            public DashboardTemplateException(string title, string message) : base(message) { Title = title; }
            public DashboardTemplateException(string title, string message, Exception innerException) : base(message, innerException) { Title = title; }
        }

        // This is mainly to give good error messages to the users during ajax calls (to create or edit for example).
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;

                bool isErrorExpected; // if false, the exception will be logged to Elmah.

                bool page = true;
                bool simple = false;

                string action = filterContext.RouteData.Values["action"].ToString();
                switch (action)
                {
                    case "Update":
                    case "Create":
                    case "Delete":
                    case "GetRolesForUser":
                        page = false;
                        break;
                }

                // If the exception is wrapped in a provider exception, find its inner exception and use that instead
                if (exception.GetType().ToString() == "System.Configuration.Provider.ProviderException" && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }

                // Exception type specific errors
                ErrorModel result;
                switch (exception.GetType().ToString())
                {
                    case "myDamco.Areas.Administration.Controllers.DashboardTemplateController+DashboardTemplateException": // Let the messages from these pass through to the client unaltered.
                        var dtexception = exception as DashboardTemplateException; 
                        if (dtexception == null) goto default;
                        result = new ErrorModel(dtexception.Title, dtexception.Message);
                        isErrorExpected = true;
                        break;
                    case "UAMSharp.UAMUserDoesNotExistException":
                        result = new ErrorModel("User does not exist", "This user does not exist");
                        isErrorExpected = new[] {"Create", "Update", "GetRolesForUser"}.Contains(action);
                        break;
                    default:
                        // TODO: Check if all of these are appropriate to write to the UI in this case...
                        result = DashboardController.ConvertExceptionToErrorModel(exception, out simple);
                        isErrorExpected = false;
                        break;
                }

                // Log to Elmah if the error were not one of the expected ones.
                if (!isErrorExpected)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                }

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();

                if (page)
                {
                    IController c = new myDamco.Controllers.StaticContentController();
                    RouteData rd = new RouteData();
                    rd.Values["controller"] = "StaticContent";
                    rd.Values["action"] = "CustomError";
                    rd.Values["model"] = result;
                    rd.Values["simple"] = simple;
                    RequestContext rc = new RequestContext(filterContext.HttpContext, rd);

                    try
                    {
                        c.Execute(rc);
                    }
                    catch (Exception) // Down-grade to generic static page if something goes wrong
                    {
                        rd.Values["action"] = "ErrorSimple";
                        c = new myDamco.Controllers.StaticContentController();
                        c.Execute(rc);
                    }
                }
                else
                {
                    this.Json(result, JsonRequestBehavior.AllowGet).ExecuteResult(this.ControllerContext);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            myDamcoDB.Dispose();
            base.Dispose(disposing);
        }

    }
}
