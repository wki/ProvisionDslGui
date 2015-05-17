using System;
using System.IO;
using RemoteControl;

namespace RsyncTester
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting rsyncd #1...");
            Const.PrintConstants();

            var envVariables = Environment.GetEnvironmentVariables();
            if (!envVariables.Contains("HOME"))
                throw new InvalidOperationException("ENV{HOME} is not defined");
            
            var rsyncRootDir = Path.Combine(envVariables["HOME"].ToString(), "tmp");

            using (new RsyncDaemon(rsyncRootDir, new [] { "log" }))
            {
                Console.WriteLine("rsyncd #1 running.");
                Console.WriteLine("Example: rsync -v ((a file)) rsync://127.0.0.1:{0}/log/file.txt", Const.RSYNC_PORT);
                Console.WriteLine("Current Thread ID:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine("Press Enter to cancel");
                Console.ReadLine();
            }
        }
    }
}
