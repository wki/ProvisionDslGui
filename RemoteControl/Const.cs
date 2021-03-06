﻿using Common.Logging;
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
        private static ILog log = LogManager.GetLogger(typeof(Const));

        private static readonly string ENV_PREFIX = "PROVISION_";

        public static int RSYNC_PORT { get; private set; } = 2873;
        public static string RSYNC { get; private set; } = "rsync";

        public static int SSH_PORT { get; private set; } = 22;


        // hint: static constructor runs befor first use
        static Const()
        {
            log.Info("Setup");

            var constTypeInfo = typeof(Const).GetTypeInfo();

            foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
            {
                var variable = e.Key.ToString();
                var value = e.Value;

                if (variable.StartsWith(ENV_PREFIX))
                {
                    var constantName = variable.Remove(0, ENV_PREFIX.Length);
                    var property = constTypeInfo.GetDeclaredProperty(constantName);

                    if (property != null)
                    {
                        log.Debug(m => m("Setting value for Const.{0} to '{1}'", constantName, value));
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
