using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
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
