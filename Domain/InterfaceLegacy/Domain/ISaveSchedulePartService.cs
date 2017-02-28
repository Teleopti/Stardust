using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISaveSchedulePartService
	{
		IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);

		IList<string> Save(IEnumerable<IScheduleDay> scheduleDays, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);
	}
}