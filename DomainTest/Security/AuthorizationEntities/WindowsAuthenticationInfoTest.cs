using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Tests for WindowsAuthenticationInfoTest
    /// </summary>
    [TestFixture]
    public class WindowsAuthenticationInfoTest
    {
        private WindowsAuthenticationInfo target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new WindowsAuthenticationInfo();
        }

        /// <summary>
        /// Verifies that default properties works.
        /// </summary>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(string.Empty, target.WindowsLogOnName);
            Assert.AreEqual(string.Empty, target.DomainName);
        }

        [Test]
        public void VerifyWindowsLogOnNameProperty()
        {
            string setName = "AcdLogOnName";
            target.WindowsLogOnName = setName;
            string getName = target.WindowsLogOnName;
            Assert.AreEqual(setName, getName);
        }

        [Test]
        public void VerifyDomainNameProperty()
        {
            string setName = "DomainName";
            target.DomainName = setName;
            string getName = target.DomainName;
            Assert.AreEqual(setName, getName);
        }
    }
}