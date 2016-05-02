using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IBlockInfo
	{
		DateOnlyPeriod BlockPeriod { get; }
		IList<double?> StandardDeviations { get; set; }
		double SumOfStandardDeviations { get; }
		double AverageOfStandardDeviations { get; }
		void LockDate(DateOnly date);
		IList<DateOnly> UnLockedDates();
		void ClearLocks();
	}
}