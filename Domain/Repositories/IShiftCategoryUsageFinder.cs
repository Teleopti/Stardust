using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IShiftCategoryUsageFinder
	{
		IEnumerable<ShiftCategoryExample> Find();
	}
}