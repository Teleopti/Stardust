using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISaveSchedulePartService
	{
		IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);

		IList<string> Save(IEnumerable<IScheduleDay> scheduleDays, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);
	}
}