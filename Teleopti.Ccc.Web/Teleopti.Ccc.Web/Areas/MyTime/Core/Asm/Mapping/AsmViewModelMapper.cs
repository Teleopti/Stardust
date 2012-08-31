﻿using System;
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

		public AsmViewModelMapper(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
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
					earliest = scheduleDay.DateOnlyAsPeriod.DateOnly;
				}
			}

			var ret = new AsmViewModel{StartDate = earliest};
			foreach (var visualLayer in layers)
			{
				ret.Layers.Add(new AsmLayer
				               	{
				               		Payload = visualLayer.DisplayDescription().Name,
											StartJavascriptBaseDate = visualLayer.Period.StartDateTime.SubtractJavascriptBaseDate().TotalMilliseconds,
											EndJavascriptBaseDate = visualLayer.Period.EndDateTime.SubtractJavascriptBaseDate().TotalMilliseconds,
											Color = ColorTranslator.ToHtml(visualLayer.DisplayColor())
				               	});
			}
			return ret;
		}
	}
}