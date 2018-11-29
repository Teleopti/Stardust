using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeTimeLineHoursViewModelMapperTest
	{
		[Test]
		public void ShouldMapTimeLine8To17WhenNoExistingSchedule()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var timeLineHoursViewModelFactory = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelFactory>();
			var person = new Person().WithName(new Name("John", "Doe"));
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);
			var date = DateOnly.Today;
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(7).AddMinutes(45),
																			  date.Date.AddHours(17).AddMinutes(15), timeZone);

			var firstTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = string.Empty, LengthInMinutesToDisplay = 15 };
			var secondTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "8", LengthInMinutesToDisplay = 60 };
			var lastTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "17", LengthInMinutesToDisplay = 15 };

			timeLineHoursViewModelFactory.Stub(x => x.CreateTimeLineHours(period))
										 .Return(new List<ShiftTradeTimeLineHoursViewModel>
											 {
												 firstTimeLineHour,
												 secondTimeLineHour,
												 lastTimeLineHour
											 });

			var target = new ShiftTradeTimeLineHoursViewModelMapper(loggedOnUser, timeLineHoursViewModelFactory);
			var result = target.Map(null, new List<ShiftTradeAddPersonScheduleViewModel>(), date);

			result.ElementAt(1).HourText.Should().Be.EqualTo("8");
			result.ElementAt(1).LengthInMinutesToDisplay.Should().Be.EqualTo(60);

			result.Last().HourText.Should().Be.EqualTo("17");
			result.Last().LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapTimeLineFromSchedules()
		{
			var date = DateOnly.Today;
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel
			{
				ScheduleLayers =
						new List<TeamScheduleLayerViewModel>
							{
								new TeamScheduleLayerViewModel
									{
										Start = date.Date.AddHours(11),
										End = date.Date.AddHours(16)
									}
							}.ToArray()
			};

			var possibleTradeSchedule = new ShiftTradeAddPersonScheduleViewModel
			{
				ScheduleLayers =
						new List<TeamScheduleLayerViewModel>
							{
								new TeamScheduleLayerViewModel
									{
										Start = date.Date.AddHours(6),
										End = date.Date.AddHours(13)
									}
							}.ToArray()
							,
				MinStart = date.Date.AddHours(6)
			};

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var timeLineHoursViewModelFactory = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelFactory>();
			var person = new Person().WithName(new Name("John", "Doe"));
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(5).AddMinutes(45),
																			  date.Date.AddHours(16).AddMinutes(15), timeZone);

			var firstTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = string.Empty, LengthInMinutesToDisplay = 15 };
			var secondTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "8", LengthInMinutesToDisplay = 60 };
			var lastTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "17", LengthInMinutesToDisplay = 15 };
			var timeLineHourViewModels = new List<ShiftTradeTimeLineHoursViewModel> { firstTimeLineHour, secondTimeLineHour, lastTimeLineHour };

			timeLineHoursViewModelFactory.Stub(x => x.CreateTimeLineHours(period)).Return(timeLineHourViewModels);

			var target = new ShiftTradeTimeLineHoursViewModelMapper(loggedOnUser, timeLineHoursViewModelFactory);
			var result = target.Map(mySchedule, new List<ShiftTradeAddPersonScheduleViewModel> { possibleTradeSchedule }, date);
			result.Should().Be.SameInstanceAs(timeLineHourViewModels);
		}

		[Test]
		public void ShouldMapTimeLineWhenThereIsOvetimeOnDayOffOnMySchedule()
		{
			var date = DateOnly.Today;
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel
			{
				DayOffName = "dayoff",
				StartTimeUtc = date.Date.AddHours(06),
				IsDayOff = true,
				ScheduleLayers =
						new List<TeamScheduleLayerViewModel>
							{
								new TeamScheduleLayerViewModel
									{
										Start = date.Date.AddHours(5),
										End = date.Date.AddHours(7),
										IsOvertime = true
									}
							}.ToArray()
			};

			var possibleTradeSchedule = new ShiftTradeAddPersonScheduleViewModel
			{
				ScheduleLayers =
						new List<TeamScheduleLayerViewModel>
							{
								new TeamScheduleLayerViewModel
									{
										Start = date.Date.AddHours(6),
										End = date.Date.AddHours(13)
									}
							}.ToArray()
							,
				MinStart = date.Date.AddHours(6)
			};

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var timeLineHoursViewModelFactory = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelFactory>();
			var person = new Person().WithName(new Name("John", "Doe"));
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(4).AddMinutes(45),
																			  date.Date.AddHours(13).AddMinutes(15), timeZone);

			var firstTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = string.Empty, LengthInMinutesToDisplay = 15 };
			var secondTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "6", LengthInMinutesToDisplay = 60 };
			var lastTimeLineHour = new ShiftTradeTimeLineHoursViewModel { HourText = "14", LengthInMinutesToDisplay = 15 };
			var timeLineHourViewModels = new List<ShiftTradeTimeLineHoursViewModel> { firstTimeLineHour, secondTimeLineHour, lastTimeLineHour };

			timeLineHoursViewModelFactory.Stub(x => x.CreateTimeLineHours(period)).Return(timeLineHourViewModels);

			var target = new ShiftTradeTimeLineHoursViewModelMapper(loggedOnUser, timeLineHoursViewModelFactory);
			var result = target.Map(mySchedule, new List<ShiftTradeAddPersonScheduleViewModel> { possibleTradeSchedule }, date);
			result.Should().Be.SameInstanceAs(timeLineHourViewModels);
		}
	}
}