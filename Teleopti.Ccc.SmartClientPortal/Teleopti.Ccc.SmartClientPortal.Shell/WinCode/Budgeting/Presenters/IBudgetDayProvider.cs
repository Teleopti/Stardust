using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface IBudgetDayProvider
	{
		IList<IBudgetGroupDayDetailModel> DayModels();
		IList<IBudgetGroupDayDetailModel> VisibleDayModels();
		void EnableCalculation();
		void DisableCalculation();
		void Recalculate();
		bool HasUnsavedChanges { get; set; }
	    bool IsInBatch { get; set; }
	    IDisposable BatchUpdater();
	}
}