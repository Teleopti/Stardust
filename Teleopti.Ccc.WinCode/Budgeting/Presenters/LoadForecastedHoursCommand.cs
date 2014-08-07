using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface ILoadForecastedHoursCommand : IExecutableCommand
	{
	}

	public class LoadForecastedHoursCommand : ILoadForecastedHoursCommand
	{
		private readonly IBudgetSkillStaffPeriodProvider _skillStaffPeriodProvider;
		private readonly BudgetGroupMainModel _mainModel;

		public LoadForecastedHoursCommand(IBudgetSkillStaffPeriodProvider skillStaffPeriodProvider, BudgetGroupMainModel mainModel)
		{
			_skillStaffPeriodProvider = skillStaffPeriodProvider;
			_mainModel = mainModel;
		}

		public void Execute()
		{
			var container = _skillStaffPeriodProvider.CreateContainer();
			var forecastedHoursExtractor = new ForecastedHoursExtractor(container);
			foreach (BudgetGroupDayDetailModel budgetGroupDayDetailModel in container.SelectedBudgetDays)
			{
				var budgetDayPeriod =
					new DateOnlyPeriod(budgetGroupDayDetailModel.BudgetDay.Day, budgetGroupDayDetailModel.BudgetDay.Day).
						ToDateTimePeriod(_mainModel.BudgetGroup.TimeZone);
				budgetGroupDayDetailModel.ForecastedHours = forecastedHoursExtractor.ForecastedHoursForPeriod(budgetDayPeriod);
			}
		}
	}
}