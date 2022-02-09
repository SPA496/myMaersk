using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using myDamco.Areas.MySupplyChainAssistantAdministration.Data;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Models
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FunctionValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return null;
            if (value is Function || value is int)
            {
                int fid = 0;
                if (value is Function)
                {
                    fid = ((Function)value).functionId;
                }
                else if (value is int)
                {
                    fid = (int)value;
                }
                if (fid <= 0) return new ValidationResult("The provided value is not a valid function identifier.");
                bool valid = false;
                using (var rep = new SCASqlCERepository())
                {
                    valid = rep.FunctionsExists(new List<int> { fid }).Count() == 0;
                }
                return valid ? null : new ValidationResult("The provided value could not be associated with a function.");
            }
            else if (typeof(IEnumerable<int>).IsAssignableFrom(value.GetType()))
            {
                var val = (IEnumerable<int>)value;
                if (val.Count() == 0) return null;

                IEnumerable<int> rejected = null;
                using (var rep = new SCASqlCERepository())
                {
                    rejected = rep.FunctionsExists(val);
                }
                return rejected.Count() == 0 ? null : new ValidationResult(string.Format("The identifier(s) {0} could not be associated with a function.", string.Join(", ", rejected)));
            }
            return new ValidationResult("The provided value is not a valid function identifier.");
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ApplicationValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return null;
            if (value is Application || value is int)
            {
                int aid = 0;
                if (value is Application)
                {
                    aid = ((Application)value).ApplicationId;
                }
                else if (value is int)
                {
                    aid = (int)value;
                }
                if (aid <= 0) return new ValidationResult("The provided value is not a valid application identifier.");
                bool valid = false;
                using (var rep = new SCASqlCERepository())
                {
                    valid = rep.GetApplication(aid) != null;
                }
                return valid ? null : new ValidationResult("The provided value could not be associated with an application.");
            }
            return new ValidationResult("The provided value is not a valid application identifier.");
        }
    }
}