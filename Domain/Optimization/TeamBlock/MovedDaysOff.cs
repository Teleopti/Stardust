using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class MovedDaysOff
	{
		public IEnumerable<DateOnly> AddedDaysOff { get; set; }
		public IEnumerable<DateOnly> RemovedDaysOff { get; set; }

		public IEnumerable<DateOnly> ModifiedDays()
		{
			return AddedDaysOff.Union(RemovedDaysOff);
		}

		public bool Contains(DateOnly date)
		{
			return ModifiedDays().Contains(date);
		}
	}
}