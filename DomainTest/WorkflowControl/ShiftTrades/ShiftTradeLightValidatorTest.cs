using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeLightValidatorTest
	{
		[Test]
		public void ShouldReturnTrueIfAllIsSatisfied()
		{
			var checkItem = new ShiftTradeAvailableCheckItem();
			var spec1 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			var spec2 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			spec1.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			spec2.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			var validator = new ShiftTradeLightValidator(new []{spec1, spec2});
			validator.Validate(checkItem).Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfAnyIsNonSatisfied()
		{
			var checkItem = new ShiftTradeAvailableCheckItem();
			var spec1 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			var spec2 = MockRepository.GenerateMock<IShiftTradeLightSpecification>();
			spec1.Expect(m => m.IsSatisfiedBy(checkItem)).Return(true);
			spec2.Expect(m => m.IsSatisfiedBy(checkItem)).Return(false);
			var validator = new ShiftTradeLightValidator(new[] { spec1, spec2 });
			validator.Validate(checkItem).Should().Be.False();
		}
	}
}