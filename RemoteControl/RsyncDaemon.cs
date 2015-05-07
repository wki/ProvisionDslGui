﻿using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwoPS.Processes;

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
    /// the modules must be provided as an array of strings
    /// on the remote side, the rsync daemon may be used like this:
    /// 
    /// $ rsync -n -avxSH rsync://127.0.0.1:2873/local/whatever .
    /// 
    /// </description>
    /// <example>
    /// using (new RsyncDaemon("/path/to/dir", new [] { "log" }))
    /// {
    ///     // do something that uses the rsync service
    /// }
    /// </example>
    public class RsyncDaemon : IDisposable
    {
        private static ILog log = LogManager.GetLogger(typeof(RsyncDaemon));

        private int localPort;
        private string rootDirectory;
        private List<string> rsyncModules;
        private string rsyncdConfigFile;

        private Process rsyncProcess;
        private Task<ProcessResult> rsyncTask;
        private Timer timer;

        private bool isDisposing;

        public RsyncDaemon(string rootDirectory)
            : this(rootDirectory, Const.RSYNC_PORT)
        {
        }

        public RsyncDaemon(string rootDirectory, int localPort)
            : this(rootDirectory, localPort, new string[] {})
        {
        }

        public RsyncDaemon(string rootDirectory, IEnumerable<string> rsyncModules)
            : this(rootDirectory, Const.RSYNC_PORT, rsyncModules)
        {
        }

        public RsyncDaemon(string rootDirectory, int localPort, IEnumerable<string>rsyncModules)
        {
            log.Info(m => 
                m("Start rsyncd for dir '{0}' on port {1} with modules {2}",
                    rootDirectory, localPort, String.Join(", ", rsyncModules.Select(x => $"'{x}'"))
                )
            );
            this.rootDirectory = rootDirectory;
            this.localPort = localPort;
            this.rsyncModules = new List<string>(rsyncModules);

            isDisposing = false;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleCancelHandler);

            BuildRsyncdConfigFile();
            StartRsyncd();
        }

        private void BuildRsyncdConfigFile()
        {
            rsyncdConfigFile = Path.GetTempFileName().Replace(".tmp", ".conf");
            log.Debug(m => m("create config file '{0}'", rsyncdConfigFile));
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

        private void ConsoleCancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            log.Debug("Program cancelled");
            Console.WriteLine("Program cancelled");
        }

        private void StartRsyncd()
        {
            var options = new ProcessOptions(
                Const.RSYNC,
                new [] 
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

            timer = new Timer(
                ObserveProcessStatus,
                new AutoResetEvent(false),
                0,
                500
            );
        }

        public void ObserveProcessStatus(object o)
        {
            // we could have died in the meantime...
            if (rsyncTask == null)
                return;
            
            Console.WriteLine("Task Status: {0}", rsyncTask.Status);

            if (isDisposing)
                return;
            
            // RanToCompletion usually occurs when rsync fails to start.
            if (rsyncTask.Status == TaskStatus.RanToCompletion)
            {
                Console.WriteLine("rsync stopped, exit status: {0}", rsyncProcess.Result.ExitCode);
                timer.Change(int.MaxValue, int.MaxValue);
                throw new InvalidOperationException("rsync terminated unexpectedly");
            }
        }

        public void Dispose()
        {
            if (isDisposing)
                return;
            
            isDisposing = true;

            // Console.WriteLine("dispose RsyncDaemon");
            StopRsyncd();
            DeleteRsyncdConfigFile();
        }

        private void StopRsyncd()
        {
            if (rsyncProcess != null)
            {
                log.Debug("stop rsyncd process");
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
                log.Debug(m => m("remove config file '{0}'", rsyncdConfigFile));
                File.Delete(rsyncdConfigFile);
            }
            rsyncdConfigFile = null;
        }
    }
}
