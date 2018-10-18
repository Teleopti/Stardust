using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IPredictShiftCategory
	{
		IShiftCategoryPredictionModel Train(IEnumerable<ShiftCategoryExample> data);
	}
}