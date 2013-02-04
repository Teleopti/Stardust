using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMappingProfile : Profile
	{
		private readonly Func<IShiftTradeRequestProvider> _shiftTradeRequestProvider;
		private readonly Func<IProjectionProvider> _projectionProvider;
		private const int TimeLineOffset = 15;

		public ShiftTradeScheduleViewModelMappingProfile(Func<IShiftTradeRequestProvider> shiftTradeRequestProvider, Func<IProjectionProvider> projectionProvider)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			CreateMap<DateOnly, ShiftTradeScheduleViewModel>()
				.ConvertUsing(s =>
								{
									var myScheduleDay = createPersonDay(_shiftTradeRequestProvider.Invoke().RetrieveMyScheduledDay(s));

									var possibleTradePersonsSchedule = _shiftTradeRequestProvider.Invoke().RetrievePossibleTradePersonsScheduleDay(s);
									var possibleTradePersonDayCollection = createPossibleTradePersonsDayCollection(possibleTradePersonsSchedule);

									var timeLineRangeTot = setTimeLineRange(myScheduleDay.ScheduleLayers, possibleTradePersonDayCollection, myScheduleDay.PersonTimeZone);

									var myScheduleViewModel = new ShiftTradePersonScheduleViewModel
																{
																	Name = UserTexts.Resources.MySchedule,
																	ScheduleLayers = createShiftTradeLayers(myScheduleDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
																	MinutesSinceTimeLineStart = myScheduleDay.ScheduleLayers.Any() ? (int)myScheduleDay.ScheduleLayers.First().Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes : TimeLineOffset,
																	DayOffText = myScheduleDay.DayOffText
																};

									var possibleTradePersonViewModelCollection = new List<ShiftTradePersonScheduleViewModel>();
									possibleTradePersonViewModelCollection.AddRange(possibleTradePersonDayCollection
																.Select(personDay => new ShiftTradePersonScheduleViewModel
																										{
																											Name = personDay.Name,
																											ScheduleLayers = createShiftTradeLayers(personDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
																											MinutesSinceTimeLineStart = personDay.ScheduleLayers.Any() ? (int)personDay.ScheduleLayers.First().Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes : TimeLineOffset,
																											DayOffText = personDay.DayOffText
																										}));
									return new ShiftTradeScheduleViewModel
											{
												MySchedule = myScheduleViewModel,
												PossibleTradePersons = possibleTradePersonViewModelCollection,
												TimeLineHours = createTimeLineHours(timeLineRangeTot, myScheduleDay.PersonTimeZone, myScheduleDay.PersonCulture),
												TimeLineLengthInMinutes = (int)timeLineRangeTot.EndDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes
											};
								})
			;
		}

		private static string dayOffText(IScheduleDay scheduleDay)
		{
			return scheduleDay.SignificantPartForDisplay() == SchedulePartView.DayOff ? 
				scheduleDay.PersonDayOffCollection().First().DayOff.Description.Name : 
				string.Empty;
		}

		private static DateTimePeriod setTimeLineRange(IEnumerable<IVisualLayer> myLayerCollection, IEnumerable<ShiftTradePersonDayData> possibleTradePersonDayCollection, TimeZoneInfo timeZone)
		{
			DateTimePeriod? timeLineRangeTot = null;

			if (myLayerCollection.Any())
			{
				timeLineRangeTot = new DateTimePeriod(myLayerCollection.Min(l => l.Period.StartDateTime), myLayerCollection.Max(l => l.Period.EndDateTime));
			}

			foreach (var personDay in possibleTradePersonDayCollection)
			{
				if (personDay.ScheduleLayers.Any())
				{
					var timeRangeTemp = new DateTimePeriod(personDay.ScheduleLayers.Min(l => l.Period.StartDateTime),
															personDay.ScheduleLayers.Max(l => l.Period.EndDateTime));
					timeLineRangeTot = timeLineRangeTot.HasValue ? 
						timeLineRangeTot.Value.MaximumPeriod(timeRangeTemp) : 
						timeRangeTemp;
				}
			}

			if (!timeLineRangeTot.HasValue)
			{
				timeLineRangeTot = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(DateTime.Now.Date.AddHours(8), DateTime.Now.Date.AddHours(17), timeZone);
			}

			timeLineRangeTot = timeLineRangeTot.Value.ChangeStartTime(TimeSpan.FromMinutes(-TimeLineOffset));
			timeLineRangeTot = timeLineRangeTot.Value.ChangeEndTime(TimeSpan.FromMinutes(TimeLineOffset));

			return timeLineRangeTot.Value;
		}

		private IEnumerable<ShiftTradePersonDayData> createPossibleTradePersonsDayCollection(IEnumerable<IScheduleDay> possibleTradePersonsSchedule)
		{
			return possibleTradePersonsSchedule.Select(createPersonDay).ToList();
		}

		private ShiftTradePersonDayData createPersonDay(IScheduleDay scheduleDay)
		{
			var returnDay = new ShiftTradePersonDayData
									{
										Name = scheduleDay.Person.Name.ToString(),
										PersonTimeZone = scheduleDay.Person.PermissionInformation.DefaultTimeZone(),
										PersonCulture = scheduleDay.Person.PermissionInformation.Culture()
									};

			var layerCollection = _projectionProvider.Invoke().Projection(scheduleDay);
			returnDay.ScheduleLayers = layerCollection.ToList();
			returnDay.DayOffText = dayOffText(scheduleDay);
			returnDay.SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay();

			return returnDay;
		}

		private IEnumerable<ShiftTradeTimeLineHoursViewModel> createTimeLineHours(DateTimePeriod timeLinePeriod, TimeZoneInfo timeZone, CultureInfo culture)
		{
			var hourList = new List<ShiftTradeTimeLineHoursViewModel>();
			ShiftTradeTimeLineHoursViewModel lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				int lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
								{
									HourText = string.Empty,
									LengthInMinutesToDisplay = lengthInMinutes
								});
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				lastHour = new ShiftTradeTimeLineHoursViewModel
							{
								HourText = createHourText(timeLinePeriod.EndDateTime, timeZone, culture),
								LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute
							};
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
			}

			for (DateTime time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
								{
									HourText = createHourText(time, timeZone, culture),
									LengthInMinutesToDisplay = 60
								});
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

		private static string createHourText(DateTime time, TimeZoneInfo timeZone, CultureInfo culture)
		{
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			var hourString = string.Format(culture, localTime.ToShortTimeString());

			const string regex = "(\\:.*\\ )";
			var output = Regex.Replace(hourString, regex, " ");
			if (output.Contains(":"))
				output = localTime.Hour.ToString();

			return output;
		}

		private IEnumerable<ShiftTradeScheduleLayerViewModel> createShiftTradeLayers(ShiftTradePersonDayData personDay, TimeZoneInfo timeZone, DateTimePeriod timeLineRange)
		{
			if (personDay.SignificantPartForDisplay == SchedulePartView.DayOff)
				return CreateShiftTradeDayOffLayer(timeLineRange, timeZone);

			if (personDay.ScheduleLayers == null || !personDay.ScheduleLayers.Any())
				return new ShiftTradeScheduleLayerViewModel[] { };

			var shiftStartTime = personDay.ScheduleLayers.Min(o => o.Period.StartDateTime);

			var scheduleLayers = (from visualLayer in personDay.ScheduleLayers
										 let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
										 let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
										 let length = visualLayer.Period.ElapsedTime().TotalMinutes
										 select new ShiftTradeScheduleLayerViewModel
										 {
											 Payload = visualLayer.DisplayDescription().Name,
											 LengthInMinutes = (int)length,
											 Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
											 StartTimeText = startDate.ToString("HH:mm"),
											 EndTimeText = endDate.ToString("HH:mm"),
											 ElapsedMinutesSinceShiftStart = (int)startDate.Subtract(TimeZoneHelper.ConvertFromUtc(shiftStartTime, timeZone)).TotalMinutes
										 }).ToList();
			return scheduleLayers;
		}

		private IEnumerable<ShiftTradeScheduleLayerViewModel> CreateShiftTradeDayOffLayer(DateTimePeriod timeLineRange, TimeZoneInfo timeZone)
		{
			return new[]
			       	{
			       		new ShiftTradeScheduleLayerViewModel
			       			{
			       				ElapsedMinutesSinceShiftStart = 0,
			       				LengthInMinutes = (int) (timeLineRange.TimePeriod(timeZone).SpanningTime().TotalMinutes - (TimeLineOffset * 2)),
								Color = string.Empty
			       			}
			       	};
		}
	}

	internal class ShiftTradePersonDayData
	{
		public string Name { get; set; }

		public IEnumerable<IVisualLayer> ScheduleLayers { get; set; }

		public string DayOffText { get; set; }

		public TimeZoneInfo PersonTimeZone { get; set; }

		public CultureInfo PersonCulture { get; set; }

		public SchedulePartView SignificantPartForDisplay { get; set; }
	}
}