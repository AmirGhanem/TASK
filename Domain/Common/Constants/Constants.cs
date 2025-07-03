using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Constants
{
    public class Roles : Admin
    {


        public const string USER = "User";
      

        public static List<string> GetAllRoles()
        {
            List<string> roles = new List<string>();

            // Get all public static readonly fields in the class
            var fields = typeof(Roles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                // Check if the field is a string and readonly
                if (field.FieldType == typeof(string))
                {
                    // Add the value to the list
                    roles.Add((string)field.GetValue(null));
                }
            }

            return roles;
        }
    }

    public class Admin
    {
        public const string ADMIN = "Admin";
    }
    public class Policies
    {
        public const string ADMIN_POLICY = "Admin_Policy";
        public const string USER_POLICY = "User_Policy";
    }
}
