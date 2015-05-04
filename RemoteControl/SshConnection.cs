using System;

namespace RemoteControl
{
    /// <summary>
    /// SshConnection
    /// </summary>
    public class SshConnection : IDisposable
    {
        public SshConnection()
        {
        }

        /// <summary>
        /// push a file or an entire directory to the remote machine
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <param name="remotePath">Remote path.</param>
        public void Push(string localPath, string remotePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// execute a script on the remote machine. For every line received
        /// on stdout or stderr a given callback is called.
        /// </summary>
        /// <param name="scriptPath">path to script on remote machine</param>
        /// <param name="callback">delegate called for every line received</param>
        public void Execute(string scriptPath, Action<Severity, string> callback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// pull a file or an entire directory from the remote machine
        /// </summary>
        /// <param name="remotePath">Remote path.</param>
        /// <param name="localPath">Local path.</param>
        public void Pull(string remotePath, string localPath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
