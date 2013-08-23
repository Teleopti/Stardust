using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerAgentGrid
	{
		
	}

	public class ShiftPerAgentGridPresenter
	{
		private IShiftPerAgentGrid _view;

		public ShiftPerAgentGridPresenter(IShiftPerAgentGrid view)
		{
			_view = view;
		}
	}
}
