using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessSwapper
	{
		bool TrySwap(object suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization);
	}

	public class ShiftCategoryFairnessSwapper: IShiftCategoryFairnessSwapper
	{
		public bool TrySwap(object suggestion, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixListForFairnessOptimization)
		{
			throw new System.NotImplementedException();
		}
	}
}