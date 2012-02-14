﻿using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface IBudgetSkillStaffPeriodContainer
	{
		IEnumerable<IBudgetGroupDayDetailModel> SelectedBudgetDays { get; }
		IEnumerable<ISkillStaffPeriod> ForPeriod(DateTimePeriod dateTimePeriod);
	}
}