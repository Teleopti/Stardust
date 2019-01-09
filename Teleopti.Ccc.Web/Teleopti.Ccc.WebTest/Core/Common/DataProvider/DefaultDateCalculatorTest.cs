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
			var now = DateOnly.Today;
			var preferencePeriodStart = DateOnly.Today.AddDays(5);
			var preferencePeriodEnd = DateOnly.Today.AddDays(35);
			var personPeriodStart = DateOnly.Today.AddDays(40);
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
			};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date.ToLocalTime()));
		}

		[Test]
		public void ShouldGetNowWhenPersonPeriodAndPreferencePeriodIncludeNow()
		{
			var now = DateOnly.Today.AddDays(15);
			var preferencePeriodStart = DateOnly.Today;
			var preferencePeriodEnd = DateOnly.Today.AddDays(30);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date.ToLocalTime()));
		}
		
		[Test]
		public void ShouldGetRecentPeriodWhenNowOverPreferencePeriod()
		{
			var now = DateOnly.Today.AddDays(50);
			var preferencePeriodStart = DateOnly.Today;
			var preferencePeriodEnd = DateOnly.Today.AddDays(30);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date.ToLocalTime()));
		}
		
		[Test]
		public void ShouldGetPreferencePeriodStartWhenThereIsAPreferencePeriodInTheFuture()
		{
			var now = DateOnly.Today;
			var preferencePeriodStart = DateOnly.Today.AddDays(30);
			var preferencePeriodEnd = DateOnly.Today.AddDays(80);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(preferencePeriodStart);
		}
		
		[Test]
		public void ShouldGetPersonPeriodStartWhenThereIsAPreferencePeriodInTheFutureButDontHavePersonPeriod()
		{
			var now = DateOnly.Today;
			var personPeriodStart = DateOnly.Today.AddDays(45);
			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(30), DateOnly.Today.AddDays(80))
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			personPeriod.SetParent (new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(personPeriodStart);
		}
		
		[Test]
		public void ShouldGetNowWhenNowPassOverPreferencePeriodAndThereIsFuturePersonPeriod()
		{
			var now = DateOnly.Today.AddDays(45);
			var preferencePeriodStart = DateOnly.Today;
			var preferencePeriodEnd = DateOnly.Today.AddDays(30);

			var workflowControlSet = new WorkflowControlSet
										{
											PreferencePeriod = new DateOnlyPeriod(preferencePeriodStart, preferencePeriodEnd)
										};

			var person = PersonFactory.CreatePersonWithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today);
			personPeriod1.SetParent(person);
			var personPeriod2= PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(60));
			personPeriod2.SetParent(person);

			var personPeriods = new List<IPersonPeriod> {personPeriod1, personPeriod2};

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date.ToLocalTime()));

		}

		[Test]
		public void ShouldGetPreferencePeriodStartWhenPreferencePeriodInSecondPersonPeriod()
		{
			var now = DateOnly.Today.AddDays(45);
			var preferencePeriodStart = DateOnly.Today.AddDays(70);
			var preferencePeriodEnd = DateOnly.Today.AddDays(100);
			var personPeriodStart1 = DateOnly.Today;
			var personPeriodStart2 = DateOnly.Today.AddDays(60);

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

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(preferencePeriodStart);
		}

		[Test]
		public void ShouldCalculateDefaultDateToFirstDateInFuturePeriod()
		{
			var defaultDateStart = DateOnly.Today.AddDays(30);
			var workflowControlSet = new WorkflowControlSet
			{
				PreferencePeriod = new DateOnlyPeriod(defaultDateStart, DateOnly.Today.AddDays(60))
			};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };

			var target = new DefaultDateCalculator(new Now(), new FakeUserTimeZone());

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

			var target = new DefaultDateCalculator(new Now(), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(workflowControlSet.PreferencePeriod.StartDate);
		}


		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenNoWorkflowControlSet()
		{
			var personPeriods = new List<IPersonPeriod> { PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today) };
			var target = new DefaultDateCalculator(new Now(), new FakeUserTimeZone());

			var result = target.Calculate(null, w => w.StudentAvailabilityPeriod, personPeriods);

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldCalculateDefaultDateToTodayWhenPeriodStartsBeforeToday()
		{
			var workflowControlSet = new WorkflowControlSet(null)
										{
											StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-7), DateOnly.Today.AddDays(30))
										};
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod (DateOnly.Today.AddDays (-20));
			personPeriod.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> { personPeriod };
			var target = new DefaultDateCalculator(new Now(), new FakeUserTimeZone());

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

			var target = new DefaultDateCalculator(new Now(), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, new List<IPersonPeriod>());

			result.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldCalculateDefaultDateToStartOfPersonPeriod()
		{
			var now = DateOnly.Today;
			var personPeriodStart = DateOnly.Today.AddDays(14);
			var workflowControlSet = new WorkflowControlSet(null)
			{
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(7), DateOnly.Today.AddDays(30))
			};
			var pp = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart);
			pp.SetParent(new Person());
			var personPeriods = new List<IPersonPeriod> {pp};
			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(personPeriodStart);
		}
		
		[Test]
		public void ShouldCalculateDefaultDateToNow()
		{
			var now = DateOnly.Today.AddDays(35);
			var personPeriodStart1 = DateOnly.Today.AddDays(15);
			var personPeriodStart2 = DateOnly.Today.AddDays(30);
			var workflowControlSet = new WorkflowControlSet(null)
			{
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(10), DateOnly.Today.AddDays(50))
			};
			var person = PersonFactory.CreatePersonWithId();
			var pp1 = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart1);
			pp1.SetParent(person);
			var pp2 = PersonPeriodFactory.CreatePersonPeriod(personPeriodStart2);
			pp2.SetParent(person);
			var personPeriods = new List<IPersonPeriod> {pp1,pp2};

			var target = new DefaultDateCalculator(new MutableNow(now.Date), new FakeUserTimeZone());

			var result = target.Calculate(workflowControlSet, w => w.PreferencePeriod, personPeriods);

			result.Should().Be.EqualTo(new DateOnly(now.Date.ToLocalTime()));
		}
	}
}