using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeRequestValidationResultTest
	{
		[Test]
		public void ShouldNotBeDeniedByDefault()
		{
			const bool isOk = false;
			const string denyReason = "Deny reason for test";

			var validationResult = new ShiftTradeRequestValidationResult(isOk, denyReason);
			validationResult.IsOk.Should().Be(isOk);
			validationResult.ShouldBeDenied.Should().Be(false);
			validationResult.DenyReason.Should().Be(denyReason);
		}
	}
}