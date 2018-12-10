using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
	public class BudgetGroupMonthFilter
	{
		private readonly IEnumerable<IBudgetGroupDayDetailModel> _budgetGroupDayDetailModels;
		private readonly CultureInfo _cultureInfo;

		public BudgetGroupMonthFilter(IEnumerable<IBudgetGroupDayDetailModel> budgetGroupDayDetailModels, CultureInfo cultureInfo)
		{
			_budgetGroupDayDetailModels = budgetGroupDayDetailModels;
			_cultureInfo = cultureInfo;
		}

		public IList<IList<IBudgetGroupDayDetailModel>> Filter()
		{
			IList<IList<IBudgetGroupDayDetailModel>> listOfMonths = new List<IList<IBudgetGroupDayDetailModel>>();
			IList<IBudgetGroupDayDetailModel> monthList = new List<IBudgetGroupDayDetailModel>();

			bool foundStart = false;
			foreach (var day in _budgetGroupDayDetailModels)
			{
				if ((_cultureInfo.Calendar.GetDayOfMonth(day.BudgetDay.Day.Date) == 1) && (!foundStart))
					foundStart = true;
				if (!foundStart) continue;
				var lastDate = DateHelper.GetLastDateInMonth(day.BudgetDay.Day.Date, _cultureInfo);
				monthList.Add(day);
				if (_cultureInfo.Calendar.GetDayOfMonth(day.BudgetDay.Day.Date) !=
					_cultureInfo.Calendar.GetDayOfMonth(lastDate)) continue;
				listOfMonths.Add(monthList);
				monthList = new List<IBudgetGroupDayDetailModel>();
			}
			return listOfMonths;
		}
	}
}