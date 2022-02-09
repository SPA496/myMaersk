using System;
using System.Web.Mvc;
using System.Web.Routing;
using myDamco.Models;

namespace myDamco.Areas.Administration.Controllers
{
    public class BaseAdminController : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                ErrorModel result = new ErrorModel("Internal Error", "An unexpected error occured.", filterContext.Exception.Message);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();

                IController c = new myDamco.Controllers.StaticContentController();
                RouteData rd = new RouteData();
                rd.Values["controller"] = "StaticContent";
                rd.Values["action"] = "CustomError";
                rd.Values["model"] = result;
                rd.Values["simple"] = false;
                rd.Values["area"] = "";
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
        }
    }
}
