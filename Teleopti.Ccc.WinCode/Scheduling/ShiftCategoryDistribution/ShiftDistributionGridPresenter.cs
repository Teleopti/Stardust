using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftDistributionGrid
	{
		
	}

	public class ShiftDistributionGridPresenter
	{
		private IShiftDistributionGrid _view;

		public ShiftDistributionGridPresenter(IShiftDistributionGrid view)
		{
			_view = view;
		}
	}
}
