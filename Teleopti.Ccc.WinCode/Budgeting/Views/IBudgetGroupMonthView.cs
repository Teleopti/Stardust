﻿using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Views
{
    public interface IBudgetGroupMonthView
    {
		IList<BudgetGroupMonthDetailModel> DataSource { get; set; }
        void AddShrinkageRow(ICustomShrinkage customShrinkage);
        void AddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage);
        void Initialize();
    }
}
