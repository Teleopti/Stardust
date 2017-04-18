using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetDayProvider : IBudgetDayProvider
	{
		private readonly IBudgetGroupDataService _dataService;
		private readonly IVisibleBudgetDays _visibleBudgetDays;
		private IList<IBudgetGroupDayDetailModel> _dayModels;
		private static readonly object LockObject=new object();

		public BudgetDayProvider(IBudgetGroupDataService dataService, IVisibleBudgetDays visibleBudgetDays)
		{
			_dataService = dataService;
			_visibleBudgetDays = visibleBudgetDays;
		}

		public IList<IBudgetGroupDayDetailModel> DayModels()
		{
			lock (LockObject)
			{
				if (_dayModels == null)
				{
					_dayModels = _dataService.FindAndCreate();
					ListenForInvalidate();
				}
			}
			return _dayModels;
		}

		public IList<IBudgetGroupDayDetailModel> VisibleDayModels()
		{
			return _visibleBudgetDays.Filter(DayModels());
		}

		public void EnableCalculation()
		{
			foreach (var budgetGroupDayDetailModel in DayModels())
			{
                budgetGroupDayDetailModel.EnablePropertyChangedInvocation();
			}
		}

        private void ListenForInvalidate()
        {
            foreach (var budgetGroupDayDetailModel in _dayModels)
            {
                budgetGroupDayDetailModel.Invalidate += budgetGroupDayDetailModel_Invalidate;
            }
        }

		public void DisableCalculation()
		{
			foreach (var budgetGroupDayDetailModel in DayModels())
			{
                budgetGroupDayDetailModel.DisablePropertyChangedInvocation();
			}
		}

		private void budgetGroupDayDetailModel_Invalidate(object sender, Domain.Common.CustomEventArgs<IBudgetGroupDayDetailModel> e)
		{
            if (!IsInBatch)
            {
                Recalculate();
                HasUnsavedChanges = true;
            }
		}

        public bool IsInBatch { get; set; }

		public IDisposable BatchUpdater()
		{
			return new BudgetDaysUpdater(this);
		}

		public void Recalculate()
		{
			_dataService.Recalculate(DayModels());
		}

		public bool HasUnsavedChanges { get; set; }
	}
}
