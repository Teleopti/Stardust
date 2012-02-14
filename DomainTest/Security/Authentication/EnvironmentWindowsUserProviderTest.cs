using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class EnvironmentWindowsUserProviderTest
    {
        private IWindowsUserProvider target;

        [SetUp]
        public void Setup()
        {
            target = new EnvironmentWindowsUserProvider();
        }

        [Test]
        public void VerifyDomainName()
        {
            Assert.AreEqual(Environment.UserDomainName,target.DomainName);
        }

        [Test]
        public void VerifyUserName()
        {
            Assert.AreEqual(Environment.UserName, target.UserName);
        }
    }
}