using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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