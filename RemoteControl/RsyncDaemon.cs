using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwoPS.Processes;

// TODO: kill rsync when main process terminates
// TODO: get info when rsync dies

namespace RemoteControl
{
    /// <summary>
    /// a locally running rsync daemon
    /// </summary>
    /// <description>
    /// on the local side, a rsync daemon is started with at least the first but
    /// usually these three modules
    ///   * local for read-only access of a whole directory
    ///   * log for read/write access of the log subdirectory
    ///   * result for read/write access of the result subdirectory
    /// 
    /// on the remote side, the rsync daemon may be used like this:
    /// 
    /// $ rsync -n -avxSH rsync://127.0.0.1:2873/local/whatever .
    /// 
    /// </description>
    /// <example>
    /// using (new RsyncDaemon("/path/to/dir", new ["log"]))
    /// {
    ///     // do something that uses the rsync service
    /// }
    /// </example>
    public class RsyncDaemon : IDisposable
    {
        // can we read from ENV or Config?
        private const int DEFAULT_RSYNC_PORT = 2873;
        private const string RSYNC = "rsync"; 

        private int localPort;
        private string rootDirectory;
        private List<string> rsyncModules;
        private string rsyncdConfigFile;

        private Process rsyncProcess;
        private Task rsyncTask;

        public RsyncDaemon(string rootDirectory)
            : this(rootDirectory, DEFAULT_RSYNC_PORT)
        {
        }

        public RsyncDaemon(string rootDirectory, int localPort)
            : this(rootDirectory, localPort, new string[] {})
        {
        }

        public RsyncDaemon(string rootDirectory, IEnumerable<string> rsyncModules)
            : this(rootDirectory, DEFAULT_RSYNC_PORT, rsyncModules)
        {
        }

        public RsyncDaemon(string rootDirectory, int localPort, IEnumerable<string>rsyncModules)
        {
            this.rootDirectory = rootDirectory;
            this.localPort = localPort;
            this.rsyncModules = new List<string>(rsyncModules);

            BuildRsyncdConfigFile();
            StartRsyncd();
        }

        private void BuildRsyncdConfigFile()
        {
            rsyncdConfigFile = Path.GetTempFileName().Replace(".tmp", ".conf");
            File.WriteAllText(rsyncdConfigFile, BuildRsyncdConfig());
        }

        private string BuildRsyncdConfig()
        {
            var config = new StringBuilder();

            config.AppendLine("use chroot = no");

            config.AppendLine("[local]");
            config.AppendLine($"    path = {rootDirectory}");
            config.AppendLine("    read only = true");

            rsyncModules.ForEach(m =>
                {
                    config.AppendLine($"[{m}]");
                    config.AppendLine($"    path = {rootDirectory}/{m}");
                    config.AppendLine("    read only = false");
                });

            return config.ToString();
        }

        private void StartRsyncd()
        {
            var options = new ProcessOptions(
                RSYNC,
                new string[] 
                {
                    "--daemon",
                    "--address", "127.0.0.1",
                    "--no-detach",
                    "--port", localPort.ToString(),
                    "--config", rsyncdConfigFile
                }
            );

            rsyncProcess = new Process(options);
            rsyncTask = Task.Run(() => rsyncProcess.Run());
        }

        public void Dispose()
        {
            Console.WriteLine("dispose RsyncDaemon");
            StopRsyncd();
            DeleteRsyncdConfigFile();
        }

        private void StopRsyncd()
        {
            if (rsyncProcess != null)
            {
                Console.WriteLine("cancel process");
                rsyncProcess.Cancel();

                // disposing rsyncTask gives an error, so we omit it

                rsyncTask = null;
                rsyncProcess = null;
            }
        }

        private void DeleteRsyncdConfigFile()
        {
            if (rsyncdConfigFile != null && File.Exists(rsyncdConfigFile))
            {
                File.Delete(rsyncdConfigFile);
            }
            rsyncdConfigFile = null;
        }
    }
}
