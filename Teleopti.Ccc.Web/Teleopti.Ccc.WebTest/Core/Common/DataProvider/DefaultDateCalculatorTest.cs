using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class DefaultDateCalculatorTest
	{
		[Test]
		public void ShouldGetNowWhenThereIsNotIntersectionOfPersonPeriodAndPreferencePeriod()
		{
			var now = new DateOnly(2019,1,1);
			
			var preferencePeriodStart = new DateOnly(2019,1,1).AddDays(5);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(35);
			var personPeriodStart = new DateOnly(2019,1,1).AddDays(40);
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
			};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date));
		}

		[Test]
		public void ShouldGetNowWhenPersonPeriodAndPreferencePeriodIncludeNow()
		{
			var now = new DateOnly(2019,1,1).AddDays(15);
			var preferencePeriodStart = new DateOnly(2019,1,1);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(30);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (new DateOnly(2019,1,1));
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date));
		}
		
		[Test]
		public void ShouldGetRecentPeriodWhenNowOverPreferencePeriod()
		{
			var now = new DateOnly(2019,1,1).AddDays(50);
			var preferencePeriodStart = new DateOnly(2019,1,1);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(30);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (new DateOnly(2019,1,1));
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date));
		}
		
		[Test]
		public void ShouldGetPreferencePeriodStartWhenThereIsAPreferencePeriodInTheFuture()
		{
			var now = new DateOnly(2019,1,1);
			var preferencePeriodStart = new DateOnly(2019,1,1).AddDays(30);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(80);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (new DateOnly(2019,1,1));
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(preferencePeriodStart);
		}
		
		[Test]
		public void ShouldGetPersonPeriodStartWhenThereIsAPreferencePeriodInTheFutureButDontHavePersonPeriod()
		{
			var now = new DateOnly(2019,1,1);
			var personPeriodStart = new DateOnly(2019,1,1).AddDays(45);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(new DateOnly(2019,1,1).AddDays(30), new DateOnly(2019,1,1).AddDays(80))
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(personPeriodStart);
		}
		
		[Test]
		public void ShouldGetNowWhenNowPassOverPreferencePeriodAndThereIsFuturePersonPeriod()
		{
			var now = new DateOnly(2019,1,1).AddDays(45);
			var preferencePeriodStart = new DateOnly(2019,1,1);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(30);

			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};

			var person = PersonFactory.CreatePersonWithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod (new DateOnly(2019,1,1));
			personPeriod1.SetParent(person);
			var personPeriod2= PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2019,1,1).AddDays(60));
			personPeriod2.SetParent(person);

			var personPeriods = new List<IPersonPeriod> {personPeriod1, personPeriod2};

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date));

		}

		[Test]
		public void ShouldGetPreferencePeriodStartWhenPreferencePeriodInSecondPersonPeriod()
		{
			var now = new DateOnly(2019,1,1).AddDays(45);
			var preferencePeriodStart = new DateOnly(2019,1,1).AddDays(70);
			var preferencePeriodEnd = new DateOnly(2019,1,1).AddDays(100);
			var personPeriodStart1 = new DateOnly(2019,1,1);
			var personPeriodStart2 = new DateOnly(2019,1,1).AddDays(60);

			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};

			var person = PersonFactory.CreatePersonWithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod (personPeriodStart1);
			personPeriod1.SetParent(person);
			var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart2);
			personPeriod2.SetParent(person);

			var personPeriods = new List<IPersonPeriod> {personPeriod1, personPeriod2};

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(preferencePeriodStart);
		}

		[Test]
		public void ShouldCalculateDefaultDateToFirstDateInFuturePeriod()
		{
			var now = new DateOnly(2019, 1, 1);
			var defaultDateStart = now.AddDays(30);
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(defaultDateStart, now.AddDays(60))
			};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(now);
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(defaultDateStart);
		}

		[Test]
		public void ShouldCalculateDefaultDateToWcsPreferencePeriodStartDateWhereActivePersonPeriodExists()
		{
			var now = new DateOnly(2019, 1, 1);
			var defaultDateStart = now;
			var person = new Person();
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(defaultDateStart, now.AddDays (5))
			};
			//have an active person period that starts before the start of the preference period
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(now.AddDays(-20));
			personPeriod.SetParent(person);
			var personPeriods = new List<IPersonPeriod>
			{
				personPeriod
			};

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(workflowControlSet.PreferencePeriod.StartDate);
		}


		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenNoWorkflowControlSet()
		{
			var now = new DateOnly(2019, 1, 1);
			var personPeriods = new List<IPersonPeriod> { PersonPeriodFactory.CreatePersonPeriod(now) };
			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(null, w => w.StudentAvailabilityPeriod, personPeriods);

			result.Should().Be.EqualTo(now);
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenPeriodStartsBeforeToday()
		{
			var now = new DateOnly(2019, 1, 1);
			var workflowControlSet = new WorkflowControlSet(null)
										{
											StudentAvailabilityPeriod = new DateOnlyPeriod(new DateOnly(2019,1,1).AddDays(-7), new DateOnly(2019,1,1).AddDays(30))
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (new DateOnly(2019,1,1).AddDays (-20));
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };
			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.StudentAvailabilityPeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(2019,1,1));
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayIfNoPersonPeriod()
		{
			var now = new DateOnly(2019, 1, 1);
			var workflowControlSet = new WorkflowControlSet(null)
										{
											PreferencePeriod = new DateOnlyPeriod(now.AddDays(7), now.AddDays(30))
										};

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, new List<IPersonPeriod>());

			result.Should().Be.EqualTo(now);
		}

		[Test]
		public void ShouldCalculateDefaultDateToStartOfPersonPeriod()
		{
			var now = new DateOnly(2019,1,1);
			var personPeriodStart = new DateOnly(2019,1,1).AddDays(14);
			var workflowControlSet = new WorkflowControlSet(null)
			{
				PreferencePeriod = new DateOnlyPeriod(new DateOnly(2019,1,1).AddDays(7), new DateOnly(2019,1,1).AddDays(30))
			};
			var pp = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			pp.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> {pp};
			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(personPeriodStart);
		}
		
		[Test]
		public void ShouldCalculateDefaultDateToNow()
		{
			var now = new DateOnly(2019,1,1).AddDays(35);
			var personPeriodStart1 = new DateOnly(2019,1,1).AddDays(15);
			var personPeriodStart2 = new DateOnly(2019,1,1).AddDays(30);
			var workflowControlSet = new WorkflowControlSet(null)
			{
				PreferencePeriod = new DateOnlyPeriod(new DateOnly(2019,1,1).AddDays(10), new DateOnly(2019,1,1).AddDays(50))
			};
			var person = PersonFactory.CreatePersonWithId();
			var pp1 = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart1);
			pp1.SetParent(person);
			var pp2 = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart2);
			pp2.SetParent(person);
			var personPeriods = new List<IPersonPeriod> {pp1,pp2};

			var target = new DefaultDateCalculator(new MutableNow(now.Date.AddHours(10)), new FakeUserTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo()));

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date));
		}
	}
}