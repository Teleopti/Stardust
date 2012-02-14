using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class BudgetGroupAllowanceValidatorTest
    {
        private IAbsenceRequestValidator _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new BudgetGroupAllowanceValidator();
        }

        [Test]
        public void ShouldExplainInvalidReason()
        {
            Assert.AreEqual("RequestDenyReasonBudgetGroupAllowance", _target.InvalidReason);
        }

        [Test]
        public void ShouldHaveDisplayText()
        {
            Assert.AreEqual(UserTexts.Resources.BudgetGroup, _target.DisplayText);
        }

        [Test]
        public void ShouldBeValidIfEnoughAllowanceLeft()
        {
            var specification = _mocks.StrictMock<IBudgetGroupAllowanceSpecification>();
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            using (_mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(absenceRequest)).IgnoreArguments().Return(true);
            }            
            using(_mocks.Playback())
            {
                _target.BudgetGroupAllowanceSpecification = specification;
                Assert.IsTrue(_target.Validate(absenceRequest));
            }
        }

        [Test]
        public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
        {
            var specification = _mocks.StrictMock<IBudgetGroupAllowanceSpecification>();
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            using (_mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(absenceRequest)).IgnoreArguments().Return(false);
            }
            using (_mocks.Playback())
            {
                _target.BudgetGroupAllowanceSpecification = specification;
                Assert.IsFalse(_target.Validate(absenceRequest));
            }
        }

        [Test]
        public void ShouldCreateNewInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.AreNotSame(_target, newInstance);
            Assert.IsTrue(typeof(BudgetGroupAllowanceValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void ShouldAllInstancesBeEqual()
        {
            var otherValidatorOfSameKind = new BudgetGroupAllowanceValidator();
            Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
        }

        [Test]
        public void ShouldNotEqualIfTheyAreInstancesOfDifferentType()
        {
            var otherValidator = new AbsenceRequestNoneValidator();
            Assert.IsFalse(_target.Equals(otherValidator));
        }
    }
}
