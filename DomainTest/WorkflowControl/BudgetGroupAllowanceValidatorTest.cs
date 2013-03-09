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
        //private IValidatedRequest _validatedRequest;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            //_validatedRequest = new ValidatedRequest(){IsValid = true, ValidationErrors = ""};
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
                var result = _target.Validate(absenceRequest);
                Assert.IsTrue(result.IsValid);
            }
        }

        [Test]
        public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
        {
            var specification = _mocks.StrictMock<IBudgetGroupAllowanceSpecification>();
            var calculator = _mocks.StrictMock<IBudgetGroupAllowanceCalculator>();
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            var validationErrors = "Not Enough Allowance left";

            using (_mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(absenceRequest)).IgnoreArguments().Return(false);
                Expect.Call(calculator.CheckBudgetGroup(absenceRequest)).IgnoreArguments().Return(validationErrors);
            }
            using (_mocks.Playback())
            {
                _target.BudgetGroupAllowanceSpecification = specification;
                _target.BudgetGroupAllowanceCalculator = calculator;
                var result = _target.Validate(absenceRequest);
                Assert.IsFalse(result.IsValid);
                //Assert.IsFalse(_target.Validate(absenceRequest));
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

        [Test]
        public void GetHashCodeCorrectly()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);
        }
    }
}
