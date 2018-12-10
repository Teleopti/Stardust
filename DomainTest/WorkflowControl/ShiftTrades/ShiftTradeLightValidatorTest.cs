using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeLightValidatorTest
	{
		[Test]
		public void ShouldReturnTrueIfAllIsSatisfied()
		{
			var checkItem = new ShiftTradeAvailableCheckItem(new DateOnly(2000, 1, 1), new Person(), new Person());
			var spec1 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			var spec2 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			spec1.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			spec2.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			var validator = new ShiftTradeLightValidator(new []{spec1, spec2});
			validator.Validate(checkItem).IsOk.Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfAnyIsNonSatisfied()
		{
			var checkItem = new ShiftTradeAvailableCheckItem(new DateOnly(2000,1,1), new Person(), new Person());
			var spec1 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			var spec2 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			spec1.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			spec2.Expect(m => m.IsSatisfiedBy(checkItem)).Return(false);
			var validator = new ShiftTradeLightValidator(new[] { spec1, spec2 });
			validator.Validate(checkItem).IsOk.Should().Be.False();
		}

		[Test]
		public void ShouldSetDenyReason()
		{
			const string denyReason = "Deny reason";
			var checkItem = new ShiftTradeAvailableCheckItem(new DateOnly(2000, 1, 1), new Person(), new Person());
			var spec = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			spec.Expect(m => m.IsSatisfiedBy(checkItem)).Return(false);
			spec.Expect(m => m.DenyReason).Return(denyReason);

			var validator = new ShiftTradeLightValidator(new[] {spec});
			validator.Validate(checkItem).DenyReason.Should().Be.EqualTo(denyReason);
		}
	}
}