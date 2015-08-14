﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	
	[TestFixture]
	public class PersonPreferenceDayOccupationFactoryTest
	{
		private PersonPreferenceDayOccupationFactory target;
		private IPerson person;


		[SetUp]
		public void Init()
		{			
			var date1 = new DateOnly(2029, 1, 1);
			var date2 = new DateOnly(2029, 1, 3);
			var date3 = new DateOnly(2029, 1, 5);


			person = PersonFactory.CreatePersonWithGuid("a", "a");
			var schedule = ScheduleDayFactory.Create(date1, person);
			var schedule3 = ScheduleDayFactory.Create(date3, person);

			var start1 = new DateTime(2029, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var end1 = new DateTime(2029, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start1, end1);	
			var pa1 = new PersonAssignment(schedule.Person, schedule.Scenario, date1);
			pa1.AddActivity(new Activity("d"), assignmentPeriod);
			schedule.Add(pa1);
			var scheduleProvider = new FakeScheduleProvider(schedule, schedule3);

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

			var personPreferenceProvider = new FakePreferenceProvider(preferenceDay2, preferenceDay3);

			var userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Utc);

			IWorkTimeMinMaxCalculator mmc = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();

			var workTimeMinMaxResult = new WorkTimeMinMaxCalculationResult();

			workTimeMinMaxResult.WorkTimeMinMax = new WorkTimeMinMax()
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(5, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(12, 0, 0), new TimeSpan(12, 0, 0))
			};

			mmc.Stub(x => x.WorkTimeMinMax(date3, person, schedule3)).Return(workTimeMinMaxResult);


			target = new PersonPreferenceDayOccupationFactory(			
				scheduleProvider, 
				personPreferenceProvider,
				userTimeZone,
				mmc);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreferenceThatHasNoStartTimeAndEndTimeLimitations()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 5));

			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(5, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(5, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(12, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(12, 0, 0));
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithShift()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 1));
			occupation.HasShift.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreference()
		{
			var occupation = target.GetPreferenceDayOccupation(person, new DateOnly(2029, 1, 3));

			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(10, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(11, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(15, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(16, 0, 0));
		}

	}
}
