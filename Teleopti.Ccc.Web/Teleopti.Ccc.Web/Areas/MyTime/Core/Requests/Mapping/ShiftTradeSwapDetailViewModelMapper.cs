using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeSwapDetailViewModelMapper
	{
		private readonly IShiftTradeTimeLineHoursViewModelFactory _timelineViewModelFactory;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private const int timeLineOffset = 15;

		public ShiftTradeSwapDetailViewModelMapper(IShiftTradeTimeLineHoursViewModelFactory timelineViewModelFactory, 
			IProjectionProvider projectionProvider, IUserCulture userCulture, IUserTimeZone userTimeZone, IPersonNameProvider personNameProvider, 
			ILoggedOnUser loggedOnUser)
		{
			_timelineViewModelFactory = timelineViewModelFactory;
			_projectionProvider = projectionProvider;
			_userCulture = userCulture;
			_userTimeZone = userTimeZone;
			_personNameProvider = personNameProvider;
			_loggedOnUser = loggedOnUser;
		}

		public ShiftTradeSwapDetailsViewModel Map(IShiftTradeSwapDetail s, NameFormatSettings nameFormatSettings)
		{
			return new ShiftTradeSwapDetailsViewModel
			{
				PersonFrom = s.PersonFrom==null ? null : _personNameProvider.BuildNameFromSetting(s.PersonFrom.Name,nameFormatSettings),
				PersonTo = s.PersonTo==null ? null : _personNameProvider.BuildNameFromSetting(s.PersonTo.Name, nameFormatSettings),
				To = Map(s.SchedulePartTo),
				From = Map(s.SchedulePartFrom),
				TimeLineHours = _timelineViewModelFactory.CreateTimeLineHours(createTimelinePeriod(s)),
				TimeLineStartDateTime = createTimelinePeriod(s).StartDateTime,
				Date = DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.DateFrom.Date, TimeZoneInfo.Utc)
			};
		}

		public ShiftTradeEditPersonScheduleViewModel Map(IScheduleDay o)
		{
			if (o == null) return new ShiftTradeEditPersonScheduleViewModel();

			var myScheduleDay = createPersonDay(o.Person, o);
			var timeZoneInfo = _userTimeZone.TimeZone();
			var timeLineRangeTot = setTimeLineRange(o.DateOnlyAsPeriod.DateOnly, myScheduleDay.ScheduleLayers,
				new List<ShiftTradePersonDayData>(), timeZoneInfo);
			var myScheduleViewModel = new ShiftTradeEditPersonScheduleViewModel
			{
				Name = _personNameProvider.BuildNameFromSetting(o.Person.Name),
				ScheduleLayers = createShiftTradeLayers(myScheduleDay, timeZoneInfo, timeLineRangeTot, o.Person),
				HasUnderlyingDayOff = myScheduleDay.SignificantPartForDisplay == SchedulePartView.ContractDayOff,
				DayOffText = myScheduleDay.DayOffText,
				StartTimeUtc = myScheduleDay.ScheduleLayers != null && myScheduleDay.ScheduleLayers.Any()
					? myScheduleDay.ScheduleLayers.First()
						.Period.StartDateTime
					: timeLineRangeTot.StartDateTime.Add(TimeSpan.FromMinutes(timeLineOffset)),
			};
			
			myScheduleViewModel.MinutesSinceTimeLineStart =
				(int) myScheduleViewModel.StartTimeUtc.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes;

			return myScheduleViewModel;
		}

		private DateTimePeriod createTimelinePeriod(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			var schedpartFrom = shiftTradeSwapDetail.SchedulePartFrom;
			var schedpartTo = shiftTradeSwapDetail.SchedulePartTo;
			if (schedpartFrom == null || schedpartTo == null)
			{
				return ((IShiftTradeRequest)shiftTradeSwapDetail.Parent).Period;
			}
			const int extraHourBeforeAndAfter = 1;
			DateTimePeriod totalPeriod;
			var fromTotalPeriod = _projectionProvider.Projection(schedpartFrom).Period();
			var toTotalPeriod = _projectionProvider.Projection(schedpartTo).Period();
			var schedulePartViewFrom = schedpartFrom.SignificantPart();
			var schedulePartViewTo = schedpartTo.SignificantPart();
			if (fromTotalPeriod.HasValue && toTotalPeriod.HasValue)
			{
				totalPeriod = fromTotalPeriod.Value.MaximumPeriod(toTotalPeriod.Value);
			}
			else if (fromTotalPeriod.HasValue)
			{
				if (schedulePartViewTo == SchedulePartView.DayOff)
				{
					totalPeriod = (DateTimePeriod)fromTotalPeriod;
				}
				else
				{
					totalPeriod = getTotalPeriod(((IShiftTradeRequest) shiftTradeSwapDetail.Parent).Period, fromTotalPeriod.Value,
						schedpartTo);
				}
			}
			else if (toTotalPeriod.HasValue)
			{
				if (schedulePartViewFrom == SchedulePartView.DayOff)
				{
					totalPeriod = (DateTimePeriod)toTotalPeriod;
				}
				else
				{
					totalPeriod = getTotalPeriod(((IShiftTradeRequest) shiftTradeSwapDetail.Parent).Period, toTotalPeriod.Value,
						schedpartFrom);
				}
			}
			else
			{
				totalPeriod = ((IShiftTradeRequest)shiftTradeSwapDetail.Parent).Period;
			}

			if (schedulePartViewFrom == SchedulePartView.DayOff &&
				schedulePartViewTo == SchedulePartView.DayOff)
			{
				totalPeriod = _loggedOnUser.CurrentUser().Equals(schedpartFrom.Person) ? schedpartFrom.Period : schedpartTo.Period;
				totalPeriod = new DateTimePeriod(totalPeriod.StartDateTime.AddHours(9), totalPeriod.EndDateTime.AddHours(-7));
			}
			return new DateTimePeriod(totalPeriod.StartDateTime.AddHours(-extraHourBeforeAndAfter), 
				totalPeriod.EndDateTime.AddHours(extraHourBeforeAndAfter));
		}

		private static DateTimePeriod getTotalPeriod(DateTimePeriod defaultPeriod, DateTimePeriod period, IScheduleDay schedpart)
		{
			var totalPeriod = period;
			var significantPart = schedpart.SignificantPart();
			if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.ContractDayOff)
			{
				totalPeriod = defaultPeriod.MaximumPeriod(totalPeriod);
			}
			return totalPeriod;
		}

		private static string dayOffText(IScheduleDay scheduleDay)
		{
			return scheduleDay.SignificantPartForDisplay() == SchedulePartView.DayOff ?
				scheduleDay.PersonAssignment().DayOff().Description.Name :
				string.Empty;
		}

		private static DateTimePeriod setTimeLineRange(DateOnly theDay, IEnumerable<IVisualLayer> myLayerCollection, IEnumerable<ShiftTradePersonDayData> possibleTradePersonDayCollection, TimeZoneInfo timeZone)
		{
			//IVisualLayerCollection has "Period" - use that instead of looping twice to get period?
			DateTimePeriod? timeLineRangeTot = null;

			if (myLayerCollection != null && myLayerCollection.Any())
			{
				timeLineRangeTot = new DateTimePeriod(myLayerCollection.Min(l => l.Period.StartDateTime), myLayerCollection.Max(l => l.Period.EndDateTime));
			}

			foreach (var personDay in possibleTradePersonDayCollection)
			{
				if (personDay.ScheduleLayers != null && personDay.ScheduleLayers.Any())
				{
					var timeRangeTemp = new DateTimePeriod(personDay.ScheduleLayers.Min(l => l.Period.StartDateTime),
						personDay.ScheduleLayers.Max(l => l.Period.EndDateTime));
					timeLineRangeTot = timeLineRangeTot?.MaximumPeriod(timeRangeTemp) ?? timeRangeTemp;
				}
			}

			if (!timeLineRangeTot.HasValue)
			{
				timeLineRangeTot = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(theDay.Date.AddHours(8), theDay.Date.AddHours(17), timeZone);
			}

			timeLineRangeTot = timeLineRangeTot.Value.ChangeStartTime(TimeSpan.FromMinutes(-timeLineOffset));
			timeLineRangeTot = timeLineRangeTot.Value.ChangeEndTime(TimeSpan.FromMinutes(timeLineOffset));

			return timeLineRangeTot.Value;
		}

		private ShiftTradePersonDayData createPersonDay(IPerson person, IScheduleDay scheduleDay)
		{
			var returnDay = new ShiftTradePersonDayData
			{
				PersonId = person.Id.HasValue ? person.Id.Value : Guid.Empty,
				Name = _personNameProvider.BuildNameFromSetting(person.Name),
				PersonTimeZone = person.PermissionInformation.DefaultTimeZone(),
				PersonCulture = person.PermissionInformation.Culture()
			};
			if (scheduleDay != null)
			{
				var layerCollection = _projectionProvider.Projection(scheduleDay);
				returnDay.ScheduleLayers = layerCollection == null ? null : layerCollection.ToList();
				returnDay.DayOffText = dayOffText(scheduleDay);
				returnDay.SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay();				
			}
			else
			{
				returnDay.ScheduleLayers = new List<IVisualLayer>();
			}

			return returnDay;
		}

		private IEnumerable<ShiftTradeEditScheduleLayerViewModel> createShiftTradeLayers(ShiftTradePersonDayData personDay, TimeZoneInfo timeZone, DateTimePeriod timeLineRange, IPerson person)
		{
			if (personDay.SignificantPartForDisplay == SchedulePartView.DayOff)
				return createShiftTradeDayOffLayer(timeLineRange, timeZone);

			if (personDay.ScheduleLayers == null || !personDay.ScheduleLayers.Any())
				return new ShiftTradeEditScheduleLayerViewModel[] { };

			var shiftStartTime = personDay.ScheduleLayers.Min(o => o.Period.StartDateTime);

			var scheduleLayers = (from visualLayer in personDay.ScheduleLayers
				let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
				let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
				let length = visualLayer.Period.ElapsedTime().TotalMinutes
				select new ShiftTradeEditScheduleLayerViewModel
				{
					Payload = visualLayer.Payload.ConfidentialDescription_DONTUSE(person).Name,
					LengthInMinutes = (int)length,
					Color = ColorTranslator.ToHtml(visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person)),
					TitleTime = createTitle(startDate, endDate),
					ElapsedMinutesSinceShiftStart = (int)startDate.Subtract(TimeZoneHelper.ConvertFromUtc(shiftStartTime, timeZone)).TotalMinutes
				}).ToList();
			return scheduleLayers;
		}

		private string createTitle(DateTime start, DateTime end)
		{
			//make a component for this?
			var userCulture = _userCulture.GetCulture();
			return string.Concat(start.ToString(userCulture.DateTimeFormat.ShortTimePattern, userCulture), "-",
				end.ToString(userCulture.DateTimeFormat.ShortTimePattern, userCulture));
		}

		private static IEnumerable<ShiftTradeEditScheduleLayerViewModel> createShiftTradeDayOffLayer(DateTimePeriod timeLineRange, TimeZoneInfo timeZone)
		{
			return new[]
			{
				new ShiftTradeEditScheduleLayerViewModel
				{
					ElapsedMinutesSinceShiftStart = 0,
					LengthInMinutes = (int) (timeLineRange.TimePeriod(timeZone).SpanningTime().TotalMinutes - (timeLineOffset * 2)),
					Color = string.Empty
				}
			};
		}
	}
}