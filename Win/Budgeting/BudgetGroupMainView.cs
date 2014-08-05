using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces.Infrastructure;
using log4net;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Budgeting.Events;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Ccc.WinCode.Common;
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
			EventSubscription();
			SetTexts();
			SetUpClipboard();
			ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdvFixed1);
			var tabView = (Control)_budgetGroupTabView;
			gradientPanelMain.Controls.Add(tabView);
			tabView.Dock = DockStyle.Fill;
			tabView.VisibleChanged += _budgetGroupTabView_VisibleChanged;
			
		}

		void _budgetGroupTabView_VisibleChanged(object sender, EventArgs e)
		{
			if (((BudgetGroupTabView) sender).Visible)
			{
				Presenter.Initialize();
			}
		}

		public BudgetGroupMainPresenter Presenter { get; set; }

		private void SetUpClipboard()
		{
			_clipboardControl = new ClipboardControl();
			var hostClipboardControl = new ToolStripControlHost(_clipboardControl);

			toolStripEx1.Items.Add(hostClipboardControl);

			_clipboardControl.CutClicked += ClipboardControlCutClicked;
			_clipboardControl.CopyClicked += ClipboardControlCopyClicked;
			_clipboardControl.PasteClicked += ClipboardControlPasteClicked;

			UpdateClipboardStatus(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = false });
		}

		private void ClipboardControlCutClicked(object sender, EventArgs e)
		{
			UpdateClipboardStatus(new ClipboardStatusEventModel{ClipboardAction = ClipboardAction.Paste, Enabled = true});
			_localEventAggregator.GetEvent<CutGridClicked>().Publish(string.Empty);
		}

		private void ClipboardControlCopyClicked(object sender, EventArgs e)
		{
			UpdateClipboardStatus(new ClipboardStatusEventModel { ClipboardAction = ClipboardAction.Paste, Enabled = true });
			_localEventAggregator.GetEvent<CopyGridClicked>().Publish(string.Empty);
		}

		private void ClipboardControlPasteClicked(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<PasteGridClicked>().Publish(string.Empty);
		}

		private void EventSubscription()
		{
			_localEventAggregator.GetEvent<AddShrinkageRow>().Subscribe(AddShrinkageRow);
			_localEventAggregator.GetEvent<AddEfficiencyShrinkageRow>().Subscribe(AddEfficiencyShrinkageRow);
			_localEventAggregator.GetEvent<DeleteCustomShrinkages>().Subscribe(DeleteCustomShrinkages);
			_localEventAggregator.GetEvent<DeleteCustomEfficiencyShrinkages>().Subscribe(DeleteCustomEfficiencyShrinkages);
		    _localEventAggregator.GetEvent<GridSelectionChanged>().Subscribe(GridSelectionChanged);
			_localEventAggregator.GetEvent<LoadDataStarted>().Subscribe(LoadDataStarted);
			_localEventAggregator.GetEvent<LoadDataFinished>().Subscribe(LoadDataFinished);
			_localEventAggregator.GetEvent<UpdateClipboardStatus>().Subscribe(UpdateClipboardStatus);
			_localEventAggregator.GetEvent<ChangeCustomShrinkage>().Subscribe(ChangeCustomShrinkage);
			_localEventAggregator.GetEvent<ChangeCustomEfficiencyShrinkage>().Subscribe(ChangeCustomEfficiencyShrinkage);
		    _localEventAggregator.GetEvent<ForceCloseBudget>().Subscribe(ForceCloseBudget);
            _globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Subscribe(budgetGroupNeedsRefresh);
		}

	    private void budgetGroupNeedsRefresh(IBudgetGroup budgetGroup)
	    {
            Presenter.UpdateBudgetGroup(budgetGroup);
	    }

	    private void ForceCloseBudget(bool obj)
	    {
            _forceClose = obj;
	        Close();
	    }

	    private void ChangeCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage efficiency)
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

	    private void UpdateClipboardStatus(ClipboardStatusEventModel e)
		{
			_clipboardControl.SetButtonState(e.ClipboardAction, e.Enabled);
		}

		private void LoadDataFinished(string loadType)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(LoadDataFinished), loadType);
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

		private void ChangeCustomShrinkage(ICustomShrinkage shrinkage)
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

		private void DeleteCustomEfficiencyShrinkages(IEnumerable<ICustomEfficiencyShrinkage> efficiencyShrinkagesToBeDeleted)
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

		private void DeleteCustomShrinkages(IEnumerable<ICustomShrinkage> shrinkagesToBeDeleted)
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
        private void AddEfficiencyShrinkageRow(string obj)
        {
            using (var efficiencyShrinkageForm = new SetEfficiencyShrinkageForm())
            {
                efficiencyShrinkageForm.Save += efficiencyShrinkageNameForm_Save;
                if (!_budgetPermissionService.IsAllowancePermitted)
                    efficiencyShrinkageForm.HideIncludedInRequestAllowance();
                efficiencyShrinkageForm.SetHelpId("NameEfficiencyShrinkage");
                efficiencyShrinkageForm.ShowDialog(this);
            }
        }

	    private void efficiencyShrinkageNameForm_Save(object sender, CustomEventArgs<CustomEfficiencyShrinkageUpdatedEventArgs> e)
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
		private void AddShrinkageRow(string obj)
		{
			var shrinkageForm = new AddShrinkageForm(_globalEventAggregator, Presenter.LoadedBudgetGroup, _unitOfWorkFactory, _repositoryFactory);
            shrinkageForm.CustomShrinkageAdded += shrinkageNameForm_CustomShrinkageAdded;
            if (!_budgetPermissionService.IsAllowancePermitted)
                shrinkageForm.HideIncludedInRequestAllowance();
			shrinkageForm.SetHelpId("NameShrinkage");
			shrinkageForm.ShowDialog(this);
		}

        private void shrinkageNameForm_CustomShrinkageAdded(object sender, CustomEventArgs<CustomShrinkageUpdatedEventArgs> e)
		{
			if (IsDisposed) return;

            Presenter.ShrinkageRowAdded(e.Value.CustomShrinkage);
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			//foreach (var quickItem in ribbonControlAdv1.Header.QuickItems.OfType<QuickButtonReflectable>())
			//{
			//	quickItem.Text = LanguageResourceHelper.Translate(quickItem.Text);
			//	quickItem.ToolTipText = LanguageResourceHelper.Translate(quickItem.ToolTipText);
			//}
		}

		private void btnSave_click(object sender, EventArgs e)
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

		private void toolStripButtonLoadForecastedHours_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadForecastedHours>().Publish("LoadForecastedHours");
		}

		private void toolStripButtonLoadStaffEmployed_Click(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<LoadStaffEmployed>().Publish("LoadStaffEmployed");
		}

		private void toolStripButtonHelp_Click(object sender, EventArgs e)
		{
			ViewBase.ShowHelp(this,false);
		}

		private void toolStripButtonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripButtonExit_Click(object sender, EventArgs e)
		{
            if (!CloseAllOtherForms(this))
                return; // a form was canceled

            Close();
            ////this canceled
            if (Visible)
                return;
            Application.Exit();
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
				ShowView(value);
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

		private void toolStripButtonWeekView_Click(object sender, EventArgs e)
		{
			Presenter.ShowWeekView();
		}

		private void toolStripButtonDayView_Click(object sender, EventArgs e)
		{
			Presenter.ShowDayView();
		}

		private void toolStripButtonMonthView_Click(object sender, EventArgs e)
		{
			Presenter.ShowMonthView();
		}

		private void ShowView(ViewType value)
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

		private void toolStripButtonOptions_Click(object sender, EventArgs e)
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.S:
                        btnSave_click(this, EventArgs.Empty);
                        break;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

		private void ribbonControlAdv1_Click(object sender, EventArgs e)
		{

		}
	}
}
