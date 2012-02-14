﻿using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting
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
				if ((_cultureInfo.Calendar.GetDayOfMonth(day.BudgetDay.Day) == 1) && (!foundStart))
					foundStart = true;
				if (!foundStart) continue;
				var lastDate = DateHelper.GetLastDateInMonth(day.BudgetDay.Day, _cultureInfo);
				monthList.Add(day);
				if (_cultureInfo.Calendar.GetDayOfMonth(day.BudgetDay.Day) !=
					_cultureInfo.Calendar.GetDayOfMonth(lastDate)) continue;
				listOfMonths.Add(monthList);
				monthList = new List<IBudgetGroupDayDetailModel>();
			}
			return listOfMonths;
		}
	}
}