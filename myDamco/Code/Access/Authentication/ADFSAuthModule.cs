using System;
using System.Web;
using System.Web.Security.SingleSignOn;

// http://build.mt.gov/2011/10/27/aspnet-mvc3-and-the-authorize-attribute.aspx

// TODO: Make this module read it's config from Web.Config, also test how roles assigned from here affect UAMRoleProvider modules
namespace myDamco.Access.Authentication
{
    public class ADFSAuthModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += new EventHandler(OnEndRequest);
        }

        public void OnEndRequest(Object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication) sender;

            if (application.Response.StatusCode == 401 && application.Context.User != null && application.Context.User.Identity is SingleSignOnIdentity)
            {
                SingleSignOnIdentity SsoId = application.Context.User.Identity as SingleSignOnIdentity;
                if (!SsoId.IsAuthenticated)
                {
                    // This redirects the user to the resource AD FS Logon service.
                    SsoId.SignIn(application.Context);
                }
            }
        }

        public void Dispose()
        {
            // TODO: implement
        }
    }
}
