using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IShiftCategoryUsageFinder
	{
		IEnumerable<ShiftCategoryExample> Find();
	}
}