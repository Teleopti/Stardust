using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SuccessfulScheduledAgents
	{
		public int Execute(IScheduleDictionary schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Count(x =>
			{
				var targetSummary = x.Value.CalculatedTargetTimeSummary(periodToCheck);
				var scheduleSummary = x.Value.CalculatedCurrentScheduleSummary(periodToCheck);
				return targetSummary.TargetTime.HasValue &&
					   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= scheduleSummary.ContractTime &&
					   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= scheduleSummary.ContractTime &&
					   x.Value.CalculatedCurrentScheduleSummary(periodToCheck).DaysWithoutSchedule == 0;
			});
		}
	}
	
	
	public class FailedScheduledAgents
	{
		public IEnumerable<IPerson> Execute(IScheduleDictionary schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Where(x =>
			{
				var targetSummary = x.Value.CalculatedTargetTimeSummary(periodToCheck);
				var scheduleSummary = x.Value.CalculatedCurrentScheduleSummary(periodToCheck);
				return !(targetSummary.TargetTime.HasValue &&
					   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= scheduleSummary.ContractTime &&
					   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= scheduleSummary.ContractTime &&
					   x.Value.CalculatedCurrentScheduleSummary(periodToCheck).DaysWithoutSchedule == 0);
			}).Select(x=>x.Key);
		}
	}
}