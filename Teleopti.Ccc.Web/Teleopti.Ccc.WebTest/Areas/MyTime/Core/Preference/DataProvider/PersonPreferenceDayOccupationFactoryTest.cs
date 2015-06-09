﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PersonPreferenceDayOccupationFactoryTest
	{
		private PersonPreferenceDayOccupationFactory target;

		[SetUp]
		public void Init()
		{			
			var date1 = new DateOnly(2029, 1, 1);
			var date2 = new DateOnly(2029, 1, 3);

			var loggedOnUser = new FakeLoggedOnUser();
			var schedule = ScheduleDayFactory.Create(date1, loggedOnUser.CurrentUser());

			var start = new DateTime(2029, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2029, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var pa = new PersonAssignment(schedule.Person, schedule.Scenario, date1);
			pa.AddActivity(new Activity("d"), assignmentPeriod);
			schedule.Add(pa);
			var scheduleProvider = new FakeScheduleProvider(schedule);


			var preferenceRestriction = new PreferenceRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),
				EndTimeLimitation = new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0))
			};

			var preferenceDay = new PreferenceDay(loggedOnUser.CurrentUser(), date2, preferenceRestriction);
			var personPreferenceProvider = new FakePreferenceProvider(preferenceDay);



			target = new PersonPreferenceDayOccupationFactory(
				loggedOnUser, 
				scheduleProvider, 
				personPreferenceProvider);
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithShift()
		{
			var occupation = target.GetPreferenceDayOccupation(new DateOnly(2029, 1, 1));
			occupation.HasShift.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(8, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(17, 0, 0));
		}

		[Test]
		public void ShouldReturnCorrectOccupationWithPreference()
		{
			var occupation = target.GetPreferenceDayOccupation(new DateOnly(2029, 1, 3));

			occupation.HasShift.Should().Be.EqualTo(false);
			occupation.HasPreference.Should().Be.EqualTo(true);
			occupation.StartTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(10, 0, 0));
			occupation.StartTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(11, 0, 0));

			occupation.EndTimeLimitation.StartTime.Should().Be.EqualTo(new TimeSpan(15, 0, 0));
			occupation.EndTimeLimitation.EndTime.Should().Be.EqualTo(new TimeSpan(16, 0, 0));
		}

	}
}
