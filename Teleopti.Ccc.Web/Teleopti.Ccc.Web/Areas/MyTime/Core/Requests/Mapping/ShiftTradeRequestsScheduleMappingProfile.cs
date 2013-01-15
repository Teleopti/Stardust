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
	public class ShiftTradeRequestsScheduleMappingProfile : Profile
	{
		private readonly Func<IShiftTradeRequestProvider> _shiftTradeRequestProvider;
		private readonly Func<IProjectionProvider> _projectionProvider;

		public ShiftTradeRequestsScheduleMappingProfile(Func<IShiftTradeRequestProvider> shiftTradeRequestProvider, Func<IProjectionProvider> projectionProvider)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			base.Configure();


			CreateMap<DateOnly, ShiftTradeRequestsScheduleViewModel>()
				.ConvertUsing(s =>
								{
									var myScheduledDay = _shiftTradeRequestProvider.Invoke().RetrieveMyScheduledDay(s);

									DateTimePeriod timeRangeTot;

									IEnumerable<IVisualLayer> myLayerCollection = _projectionProvider.Invoke().Projection(myScheduledDay);
									var myScheduleLayersViewModel = CreateShiftTradeLayers(myLayerCollection, 
									                                                   myScheduledDay.Person.PermissionInformation.DefaultTimeZone(),
																					   out timeRangeTot);

									var possibleTradePersonsSchedule = _shiftTradeRequestProvider.Invoke().RetrievePossibleTradePersonsScheduleDay(s);

									var possibleTradePersonsViewModel = new List<ShiftTradePersonViewModel>();
									foreach (IScheduleDay scheduleDay in possibleTradePersonsSchedule)
									{
										DateTimePeriod timeRangeTemp;
										possibleTradePersonsViewModel.Add(new ShiftTradePersonViewModel
										{
											Name = scheduleDay.Person.Name.ToString(),
											ScheduleLayers = CreateShiftTradeLayers(_projectionProvider.Invoke().Projection(scheduleDay),
																	scheduleDay.Person.PermissionInformation.DefaultTimeZone(),
																	out timeRangeTemp)
										});
										timeRangeTot = timeRangeTot.MaximumPeriod(timeRangeTemp);
									}

									int minutesSinceTimeLineStart = 0;
									IEnumerable<ShiftTradeTimeLineHoursViewModel> timeLineHours;
									int timeLineLengthInMinutes = 0;
									if (myScheduleLayersViewModel.Any() || possibleTradePersonsViewModel.Any())
									{
										timeRangeTot = timeRangeTot.ChangeStartTime(TimeSpan.FromMinutes(-15));
										timeRangeTot = timeRangeTot.ChangeEndTime(TimeSpan.FromMinutes(15));
										timeLineHours = CreateTimeLineHours(timeRangeTot,
										                                    myScheduledDay.Person.PermissionInformation.DefaultTimeZone(),
										                                    myScheduledDay.Person.PermissionInformation.Culture());
										timeLineLengthInMinutes = (int)timeRangeTot.EndDateTime.Subtract(timeRangeTot.StartDateTime).TotalMinutes;
										minutesSinceTimeLineStart = (int)myLayerCollection.First().Period.StartDateTime.Subtract(timeRangeTot.StartDateTime).TotalMinutes;
									}
									else
									{
										timeLineHours = Enumerable.Empty<ShiftTradeTimeLineHoursViewModel>();
									}
									return new ShiftTradeRequestsScheduleViewModel
											{
												MySchedule = new ShiftTradeMyScheduleViewModel
												             	{
												             		ScheduleLayers = myScheduleLayersViewModel,
																	MinutesSinceTimeLineStart = minutesSinceTimeLineStart
												             	},
												PossibleTradePersons = possibleTradePersonsViewModel,
												TimeLineHours = timeLineHours,
												TimeLineLengthInMinutes = timeLineLengthInMinutes
											};
								})
			;
		}

		private IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod, TimeZoneInfo timeZone, CultureInfo culture)
		{
			var hourList = new List<ShiftTradeTimeLineHoursViewModel>();
			ShiftTradeTimeLineHoursViewModel lastHour = null;
			DateTime shiftStartRounded = timeLinePeriod.StartDateTime;
			DateTime shiftEndRounded = timeLinePeriod.EndDateTime;

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
								HourText = CreateHourText(timeLinePeriod.EndDateTime, timeZone, culture),
								LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute
				           	};
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
			}

			for (DateTime time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel
				             	{
									HourText = CreateHourText(time, timeZone, culture), 
									LengthInMinutesToDisplay = 60
				             	});
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

		private string CreateHourText(DateTime time, TimeZoneInfo timeZone, CultureInfo culture)
		{
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			var hourString = string.Format(culture, localTime.ToShortTimeString());

			const string regex = "(\\:.*\\ )";
			var output = Regex.Replace(hourString, regex, " ");
			if (output.Contains(":"))
				output = localTime.Hour.ToString();

			return output;
		}

		private static IEnumerable<ShiftTradeScheduleLayerViewModel> CreateShiftTradeLayers(IEnumerable<IVisualLayer> layers, TimeZoneInfo timeZone, out DateTimePeriod layersTimeRange)
		{
			if (layers == null || !layers.Any())
			{
				layersTimeRange = new DateTimePeriod();
				return new ShiftTradeScheduleLayerViewModel[] { };
			}

			DateTime shiftStartTime = layers.Min(o => o.Period.StartDateTime);
			layersTimeRange = new DateTimePeriod(shiftStartTime, layers.Max(o => o.Period.EndDateTime));

			var scheduleLayers = (from visualLayer in layers
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
	}
}