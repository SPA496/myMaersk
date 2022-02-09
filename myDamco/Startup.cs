using Microsoft.IdentityModel.Logging;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using System.Configuration;
using System.Web;

[assembly: OwinStartup(typeof(myDamco.Startup))]

namespace myDamco
{
    public class Startup
    {
        private static string realm = ConfigurationManager.AppSettings["ida:Wtrealm"];
        private static string adfsMetadata = ConfigurationManager.AppSettings["ida:ADFSMetadata"];
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);// Uncomment before check-in code
        }
        public void ConfigureAuth(IAppBuilder app)
        {
            if (HttpContext.Current.User == null)
            {
                IdentityModelEventSource.ShowPII = true;
                app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                app.UseCookieAuthentication(new CookieAuthenticationOptions());

                app.UseWsFederationAuthentication(
                    new WsFederationAuthenticationOptions
                    {
                        Wtrealm = realm,
                        MetadataAddress = adfsMetadata
                    });
            }

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}