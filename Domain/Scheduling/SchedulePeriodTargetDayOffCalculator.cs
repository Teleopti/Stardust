using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulePeriodTargetDayOffCalculator : ISchedulePeriodTargetDayOffCalculator
	{
		public MinMax<int> TargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			var daysOff = virtualSchedulePeriod.DaysOff();
			var contract = virtualSchedulePeriod.Contract;
			if (contract == null)
				return new MinMax<int>(daysOff, daysOff);
			var lower = daysOff - contract.NegativeDayOffTolerance;
			var upper = daysOff + contract.PositiveDayOffTolerance;
			return new MinMax<int>(lower, upper);
		}
	}
}