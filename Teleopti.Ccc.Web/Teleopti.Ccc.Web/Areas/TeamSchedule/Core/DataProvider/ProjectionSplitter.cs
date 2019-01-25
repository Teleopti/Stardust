using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core.Extensions;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface IProjectionSplitter
	{
		IList<GroupScheduleProjectionViewModel> SplitMergedPersonalLayers(IScheduleDay scheduleDay, IVisualLayer layer, PersonalShiftLayer[] matchedPersonalShiftLayers,
			TimeZoneInfo userTimeZone);
	}
	public class ProjectionSplitter : IProjectionSplitter
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IScheduleProjectionHelper _projectionHelper;

		public ProjectionSplitter(IProjectionProvider projectionProvider, IScheduleProjectionHelper projectionHelper)
		{
			_projectionProvider = projectionProvider;
			_projectionHelper = projectionHelper;
		}

		public IList<GroupScheduleProjectionViewModel> SplitMergedPersonalLayers(IScheduleDay scheduleDay, IVisualLayer layer,
			PersonalShiftLayer[] matchedPersonalShiftLayers,
			TimeZoneInfo userTimeZone)
		{
			var splittedVisualLayers = new List<GroupScheduleProjectionViewModel>();
			var unMergedCollection = _projectionProvider.UnmergedProjection(scheduleDay);
			var splittedLayers = unMergedCollection.Where(l => layer.Period.Contains(l.Period)).ToList();

			splittedLayers.ForEach(l =>
			{
				var matchShiftLayers = _projectionHelper.GetMatchedShiftLayers(scheduleDay, l, false);
				var personalLayersInPeriod = matchedPersonalShiftLayers.Where(pl => pl.Period.Contains(l.Period));
				splittedVisualLayers.Add(new GroupScheduleProjectionViewModel
				{
					ShiftLayerIds =
						personalLayersInPeriod.Any()
							? personalLayersInPeriod.Select(pl => pl.Id.GetValueOrDefault()).ToArray()
							: _projectionHelper.GetMatchedShiftLayerIds(scheduleDay, l)
								.Except(matchedPersonalShiftLayers.Select(pl => pl.Id.GetValueOrDefault()))
								.ToArray(),
					Description = layer.Payload.ConfidentialDescription_DONTUSE(scheduleDay.Person).Name,
					Color = layer.Payload.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person).ToHtml(),
					Start = TimeZoneInfo.ConvertTimeFromUtc(l.Period.StartDateTime, userTimeZone).ToServiceDateTimeFormat(),
					StartInUtc = l.Period.StartDateTime.ToServiceDateTimeFormat(),
					End = TimeZoneInfo.ConvertTimeFromUtc(l.Period.EndDateTime, userTimeZone).ToServiceDateTimeFormat(),
					EndInUtc = l.Period.EndDateTime.ToServiceDateTimeFormat(),
					Minutes = (int)l.Period.ElapsedTime().TotalMinutes,
					ActivityId = layer.Payload.Id.GetValueOrDefault(),
					TopShiftLayerId = _projectionHelper.GetTopShiftLayerId(matchShiftLayers, l),
					IsPersonalActivity = true,
					FloatOnTop = true
				});
			});
			return splittedVisualLayers;
		}
	}
}