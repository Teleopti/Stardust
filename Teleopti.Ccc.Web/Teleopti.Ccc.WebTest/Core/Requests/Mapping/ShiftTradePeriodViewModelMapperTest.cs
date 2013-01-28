using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradePeriodViewModelMapperTest
	{
		[Test]
		public void ShouldMapHasWorkflowControlSetToFalse()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(null);

			result.HasWorkflowControlSet.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToTrue()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet());

			result.HasWorkflowControlSet.Should().Be.True();
		}

		[Test]
		public void ShouldMapOpenPeriod()
		{
			var mapper = new ShiftTradePeriodViewModelMapper();
			var result = mapper.Map(new WorkflowControlSet { ShiftTradeOpenPeriodDaysForward = new MinMax<int>(2, 8) });

			result.OpenPeriodRelativeStart.Should().Be.EqualTo(2);
			result.OpenPeriodRelativeEnd.Should().Be.EqualTo(8);
		}
	}
}
