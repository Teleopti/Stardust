using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PersonPreferenceDayOccupationFactoryTest
	{
		FakeScheduleProvider scheduleProvider;
		[SetUp]
		public void Init()
		{
			var date1 = new DateOnly(2029, 1, 1);
			var date2 = new DateOnly(2029, 1, 3);
			var date3 = new DateOnly(2029, 1, 5);
			var date4 = new DateOnly(2029, 1, 7);
			var date5 = new DateOnly(2029, 1, 9);

			person = PersonFactory.CreatePersonWithGuid("a", "a");
			var schedule = ScheduleDayFactory.Create(date1, person);
			var schedule2 = ScheduleDayFactory.Create(date2, person);
			var schedule3 = ScheduleDayFactory.Create(date3, person);
			var schedule5 = ScheduleDayFactory.Create(date5, person);
			var bag = new RuleSetBag();
			var provider = MockRepository.GenerateMock<IPersonRuleSetBagProvider>();

			var start1 = new DateTime(2029, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var end1 = new DateTime(2029, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start1, end1);
			var pa1 = new PersonAssignment(schedule.Person, schedule.Scenario, date1);
			pa1.AddActivity(new Activity("d"), assignmentPeriod);
			schedule.Add(pa1);

			var pa2 = new PersonAssignment(schedule2.Person, schedule2.Scenario, date2);
			schedule2.Add(pa2);

			var pa5 = new PersonAssignment(person, schedule5.Scenario, date4);
			var assOvernightPeriod = new DateTimePeriod(new DateTime(2029, 1, 9, 20, 0, 0, DateTimeKind.Utc), new DateTime(2029, 1, 11, 5, 0, 0, DateTimeKind.Utc));
			pa5.AddActivity(new Activity("activity"), assOvernightPeriod);
			schedule5.Add(pa5);
			scheduleProvider = new FakeScheduleProvider(schedule, schedule2, schedule3, schedule5);

			var preferenceRestriction1 = new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0))
			};

			var preferenceRestriction3 = new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(null, null),
				EndTimeLimitation = new EndTimeLimitation(null, null)
			};


			var preferenceDay2 = new PreferenceDay(person, date2, preferenceRestriction1);
			var preferenceDay3 = new PreferenceDay(person, date3, preferenceRestriction3);
			var preferenceDay4 = new PreferenceDay(person, date4, preferenceRestriction1);

			var personPreferenceProvider = new FakePreferenceProvider(preferenceDay2, preferenceDay3, preferenceDay4);

			var userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Utc);

			var mmc = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();

			var workTimeMinMaxResult = new WorkTimeMinMaxCalculationResult
			{
				WorkTimeMinMax = new WorkTimeMinMax
				{
					StartTimeLimitation = new StartTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(5, 0, 0)),
					EndTimeLimitation = new EndTimeLimitation(new TimeSpan(12, 0, 0), new TimeSpan(12, 0, 0))
				}
			};

			provider.Stub(x => x.ForDate(person, date3)).Return(bag);

			var ruleSetBags1 = new Dictionary<DateOnly, IRuleSetBag>();
			foreach (var date in new DateOnlyPeriod(date1, date4).DayCollection())
			{
				ruleSetBags1.Add(date, bag);
			}
			provider.Stub(x => x.ForPeriod(person, new DateOnlyPeriod(date1, date4))).IgnoreArguments().Return(ruleSetBags1);


			mmc.Stub(x => x.WorkTimeMinMax(date3, bag, schedule3)).Return(workTimeMinMaxResult);

			target = new PersonPreferenceDayOccupationFactory(scheduleProvider, personPreferenceProvider, provider,
				userTimeZone, mmc);
		}

		private PersonPreferenceDayOccupationFactory target;
		private IPerson person;

		private void shouldBeOccupationWithShift(PersonPreferenceDayOccupation occupation)
		{
			occupation.HasShift.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
		}

		private void shouldBeOccupationWithNoSchedule(PersonPreferenceDayOccupation occupation)
		{
			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(10, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(11, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(15, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(16, 0, 0));
		}

		private void shouldBeOccupationWithoutScheduleNorPreference(PersonPreferenceDayOccupation occupation)
		{
			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(false);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(null);
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(null);

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(null);
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(null);
		}

		private void shouldBeOccupationHasNoStartTimeAndEndTimeLimitations(PersonPreferenceDayOccupation occupation)
		{
			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(5, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(5, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(12, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(12, 0, 0));
		}

		private void shouldBeOccupationWithEmptySchedule(PersonPreferenceDayOccupation occupation)
		{
			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(10, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(11, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(15, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(16, 0, 0));
		}

		[Test]
		public void ShouldReturnCorrectOccupationForPeriod()
		{
			var period = new DateOnlyPeriod(new DateOnly(2029, 1, 1), new DateOnly(2029, 1, 7));
			var occupations = target.GetPreferencePeriodOccupation(person, period);

			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);

			occupations.Count.Should().Be.EqualTo(7);
			shouldBeOccupationWithShift(occupations[new DateOnly(2029, 1, 1)]);
			shouldBeOccupationWithoutScheduleNorPreference(occupations[new DateOnly(2029, 1, 2)]);
			shouldBeOccupationWithEmptySchedule(occupations[new DateOnly(2029, 1, 3)]);
			shouldBeOccupationWithoutScheduleNorPreference(occupations[new DateOnly(2029, 1, 4)]);
			shouldBeOccupationHasNoStartTimeAndEndTimeLimitations(occupations[new DateOnly(2029, 1, 5)]);
			shouldBeOccupationWithoutScheduleNorPreference(occupations[new DateOnly(2029, 1, 6)]);
			shouldBeOccupationWithNoSchedule(occupations[new DateOnly(2029, 1, 7)]);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreferenceThatHasNoStartTimeAndEndTimeLimitations()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 5));
			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			shouldBeOccupationHasNoStartTimeAndEndTimeLimitations(occupation);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreferenceWhenEmptySchedule()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 3));
			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			shouldBeOccupationWithEmptySchedule(occupation);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreferenceWhenNoSchedule()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 7));
			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			shouldBeOccupationWithNoSchedule(occupation);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithShift()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 1));
			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			shouldBeOccupationWithShift(occupation);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithOvernightShiftEndOnDayAfterNextDay()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 9));

			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			occupation.HasShift.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(20, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(20, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(47, 59, 59));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(47, 59, 59));
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithWithoutScheduleNorPreference()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 2));
			Assert.IsTrue(scheduleProvider.LatestScheduleLoadOptions.LoadRestrictions);
			shouldBeOccupationWithoutScheduleNorPreference(occupation);
		}
	}
}