using System;
using System.Collections.Generic;
using System.Configuration.Provider;
//using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using UAMSharp;

namespace myDamco.Access.Authorization
{
    public class UAMRoleProvider : RoleProvider
    {
        private static UAMClient UAM = new UAMClient();

        public UAMRoleProvider()
        {
            UAM = new UAMClient();
            UAM.EnableWebCaching(HttpContext.Current.Cache);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            // Remove domain from username tlb013@itk.local
            // username = username.Split('@')[0]; 
            var newClaim = (HttpContext.Current.User.Identity as System.Security.Claims.ClaimsIdentity).FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            username = (newClaim != null ? newClaim.Value.Split('@')[0].ToLower() : null);
            //string strFilePath = @"C:\\log\CS_Log.txt";
            // TODO: Implement configuration rule for how we map to ASP.Net fx. in xml <RoleMap>UAM:{0}:{1}</RoleMap>
            string appMap = "UAM:{0}";
            string opMap = "UAM:{0}:{1}";

            UAMRole role;
            try
            {
                role = UAM.GetCurrentRole(username);
                //using (StreamWriter sw = new StreamWriter(strFilePath, false))
                //{
                //    sw.WriteLine("_______________UAM Role provider POST UAM call__________________________");
                //    sw.WriteLine("Name: " + username.ToString());
                //    sw.WriteLine("role: " + role.Id);
                //    sw.WriteLine("_______________UAM Role provider POST UAM call__________________________");
                //}
            }
            catch (Exception ex)
            {
                //using (StreamWriter sw = new StreamWriter(strFilePath, false))
                //{
                //    sw.WriteLine("_______________UAM Role provider page Exception at UAM call__________________________");
                //    sw.WriteLine("Name: " + username.ToString());
                //    sw.WriteLine("opMap: " + opMap);
                //    sw.WriteLine("Exception : " + ex.InnerException + ex.StackTrace);
                //    sw.WriteLine("_______________UAM Role provider page Exception at UAM call__________________________");
                //}
                throw new ProviderException(ex.Message, ex);
            }

            List<string> result = new List<string>();

            foreach (var app in role.Applications)
            {
                result.Add(string.Format(appMap, app.Name));
                foreach (var op in app.Operations)
                {
                    result.Add(string.Format(opMap, app.Name, op.Name));
                }
            }

            return result.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var roles = GetRolesForUser(username);
            return roles.Contains(roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}