using System;
using RemoteControl;

namespace RsyncTester
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting rsyncd #1...");

            using (new RsyncDaemon("/Users/wolfgang/tmp"))
            {
                Console.WriteLine("rsyncd #1 running. Press Enter to cancel");
                Console.ReadLine();
            }
        }
    }
}
