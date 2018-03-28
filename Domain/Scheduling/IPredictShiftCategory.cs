using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IPredictShiftCategory
	{
		IShiftCategoryPredictionModel Train(IEnumerable<ShiftCategoryExample> data);
	}
}