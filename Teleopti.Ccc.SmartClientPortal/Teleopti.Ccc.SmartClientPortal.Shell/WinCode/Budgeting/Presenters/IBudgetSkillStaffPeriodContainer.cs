using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public interface IBudgetSkillStaffPeriodContainer
	{
		IEnumerable<IBudgetGroupDayDetailModel> SelectedBudgetDays { get; }
		IEnumerable<ISkillStaffPeriod> ForPeriod(DateTimePeriod dateTimePeriod);
	}
}