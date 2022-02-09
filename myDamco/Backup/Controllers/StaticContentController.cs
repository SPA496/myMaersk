using System.Linq;
using System.Web.Mvc;
using myDamco.Database;
using myDamco.Models;

namespace myDamco.Controllers
{
    public class StaticContentController : Controller
    {
        private readonly myDamcoEntities db = new myDamcoEntities();

        public ActionResult Database(string id) {
            return View(db.Page.Single(x => x.UId == id));
        }

        public ActionResult Error()
        {
            Response.StatusCode = 500;
            ErrorModel model = new ErrorModel("Unexpected Error", "Sorry, an error occurred while processing your request.");
            return View("Error", model);
        }

        public ActionResult ErrorSimple()
        {
            Response.StatusCode = 500;
            ErrorModel model = new ErrorModel("Unexpected Error", "Sorry, an error occurred while processing your request.");
            return View("ErrorSimple", model);
        }

        // Resource not found
        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("Error", new ErrorModel("404 - Page Not Found", "Sorry, the page you were looking for could not be found."));
        }

        // Unauthorized
        public ActionResult Error401()
        {
            // Below the status code is intentionally set twice in a row, to solve a subtle error. (this is called from global.asax.cs EndRequest() - could have been set from there instead)
            // If this is not done, IIS (+IIS Express) will NOT show our custom error page on 401, but instead show its own error page, when called from a remote client (a client which is not on the webservers 
            // localhost). You can test this by connecting to your dev environment from non-localhost (from a KVM for example). Test it by going to the admin page and change into a non-admin role.
            // 
            // Explanation:
            //  From decompiling and peeking into the .NET library, it seems that the StatusCode setter eventually calls the setter HttpResponse.StatusCode. This setter, sets multiple values. It seems
            //  that setting the field "HttpResponse._statusSet = true" is what solves the problem, since debugging shows it is otherwise false. When this is called the StatusCode is already 401, and
            //  the setter of HttpResponse.StatusCode aborts early if you set the status code to the same value as it already is => We have to set it to something other than 401, in order to set 
            //  "_statusSet = true", and then set it back to 401. (So why does it skip the custom error page if _statusSet = false? Something in HttpResponse.UpdateNativeResponse() or HttpResponse.SyncStatusIntegrated(). Custom error module?)
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 418; // 418 = "I'm a teapot". (the status code does not matter, it just has to be different from 401). 
            Response.StatusCode = 401;
            return View("Error", new ErrorModel("401 - Unauthorized", "Sorry, you are not authorized to view this page."));
        }

        // UAM not found
        public ActionResult ErrorUAM()
        {
            Response.StatusCode = 500;
            ErrorModel model = new ErrorModel("Unexpected Error in UAM", "Sorry, an error occurred while processing your request. The UAM service did not respond.");
            
            return View("ErrorSimple", model);
        }

        // Error rendering navigation menu
        public ActionResult ErrorNavigation(ErrorModel model)
        {
            Response.StatusCode = 500;
            return PartialView("_ErrorNavigation", model);
        }

        public ActionResult CustomError(ErrorModel model, bool simple)
        {
            Response.StatusCode = 500;
            string view = simple ? "ErrorSimple" : "Error";
            return View(view, model);
        }

		// For moved pages linked to by external systems. Redirect instead of 404 error.
        public ActionResult RedirectToFrontpage()
        {
            return RedirectPermanent("~/");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
