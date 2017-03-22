using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class MovedDaysOff
	{
		public IList<DateOnly> AddedDaysOff { get; set; }
		public IList<DateOnly> RemovedDaysOff { get; set; }

		public IEnumerable<DateOnly> ModifiedDays()
		{
			return AddedDaysOff.Union(RemovedDaysOff);
		}
	}
}