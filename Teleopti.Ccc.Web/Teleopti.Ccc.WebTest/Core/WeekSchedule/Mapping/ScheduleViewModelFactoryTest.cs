using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleViewModelFactoryTest2 : ISetup
	{
		public IScheduleViewModelFactory Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateEnglishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldMap()
		{
			var viewModel = Target.CreateWeekViewModel(new DateOnly(Now.UtcDateTime()));

			viewModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapTimeLineEdges()
		{
			Culture.IsSwedish();
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDst()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-28 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDay()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("01:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(6).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnEndDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("01:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapBaseUtcOffset()
		{
			Culture.IsSwedish();
			TimeZone.IsHawaii();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}


		[Test]
		public void ShouldMapDaylightSavingTimeAdjustment()
		{

			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.StartDateTime.Should().Be(new DateTime(2015, 3, 29, 1, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.EndDateTime.Should().Be(new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldNotMapDaylightSavingTimeAdjustment()
		{
			Culture.IsSwedish();
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);

		}

		[Test]
		public void ShouldValidatePeriodSelectionStartDateAndEndDateFormatCorrectly()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			var date = new DateOnly(2015, 07, 06);
			var viewModel = Target.CreateWeekViewModel(date);


			Assert.AreEqual("2015-07-06", viewModel.CurrentWeekStartDate);
			Assert.AreEqual("2015-07-12", viewModel.CurrentWeekEndDate);
		}

	}


}
