using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface IProjectionSplitter
	{
		IList<GroupScheduleProjectionViewModel> SplitMergedPersonalLayers(IScheduleDay scheduleDay, IVisualLayer layer,
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
			TimeZoneInfo userTimeZone)
		{
			var splittedVisualLayers = new List<GroupScheduleProjectionViewModel>();
			var unMergedCollection = _projectionProvider.UnmergedProjection(scheduleDay);
			var splittedLayers = unMergedCollection.Where(l => layer.Period.Contains((DateTimePeriod) l.Period)).ToList();

			splittedLayers.ForEach(l =>
			{
				splittedVisualLayers.Add(new GroupScheduleProjectionViewModel
				{
					ShiftLayerIds = new[] { _projectionHelper.GetMatchedShiftLayerIds(scheduleDay, l).Last() },
					Description = layer.DisplayDescription().Name,
					Color = layer.DisplayColor().ToHtml(),
					Start = TimeZoneInfo.ConvertTimeFromUtc(l.Period.StartDateTime, userTimeZone).ToFixedDateTimeFormat(),
					End = TimeZoneInfo.ConvertTimeFromUtc(l.Period.EndDateTime, userTimeZone).ToFixedDateTimeFormat(),
					Minutes = (int)l.Period.ElapsedTime().TotalMinutes
				});
			});
			return splittedVisualLayers;
		}
	}
}