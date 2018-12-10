using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetSkillStaffPeriodContainer : IBudgetSkillStaffPeriodContainer
	{
		private readonly IEnumerable<ISkillStaffPeriod> _skillStaffPeriods;

		public IEnumerable<IBudgetGroupDayDetailModel> SelectedBudgetDays { get; }
		
		public BudgetSkillStaffPeriodContainer(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, IEnumerable<IBudgetGroupDayDetailModel> selectedBudgetDays)
		{
			SelectedBudgetDays = selectedBudgetDays;
			_skillStaffPeriods = skillStaffPeriods;
		}

		public IEnumerable<ISkillStaffPeriod> ForPeriod(DateTimePeriod dateTimePeriod)
		{
			return _skillStaffPeriods.Where(p => dateTimePeriod.Contains(p.Period));
		}
	}
}