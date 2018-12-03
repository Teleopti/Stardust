using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails
{
    [TestFixture]
    public class PasswordPolicyTest
    {
        private MockRepository _mocks;
        private ILoadPasswordPolicyService _loadPasswordPolicyService;
        private IPasswordPolicy _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _loadPasswordPolicyService = _mocks.StrictMock<ILoadPasswordPolicyService>();
            _target = new PasswordPolicy(_loadPasswordPolicyService);
        }

        [Test]
        public void GetPasswordValidForDayCount()
        {
            Assert.IsNotNull(_target.PasswordValidForDayCount);
        }

        [Test]
        public void GetPasswordExpireWarningDayCount()
        {
            Assert.IsNotNull(_target.PasswordExpireWarningDayCount);
        }

        [Test]
        public void VerifyGetsInvalidAttemptWindowFromLoadPasswordPolicyService()
        {
            TimeSpan expected = TimeSpan.FromMinutes(12);
        
            
            using(_mocks.Record())
            {
                Expect.Call(_loadPasswordPolicyService.LoadInvalidAttemptWindow()).Return(expected);
            }

            using(_mocks.Playback())
            {
                Assert.AreEqual(expected,_target.InvalidAttemptWindow);
            }
        }

        [Test]
        public void VerifyGetsMaxAttemptCountFromLoadPasswordPolicyService()
        {
            int expected = 3;
            using (_mocks.Record())
            {
                Expect.Call(_loadPasswordPolicyService.LoadMaxAttemptCount()).Return(expected);
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(expected, _target.MaxAttemptCount);
            }
        }


        [Test]
        public void VerifyCheckPasswordStrength()
        {
            string password = "password";
            IPasswordStrengthRule rule1 = _mocks.StrictMock<IPasswordStrengthRule>();
            IPasswordStrengthRule rule2 = _mocks.StrictMock<IPasswordStrengthRule>();
            IList<IPasswordStrengthRule> rules = new List<IPasswordStrengthRule> {rule1,rule2};


            using (_mocks.Record())
            {
                Expect.Call(_loadPasswordPolicyService.LoadPasswordStrengthRules()).Return(rules);
                Expect.Call(rule1.VerifyPasswordStrength(password)).Return(true);
                Expect.Call(rule2.VerifyPasswordStrength(password)).Return(true);
            }

            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.CheckPasswordStrength(password));
            }
        }

        [Test]
        public void VerifyAllRulesMustReturnTrueWhenCheckingPasswordStrength()
        {
            string password = "password";
            IPasswordStrengthRule rule1 = _mocks.StrictMock<IPasswordStrengthRule>();
            IPasswordStrengthRule rule2 = _mocks.StrictMock<IPasswordStrengthRule>();
            IList<IPasswordStrengthRule> rules = new List<IPasswordStrengthRule> { rule1, rule2 };


            using (_mocks.Record())
            {
                Expect.Call(_loadPasswordPolicyService.LoadPasswordStrengthRules()).Return(rules);
                Expect.Call(rule1.VerifyPasswordStrength(password)).Return(true);
                Expect.Call(rule2.VerifyPasswordStrength(password)).Return(false);
            }

            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.CheckPasswordStrength(password));
            }
        }
    }

    
}
