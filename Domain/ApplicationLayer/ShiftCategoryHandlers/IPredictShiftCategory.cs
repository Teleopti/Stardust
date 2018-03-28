using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public interface IPredictShiftCategory
	{
		IShiftCategoryPredictionModel Train(IEnumerable<ShiftCategoryExample> data);
	}
}