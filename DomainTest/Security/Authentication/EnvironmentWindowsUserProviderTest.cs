using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class EnvironmentWindowsUserProviderTest
    {
      [Test]
        public void VerifyIdentity()
        {
					Assert.AreEqual(Environment.UserDomainName + "\\" + Environment.UserName, new EnvironmentWindowsUserProvider().Identity());
        }
    }
}