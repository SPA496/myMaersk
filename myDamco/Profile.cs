using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Profile;
using Newtonsoft.Json;
using UAMSharp;

namespace myDamco
{
    public class Profile : ProfileBase
    {
        //static string strFilePath = @"C:\\log\CS_Log.txt";

        [Display(Name = "Login Id")]
        public virtual string LoginId
        {
            get
            {
                return (this.GetPropertyValue("LoginId").ToString());
            }
            set
            {
                this.SetPropertyValue("LoginId", value);
            }
        }

        [Display(Name = "User Type")]
        public virtual string UserType
        {
            get
            {
                return (this.GetPropertyValue("Usertype").ToString());
            }
            set
            {
                this.SetPropertyValue("Usertype", value);
            }
        }

        [Display(Name = "First Name")]
        public virtual string FirstName
        {
            get
            {
                return (this.GetPropertyValue("FirstName").ToString());
            }
            set
            {
                this.SetPropertyValue("FirstName", value);
            }
        }

        [Display(Name = "Last Name")]
        public virtual string LastName
        {
            get
            {
                return (this.GetPropertyValue("LastName").ToString());
            }
            set
            {
                this.SetPropertyValue("LastName", value);
            }
        }

        [Display(Name = "E-mail")]
        public virtual string Email
        {
            get
            {
                return (this.GetPropertyValue("Email").ToString());
            }
            set
            {
                this.SetPropertyValue("Email", value);
            }
        }

        [Display(Name = "Addresss Line 1")]
        public virtual string AddressLine1
        {
            get
            {
                return (this.GetPropertyValue("AddressLine1") ?? "").ToString();
            }
            set
            {
                this.SetPropertyValue("AddressLine1", value);
            }
        }

        [Display(Name = "Addresss Line 2")]
        public virtual string AddressLine2
        {
            get
            {
                return (this.GetPropertyValue("AddressLine2") ?? "").ToString();
            }
            set
            {
                this.SetPropertyValue("AddressLine2", value);
            }
        }

        [Display(Name = "Country")]
        public virtual string Country
        {
            get
            {
                return (this.GetPropertyValue("Country") ?? "").ToString();
            }
            set
            {
                this.SetPropertyValue("Country", value);
            }
        }

        [Display(Name = "Phone")]
        public virtual string Phone
        {
            get
            {
                return (this.GetPropertyValue("Phone") ?? "").ToString();
            }
            set
            {
                this.SetPropertyValue("Phone", value);
            }
        }

        [Display(Name = "Organization")]
        public virtual UAMOrganization Organization
        {
            get
            {
                return ((UAMOrganization)this.GetPropertyValue("Organization"));
            }
            set
            {
                this.SetPropertyValue("Organization", value);
            }
        }

        [Display(Name = "Role Organization Object")]
        public virtual UAMOrganization RoleOrganizationObj
        {
            get
            {
                try
                {
                    var role = (from r in AvailableRoles where r.Id == RoleId select r).First();
                    return role.Organization;
                }
                catch
                {
                    return null;
                }
            }
        }

        [Display(Name = "Time Stamp")]
        public virtual DateTime TimeStamp
        {
            get
            {
                return ((DateTime)this.GetPropertyValue("TimeStamp"));
            }
            set
            {
                this.SetPropertyValue("TimeStamp", value);
            }
        }

        [Display(Name = "Original Profile")]
        public virtual UAMUserProfile OriginalProfile
        {
            get
            {
                return ((UAMUserProfile)this.GetPropertyValue("OriginalProfile"));
            }
            set
            {
                this.SetPropertyValue("OriginalProfile", value);
            }
        }

        [Display(Name = "Is Impersonation?")]
        public virtual bool isImpersonation
        {
            get
            {
                return ((bool)this.GetPropertyValue("isImpersonation"));
            }
            set
            {
                this.SetPropertyValue("isImpersonation", value);
            }
        }

        [Display(Name = "Role ID")]
        public virtual long RoleId
        {
            get
            {
                return ((long)this.GetPropertyValue("RoleId"));
            }
            set
            {
                this.SetPropertyValue("RoleId", value);
            }
        }

        [Display(Name = "Role Name")]
        public virtual string RoleName
        {
            get
            {
                return ((string)this.GetPropertyValue("RoleName"));
            }
            set
            {
                this.SetPropertyValue("RoleName", value);
            }
        }

        [Display(Name = "Role Organization")]
        public virtual string RoleOrganization
        {
            get
            {
                return ((string)this.GetPropertyValue("RoleOrganization"));
            }
            set
            {
                this.SetPropertyValue("RoleOrganization", value);
            }
        }


        [Display(Name = "AvailableRoles")]
        public virtual UAMRole[] AvailableRoles
        {
            get
            {
                return ((UAMRole[])this.GetPropertyValue("AvailableRoles")) ?? new UAMRole[0];
            }
            set
            {
                this.SetPropertyValue("AvailableRoles", value);
            }
        }

        public Dictionary<string, string> AsStringMap()
        {
            var result = new Dictionary<string, string>();
            PropertyInfo[] properties = typeof(Profile).GetProperties();
            foreach (PropertyInfo prop in properties.Where(p => p.Name != "Item"))
            {
                object value = prop.GetValue(this, null);
                string valuestr;
                switch (prop.Name)
                {
                    case "Organization":
                        var organization = value as UAMOrganization;
                        valuestr = organization != null ? organization.Name : null;
                        break;
                    default:
                        valuestr = prop.GetValue(this, null) as string;
                        break;
                }
                result.Add(prop.Name, valuestr);
            }
            return result;
        }

        public string AvailableRolesJSON()
        {
            var result = new { roles = AvailableRoles.Select(o => new { Id = o.Id, Name = o.Name, Organization = o.Organization.Name }) };
            return JsonConvert.SerializeObject(result);
        }

        public static Profile GetProfile(string username)
        {
            return Create(username) as Profile;
        }

        public static Profile GetProfile()
        {
            //  var username = HttpContext.Current.User == null ? null : HttpContext.Current.User.Identity.Name.Split('@')[0].ToLower();
            //if (!string.IsNullOrEmpty(username) && HttpContext.Current.User.Identity.IsAuthenticated)
            //{
            //    return GetProfile(username);
            //} LPS023-Commented to implement ADFS redirection Jul-26-21

            //var username = HttpContext.Current.User == null ? null : HttpContext.Current.User.Identity.Name.Split('@')[0].ToLower();
            //if (username == null)
            //{
            var newClaim = (HttpContext.Current.User.Identity as System.Security.Claims.ClaimsIdentity).FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var username = (newClaim != null ? newClaim.Value.Split('@')[0].ToLower() : null);
            //using (StreamWriter sw = (File.Exists(strFilePath)) ? File.AppendText(strFilePath) : File.CreateText(strFilePath))
            //{
            //    sw.WriteLine("HttpContext.Current.User.Identity.Name: " + username.ToString());
            //    sw.WriteLine("newClaim : " + newClaim.Value.ToString());
            //    sw.WriteLine("IsAuthenticated : " + HttpContext.Current.User.Identity.IsAuthenticated);
            //}

            // System.Security.Claims.Claim newClaimCustomized = new System.Security.Claims.Claim(ClaimTypes.Name, username);
            // (HttpContext.Current.User.Identity as System.Security.Claims.ClaimsIdentity).AddClaim(newClaimCustomized);
            if (!string.IsNullOrEmpty(username) && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var a = GetProfile(username);
                //using (StreamWriter sw = (File.Exists(strFilePath)) ? File.AppendText(strFilePath) : File.CreateText(strFilePath))
                //{
                //    sw.WriteLine("profile details: loginID " + a.LoginId);
                //    sw.WriteLine("profile details: RoleId " + a.RoleId);
                //    sw.WriteLine("profile details: UserName " + a.UserName);
                //}
                return a;
            }

            // }
            return null;
        }
    }
}
