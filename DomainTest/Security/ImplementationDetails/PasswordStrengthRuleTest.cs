using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails
{
    [TestFixture]
    public class PasswordStrengthRuleTest
    {
        private MockRepository _mocks;
        private List<ISpecification<string>> _rules;
        private string _password;

        [SetUp]
        public void Setup()
        {
            _password = "password";
            _mocks = new MockRepository();
            ISpecification<string> rule1 = _mocks.StrictMock<ISpecification<string>>();
            ISpecification<string> rule2 = _mocks.StrictMock<ISpecification<string>>();
            ISpecification<string> rule3 = _mocks.StrictMock<ISpecification<string>>();
            ISpecification<string> rule4 = _mocks.StrictMock<ISpecification<string>>();
            _rules = new List<ISpecification<string>>() {rule1, rule2, rule3, rule4};
        }

        [Test]
        public void VerifyChecksAllRulesIfOk()
        {

            IPasswordStrengthRule target = new PasswordStrengthRule(3, _rules);

            using(_mocks.Record())
            {
                foreach (var rule in _rules)
                {
                    Expect.Call(rule.IsSatisfiedBy(_password)).Return(true);
                }
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(target.VerifyPasswordStrength(_password));
            }
        }

        [Test]
        public void VerifyReturnsFalseIfAnyOfTheRulesIsFalse()
        {
            ISpecification<string> falseRule = _mocks.StrictMock<ISpecification<string>>();
            using (_mocks.Record())
            {
                foreach (ISpecification<string> rule in _rules)
                {
                    Expect.Call(rule.IsSatisfiedBy(_password)).Repeat.Any().Return(true);
                }
                Expect.Call(falseRule.IsSatisfiedBy(_password)).Return(false);
            }
            using(_mocks.Playback())
            {
                _rules.Add(falseRule);
                var target = new PasswordStrengthRule(_rules.Count, _rules);
                Assert.IsFalse(target.VerifyPasswordStrength(_password));

            }
        }

        [Test]
        public void VerifyReturnsTrueIfSatisfiedByMinimumAmountOfRules()
        {
            ISpecification<string> falseRule = _mocks.StrictMock<ISpecification<string>>();
            ISpecification<string> falseRule2 = _mocks.StrictMock<ISpecification<string>>();
            using (_mocks.Record())
            {
                foreach (ISpecification<string> rule in _rules)
                {
                    Expect.Call(rule.IsSatisfiedBy(_password)).Repeat.Any().Return(true);
                }
                Expect.Call(falseRule.IsSatisfiedBy(_password)).Return(false).Repeat.Any();
                Expect.Call(falseRule2.IsSatisfiedBy(_password)).Return(false).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                _rules.Add(falseRule);
                _rules.Add(falseRule2);

                var target = new PasswordStrengthRule(_rules.Count, _rules);
                Assert.IsFalse(target.VerifyPasswordStrength(_password),"Should be false, 2 false and x out of x must be true");

                target = new PasswordStrengthRule(_rules.Count-1, _rules);
                Assert.IsFalse(target.VerifyPasswordStrength(_password), "Should be false, 2 false and x-1 out of x must be true");

                target = new PasswordStrengthRule(_rules.Count - 2, _rules);
                Assert.IsTrue(target.VerifyPasswordStrength(_password), "Should be true, 2 false and x-2 out of x must be true");

                target = new PasswordStrengthRule(0, _rules);
                Assert.IsTrue(target.VerifyPasswordStrength(_password), "Should be true, 0 out of x must be true");
            }
        }
    }
}
