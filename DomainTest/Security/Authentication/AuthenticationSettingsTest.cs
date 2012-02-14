using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class AuthenticationSettingsTest
    {
        private AuthenticationSettings _target;

        [SetUp]
        public void Setup()
        {
            _target = new AuthenticationSettings();
        }

        [Test]
        public void VerifyLogOnMode()
        {
            LogOnModeOption setValue = LogOnModeOption.Win;
            _target.LogOnMode = setValue;
            Assert.AreEqual(setValue, _target.LogOnMode);
        }
    }
}
