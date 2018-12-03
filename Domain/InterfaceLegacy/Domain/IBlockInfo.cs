using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBlockInfo
	{
		DateOnlyPeriod BlockPeriod { get; }
		IList<double?> StandardDeviations { get; set; }
		double AverageOfStandardDeviations { get; }
		void LockDate(DateOnly date);
		IList<DateOnly> UnLockedDates();
		void ClearLocks();
	}
}