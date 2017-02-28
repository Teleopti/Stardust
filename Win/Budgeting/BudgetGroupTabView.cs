using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Budgeting.Events;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Budgeting;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting
{
	public partial class BudgetGroupTabView : BaseUserControl, IBudgetDaySource, IBudgetGroupTabView
	{
		private readonly IBudgetGroupDayView _budgetGroupDayView;
		private readonly IBudgetGroupWeekView _budgetGroupWeekView;
		private readonly IBudgetGroupMonthView _budgetGroupMonthView;
		private readonly IEventAggregator _eventAggregator;
		private readonly BudgetDayReassociator _budgetDayReassociator;
	    private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler;

        public BudgetGroupTabView(IBudgetGroupDayView budgetGroupDayView, IBudgetGroupWeekView budgetGroupWeekView, IBudgetGroupMonthView budgetGroupMonthView, IEventAggregator eventAggregator, BudgetDayReassociator budgetDayReassociator, IGracefulDataSourceExceptionHandler dataSourceExceptionHandler)
		{
			_budgetGroupDayView = budgetGroupDayView;
			_budgetGroupWeekView = budgetGroupWeekView;
			_budgetGroupMonthView = budgetGroupMonthView;
			_eventAggregator = eventAggregator;
			_budgetDayReassociator = budgetDayReassociator;
            _dataSourceExceptionHandler = dataSourceExceptionHandler;
			InitializeComponent();
			EventSubscriptions();
			SetTexts();
		}

		public BudgetGroupTabPresenter Presenter { get; set; }

		private void EventSubscriptions()
		{
			_eventAggregator.GetEvent<LoadStaffEmployed>().Subscribe(OnLoadStaffEmployed);
			_eventAggregator.GetEvent<LoadForecastedHours>().Subscribe(OnLoadForecastedHours);
			_eventAggregator.GetEvent<CutGridClicked>().Subscribe(OnCutGridClicked);
			_eventAggregator.GetEvent<CopyGridClicked>().Subscribe(OnCopyGridClicked);
			_eventAggregator.GetEvent<PasteGridClicked>().Subscribe(OnPasteGridClicked);
			_eventAggregator.GetEvent<BeginBudgetDaysUpdate>().Subscribe(OnBeginBudgetDaysUpdate);
			_eventAggregator.GetEvent<EndBudgetDaysUpdate>().Subscribe(OnEndBudgetDaysUpdate);
			_eventAggregator.GetEvent<CloseBudgetGroupDayView>().Subscribe(OnClose);
			_eventAggregator.GetEvent<SaveBudgetGroupDayView>().Subscribe(OnSave);
		}

		private void OnEndBudgetDaysUpdate(bool obj)
		{
			Presenter.EndUpdate();
		}

		private void OnBeginBudgetDaysUpdate(bool obj)
		{
			Presenter.BeginUpdate();
		}

		public void OnClose(ICancelEventModel cancelEventModel)
		{
			Presenter.Close(cancelEventModel);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HideTabControl();
			backgroundWorkerLoadBudgetDays.RunWorkerAsync();
		}

		private void HideTabControl()
		{
			Visible = false;
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		private void OnCutGridClicked(string obj)
		{
			_eventAggregator.GetEvent<ViewsClipboardAction>().Publish(new ClipboardActionOnViews { ViewType = ResolveSelectedTabView(), ClipboardAction = ClipboardAction.Cut });
		}

		private void OnCopyGridClicked(string obj)
		{
			_eventAggregator.GetEvent<ViewsClipboardAction>().Publish(new ClipboardActionOnViews { ViewType = ResolveSelectedTabView(), ClipboardAction = ClipboardAction.Copy });
		}

		private void OnPasteGridClicked(string obj)
		{
			_eventAggregator.GetEvent<ViewsClipboardAction>().Publish(new ClipboardActionOnViews { ViewType = ResolveSelectedTabView(), ClipboardAction = ClipboardAction.Paste });
		}

		private BaseUserControl ResolveSelectedTabView()
		{
			var selectedTab = tabControlAdv.SelectedTab;
			if (selectedTab != null)
			{
				var selectedView = selectedTab.Controls.OfType<BaseUserControl>().FirstOrDefault();
				if (selectedView != null)
					return selectedView;
			}
			return null;
		}

		private void OnLoadStaffEmployed(string obj)
		{
			if (!backgroundWorkerLoadStaffEmployed.IsBusy)
			{
				_eventAggregator.GetEvent<ExitEditMode>().Publish(true);
				backgroundWorkerLoadStaffEmployed.RunWorkerAsync();
			}
		}

		private void OnLoadForecastedHours(string obj)
		{
			if (!backgroundWorkerLoadForecastedHours.IsBusy)
			{
				_eventAggregator.GetEvent<ExitEditMode>().Publish(true);
				backgroundWorkerLoadForecastedHours.RunWorkerAsync();
			}
		}

		private void InitializeTabControl()
		{
			foreach (TabPageAdv tabPage in tabControlAdv.TabPages)
			{
				tabPage.TabVisible = false;
			}
		}

		private void InitializeDayView(IBudgetGroupDayView budgetGroupDayView)
		{
			var view = (Control)budgetGroupDayView;
			tabPageAdvDay.Controls.Add(view);
			view.Dock = DockStyle.Fill;

		    budgetGroupDayView.Initialize();
		}

		private void InitializeWeekView(IBudgetGroupWeekView budgetGroupWeekView)
		{
			var view = (Control)budgetGroupWeekView;
			tabPageAdvWeek.Controls.Add(view);
			view.Dock = DockStyle.Fill;

		    budgetGroupWeekView.Initialize();
		}

		private void InitializeMonthView(IBudgetGroupMonthView budgetGroupMonthView)
		{
			var view = (Control)budgetGroupMonthView;
			tabPageAdvMonth.Controls.Add(view);
			view.Dock = DockStyle.Fill;

		    budgetGroupMonthView.Initialize();
		}

		public void ShowMonthView()
		{
			tabControlAdv.SelectedTab = tabPageAdvMonth;
		}

		public void ShowWeekView()
		{
			tabControlAdv.SelectedTab = tabPageAdvWeek;
		}

		public void ShowDayView()
		{
			tabControlAdv.SelectedTab = tabPageAdvDay;
		}

		public IEnumerable<IBudgetGroupDayDetailModel> Find()
		{
			var selectedTab = tabControlAdv.SelectedTab;
			if (selectedTab != null)
			{
				var selectedDayProvider = selectedTab.Controls.OfType<ISelectedBudgetDays>().FirstOrDefault();
				if (selectedDayProvider != null)
				{
					return selectedDayProvider.Find();
				}
			}
			return new List<IBudgetGroupDayDetailModel>();
		}

		public double? GetFulltimeEquivalentHoursPerDay(double fulltimeEquivalentHours)
		{
			if (InvokeRequired)
			{
				return (double?)Invoke(new Func<double, double?>(GetFulltimeEquivalentHoursPerDay), fulltimeEquivalentHours);
			}
			using (var promptDoubleBox = new PromptDoubleBox(fulltimeEquivalentHours,
																  UserTexts.Resources.FulltimeEquivalentHoursPerDay, 0d, 12d))
			{
				promptDoubleBox.SetHelpId("FulltimeEquivalentHours");
				promptDoubleBox.ShowDialog(this);

				return promptDoubleBox.Result;
			}
		}

		private void backgroundWorkerLoadStaffEmployed_DoWork(object sender, DoWorkEventArgs e)
		{
			PublishLoadDataStartedEvent();
			_eventAggregator.GetEvent<BeginBudgetDaysUpdate>().Publish(true);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_budgetDayReassociator.Reassociate();
				Presenter.LoadStaff();
			}
		}

		private void PublishLoadDataStartedEvent()
		{
			_eventAggregator.GetEvent<LoadDataStarted>().Publish(UserTexts.Resources.LoadingDataTreeDots);
		}

		private void PublishLoadDataFinishedEvent()
		{
			_eventAggregator.GetEvent<EndBudgetDaysUpdate>().Publish(true);
			_eventAggregator.GetEvent<LoadDataFinished>().Publish(string.Empty);
		}

        private void forceCloseEvent()
        {
            _eventAggregator.GetEvent<ForceCloseBudget>().Publish(true);
        }

		private bool HandleError(RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
		{
            return _dataSourceExceptionHandler.DataSourceExceptionOccurred(runWorkerCompletedEventArgs.Error);
		}

		private void backgroundWorkerLoadStaffEmployed_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
			HandleError(e);
			PublishLoadDataFinishedEvent();
		}

		private void backgroundWorkerLoadForecastedHours_DoWork(object sender, DoWorkEventArgs e)
		{
			PublishLoadDataStartedEvent();
			_eventAggregator.GetEvent<BeginBudgetDaysUpdate>().Publish(true);
			using (PerformanceOutput.ForOperation("LoadForecastedHours"))
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					_budgetDayReassociator.Reassociate();
					Presenter.LoadForecastedHours();
				}
			}
		}

		private void backgroundWorkerLoadForecastedHours_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
		    HandleError(e);
			PublishLoadDataFinishedEvent();
		}

		private void backgroundWorkerLoadBudgetDays_DoWork(object sender, DoWorkEventArgs e)
		{
			PublishLoadDataStartedEvent();
			using (PerformanceOutput.ForOperation("LoadBudgetDays"))
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					_budgetDayReassociator.Reassociate();
					Presenter.LoadDayModels();
				}
			}
		}

		private void backgroundWorkerLoadBudgetDays_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
            if (HandleError(e))
            {
                forceCloseEvent();
                return;
            }
            var attemptSucceeded = _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
            {
                InitializeDayView(_budgetGroupDayView);
                InitializeWeekView(_budgetGroupWeekView);
                InitializeMonthView(_budgetGroupMonthView);
            });
            if (!attemptSucceeded) { forceCloseEvent(); return;}
            InitializeTabControl();
			ShowTabControl();

			PublishLoadDataFinishedEvent();
		}

		private void ShowTabControl()
		{
			Visible = true;
		}

		public DialogResult AskToCommitChanges()
		{
			var result = ViewBase.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade, UserTexts.Resources.Save);
			return result;
		}

		public void OnSave(object obj)
		{
            _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    _budgetDayReassociator.Reassociate();
                    Presenter.Save(uow);
                }
            });
		}

		public void NotifyBudgetDaysUpdatedByOthers()
		{
			ViewBase.ShowErrorMessage(UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot, UserTexts.Resources.OptimisticLockHeader);
		}
	}
}
