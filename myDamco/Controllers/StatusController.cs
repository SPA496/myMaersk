using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using myDamco.Config;
using myDamco.Database;
using myDamco.Utils;
using Newtonsoft.Json;
using ResourceHelper;
using UAMSharp;
using ReportingClient = myDamco.Reporting.ReportingWebServicesSoapBindingClient;
using DocManClient = myDamco.DocumentManagement.RecentPouchWidgetWSSoapBindingClient;

namespace myDamco.Controllers
{
    /// <summary>
    /// 
    /// This controller runs a number of tests, to check if any of the services MyDamco relies on are down or if parts 
    /// of the MyDamco system itself is down (e.g. database).
    /// 
    /// Calling the Status action with no parameters returns a webpage displaying the result of each test. 
    /// The http status code of this page is 500 if the database or UAM is down (or internal error), otherwise 200.
    /// 
    /// Calling the Status action with any of the parameters in the TestNames class, runs these tests only, and returns
    /// the result as JSON instead of HTML. 
    /// The http status code of the returned response is 500 if any tests failed, otherwise 200. 
    /// (Example: https://portal.damco.com/status?uam&reporting runs only the UAM and Reporting tests and returns JSON)
    /// 
    /// Note: This controller is not protected by ADFS and should work even if no user is logged in.
    /// 
    /// Note: The load-balancer uses this page to detect whether myDamco is down on this web-server. 
    ///       If the HTML-page returns a HTTP status code other than 200, the load-balancer considers myDamco as being down. 
    ///       This is how it detects if myDamco is up or down.
    ///       (There are 2 web-servers in each environment, so if myDamco is down on one of these web-servers (returns != 200), 
    ///       the load-balancer will send the users to the other web-server)
    ///       (Note, the LB does not check if the JSON responses returns 200 or not. It is only the HTML page it checks.)
    /// 
    /// TODO: Implement checks for all the services we use
    /// 
    /// </summary>
    public class StatusController : Controller
    {
        // The test-user used in the tests requiring UAM login (it is assumed that this user exists in all environments (dev, test, prod, ...))
        private string testUserLoginId = WebConfigurationManager.AppSettings["StatusTestUser"];
        private string testUserRoleId  = WebConfigurationManager.AppSettings["StatusTestUserRole"];

        // Represents the result of a test run 
        public class StatusResult
        {
            public readonly string testName; // This name is used as URL-parameter to run the test, is used in the JSON and is used as IDs in the HTML.
            public readonly bool success;
            public readonly string message;

            public StatusResult(string testName, bool success, string errorMessage)
            {
                if (testName == null || errorMessage == null) success = false; // If this happens, something is seriously wrong => not success.
                this.testName = testName ?? "(unknown)";
                this.success = success;
                this.message = errorMessage ?? "(message was null)";
            }
        }

        // The names of all tests
        private class TestNames
        {
            public const string DATABASE = "database";                      // Check if we can connect to the database
            public const string DATABASESCHEMA = "databaseschema";          // Check if the database schema corresponds to the schema expected by our Entity Framework model
            public const string UAM = "uam";                                // Check if we can connect to UAM 
            public const string ELEARNING = "elearning";                    // Check E-Learning
            public const string REPORTING = "reporting";                    // Check the webservices used in reporting widgets
            public const string DOCUMENTMANAGEMENT = "documentmanagement";  // Check the webservice used in the docman widget
            public const string PIWIK = "piwik";                            // Check if we can connect to the Piwik server (unless piwik is disabled)
            public const string LDAP = "ldap";                              // Check if we can connect to the LDAP server
            public const string CACHEDIR = "cachedir";                      // Check if the webserver-user has read and write access to the CSS/JS/Image-cache dir
        }

        public class StatusException : Exception
        {
            public StatusException() {}
            public StatusException(string message) : base(message) {}
            public StatusException(string message, Exception inner) : base(message, inner) { }
        }

        public ActionResult Status()
        {
            // If no query parameters, render a webpage, otherwise return JSON response.
            return Request.Url.Query == ""
                 ? StatusHtmlPage()
                 : StatusJson();
        }

        private ActionResult StatusHtmlPage()
        {
            Dictionary<string, Widget> widgets = null;
            
            // The tests to be called using ajax
            IList<String> testsToRun = new List<String>();
            testsToRun.Add(TestNames.DATABASE);
            testsToRun.Add(TestNames.DATABASESCHEMA);
            testsToRun.Add(TestNames.UAM);
            testsToRun.Add(TestNames.ELEARNING);
            testsToRun.Add(TestNames.REPORTING);
            testsToRun.Add(TestNames.DOCUMENTMANAGEMENT);
            testsToRun.Add(TestNames.PIWIK);
            testsToRun.Add(TestNames.LDAP);
            testsToRun.Add(TestNames.CACHEDIR);

            // run the tests which must cause the webpage return 500 if they fail (they will be run again when called by ajax)
            var testResults = new List<StatusResult>();
            testResults.Add(CheckUAM());
            testResults.Add(CheckDatabase(ref widgets));

            var success = testResults.All(x => x.success); // All tests returned true
            if (!success)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true; // <- otherwise IIS throws away the content and replaces it with its standard 500 error page on prod/preprod/integration/test. (http://stackoverflow.com/questions/4495961/how-to-send-a-status-code-500-in-asp-net-and-still-write-to-the-response)
                TryLogFailuresToElmah(testResults);
            }

            return View(testsToRun);
        }

        private ActionResult StatusJson()
        {
            string query = Request.Url.Query;
            if (query.Length > 1) query = query.Substring(1);
            string[] queryParams = query.Split(new Char[] { '&' }).Select(s => (s.Contains('=') ? s.Substring(0, s.IndexOf("=")) : s).Trim()).ToArray();

            var testResults = new List<StatusResult>();
            Dictionary<string, Widget> widgets = null;

            if (queryParams.Contains(TestNames.DATABASE))
            {
                testResults.Add(CheckDatabase(ref widgets));
            }

            if (queryParams.Contains(TestNames.DATABASESCHEMA))
            {
                testResults.Add(CheckDatabaseSchema());
            }

            if (queryParams.Contains(TestNames.ELEARNING))
            {
                if (widgets == null) CheckDatabase(ref widgets);
                testResults.Add(CheckElearning(widgets));
            }

            if (queryParams.Contains(TestNames.REPORTING))
            {
                if (widgets == null) CheckDatabase(ref widgets);
                testResults.Add(CheckReporting(widgets));
            }

            if (queryParams.Contains(TestNames.DOCUMENTMANAGEMENT))
            {
                if (widgets == null) CheckDatabase(ref widgets);
                testResults.Add(CheckDocumentManagement(widgets));
            }

            if (queryParams.Contains(TestNames.UAM))
            {
                testResults.Add(CheckUAM());
            }

            if (queryParams.Contains(TestNames.PIWIK))
            {
                testResults.Add(CheckPiwik());
            }

            if (queryParams.Contains(TestNames.LDAP))
            {
                testResults.Add(CheckLdap());
            }

            if (queryParams.Contains(TestNames.CACHEDIR))
            {
                testResults.Add(CheckCacheDirAccess());
            }

            var success = testResults.All(x => x.success); // All tests returned true
            if (!success)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true; // <- otherwise IIS throws away the content and replaces it with its standard 500 error page on prod/preprod/integration/test. (http://stackoverflow.com/questions/4495961/how-to-send-a-status-code-500-in-asp-net-and-still-write-to-the-response)
                TryLogFailuresToElmah(testResults);
            }

            string json = CreateJsonResponse(testResults);
            return Content(json, "application/json");
        }

        // Converts a list of StatusResults to a JSON string.
        private string CreateJsonResponse(IList<StatusResult> statusResults)
        {
            string json = "{\n";

            for (int i=0; i<statusResults.Count; i++) 
            {
                StatusResult statusResult = statusResults[i] ?? new StatusResult("(unknown test)", false, "Internal error: statusResult object was null, when creating the JSON string!");
                string message = HttpUtility.JavaScriptStringEncode(statusResult.message.Replace("\n","<br>"));

                json += "    \"" + statusResult.testName + "\" : {\"result\" : \"" + statusResult.success.ToString().ToLower() + "\", \"message\" : \"" + message + "\"} ";
                if (i < statusResults.Count - 1) json += ",";
                json += "\n";
            }

            json += "}";
            return json;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.ClearError();
                this.Content("Internal error: An uncaught exception caused OnException to be executed: " + exception.Message + ". The exception has been logged to ELMAH.", "text/plain").ExecuteResult(this.ControllerContext);
            }
        }

        // Try logging any failures to elmah (Naturally if the DB is down, nothing can be logged)
        private void TryLogFailuresToElmah(IList<StatusResult> testResults)
        {
            try { testResults.Where(x => !x.success).ToList().ForEach(x => Elmah.ErrorSignal.FromCurrentContext().Raise(new StatusException(x.testName + ": " + x.message))); }
            catch (Exception) { /* ignore and give up */ }
        }

        private StatusResult CheckUAM()
        {
            try
            {
                UAMClient uam = new UAMClient();

                // try calling methods which contacts the UAM service (Since caching is not enabled on the UAMClient "uam", these calls should contact the service)
                //UAMRole role = uam.GetCurrentRole(testUserLoginId);
                //UAMUserProfile profile = uam.GetUserProfile(testUserLoginId);
                UAMUserSimple[] users = uam.GetUsersByRole(25); // 25 is the role-id for the "myDamco Project Team" role - we assume this role will always exist!
            }
            catch (Exception e)
            {
                return new StatusResult(TestNames.UAM, false, "Exception during UAM test (" + e.GetType().ToString() + "): " + e.Message);
            }
            return new StatusResult(TestNames.UAM, true, "");
        }

        // Check that we can connect to piwik. (Note that since the piwik login page is protected by ADFS, we can't check that - so we check the JS file instead)
        private StatusResult CheckPiwik()
        {
            try
            {
                // check configuration to see if piwik is disabled
                string piwikServerUrl = Settings.Piwik.ServerUrl;
                if (string.IsNullOrWhiteSpace(piwikServerUrl) || !Settings.Piwik.TrackingEnabled) return new StatusResult(TestNames.PIWIK, true , "Is not enabled on this server. Ignored.");

                // create request - we check the JS file since it is not protected by ADFS (the piwik login page is)
                string siteScheme = ControllerUtil.GetSiteScheme(); // "http" or "https"
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(siteScheme + piwikServerUrl + "piwik.js");
                request.Method = "GET";

                // Do request via proxy server (When we are inside Damco's network, we can only connect to the outside through their proxy-server (also for sites which point back to damco again))
                // (Copy/Pasted from ServicesController.GetExternalNewsFeedXml - make an util method for this?)
                if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["RSSNewsProxy"])) // TODO: Hack to use RSSNewsProxy - but it is the proxy. Maybe call it something else in web.config?
                {
                    var proxy = new WebProxy(ConfigurationManager.AppSettings["RSSNewsProxy"]);
                    request.Proxy = proxy;
                }

                // Support https
                // (Copy/Pasted from ServicesController.GetExternalNewsFeedXml - make an util method for this?)
                if ("https" == request.RequestUri.Scheme)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((x, y, z, xyz) => true);
                    request.PreAuthenticate = true;
                    request.KeepAlive = true;
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.AllowAutoRedirect = true;
                }

                // send request and receive + check response
                using (WebResponse response = request.GetResponse())        // <- send request and receive response
                using (Stream dataStream = response.GetResponseStream())    // <- for reading the response content
                using (StreamReader reader = new StreamReader(dataStream))  // <- for reading the response content easily
                {
                    // Fail if status code is not 200
                    var statusCode = ((HttpWebResponse) response).StatusCode;
                    if (statusCode != HttpStatusCode.OK)
                        return new StatusResult(TestNames.PIWIK, false, "Returned "+statusCode);

                    // Very simple check for that we get JS (and not HTML) back in the response (just to check that it doesn't return an empty response or total nonsense) and that piwik is mentioned (so it's not the wrong server)
                    string responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Contains("<html>") || !responseFromServer.Contains("piwik"))
                        return new StatusResult(TestNames.PIWIK, false, "Unexpected response content");
                }
            }
            catch (Exception e)
            {
                return new StatusResult(TestNames.PIWIK, false, "Exception during piwik test (" + e.GetType().ToString() + "): " + e.Message);
            }
            return new StatusResult(TestNames.PIWIK, true, "");
        }

        private StatusResult CheckLdap()
        {
            try
            {
                var url = WebConfigurationManager.ConnectionStrings["ADAM"].ConnectionString;
                var conn = LDAPUtils.LDAPAccount.LdapConnectBind(new Uri(url), "test1234_connection_check_from_mydamco", "");
                // if no exception at this point, we could successfully connect and bind (the server exists! And that's all we currently want to check.)
            }
            catch (Exception e)
            {
                return new StatusResult(TestNames.LDAP, false, "Exception during LDAP test (" + e.GetType().ToString() + "): " + e.Message);
            }
            return new StatusResult(TestNames.LDAP, true, "");
        }

        private StatusResult CheckReporting(Dictionary<string, Widget> widgets)
        {
            bool success = true;
            string message = "";

            var reportingWidgetNames = new string[] { "recentsearches", "recentreports", "scheduledreports" };

            // Try to connect to the webservice of each widget and do basic checks on their output 
            foreach (string widgetName in reportingWidgetNames)
            {
                try
                {
                    // Check if the widget was in the dictionary returned from the database call
                    Widget widget;
                    if (!widgets.TryGetValue(widgetName, out widget))
                    {
                        success = false;
                        message += "Widget '" + widgetName + "' was not found in the database<br>";
                        continue;
                    }

                    // Get service configuration
                    var conftemplate = new { remoteaddress = "", cache = 0 };
                    var conf = JsonConvert.DeserializeAnonymousType(widget.ServiceConfiguration, conftemplate);
                    var reportingClient = new ReportingClient("ReportingWebServicesSoapBinding", conf.remoteaddress);

                    // Make service request
                    string response = widgetName == "recentsearches" ? reportingClient.GetRecentSearches("", testUserLoginId, int.Parse(testUserRoleId))
                                    : widgetName == "recentreports" ? reportingClient.GetRecentReports("", testUserLoginId)
                                    : widgetName == "scheduledreports" ? reportingClient.GetScheduleReports("", testUserLoginId)
                                    : null;

                    // Check that the request is as expected (basic tests only)
                    XElement xml = XElement.Parse(response);

                    // perform basic checks of the webservice output
                    if (xml == null) throw new Exception("xml output is null");
                    if (xml.Name != "widget") throw new Exception("xml root element was not <widget>");
                }
                catch (Exception e)
                {
                    success = false;
                    message += widgetName + ": Exception occurred while trying to contact webservice ("+e.GetType().ToString()+"): " + e.Message + "<br>";
                }
            }

            return new StatusResult(TestNames.REPORTING, success, message);
        }


        private StatusResult CheckDocumentManagement(Dictionary<string, Widget> widgets)
        {
            bool success = true;
            string message = "";

            var widgetName = "recentpouches";
            
            try
            {
                // Check if the widget was in the dictionary returned from the database call
                Widget widget;
                if (!widgets.TryGetValue(widgetName, out widget))
                    return new StatusResult(TestNames.DOCUMENTMANAGEMENT, false, "Widget '" + widgetName + "' was not found in the database<br>");

                // Get service configuration
                var conftemplate = new { remoteaddress = "", cache = 0 };
                var conf = JsonConvert.DeserializeAnonymousType(widget.ServiceConfiguration, conftemplate);
                DocManClient docManClient = new DocManClient("RecentPouchWidgetWSSoapBinding", conf.remoteaddress);

                // Make service request
                string response = docManClient.getReleasedPouchData(testUserLoginId, testUserRoleId);

                // Check that the request is as expected (basic tests only)
                XElement xml = XElement.Parse(response);

                // perform basic checks of the webservice output
                if (xml == null) throw new Exception("XML output is null");
                if (xml.Name != "widget") throw new Exception("XML root element was not <widget>");
            }
            catch (Exception e)
            {
                success = false;
                message += widgetName + ": Exception occurred when trying to contact webservice (" + e.GetType().ToString() + "): " + e.Message + "<br>";
            }
        
            return new StatusResult(TestNames.DOCUMENTMANAGEMENT, success, message);
        }

        private StatusResult CheckElearning(Dictionary<string, Widget> widgets)
        {
            bool success = true;
            string message = "";


            if (widgets.ContainsKey("elearning"))
            {
                Widget widget = widgets["elearning"];

                var conftemplate = new { folders = new[] { new { application = "", folder = "" } } };
                var configuration = JsonConvert.DeserializeAnonymousType(widget.ServiceConfiguration, conftemplate);
                foreach (var conf in configuration.folders)
                {
                    if (!Directory.Exists(conf.folder))
                    {
                        success = false;
                        message += "Folder " + conf.folder + " does not exist.<br>";
                    }
                }
            }
            else
            {
                success = false;
                message += "E-learning widget does not exist in the database.<br>";
            }
            return new StatusResult(TestNames.ELEARNING, success, message);
        }

        // Check that we can read, write, list content of the JS/CSS/Image cache directory. During web-server set up (and maybe maintanence) it's easy to forget setting this up, so this is a test for it.
        // NOTE! THIS NATURALLY ONLY CHECKS IT FOR THE WEB-SERVER IT IS RUNNING ON, NOT THE OTHER WEB-SERVER. So should ideally run this on both servers, to be sure it's ok.
        private StatusResult CheckCacheDirAccess()
        {
            const string FILENAME = "statuschecktest.txt";

            try
            {
                var server = HttpContext.Server;
                string cacheFolder = server.MapPath(new HtmlResources().CacheFolder);
                string filepath = cacheFolder + FILENAME;

                // create a file in the cache dir, to see if we have write permission
                using (StreamWriter writer = System.IO.File.CreateText(filepath))
                {
                    writer.WriteLine("test");
                }

                // get list of files in that dir, to see if we have permission to do that (check that the file we just wrote is there...)
                string[] filesInDir = Directory.GetFiles(cacheFolder);
                if (!filesInDir.Any(x => x.EndsWith(FILENAME)))
                    return new StatusResult(TestNames.CACHEDIR, false, "Testfile was not in dir");

                // alter the file, to see if we have permission to do that 
                using (StreamWriter writer = System.IO.File.AppendText(filepath))
                {
                    writer.WriteLine("test2");
                }

                // delete the file again, to see if we have permission to do that (and to clean up)
                System.IO.File.Delete(filepath);
            }
            catch (Exception e)
            {
                return new StatusResult(TestNames.CACHEDIR, false, "Exception during test (" + e.GetType().ToString() + "): " + e.Message);
            }

            return new StatusResult(TestNames.CACHEDIR, true, "");
        }

        // Apart from checking the database connection, this also returns the widgets in the database through the ref parameter
        private StatusResult CheckDatabase(ref Dictionary<string, Widget> widgets)
        {
            bool success = true;
            string message = "";

            try
            {
                using (myDamcoEntities myDamcoDB = new myDamcoEntities())
                {
                    widgets = myDamcoDB.Widget.ToList()
                        .Where(w => !w.Disabled)
                        .AsEnumerable()
                        .ToDictionary(w => w.UId, w => w);
                }
            }
            catch
            {
                success = false;
                message = "Could not connect to database";
            }

            if (widgets == null)  // in case db is down, don't set widgets to null.
                widgets = new Dictionary<string, Widget>();

            return new StatusResult(TestNames.DATABASE, success, message);
        }

        // Check if all tables exists and that their structure are as extected by our Entity Framework model. 
        private StatusResult CheckDatabaseSchema()
        {
            bool success = true;
            string message = "";

            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["myDamcoDatabase"].ConnectionString;
                string providerName     = System.Configuration.ConfigurationManager.ConnectionStrings["myDamcoDatabase"].ProviderName;

                var dbChecker = new DatabaseSchemaChecker();
                string dbSsdlXmlString = dbChecker.CreateSSDLFromDatabaseSchema(connectionString, providerName);
                string efSsdlXmlString = dbChecker.ReadEntityFrameworkModelSSDL();
                dbChecker.CompareSsdlXmlStrings(dbSsdlXmlString, efSsdlXmlString);
            }
            catch (StatusException e)
            {
                success = false;
                message = e.Message;
            }
            catch (Exception e)
            {
                success = false;
                message = "An exception occurred during testing ("+e.GetType().Name+"): "+e.Message;
            }

            return new StatusResult(TestNames.DATABASESCHEMA, success, message);        
        }




        // Used to check if the database schema is as expected. This is done by gererating an SSDL xml-string from the current database schema. Then this SSDL XML is compared with the SSDL XML file of the 
        // entity framework model we use. This checks the tables, columns and column types. (It is ok that there are more tables in the current database than there is in our EF model. But the other way around, 
        // all tables in the EF model must exist in the database. Furthermore, the same columns must exist, all columns must have the same names and types.)
        // (SSDL = the storage (database) model used by EF. I.e. what the database looks like. There are also CSDL and MDL in an EDMX file (and in the assembly).)
        private class DatabaseSchemaChecker
        {
            /* Generate SSDL from the current database. http://blogs.msdn.com/b/wriju/archive/2010/09/28/entity-framework-finding-the-differences-between-production-database-and-model-s-schema.aspx
             * Will throw exceptions on error, which must be catched by the caller.
             */
            public string CreateSSDLFromDatabaseSchema(string connectionString, string providerName)
            {

                // Retrieve the database name from the connection string
                var connStrBuilder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = connStrBuilder.InitialCatalog;

                // Generate the SSDL from the current database
                string ssdlNamespace = databaseName + "Model.Store";
                EntityStoreSchemaGenerator essg = new EntityStoreSchemaGenerator(providerName, connectionString, ssdlNamespace);
                IList<EdmSchemaError> ssdlErrors = essg.GenerateStoreMetadata();

                // Check for errors (and warnings)
                if (ssdlErrors != null && ssdlErrors.Count > 0)
                {
                    bool errorsEncountered = false;
                    string errorMessage = "";
                    foreach (EdmSchemaError error in ssdlErrors)
                    {
                        if (error.Severity == EdmSchemaErrorSeverity.Error)
                            errorsEncountered = true;
                        errorMessage += error + "<br>";
                    }
                    if (errorsEncountered)
                        throw new StatusException("Error occurred during SSDL generation from current database (" + ssdlErrors.Count + " errors and warnings):<br> " + errorMessage);
                }

                // Get the generated XML
                StringBuilder xmlSb = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(xmlSb);
                essg.WriteStoreSchema(xmlWriter);
                xmlWriter.Flush(); // needed to write everything to the string builder without being cutoff
                return xmlSb.ToString();
            }

            /* Read the SSDL used by our Entity Framework model from the assembly.
             * Will throw exceptions on error, which must be catched by the caller.
             */
            public string ReadEntityFrameworkModelSSDL()
            {
                //Reading SSDL from assembly (the one used by our entity framework model)
                //XElement ssdlXML = XElement.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Database.myDamco.ssdl")); // **TODO** DOES THESE FILES ALWAYS EXIST? ALSO IN PROD?
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Database.myDamco.ssdl");

                // read it into a string
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }


            /* This method will compare the SSDL from the EF (Entity Framework) with the SSDL generated from the 
             * database, to see if the database structure corresponds to what our EF-model expects.
             * 
             * Will throw exceptions on error, which must be catched by the caller.
             * See the spec of SSDL XML-files (what each element means) here: http://msdn.microsoft.com/en-us/library/vstudio/bb399559%28v=vs.100%29.aspx
             * (TODO: Would be nice if it could show all errors, instead of stopping at the first error encountered)
             */
            public void CompareSsdlXmlStrings(string dbSsdlXmlString, string efSsdlXmlString)
            {

                // Convert to xml
                XElement dbXmlRoot = XElement.Parse(dbSsdlXmlString);
                XElement efXmlRoot = XElement.Parse(efSsdlXmlString);

                /*
                // MAYBE USE THIS INSTEAD OF XML?
                StoreItemCollection dbSsdl = ...;
                StoreItemCollection efSsdl = ...;
                */

                // namespaces (might be different in DB and EF files)
                XNamespace dbNs = dbXmlRoot.GetDefaultNamespace();
                XNamespace efNs = efXmlRoot.GetDefaultNamespace();

                // Check all tables in the EF-XML are in the DB-XML (The other way around is not necessary - it's ok for the current database to have more tables than used by the model, but it must contain all the tables used by the model)
                // (An EntitySet represents a table (but does not contain the columns - they are defined in EntityTypes later))
                XElement dbEntityContainer = dbXmlRoot.Element(dbNs + "EntityContainer");
                XElement efEntityContainer = efXmlRoot.Element(efNs + "EntityContainer");
                if (dbEntityContainer == null) throw new StatusException("The SSDL for the current database did not contain an <EntityContainer> element");
                if (efEntityContainer == null) throw new StatusException("The SSDL for the entity framework model did not contain an <EntityContainer> element");

                IEnumerable<XElement> dbEntitySets = dbEntityContainer.Elements(dbNs + "EntitySet");
                IEnumerable<XElement> efEntitySets = efEntityContainer.Elements(efNs + "EntitySet");
                if (dbEntitySets.Count() == 0) throw new StatusException("The SSDL for the current database did not contain an <EntitySet> element in the <EntityContainer> element");
                if (efEntitySets.Count() == 0) throw new StatusException("The SSDL for the entity framework model did not contain an <EntitySet> element in the <EntityContainer> element");

                foreach (XElement efEntitySet in efEntitySets)
                {
                    string efTableName = efEntitySet.Attribute("Name").Value;
                    bool foundInDb = dbEntitySets.Where(e => e.Attribute("Name").Value == efTableName).Count() > 0;
                    if (!foundInDb) throw new StatusException("Table '" + efTableName + "' was not found in the database (EntitySet)");
                }

                // For each table (in EF - again, it's ok if there are more tables in DB), check the columns (same names and types), that there are the same number of columns.
                // (for <Property> see http://msdn.microsoft.com/en-us/library/vstudio/bb399168%28v=vs.100%29.aspx, for <EntityType> see http://msdn.microsoft.com/en-us/library/vstudio/bb399579%28v=vs.100%29.aspx)
                IEnumerable<XElement> dbEntityTypes = dbXmlRoot.Elements(dbNs + "EntityType");
                IEnumerable<XElement> efEntityTypes = efXmlRoot.Elements(efNs + "EntityType");
                if (dbEntityTypes.Count() == 0) throw new StatusException("The SSDL for the current database did not contain any <EntityType> elements");
                if (efEntityTypes.Count() == 0) throw new StatusException("The SSDL for the entity framework model did not contain any <EntityType> elements");
                foreach (XElement efEntityType in efEntityTypes)
                {

                    // get the EntityType from the DB-XML for the same table.
                    string efTableName = efEntityType.Attribute("Name").Value;
                    XElement dbEntityType = dbEntityTypes.Where(e => e.Attribute("Name").Value == efTableName).SingleOrDefault(); // NOTE: Throws exception if >1 element, but should not be two tables with the same name
                    if (dbEntityType == null)
                        throw new StatusException("Table " + efTableName + " was not found in the database (EntityType)");

                    // Loop through the columns (<Property> elements) of this table and compare them
                    IEnumerable<XElement> dbProperties = dbEntityType.Elements(dbNs + "Property");
                    IEnumerable<XElement> efProperties = efEntityType.Elements(efNs + "Property");
                    if (dbProperties.Count() != efProperties.Count())
                        throw new StatusException("Table " + efTableName + " does not have the expected number of columns in the database (expected = " + efProperties.Count() + ", actual = " + dbProperties.Count() + ")");// "(efEntityType='<pre>"+HttpUtility.HtmlEncode(efEntityType)+"</pre>', dbEntityType='<pre>"+HttpUtility.HtmlEncode(dbEntityType)+"</pre>')");
                    if (dbProperties.Count() == 0) throw new StatusException("The SSDL for the both the current database and the entity framework model did not contain any Property elements for the table " + efTableName);

                    foreach (XElement efProperty in efProperties)
                    {
                        // get the Property from the DB-XML for the same column (in the same table).
                        string efPropertyName = efProperty.Attribute("Name").Value;
                        XElement dbProperty = dbProperties.Where(e => e.Attribute("Name").Value == efPropertyName).SingleOrDefault(); // NOTE: Throws exception if >1 element, but should not be two columns with the same name in a table
                        if (dbProperty == null)
                            throw new StatusException("The column '" + efPropertyName + "' of table '" + efTableName + "' was not found in the database");

                        // Check the type of the column (we have just checked the name, by determining that it exists) and other values associated with a column
                        IEnumerable<XAttribute> dbPropAttribs = dbProperty.Attributes();
                        IEnumerable<XAttribute> efPropAttribs = efProperty.Attributes();
                        if (dbPropAttribs.Count() != efPropAttribs.Count())
                            throw new StatusException("The Property element for column '" + efPropertyName + "' in table '" + efTableName + "' did not have the expected number of attributes in the database SSDL (expected = " + efPropAttribs.Count() + ", actual = " + dbPropAttribs.Count() + ")"); // (efProperty='<pre>" + HttpUtility.HtmlEncode(efProperty) + "</pre>', dbProperty='<pre>" + HttpUtility.HtmlEncode(dbProperty) + "</pre>')");
                        if (dbPropAttribs.Count() == 0) throw new StatusException("The Property element for column '" + efPropertyName + "' in table '" + efTableName + "' did not have any attributes in the SSDL for both the database and the entity framework model");

                        foreach (XAttribute efPropAttrib in efPropAttribs)
                        {
                            XAttribute dbPropAttrib = dbPropAttribs.Where(e => e.Name == efPropAttrib.Name).SingleOrDefault(); // NOTE: Throws exception if >1 attributes, but should not be two attributes with the same name in an element.
                            if (dbPropAttrib == null)
                                throw new StatusException("The Property element for column '" + efPropertyName + "' in table '" + efTableName + "' did not have an attribute with the name '" + efPropAttrib.Name + "'.");

                            if (dbPropAttrib.Value != efPropAttrib.Value) // Is this ok? Or should i only check attributes i know about? (This is easiest for sure and should be correct, unless there is some non-determinism / version-difference in the SSDL generation)
                                throw new StatusException("The attribute '" + efPropAttrib.Name + "' of the Property element for column '" + efPropertyName + "' in table '" + efTableName + "' had a different value in the database than expected (expected='" + efPropAttrib.Value + "', actual='" + dbPropAttrib.Value + "')");
                        }
                    }

                    // TODO? Also check the keys are the same?
                    // TODO? Could also check if foreign keys are the same (<association> elements)
                }
            }
        }
    }

}
