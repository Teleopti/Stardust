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
	}
}
