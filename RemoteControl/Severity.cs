using System;

namespace RemoteControl
{
    /// <summary>
    /// the severity level of a line currently received
    /// </summary>
    public enum Severity
    {
        Debug,
        Info,
        Warn,
        Error,
        Output // not a real severity level but a regular output
    }
}

