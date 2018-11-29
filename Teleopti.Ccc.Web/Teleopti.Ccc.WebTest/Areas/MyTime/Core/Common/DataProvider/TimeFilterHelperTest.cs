using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[MyTimeWebTest]
	[TestFixture]
	public class TimeFilterHelperTest : IIsolateSystem
	{
		public ITimeFilterHelper Target;
		private readonly DateOnly _testDate = new DateOnly(2015, 03, 02);
		public ILoggedOnUser User;
		public FakeUserTimeZone TimeZone;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TimeFilterHelper>().For<ITimeFilterHelper>();
			var person = PersonFactory.CreatePersonWithId();
			isolate.UseTestDouble(new FakeLoggedOnUser(person)).For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldGetFilterOnlyWithDayoff()
		{
			var result = Target.GetFilter(_testDate, null, null, true, false);

			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterOnlyWithEmptyDay()
		{
			var result = Target.GetFilter(_testDate, null, null, false, true);

			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterWithNothing()
		{
			var result = Target.GetFilter(_testDate, null, null, false, false);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetFilterWithStartTimeAsUtc()
		{
			setTimeZoneToSweden();

			const string startTime = "8:00-10:00";
			var result = Target.GetFilter(_testDate, startTime, null, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
		}

		[Test]
		public void ShouldGetFilterWithStartTimeAsUtcIncludingMinute()
		{
			setTimeZoneToSweden();

			const string startTime = "8:30-10:30";
			var result = Target.GetFilter(_testDate, startTime, null, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 30, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
		}

		[Test]
		public void ShouldGetFilterWithEndTimeAsUtc()
		{
			setTimeZoneToSweden();

			const string endTime = "8:00-10:00";
			var result = Target.GetFilter(_testDate, null, endTime, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithEndTimeAsUtcIncludingMinute()
		{
			setTimeZoneToSweden();

			const string endTime = "8:30-10:30";
			var result = Target.GetFilter(_testDate, null, endTime, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 30, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndDayoff()
		{
			setTimeZoneToSweden();

			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isDayOff = true;
			var result = Target.GetFilter(_testDate, startTime, endTime, isDayOff, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(8));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(10));
			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndEmptyDay()
		{
			setTimeZoneToSweden();

			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isEmptyDay = true;
			var result = Target.GetFilter(_testDate, startTime, endTime, false, isEmptyDay);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(8));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(10));
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterForNightShift()
		{
			setTimeZoneToSweden();

			const string endTime = "06:00-08:00";
			var result = Target.GetFilter(_testDate, "", endTime, false, false);

			var utcTime = new DateTime(2015, 3, 2, 5, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.Last().StartDateTime.Should().Be.EqualTo(utcTime.AddDays(1));
			result.EndTimes.Last().EndDateTime.Should().Be.EqualTo(utcTime.AddDays(1).AddHours(2));
		}

		[Test]
		public void ShouldForGetFilterWithPlusEndTime()
		{
			setTimeZoneToSweden();

			const string startTime = "06:00-08:00";
			var result = Target.GetFilter(_testDate, startTime, "", false, false);

			var utcTime = new DateTime(2015, 3, 2, 0, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(-1));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddDays(1).AddHours(23));
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithOnlyDayoff()
		{
			var scheduleFilter = new ScheduleFilter {IsDayOff = true};

			var result = Target.GetTeamSchedulesFilter(_testDate, scheduleFilter);

			result.IsDayOff.Should().Be.True();
		}
		
		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithOnlyNightShift()
		{
			var scheduleFilter = new ScheduleFilter{OnlyNightShift = true};

			var result = Target.GetTeamSchedulesFilter(_testDate, scheduleFilter);

			result.OnlyNightShift.Should().Be.True();
		}
		
		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithNothing()
		{
			var result = Target.GetTeamSchedulesFilter(_testDate, new ScheduleFilter());

			result.Should().Be.Null();
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithBothStartTimeAndEndTime()
		{
			setTimeZoneToSweden();

			var scheduleFilter = new ScheduleFilter
			{
				FilteredStartTimes = "8:00-10:00",
				FilteredEndTimes = "16:00-18:00",
				IsDayOff = false
			};

			var result = Target.GetTeamSchedulesFilter(_testDate, scheduleFilter);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(8));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(10));
			result.IsDayOff.Should().Be.False();
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithoutEndTime()
		{
			setTimeZoneToSweden();

			var scheduleFilter = new ScheduleFilter
			{
				FilteredStartTimes = "06:00-08:00",
				FilteredEndTimes = "",
				IsDayOff = false
			};

			var result = Target.GetTeamSchedulesFilter(_testDate, scheduleFilter);

			var utcTime = new DateTime(2015, 3, 2, 5, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			(result.EndTimes == null).Should().Be.EqualTo(true);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_NewTeamScheduleViewDesktop_76313)]
		public void ShouldGetTeamSchedulesFilterWithoutStartTime()
		{
			setTimeZoneToSweden();

			var scheduleFilter = new ScheduleFilter
			{
				FilteredStartTimes = "",
				FilteredEndTimes = "06:00-08:00",
				IsDayOff = false
			};

			var result = Target.GetTeamSchedulesFilter(_testDate, scheduleFilter);

			var utcTime = new DateTime(2015, 3, 2, 5, 0, 0, DateTimeKind.Utc);

			(result.StartTimes == null).Should().Be.EqualTo(true);
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.Last().StartDateTime.Should().Be.EqualTo(utcTime.AddDays(1));
			result.EndTimes.Last().EndDateTime.Should().Be.EqualTo(utcTime.AddDays(1).AddHours(2));
		}

		private void setTimeZoneToSweden()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
		}
	}
}
