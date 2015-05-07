using NUnit.Framework;
using RemoteControl;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RemoteControl.Tests
{
    [TestFixture]
    public class RsyncDaemonTest
    {
        private string dir;
        private int port;
        private RsyncDaemon rsync;

        [SetUp]
        public void SetUp()
        {
            port = 2222;

            // dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            dir = "/tmp/rsync";
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(Path.Combine(dir, "log"));

            rsync = new RsyncDaemon(dir, port, new [] { "log" });

            Thread.Sleep(500);
        }

        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(500);

            rsync.Dispose();
            rsync = null;

            if (dir != null && dir.Length > 5)
                Directory.Delete(dir, true);
            dir = null;
        }

        [Test]
        public void RemoteControlRsync_Write_CreatesFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "huhu, there is a file");

            Process
                .Start("rsync", $"{tempFile} rsync://127.0.0.1:{port}/log/file.txt")
                .WaitForExit(1000);

            // Assert
            var destFile = Path.Combine(dir, "log", "file.txt");
            Assert.IsTrue(File.Exists(destFile));
            Assert.AreEqual("huhu, there is a file", File.ReadAllText(destFile));

            // Cleanup
            File.Delete(tempFile);
        }
    }
}

