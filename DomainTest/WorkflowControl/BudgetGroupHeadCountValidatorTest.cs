using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount"), TestFixture]
    public class BudgetGroupHeadCountValidatorTest
    {
        private IAbsenceRequestValidator _target;
        private MockRepository _mocks;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new BudgetGroupHeadCountValidator();    
        }

        [Test]
        public void ShouldExplainInvalidReason()
        {
            Assert.AreEqual("RequestDenyReasonBudgetGroupAllowance", _target.InvalidReason);
        }

        [Test]
        public void ShouldHaveDisplayText()
        {
            Assert.AreEqual("Budget Group Head Count", _target.DisplayText);
        }

        [Test]
        public void ShouldCreateNewInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.AreNotSame(_target, newInstance);
            Assert.IsTrue(typeof(BudgetGroupHeadCountValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void ShouldAllInstancesBeEqual()
        {
            var otherValidatorOfSameKind = new BudgetGroupHeadCountValidator();
            Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
        }

        [Test]
        public void ShouldNotEqualIfTheyAreInstancesOfDifferentType()
        {
            var otherValidator = new AbsenceRequestNoneValidator();
            Assert.IsFalse(_target.Equals(otherValidator));
        }

        [Test]
        public void GetHashCodeCorrectly()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);
        }

        [Test]
        public void ShouldReturnFalseIfNotEnoughAllowanceLeft()
        {
            var specification = _mocks.StrictMock<IBudgetGroupHeadCountSpecification>();
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            
            using (_mocks.Record())
            {
                Expect.Call(specification.IsSatisfied(absenceRequest).IsValid).IgnoreArguments().Return(false);
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(null, null, null, null, specification));
                Assert.IsFalse(result.IsValid);
            }
        }
    }
}
