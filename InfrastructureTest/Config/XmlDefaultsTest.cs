using System;
using System.Xml.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Config;

namespace Teleopti.Ccc.InfrastructureTest.Config
{
    [TestFixture,Category("LongRunning")]
    public class XmlDefaultsTest
    {
        private LoadPasswordPolicyService _service;

        [SetUp]
        public void Setup()
        {
            _service = new LoadPasswordPolicyService(ReadXmlTestFile());
        }

        [Test]
        public void VerifyLoadFromFile()
        {
            PasswordPolicy policy = new PasswordPolicy(_service);

            Assert.AreEqual(TimeSpan.Zero, policy.InvalidAttemptWindow, "InvalidAttemptWindow");
            Assert.AreEqual(1, policy.MaxAttemptCount, "MaxAttemptCount");
            Assert.AreEqual(0, policy.PasswordExpireWarningDayCount, "PasswordExpireWarningDayCount");
            Assert.AreEqual(int.MaxValue, policy.PasswordValidForDayCount, "PasswordValidForDayCount");

        }


        [Test]
        public void VerifyDefaultStrengthRules()
        {
            PasswordPolicy policy = new PasswordPolicy(_service);

            //Must be 8 characters:
            Assert.IsFalse(policy.CheckPasswordStrength("abc#6Z"),"Ok but to short");
            Assert.IsFalse(policy.CheckPasswordStrength("324567324#63"),"Ok but needs a-z or A-Z");
            Assert.IsTrue(policy.CheckPasswordStrength("324567324#63a"), "Ok  (has a-z)");
            Assert.IsTrue(policy.CheckPasswordStrength("324567324#63A"), "Ok  (hasA-Z)");
            Assert.IsFalse(policy.CheckPasswordStrength("ghjdasdgjasgdhjasgdhasgdhjasgdHGHJGHJGAJSD"),"Ok but needs 0-9 or special");

        }

        private static XDocument ReadXmlTestFile()
        {
            XDocument doc = XDocument.Load(@"..\..\Config\Defaults.xml");
            return doc;
        }
    }
}
