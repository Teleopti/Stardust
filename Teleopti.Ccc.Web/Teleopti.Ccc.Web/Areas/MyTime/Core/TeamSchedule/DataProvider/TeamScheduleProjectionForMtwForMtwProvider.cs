using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamScheduleProjectionForMtwForMtwProvider : ITeamScheduleProjectionForMTWProvider
	{
		public const int AbsenceFullDayExtraDays = 5;
		public const int DayOffExtraDays = 10;
		public const int EmptyExtraDays = 20;

		private readonly IProjectionProvider _projectionProvider;

		public TeamScheduleProjectionForMtwForMtwProvider(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		public ITeamScheduleProjection Projection(IScheduleDay scheduleDay)
		{
			var projection = _projectionProvider.Projection(scheduleDay);
			if (projection != null && projection.HasLayers)
				return MakeProjection(projection, scheduleDay.Person);

			if (scheduleDay.HasDayOff())
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
			var dayOff = scheduleDay.PersonAssignment().DayOff();
			return new TeamScheduleProjection
			       	{
			       		Layers = new ITeamScheduleLayer[] {},
			       		DayOff = dayOff,
			       		SortDate = scheduleDay.Period.StartDateTime.AddDays(DayOffExtraDays)
			       	};
				       			}

		private static ITeamScheduleProjection MakeProjection(IVisualLayerCollection projection, IPerson person)
		{
			var layers = (from l in projection
			              select new TeamScheduleLayer
			                     	{
			                     		Period = l.Period,
			                     		DisplayColor = l.Payload.ConfidentialDisplayColor_DONTUSE(person),
										ActivityName = l.Payload.ConfidentialDescription_DONTUSE(person).Name
			                     	})
				.ToArray();
			var sortDate = isFullDayAbsence(projection)
			               	? projection.Period().Value.StartDateTime.AddDays(AbsenceFullDayExtraDays)
			               	: projection.Period().Value.StartDateTime;

			return new TeamScheduleProjection
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