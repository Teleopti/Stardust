using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class DefaultDateCalculatorTest
	{
		[Test]
		public void ShouldCalculateDefaultDateToFirstDateInFuturePeriod()
		{
			var defaultDateStart = DateOnly.Today.AddDays(30);
			var workflowControlSet = new WorkflowControlSet
			                         	{
			                         		PreferencePeriod = new DateOnlyPeriod(defaultDateStart, DateOnly.Today.AddDays(60))
			                         	};

			var target = new DefaultDateCalculator();

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod);

			result.Should().Be.EqualTo(defaultDateStart);
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenNoWorkflowControlSet()
		{
			var target = new DefaultDateCalculator();

			var result = target.Calculate(null, w => w.StudentAvailabilityPeriod);

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenPeriodStartsBeforeToday()
		{
			var workflowControlSet = new WorkflowControlSet(null)
			                         	{
			                         		StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-7), DateOnly.Today.AddDays(30))
			                         	};

			var target = new DefaultDateCalculator();

			var result = target.Calculate(workflowControlSet, w => w.StudentAvailabilityPeriod);

			result.Should().Be.EqualTo(DateOnly.Today);
		}
	}
}