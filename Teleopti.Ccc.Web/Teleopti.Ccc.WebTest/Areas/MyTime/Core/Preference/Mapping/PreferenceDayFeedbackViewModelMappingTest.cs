using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	public class MyTimeWebPreferenceMappingTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			
			var person = PersonFactory.CreatePersonWithGuid("a", "a");
			extend.AddService(person);
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble<FakePreferenceProvider>().For<IPreferenceProvider>();
			isolate.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
		}
	}

	public class FakePersonRuleSetBagProvider : IPersonRuleSetBagProvider
	{
		public IRuleSetBag ForDate(IPerson person, DateOnly date)
		{
			return person.Period(date).RuleSetBag;
		}

		public IDictionary<DateOnly, IRuleSetBag> ForPeriod(IPerson person, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}

	[TestFixture]
	[MyTimeWebPreferenceMappingTest]
	public class PreferenceDayFeedbackViewModelMappingTestByFake
	{
		public IScheduleProvider ScheduleProvider;
		public IPreferenceProvider PreferenceProvider;
		public FakePersonContractProvider PersonContractProvider;
		public Person Person;
		public PreferenceDayFeedbackViewModelMapper Mapper;
		public FakeRuleSetBagRepository RuleSetBagRepository;

		private IScheduleDay buildPersonSchedule(DateOnly date, TimeSpan startTime, TimeSpan endTime)
		{
			var schedule = ScheduleDayFactory.Create(date, Person);
			var start = DateTime.SpecifyKind(date.Date.Add(startTime), DateTimeKind.Utc);
			var end = DateTime.SpecifyKind(date.Date.Add(endTime), DateTimeKind.Utc);

			var assignmentPeriod = new DateTimePeriod(start, end);
			var personAssignment = new PersonAssignment(schedule.Person, schedule.Scenario, date);
			personAssignment.AddActivity(new Activity("d"), assignmentPeriod);
			schedule.Add(personAssignment);
			return schedule;
		}

		private IScheduleDay buildPersonScheduleDayOff(DateOnly date)
		{
			var schedule = ScheduleDayFactory.Create(date, Person);
			schedule.CreateAndAddDayOff(DayOffFactory.CreateDayOff());
			return schedule;
		}

		private void Setup()
		{
			var nightRest1 = new TimeSpan(12, 0, 0);
			var nightRest2 = new TimeSpan(6, 0, 0);

			var date1 = new DateOnly(2029, 1, 1);
			var date2 = new DateOnly(2029, 1, 2);
			var date3 = new DateOnly(2029, 1, 3);
			var date4 = new DateOnly(2029, 1, 4);
			var date5 = new DateOnly(2029, 1, 5);
			var date6 = new DateOnly(2029, 1, 6);
			var date9 = new DateOnly(2029, 1, 9);
			var date10 = new DateOnly(2029, 1, 10);
			var date11 = new DateOnly(2029, 1, 11);
			var date20 = new DateOnly(2029, 1, 20);
			var date21 = new DateOnly(2029, 1, 21);

			var personContract1 = PersonContractProvider.GetPersonContractWithSpecifiedNightRest("Fake Contract", nightRest1);
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2028, 1, 1), personContract1, TeamFactory.CreateSimpleTeam());
			Person.AddPersonPeriod(personPeriod1);

			var personContract2 = PersonContractProvider.GetPersonContractWithSpecifiedNightRest("Another Fake Contract", nightRest2);
			var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2029, 1, 11), personContract2, TeamFactory.CreateSimpleTeam());
			Person.AddPersonPeriod(personPeriod2);

			var schedule1 = buildPersonSchedule(date1, new TimeSpan(8, 0, 0), new TimeSpan(18, 30, 0));
			var schedule3 = buildPersonSchedule(date3, new TimeSpan(6, 0, 0), new TimeSpan(18, 30, 0));
			var schedule9 = buildPersonScheduleDayOff(date9);

			var scheduleProvider = ScheduleProvider as FakeScheduleProvider;
			if (scheduleProvider != null)
			{
				scheduleProvider.AddScheduleDay(schedule1);
				scheduleProvider.AddScheduleDay(schedule3);
				scheduleProvider.AddScheduleDay(schedule9);
			}

			var preferenceDay2 = new PreferenceDay(Person, date2, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(6, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(19, 0, 0), new TimeSpan(19, 30, 0))
			});

			var preferenceDay4 = new PreferenceDay(Person, date4, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(19, 10, 0), new TimeSpan(19, 30, 0))
			});

			var preferenceDay5 = new PreferenceDay(Person, date5, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(7, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0))
			});

			var preferenceDay6 = new PreferenceDay(Person, date6, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), new TimeSpan(4, 30, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0))
			});

			var preferenceDay10 = new PreferenceDay(Person, date10, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), new TimeSpan(4, 30, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0))
			});

			var preferenceDay11 = new PreferenceDay(Person, date11, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), new TimeSpan(4, 30, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0))
			});

			var preferenceDay20 = new PreferenceDay(Person, date20, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(1, 5, 0, 0), new TimeSpan(1, 6, 30, 0))
			});

			var preferenceDay21 = new PreferenceDay(Person, date21, new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 30, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(14, 0, 0), new TimeSpan(15, 30, 0))
			});

			var personPreferenceProvider = PreferenceProvider as FakePreferenceProvider;
			if (personPreferenceProvider != null)
			{
				personPreferenceProvider.AddPreference(preferenceDay2);
				personPreferenceProvider.AddPreference(preferenceDay4);
				personPreferenceProvider.AddPreference(preferenceDay5);
				personPreferenceProvider.AddPreference(preferenceDay6);
				personPreferenceProvider.AddPreference(preferenceDay10);
				personPreferenceProvider.AddPreference(preferenceDay11);
				personPreferenceProvider.AddPreference(preferenceDay20);
				personPreferenceProvider.AddPreference(preferenceDay21);
			}
		}

		[Test]
		public void ShouldMapDate()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 2);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.Date.Should().Be.EqualTo(testDate.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapExpectedNightRest()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 2);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.ExpectedNightRestTimeSpan.Should().Be.EqualTo("12:00");
		}

		[Test]
		public void ShouldMapHasNightRestViolationWhenPreferenceConflictsWithShifts()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 2);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.HasNightRestViolationToNextDay.Should().Be.True();
			targetReturn.HasNightRestViolationToPreviousDay.Should().Be.True();
		}

		[Test]
		public void ShouldMapHasNightRestViolationWhenTheNeighbouringDayIsEmptyDay()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 6);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.HasNightRestViolationToNextDay.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasNightRestViolationWhenPreferenceConflictsWithPerferences()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 5);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.HasNightRestViolationToNextDay.Should().Be.True();
			targetReturn.HasNightRestViolationToPreviousDay.Should().Be.True();
		}

		[Test]
		public void ShouldMapHasNightRestViolationWhenTheNeighbouringDayIsDayOff()
		{
			Setup();
			var testDate = new DateOnly(2029, 1, 10);
			var targetReturn = Mapper.Map(testDate);
			targetReturn.HasNightRestViolationToPreviousDay.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasNightRestViolationUsingTheRightContract()
		{
			Setup();
			var testDate1 = new DateOnly(2029, 1, 10);
			var targetReturn1 = Mapper.Map(testDate1);
			targetReturn1.HasNightRestViolationToNextDay.Should().Be.True();

			var testDate2 = new DateOnly(2029, 1, 10);
			var targetReturn2 = Mapper.Map(testDate2);
			targetReturn2.HasNightRestViolationToPreviousDay.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasNightRestViolationForOverNightPreference()
		{
			Setup();
			var testDate1 = new DateOnly(2029, 1, 20);
			var targetReturn1 = Mapper.Map(testDate1);
			targetReturn1.HasNightRestViolationToNextDay.Should().Be.True();

			var testDate2 = new DateOnly(2029, 1, 21);
			var targetReturn2 = Mapper.Map(testDate2);
			targetReturn2.HasNightRestViolationToPreviousDay.Should().Be.True();
		}
	}
}