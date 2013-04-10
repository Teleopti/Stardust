﻿using NUnit.Framework;
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
            var calculator = _mocks.StrictMock<IBudgetGroupAllowanceCalculator>();
            var absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            const string validationErrors = "Not Enough Allowance left";

            using (_mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(absenceRequest)).IgnoreArguments().Return(false);
                Expect.Call(calculator.CheckHeadCountInBudgetGroup(absenceRequest)).IgnoreArguments().Return(validationErrors);
            }
            using (_mocks.Playback())
            {
                _target.BudgetGroupHeadCountSpecification = specification;
                _target.BudgetGroupAllowanceCalculator = calculator;
                var result = _target.Validate(absenceRequest);
                Assert.IsFalse(result.IsValid);
            }
        }
    }
}
