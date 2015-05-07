using NUnit.Framework;
using System;
using RemoteControl;

namespace RemoteControl.Tests
{
    [TestFixture]
    public class ConstTest
    {
        [TestFixtureSetUp]
        public void Init()
        {
            Environment.SetEnvironmentVariable("PROVISION_SSH_PORT", "42");
            Environment.SetEnvironmentVariable("PROVISION_RSYNC", "");
        }

        [Test]
        public void RemoteControlConst_WithoutEnv_RsyncIsDefault()
        {
            // Assert
            Assert.AreEqual(Const.RSYNC, "rsync");
        }

        [Test]
        public void RemoteControlConst_WithEnv_SshPortIsChanged()
        {
            // Assert
            Assert.AreEqual(Const.SSH_PORT, 42);
        }
    }
}
