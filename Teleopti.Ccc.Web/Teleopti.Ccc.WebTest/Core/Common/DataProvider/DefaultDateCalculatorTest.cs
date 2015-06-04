using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
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
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(defaultDateStart);
		}

		[Test]
		public void ShouldCalculateDefaultDateToWcsPreferencePeriodStartDateWhereActivePersonPeriodExists()
		{
			var defaultDateStart = DateOnly.Today;
			var person = new Person();
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(defaultDateStart, DateOnly.Today.AddDays (5))
			};
			//have an active person period that starts before the start of the preference period
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-20));
			personPeriod.SetParent(person);
			var personPeriods = new List<IPersonPeriod>
			{
				personPeriod
			};

			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(workflowControlSet.PreferencePeriod.StartDate);
		}


		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenNoWorkflowControlSet()
		{
			var personPeriods = new List<IPersonPeriod> { PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today) };
			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(null, w => w.StudentAvailabilityPeriod, personPeriods);

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test,Ignore]
		public void ShouldCalculateDefaultDateToTodayWhenPeriodStartsBeforeToday()
		{
			var workflowControlSet = new WorkflowControlSet(null)
										{
											StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-7), DateOnly.Today.AddDays(30))
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today.AddDays (-20));
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };
			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(workflowControlSet, w => w.StudentAvailabilityPeriod, personPeriods);

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayIfNoPersonPeriod()
		{
			var workflowControlSet = new WorkflowControlSet(null)
										{
											PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(7), DateOnly.Today.AddDays(30))
										};

			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, new List<IPersonPeriod>());

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test,Ignore]
		public void ShouldCalculateDefaultDateToStartOfPersonPeriod()
		{
			var workflowControlSet = new WorkflowControlSet(null)
										{
											PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(7), DateOnly.Today.AddDays(30))
										};
			var pp = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(14));
			pp.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { pp };
			var target = new DefaultDateCalculator(new Now());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(DateOnly.Today.AddDays(14));
		}
	}
}