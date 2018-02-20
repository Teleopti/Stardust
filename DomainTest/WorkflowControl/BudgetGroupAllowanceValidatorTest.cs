using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	public class BudgetGroupAllowanceValidatorTest
	{
		private IAbsenceRequestValidator _target;

		[SetUp]
		public void Setup()
		{
			_target = new BudgetGroupAllowanceValidator();
		}

		[Test]
		public void ShouldHaveDisplayText()
		{
			Assert.AreEqual(UserTexts.Resources.BudgetGroup, _target.DisplayText);
		}

		[Test]
		public void ShouldBeValidIfEnoughAllowanceLeft()
		{
			var specification = MockRepository.GenerateStrictMock<IBudgetGroupAllowanceSpecification>();
			var absenceRequest = MockRepository.GenerateStrictMock<IAbsenceRequest>();
			specification.Stub(x => x.IsSatisfied(new AbsenceRequstAndSchedules())).IgnoreArguments().Return(new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty });
			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(null, null, null, specification, null));
			Assert.IsTrue(result.IsValid);
		}

		[Test]
		public void ShouldBeInvalidIfNotEnoughAllowanceLeft()
		{
			var specification = MockRepository.GenerateStrictMock<IBudgetGroupAllowanceSpecification>();
			var absenceRequest = MockRepository.GenerateStrictMock<IAbsenceRequest>();
			specification.Stub(x => x.IsSatisfied(new AbsenceRequstAndSchedules())).IgnoreArguments().Return(new ValidatedRequest { IsValid = false, ValidationErrors = string.Empty });
			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(null, null, null, specification, null));
			Assert.IsFalse(result.IsValid);
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
