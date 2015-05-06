using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace RemoteControl
{
    /// <summary>
    /// Some constants
    /// </summary>
    /// <description>
    /// The constants may be overriden by environment variables. Every environment
    /// variable is prefixed with "PROVISION_" and then has the same suffix
    /// as the name of the constant.
    /// </description>
    public static class Const
    {
        public static int RSYNC_PORT { get; private set; } = 2873;
        public static string RSYNC { get; private set; } = "rsync";

        // hint: static constructor runs befor first use
        static Const()
        {
            var constTypeInfo = typeof(Const).GetTypeInfo();

            foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
            {
                var variable = e.Key.ToString();
                var value = e.Value;

                if (variable.StartsWith("PROVISION_"))
                {
                    var constantName = variable.Remove(0, 10);
                    var property = constTypeInfo.GetDeclaredProperty(constantName);

                    if (property != null)
                    {
                        property.SetValue(null, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
        }

        /// <summary>
        /// diagnostic output of all constants after setting them via environment or defaults
        /// </summary>
        public static void PrintConstants()
        {
            typeof(Const).GetTypeInfo()
                .DeclaredProperties
                .OrderBy(p => p.Name)
                .ToList()
                .ForEach(p => Console.WriteLine("{0} = {1}", p.Name, p.GetValue(null,null)));
        }
    }
}
