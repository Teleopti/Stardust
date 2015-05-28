using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class DummyPasswordPolicyTest
    {
        private PasswordPolicyFake _target;

        [SetUp]
        public void Setup()
        {
            _target = new PasswordPolicyFake();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsTrue(_target.CheckPasswordStrength(string.Empty));
            Assert.AreEqual(3, _target.MaxAttemptCount);
            Assert.AreEqual(TimeSpan.FromMinutes(30), _target.InvalidAttemptWindow);
            Assert.AreEqual(60, _target.PasswordValidForDayCount);
            Assert.AreEqual(5, _target.PasswordExpireWarningDayCount);
        }
    }
}
