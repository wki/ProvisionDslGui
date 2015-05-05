using System;

namespace RemoteControl
{
    /// <summary>
    /// the severity level of a line currently received
    /// </summary>
    public enum Severity
    {
        Emergency = 0,
        Alert = 1,
        Critical = 2,
        Error = 3,
        Warning = 4,
        Notice = 5,
        Info = 6,
        Debug = 7,

        Output = 10 // not a real severity level but a regular output
    }
}
