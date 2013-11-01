using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMappingProfile : Profile
	{
		private readonly IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IShiftTradeTimeLineHoursViewModelFactory _shiftTradeTimelineHoursViewModelFactory;
		private readonly IUserCulture _userCulture;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private const int TimeLineOffset = 15;

		public ShiftTradeScheduleViewModelMappingProfile(IShiftTradeRequestProvider shiftTradeRequestProvider, 
																										IProjectionProvider projectionProvider, 
																										IShiftTradeTimeLineHoursViewModelFactory shiftTradeTimelineHoursViewModelFactory,
																										IUserCulture userCulture,
																										IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_projectionProvider = projectionProvider;
			_shiftTradeTimelineHoursViewModelFactory = shiftTradeTimelineHoursViewModelFactory;
			_userCulture = userCulture;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
		}

		protected override void Configure()
		{
			CreateMap<IScheduleDay, ShiftTradePersonScheduleViewModel>()
				.ConvertUsing(o =>
					                {
											
											 var myScheduleDay = createPersonDay(o.Person, o);
											 var timeLineRangeTot = setTimeLineRange(o.DateOnlyAsPeriod.DateOnly, myScheduleDay.ScheduleLayers, new List<ShiftTradePersonDayData>(), myScheduleDay.PersonTimeZone);
											 var myScheduleViewModel = new ShiftTradePersonScheduleViewModel
											 {
												 Name = o.Person.Name.ToString(),
												 ScheduleLayers = createShiftTradeLayers(myScheduleDay, myScheduleDay.PersonTimeZone, timeLineRangeTot),
												 HasUnderlyingDayOff = myScheduleDay.SignificantPartForDisplay == SchedulePartView.ContractDayOff,
												  DayOffText = myScheduleDay.DayOffText,
											 };

											 myScheduleViewModel.StartTimeUtc = myScheduleDay.ScheduleLayers.Any()
													 ? myScheduleDay.ScheduleLayers.First()
																				 .Period.StartDateTime
																				 : timeLineRangeTot.StartDateTime.Add(TimeSpan.FromMinutes(TimeLineOffset));

											 myScheduleViewModel.MinutesSinceTimeLineStart = (int) myScheduleViewModel.StartTimeUtc.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes;
												
											 return myScheduleViewModel;
					                });
				
			
			CreateMap<DateOnly, ShiftTradeScheduleViewModel>()
				.ConvertUsing(dateOnly =>
					{
						var myDomainScheduleDay = _shiftTradeRequestProvider.RetrieveMyScheduledDay(dateOnly);
						ShiftTradePersonDayData myScheduleDay = createPersonDay(myDomainScheduleDay.Person, myDomainScheduleDay);
						var possibleShiftTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(dateOnly);

						var possibleTradePersonsSchedule = _shiftTradeRequestProvider.RetrievePossibleTradePersonsScheduleDay(dateOnly, possibleShiftTradePersons);
						var possibleTradePersonDayCollection = createPossibleTradePersonsDayCollection(possibleShiftTradePersons, possibleTradePersonsSchedule);

						var timeLineRangeTot = setTimeLineRange(dateOnly, myScheduleDay.ScheduleLayers, possibleTradePersonDayCollection,
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
																									PersonId = personDay.PersonId,
																									Name = personDay.Name,
																									//ScheduleLayers = createShiftTradeLayers(personDay, personDay.PersonTimeZone, timeLineRangeTot),
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
								TimeLineHours = createTimeLineHours(timeLineRangeTot),
								TimeLineLengthInMinutes = (int)timeLineRangeTot.EndDateTime.Subtract(timeLineRangeTot.StartDateTime).TotalMinutes
							};
					});
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
				timeLineRangeTot = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(theDay.Date.AddHours(8), theDay.Date.AddHours(17), timeZone);
			}

			timeLineRangeTot = timeLineRangeTot.Value.ChangeStartTime(TimeSpan.FromMinutes(-TimeLineOffset));
			timeLineRangeTot = timeLineRangeTot.Value.ChangeEndTime(TimeSpan.FromMinutes(TimeLineOffset));

			return timeLineRangeTot.Value;
		}

		private IEnumerable<ShiftTradePersonDayData> createPossibleTradePersonsDayCollection(IEnumerable<IPerson> possibleShiftTradePersons, IEnumerable<IScheduleDay> possibleTradePersonsSchedule)
		{
			var ret = new List<ShiftTradePersonDayData>();
			if (possibleShiftTradePersons != null)
			{
				ret.AddRange(from tradePerson in possibleShiftTradePersons 
										 let scheduleDay = possibleTradePersonsSchedule.FirstOrDefault(day => day.Person.Equals(tradePerson)) 
										 select createPersonDay(tradePerson, scheduleDay));
			}
			return ret;
		}

		private ShiftTradePersonDayData createPersonDay(IPerson person, IScheduleDay scheduleDay)
		{
			var returnDay = new ShiftTradePersonDayData
									{
										PersonId = person.Id.HasValue ? person.Id.Value : Guid.Empty,
										Name = person.Name.ToString(),
										PersonTimeZone = person.PermissionInformation.DefaultTimeZone(),
										PersonCulture = person.PermissionInformation.Culture()
									};
			if (scheduleDay != null)
			{
				var layerCollection = _projectionProvider.Projection(scheduleDay);
				returnDay.ScheduleLayers = layerCollection.ToList();
				returnDay.DayOffText = dayOffText(scheduleDay);
				returnDay.SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay();				
			}
			else
			{
				returnDay.ScheduleLayers = new List<IVisualLayer>();
			}

			return returnDay;
		}

		private  IEnumerable<ShiftTradeTimeLineHoursViewModel> createTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			return _shiftTradeTimelineHoursViewModelFactory.CreateTimeLineHours(timeLinePeriod);
		}


		private IEnumerable<ShiftTradeScheduleLayerViewModel> createShiftTradeLayers(ShiftTradePersonDayData personDay, TimeZoneInfo timeZone, DateTimePeriod timeLineRange)
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
											 Title = createTitle(startDate, endDate),
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

		public Guid PersonId { get; set; }
	}
}