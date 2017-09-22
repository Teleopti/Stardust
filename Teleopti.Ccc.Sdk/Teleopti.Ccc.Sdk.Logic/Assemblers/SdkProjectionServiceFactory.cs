using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class SdkProjectionServiceFactory : ISdkProjectionServiceFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IProjectionService CreateProjectionService(IScheduleDay scheduleDay, string specialProjection, TimeZoneInfo timeZoneInfo)
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