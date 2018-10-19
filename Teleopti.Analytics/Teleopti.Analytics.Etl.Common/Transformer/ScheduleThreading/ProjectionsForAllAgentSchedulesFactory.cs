using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public static class ProjectionsForAllAgentSchedulesFactory
	{
		public static IList<ScheduleProjection> CreateProjectionsForAllAgentSchedules(
			 IEnumerable<IScheduleDay> scheduleList)
		{
			return scheduleList.Select(schedule =>
			{
				IProjectionService projectionService = schedule.ProjectionService();
				return new ScheduleProjection(schedule, projectionService.CreateProjection());
			}).ToArray();
		}
	}
}