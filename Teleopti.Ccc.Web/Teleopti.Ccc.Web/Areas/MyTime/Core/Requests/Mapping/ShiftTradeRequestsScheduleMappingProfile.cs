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
									IEnumerable<IVisualLayer> myLayerCollection = _projectionProvider.Invoke().Projection(myScheduledDay);

									var possibleTradePersonsSchedule = _shiftTradeRequestProvider.Invoke().RetrievePossibleTradePersonsScheduleDay(s);
									IEnumerable<IEnumerable<IVisualLayer>> possibleTradePersonsLayerCollection = 
										CreatePossibleTradePersonsLayerCollection(possibleTradePersonsSchedule);

									var myScheduleViewModel = new ShiftTradeScheduleViewModel();
									var possibleTradePersonsViewModel = new List<ShiftTradeScheduleViewModel>();

									IEnumerable<ShiftTradeTimeLineHoursViewModel> timeLineHours;
									int timeLineLengthInMinutes = 0;
									if (myLayerCollection.Any() || possibleTradePersonsLayerCollection.Any())
									{
										DateTimePeriod timeLineRangeTot = SetTimeLineRange(myLayerCollection, possibleTradePersonsLayerCollection);
										var timeZone = myScheduledDay.Person.PermissionInformation.DefaultTimeZone();
										
										timeLineHours = CreateTimeLineHours(timeLineRangeTot,
																			timeZone,
																			myScheduledDay.Person.PermissionInformation.Culture());
										timeLineLengthInMinutes = (int)timeLineRangeTot.EndDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes;

										myScheduleViewModel = new ShiftTradeScheduleViewModel
																	{
																		Name = myScheduledDay.Person.Name.ToString(),
																		ScheduleLayers = CreateShiftTradeLayers(myLayerCollection, timeZone),
																		MinutesSinceTimeLineStart = myLayerCollection.Any() ? (int)myLayerCollection.First().Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes : 0
																	};
										possibleTradePersonsViewModel.AddRange(possibleTradePersonsLayerCollection
																		.Select(layers => new ShiftTradeScheduleViewModel
																								{
																									Name = layers.First().Person.Name.ToString(),
																									ScheduleLayers = CreateShiftTradeLayers(layers, timeZone), 
																									MinutesSinceTimeLineStart = (int) layers.First().Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes
																								}));
									}
									else
									{
										timeLineHours = Enumerable.Empty<ShiftTradeTimeLineHoursViewModel>();
									}
									return new ShiftTradeRequestsScheduleViewModel
											{
												MySchedule = myScheduleViewModel,
												PossibleTradePersons = possibleTradePersonsViewModel,
												TimeLineHours = timeLineHours,
												TimeLineLengthInMinutes = timeLineLengthInMinutes
											};
								})
			;
		}

		private DateTimePeriod SetTimeLineRange(IEnumerable<IVisualLayer> myLayerCollection, IEnumerable<IEnumerable<IVisualLayer>> possibleTradePersonsLayerCollection)
		{
			var timeLineRangeTot = new DateTimePeriod();

			if (myLayerCollection.Any())
			{
				timeLineRangeTot = new DateTimePeriod(myLayerCollection.Min(l => l.Period.StartDateTime), myLayerCollection.Max(l => l.Period.EndDateTime));
			}

			if (possibleTradePersonsLayerCollection.Any())
			{
				foreach (IEnumerable<IVisualLayer> visualLayers in possibleTradePersonsLayerCollection)
				{
					var timeRangeTemp = new DateTimePeriod(visualLayers.Min(l => l.Period.StartDateTime),
															visualLayers.Max(l => l.Period.EndDateTime));
					timeLineRangeTot = timeLineRangeTot.MaximumPeriod(timeRangeTemp);
				}
			}

			timeLineRangeTot = timeLineRangeTot.ChangeStartTime(TimeSpan.FromMinutes(-15));
			timeLineRangeTot = timeLineRangeTot.ChangeEndTime(TimeSpan.FromMinutes(15));

			return timeLineRangeTot;
		}

		private IEnumerable<IEnumerable<IVisualLayer>> CreatePossibleTradePersonsLayerCollection(IEnumerable<IScheduleDay> possibleTradePersonsSchedule)
		{
			var returnList = new List<List<IVisualLayer>>();

			foreach (var scheduleDay in possibleTradePersonsSchedule)
			{
				var layerCollection = _projectionProvider.Invoke().Projection(scheduleDay);
				if (layerCollection.Any())
				{
					returnList.Add(layerCollection.ToList());
				}
			}

			return returnList;
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

		private static IEnumerable<ShiftTradeScheduleLayerViewModel> CreateShiftTradeLayers(IEnumerable<IVisualLayer> layers, TimeZoneInfo timeZone)
		{
			if (layers == null || !layers.Any())
			{
				return new ShiftTradeScheduleLayerViewModel[] { };
			}

			DateTime shiftStartTime = layers.Min(o => o.Period.StartDateTime);

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