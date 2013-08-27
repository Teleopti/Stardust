using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftFairnessGrid
	{
		
	}

	public class ShiftFairnessGridPresenter
	{
		private IShiftFairnessGrid _view;

		public ShiftFairnessGridPresenter(IShiftFairnessGrid view)
		{
			_view = view;
		}


		public double CalculateTotalStandardDeviation(IList<ShiftFairness> fairnessList)
		{
			var total = fairnessList.Sum(shiftFairness => shiftFairness.StandardDeviationValue);
			return total;
		}
	}
}
