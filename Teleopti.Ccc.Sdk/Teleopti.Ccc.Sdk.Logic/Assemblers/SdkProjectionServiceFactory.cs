using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class SdkProjectionServiceFactory : ISdkProjectionServiceFactory
	{
		public IProjectionService CreateProjectionService(IScheduleDay scheduleDay, string specialProjection, ICccTimeZoneInfo timeZoneInfo)
		{
			var merger = specialProjection.ContainsIgnoreCase("midnightsplit")
			                           	? (IProjectionMerger) new ProjectionMidnightSplitterMerger(timeZoneInfo)
			                           	: new ProjectionPayloadMerger();


			if (specialProjection.ContainsIgnoreCase("excludeabsences"))
			{
				var scheduleDayWithoutAbsences = (IScheduleDay)scheduleDay.Clone();
				scheduleDayWithoutAbsences.Clear<IPersonAbsence>();
				return new ScheduleProjectionService(scheduleDayWithoutAbsences, merger);
			}
			return new ScheduleProjectionService(scheduleDay, merger);
		}
	}
}