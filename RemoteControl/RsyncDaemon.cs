using System;

namespace RemoteControl
{
    /// <summary>
    /// a locally running rsync daemon
    /// </summary>
    public class RsyncDaemon : IDisposable
    {
        public int Port { get; private set; }

        // TODO: specify modules and directories to access somehow
        public RsyncDaemon(int localPort)
        {
            Port = localPort;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
