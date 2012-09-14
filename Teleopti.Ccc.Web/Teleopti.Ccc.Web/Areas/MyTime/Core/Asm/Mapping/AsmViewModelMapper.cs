using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
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

		public AsmViewModel Map(DateTime asmZero, IEnumerable<IScheduleDay> scheduleDays)
		{
			var layers = new List<IVisualLayer>();
			foreach (var proj in scheduleDays.Select(scheduleDay => _projectionProvider.Projection(scheduleDay)))
			{
				layers.AddRange(proj);
			}
			var timeZone = _userTimeZoneInfo.TimeZone();
			return new AsmViewModel
			          	{
			          		Layers = createAsmLayers(asmZero, timeZone, layers),
								Hours = createHours(asmZero, timeZone)
			          	};
		}

		private static IEnumerable<string> createHours(DateTime asmZero, ICccTimeZoneInfo timeZone)
		{
			const int numberOfHoursToShow = 24*3;
			var hoursAsInts = new List<int>();
			var asmZeroAsUtc = new DateTime(asmZero.Ticks, DateTimeKind.Utc);
			
			for (var hour = 0; hour < numberOfHoursToShow; hour++)
			{
				var localTime = timeZone.ConvertTimeFromUtc(asmZeroAsUtc.AddHours(hour));
				hoursAsInts.Add(localTime.Hour);
			}

			return hoursAsInts.Take(numberOfHoursToShow).Select(x => x.ToString(CultureInfo.InvariantCulture));
		}

		private static IEnumerable<AsmLayer> createAsmLayers(DateTime asmZeroAsUtc, ICccTimeZoneInfo timeZone, IEnumerable<IVisualLayer> layers)
		{
			var asmLayers = (from visualLayer in layers
			                 let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
			                 let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
			                 let length = visualLayer.Period.ElapsedTime().TotalMinutes
			                 select new AsmLayer
			                        	{
			                        		Payload = visualLayer.DisplayDescription().Name,
													StartJavascriptBaseDate = startDate.Subtract(asmZeroAsUtc).TotalMinutes,//todo -rename
			                        		LengthInMinutes = length,
			                        		Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
			                        		StartTimeText = startDate.ToString("HH:mm"),
			                        		EndTimeText = endDate.ToString("HH:mm")
			                        	}).ToList();
			return asmLayers;
		}
	}
}