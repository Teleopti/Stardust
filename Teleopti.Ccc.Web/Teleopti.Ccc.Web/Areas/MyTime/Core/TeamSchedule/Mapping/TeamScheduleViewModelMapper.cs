using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleViewModelMapper
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICreateHourText _createHourText;
		private readonly IPersonNameProvider _personNameProvider;

		public TeamScheduleViewModelMapper(IUserTimeZone userTimeZone, ICreateHourText createHourText, IPersonNameProvider personNameProvider)
		{
			_userTimeZone = userTimeZone;
			_createHourText = createHourText;
			_personNameProvider = personNameProvider;
		}

		private class TimeLineMappingData
		{
			public DateTime Time { get; set; }
			public DateTimePeriod DisplayTimePeriod { get; set; }
		}

		private class LayerMappingData
		{
			public DateTimePeriod DisplayTimePeriod { get; set; }
			public ITeamScheduleLayer Layer { get; set; }
		}

		public TeamScheduleViewModel Map(TeamScheduleDomainData s)
		{
			return new TeamScheduleViewModel
			{
				ShiftTradePermisssion = false,
				ShiftTradeBulletinBoardPermission = false,
				AgentSchedules = map(s.Days),
				PeriodSelection = mapPeriodSelection(s),
				TeamSelection = s.TeamOrGroupId,
				TimeLine = mapTimeline(s)
			};
		}

		private AgentScheduleViewModel[] map(IEnumerable<TeamScheduleDayDomainData> s)
		{
			var map = new Func<LayerMappingData, LayerViewModel>(t =>
			{
				if (!t.Layer.Period.HasValue)
				{
					return new LayerViewModel
					{
						ActivityName = t.Layer.ActivityName,
						Color = t.Layer.DisplayColor.ToHtml(),
					};
				}

				decimal positionPercent = 0;
				decimal endPositionPercent = 0;
				if (t.DisplayTimePeriod.StartDateTime != DateTime.MinValue)
				{
					decimal positionTicks = (t.Layer.Period.Value.StartDateTime - t.DisplayTimePeriod.StartDateTime).Ticks;
					decimal endPositionTicks = (t.Layer.Period.Value.EndDateTime - t.DisplayTimePeriod.StartDateTime).Ticks;
					decimal displayedTicks = t.DisplayTimePeriod.ElapsedTime().Ticks;
					positionPercent = positionTicks/displayedTicks;
					endPositionPercent = endPositionTicks/displayedTicks;
				}
				
				return new LayerViewModel
				{
					ActivityName = t.Layer.ActivityName,
					StartTime = _createHourText.CreateText(t.Layer.Period.Value.StartDateTime),
					EndTime = _createHourText.CreateText(t.Layer.Period.Value.EndDateTime),
					Color = t.Layer.DisplayColor.ToHtml(),
					PositionPercent = positionPercent,
					EndPositionPercent = endPositionPercent
				};
			});
			return s.Select(t => new AgentScheduleViewModel
			{
				AgentName = _personNameProvider.BuildNameFromSetting(t.Person.Name),
				DayOffText = t.Projection?.DayOff?.Description.Name,
				HasDayOffUnder = t.HasDayOffUnder,
				Layers = t.Projection?.Layers == null
					? new LayerViewModel[0]
					: (from l in t.Projection.Layers
						select map(new LayerMappingData
						{
							Layer = l,
							DisplayTimePeriod = t.DisplayTimePeriod,
						})).ToArray()
			}).ToArray();
		}

		private TimeLineViewModel[] mapTimeline(TeamScheduleDomainData s)
		{
			var startTime = s.DisplayTimePeriod.StartDateTime;
			var endTime = s.DisplayTimePeriod.EndDateTime;
			var firstHour = startTime
				.Subtract(new TimeSpan(0, 0, startTime.Minute, startTime.Second, startTime.Millisecond))
				.AddHours(1);
			var times = firstHour
				.TimeRange(endTime, TimeSpan.FromHours(1))
				.Union(new[] { startTime })
				.Union(new[] { endTime })
				.Union(new[] { firstHour })
				.OrderBy(t => t)
				.Distinct()
				;

			var map = new Func<TimeLineMappingData, TimeLineViewModel>(t =>
			{
				var displayedTicks = (decimal)t.DisplayTimePeriod.EndDateTime.Ticks - t.DisplayTimePeriod.StartDateTime.Ticks;
				var positionTicks = (decimal)t.Time.Ticks - t.DisplayTimePeriod.StartDateTime.Ticks;
				var positionPercent = positionTicks / displayedTicks;
				return new TimeLineViewModel
				{
					ShortTime = _createHourText.CreateText(t.Time),
					PositionPercent = positionPercent,
					IsFullHour = TimeZoneInfo.ConvertTimeFromUtc(t.Time, _userTimeZone.TimeZone()).TimeOfDay.Minutes == 0
				};
			});

			return times.Select(t => map(new TimeLineMappingData
			{
				DisplayTimePeriod = s.DisplayTimePeriod, Time = t
			})).ToArray();
		}

		private PeriodSelectionViewModel mapPeriodSelection(TeamScheduleDomainData s)
		{
			return new PeriodSelectionViewModel
			{
				Date = s.Date.ToFixedClientDateOnlyFormat(),
				Display = s.Date.ToShortDateString(),
				StartDate = s.DisplayTimePeriod.StartDateTime.Date,
				EndDate = s.DisplayTimePeriod.EndDateTime.Date,
				SelectableDateRange =
					mapRange(new DateOnlyPeriod(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime),
						new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime))),
				SelectedDateRange = mapRange(s.Date.ToDateOnlyPeriod()),
				PeriodNavigation = mapPeriodNavigation(s)
			};
		}

		private PeriodDateRangeViewModel mapRange(DateOnlyPeriod s)
		{
			return new PeriodDateRangeViewModel
			{
				MaxDate = s.EndDate.ToFixedClientDateOnlyFormat(),
				MinDate = s.StartDate.ToFixedClientDateOnlyFormat()
			};
		}

		private PeriodNavigationViewModel mapPeriodNavigation(TeamScheduleDomainData s)
		{
			return new PeriodNavigationViewModel
			{
				CanPickPeriod = true,
				HasNextPeriod = true,
				HasPrevPeriod = true,
				NextPeriod = s.Date.AddDays(1).ToFixedClientDateOnlyFormat(),
				PrevPeriod = s.Date.AddDays(-1).ToFixedClientDateOnlyFormat()
			};
		}
	}
}
