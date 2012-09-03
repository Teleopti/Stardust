using System;
using System.Collections.Generic;
using System.Drawing;
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
			DateTime earliest = DateTime.MaxValue;
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
			var ret = new AsmViewModel { StartDate = TimeZoneHelper.ConvertFromUtc(earliest, timeZone)};
			foreach (var visualLayer in layers)
			{
				var startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone);
				var endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone);
				               	
				ret.Layers.Add(new AsmLayer
				               	{
											Payload = visualLayer.DisplayDescription().Name,
											StartJavascriptBaseDate = startDate.SubtractJavascriptBaseDate().TotalMilliseconds,
											EndJavascriptBaseDate = endDate.SubtractJavascriptBaseDate().TotalMilliseconds,
											Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
											StartTimeText = startDate.ToString("HH:mm"),
											EndTimeText = endDate.ToString("HH:mm")
				               	});
			}
			return ret;
		}
	}
}