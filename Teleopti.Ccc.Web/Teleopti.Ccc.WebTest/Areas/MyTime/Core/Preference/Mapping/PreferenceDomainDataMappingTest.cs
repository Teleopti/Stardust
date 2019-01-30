using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDomainDataMappingTest
	{
		private IVirtualSchedulePeriodProvider virtualScheduleProvider;
		private FakePreferenceDayRepository preferenceDayRepository;
		private IPerson person;
		private PreferenceDomainDataMapper target;
		private FakeScheduleProvider scheduleProvider;
		private IScenario scenario;

		[SetUp]
		public void Setup()
		{
			virtualScheduleProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();

			person = new Person
			{
				WorkflowControlSet = new WorkflowControlSet(null)
				{
					PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
					PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
				}
			};

			preferenceDayRepository = new FakePreferenceDayRepository();
			var mustHaveRestrictionProvider = new MustHaveRestrictionProvider(preferenceDayRepository);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			scheduleProvider = new FakeScheduleProvider();

			scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			target = new PreferenceDomainDataMapper(virtualScheduleProvider,loggedOnUser,mustHaveRestrictionProvider, scheduleProvider);
		}
		
		[Test]
		public void ShouldMapSelectedDate()
		{
			var result = target.Map(DateOnly.Today);

			result.SelectedDate.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldMapPeriod()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = target.Map(DateOnly.Today);

			result.Period.Should().Be(period);
		}

		[Test]
		public void ShouldMapWorkflowControlSet()
		{
			var result = target.Map(DateOnly.Today);

			result.WorkflowControlSet.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldMapDays()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = target.Map(DateOnly.Today);

			var expected = DateOnly.Today.DateRange(4);
			result.Days.Select(d => d.Date).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapMaxMustHave()
		{
			const int maxMustHave = 8;
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.MustHavePreference).Return(maxMustHave);
			virtualScheduleProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);

			var result = target.Map(DateOnly.Today);

			result.MaxMustHave.Should().Be.EqualTo(maxMustHave);
		}

		[Test]
		public void ShouldReturnCorrectCurrentMustHave()
		{
			preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction
			{
				MustHave = true
			}));
			preferenceDayRepository.Add(new PreferenceDay(person,DateOnly.Today.AddDays(1),new PreferenceRestriction
			{
				MustHave = false
			}));

			person.AddSchedulePeriod(new SchedulePeriod(DateOnly.Today.AddDays(-1), SchedulePeriodType.Day, 10000 ));

			const int maxMustHave = 8;
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.MustHavePreference).Return(maxMustHave);
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today))
				.Return(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3)));

			var result = target.Map(DateOnly.Today);
			result.CurrentMustHave.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectScheduledDays()
		{
			var _date = new DateTime(2019, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var date = new DateOnly(_date);
			var period = new DateOnlyPeriod(date, date.AddDays(1));
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(date)).Return(period);
			var _period = new DateTimePeriod(_date.AddHours(8), _date.AddHours(17));
			var schedule = ScheduleDayFactory.Create(date,person,scenario);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, _period);
			personAssignment.AddActivity(ActivityFactory.CreateActivity("Phone"), _period);
			schedule.Add(personAssignment);
			scheduleProvider.AddScheduleDay(schedule);

			var result = target.Map(date);
			result.ScheduledDays.First().Should().Be.EqualTo(date);
		}

		
	}
}