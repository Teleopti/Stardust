using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMappingProfile : Profile
	{
		private readonly Func<IShiftTradeRequestProvider> _shiftTradeRequestProvider;
		private readonly Func<IProjectionProvider> _projectionProvider;
		private readonly IResolve<IShiftTradeTimeLineHoursViewModelFactory> _shiftTradeTimelineHoursViewModelFactory;
		private const int TimeLineOffset = 15;

		public ShiftTradeScheduleViewModelMappingProfile(Func<IShiftTradeRequestProvider> shiftTradeRequestProvider, Func<IProjectionProvider> projectionProvider, IResolve<IShiftTradeTimeLineHoursViewModelFactory> shiftTradeTimelineHoursViewModelFactory)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_projectionProvider = projectionProvider;
			_shiftTradeTimelineHoursViewModelFactory = shiftTradeTimelineHoursViewModelFactory;
		}

		protected override void Configure()
		{
			CreateMap<IScheduleDay, ShiftTradePersonScheduleViewModel>()
				.ConvertUsing(o =>
					                {
											 var myScheduleDay = createPersonDay(o);
											 var timeLineRangeTot = setTimeLineRange(myScheduleDay.ScheduleLayers, new List<ShiftTradePersonDayData>(), myScheduleDay.PersonTimeZone);
											 var myScheduleViewModel = new ShiftTradePersonScheduleViewModel
											 {
												 Name = o.Person.Name.ToString(),
												 ScheduleLayers = createShiftTradeLayers(myScheduleDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
												 MinutesSinceTimeLineStart =
													 myScheduleDay.ScheduleLayers.Any() ?
														 (int)myScheduleDay.ScheduleLayers.First()
																			 .Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime)
																			 .TotalMinutes :
														 TimeLineOffset,
												 DayOffText = myScheduleDay.DayOffText,
												 HasUnderlyingDayOff = myScheduleDay.SignificantPartForDisplay == SchedulePartView.ContractDayOff,
											 };
											 return myScheduleViewModel;
					                });
				
			
			CreateMap<DateOnly, ShiftTradeScheduleViewModel>()
				.ConvertUsing(s =>
					{
						
						var myScheduleDay = createPersonDay(_shiftTradeRequestProvider.Invoke().RetrieveMyScheduledDay(s));

						var possibleTradePersonsSchedule = _shiftTradeRequestProvider.Invoke().RetrievePossibleTradePersonsScheduleDay(s);
						var possibleTradePersonDayCollection = createPossibleTradePersonsDayCollection(possibleTradePersonsSchedule);

						var timeLineRangeTot = setTimeLineRange(myScheduleDay.ScheduleLayers, possibleTradePersonDayCollection,
																			 myScheduleDay.PersonTimeZone);

						var myScheduleViewModel = new ShiftTradePersonScheduleViewModel
							{
								Name = UserTexts.Resources.MySchedule,
								ScheduleLayers = createShiftTradeLayers(myScheduleDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
								MinutesSinceTimeLineStart =
									myScheduleDay.ScheduleLayers.Any() ? 
										(int) myScheduleDay.ScheduleLayers.First()
															.Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime)
															.TotalMinutes : 
										TimeLineOffset,
								DayOffText = myScheduleDay.DayOffText,
								HasUnderlyingDayOff = myScheduleDay.SignificantPartForDisplay == SchedulePartView.ContractDayOff,
							};

						var possibleTradePersonViewModelCollection = new List<ShiftTradePersonScheduleViewModel>();
						possibleTradePersonViewModelCollection.AddRange(possibleTradePersonDayCollection
																							.Select(personDay => new ShiftTradePersonScheduleViewModel
																								{
																									Name = personDay.Name,
																									ScheduleLayers = createShiftTradeLayers(personDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
																									MinutesSinceTimeLineStart =
																										personDay.ScheduleLayers.Any() ?
																											(int)personDay.ScheduleLayers.First()
																												.Period.StartDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes : 
																											TimeLineOffset,
																									DayOffText = personDay.DayOffText,
																									HasUnderlyingDayOff = personDay.SignificantPartForDisplay == SchedulePartView.ContractDayOff
																								}));
						return new ShiftTradeScheduleViewModel
							{
								MySchedule = myScheduleViewModel,
								PossibleTradePersons = possibleTradePersonViewModelCollection,
								TimeLineHours = CreateTimeLineHours(timeLineRangeTot),
								TimeLineLengthInMinutes = (int)timeLineRangeTot.EndDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes
							};
					});
		}

		private static string dayOffText(IScheduleDay scheduleDay)
		{
			return scheduleDay.SignificantPartForDisplay() == SchedulePartView.DayOff ?
				scheduleDay.PersonDayOffCollection().First().DayOff.Description.Name :
				string.Empty;
		}

		private static DateTimePeriod setTimeLineRange(IEnumerable<IVisualLayer> myLayerCollection, IEnumerable<ShiftTradePersonDayData> possibleTradePersonDayCollection, TimeZoneInfo timeZone)
		{
			//IVisualLayerCollection has "Period" - use that instead of looping twice to get period?
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

		private  IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			return _shiftTradeTimelineHoursViewModelFactory.Invoke().CreateTimeLineHours(timeLinePeriod);
		}


		private static IEnumerable<ShiftTradeScheduleLayerViewModel> createShiftTradeLayers(ShiftTradePersonDayData personDay, TimeZoneInfo timeZone, DateTimePeriod timeLineRange)
		{
			if (personDay.SignificantPartForDisplay == SchedulePartView.DayOff)
				return createShiftTradeDayOffLayer(timeLineRange, timeZone);

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
											 Title = string.Concat(startDate.ToString("HH:mm"), " - ", endDate.ToString("HH:mm")),
											 ElapsedMinutesSinceShiftStart = (int)startDate.Subtract(TimeZoneHelper.ConvertFromUtc(shiftStartTime, timeZone)).TotalMinutes
										 }).ToList();
			return scheduleLayers;
		}

		private static IEnumerable<ShiftTradeScheduleLayerViewModel> createShiftTradeDayOffLayer(DateTimePeriod timeLineRange, TimeZoneInfo timeZone)
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