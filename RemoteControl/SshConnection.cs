using System;
using Renci.SshNet;
using System.Collections.Generic;

namespace RemoteControl
{
    /// <summary>
    /// SshConnection
    /// </summary>
    /// <example>
    /// // TODO: host, username, password/public key, ports
    /// var remote = new SshConnection(host, username);
    /// 
    /// remote.Connect();
    /// remote.Push(localDir, remoteDir);
    /// remote.Execute(remoteScript, Handler);
    /// remote.Pull(remoteLogDir, logDir);
    /// 
    /// // CHAINED EXAMPLE
    /// new SshConnection(host, 2222, username)
    ///     .ForwardRemote(2873, "localhost", 2873)
    ///     .ConnectWithKeyFile("~/.ssh/id_rsa.pub")
    ///     .Push(localDir, remoteDir)
    ///     .Execute(remoteScript, Handler)
    ///     .Pull(remoteLogDir, logDir);
    /// 
    /// // dispose example
    /// using (var remote = new SshConnection(host, username))
    /// {
    ///     remote.ForwardRemote(2873, "localhost", 2873)
    ///         .ConnectWithKeyFile("~/.ssh/id_rsa.pub")
    ///         .Execute(remoteScript, Handler);
    /// }
    /// 
    /// 
    /// public void Handler(Severity severity, string line)
    /// {
    ///     // handle line
    /// }
    /// </example>
    public class SshConnection : IDisposable
    {
        private const int SSH_PORT = 22;

        private SshClient sshClient;
        private string host;
        private int port;
        private string username;
        private string keyFile;
        private string password;
        private bool isConnected;

        private IList<ForwardedPort> ports;

        public SshConnection(string host, string username)
            : this(host, SSH_PORT, username)
        {
        }

        public SshConnection(string host, int port, string username)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            ports = new List<ForwardedPort>();
            isConnected = false;
        }

        /// <summary>
        /// binds a port on the local machine and forwards to host/port on the remote side.
        /// </summary>
        /// <param name="boundPort">Bound port.</param>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        public SshConnection ForwardLocal(uint boundPort, string host, uint port)
        {
            ports.Add(new ForwardedPortLocal(boundPort, host, port));

            return this;
        }

        /// <summary>
        /// binds a port on the remote machine and forwards to host/port on the local side.
        /// </summary>
        /// <param name="boundPort">Bound port.</param>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        public SshConnection ForwardRemote(uint boundPort, string host, uint port)
        {
            ports.Add(new ForwardedPortRemote(boundPort, host, port));

            return this;
        }

        /// <summary>
        /// Connects to the remote using a key file.
        /// </summary>
        /// <returns>The with key file.</returns>
        /// <param name="keyFile">Key file.</param>
        public SshConnection ConnectWithKeyFile(string keyFile)
        {
            this.keyFile = keyFile;
            return Connect();
        }

        /// <summary>
        /// Connects to the remote using a password.
        /// </summary>
        /// <returns>The with password.</returns>
        /// <param name="password">Password.</param>
        public SshConnection ConnectWithPassword(string password)
        {
            this.password = password;
            return Connect();
        }

        private SshConnection Connect()
        {
            if (isConnected)
                throw new InvalidOperationException("already connected");
            
            sshClient = keyFile != null
                ? new SshClient(host, port, username, BuildPrivateKeyFiles())
                : new SshClient(host, port, username, password);

            ports.ForEach(sshClient.AddForwardedPort);

            isConnected = true;
            return this;
        }

        private PrivateKeyFile[] BuildPrivateKeyFiles()
        {
            return new []
            {
                new PrivateKeyFile(keyFile)
            };
        }


        /// <summary>
        /// push a file or an entire directory to the remote machine
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <param name="remotePath">Remote path.</param>
        public SshConnection Push(string localPath, string remotePath)
        {
            throw new NotImplementedException();

            // TODO: rsync to remote

            return this;
        }

        /// <summary>
        /// execute a script on the remote machine. For every line received
        /// on stdout or stderr a given callback is called.
        /// </summary>
        /// <param name="scriptPath">path to script on remote machine</param>
        /// <param name="callback">delegate called for every line received</param>
        public SshConnection Execute(string scriptPath, Action<Severity, string> callback)
        {
            throw new NotImplementedException();

            // TODO: execute

            return this;
        }

        /// <summary>
        /// pull a file or an entire directory from the remote machine
        /// </summary>
        /// <param name="remotePath">Remote path.</param>
        /// <param name="localPath">Local path.</param>
        public SshConnection Pull(string remotePath, string localPath)
        {
            throw new NotImplementedException();

            // TODO: rsync from remote side

            return this;
        }

        public void Dispose()
        {
            if (sshClient == null)
                return;

            if (sshClient.IsConnected)
                sshClient.Disconnect();

            sshClient.Dispose();
        }
    }
}
