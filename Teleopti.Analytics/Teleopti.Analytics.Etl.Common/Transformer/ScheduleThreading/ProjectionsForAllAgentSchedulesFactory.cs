using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	static public class ProjectionsForAllAgentSchedulesFactory
	{
		public static IList<ScheduleProjection> CreateProjectionsForAllAgentSchedules(
			 IEnumerable<IScheduleDay> scheduleList)
		{
			IList<ScheduleProjection> retList = new List<ScheduleProjection>();

			foreach (IScheduleDay schedule in scheduleList)
			{
				IProjectionService projectionService = schedule.ProjectionService();
				var scheduleProjection = new ScheduleProjection(schedule, projectionService.CreateProjection());
				retList.Add(scheduleProjection);
			}

			return retList;
		}

	}
}