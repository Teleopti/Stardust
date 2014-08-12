using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces.Infrastructure;
using log4net;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Budgeting.Events;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;
using ViewType = Teleopti.Ccc.WinCode.Budgeting.ViewType;

namespace Teleopti.Ccc.Win.Budgeting
{
	public partial class BudgetGroupMainView : BaseRibbonForm, IBudgetGroupMainView
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(BudgetGroupMainView));
		private readonly IBudgetGroupTabView _budgetGroupTabView;
		private readonly IEventAggregator _localEventAggregator;
		private ClipboardControl _clipboardControl;
		private bool _forceClose;
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler;
		private readonly IBudgetPermissionService _budgetPermissionService;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IToggleManager _toggleManager;
		private readonly IEventAggregator _globalEventAggregator;
		private bool _dayHasSelectedCell;
		private bool _weekHasSelectedCell;
		private bool _monthHasSelectedCell;

		public BudgetGroupMainView(IBudgetGroupTabView budgetGroupTabView, IEventAggregatorLocator eventAggregatorLocator, IGracefulDataSourceExceptionHandler dataSourceExceptionHandler, IBudgetPermissionService budgetPermissionService, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IToggleManager toggleManager)
		{
			_dataSourceExceptionHandler = dataSourceExceptionHandler;
			_budgetPermissionService = budgetPermissionService;
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_toggleManager = toggleManager;
			_localEventAggregator = eventAggregatorLocator.LocalAggregator();
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();
			InitializeComponent();
			_budgetGroupTabView = budgetGroupTabView;
			eventSubscription();
			SetTexts();
			setUpClipboard();
			ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdvFixed1);
			var tabView = (Control)_budgetGroupTabView;
			gradientPanelMain.Controls.Add(tabView);
			tabView.Dock = DockStyle.Fill;
			tabView.VisibleChanged += budgetGroupTabViewVisibleChanged;
			ribbonControlAdvFixed1.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
		}

		void budgetGroupTabViewVisibleChanged(object sender, EventArgs e)
		{
			if (((BudgetGroupTabView) sender).Visible)
			{
				Presenter.Initialize();
			}
		}

		public BudgetGroupMainPresenter Presenter { get; set; }

		private void setUpClipboard()
		{
			_clipboardControl = new ClipboardControl();
			var hostClipboardControl = new ToolStripControlHost(_clipboardControl);

			toolStripEx1.Items.Add(hostClipboardControl);

			_clipboardControl.CutClicked += clipboardControlCutClicked;
			_clipboardControl.CopyClicked += clipboardControlCopyClicked;
			_clipboardControl.PasteClicked += clipboardControlPasteClicked;

			updateClipboardStatus(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = false });
		}

		private void clipboardControlCutClicked(object sender, EventArgs e)
		{
			updateClipboardStatus(new ClipboardStatusEventModel{ClipboardAction = ClipboardAction.Paste, Enabled = true});
			_localEventAggregator.GetEvent<CutGridClicked>().Publish(string.Empty);
		}

		private void clipboardControlCopyClicked(object sender, EventArgs e)
		{
			updateClipboardStatus(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
			_localEventAggregator.GetEvent<CopyGridClicked>().Publish(string.Empty);
		}

		private void clipboardControlPasteClicked(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<PasteGridClicked>().Publish(string.Empty);
		}

		private void eventSubscription()
		{
			_localEventAggregator.GetEvent<AddShrinkageRow>().Subscribe(addShrinkageRow);
			_localEventAggregator.GetEvent<AddEfficiencyShrinkageRow>().Subscribe(addEfficiencyShrinkageRow);
			_localEventAggregator.GetEvent<DeleteCustomShrinkages>().Subscribe(deleteCustomShrinkages);
			_localEventAggregator.GetEvent<DeleteCustomEfficiencyShrinkages>().Subscribe(deleteCustomEfficiencyShrinkages);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Subscribe(GridSelectionChanged);
			_localEventAggregator.GetEvent<LoadDataStarted>().Subscribe(LoadDataStarted);
			_localEventAggregator.GetEvent<LoadDataFinished>().Subscribe(loadDataFinished);
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Subscribe(updateClipboardStatus);
			_localEventAggregator.GetEvent<ChangeCustomShrinkage>().Subscribe(changeCustomShrinkage);
			_localEventAggregator.GetEvent<ChangeCustomEfficiencyShrinkage>().Subscribe(changeCustomEfficiencyShrinkage);
			_localEventAggregator.GetEvent<ForceCloseBudget>().Subscribe(forceCloseBudget);
			_globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Subscribe(budgetGroupNeedsRefresh);
		}

		private void budgetGroupNeedsRefresh(IBudgetGroup budgetGroup)
		{
			Presenter.UpdateBudgetGroup(budgetGroup);
		}

		private void forceCloseBudget(bool obj)
		{
			_forceClose = obj;
			Close();
		}

		private void changeCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage efficiency)
		{
			using (var updateCustomShrinkageEfficiencyForm = new SetEfficiencyShrinkageForm(efficiency))
			{
				updateCustomShrinkageEfficiencyForm.Save += efficiencyShrinkageUpdateFormSave;
				if (!_budgetPermissionService.IsAllowancePermitted)
					updateCustomShrinkageEfficiencyForm.HideIncludedInRequestAllowance();
				updateCustomShrinkageEfficiencyForm.SetHelpId("NameShrinkage");
				updateCustomShrinkageEfficiencyForm.ShowDialog(this);
			}
		}

		private void updateClipboardStatus(ClipboardStatusEventModel e)
		{
			_clipboardControl.SetButtonState(e.ClipboardAction, e.Enabled);
		}

		private void loadDataFinished(string loadType)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(loadDataFinished), loadType);
			}
			else
			{
				Cursor = Cursors.Default;
				toolStripStatusLabelBudgetGroupMainView.Visible = false;
				toolStripSpinningProgressControlStatus.Visible = false;
				toolStripButtonLoadForecastedHours.Enabled = true; 
				toolStripButtonStaffEmployed.Enabled = false;
				toolStripExViews.Enabled = true;
				toolStripButtonSave.Enabled = true;
				_clipboardControl.Enabled = true;

				
			}
		}

		private void LoadDataStarted(string statusText)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(LoadDataStarted), statusText);
			}
			else
			{
				Cursor = Cursors.WaitCursor;
				toolStripStatusLabelBudgetGroupMainView.Text = statusText;
				toolStripStatusLabelBudgetGroupMainView.Visible = true;
				toolStripSpinningProgressControlStatus.Visible = true;
				toolStripButtonLoadForecastedHours.Enabled = false;
				toolStripButtonStaffEmployed.Enabled = false;
				toolStripExViews.Enabled = false;
				toolStripButtonSave.Enabled = false;
				_clipboardControl.Enabled = false;
			}
		}

		private void changeCustomShrinkage(ICustomShrinkage shrinkage)
		{
			using (var updateShrinkageForm = new EditShrinkageForm(_globalEventAggregator, shrinkage, _unitOfWorkFactory, _repositoryFactory))
			{
				updateShrinkageForm.CustomShrinkageUpdated += updateShrinkageFormCustomShrinkageUpdated;
				if (!_budgetPermissionService.IsAllowancePermitted)
					updateShrinkageForm.HideIncludedInRequestAllowance();
				updateShrinkageForm.SetHelpId("NameShrinkage");
				updateShrinkageForm.ShowDialog(this);
			}
		}

		private void efficiencyShrinkageUpdateFormSave(object sender, CustomEventArgs<CustomEfficiencyShrinkageUpdatedEventArgs> e)
		{
			if (IsDisposed) return;

			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					Presenter.SaveUpdateEfficiencyShrinkage(e.Value, unitOfWork);
					_globalEventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Publish("efficiencyShrinkageUpdateFormSave");
				}
			});
		}

		private void updateShrinkageFormCustomShrinkageUpdated(object sender, CustomEventArgs<CustomShrinkageUpdatedEventArgs> e)
		{
			if (IsDisposed) return;
			Presenter.CustomShrinkageUpdated(e.Value);
		}

		private void deleteCustomEfficiencyShrinkages(IEnumerable<ICustomEfficiencyShrinkage> efficiencyShrinkagesToBeDeleted)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.AreYouSureYouWantToDelete);
			if (ViewBase.ShowYesNoMessage(questionString, UserTexts.Resources.Delete) != DialogResult.Yes) return;

			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					Presenter.DeleteEfficiencyShrinkageRows(efficiencyShrinkagesToBeDeleted, unitOfWork);
					_globalEventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Publish("DeleteCustomEfficiencyShrinkages");
				}
			});
		}

		private void deleteCustomShrinkages(IEnumerable<ICustomShrinkage> shrinkagesToBeDeleted)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.AreYouSureYouWantToDelete);
			if (ViewBase.ShowYesNoMessage(questionString, UserTexts.Resources.Delete) != DialogResult.Yes) return;

			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					Presenter.DeleteShrinkageRows(shrinkagesToBeDeleted, unitOfWork);
					_globalEventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Publish("DeleteCustomShrinkages");
				}
			});
		}

		public void GridSelectionChanged(bool isSelected)
		{
			if (isSelected)
			{
				switch (SelectedView)
				{
					case ViewType.Day:
						_dayHasSelectedCell = true;
						break;
					case ViewType.Week:
						_weekHasSelectedCell = true;
						break;
					case ViewType.Month:
						_monthHasSelectedCell = true;
						break;
				}
				toolStripButtonStaffEmployed.Enabled = true;
			}
			else
			{
				_dayHasSelectedCell = false;
				_weekHasSelectedCell = false;
				_monthHasSelectedCell = false;
				toolStripButtonStaffEmployed.Enabled = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void addEfficiencyShrinkageRow(string obj)
		{
			using (var efficiencyShrinkageForm = new SetEfficiencyShrinkageForm())
			{
				efficiencyShrinkageForm.Save += efficiencyShrinkageNameFormSave;
				if (!_budgetPermissionService.IsAllowancePermitted)
					efficiencyShrinkageForm.HideIncludedInRequestAllowance();
				efficiencyShrinkageForm.SetHelpId("NameEfficiencyShrinkage");
				efficiencyShrinkageForm.ShowDialog(this);
			}
		}

		private void efficiencyShrinkageNameFormSave(object sender, CustomEventArgs<CustomEfficiencyShrinkageUpdatedEventArgs> e)
		{
			if (IsDisposed) return;

			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var efficiencyShrinkage = new CustomEfficiencyShrinkage(e.Value.ShrinkageName, e.Value.IncludedInAllowance);
					Presenter.AddEfficiencyShrinkageRow(efficiencyShrinkage, unitOfWork);
					_globalEventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Publish("efficiencyShrinkageNameForm_Save");
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void addShrinkageRow(string obj)
		{
			var shrinkageForm = new AddShrinkageForm(_globalEventAggregator, Presenter.LoadedBudgetGroup, _unitOfWorkFactory, _repositoryFactory);
			shrinkageForm.CustomShrinkageAdded += shrinkageNameFormCustomShrinkageAdded;
			if (!_budgetPermissionService.IsAllowancePermitted)
				shrinkageForm.HideIncludedInRequestAllowance();
			shrinkageForm.SetHelpId("NameShrinkage");
			shrinkageForm.ShowDialog(this);
		}

		private void shrinkageNameFormCustomShrinkageAdded(object sender, CustomEventArgs<CustomShrinkageUpdatedEventArgs> e)
		{
			if (IsDisposed) return;

			Presenter.ShrinkageRowAdded(e.Value.CustomShrinkage);
		}

		private void btnSaveClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<SaveBudgetGroupDayView>().Publish("BudgetGroupMainView.btnSave_click");
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_forceClose) return;

			_localEventAggregator.GetEvent<CloseBudgetGroupDayView>().Publish(new CancelEventModel(e));
			
			try
			{
				Presenter.SaveSettings();
			}
			catch (DataSourceException dataSourceException)
			{
				_logger.Error("An error occurred when trying to save settings for budget group.", dataSourceException);
			}
		}

		public void OnAddShrinkageRow(ICustomShrinkage customShrinkage)
		{
			_localEventAggregator.GetEvent<ShrinkageRowAdded>().Publish(customShrinkage);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(false);
		}

		public void NotifyCustomShrinkageUpdatedByOthers()
		{
			ViewBase.ShowErrorMessage(UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot, UserTexts.Resources.OptimisticLockHeader);
		}

		public void OnAddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			_localEventAggregator.GetEvent<EfficiencyShrinkageRowAdded>().Publish(customEfficiencyShrinkage);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(false);
		}

		public void OnDeleteShrinkageRows(IEnumerable<ICustomShrinkage> customShrinkages)
		{
			_localEventAggregator.GetEvent<ShrinkageRowsDeleted>().Publish(customShrinkages);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(false);
		}

		public void OnDeleteEfficiencyShrinkageRows(IEnumerable<ICustomEfficiencyShrinkage> customEfficiencyShrinkages)
		{
			_localEventAggregator.GetEvent<EfficiencyShrinkageRowsDeleted>().Publish(customEfficiencyShrinkages);
			_localEventAggregator.GetEvent<GridSelectionChanged>().Publish(false);
		}

		public void SetText(string windowTitle)
		{
			Text = windowTitle;
		}

		private void toolStripButtonLoadForecastedHoursClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadForecastedHours>().Publish("LoadForecastedHours");
		}

		private void toolStripButtonLoadStaffEmployedClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadStaffEmployed>().Publish("LoadStaffEmployed");
		}

		public ViewType SelectedView
		{
			get
			{
				if (toolStripButtonDayView.Checked)
					return ViewType.Day;
				return toolStripButtonWeekView.Checked ? ViewType.Week : ViewType.Month;
			} 
			set {
				showView(value);
			}
		}

		public void UpdateShrinkageProperty(ICustomShrinkage customShrinkage)
		{
			_localEventAggregator.GetEvent<UpdateShrinkageProperty>().Publish(customShrinkage);
		}

		public void UpdateEfficiencyShrinkageProperty(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			_localEventAggregator.GetEvent<UpdateEfficiencyShrinkageProperty>().Publish(customEfficiencyShrinkage);
		}

		public void ShowMonthView()
		{
			_budgetGroupTabView.ShowMonthView();
			toolStripButtonStaffEmployed.Enabled = _monthHasSelectedCell;
		}

		public void ShowWeekView()
		{
			_budgetGroupTabView.ShowWeekView(); 
			toolStripButtonStaffEmployed.Enabled = _weekHasSelectedCell;
		}

		public void ShowDayView()
		{
			_budgetGroupTabView.ShowDayView(); 
			toolStripButtonStaffEmployed.Enabled = _dayHasSelectedCell;
		}

		public bool MonthView
		{
			get { return toolStripButtonMonthView.Checked; }
			set { toolStripButtonMonthView.Checked = value; }
		}

		public bool WeekView
		{
			get { return toolStripButtonWeekView.Checked; }
			set { toolStripButtonWeekView.Checked = value; }
		}

		public bool DayView
		{
			get { return toolStripButtonDayView.Checked; }
			set { toolStripButtonDayView.Checked = value; }
		}

		private void toolStripButtonWeekViewClick(object sender, EventArgs e)
		{
			Presenter.ShowWeekView();
		}

		private void toolStripButtonDayViewClick(object sender, EventArgs e)
		{
			Presenter.ShowDayView();
		}

		private void toolStripButtonMonthViewClick(object sender, EventArgs e)
		{
			Presenter.ShowMonthView();
		}

		private void showView(ViewType value)
		{
			switch (value)
			{
				case ViewType.Day:
					Presenter.ShowDayView();
					break;
				case ViewType.Week:
					Presenter.ShowWeekView();
					break;
				case ViewType.Month:
					Presenter.ShowMonthView();
					break;
			}
		}
		
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;

			if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
			{
				switch (keyData)
				{
					case Keys.Control | Keys.S:
						btnSaveClick(this, EventArgs.Empty);
						break;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		
		private void backStageButton1Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<SaveBudgetGroupDayView>().Publish("BudgetGroupMainView.btnSave_click");
		}

		private void backStageButton2Click(object sender, EventArgs e)
		{
			Close();
		}

		private void backStageButton3Click(object sender, EventArgs e)
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager)));
				settings.Show();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void backStageButton4Click(object sender, EventArgs e)
		{
			if (!CloseAllOtherForms(this)) return;

			Close();

			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}
	}
}
