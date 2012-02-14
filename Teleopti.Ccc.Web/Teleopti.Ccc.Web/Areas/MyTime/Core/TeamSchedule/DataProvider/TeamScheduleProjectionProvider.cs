﻿using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamScheduleProjectionProvider : ITeamScheduleProjectionProvider
	{
		public const int AbsenceFullDayExtraDays = 5;
		public const int DayOffExtraDays = 10;
		public const int EmptyExtraDays = 20;

		private readonly IProjectionProvider _projectionProvider;

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		public ITeamScheduleProjection Projection(IScheduleDay scheduleDay)
		{
			var projection = _projectionProvider.Projection(scheduleDay);
			if (projection != null && projection.HasLayers)
				return MakeProjection(projection);

			var dayOffs = scheduleDay.PersonDayOffCollection();

			var hasDayOffs = dayOffs != null && dayOffs.Count > 0;

			if (hasDayOffs)
				return MakeDayOffProjection(scheduleDay);

			return MakeEmptyProjection(scheduleDay);
		}

		private static ITeamScheduleProjection MakeEmptyProjection(IScheduleDay scheduleDay)
			{
			return new TeamScheduleProjection
				       	{
						Layers = new ITeamScheduleLayer[] { },
						SortDate = scheduleDay.Period.StartDateTime.AddDays(EmptyExtraDays)
			       	};
		}

		private static ITeamScheduleProjection MakeDayOffProjection(IScheduleDay scheduleDay)
				       			{
			var dayOff = scheduleDay.PersonDayOffCollection().First();
			return new TeamScheduleProjection()
			       	{
			       		Layers = new ITeamScheduleLayer[] {},
			       		DayOff = dayOff,
			       		SortDate = scheduleDay.Period.StartDateTime.AddDays(DayOffExtraDays)
			       	};
				       			}

		private static ITeamScheduleProjection MakeProjection(IVisualLayerCollection projection)
		{
			var layers = (from l in projection
			              select new TeamScheduleLayer
			                     	{
			                     		Period = l.Period,
			                     		DisplayColor = l.DisplayColor(),
										ActivityName = l.DisplayDescription().Name
			                     	})
				.ToArray();
			var sortDate = isFullDayAbsence(projection)
			               	? projection.Period().Value.StartDateTime.AddDays(AbsenceFullDayExtraDays)
			               	: projection.Period().Value.StartDateTime;

			return new TeamScheduleProjection()
			       	{
			       		Layers = layers,
			       		SortDate = sortDate
			       	};
		}

		private static bool isFullDayAbsence(IVisualLayerCollection projection)
		{
			return projection.All(layer => (layer.Payload is IAbsence));
		}
	}
}