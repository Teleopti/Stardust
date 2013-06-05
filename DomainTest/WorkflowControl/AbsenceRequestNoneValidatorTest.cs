using NUnit.Framework;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class AbsenceRequestNoneValidatorTest
    {
        private IAbsenceRequestValidator _target;
        
        [SetUp]
        public void Setup()
        {
            _target = new AbsenceRequestNoneValidator();
        }

        [Test]
        public void VerifyInvalidReasonIsEmptyString()
        {
            Assert.AreEqual(string.Empty, _target.InvalidReason);
        }

        [Test]
        public void VerifyValidateAlwaysReturnsTrue()
        {
            var result = _target.Validate(null, new RequiredForHandlingAbsenceRequest());
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(UserTexts.Resources.No, _target.DisplayText);
        }

        [Test]
        public void VerifyCanCreateNewInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.AreNotSame(_target,newInstance);
            Assert.IsTrue(typeof(AbsenceRequestNoneValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void VerifyEquals()
        {
            var otherValidatorOfSameKind = new AbsenceRequestNoneValidator();
            var otherValidator = new PersonAccountBalanceValidator();

            Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
            Assert.IsFalse(_target.Equals(otherValidator));
        }

        [Test]
        public void ShouldGetHashCodeInReturn()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);
        }
    }
}
