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
									MinMax<DateTime> layersTimeRange;
									var myScheduleLayersViewModel = CreateShiftTradeLayers(_projectionProvider.Invoke().Projection(myScheduledDay), 
																						myScheduledDay.Person.PermissionInformation.DefaultTimeZone(),
																						out layersTimeRange);
									var earliestShiftStart = layersTimeRange.Minimum;
									var latestShiftEnd = layersTimeRange.Maximum;

									var possibleTradePersonsSchedule = _shiftTradeRequestProvider.Invoke().RetrievePossibleTradePersonsScheduleDay(s);

									var possibleTradePersonsViewModel = new List<ShiftTradePersonViewModel>();
									foreach (IScheduleDay scheduleDay in possibleTradePersonsSchedule)
									{
										possibleTradePersonsViewModel.Add(new ShiftTradePersonViewModel
										{
											Name = scheduleDay.Person.Name.ToString(),
											ScheduleLayers = CreateShiftTradeLayers(_projectionProvider.Invoke().Projection(scheduleDay),
																	scheduleDay.Person.PermissionInformation.DefaultTimeZone(),
																	out layersTimeRange)
										});
										if (layersTimeRange.Minimum < earliestShiftStart)
											earliestShiftStart = layersTimeRange.Minimum;
										if (layersTimeRange.Maximum > latestShiftEnd)
											latestShiftEnd = layersTimeRange.Maximum;
									}
									
									return new ShiftTradeRequestsScheduleViewModel
											{
												MyScheduleLayers = myScheduleLayersViewModel,
												PossibleTradePersons = possibleTradePersonsViewModel,
												TimeLineHours = CreateTimeLineHours(earliestShiftStart, latestShiftEnd, 
																	myScheduledDay.Person.PermissionInformation.DefaultTimeZone(), 
																	myScheduledDay.Person.PermissionInformation.Culture()),
												TimeLineLengthInMinutes = (int) latestShiftEnd.AddMinutes(15).Subtract(earliestShiftStart.AddMinutes(-15)).TotalMinutes
											};
								})
			;
		}

		private IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTime earliestShiftStart, DateTime latestShiftEnd, TimeZoneInfo timeZone, CultureInfo culture)
		{
			var hourList = new List<ShiftTradeTimeLineHoursViewModel>();
			ShiftTradeTimeLineHoursViewModel lastHour = null;

			earliestShiftStart = earliestShiftStart.AddMinutes(-15);
			latestShiftEnd = latestShiftEnd.AddMinutes(15);
			if (earliestShiftStart.Minute != 0)
			{
				hourList.Add(new ShiftTradeTimeLineHoursViewModel { HourText = string.Empty, MinutesToDisplay = 60 - earliestShiftStart.Minute });
				earliestShiftStart = earliestShiftStart.AddMinutes(60 - earliestShiftStart.Minute);
			}
			if (latestShiftEnd.Minute != 0)
			{
				lastHour = new ShiftTradeTimeLineHoursViewModel { HourText = string.Empty, MinutesToDisplay = latestShiftEnd.Minute };
				latestShiftEnd = latestShiftEnd.AddMinutes(-latestShiftEnd.Minute);
			}

			for (DateTime time = earliestShiftStart; time < latestShiftEnd; time = time.AddHours(1))
			{
				var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
				var hourString = string.Format(culture, localTime.ToShortTimeString());

				const string regex = "(\\:.*\\ )";
				var output = Regex.Replace(hourString, regex, " ");
				if (output.Contains(":"))
					output = localTime.Hour.ToString();

				hourList.Add(new ShiftTradeTimeLineHoursViewModel { HourText = output, MinutesToDisplay = 60 });
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}

		private static IEnumerable<ShiftTradeScheduleLayerViewModel> CreateShiftTradeLayers(IEnumerable<IVisualLayer> layers, TimeZoneInfo timeZone, out MinMax<DateTime> layersTimeRange)
		{
			if (layers == null || !layers.Any())
			{
				layersTimeRange = new MinMax<DateTime>();
				return new ShiftTradeScheduleLayerViewModel[] { };
			}

			DateTime shiftStartTime = layers.Min(o => o.Period.StartDateTime);
			layersTimeRange = new MinMax<DateTime>(shiftStartTime, layers.Max(o => o.Period.EndDateTime));

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