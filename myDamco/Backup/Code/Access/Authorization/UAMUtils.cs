using System;
using System.Linq;

namespace myDamco.Access.Authorization
{

    public class UAMDataRight {
        
    }

    public class UAMUtils
    {
        // TODO: Implement
        public static bool UAMHasDataRight(string type, string becode, UAMDataRight[] userdatarights)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return true if the required operations exists in the operations list
        /// </summary>
        /// <param name="application">Application name</param>
        /// <param name="function">Function name</param>
        /// <param name="useroperations">List of operations the user has in following format UAM:APPLICATION:FUNCTION</param>
        /// <returns></returns>
        public static bool UAMHasOperation(string application, string function, string[] useroperations)
        {
            if (!String.IsNullOrEmpty(application))
            {
                var uamright = "UAM:" + application + (String.IsNullOrEmpty(function) ? "" : ":" + function);
                if (useroperations.Contains(uamright))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}