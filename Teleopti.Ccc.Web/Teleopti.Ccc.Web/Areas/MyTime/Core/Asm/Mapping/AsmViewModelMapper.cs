using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping
{
	public class AsmViewModelMapper : IAsmViewModelMapper
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZoneInfo;

		public AsmViewModelMapper(IProjectionProvider projectionProvider, IUserTimeZone userTimeZoneInfo)
		{
			_projectionProvider = projectionProvider;
			_userTimeZoneInfo = userTimeZoneInfo;
		}

		public AsmViewModel Map(IEnumerable<IScheduleDay> scheduleDays)
		{
			var layers = new List<IVisualLayer>();
			var earliest = DateTime.MaxValue;
			foreach (var scheduleDay in scheduleDays)
			{
				var proj = _projectionProvider.Projection(scheduleDay);
				if (proj != null)
				{
					layers.AddRange(proj);					
				}
				if (scheduleDay.DateOnlyAsPeriod.DateOnly < earliest)
				{
					earliest = scheduleDay.DateOnlyAsPeriod.DateOnly.Date;
				}
			}
			var timeZone = _userTimeZoneInfo.TimeZone();
			return new AsmViewModel
			          	{
			          		StartDateTime = TimeZoneHelper.ConvertFromUtc(earliest, timeZone),
			          		Layers = createAsmLayers(timeZone, layers),
								Hours = createHours(earliest, timeZone)
			          	};
		}

		private static IEnumerable<string> createHours(DateTime start, ICccTimeZoneInfo timeZone)
		{
			const int numberOfHoursToShow = 24*3;
			var hoursAsInts = new List<int>();
			var localStart = timeZone.ConvertTimeFromUtc(start);
			
			for (var hour = 0; hour < numberOfHoursToShow; hour++)
			{
				var localTime = timeZone.ConvertTimeFromUtc(start.AddHours(hour));
				hoursAsInts.Add(localTime.Hour);
			}

			//hoursAsInts.AddRange(Enumerable.Range(localStartHour, 24 - localStartHour));
			//hoursAsInts.AddRange(Enumerable.Range(0, 24));
			//hoursAsInts.AddRange(Enumerable.Range(0, 24));
			//hoursAsInts.AddRange(Enumerable.Range(0, 24));

			return hoursAsInts.Take(numberOfHoursToShow).Select(x => x.ToString(CultureInfo.InvariantCulture));
		}

		private static IEnumerable<AsmLayer> createAsmLayers(ICccTimeZoneInfo timeZone, IEnumerable<IVisualLayer> layers)
		{
			var asmLayers = (from visualLayer in layers
			                 let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
			                 let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
			                 select new AsmLayer
			                        	{
			                        		Payload = visualLayer.DisplayDescription().Name,
			                        		StartJavascriptBaseDate = startDate.SubtractJavascriptBaseDate().TotalMilliseconds,
			                        		EndJavascriptBaseDate = endDate.SubtractJavascriptBaseDate().TotalMilliseconds,
			                        		Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
			                        		StartTimeText = startDate.ToString("HH:mm"),
			                        		EndTimeText = endDate.ToString("HH:mm")
			                        	}).ToList();
			return asmLayers;
		}
	}
}