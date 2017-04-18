using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class VisibleBudgetDays : IVisibleBudgetDays
	{
		private readonly BudgetGroupMainModel _mainModel;

		public VisibleBudgetDays(BudgetGroupMainModel mainModel)
		{
			_mainModel = mainModel;
		}

		public IList<IBudgetGroupDayDetailModel> Filter(IEnumerable<IBudgetGroupDayDetailModel> detailedDays)
		{
			var period = _mainModel.Period;
			return detailedDays.Where(day => period.Contains(day.BudgetDay.Day)).ToList();
		}
	}
}
