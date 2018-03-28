using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public interface IPredictCategory
	{
		IShiftCategorySelectionModel Train(IEnumerable<IShiftCategoryPredictorModel> data);
	}
}