using System;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
	public class BudgetDaysUpdater : IDisposable
	{
		private readonly IBudgetDayProvider _budgetDayProvider;
		private bool disposed;
	    private readonly bool _outerBatchRunning;

	    public BudgetDaysUpdater(IBudgetDayProvider budgetDayProvider)
		{
			if (budgetDayProvider == null) throw new ArgumentNullException(nameof(budgetDayProvider));
            if (budgetDayProvider.IsInBatch)
            {
                _outerBatchRunning = true;
                return;
            }

	        _budgetDayProvider = budgetDayProvider;
            _budgetDayProvider.IsInBatch = true;
            _budgetDayProvider.DisableCalculation();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
                if (disposing)
				{
                    if (!_outerBatchRunning)
                    {
                        _budgetDayProvider.HasUnsavedChanges = true;
                        _budgetDayProvider.Recalculate();
                        _budgetDayProvider.EnableCalculation();
                        _budgetDayProvider.IsInBatch = false;
                    }
				}
				disposed = true;
			}
		}
	}
}