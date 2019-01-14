using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetingSkillStaffPeriodProvider : IBudgetSkillStaffPeriodProvider
	{
		private readonly BudgetGroupMainModel _mainModel;
		private readonly ISelectedBudgetDays _selectedBudgetDays;
		private readonly SkillDayLoadHelper _skillDayLoadHelper;

		public BudgetingSkillStaffPeriodProvider(BudgetGroupMainModel mainModel, ISelectedBudgetDays selectedBudgetDays, SkillDayLoadHelper skillDayLoadHelper)
		{
			_mainModel = mainModel;
			_selectedBudgetDays = selectedBudgetDays;
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IBudgetSkillStaffPeriodContainer CreateContainer()
		{
			var selectedBudgetDays = _selectedBudgetDays.Find();
			if (selectedBudgetDays.IsEmpty())
			{
				return new BudgetSkillStaffPeriodContainer(new List<ISkillStaffPeriod>(),new List<IBudgetGroupDayDetailModel>());
			}

			var selectedDatePeriod = GetPeriodFromBudgetDays(selectedBudgetDays);
            var skillDaysForSkills = _skillDayLoadHelper.LoadBudgetSkillDays(selectedDatePeriod,
			                                                                    _mainModel.BudgetGroup.SkillCollection,
			                                                                    _mainModel.Scenario);

            var resolver = new ClosedBudgetDayResolver(selectedBudgetDays,skillDaysForSkills);
            resolver.InjectClosedDaysFromSkillDays();

		    var allSkillStaffPeriods = GetAllSkillStaffPeriods(skillDaysForSkills);

			return new BudgetSkillStaffPeriodContainer(allSkillStaffPeriods, selectedBudgetDays);
		}

	    private IEnumerable<ISkillStaffPeriod> GetAllSkillStaffPeriods(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysForSkills)
	    {
	    	var skillDayList = new List<ISkillDay>();
			foreach (var skill in _mainModel.BudgetGroup.SkillCollection)
	    	{
	    		skillDayList.AddRange(skillDaysForSkills[skill]);
	    	}

		    return skillDayList.SelectMany(s => s.SkillStaffPeriodCollection);
		}

		private static DateOnlyPeriod GetPeriodFromBudgetDays(IEnumerable<IBudgetGroupDayDetailModel> selectedBudgetDays)
		{
			var startDate = selectedBudgetDays.Min(d => d.BudgetDay.Day);
			var endDate = selectedBudgetDays.Max(d => d.BudgetDay.Day);
			return new DateOnlyPeriod(startDate, endDate);
		}
	}
}