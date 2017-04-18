using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public interface ISelectedBudgetDays
	{
		IEnumerable<IBudgetGroupDayDetailModel> Find();
	}
}