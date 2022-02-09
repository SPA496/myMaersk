using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Profile;
using UAMSharp;

namespace myDamco.Access.Authorization
{
    public class UAMProfileProvider : ProfileProvider
    {
        private UAMClient uam;
        private UAMRole[] roles;

        public UAMProfileProvider() : base()
        {
            uam = new UAMClient();
            uam.EnableWebCaching(HttpContext.Current.Cache);
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            UAMUserProfile profile = uam.GetUserProfile(usernameToMatch.ToString());
            ProfileInfoCollection collection = new ProfileInfoCollection();
            ProfileInfo info = new ProfileInfo(profile.LoginId, false, DateTime.Now, DateTime.Now, 0);
            collection.Add(info);
            totalRecords = 1;
            return collection;
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
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

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            // Remove domain from username tlb013@itk.local
            var username = context["UserName"].ToString().Split('@')[0];

            UAMUserProfile profile = uam.GetUserProfile(username);
            roles = uam.GetAllUserRoles(username);

            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();
            foreach (SettingsProperty prop in collection)
            {
                SettingsPropertyValue pv = new SettingsPropertyValue(prop);
                switch (prop.Name)
                {
                    case "RoleId" :
                        pv.PropertyValue = profile.Role.Id;
                        break;
                    case "RoleName" :
                        pv.PropertyValue = profile.Role.Name;
                        break;
                    case "RoleOrganization" :
                        pv.PropertyValue = profile.Role.Organization.Name;
                        break;
                    case "AvailableRoles" :
                        pv.PropertyValue = roles;
                        break;
                    case "RoleOrganizationObj" :
                        break;
                    default :
                        FieldInfo pi = typeof(UAMUserProfile).GetField(pv.Name);
                        pv.PropertyValue = pi.GetValue(profile);
                        break;
                }
                
                svc.Add(pv);
            }

            return svc;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            throw new NotImplementedException();
        }
    }
}