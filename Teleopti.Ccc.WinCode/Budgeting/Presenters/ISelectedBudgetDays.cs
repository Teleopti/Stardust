using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface ISelectedBudgetDays
	{
		IEnumerable<IBudgetGroupDayDetailModel> Find();
	}
}