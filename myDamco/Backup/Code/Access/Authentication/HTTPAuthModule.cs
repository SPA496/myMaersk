using System;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

// TODO: Make this module read it's config from Web.Config, also test how roles assigned from here affect UAMRoleProvider modules

namespace myDamco.Access.Authentication
{
    public class HTTPAuthModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += new EventHandler(OnAuthenticateRequest);
            context.EndRequest += new EventHandler(OnEndRequest);
        }

        public void OnAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            
            var request = application.Context.Request;
            var response = application.Context.Response;
            if (!request.IsAuthenticated)
            {
                // Support logout
                if (request.Cookies["httpauthlogout"] == null)
                {
                    // Support logout
                    if (request.AppRelativeCurrentExecutionFilePath == "~/Dashboard/Logout")
                    {
                        response.Cookies["httpauthsession"].Expires = new DateTime(1970, 1, 1, 0, 0, 1); // Set to 1
                        response.Cookies["httpauthlogout"].Value = "true";
                        return;
                    }
                }
                else
                {
                    response.Cookies["httpauthlogout"].Expires = new DateTime(1970, 1, 1, 0, 0, 1); // Set to 1
                }

                // Validate login information
                string sAUTH = application.Request.ServerVariables["HTTP_AUTHORIZATION"];

                // A fake login url, as an alternative to basic authentication (Basic Authentication in URLs is no longer supported in IE and deprecated in Chrome)
                if (request.AppRelativeCurrentExecutionFilePath.StartsWith("~/FakeLogIn/"))
                {
                    string path = request.AppRelativeCurrentExecutionFilePath;

                    Match match = Regex.Match(path, @"~/FakeLogIn/([A-Za-z0-9_]+)/(([A-Za-z0-9_]*)/)?.*", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string username = match.Groups[1].Value;
                        string password = match.Groups[3].Value;

                        if (Membership.ValidateUser(username, password)) // TODO: Not really needed
                        {
                            var UserPrincipal = new GenericPrincipal(new GenericIdentity(username + "@itk.local"), null);
                            application.Context.User = UserPrincipal;
                            response.Cookies["httpauthsession"].Value = username + "@itk.local";
                            response.Cookies["httpauthsession"].Path = "/";

                            // Remove logout cookie?
                        }
                    }

                    response.Redirect("~");
                }
                else if (sAUTH != null)
                {
                    if (sAUTH.Substring(0, 5).ToUpper() == "BASIC")
                    {
                        // Get 0=Username, 1=Password
                        string[] sCredentials = Base64Decode(sAUTH.Substring(6)).Split(':');
                        
                        if (Membership.ValidateUser(sCredentials[0], sCredentials[1])) // TODO: Not really needed
                        {
                            var UserPrincipal = new GenericPrincipal(new GenericIdentity(sCredentials[0] + "@itk.local"), new string[0]);
                            application.Context.User = UserPrincipal;
                            response.Cookies["httpauthsession"].Value = sCredentials[0] + "@itk.local";
                            response.Cookies["httpauthsession"].Path = "/";
                        }
                    }
                }
                else if (request.Cookies["httpauthsession"] != null && !String.IsNullOrEmpty(request.Cookies["httpauthsession"].Value))
                {
                    var UserPrincipal = new GenericPrincipal(new GenericIdentity(request.Cookies["httpauthsession"].Value), new string[0]);
                    application.Context.User = UserPrincipal;
                }
                else
                {
                    var UserPrincipal = new GenericPrincipal(new NewGenericIdentity("Anonymous"), new string[0]); // Using Name="Anonymous" here, to make it behave closer to prod, since that is the name SingleSignOnIdentity (ADFS) uses for anonymous users.
                    application.Context.User = UserPrincipal;
                }
            }
        }

        // This "fixes" IsAuthenticated in GenericIdentity. In GenericIdentity, the property returns false iff name="". We also want it to return false if name = "anonymous" (ignoring case), since that is 
        // the name SingleSignOnIdentity uses for anonymous users (check decompilation of the two classes). This makes this Basic Authentication class behaves closer to prod.
        private class NewGenericIdentity : GenericIdentity
        {
            public NewGenericIdentity(string name) : base(name) {}

            public override bool IsAuthenticated
            {
                get
                {
                    if (Name != null && Name.ToLower() == "anonymous") return false;
                    return base.IsAuthenticated;
                }
            }
        }

        private bool ResponseCookieExists(HttpApplication a, string key)
        {
            return a.Response.Cookies.AllKeys.ToList().Exists(x => x == key);
        }

        private void OnEndRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;

            bool cookieexpired = application.Request.Cookies["httpauthsession"] != null && application.Request.Cookies["httpauthsession"].Expires != DateTime.MinValue; // Check if the cookie is still a session cookie
            bool cookiehaslogin = (application.Request.Cookies["httpauthsession"] != null && !String.IsNullOrWhiteSpace(application.Request.Cookies["httpauthsession"].Value));
            /*    ||
                                    (ResponseCookieExists(application, "httpauthsession") && !String.IsNullOrWhiteSpace(application.Response.Cookies["httpauthsession"].Value));*/

            if (application.Response.StatusCode == 401 && (cookieexpired || !cookiehaslogin))
            {
                application.Response.AddHeader("WWW-Authenticate", "BASIC Realm=myDamco");
            }
        }

        private string Base64Decode(string EncodedData)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();  
                System.Text.Decoder utf8Decode = encoder.GetDecoder();    
                byte[] todecode_byte = Convert.FromBase64String(EncodedData);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                return new String(decoded_char);
            }
            catch(Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }

        public void Dispose()
        {
            // TODO: implement
        }
    }
}
