using System.Collections.Generic;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.WinCode.Budgeting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetGroupMainPresenter
	{
		private readonly IBudgetGroupMainView _view;
		private readonly BudgetGroupMainModel _model;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupMainPresenter));

		public BudgetGroupMainPresenter(IBudgetGroupMainView view, BudgetGroupMainModel model)
		{
			_view = view;
			_model = model;
		}

	    public IBudgetGroup LoadedBudgetGroup
	    {
            get { return _model.BudgetGroup; }
	    }

	    public void Initialize()
		{
			_view.SetText(string.Format(CultureInfo.CurrentCulture,
			                            UserTexts.Resources.TeleoptiCCCColonModuleColonFromToDateScenarioColon,
										UserTexts.Resources.Budgets,
										_model.Period.StartDate.ToShortDateString(CultureInfo.CurrentCulture),
			                            _model.Period.EndDate.ToShortDateString(CultureInfo.CurrentCulture),
										_model.Scenario.Description.Name));
			_view.SelectedView = _model.BudgetSettings.SelectedView;
		}

		public void ShrinkageRowAdded(ICustomShrinkage customShrinkage)
		{
			_view.OnAddShrinkageRow(customShrinkage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void AddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage, IUnitOfWork unitOfWork)
		{
			try
			{
				unitOfWork.Reassociate(_model.BudgetGroup);
				_model.BudgetGroup.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);
				unitOfWork.PersistAll();
				_view.OnAddEfficiencyShrinkageRow(customEfficiencyShrinkage);
			}
			catch (OptimisticLockException optimisticLockException)
			{
				HandleOptimisticLockException(optimisticLockException);
				_model.BudgetGroup.RemoveCustomEfficiencyShrinkage(customEfficiencyShrinkage);	
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void DeleteShrinkageRows(IEnumerable<ICustomShrinkage> customShrinkages, IUnitOfWork unitOfWork)
		{
			try
			{
				unitOfWork.Reassociate(_model.BudgetGroup);
				customShrinkages.ForEach(_model.BudgetGroup.RemoveCustomShrinkage);
				unitOfWork.PersistAll();
				_view.OnDeleteShrinkageRows(customShrinkages);
			}
			catch (OptimisticLockException optimisticLockException)
			{
				HandleOptimisticLockException(optimisticLockException);
			}
		}

		private void HandleOptimisticLockException(OptimisticLockException optimisticLockException)
		{
			Logger.Warn("OptimisticLockException when saving custom rows to budgetgroup.", optimisticLockException);
			_view.NotifyCustomShrinkageUpdatedByOthers();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void DeleteEfficiencyShrinkageRows(IEnumerable<ICustomEfficiencyShrinkage> customEfficiencyShrinkages, IUnitOfWork unitOfWork)
		{
			try
			{
				unitOfWork.Reassociate(_model.BudgetGroup);
				customEfficiencyShrinkages.ForEach(_model.BudgetGroup.RemoveCustomEfficiencyShrinkage);
				unitOfWork.PersistAll();
				_view.OnDeleteEfficiencyShrinkageRows(customEfficiencyShrinkages);
			}
			catch (OptimisticLockException optimisticLockException)
			{
				HandleOptimisticLockException(optimisticLockException);
			}
		}

		public void ShowMonthView()
		{
			ClearButtons();
			_view.MonthView = true;
			_model.BudgetSettings.SelectedView = ViewType.Month;
			_view.ShowMonthView();
		}

		public void ShowWeekView()
		{
			ClearButtons();
			_view.WeekView = true;
			_model.BudgetSettings.SelectedView = ViewType.Week;
			_view.ShowWeekView();
		}

		public void ShowDayView()
		{
			ClearButtons();
			_view.DayView = true;
			_model.BudgetSettings.SelectedView = ViewType.Day;
			_view.ShowDayView();
		}

		public void SaveSettings()
		{
			_model.SaveSettings();
		}

		private void ClearButtons()
		{
			_view.DayView = false;
			_view.WeekView = false;
			_view.MonthView = false;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void CustomShrinkageUpdated(CustomShrinkageUpdatedEventArgs eventArgs)
		{
            _view.UpdateShrinkageProperty(_model.BudgetGroup.GetShrinkage(eventArgs.CustomShrinkage.Id.GetValueOrDefault()));
		}

        public void SaveUpdateEfficiencyShrinkage(CustomEfficiencyShrinkageUpdatedEventArgs eventArgs, IUnitOfWork unitOfWork)
		{
			unitOfWork.Reassociate(_model.BudgetGroup);
            var newCustomEfficientyShrinkage = new CustomEfficiencyShrinkage(eventArgs.ShrinkageName, eventArgs.IncludedInAllowance);
            _model.BudgetGroup.UpdateCustomEfficiencyShrinkage(eventArgs.Id.GetValueOrDefault(), newCustomEfficientyShrinkage);
			unitOfWork.PersistAll();
            _view.UpdateEfficiencyShrinkageProperty(_model.BudgetGroup.GetEfficiencyShrinkage(eventArgs.Id.GetValueOrDefault()));
		}

	    public void UpdateBudgetGroup(IBudgetGroup budgetGroup)
	    {
	        _model.BudgetGroup = budgetGroup;
	    }
	}
}
