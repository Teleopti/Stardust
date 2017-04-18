﻿#region wohoo!! 95 usings in one form

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autofac;
using log4net;
using MbCache.Core;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingSessionPreferences;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Editor;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Notes;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{

	public partial class SchedulingScreen : BaseRibbonForm
	{
		private readonly HashSet<TimeZoneInfo> _detectedTimeZoneInfos = new HashSet<TimeZoneInfo>();
		private readonly ILifetimeScope _container;
		private static readonly ILog log = LogManager.GetLogger(typeof (SchedulingScreen));
		private ISchedulerStateHolder _schedulerState;
		private readonly ClipHandler<IScheduleDay> _clipHandlerSchedule;
		private readonly SkillDayGridControl _skillDayGridControl;
		private readonly SkillIntradayGridControl _skillIntradayGridControl;
		private readonly SkillWeekGridControl _skillWeekGridControl;
		private readonly SkillMonthGridControl _skillMonthGridControl;
		private readonly SkillFullPeriodGridControl _skillFullPeriodGridControl;
		private readonly SkillResultHighlightGridControl _skillResultHighlightGridControl;
		private DateOnly _currentIntraDayDate;
		private AgentInfoControl _agentInfoControl;
		private ShiftCategoryDistributionModel _shiftCategoryDistributionModel;
		private ScheduleViewBase _scheduleView;
		private RequestView _requestView;
		private ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private readonly IVirtualSkillHelper _virtualSkillHelper;
		private SchedulerMeetingHelper _schedulerMeetingHelper;
		private readonly IGridlockManager _gridLockManager;
		private readonly IList<IEntity> _temporarySelectedEntitiesFromTreeView;
		private GridChartManager _gridChartManager;
		private string _chartDescription;
		private GridRowInChartSettingButtons _gridrowInChartSettingButtons;
		private GridRow _currentSelectedGridRow;
		private readonly GridControl _grid;
		private readonly ChartControl _chartControlSkillData;
		private readonly TeleoptiLessIntelligentSplitContainer _splitContainerAdvMain;
		private readonly TeleoptiLessIntelligentSplitContainer _splitContainerAdvResultGraph;
		private readonly TeleoptiLessIntelligentSplitContainer _splitContainerLessIntellegentEditor;
		private readonly TeleoptiLessIntelligentSplitContainer _splitContainerLessIntellegentRestriction;
		private readonly TabControlAdv _tabSkillData;
		private readonly ElementHost _elementHostRequests;
		private readonly HandlePersonRequestView _handlePersonRequestView1;
		private readonly IEventAggregator _eventAggregator = new EventAggregator();
		private ClipboardControl _clipboardControl;
		private ClipboardControl _clipboardControlRestrictions;
		private EditControl _editControl;
		private EditControl _editControlRestrictions;
		private bool _uIEnabled = true;
		private SchedulePartFilter SchedulePartFilter = SchedulePartFilter.None;
		private bool _chartInIntradayMode;
		private IHandleBusinessRuleResponse _handleBusinessRuleResponse;
		private IRequestPresenter _requestPresenter;
		private readonly IScenario _scenario;
		private int _scheduleCounter;
		private bool _backgroundWorkerRunning;
		private ModifyEventArgs _lastModifiedPart;
		private readonly bool _shrinkage;
		private bool _validation;
		private readonly bool _teamLeaderMode;
		private readonly bool _loadRequsts;
		private bool _showEditor = true;
		private bool _showResult = true;
		private bool _showGraph = true;
		private bool _showRibbonTexts = true;
		private bool _showInfoPanel = true;
		private ControlType _controlType;
		private SchedulerMessageBrokerHandler _schedulerMessageBrokerHandler;
		private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
		private readonly ContextMenuStrip _contextMenuSkillGrid = new ContextMenuStrip();
		private readonly IOptimizerOriginalPreferences _optimizerOriginalPreferences;
		private readonly IOptimizationPreferences _optimizationPreferences;
		private readonly IBudgetPermissionService _budgetPermissionService;
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly IResourceOptimizationHelperExtended _optimizationHelperExtended;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
		private readonly ICollection<IPerson> _personsToValidate = new HashSet<IPerson>();
		private readonly ICollection<IPerson> _restrictionPersonsToReload = new HashSet<IPerson>();
		private readonly BackgroundWorker _backgroundWorkerValidatePersons = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerResourceCalculator = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerDelete = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerScheduling = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerOptimization = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerOvertimeScheduling = new BackgroundWorker();
		private readonly IUndoRedoContainer _undoRedo;

		private readonly ICollection<IPersonWriteProtectionInfo> _modifiedWriteProtections =
			new HashSet<IPersonWriteProtectionInfo>();

		private SchedulingScreenSettings _currentSchedulingScreenSettings;
		private ZoomLevel _currentZoomLevel;
		private ZoomLevel _previousZoomLevel;
		private SplitterManagerRestrictionView _splitterManager;
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private bool _inUpdate;
		private int _totalScheduled;
		private readonly IPersonRequestCheckAuthorization _personRequestAuthorizationChecker;
		private bool _forceClose;
		private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private readonly DateNavigateControl _dateNavigateControl;
		private bool _isAuditingSchedules;
		private IScheduleTag _defaultScheduleTag = NullScheduleTag.Instance;
		private System.Windows.Forms.Timer _tmpTimer = new System.Windows.Forms.Timer();
		private readonly ISchedulerGroupPagesProvider _groupPagesProvider;
		public IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSet { get; private set; }
		private SkillResultViewSetting _skillResultViewSetting;
		private const int maxCalculatMinMaxCacheEnries = 100000;
		private DateTimePeriod _selectedPeriod;
		private ScheduleTimeType _scheduleTimeType;
		private DateTime _lastSaved = DateTime.Now;
		private SchedulingScreenPermissionHelper _permissionHelper;
		private readonly CutPasteHandlerFactory _cutPasteHandlerFactory;
		private Form _mainWindow;
		private bool _cancelButtonPressed;
		private IDaysOffPreferences _daysOffPreferences;

		#region Constructors

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility",
			"CA1601:DoNotUseTimersThatPreventPowerStateChanges"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected SchedulingScreen()
		{
			InitializeComponent();
			_dateNavigateControl = new DateNavigateControl();

			var hostDatePicker = new ToolStripControlHost(_dateNavigateControl);
			toolStripExScheduleViews.Items.Add(hostDatePicker);
			_grid = schedulerSplitters1.Grid;
			_chartControlSkillData = schedulerSplitters1.ChartControlSkillData;
			_splitContainerAdvMain = schedulerSplitters1.SplitContainerAdvMainContainer;
			_splitContainerAdvResultGraph = schedulerSplitters1.SplitContainerAdvResultGraph;
			_splitContainerLessIntellegentEditor = schedulerSplitters1.SplitContainerLessIntelligent1;
			_splitContainerLessIntellegentRestriction = schedulerSplitters1.SplitContainerView;
			_elementHostRequests = schedulerSplitters1.ElementHostRequests;
			_handlePersonRequestView1 = schedulerSplitters1.HandlePersonRequestView1;
			_tabSkillData = schedulerSplitters1.TabSkillData;
			wpfShiftEditor1 = new WpfShiftEditor(_eventAggregator, new CreateLayerViewModelService(), true);
			notesEditor =
				new NotesEditor(
					PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
			schedulerSplitters1.MultipleHostControl3.AddItem(Resources.ShiftEditor, wpfShiftEditor1);
			schedulerSplitters1.MultipleHostControl3.AddItem(Resources.Note, notesEditor);
			toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = false;
			toolStripSpinningProgressControl1.Visible = true;
			_scheduleTimeType = ScheduleTimeType.ContractTime;
			toolStripMenuItemContractTime.Tag = ScheduleTimeType.ContractTime;
			toolStripMenuItemWorkTime.Tag = ScheduleTimeType.WorkTime;
			toolStripMenuItemPaidTime.Tag = ScheduleTimeType.PaidTime;
			toolStripMenuItemOverTime.Tag = ScheduleTimeType.OverTime;
			if (!DesignMode) SetTexts();

			// this timer is just for fixing bug 17948 regarding dateNavigationControl && show agent info when RTL and coming back from File
			_tmpTimer.Interval = 50;
			_tmpTimer.Enabled = false;

			//if it disappears again in the designer
			ribbonControlAdv1.QuickPanelVisible = true;	
		}

		private void checkSmsLinkLicense()
		{
			var dataSource = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().Name;
			var hasLicense = DefinedLicenseDataFactory.HasLicense(dataSource) &&
							 DefinedLicenseDataFactory.GetLicenseActivator(dataSource)
								 .EnabledLicenseOptionPaths.Contains(
									 DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			toolStripMenuItemNotifyAgent.Visible = hasLicense;
		}

		private void setMenuItemsHardToLeftToRight()
		{
			foreach (ToolStripTabItem	ribbonTabItem in ribbonControlAdv1.Header.MainItems)
			{
				foreach (ToolStripEx toolStrip in ribbonTabItem.Panel.Controls)
				{
					foreach (var stripPanelItem in toolStrip.Items)
					{
						var toolStripPanelItem = stripPanelItem as ToolStripPanelItem;
						if (toolStripPanelItem == null)
							continue;

						foreach (var item in toolStripPanelItem.Items)
						{
							var toolstripButton = item as ToolStripItem;
							if (toolstripButton != null)
							{
								toolstripButton.RightToLeft = RightToLeft.No;
							}
						}
					}
				}
			}
		}

		private void tmpTimerTick(object sender, EventArgs e)
		{
			_tmpTimer.Enabled = false;
			updateShiftEditor();
			if (_showInfoPanel) schedulerSplitters1.ToggelPropertyPanel(!toolStripButtonShowPropertyPanel.Checked);
		}

		private void dateNavigateControlClosedPopup(object sender, EventArgs e)
		{
			_grid.Focus();
		}

		private void dateNavigateControlSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
		{
			_scheduleView.SetSelectedDateLocal(e.Value);
			_grid.Invalidate();

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday) && _scheduleView is DayViewNew)
			{
				drawSkillGrid();
				reloadChart();
			}

			_tmpTimer.Enabled = true;
		}

		private void setShowRibbonTexts()
		{
			SchedulerRibbonHelper.SetShowRibbonTexts(_showRibbonTexts, toolStripPanelItemLoadOptions, toolStripPanelItem1,
				toolStripPanelItemLocks, toolStripPanelItemAssignments, toolStripPanelItemViews2, _editControl,
				_editControlRestrictions, _clipboardControl, _clipboardControlRestrictions, toolStripExHandleRequests);
		}

		private void updateLifeTimeScopeWith2ThingsWithFullDependencyChain()
		{
			var updater = new ContainerBuilder();

			updater.RegisterType<SchedulingScreenPersister>().As<ISchedulingScreenPersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleRangePersister>().As<IScheduleRangePersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleRangeConflictCollector>()
				.As<IScheduleRangeConflictCollector>()
				.InstancePerLifetimeScope();
			updater.Register(c => _schedulerMessageBrokerHandler)
				.As<IInitiatorIdentifier>()
				.As<IReassociateDataForSchedules>();

			updater.RegisterType<PersonAccountPersister>().As<IPersonAccountPersister>().InstancePerLifetimeScope();
			updater.RegisterType<PersonAccountConflictCollector>()
				.As<IPersonAccountConflictCollector>()
				.InstancePerLifetimeScope();
			updater.RegisterType<PersonAccountConflictResolver>().As<IPersonAccountConflictResolver>().InstancePerLifetimeScope();
			updater.RegisterType<RequestPersister>().As<IRequestPersister>().InstancePerLifetimeScope();
			updater.RegisterType<WriteProtectionPersister>().As<IWriteProtectionPersister>().InstancePerLifetimeScope();
			updater.RegisterType<WorkflowControlSetPublishDatePersister>()
				.As<IWorkflowControlSetPublishDatePersister>()
				.InstancePerLifetimeScope();

			updater.Update(_container.ComponentRegistry);
		}

		public SchedulingScreen(IComponentContext componentContext, DateOnlyPeriod loadingPeriod, IScenario loadScenario,
			bool shrinkage, bool calculation, bool validation, bool teamLeaderMode, bool loadRequsts, IList<IEntity> allSelectedEntities,
			Form ownerWindow)
			: this()
		{
			Application.DoEvents();
			_mainWindow = ownerWindow;

			_container = componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope();
			_undoRedo = new UndoRedoWithScheduleCallbackContainer(_container.Resolve<IScheduleDayChangeCallback>());
			_schedulerMessageBrokerHandler = new SchedulerMessageBrokerHandler(this, _container);
			updateLifeTimeScopeWith2ThingsWithFullDependencyChain();

			var toggleManager = _container.Resolve<IToggleManager>();
			_skillDayGridControl = new SkillDayGridControl(_container.Resolve<ISkillPriorityProvider>())
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
				ToggleManager = toggleManager
			};
			_skillWeekGridControl = new SkillWeekGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
				ToggleManager = toggleManager
			};
			_skillMonthGridControl = new SkillMonthGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
				ToggleManager = toggleManager
			};
			_skillFullPeriodGridControl = new SkillFullPeriodGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
				ToggleManager = toggleManager
			};
			_skillResultHighlightGridControl = new SkillResultHighlightGridControl();

			setUpZomMenu();

			_skillIntradayGridControl = new SkillIntradayGridControl("SchedulerSkillIntradayGridAndChart", _container.Resolve<ISkillPriorityProvider>())
			{
				ContextMenu = contextMenuStripResultView.ContextMenu
			};
			_optimizerOriginalPreferences = new OptimizerOriginalPreferences(new SchedulingOptions());
			_optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
			_daysOffPreferences = _container.Resolve<IDaysOffPreferences>();
			_overriddenBusinessRulesHolder = _container.Resolve<IOverriddenBusinessRulesHolder>();
			_workShiftWorkTime = _container.Resolve<IWorkShiftWorkTime>();
			_temporarySelectedEntitiesFromTreeView = allSelectedEntities;
			_virtualSkillHelper = _container.Resolve<IVirtualSkillHelper>();
			_budgetPermissionService = _container.Resolve<IBudgetPermissionService>();
			_schedulerState = _container.Resolve<ISchedulerStateHolder>();
			_groupPagesProvider = _container.Resolve<ISchedulerGroupPagesProvider>();
			_scheduleOptimizerHelper = _container.Resolve<ScheduleOptimizerHelper>();
			_restrictionExtractor = _container.Resolve<IRestrictionExtractor>();
			_optimizationHelperExtended = _container.Resolve<IResourceOptimizationHelperExtended>();
			_skillDayLoadHelper = _container.Resolve<ISkillDayLoadHelper>();
			_peopleAndSkillLoaderDecider = _container.Resolve<IPeopleAndSkillLoaderDecider>();

			_schedulerState.SetRequestedScenario(loadScenario);
			_schedulerState.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(loadingPeriod,
				TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
#pragma warning disable 618
			_schedulerState.UndoRedoContainer = _undoRedo;
#pragma warning restore 618
			_schedulerMeetingHelper = new SchedulerMeetingHelper(_schedulerMessageBrokerHandler, _schedulerState, _container.Resolve<IResourceCalculation>(), _container.Resolve<ISkillPriorityProvider>(), _container.Resolve<IScheduleStorageFactory>());
			//Using the same module id when saving meeting changes to avoid getting them via MB as well

			_clipHandlerSchedule = new ClipHandler<IScheduleDay>();

			_scenario = loadScenario;
			_shrinkage = shrinkage;
			_schedulerState.SchedulingResultState.SkipResourceCalculation = !calculation;
			_validation = validation;
			_schedulerState.SchedulingResultState.UseValidation = validation;
			_schedulerState.SchedulingResultState.UseMinWeekWorkTime =
				_container.Resolve<IToggleManager>().IsEnabled(Toggles.Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635);
			_teamLeaderMode = teamLeaderMode;
			_loadRequsts = loadRequsts;
			_schedulerState.SchedulingResultState.TeamLeaderMode = teamLeaderMode;

			toolStripProgressBar1.Visible = true;
			toolStripProgressBar1.Maximum = loadingPeriod.DayCount() + 5;
			toolStripProgressBar1.Step = 1;


			GridHelper.GridStyle(_grid);

			setColor();
			setUpRibbon();
			ribbonTemplatePanelsClose();

			_gridLockManager = _container.Resolve<IGridlockManager>();

			setEventHandlers();
			instantiateClipboardControl();
			instantiateEditControl();
			instantiateEditControlRestrictions();
			instantiateClipboardControlRestrictions();

			AddControlHelpContext(_grid);
			AddControlHelpContext(_chartControlSkillData);
			AddControlHelpContext(_skillDayGridControl);
			AddControlHelpContext(_skillIntradayGridControl);
			AddControlHelpContext(_skillWeekGridControl);
			AddControlHelpContext(_skillMonthGridControl);
			AddControlHelpContext(_skillFullPeriodGridControl);

			_dateNavigateControl.SetAvailableTimeSpan(loadingPeriod);
			_dateNavigateControl.SetSelectedDateNoInvoke(loadingPeriod.StartDate);
			_dateNavigateControl.SelectedDateChanged += dateNavigateControlSelectedDateChanged;
			_dateNavigateControl.ClosedPopup += dateNavigateControlClosedPopup;

			_backgroundWorkerDelete.WorkerSupportsCancellation = true;
			_backgroundWorkerDelete.DoWork += backgroundWorkerDeleteDoWork;
			_backgroundWorkerDelete.RunWorkerCompleted += backgroundWorkerDeleteRunWorkerCompleted;

			_backgroundWorkerResourceCalculator.WorkerSupportsCancellation = true;
			_backgroundWorkerResourceCalculator.DoWork += backgroundWorkerResourceCalculatorDoWork;
			_backgroundWorkerResourceCalculator.ProgressChanged += backgroundWorkerResourceCalculatorProgressChanged;
			_backgroundWorkerResourceCalculator.RunWorkerCompleted += backgroundWorkerResourceCalculatorRunWorkerCompleted;

			_backgroundWorkerValidatePersons.WorkerSupportsCancellation = true;
			_backgroundWorkerValidatePersons.RunWorkerCompleted += backgroundWorkerValidatePersonsRunWorkerCompleted;
			_backgroundWorkerValidatePersons.DoWork += backgroundWorkerValidatePersonsDoWork;

			_backgroundWorkerScheduling.WorkerReportsProgress = true;
			_backgroundWorkerScheduling.WorkerSupportsCancellation = true;
			_backgroundWorkerScheduling.DoWork += backgroundWorkerSchedulingDoWork;
			_backgroundWorkerScheduling.ProgressChanged += backgroundWorkerSchedulingProgressChanged;
			_backgroundWorkerScheduling.RunWorkerCompleted += backgroundWorkerSchedulingRunWorkerCompleted;

			_backgroundWorkerOvertimeScheduling.WorkerReportsProgress = true;
			_backgroundWorkerOvertimeScheduling.WorkerSupportsCancellation = true;
			_backgroundWorkerOvertimeScheduling.DoWork += backgroundWorkerOvertimeSchedulingDoWork;
			_backgroundWorkerOvertimeScheduling.ProgressChanged += backgroundWorkerOvertimeSchedulingProgressChanged;
			_backgroundWorkerOvertimeScheduling.RunWorkerCompleted += backgroundWorkerOvertimeSchedulingRunWorkerCompleted;

			_backgroundWorkerOptimization.WorkerReportsProgress = true;
			_backgroundWorkerOptimization.WorkerSupportsCancellation = true;
			_backgroundWorkerOptimization.DoWork += backgroundWorkerOptimizationDoWork;
			_backgroundWorkerOptimization.ProgressChanged += backgroundWorkerOptimizationProgressChanged;
			_backgroundWorkerOptimization.RunWorkerCompleted += backgroundWorkerOptimizationRunWorkerCompleted;

			//setPermissionOnControls();
			setInitialClipboardControlState();
			setupContextMenuSkillGrid();
			setupToolbarButtonsChartViews();
			contextMenuViews.Opened += contextMenuViewsOpened;
			setHeaderText(loadingPeriod.StartDate, loadingPeriod.EndDate, null, null);
			setLoadingOptions();
			setShowRibbonTexts();
			setMenuItemsHardToLeftToRight();

			_personRequestAuthorizationChecker = new PersonRequestCheckAuthorization();

			_permissionHelper = new SchedulingScreenPermissionHelper();
			_cutPasteHandlerFactory = new CutPasteHandlerFactory(this, () => _scheduleView, deleteFromSchedulePart,
				checkPastePermissions, pasteFromClipboard, enablePasteOperation);

			checkSmsLinkLicense();
		}

		private SchedulingScreenSettings loadSchedulingScreenSettings()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingRepository = new PersonalSettingDataRepository(uow);
					return settingRepository.FindValueByKey("SchedulingScreen", new SchedulingScreenSettings());
				}
			}
			catch (CouldNotCreateTransactionException ex)
			{
				log.Error("An error occurred while trying to load settings.", ex);
				return new SchedulingScreenSettings();
			}
		}

		private readonly List<IBusinessRuleResponse> _personAbsenceAccountPersistValidationBusinessRuleResponses =
			new List<IBusinessRuleResponse>();

		private void setLoadingOptions()
		{
			toolStripButtonShrinkage.Checked = _shrinkage;
			toolStripButtonCalculation.Checked = !_schedulerState.SchedulingResultState.SkipResourceCalculation;
			toolStripButtonValidation.Checked = _validation;
		}

		[RemoveMeWithToggle("Remove inside !_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_LoadingLessSchedules_42639) ", Toggles.ResourcePlanner_LoadingLessSchedules_42639)]
		private void setHeaderText(DateOnly start, DateOnly end, DateOnly? outerStart, DateOnly? outerEnd)
		{
			var currentCultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var startDate = start.Date.ToString("d", currentCultureInfo);
			var endDate = end.Date.ToString("d", currentCultureInfo);

			if (!_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_LoadingLessSchedules_42639))
			{
				if (outerStart.HasValue && outerEnd.HasValue)
				{
					if (start.AddDays(-7) != outerStart || end.AddDays(14) != outerEnd)
					{
						startDate = start.Date.ToString("d", currentCultureInfo) + "-" + end.Date.ToString("d", currentCultureInfo);
						endDate = "(" + outerStart.Value.Date.ToString("d", currentCultureInfo) + "-" +
								  outerEnd.Value.Date.ToString("d", currentCultureInfo) + ")";
					}
				}
			}

			Text = string.Format(currentCultureInfo, Resources.TeleoptiCCCColonModuleColonFromToDateScenarioColon, Resources.Schedules, startDate, endDate, _scenario.Description.Name);
		}

		private void schedulerMeetingHelperModificationOccured(object sender, ModifyMeetingEventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{

				uow.Reassociate(_schedulerState.SchedulingResultState.PersonsInOrganization);
				_schedulerMessageBrokerHandler.HandleMeetingChange(e.ModifiedMeeting, e.Delete);
				invalidateAfterMeetingChange(e);
			}
			if (_scheduleView != null &&
				_scheduleView.ViewGrid != null)
			{
				_scheduleView.ViewGrid.InvalidateRange(_scheduleView.ViewGrid.ViewLayout.VisibleCellsRange);
				RecalculateResources();
			}
		}

		private void invalidateAfterMeetingChange(ModifyMeetingEventArgs e)
		{
			var changeProvider = (IProvideCustomChangeInfo) e.ModifiedMeeting;
			var tracker = new MeetingChangeTracker();
			tracker.TakeSnapshot((IMeeting) changeProvider.BeforeChanges());
			var changes =
				tracker.CustomChanges(e.ModifiedMeeting, e.Delete ? DomainUpdateType.Delete : DomainUpdateType.Update)
					.Select(r => (MeetingChangedEntity) r.Root);
			changes.ForEach(c =>
			{
				var period = c.Period.ToDateOnlyPeriod(_schedulerState.TimeZoneInfo);
				period = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
				period.DayCollection().ForEach(_schedulerState.MarkDateToBeRecalculated);
			});
			_schedulerState.Schedules.Where(s => changes.Any(c => s.Key.Equals(c.MainRoot)))
				.ForEach(p => p.Value.ForceRecalculationOfTargetTimeContractTimeAndDaysOff());
		}

		private void contextMenuViewsOpened(object sender, EventArgs e)
		{
			if (_scheduleView != null)
				_scheduleView.Presenter.UpdateFromEditor();
		}

		#endregion

		#region editcontrol

		private void instantiateEditControl()
		{
			_editControl = new EditControl();
			SchedulerRibbonHelper.InstantiateEditControl(_editControl);
			_editControl.NewClicked += editControlNewClicked;
			_editControl.NewSpecialClicked += editControlNewSpecialClicked;
			_editControl.DeleteClicked += editControlDeleteClicked;
			_editControl.DeleteSpecialClicked += editControlDeleteSpecialClicked;
			var editControlHost = new ToolStripControlHost(_editControl);
			toolStripExEdit2.Items.Add(editControlHost);
		}

		private void instantiateEditControlRestrictions()
		{
			_editControlRestrictions = new EditControl();
			SchedulerRibbonHelper.InstantiateEditControlRestrictions(_editControlRestrictions);
			_editControlRestrictions.NewClicked += editControlRestrictionsNewClicked;
			_editControlRestrictions.NewSpecialClicked += editControlRestrictionsNewSpecialClicked;
			_editControlRestrictions.DeleteClicked += toolStripMenuItemRestrictionDeleteClick;
			var editControlHostRestrictions = new ToolStripControlHost(_editControlRestrictions);
			toolStripExEdit2.Items.Add(editControlHostRestrictions);
			editControlHostRestrictions.Visible = false;
		}

		private void editControlDeleteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems) e.ClickedItem.Tag)
			{
				case ClipboardItems.Special:
					_cutPasteHandlerFactory.For(_controlType).DeleteSpecial();
					break;
			}
		}

		private void editControlDeleteClicked(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Delete();
		}

		private void editControlNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			_editControl.CloseDropDown();
			if ((ClipboardItems) e.ClickedItem.Tag == ClipboardItems.DayOff)
				addDayOff();
			else
				addNewLayer((ClipboardItems) e.ClickedItem.Tag);
		}

		private void addNewLayer(ClipboardItems addType)
		{
			if (_scheduleView != null)
			{
				switch (addType)
				{
					case ClipboardItems.Shift:
						_scheduleView.Presenter.AddActivity();
						break;
					case ClipboardItems.Overtime:
						var definitionSets = MultiplicatorDefinitionSet.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime);
						_scheduleView.Presenter.AddOvertime(definitionSets.ToList());
						break;
					case ClipboardItems.Absence:
						if (!SchedulerState.CommonStateHolder.ActiveAbsences.Any())
						{
							ShowInformationMessage(Resources.NoAbsenceDefined, Resources.NoAbsenceDefinedCaption);
							return;
						}
						_scheduleView.Presenter.AddAbsence();
						break;
					case ClipboardItems.PersonalShift:
						_scheduleView.Presenter.AddPersonalShift();
						break;
				}
				_scheduleView.Presenter.ClipHandlerSchedule.Clear();
				RecalculateResources();
				RunActionWithDelay(updateShiftEditor, 50);
			}
		}

		private void editControlNewClicked(object sender, EventArgs e)
		{
			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
				addNewLayer(ClipboardItems.Shift);
			else
				_editControl.ToolStripButtonNew.ShowDropDown();

		}

		#endregion

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8 && e.Modifiers == Keys.Shift)
			{
				toggleCalculation();
			}
			if (e.KeyCode == Keys.M && e.Alt && e.Shift)
			{
				TestMode.Micke = !TestMode.Micke;
				toolStripMenuItemFindMatching.Visible = TestMode.Micke;
				toolStripMenuItemFindMatching2.Visible = TestMode.Micke;
				Refresh();
				drawSkillGrid();
				if (TestMode.Micke)
				{
					var skillGridMenuItem1 = new ToolStripMenuItem("Analyze primary/shoveled resources...");
					skillGridMenuItem1.Click += skillGridMenuItemAnalyzeResorceChangesClick;
					_contextMenuSkillGrid.Items.Add(skillGridMenuItem1);
					var skillGridMenuItem2 = new ToolStripMenuItem("Analyze shoveling");
					skillGridMenuItem2.Click += skillGridMenuItemShovelAnalyzerClick;
					_contextMenuSkillGrid.Items.Add(skillGridMenuItem2);
					var skillGridMenuItem = new ToolStripMenuItem("Agent Skill Analyzer...");
					skillGridMenuItem.Click += skillGridMenuItemAgentSkillAnalyser_Click;
					_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
				}
			}
			if (e.KeyCode == Keys.I && e.Shift && e.Alt)
			{
				SikuliHelper.SetInteractiveMode(true);
			}
			if (e.KeyCode == Keys.N && e.Shift && e.Alt)
			{
				SikuliHelper.SetInteractiveMode(false);
			}
			if (e.KeyCode == Keys.V && e.Alt && e.Shift)
			{
				SikuliHelper.EnterValidator(this);
			}
			if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
			{
				_cutPasteHandlerFactory.For(_controlType).PastePersonalShift();
			}
			if (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control)
			{
				undoKeyDown();
			}
			if (e.KeyCode == Keys.Y && e.Modifiers == Keys.Control)
			{
				redoKeyDown();
			}
			if (e.KeyCode == Keys.S && e.Modifiers == Keys.Control)
			{
				save();
			}

			if (e.KeyCode == Keys.Q && e.Control && e.Shift)
			{
				if (TestMode.Micke)
				{
					using (var agentSkillExplorer = new AgentSkillExplorer())
					{
						agentSkillExplorer.Setup(_schedulerState, _container);
						agentSkillExplorer.ShowDialog(this);
					}
				}
			}

			if (e.KeyCode == Keys.Enter && toolStripTextBoxFilter.Focused)
			{
				_requestView.FilterGrid(toolStripTextBoxFilter.Text.Split(' '), SchedulerState.FilteredCombinedAgentsDictionary);
				e.Handled = true;
				e.SuppressKeyPress = true;
			}

			base.OnKeyDown(e);
		}

		private void redoKeyDown()
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Redo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void undoKeyDown()
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Undo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void toggleCalculation()
		{		
			_schedulerState.SchedulingResultState.SkipResourceCalculation =
				!_schedulerState.SchedulingResultState.SkipResourceCalculation;
			toolStripButtonCalculation.Checked = !_schedulerState.SchedulingResultState.SkipResourceCalculation;
			if (_schedulerState.SchedulingResultState.SkipResourceCalculation)
			{
				statusStrip1.BackColor = Color.Salmon;
			}
			else
			{
				foreach (var date in _schedulerState.RequestedPeriod.DateOnlyPeriod.DayCollection())
				{
					_schedulerState.MarkDateToBeRecalculated(date);
				}
				RecalculateResources();
				statusStrip1.BackColor = Color.FromArgb(22, 165, 220);
			}
		}

		#region Clipboardcontrol

		private void instantiateClipboardControl()
		{
			_clipboardControl = new ClipboardControl();
			SchedulerRibbonHelper.InstantiateClipboardControl(_clipboardControl);
			_clipboardControl.CopyClicked += clipboardControlCopyClicked;
			_clipboardControl.CutSpecialClicked += clipboardControlCutSpecialClicked;
			_clipboardControl.CutClicked += clipboardControlCutClicked;
			_clipboardControl.PasteSpecialClicked += clipboardControlPasteSpecialClicked;
			_clipboardControl.PasteClicked += clipboardControlPasteClicked;
			var clipboardhost = new ToolStripControlHost(_clipboardControl);
			toolStripExClipboard.Items.Add(clipboardhost);
		}

		private void instantiateClipboardControlRestrictions()
		{
			_clipboardControlRestrictions = new ClipboardControl();
			SchedulerRibbonHelper.InstantiateClipboardControlRestrictions(_clipboardControlRestrictions);
			_clipboardControlRestrictions.CopyClicked += toolStripMenuItemRestrictionCopyClick;
			_clipboardControlRestrictions.PasteClicked += toolStripMenuItemRestrictionPasteClick;
			var clipboardhost = new ToolStripControlHost(_clipboardControlRestrictions);
			toolStripExClipboard.Items.Add(clipboardhost);
			clipboardhost.Visible = false;
		}

		private void setInitialClipboardControlState()
		{
			if (_clipboardControl != null)
			{
				_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
			}

			if (_clipboardControlRestrictions != null)
			{
				_clipboardControlRestrictions.SetButtonState(ClipboardAction.Paste, false);
			}
		}

		private void clipboardControlCutClicked(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Cut();
		}

		private void clipboardControlPasteClicked(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Paste();
		}

		private void clipboardControlCopyClicked(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Copy();
		}

		private void clipboardControlPasteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var handler = _cutPasteHandlerFactory.For(_controlType);
			switch ((ClipboardItems) e.ClickedItem.Tag)
			{
				case ClipboardItems.Shift:
					handler.PasteAssignment();
					break;
				case ClipboardItems.Absence:
					handler.PasteAbsence();
					break;
				case ClipboardItems.DayOff:
					handler.PasteDayOff();
					break;
				case ClipboardItems.PersonalShift:
					handler.PastePersonalShift();
					break;
				case ClipboardItems.Special:
					handler.PasteSpecial();
					break;
				case ClipboardItems.ShiftFromShifts:
					handler.PasteShiftFromShifts();
					break;
			}
		}

		private void clipboardControlCutSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var handler = _cutPasteHandlerFactory.For(_controlType);
			switch ((ClipboardItems) e.ClickedItem.Tag)
			{
				case ClipboardItems.Shift:
					handler.CutAssignment();
					break;
				case ClipboardItems.Absence:
					handler.CutAbsence();
					break;
				case ClipboardItems.DayOff:
					handler.CutDayOff();
					break;
				case ClipboardItems.PersonalShift:
					handler.CutPersonalShift();
					break;
				case ClipboardItems.Special:
					handler.CutSpecial();
					break;
			}
		}

		private void clipboardControlCopySpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems) e.ClickedItem.Tag)
			{
				case ClipboardItems.Special:
					_cutPasteHandlerFactory.For(_controlType).CopySpecial();
					break;
			}
		}

		#endregion

		#region Interface

		public ISchedulerStateHolder SchedulerState
		{
			get { return _schedulerState; }
		}

		public ClipHandler<IScheduleDay> ClipsHandlerSchedule
		{
			get { return _clipHandlerSchedule; }
		}

		public IGridlockManager LockManager
		{
			get { return _gridLockManager; }
		}

		#endregion

		#region Form events

		private void schedulingScreenLoad(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			Application.DoEvents();

			_currentSchedulingScreenSettings = loadSchedulingScreenSettings();
			_skillResultViewSetting = _currentSchedulingScreenSettings.SkillResultViewSetting;
			displayOptionsFromSetting(_currentSchedulingScreenSettings);

			//leave this at the top of this method
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXLoadingThreeDots");
			toolStripProgressBar1.Value = 0;
			toolStripProgressBar1.Maximum = _schedulerState.RequestedPeriod.DateOnlyPeriod.DayCount() + 19;
			toolStripProgressBar1.Visible = true;

			_splitContainerAdvMain.Visible = false;
			_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
			_clipboardControlRestrictions.SetButtonState(ClipboardAction.Paste, true);


			loadQuickAccessState();
			disableAllExceptCancelInRibbon();


			if (TestMode.Micke)
			{
				toolStripMenuItemFindMatching2.Visible = true;
				toolStripMenuItemFindMatching.Visible = true;
			}

			backgroundWorkerLoadData.WorkerSupportsCancellation = true;
			backgroundWorkerLoadData.DoWork += backgroundWorkerLoadDataDoWork;
			backgroundWorkerLoadData.RunWorkerCompleted += backgroundWorkerLoadDataRunWorkerCompleted;
			backgroundWorkerLoadData.ProgressChanged += backgroundWorkerLoadDataProgressChanged;


			var authorization = PrincipalAuthorization.Current();
			toolStripMenuItemMeetingOrganizer.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
			toolStripMenuItemWriteProtectSchedule.Enabled =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			toolStripMenuItemAddOvertimeAvailability.Visible =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);

			var publishScedule = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.PublishSchedule);
			toolStripMenuItemPublish.Visible = publishScedule;

			setPermissionOnControls();
			schedulerSplitters1.AgentRestrictionGrid.SelectedAgentIsReady += agentRestrictionGridSelectedAgentIsReady;
			schedulerSplitters1.MultipleHostControl3.GotFocus += multipleHostControl3OnGotFocus;

			//releaseEvents(this);

			Show();
			Application.DoEvents();

			_backgroundWorkerRunning = true;
			backgroundWorkerLoadData.RunWorkerAsync();
			//No code after the call to runworkerasynk
		}

		private void multipleHostControl3OnGotFocus(object sender, System.Windows.RoutedEventArgs routedEventArgs)
		{
			updateRibbon(ControlType.ShiftEditor);
		}

		private void loadQuickAccessState()
		{
			var loader = new QuickAccessState(_currentSchedulingScreenSettings, ribbonControlAdv1);
			loader.Load(toolStripButtonSaveLarge, toolStripSplitButtonQuickAccessUndo, toolStripButtonQuickAccessRedo,
				toolStripButtonQuickAccessCancel, toolStripButtonShowTexts);
		}

		private void saveQuickAccessState()
		{
			var saver = new QuickAccessState(_currentSchedulingScreenSettings, ribbonControlAdv1);
			saver.Save();
		}

		private bool formClosingInProgress;
		private void schedulingScreenFormClosing(object sender, FormClosingEventArgs e)
		{
			if (formClosingInProgress)
			{
				e.Cancel = true;
				return;
			}

			formClosingInProgress = true;

			cancelAllBackgroundWorkers();

			if (_forceClose || _schedulerState == null)
				return;

			if (_cachedPersonsFilterView != null && _cachedPersonsFilterView.Disposing == false)
				_cachedPersonsFilterView.Dispose();

			if (checkIfUserWantsToSaveUnsavedData() == -1)
			{
				e.Cancel = true;
				formClosingInProgress = false;
			}
			

			if (!e.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					saveAllChartSetting();
					saveQuickAccessState();
					_currentSchedulingScreenSettings.EditorSnapToResolution = wpfShiftEditor1.Interval;
					_currentSchedulingScreenSettings.HideEditor = !_showEditor;
					_currentSchedulingScreenSettings.HideGraph = !_showGraph;
					_currentSchedulingScreenSettings.HideResult = !_showResult;
					_currentSchedulingScreenSettings.HideInfoPanel = !_showInfoPanel;
					_currentSchedulingScreenSettings.HideRibbonTexts = !_showRibbonTexts;
					_currentSchedulingScreenSettings.DefaultScheduleTag = _defaultScheduleTag.Id;
					_currentSchedulingScreenSettings.SkillResultViewSetting = _skillResultViewSetting;

					if (_scheduleView != null)
					{
						var mapper = new SchedulerSortCommandMapper(SchedulerState, SchedulerSortCommandSetting.NoSortCommand, _container);
						var sortSetting = mapper.GetSettingFromCommand(_scheduleView.Presenter.SortCommand);
						_currentSchedulingScreenSettings.SortCommandSetting = sortSetting;
					}

					using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						var settingDataRepository = new PersonalSettingDataRepository(uow);
						OpenScenarioForPeriodSetting openScenarioForPeriodSetting = settingDataRepository.FindValueByKey("OpenScheduler",
							new OpenScenarioForPeriodSetting());
						openScenarioForPeriodSetting.NoShrinkage = !toolStripButtonShrinkage.Checked;
						openScenarioForPeriodSetting.NoCalculation = !toolStripButtonCalculation.Checked;
						openScenarioForPeriodSetting.NoValidation = !toolStripButtonValidation.Checked;
						settingDataRepository.PersistSettingValue(openScenarioForPeriodSetting);
						settingDataRepository.PersistSettingValue(_currentSchedulingScreenSettings);
						uow.PersistAll(_schedulerMessageBrokerHandler);
					}
				}
				catch (CouldNotCreateTransactionException dataSourceException)
				{
					log.Error("An error occurred when trying to save settings on closing scheduler.", dataSourceException);
				}
				catch (NullReferenceException nullReferenceException)
				{
					log.Error("An error occurred when trying to save settings on closing scheduler.", nullReferenceException);
				}
			}

			Cursor.Current = Cursors.Default;
		}



		#endregion

		#region Schedules RibbonBar Events

		#region Toolstrip events

		private void toolStripMenuItemBackToLegalStateClick(object sender, EventArgs e)
		{
			if (_backgroundWorkerRunning)
				return;

			if (_scheduleView == null)
				return;

			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (!selectedSchedules.Any())
				return;

			IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
			var hideMaxSeat = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_MaxSeatsNew_40939);
			using (
				var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions,
					daysOffPreferences, _schedulerState.CommonStateHolder.ActiveShiftCategories,
					true, _groupPagesProvider, _schedulerState.CommonStateHolder.ActiveScheduleTags, "SchedulingOptions",
					_schedulerState.CommonStateHolder.ActiveActivities, hideMaxSeat))
			{
				if (options.ShowDialog(this) == DialogResult.OK)
				{
					Cursor = Cursors.WaitCursor;
					disableAllExceptCancelInRibbon();
					_backgroundWorkerRunning = true;
					_backgroundWorkerOptimization.RunWorkerAsync(new SchedulingAndOptimizeArgument(selectedSchedules)
					{
						OptimizationMethod = OptimizationMethod.BackToLegalState,
						DaysOffPreferences = daysOffPreferences
					});
				}
			}
		}

		private void toolStripMenuItemOptimizeClick(object sender, EventArgs e)
		{
			if (_backgroundWorkerRunning) return;

			if (_scheduleView != null)
			{
				var selectedSchedules = _scheduleView.SelectedSchedules();
				if (!selectedSchedules.Any())
					return;

				using (var optimizationPreferencesDialog =
					new OptimizationPreferencesDialog(_optimizationPreferences, _groupPagesProvider,
						_schedulerState.CommonStateHolder.ActiveScheduleTags,
						_schedulerState.CommonStateHolder.ActiveActivities,
						SchedulerState.DefaultSegmentLength, _schedulerState.Schedules,
						_scheduleView.AllSelectedPersons(selectedSchedules), _daysOffPreferences))
				{
					if (optimizationPreferencesDialog.ShowDialog(this) == DialogResult.OK)
					{
						var optimizationPreferences = new SchedulingAndOptimizeArgument(selectedSchedules)
						{
							OptimizationMethod = OptimizationMethod.Optimize,
							DaysOffPreferences = _daysOffPreferences
						};

						startBackgroundScheduleWork(_backgroundWorkerOptimization, optimizationPreferences, false);
					}
				}
			}
		}

		private void toolStripButtonChartPeriodViewClick(object sender, EventArgs e)
		{
			var button = sender as ToolStripButton;
			if (button == toolStripButtonChartPeriodView) _skillResultViewSetting = SkillResultViewSetting.Period;
			if (button == toolStripButtonChartMonthView) _skillResultViewSetting = SkillResultViewSetting.Month;
			if (button == toolStripButtonChartWeekView) _skillResultViewSetting = SkillResultViewSetting.Week;
			if (button == toolStripButtonChartDayView) _skillResultViewSetting = SkillResultViewSetting.Day;
			if (button == toolStripButtonChartIntradayView) _skillResultViewSetting = SkillResultViewSetting.Intraday;

			((ToolStripMenuItem) _contextMenuSkillGrid.Items["IntraDay"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			((ToolStripMenuItem) _contextMenuSkillGrid.Items["Day"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			((ToolStripMenuItem) _contextMenuSkillGrid.Items["Period"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			((ToolStripMenuItem) _contextMenuSkillGrid.Items["Month"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			((ToolStripMenuItem) _contextMenuSkillGrid.Items["Week"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Week);

			if (toolStripButtonChartIntradayView != null)
				toolStripButtonChartIntradayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			if (toolStripButtonChartDayView != null)
				toolStripButtonChartDayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			if (toolStripButtonChartPeriodView != null)
				toolStripButtonChartPeriodView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			if (toolStripButtonChartMonthView != null)
				toolStripButtonChartMonthView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			if (toolStripButtonChartWeekView != null)
				toolStripButtonChartWeekView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week);

			_currentSelectedGridRow = null;

			drawSkillGrid();
			reloadChart();
		}

		private void toolStripButtonZoomClick(object sender, EventArgs e)
		{
			var button = sender as ToolStripButton;
			if (button != null && button.Tag != null) zoom((ZoomLevel) button.Tag);
			else
			{
				var quickButton = sender as QuickButtonReflectable;
				if (quickButton != null && quickButton.ReflectedButton.Tag != null)
					zoom((ZoomLevel) quickButton.ReflectedButton.Tag);
			}
		}

		private void changeRequestStatus(IHandlePersonRequestCommand command,
			IList<PersonRequestViewModel> requestViewAdapters)
		{
			_requestPresenter.ApproveOrDeny(requestViewAdapters, command, string.Empty);
			recalculateResourcesForRequests(requestViewAdapters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void toolStripMenuItemSwapAndRescheduleClick(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				swapSelectedSchedules();

				var bestShiftChooser = _container.Resolve<BestShiftChooser>();
				var schedulingOptions = new SchedulingOptions {UseRotations = false};
				var finderService = _container.Resolve<IWorkShiftFinderService>();
				// This is not working now I presume (SelectedSchedules is probably not correct)
				foreach (IScheduleDay schedulePart in _scheduleView.SelectedSchedules())
				{
					if (!schedulePart.HasDayOff())
					{
						IEditableShift selectedShift = bestShiftChooser.PrepareAndChooseBestShift(schedulePart, schedulingOptions,
							finderService);
						if (selectedShift != null)
						{
							schedulePart.AddMainShift(selectedShift);
							_scheduleView.Presenter.ModifySchedulePart(new List<IScheduleDay> {schedulePart});
						}
					}
				}
				RecalculateResources();
			}
		}

		private void toolStripMenuItemSwapClick(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				swapSelectedSchedules();
				Refresh();
				RefreshSelection();
			}
		}

		private void swapRaw()
		{
			var swapper = new Swapper(_scheduleView, _undoRedo, _schedulerState, _gridLockManager, this, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
			swapper.SwapRaw();
		}

		private void swapSelectedSchedules()
		{
			var swapper = new Swapper(_scheduleView, _undoRedo, _schedulerState, _gridLockManager, this, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
			swapper.SwapSelectedSchedules(_handleBusinessRuleResponse, _overriddenBusinessRulesHolder);
		}

		private void toolStripMenuItemScheduleClick(object sender, EventArgs e)
		{
			scheduleSelected(false);
		}

		private void toolStripMenuItemScheduleSelectedClick(object sender, EventArgs e)
		{
			scheduleSelected(false);
		}

		private void toolStripButtonMainMenuSaveClick(object sender, EventArgs e)
		{
			save();
		}

		private void toolStripButtonMainMenuHelpClick(object sender, EventArgs e)
		{
			ViewBase.ShowHelp(this, false);
		}

		private void toolStripButtonMainMenuCloseClick(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripButtonSystemExitClick(object sender, EventArgs e)
		{
			if (!CloseAllOtherForms(this))
				return; // a form was canceled

			Close();
			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

		private void toolStripButtonQuickAccessCancelClick(object sender, EventArgs e)
		{
			//cancelAllBackgroundWorkers();
			_cancelButtonPressed = true;
			toolStripButtonQuickAccessCancel.Enabled = false;
		}

		// the loop is used so the form would not close before all backgroundworkers are canceled
		// have to use Application.DoEvents(); here. Else the loop will continue for ever
		private void cancelAllBackgroundWorkers()
		{
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancellingThreeDots");

			cancelBackgroundWorker(_backgroundWorkerValidatePersons);
			cancelBackgroundWorker(_backgroundWorkerResourceCalculator);
			cancelBackgroundWorker(backgroundWorkerLoadData);
			cancelBackgroundWorker(_backgroundWorkerDelete);
			cancelBackgroundWorker(_backgroundWorkerScheduling);
			cancelBackgroundWorker(_backgroundWorkerOvertimeScheduling);
			cancelBackgroundWorker(_backgroundWorkerOptimization);
		}

		private void cancelBackgroundWorker(BackgroundWorker worker)
		{
			if (worker.IsBusy)
			{
				worker.CancelAsync();
				while (worker.IsBusy)
				{
					Application.DoEvents();
					Thread.Sleep(10);
				}
			}
		}

		#region Insert

		private void toolStripButtonInsertDayOffClick(object sender, EventArgs e)
		{
			addDayOff();
		}

		private void toolStripMenuItemAddActivityClick(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Shift);
		}

		private void toolStripMenuItemAddOverTimeClick(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Overtime);
		}

		private void toolStripMenuItemInsertAbsenceClick(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Absence);
		}

		private void toolStripMenuItemAddPersonalActivityClick(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.PersonalShift);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void addDayOff()
		{
			if (_scheduleView != null)
			{
				if (SchedulerState.FilteredCombinedAgentsDictionary.Count > 0)
				{
					IList<IDayOffTemplate> displayList = _schedulerState.CommonStateHolder.ActiveDayOffs.ToList();
					if (displayList.Count > 0)
					{
						// todo: remove comment when the user warning is ready for the other activities(delete, paste, swap etc.)
						var clone =
							(IScheduleDay)
								SchedulerState.Schedules[SchedulerState.FilteredCombinedAgentsDictionary.ElementAt(0).Value].
									ScheduledDay(new DateOnly(DateTime.MinValue.AddDays(1))).Clone();
						var selectedSchedules = _scheduleView.SelectedSchedules();
						if (!selectedSchedules.Any())
							return;

						var sortedList = selectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly.Date).OrderBy(d => d);

						var first = sortedList.FirstOrDefault();
						var last = sortedList.LastOrDefault();
						var period =
							new DateTimePeriod(
								DateTime.SpecifyKind(TimeZoneHelper.ConvertFromUtc(first, TimeZoneGuard.Instance.TimeZone), DateTimeKind.Utc),
								DateTime.SpecifyKind(TimeZoneHelper.ConvertFromUtc(last.AddDays(1), TimeZoneGuard.Instance.TimeZone),
									DateTimeKind.Utc));
						var addDayOffDialog = _scheduleView.CreateAddDayOffViewModel(displayList, TimeZoneGuard.Instance.TimeZone, period);

						if (!addDayOffDialog.Result)
							return;

						var dayOffTemplate = addDayOffDialog.SelectedItem;
						clone.PersonAssignment(true).SetDayOff(dayOffTemplate);
						_scheduleView.Presenter.ClipHandlerSchedule.Clear();
						_scheduleView.Presenter.ClipHandlerSchedule.AddClip(1, 1, clone);
						_externalExceptionHandler.AttemptToUseExternalResource(
							() => Clipboard.SetData("PersistableScheduleData", new int()));
						_cutPasteHandlerFactory.For(_controlType).PasteDayOff();
						_scheduleView.Presenter.ClipHandlerSchedule.Clear();
					}
				}
			}
		}

		#endregion

		private void toolStripMenuItemCopyClick(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Copy();
		}

		private void enablePasteOperation()
		{
			if (_clipboardControl != null)
				_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
			if (_clipboardControlRestrictions != null)
				_clipboardControlRestrictions.SetButtonState(ClipboardAction.Paste, true);
		}

		private void toolStripButtonDeleteClick(object sender, EventArgs e)
		{
			editControlDeleteClicked(sender, e);
		}

		private void toolStripMenuItemDeleteSpecial2Click(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).DeleteSpecial();
		}

		#region Lock

		private void toolStripMenuItemLockAllTagsMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;

			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayTagExtractor scheduleDayTagExtractor =
				new ScheduleDayTagExtractor(gridSchedulesExtractor.Extract());
			var gridlockAllTagsCommand = new GridlockAllTagsCommand(LockManager, scheduleDayTagExtractor);
			gridlockAllTagsCommand.Execute();

			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockTag(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var scheduleTag = (IScheduleTag) (((ToolStripMenuItem) (sender)).Tag);
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayTagExtractor scheduleDayTagExtractor = new ScheduleDayTagExtractor(gridSchedulesExtractor.Extract());
			var gridlockTagCommand = new GridlockTagCommand(LockManager, scheduleDayTagExtractor, scheduleTag);
			gridlockTagCommand.Execute();
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripComboBoxAutoTagSelectedIndexChanged(object sender, EventArgs e)
		{
			_defaultScheduleTag = (IScheduleTag) toolStripComboBoxAutoTag.SelectedItem;
			_scheduleView.Presenter.DefaultScheduleTag = _defaultScheduleTag;
		}

		private void toolStripMenuItemChangeTag(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var scheduleTag = (IScheduleTag) (((ToolStripMenuItem) (sender)).Tag);
			var gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			var setTagCommand = new SetTagCommand(_undoRedo, gridSchedulesExtractor, _scheduleView.Presenter, _scheduleView,
				scheduleTag, LockManager);
			_backgroundWorkerRunning = true;
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXScheduleTags");
			statusStrip1.Refresh();
			setTagCommand.Execute();
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXReady");
			statusStrip1.Refresh();
			_backgroundWorkerRunning = false;
			updateSelectionInfo(gridSchedulesExtractor.ExtractSelected());
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripSplitButtonLockButtonClick(object sender, EventArgs e)
		{
			lockSelection();
		}

		private void toolStripMenuItemLockSelectionClick(object sender, EventArgs e)
		{
			lockSelection();
		}

		private void lockSelection()
		{
			GridHelper.GridlockSelection(_grid, LockManager);
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemLockAbsenceDaysClick(object sender, EventArgs e)
		{
			lockAllAbsences();
		}

		private void toolStripMenuItemLockAbsenceDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllAbsences();
		}

		private void lockAllAbsences()
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllAbsences(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockFreeDaysClick(object sender, EventArgs e)
		{
			lockAllDaysOff();
		}

		private void toolStripMenuItemDayOffLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllDaysOff();
		}

		private void lockAllDaysOff()
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockFreeDays(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockSpecificDayOffClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var dayOffTemplate = (IDayOffTemplate) (((ToolStripMenuItem) (sender)).Tag);
			GridHelper.GridlockSpecificDayOff(_grid, LockManager, dayOffTemplate);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockAbsencesClick(object sender, EventArgs e)
		{
			lockAbsence(sender);
		}

		private void toolStripMenuItemAbsenceLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAbsence(sender);
		}

		private void lockAbsence(object sender)
		{
			Cursor = Cursors.WaitCursor;
			var absence = (Absence) (((ToolStripMenuItem) (sender)).Tag);
			GridHelper.GridlockAbsences(_grid, LockManager, absence);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockShiftCategoriesClick(object sender, EventArgs e)
		{
			lockShiftCategory(sender);
		}

		private void toolStripMenuItemLockShiftCategoriesMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockShiftCategory(sender);
		}

		private void lockShiftCategory(object sender)
		{
			Cursor = Cursors.WaitCursor;
			var shiftCategory = (ShiftCategory) (((ToolStripMenuItem) (sender)).Tag);
			GridHelper.GridlockShiftCategories(_grid, LockManager, shiftCategory);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockShiftCategoryDaysClick(object sender, EventArgs e)
		{
			lockAllShiftCategories();
		}

		private void toolStripMenuItemLockShiftCategoryDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllShiftCategories();
		}

		private void lockAllShiftCategories()
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllShiftCategories(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemNotifyAgentClick(object sender, EventArgs e)
		{
			var builder = new StringBuilder();
			var agents = _scheduleView.AllSelectedPersons(_scheduleView.SelectedSchedules()).ToArray();

			foreach (var agent in agents)
			{
				builder.Append(agent.Id);
				if (agent != agents.Last())
					builder.Append(",");
			}

			var url = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle") + "Messages#" + builder;
			if (url.IsAnUrl())
				Process.Start(url);
		}

		private void toolStripMenuItemLockAllRestrictionsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.LockAllRestrictions(e.Button);
		}

		private void toolStripMenuItemAllPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllPreferences(e.Button);
		}

		private void toolStripMenuItemAllDaysOffMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllDaysOff(e.Button);
		}

		private void toolStripMenuItemAllShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllShiftsPreferences(e.Button);
		}

		private void toolStripMenuItemAllMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllMustHave(e.Button);
		}

		private void toolStripMenuItemAllFulfilledMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledMustHave(e.Button);
		}

		private void toolStripMenuItemAllFulFilledPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledPreferences(e.Button);
		}

		private void toolStripMenuItemAllAbsencePreferenceMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllAbsencePreference(e.Button);
		}

		private void toolStripMenuItemAllFulFilledAbsencesPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledAbsencesPreferences(e.Button);
		}

		private void toolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledDaysOffPreferences(e.Button);
		}

		private void toolStripMenuItemAllFulFilledShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledShiftsPreferences(e.Button);
		}

		private void toolStripMenuItemAllRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllRotations(e.Button);
		}

		private void toolStripMenuItemAllDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllDaysOffRotations(e.Button);
		}

		private void toolStripMenuItemAllShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllShiftsRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledDaysOffRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledShiftsRotations(e.Button);
		}

		private void toolStripMenuItemAllUnavailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllUnavailableStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllAvailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllAvailableStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllFulFilledStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllUnavailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllUnavailableAvailability(e.Button);
		}

		private void toolStripMenuItemAllAvailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllAvailableAvailability(e.Button);
		}

		private void toolStripMenuItemAllFulFilledAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, LockManager, this);
			executer.AllFulfilledAvailability(e.Button);
		}

		#endregion

		private void toolStripMenuItemPasteClick(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Paste();
			updateShiftEditor();
		}

		private void toolStripMenuItemPasteSpecial2Click(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).PasteSpecial();
			updateShiftEditor();
		}

		private void toolStripMenuItemPasteShiftFromShiftsClick(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).PasteShiftFromShifts();
		}

		private void pasteFromClipboard(PasteOptions options)
		{
			_backgroundWorkerRunning = true;
			_scheduleView.GridClipboardPaste(options, _undoRedo);
			_backgroundWorkerRunning = false;
			RecalculateResources();
		}

		#endregion

		#region Context menu events

		private void contextMenuViewsOpening(object sender, CancelEventArgs e)
		{
			if (_scheduleView == null)
				e.Cancel = true;

			ToolStripMenuItemCreateMeeting.Enabled =
				toolStripMenuItemDeleteMeeting.Enabled =
					toolStripMenuItemRemoveParticipant.Enabled =
						_permissionHelper.IsPermittedToEditMeeting(_scheduleView, _temporarySelectedEntitiesFromTreeView, _scenario);
			toolStripMenuItemMeetingOrganizer.Enabled =
				toolStripMenuItemEditMeeting.Enabled =
					_permissionHelper.IsPermittedToViewMeeting(_scheduleView, _temporarySelectedEntitiesFromTreeView);
			toolStripMenuItemWriteProtectSchedule.Enabled =
				toolStripMenuItemWriteProtectSchedule2.Enabled =
					_permissionHelper.IsPermittedToWriteProtect(_scheduleView, _temporarySelectedEntitiesFromTreeView);

			toolStripMenuItemViewHistory.Enabled = false;
			if (_scenario.DefaultScenario)
				toolStripMenuItemViewHistory.Enabled = _isAuditingSchedules;

			toolStripMenuItemSwitchToViewPointOfSelectedAgent.Enabled = _scheduleView.SelectedSchedules().Any();
		}

		#region Virtual skill handling

		private void updateSkillGridMenuItem()
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, _skillResultViewSetting,
				toolStripButtonChartPeriodView, toolStripButtonChartMonthView,
				toolStripButtonChartWeekView, toolStripButtonChartDayView,
				toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void skillGridMenuItemPeriodClick(object sender, EventArgs e)
		{
			_skillResultViewSetting = SkillResultViewSetting.Period;
			updateSkillGridMenuItem();
		}

		private void skillGridMenuItemMonthClick(object sender, EventArgs e)
		{
			_skillResultViewSetting = SkillResultViewSetting.Month;
			updateSkillGridMenuItem();
		}

		private void skillGridMenuItemWeekClick(object sender, EventArgs e)
		{
			_skillResultViewSetting = SkillResultViewSetting.Week;
			updateSkillGridMenuItem();
		}

		private void skillGridMenuItemDayClick(object sender, EventArgs e)
		{
			_skillResultViewSetting = SkillResultViewSetting.Day;
			updateSkillGridMenuItem();
		}

		private void skillGridMenuItemIntraDayClick(object sender, EventArgs e)
		{
			_skillResultViewSetting = SkillResultViewSetting.Intraday;
			updateSkillGridMenuItem();
		}

		private void skillGridMenuItemClick(object sender, EventArgs e)
		{
			using (var skillSummery = new SkillSummary(_schedulerState.SchedulingResultState.Skills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					handleCreateSummeryMenuItems(skillSummery);
				}
			}
		}

		private void skillGridMenuItemEditClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem) sender;
			var skill = (ISkill) menuItem.Tag;

			using (var skillSummery = new SkillSummary(skill, _schedulerState.SchedulingResultState.Skills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					IAggregateSkill newSkill = handleSummeryEditMenuItems(menuItem, skillSummery);

					if (newSkill.AggregateSkills.Count != 0)
					{
						_virtualSkillHelper.EditAndRenameVirtualSkill(newSkill, skill.Name);
						schedulerSplitters1.ReplaceOldWithNew((ISkill) newSkill, skill);
						schedulerSplitters1.SortSkills();
						if (_tabSkillData.SelectedTab.Tag == newSkill)
							drawSkillGrid();
					}
					else
					{
						removeVirtualSkill(newSkill);
					}
				}
			}
		}

		private void skillGridMenuItemDeleteClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem) sender;
			var virtualSkill = (IAggregateSkill) menuItem.Tag;
			removeVirtualSkill(virtualSkill);
		}

		private void removeVirtualSkill(IAggregateSkill virtualSkill)
		{
			virtualSkill.ClearAggregateSkill();
			schedulerSplitters1.RemoveVirtualSkill((Skill) virtualSkill);
			foreach (TabPageAdv tabPage in _tabSkillData.TabPages)
			{
				if (tabPage.Tag == virtualSkill)
				{
					removeVirtualSkillToolStripMenuItem(tabPage, virtualSkill, "Delete");
					removeVirtualSkillToolStripMenuItem(tabPage, virtualSkill, "Edit");
					break;
				}
			}
			_virtualSkillHelper.SaveVirtualSkill(virtualSkill);
		}

		private void removeVirtualSkillToolStripMenuItem(TabPageAdv tabPage, IAggregateSkill virtualSkill, string action)
		{
			var skillGridMenuItem = (ToolStripMenuItem) _contextMenuSkillGrid.Items[action];
			_tabSkillData.TabPages.Remove(tabPage);
			foreach (ToolStripMenuItem subItem in skillGridMenuItem.DropDownItems)
			{
				if (subItem.Tag == virtualSkill)
				{
					skillGridMenuItem.DropDownItems.Remove(subItem);
					if (skillGridMenuItem.DropDownItems.Count == 0)
						_contextMenuSkillGrid.Items[action].Enabled = false;
					break;
				}
			}
		}

		private void handleCreateSummeryMenuItems(SkillSummary skillSummary)
		{
			var virtualSkill = (ISkill) skillSummary.AggregateSkillSkill;
			virtualSkill.SetId(Guid.NewGuid());
			TabPageAdv tab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
			tab.ImageIndex = 4;
			tab.Tag = skillSummary.AggregateSkillSkill;
			_tabSkillData.TabPages.Add(tab);
			_virtualSkillHelper.SaveVirtualSkill(virtualSkill);
			schedulerSplitters1.AddVirtualSkill(virtualSkill);
			schedulerSplitters1.SortSkills();
			enableEditVirtualSkill(virtualSkill);
			enableDeleteVirtualSkill(virtualSkill);
		}

		private void enableEditVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem) _contextMenuSkillGrid.Items["Edit"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemEditClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private void enableDeleteVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem) _contextMenuSkillGrid.Items["Delete"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemDeleteClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private IAggregateSkill handleSummeryEditMenuItems(ToolStripMenuItem menuItem, SkillSummary skillSummary)
		{
			var virtualSkill = (ISkill) skillSummary.AggregateSkillSkill;
			_tabSkillData.SelectedTab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
			foreach (TabPageAdv tabPage in _tabSkillData.TabPages)
			{
				handleTabsAndMenuItemsVirtualSkill(skillSummary, virtualSkill, tabPage, menuItem);
			}
			return virtualSkill;
		}

		private void handleTabsAndMenuItemsVirtualSkill(SkillSummary skillSummary, ISkill virtualSkill, TabPageAdv tabPage,
			ToolStripMenuItem menuItem)
		{
			if (tabPage.Tag == virtualSkill)
			{
				if (skillSummary.AggregateSkillSkill.AggregateSkills.Count == 0)
				{
					removeVirtualSkillToolStripMenuItem(tabPage, virtualSkill, "Edit");
					removeVirtualSkillToolStripMenuItem(tabPage, virtualSkill, "Delete");
					return;
				}
				tabPage.Text = virtualSkill.Name;
				menuItem.Name = virtualSkill.Name;
				menuItem.Text = virtualSkill.Name;
				var skillGridMenuItem = (ToolStripMenuItem) _contextMenuSkillGrid.Items["Delete"];
				foreach (ToolStripMenuItem subItem in skillGridMenuItem.DropDownItems)
				{
					if (subItem.Tag == virtualSkill)
					{
						subItem.Name = virtualSkill.Name;
						subItem.Text = virtualSkill.Name;
						break;
					}
				}
			}
		}

		#endregion//Virtual skill handling

		#endregion//other

		#endregion

		#region Gridevents

		private void gridCurrentCellKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				if (!(_scheduleView is AgentRestrictionsDetailView))
				{
					GridHelper.HandleSelectAllSchedulingView((GridControl) sender);
					return;
				}
			}

			GridHelper.HandleSelectionKeys((GridControl) sender, e);
		}

		private void currentViewViewPasteCompleted(object sender, EventArgs e)
		{
			RecalculateResources();
			_grid.Invalidate();
		}

		public void RecalculateResources()
		{
			if (_backgroundWorkerRunning) return;

			if (_backgroundWorkerResourceCalculator.IsBusy)
				return;

			var daysToRecalculate = _schedulerState.DaysToRecalculate;
			var numberOfDaysToRecalculate = daysToRecalculate.Count();
			if (numberOfDaysToRecalculate == 0 && _uIEnabled)
				return;

			if ((_schedulerState.SchedulingResultState.SkipResourceCalculation || _teamLeaderMode) && _uIEnabled)
			{
				validatePersons();
				_schedulerState.ClearDaysToRecalculate();
				return;
			}

			disableAllExceptCancelInRibbon();

			if (toolStripProgressBar1.CustomLabelControl == null) //Somone knows why this is null in some cases?
				toolStripProgressBar1 = new MetroToolStripProgressBar();

			toolStripProgressBar1.Maximum = numberOfDaysToRecalculate;
			toolStripProgressBar1.Value = 0;

			toolStripProgressBar1.Visible = true;
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCalculatingResourcesDotDotDot");

			_backgroundWorkerResourceCalculator.WorkerReportsProgress = true;
			_backgroundWorkerRunning = true;
			_backgroundWorkerResourceCalculator.RunWorkerAsync();
		}

		private void backgroundWorkerResourceCalculatorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//updateDistrbutionInformation(TODO);

			if (Disposing)
				return;

			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;

			if (rethrowBackgroundException(e))
				return;

			toolStripProgressBar1.Visible = false;
			if (e.Cancelled)
			{
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancel");
				releaseUserInterface(e.Cancelled);
				return;
			}
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXReady");

			if (_personsToValidate.IsEmpty())
			{
				afterBackgroundWorkersCompleted(e.Cancelled);
				return;
			}		

			validatePersons();
		}

		private void backgroundWorkerResourceCalculatorProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;

			var progress = e.UserState as ResourceOptimizerProgressEventArgs;
			if (progress != null)
			{
				if (_cancelButtonPressed)
				{
					progress.Cancel = true;
					progress.CancelAction();
				}
			}

			toolStripProgressBar1.PerformStep();
		}

		private void backgroundWorkerResourceCalculatorDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			if (!_schedulerState.SchedulingResultState.Skills.Any()) return;
			if (!_schedulerState.DaysToRecalculate.Any()) return;
			if (_schedulerState.SchedulingResultState.SkillDays == null) return;

			using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(new DateOnlyPeriod(_schedulerState.DaysToRecalculate.Min(), _schedulerState.DaysToRecalculate.Max())))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new BackgroundWorkerWrapper(_backgroundWorkerResourceCalculator), SchedulerState.ConsiderShortBreaks, true);
			}
		}

		private void validateAllPersons()
		{
			_personsToValidate.Clear();
			_schedulerState.AllPermittedPersons.ForEach(_personsToValidate.Add);
			validatePersons();
		}

		private void validatePersons()
		{
			if (_backgroundWorkerRunning) return;
			disableAllExceptCancelInRibbon();
			scheduleStatusBarUpdate(string.Format(CultureInfo.CurrentCulture,
				LanguageResourceHelper.Translate("XXValidatingPersons"), _personsToValidate.Count));
			_backgroundWorkerRunning = true;
			_backgroundWorkerValidatePersons.RunWorkerAsync();
			Application.DoEvents();
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals",
			MessageId = "result")]
		private void backgroundWorkerValidatePersonsDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			if (_scheduleView != null)
			{
				_scheduleView.ValidatePersons(_personsToValidate);
			}
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}

		private void backgroundWorkerValidatePersonsRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;

			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;

			if (rethrowBackgroundException(e))
				return;

			scheduleStatusBarUpdate(LanguageResourceHelper.Translate("XXLoadingFormThreeDots"));
			afterBackgroundWorkersCompleted(e.Cancelled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		private void afterBackgroundWorkersCompleted(bool canceled)
		{
			_personsToValidate.Clear();

			using (PerformanceOutput.ForOperation("After validate"))
			{
				updateShiftEditor();
				if (_requestView != null)
					_requestView.NeedUpdate = true;
				reloadRequestView();
				if (_currentZoomLevel == ZoomLevel.RestrictionView)
				{
					schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions,
						_restrictionPersonsToReload);
					_restrictionPersonsToReload.Clear();
				}

				drawSkillGrid();
			}
			releaseUserInterface(canceled);
			if (!_scheduleOptimizerHelper.WorkShiftFinderResultHolder.LastResultIsSuccessful)
			{
				var workShiftFinderResultHolder = _scheduleOptimizerHelper.WorkShiftFinderResultHolder;
				if (_optimizerOriginalPreferences.SchedulingOptions.ShowTroubleshot ||
					workShiftFinderResultHolder.AlwaysShowTroubleshoot)
					new SchedulingResult(workShiftFinderResultHolder, true, _schedulerState.CommonNameDescription).Show(this);
				else
					ViewBase.ShowInformationMessage(this,
						string.Format(CultureInfo.CurrentCulture, Resources.NoOfAgentDaysCouldNotBeScheduled,
							_scheduleOptimizerHelper.WorkShiftFinderResultHolder.GetResults(false, true).Count)
						, Resources.SchedulingResult);
			}
			_scheduleOptimizerHelper.ResetWorkShiftFinderResults();

			if (SikuliHelper.InteractiveMode)
			{
				var skillTabPage = _tabSkillData.TabPages[0];
				var totalSkill = skillTabPage.Tag as IAggregateSkill;
				var currentValidator = SikuliHelper.CurrentValidator;

				if (currentValidator != null)
					SikuliHelper.Validate(currentValidator, this, new SchedulerTestData(_schedulerState, totalSkill));
				else
					SikuliHelper.ShowTaskDoneView(this);
			}

			var agentsDictionary = _schedulerState.FilteredCombinedAgentsDictionary;
			if (agentsDictionary.Count == 0)
			{
				_schedulerState.ResetFilteredPersons();
				agentsDictionary = _schedulerState.FilteredCombinedAgentsDictionary;
			}
			schedulerSplitters1.RefreshTabInfoPanels(agentsDictionary.Values);
		}

		private GridRangeInfo _lastGridSelection;
		private bool gridSelectionChangedRunning;
		private void gridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (e.Range == _lastGridSelection)
				return;

			if (_scheduleView == null)
				return;

			if (gridSelectionChangedRunning)
				return;

			using (PerformanceOutput.ForOperation("Changing selection in view"))
			{
				if (_scheduleView != null &&
					(e.Reason == GridSelectionReason.SetCurrentCell || e.Reason == GridSelectionReason.MouseUp) ||
					e.Reason == GridSelectionReason.ArrowKey || e.Reason == GridSelectionReason.SelectRange)
				{
					gridSelectionChangedRunning = true;

					SchedulerRibbonHelper.EnableScheduleButton(toolStripSplitButtonSchedule, _scheduleView, _splitterManager,
						_teamLeaderMode);

					disableButtonsIfTeamLeaderMode();
					_scheduleView.Presenter.UpdateFromEditor();
					if (_showEditor)
					{
						updateShiftEditor();
						RunActionWithDelay(updateShiftEditor, 50);
					}
					var currentCell = _scheduleView.ViewGrid.CurrentCell;
					var selectedCols = _scheduleView.ViewGrid.Model.Selections.Ranges.ActiveRange.Width;
					if (!(_scheduleView is AgentRestrictionsDetailView) && currentCell.RowIndex == 0 && selectedCols == 1 &&
						currentCell.ColIndex >= (int) ColumnType.StartScheduleColumns)
					{
						_scheduleView.AddWholeWeekAsSelected(currentCell.RowIndex, currentCell.ColIndex);
					}
					
					var selectedDate = _scheduleView.SelectedDateLocal();
					if (_currentIntraDayDate != selectedDate)
					{
						if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
						{
							drawSkillGrid();
							reloadChart();
						}

						_currentIntraDayDate = selectedDate;
					}

					var selectedSchedules = _scheduleView.SelectedSchedules();
					updateSelectionInfo(selectedSchedules);
					enableSwapButtons(selectedSchedules);
					if (selectedSchedules.Any())
						_dateNavigateControl.SetSelectedDateNoInvoke(selectedSchedules[0].DateOnlyAsPeriod.DateOnly);

					_lastGridSelection = e.Range;
				}
			}
			gridSelectionChangedRunning = false;
		}

		private void saveAllChartSetting()
		{
			_skillIntradayGridControl.SaveSetting();
			_skillDayGridControl.SaveSetting();
			_skillWeekGridControl.SaveSetting();
			_skillMonthGridControl.SaveSetting();
			_skillFullPeriodGridControl.SaveSetting();
		}

		public async void RunActionWithDelay(System.Action action, int milliseconds)
		{
			await System.Threading.Tasks.Task.Delay(milliseconds);
			action();
		}

		private void updateShiftEditor()
		{
			if (_scheduleView == null) return;

			using (PerformanceOutput.ForOperation("Updating shift editor"))
			{
                toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXSelectedAgentsColon") + " " +
                                                      _scheduleView.NumberOfSelectedPersons() + " " + 
                                                      LanguageResourceHelper.Translate("XXAgentsColon") + " " +
                                                      _schedulerState.FilteredCombinedAgentsDictionary.Count + " " +
                                                      LanguageResourceHelper.Translate("XXLoadedColon") +
                                                      " " + _schedulerState.SchedulingResultState.PersonsInOrganization.Count;

                notesEditor.LoadNote(null);

				var scheduleDay = getSelectedScheduleDayForShiftEditor();

				if (scheduleDay == null)
				{
					SplitterManager.DisableShiftEditor();
					return;
				}

				SplitterManager.EnableShiftEditor();

				scheduleDay = _schedulerState.Schedules[scheduleDay.Person].ReFetch(scheduleDay);

				if (_showEditor)
				{
					schedulePartToEditor(scheduleDay);
				}

				checkEditable(_scheduleView.PartIsEditable());
				if (scheduleDay != null)
				{
					schedulerSplitters1.MultipleHostControl3.UpdateItems();
					if (!(_scheduleView is DayViewNew))
					{
						_scheduleView.SetSelectedDateLocal(scheduleDay.DateOnlyAsPeriod.DateOnly);
					}

				}				
			}
		}

		private IScheduleDay getSelectedScheduleDayForShiftEditor()
		{
			var getFirstDataColumnIndex = new Func<ScheduleViewBase, int>((scheduleView) =>
			{
				int colIndex = scheduleView.ViewGrid.CurrentCell.ColIndex;
				if (colIndex < scheduleView.ViewGrid.Cols.FrozenCount)
					colIndex = Math.Min(scheduleView.ViewGrid.ColCount, scheduleView.ViewGrid.Cols.FrozenCount + 1);
				return colIndex;
			});

			// in case of DayViewNew the selected schedule day is according to the first datacolumn
			if (_scheduleView is DayViewNew)
			{
				var firstDataColumnIndex = getFirstDataColumnIndex(_scheduleView);
				var scheduleDay = _scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, firstDataColumnIndex]
						.CellValue as IScheduleDay;
				return scheduleDay;
			}
			return _scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, _scheduleView.ViewGrid.CurrentCell.ColIndex]
					.CellValue as IScheduleDay;
		}

		private void schedulePartToEditor(IScheduleDay part)
		{
			wpfShiftEditor1.LoadSchedulePart(part);
			notesEditor.LoadNote(part);
		}

		private void checkEditable(bool isEditable)
		{
			schedulerSplitters1.ElementHost1.Enabled = isEditable;
			toolStripMenuItemPaste.Enabled = isEditable;
			toolStripMenuItemPasteSpecial.Enabled = isEditable;
			toolStripMenuItemCopy.Enabled = isEditable;
			toolStripMenuItemCut.Enabled = isEditable;
			toolStripMenuItemCutSpecial.Enabled = isEditable;
			toolStripMenuItemDelete.Enabled = isEditable;
			toolStripMenuItemDeleteSpecial.Enabled = isEditable;
			_editControl.Enabled = isEditable;
			_editControlRestrictions.Enabled = isEditable;
			_clipboardControl.Enabled = isEditable;
			_clipboardControlRestrictions.Enabled = isEditable;
			_permissionHelper.CheckModifyPermissions(ToolStripMenuItemAddActivity, toolStripMenuItemAddOverTime,
				toolStripMenuItemInsertAbsence, toolStripMenuItemInsertDayOff);
		}

		#endregion

		#region BackgroundWorkerLoadData events

		private void backgroundWorkerLoadDataProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;

			if(e.ProgressPercentage > 0)
				toolStripProgressBar1.PerformStep();		
			var status = e.UserState as string;
			if (status != null)
				toolStripStatusLabelStatus.Text = status;
			statusStrip1.Refresh();
		}

		private void backgroundWorkerLoadDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;

			if (stateHolderExceptionOccurred(e))
				return;

			if (datasourceExceptionOccurred(e))
				return;

			if (rethrowBackgroundException(e))
				return;

			if (Disposing)
				return;

			if (e.Cancelled)
			{
				backgroundWorkerLoadData.Dispose();
				Close();
				return;
			}

			SuspendLayout();
			setupAvailTimeZones();
			
			zoom(ZoomLevel.PeriodView);
			DateOnly dateOnly = SchedulerState.RequestedPeriod.DateOnlyPeriod.StartDate;
			_scheduleView.SetSelectedDateLocal(dateOnly);
			_scheduleView.ViewPasteCompleted += currentViewViewPasteCompleted;
			schedulerSplitters1.ElementHost1.Enabled = true;
			_splitContainerAdvMain.Visible = true;
			_grid.Cursor = Cursors.WaitCursor;
			wpfShiftEditor1.LoadFromStateHolder(_schedulerState.CommonStateHolder);
			wpfShiftEditor1.Interval = _currentSchedulingScreenSettings.EditorSnapToResolution;

			loadLockMenues();
			loadScenarioMenuItems();

			toolStripStatusLabelStatus.Text = "SETTING UP SKILL TABS...";
			ResumeLayout(true);
			Refresh();
			SuspendLayout();
			setupSkillTabs();

			toolStripStatusLabelStatus.Text = "SETTING UP INFO TABS...";
			Refresh();
			setupInfoTabs();
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXLoadingFormThreeDots");
			ResumeLayout(true);
			Refresh();
			SuspendLayout();

			if (schedulerSplitters1.PinnedPage != null)
				schedulerSplitters1.TabSkillData.SelectedTab = schedulerSplitters1.PinnedPage;

			toolStripStatusLabelScheduleTag.Visible = true;
			toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXAgentsColon") + " " +
													  _schedulerState.FilteredCombinedAgentsDictionary.Count + " " + 
													  LanguageResourceHelper.Translate("XXLoadedColon") +
													  " " + _schedulerState.SchedulingResultState.PersonsInOrganization.Count;
			toolStripStatusLabelNumberOfAgents.Visible = true;
			var loadedPeriod = _schedulerState.LoadedPeriod.Value.ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			setHeaderText(_schedulerState.RequestedPeriod.DateOnlyPeriod.StartDate, _schedulerState.RequestedPeriod.DateOnlyPeriod.EndDate, loadedPeriod.StartDate, loadedPeriod.EndDate);
			
			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _loadRequsts)
			{
				using (PerformanceOutput.ForOperation("Creating new RequestView"))
				{
					_requestView = new RequestView(_handlePersonRequestView1, _schedulerState, _undoRedo,
						SchedulerState.SchedulingResultState.AllPersonAccounts, _eventAggregator);
				}
				
				_requestView.PropertyChanged += _requestView_PropertyChanged;
				_requestView.SelectionChanged += requestViewSelectionChanged;
			}
			else
			{
				toolStripTabItem1.Visible = false;
			    toolStripButtonRequestView.Visible = false;
			}

			_grid.VScrollPixel = false;
			_grid.HScrollPixel = false;
			_grid.Selections.Clear(true);
			var point = new Point((int) ColumnType.StartScheduleColumns, _grid.Rows.HeaderCount + 1);
			_grid.CurrentCell.MoveTo(point.Y, point.X, GridSetCurrentCellOptions.None);
			_grid.Selections.SelectRange(GridRangeInfo.Cell(point.Y, point.X), true);
			_grid.Select();
			var schedulerSortCommandSetting = _currentSchedulingScreenSettings.SortCommandSetting;
			var sortCommandMapper = new SchedulerSortCommandMapper(SchedulerState, SchedulerSortCommandSetting.NoSortCommand,
				_container);
			var sortCommand = sortCommandMapper.GetCommandFromSetting(schedulerSortCommandSetting);
			_scheduleView.Sort(sortCommand);

			drawSkillGrid();
			reloadChart();
			setupRequestPresenter();
			setupRequestViewButtonStates();
			releaseUserInterface(e.Cancelled);
			ResumeLayout(true);
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXReadyThreeDots");
			Cursor = Cursors.Default;

			SikuliHelper.ShowLoadedView(this);
		}

		private void requestViewSelectionChanged(object sender, EventArgs e)
		{
			var request = _requestView.SelectedAdapters().FirstOrDefault();
			if (request == null) return;

			var localDate = TimeZoneHelper.ConvertFromUtc(request.FirstDateInRequest, TimeZoneHelper.CurrentSessionTimeZone);
			selectCellFromPersonDate(request.PersonRequest.Person, new DateOnly(localDate));
		}

		private void setupRequestViewButtonStates()
		{
			toolStripButtonViewAllowance.Available = _budgetPermissionService.IsAllowancePermitted ||
													 PrincipalAuthorization.Current()
														 .IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance);
			toolStripMenuItemViewAllowance.Visible = _budgetPermissionService.IsAllowancePermitted ||
													 PrincipalAuthorization.Current()
														 .IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance);
			toolStripMenuItemViewAllowance.Enabled = _budgetPermissionService.IsAllowancePermitted ||
													 PrincipalAuthorization.Current()
														 .IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance);
		}

		private bool stateHolderExceptionOccurred(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var sourceException = e.Error as StateHolderException;
				if (sourceException == null)
					return false;

				using (
					var view = new SimpleExceptionHandlerView(sourceException, Resources.OpenTeleoptiCCC, sourceException.Message))
				{
					view.ShowDialog();
				}

				_forceClose = true;
				Close();
				return true;
			}
			return false;
		}

		private bool datasourceExceptionOccurred(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var dataSourceException = e.Error ; //as CouldNotCreateTransactionException;
				if (dataSourceException == null)
					return false;

				using (
					var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC,
						Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				_forceClose = true;
				Close();
				return true;
			}
			return false;
		}

		private void displayOptionsFromSetting(SchedulingScreenSettings settings)
		{
			SplitterManager.ShowResult = !settings.HideResult;
			toolStripButtonShowResult.Checked = !settings.HideResult;
			_showResult = !settings.HideResult;
			SplitterManager.ShowGraph = !settings.HideGraph;
			toolStripButtonShowGraph.Checked = !settings.HideGraph;
			_showGraph = !settings.HideGraph;
			SplitterManager.ShowEditor = !settings.HideEditor;
			toolStripButtonShowEditor.Checked = !settings.HideEditor;
			_showEditor = !settings.HideEditor;
			_showInfoPanel = !settings.HideInfoPanel;
			toolStripButtonShowPropertyPanel.Checked = _showInfoPanel;

			toolStripButtonShowTexts.Checked = !settings.HideRibbonTexts;
			_showRibbonTexts = !settings.HideRibbonTexts;
			if (_teamLeaderMode)
			{
				SplitterManager.ShowGraph = false;
				SplitterManager.ShowResult = false;
			}
		}

		private void schedulerMessageBrokerHandlerSchedulesUpdatedFromBroker(object sender, EventArgs e)
		{
			if (toolStripTabItem1.Visible)
			{
				if (_requestView != null)
					_requestView.NeedUpdate = true;

				reloadRequestView();
			}
		}

		private void reloadRequestView()
		{
			if (_currentZoomLevel == ZoomLevel.RequestView)
			{
				if (_requestView != null && _requestView.NeedUpdate)
				{

					_requestView.UpdatePersonRequestViewModel();
					_requestView.NeedUpdate = false;
				}
				if (_requestView != null && _requestView.NeedReload)
				{
					_requestView.CreatePersonRequestViewModels(_schedulerState, _handlePersonRequestView1);
					_requestView.NeedReload = false;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private static bool rethrowBackgroundException(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}
			return false;
		}

		private void backgroundWorkerLoadDataDoWork(object sender, DoWorkEventArgs e)
		{
			// this is a workaround because if we press cancel before loadPeople has finished the scheduleingscreen will not unload
			// can't find out why / Ola
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, false);
			setThreadCulture();
			loadAndOptimizeData(e);
		}

		#endregion

		#region Other Events

		private void editMeeting()
		{
			if (_scheduleView != null)
			{
				int rowIndex = _scheduleView.ViewGrid.CurrentCell.RowIndex;
				int colIndex = _scheduleView.ViewGrid.CurrentCell.ColIndex;
				var dest = _scheduleView.ViewGrid[rowIndex, colIndex].CellValue as IScheduleDay;

				if (dest != null)
				{
					IMeeting meeting = _schedulerMeetingHelper.MeetingFromList(dest.Person,
						dest.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo), dest.PersonMeetingCollection());
					if (meeting != null)
					{
						meeting = meeting.EntityClone();
						//We don't want to work with the actual meeting, that will be a bad idea!
						IList<ITeam> meetingPersonsTeams = getDistinctTeamList(meeting);
						bool editPermission =
							_permissionHelper.HasFunctionPermissionForTeams(meetingPersonsTeams,
								DefinedRaptorApplicationFunctionPaths.ModifyMeetings) &&
							_permissionHelper.IsPermittedToEditMeeting(_scheduleView, _temporarySelectedEntitiesFromTreeView, _scenario);
						bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
						_schedulerMeetingHelper.MeetingComposerStart(meeting, _scheduleView, editPermission, viewSchedulesPermission,
							_container.Resolve<IToggleManager>());
					}
				}
			}
		}

		private IList<ITeam> getDistinctTeamList(IMeeting meeting)
		{
			IList<ITeam> teams = new List<ITeam>();
			foreach (IMeetingPerson meetingPerson in meeting.MeetingPersons)
			{
				bool quit = !SchedulerState.AllPermittedPersons.Contains(meetingPerson.Person);

				if (!quit)
				{
					ITeam team = meetingPerson.Person.MyTeam(meeting.StartDate);
					if (!teams.Contains(team))
					{
						teams.Add(team);
					}
				}
			}
			return teams;
		}

		private void deleteMeeting()
		{
			if (_scheduleView != null)
			{
				var selectedPersonMeetings = GridHelper.MeetingsFromSelection(_scheduleView.ViewGrid);

				IList<IMeeting> meetings = new List<IMeeting>();

				foreach (var selectedPersonMeeting in selectedPersonMeetings)
				{
					var meeting = selectedPersonMeeting.BelongsToMeeting;

					if (!meetings.Contains(meeting))
						meetings.Add(meeting);
				}

				foreach (var meeting in meetings)
				{
					_schedulerMeetingHelper.MeetingRemove(meeting, _scheduleView);
				}
			}
		}

		private bool _updating;

		private void gridGotFocus(object sender, EventArgs e)
		{
			if (_updating)
			{
				return;
			}
			_updating = true;
			updateRibbon(ControlType.SchedulerGridMain);

			_updating = false;
		}

		private void skillGridControlGotFucus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void wpfShiftEditor1ShiftUpdated(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView != null)
			{
				//Spara undan AgentDayInformatiom
				//Vid byte av cell i vy eller vid stäng eller lostfocus, så utför ändringarna.
				_scheduleView.Presenter.LastUnsavedSchedulePart = e.SchedulePart;
				//Trace.WriteLine("AgentDayInformation changed");
			}
		}

		private void wpfShiftEditor1CommitChanges(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView != null)
			{
				var negativeOrderIndex = haveNegativeOrderIndex();
				_scheduleView.Presenter.UpdateFromEditor();
				if (_currentZoomLevel == ZoomLevel.RestrictionView)
					schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions);

				if (_currentZoomLevel == ZoomLevel.DayView && !(_scheduleView.Presenter.SortCommand is NoSortCommand))
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> {e.SchedulePart});

				updateShiftEditor();

				if (negativeOrderIndex)
				{
					_forceClose = true;
					Close();
				}
			}
		}

		[RemoveMeWithToggle("Remove this method when toggle is removed. Just a temp check to haunt bug #39062", Toggles.ResourcePlanner_NegativeOrderIndex_39062)]
		private bool haveNegativeOrderIndex()
		{
			if (!_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_NegativeOrderIndex_39062))
				return false;
			if (_scheduleView.Presenter.LastUnsavedSchedulePart == null)
				return false;

			if (!_scheduleView.Presenter.LastUnsavedSchedulePart.PersonAssignment(true).ShiftLayers.Any(shiftLayer => ((ShiftLayer) shiftLayer).OrderIndex < 0))
				return false;
			var exception = new Exception("ShiftLayer has negative OrderIndex" + Environment.NewLine + "Scheduler have to close. You may start Scheduler again and continue working.");
			using (var view = new SimpleExceptionHandlerView(exception, "Negative OrderIndex", exception.Message))
			{
				view.ShowDialog();
			}

			return true;
		}

		private void backgroundWorkerDeleteRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;

			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();

			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;

			if (rethrowBackgroundException(e))
				return;

			_grid.Refresh();
			if (e.Cancelled)
			{
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancel");
				releaseUserInterface(e.Cancelled);
				return;
			}

			if (_schedulerState.SchedulingResultState.SkipResourceCalculation)
				releaseUserInterface(e.Cancelled);

			updateShiftEditor();
			RecalculateResources();
		}

		private bool shouldCancelBeforeStartingDeleteAction()
		{
			return (_backgroundWorkerRunning || _backgroundWorkerDelete.IsBusy);
		}

		private void deleteFromSchedulePart(DeleteOption deleteOption)
		{
			if (_scheduleView != null)
			{
				if (shouldCancelBeforeStartingDeleteAction()) return;

				disableAllExceptCancelInRibbon();
				var clipHandler = new ClipHandler<IScheduleDay>();
				GridHelper.GridCopySelection(_scheduleView.ViewGrid, clipHandler, true);
				var list = _scheduleView.DeleteList(clipHandler, deleteOption);
				IGridlockRemoverForDelete gridlockRemoverForDelete = new GridlockRemoverForDelete(_gridLockManager);
				list = gridlockRemoverForDelete.RemoveLocked(list);
				toolStripStatusLabelStatus.Text = string.Format(CultureInfo.CurrentCulture,
					LanguageResourceHelper.Translate("XXDeletingSchedules"), list.Count);

				Cursor = Cursors.WaitCursor;
				_backgroundWorkerDelete.WorkerReportsProgress = true;
				_backgroundWorkerRunning = true;
				_backgroundWorkerDelete.RunWorkerAsync(new Tuple<DeleteOption, IList<IScheduleDay>>(deleteOption, list));
			}
		}

		private void backgroundWorkerDeleteDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			var argument = (Tuple<DeleteOption, IList<IScheduleDay>>) e.Argument;
			var list = argument.Item2;
			_undoRedo.CreateBatch(string.Format(CultureInfo.CurrentCulture, Resources.UndoRedoDeleteSchedules, list.Count));
			var deleteService = new DeleteSchedulePartService();
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(SchedulerState.SchedulingResultState,
					_container.Resolve<IScheduleDayChangeCallback>(),
					new ScheduleTagSetter(_defaultScheduleTag));
			if (!list.IsEmpty())
			{
				deleteService.Delete(list, argument.Item1, rollbackService, new BackgroundWorkerWrapper(_backgroundWorkerDelete),
					NewBusinessRuleCollection.AllForDelete(SchedulerState.SchedulingResultState));
			}

			_undoRedo.CommitBatch();
		}

		private void tabSkillDataSelectedIndexChanged(object sender, EventArgs e)
		{
			drawSkillGrid();
			reloadChart();
		}

		#endregion

		#region Chart Events

		private void gridrowInChartSettingLineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGridRow, _gridrowInChartSettingButtons, e.Enabled);
		}

		private void gridlinesInChartSettingsLineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis,
				e.LineColor);
		}

		private void skillGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			var skillResultGridControlBase = (SkillResultGridControlBase) sender;
			if (skillResultGridControlBase.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = skillResultGridControlBase.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = skillResultGridControlBase.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation,
					chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void skillIntradayGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillIntradayGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillIntradayGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillIntradayGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation,
					chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}


		private void toolStripButtonGridInChartClick(object sender, EventArgs e)
		{
			reloadChart();
		}

		private void chartControlSkillDataChartRegionMouseHover(object sender, ChartRegionMouseEventArgs e)
		{
			GridChartManager.SetChartToolTip(e.Region, _chartControlSkillData);
		}

		private void chartControlSkillDataChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = Math.Max(1, (int) GridChartManager.GetIntervalValueForChartPoint(_chartControlSkillData, e.Point));
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week) && !_chartInIntradayMode)
				_skillWeekGridControl.ScrollCellInView(0, column);

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month) && !_chartInIntradayMode)
				_skillMonthGridControl.ScrollCellInView(0, column);

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period) && !_chartInIntradayMode)
				_skillFullPeriodGridControl.ScrollCellInView(0, column);

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday) && _chartInIntradayMode)
				_skillIntradayGridControl.ScrollCellInView(0, column);

			if (!_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday) && !_chartInIntradayMode)
			{
				_skillDayGridControl.ScrollCellInView(0, column);
				_grid.ScrollCellInView(0, column + 1);
			}

			if (!_chartInIntradayMode)
			{
				_grid.ScrollCellInView(0, column + 1);
			}
		}

		#endregion

		#region Private methods

		private void toolStripMenuItemCutClick(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).Cut();
		}

		private void toolStripMenuItemCutSpecial2Click(object sender, EventArgs e)
		{
			_cutPasteHandlerFactory.For(_controlType).CutSpecial();
		}

		private void scheduleSelected(bool backToLegalShift)
		{
			if (_backgroundWorkerScheduling.IsBusy) return;

			if (_scheduleView != null)
			{
				var selectedSchedules = _scheduleView.SelectedSchedules();
				if (!selectedSchedules.Any())
					return;

				_optimizerOriginalPreferences.SchedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff;
				_optimizerOriginalPreferences.SchedulingOptions.WorkShiftLengthHintOption =
					WorkShiftLengthHintOption.AverageWorkTime;
				IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
				try
				{
					if (backToLegalShift)
					{
						startBackgroundScheduleWork(_backgroundWorkerScheduling,
							new SchedulingAndOptimizeArgument(selectedSchedules)
							{
								OptimizationMethod = OptimizationMethod.BackToLegalShift
							}, true);
						return;
					}

					var hideMaxSeat = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_MaxSeatsNew_40939);
					using (
						var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions,
							daysOffPreferences,
							_schedulerState.CommonStateHolder.ActiveShiftCategories,
							false, _groupPagesProvider, _schedulerState.CommonStateHolder.ActiveScheduleTags,
							"SchedulingOptions", _schedulerState.CommonStateHolder.ActiveActivities, hideMaxSeat))
					{
						if (options.ShowDialog(this) == DialogResult.OK)
						{
							options.Refresh();
							startBackgroundScheduleWork(_backgroundWorkerScheduling,
								new SchedulingAndOptimizeArgument(selectedSchedules), true);

						}
					}
				}
				catch (CouldNotCreateTransactionException dataSourceException)
				{
					using (
						var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC,
							Resources.ServerUnavailable))
					{
						view.ShowDialog();
					}
				}
			}
		}

		private void scheduleHourlyEmployees()
		{
			if (_backgroundWorkerScheduling.IsBusy) return;

			if (_scheduleView != null)
			{
				var selectedSchedules = _scheduleView.SelectedSchedules();
				if (!selectedSchedules.Any())
					return;

				_optimizerOriginalPreferences.SchedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.HourlyStaff;
				_optimizerOriginalPreferences.SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free;
				IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
				var hideMaxSeat = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_MaxSeatsNew_40939);
				using (
					var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions,
						daysOffPreferences, _schedulerState.CommonStateHolder.ActiveShiftCategories,
						false, _groupPagesProvider, _schedulerState.CommonStateHolder.ActiveScheduleTags, "SchedulingOptionsActivities",
						_schedulerState.CommonStateHolder.ActiveActivities, hideMaxSeat))
				{
					if (options.ShowDialog(this) == DialogResult.OK)
					{
						_optimizerOriginalPreferences.SchedulingOptions.OnlyShiftsWhenUnderstaffed = true;
						Refresh();
						startBackgroundScheduleWork(_backgroundWorkerScheduling,
							new SchedulingAndOptimizeArgument(selectedSchedules), true);
					}
				}
			}
		}

		private void startBackgroundScheduleWork(BackgroundWorker backgroundWorker, object argument, bool showProgressBar)
		{
			if (_backgroundWorkerRunning) return;

			var scheduleDays = ((SchedulingAndOptimizeArgument) argument).SelectedScheduleDays;
			int selectedScheduleCount = scheduleDays.Count;

			var startDay = scheduleDays.FirstOrDefault();
			var endDay = scheduleDays.LastOrDefault();

			if (startDay != null && endDay != null && startDay.Period.StartDateTime <= endDay.Period.EndDateTime)
			{
				var startDate = startDay.Period.StartDateTime;
				var endDate = endDay.Period.EndDateTime;
				_selectedPeriod = new DateTimePeriod(startDate, endDate);
			}
			else
			{
				_selectedPeriod = new DateTimePeriod(DateTime.MinValue, DateTime.MaxValue);
			}

			toolStripStatusLabelStatus.Text = string.Format(CultureInfo.CurrentCulture,
				LanguageResourceHelper.Translate("XXSchedulingDays"), selectedScheduleCount);
			Cursor = Cursors.WaitCursor;
			disableAllExceptCancelInRibbon();
			if (showProgressBar)
			{
				toolStripProgressBar1.Maximum = selectedScheduleCount;
				toolStripProgressBar1.Visible = true;
				toolStripProgressBar1.Value = 0;
			}
			_backgroundWorkerRunning = true;
			backgroundWorker.RunWorkerAsync(argument);
		}

		private void backgroundWorkerSchedulingRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;
			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();
			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;
			if (rethrowBackgroundException(e))
				return;

			//Next line will start work on another background thread.
			//No code after next line please.
			RecalculateResources();
		}

		private void backgroundWorkerSchedulingProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;

			if (InvokeRequired)
				BeginInvoke(new EventHandler<ProgressChangedEventArgs>(backgroundWorkerSchedulingProgressChanged), sender, e);
			else
			{
				if (e.UserState is BackToLegalShiftArgs)
				{
					var args = (BackToLegalShiftArgs) e.UserState;
					if (_cancelButtonPressed)
					{
						args.Cancel = true;
					}
					_totalScheduled = args.ProcessedBlocks;
					toolStripProgressBar1.Maximum = args.TotalBlocks;
					schedulingProgress(null);
				}
				else if (e.UserState is TeleoptiProgressChangeMessage)
				{
					var arg = (TeleoptiProgressChangeMessage) e.UserState;
					scheduleStatusBarUpdate(arg.Message);
				}
				else
				{
					var progress = e.UserState as SchedulingServiceBaseEventArgs;
					if (progress != null)
					{
						if (_cancelButtonPressed)
						{
							_backgroundWorkerScheduling.CancelAsync();
							progress.Cancel = true;
							progress.CancelCallback();
						}
					}

					if (e.ProgressPercentage <= 0)
					{
						schedulingProgress(Math.Abs(e.ProgressPercentage));
					}
					else
					{
						schedulingProgress(null);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void backgroundWorkerSchedulingDoWork(object sender, DoWorkEventArgs e)
		{
			_totalScheduled = 0;
			_undoRedo.CreateBatch(Resources.UndoRedoScheduling);
			var argument = (SchedulingAndOptimizeArgument) e.Argument;
			var scheduleDays = argument.SelectedScheduleDays;
			var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(scheduleDays).Value;
			turnOffCalculateMinMaxCacheIfNeeded(_optimizerOriginalPreferences.SchedulingOptions);
			_optimizerOriginalPreferences.SchedulingOptions.NotAllowedShiftCategories.Clear();

			AdvanceLoggingService.LogSchedulingInfo(_optimizerOriginalPreferences.SchedulingOptions,
				scheduleDays.Select(x => x.Person).Distinct().Count(),
				selectedPeriod.DayCount(),
				() => runBackgroundWorkerScheduling(e));
			_undoRedo.CommitBatch();

		}

		private void runBackgroundWorkerScheduling(DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument) e.Argument;
			if (argument.OptimizationMethod == OptimizationMethod.BackToLegalShift)
			{
				var command = _container.Resolve<BackToLegalShiftCommand>();
				command.Execute(new BackgroundWorkerWrapper(_backgroundWorkerScheduling), argument.SelectedScheduleDays,
					_schedulerState.SchedulingResultState, _schedulerState.AllPermittedPersons);
			}
			else
			{
				var desktopScheduling = _container.Resolve<DesktopScheduling>();
				desktopScheduling.Execute(_optimizerOriginalPreferences, new BackgroundWorkerWrapper(_backgroundWorkerScheduling),
					argument.SelectedScheduleDays, _optimizationPreferences, _daysOffPreferences);
			}
		}

		private void turnOffCalculateMinMaxCacheIfNeeded(ISchedulingOptions schedulingOptions)
		{
			var calculateMinMaxCacheDecider = new CalculateMinMaxCacheDecider();
			bool turnOfCache = calculateMinMaxCacheDecider.ShouldCacheBeDisabled(_schedulerState, schedulingOptions,
				_container.Resolve<IEffectiveRestrictionCreator>
					(), maxCalculatMinMaxCacheEnries);
			if (turnOfCache)
			{
				_container.Resolve<IMbCacheFactory>().DisableCache<IWorkShiftWorkTime>();
			}
			else
			{
				_container.Resolve<IMbCacheFactory>().EnableCache<IWorkShiftWorkTime>();
			}
		}

		private void backgroundWorkerOptimizationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;
			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();
			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;
			if (rethrowBackgroundException(e))
				return;

			RecalculateResources();
		}

		private void backgroundWorkerOptimizationProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;
			if (InvokeRequired)
				BeginInvoke(new EventHandler<ProgressChangedEventArgs>(backgroundWorkerOptimizationProgressChanged), sender, e);
			else
			{
				optimizationProgress(e);
			}
		}

		private void scheduleStatusBarUpdate(string message)
		{
			toolStripStatusLabelStatus.Text = message;
			statusStrip1.Refresh();
			Application.DoEvents();
		}

		private void schedulingProgress(int? percent)
		{
			if (_inUpdate)
				return;

			_inUpdate = true;

			if (percent.HasValue)
			{
				toolStripProgressBar1.Maximum = 100;
				toolStripProgressBar1.Value = percent.Value;
			}
			else
			{
				if (_totalScheduled <= toolStripProgressBar1.Maximum) toolStripProgressBar1.Value = _totalScheduled;
				if (_totalScheduled > toolStripProgressBar1.Maximum) _totalScheduled = toolStripProgressBar1.Maximum;
			}

			string statusText = string.Format(CultureInfo.CurrentCulture,
				LanguageResourceHelper.Translate("XXSchedulingProgress"), _totalScheduled, toolStripProgressBar1.Maximum);
			toolStripStatusLabelStatus.Text = statusText;
			_grid.Invalidate();
			refreshSummarySkillIfActive();
			_skillIntradayGridControl.Invalidate(true);
			_skillDayGridControl.Invalidate(true);
			_skillWeekGridControl.Invalidate(true);
			_skillMonthGridControl.Invalidate(true);
			_skillFullPeriodGridControl.Invalidate(true);
			refreshChart();
			statusStrip1.Refresh();
			Application.DoEvents();
			_inUpdate = false;
		}

		private void optimizationProgress(ProgressChangedEventArgs e)
		{
			//This one is for use of reoptimize, reoptimize days off and move shifts. We will probably need someting else for get back to legal state
			var progress = e.UserState as ResourceOptimizerProgressEventArgs;
			if (progress != null)
			{
				if (_cancelButtonPressed)
				{
					_backgroundWorkerOptimization.CancelAsync();
					progress.Cancel = true;
					progress.CancelAction();
				}

				if (_scheduleCounter >= progress.ScreenRefreshRate)
				{
					_grid.Invalidate();
					refreshSummarySkillIfActive();
					_skillIntradayGridControl.Invalidate(true);
					_skillDayGridControl.Invalidate(true);
					_skillWeekGridControl.Invalidate(true);
					_skillMonthGridControl.Invalidate(true);
					_skillFullPeriodGridControl.Invalidate(true);
					refreshChart();
					_scheduleCounter = 0;
				}

				if (_cancelButtonPressed)
				{
					toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancellingThreeDots");
				}

				else if (!string.IsNullOrEmpty(progress.Message))
				{
					toolStripStatusLabelStatus.Text = progress.Message;
				}
				else
				{
					toolStripStatusLabelStatus.Text = "Period value = " +
													  progress.Value + " (" + progress.Delta + ")";
				}
			}
			statusStrip1.Refresh();
			_scheduleCounter++;
		}

		private void releaseUserInterface(bool canceled)
		{
			using (PerformanceOutput.ForOperation("ReleaseUserInterface"))
			{
				SuspendLayout();
				_backgroundWorkerRunning = false;
				_grid.Cursor = Cursors.Default;
				_grid.Enabled = true;
				_grid.Cursor = Cursors.Default;
				_skillIntradayGridControl.Invalidate(true);
				_skillDayGridControl.Invalidate(true);
				_skillWeekGridControl.Invalidate(true);
				_skillMonthGridControl.Invalidate(true);
				_skillFullPeriodGridControl.Invalidate(true);
				refreshChart();

				if (_scheduleView != null)
					updateSelectionInfo(_scheduleView.SelectedSchedules());

				toolStripProgressBar1.Visible = false;
				Cursor = Cursors.Default;
				enableAllExceptCancelInRibbon();

				schedulerSplitters1.ElementHost1.Enabled = true;
				//shifteditor                                               
				if (_scheduleView != null)
				{
					ActiveControl = _scheduleView.ViewGrid;
				}

				enableUndoRedoButtons();
				enableSave();

				toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = false;
				if (canceled)
				{
					toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancel");
					return;
				}
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXReady");
				if (_schedulerState.SchedulingResultState.SkipResourceCalculation)
					statusStrip1.BackColor = Color.Salmon;

				ResumeLayout(true);
			}
		}

		private void refreshChart()
		{
			try
			{
				_gridChartManager.ReloadChart();
			}
			catch (NullReferenceException ex)
			{
				LogManager.GetLogger(typeof (SchedulingScreen)).Error(ex.ToString());
			}
		}

		private void backgroundWorkerOvertimeSchedulingDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			var schedulingOptions = _optimizerOriginalPreferences.SchedulingOptions;
			schedulingOptions.DayOffTemplate = _schedulerState.CommonStateHolder.DefaultDayOffTemplate;
			bool lastCalculationState = _schedulerState.SchedulingResultState.SkipResourceCalculation;
			_schedulerState.SchedulingResultState.SkipResourceCalculation = false;
			//if (lastCalculationState)
			//{
			//	_optimizationHelperExtended.ResourceCalculateAllDays(new BackgroundWorkerWrapper(_backgroundWorkerOvertimeScheduling), true);
			//}

			_totalScheduled = 0;
			var argument = (SchedulingAndOptimizeArgument) e.Argument;

			turnOffCalculateMinMaxCacheIfNeeded(schedulingOptions);

			var scheduleDays = argument.SelectedScheduleDays;

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays =
				_container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(_schedulerState.Schedules, scheduleDays);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			_undoRedo.CreateBatch(Resources.UndoRedoScheduling);

			_container.Resolve<ScheduleOvertime>()
				.Execute(argument.OvertimePreferences, new BackgroundWorkerWrapper(_backgroundWorkerOvertimeScheduling),
								_gridLockManager.UnlockedDays(scheduleDays));

			_schedulerState.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
			_undoRedo.CommitBatch();
		}

		private void backgroundWorkerOvertimeSchedulingProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;

			if (InvokeRequired)
				BeginInvoke(new EventHandler<ProgressChangedEventArgs>(backgroundWorkerOvertimeSchedulingProgressChanged), sender,
					e);
			else
			{
				var progress = e.UserState as SchedulingServiceBaseEventArgs;

				if (progress != null && _cancelButtonPressed)
				{
					progress.Cancel = true;
					progress.CancelCallback();
				}

				if (e.ProgressPercentage <= 0)
				{
					schedulingProgress(Math.Abs(e.ProgressPercentage));
				}
				else
				{
					schedulingProgress(null);
				}
			}
		}

		private void backgroundWorkerOvertimeSchedulingRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;
			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();
			_backgroundWorkerRunning = false;
			_cancelButtonPressed = false;
			if (rethrowBackgroundException(e))
				return;

			_personsToValidate.Clear();
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				_personsToValidate.Add(permittedPerson);
			}

			RecalculateResources();
		}

		private void backgroundWorkerOptimizationDoWork(object sender, DoWorkEventArgs e)
		{
			_undoRedo.CreateBatch(Resources.UndoRedoReOptimize);
			var argument = (SchedulingAndOptimizeArgument) e.Argument;
			var scheduleDays = argument.SelectedScheduleDays;
			var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(scheduleDays).Value;
			var dateOnlyList = selectedPeriod.DayCollection();
			_schedulerState.SchedulingResultState.SkillDaysOnDateOnly(dateOnlyList);
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var schedulingOptions = _container.Resolve<ISchedulingOptionsCreator>().CreateSchedulingOptions(optimizerPreferences);
			schedulingOptions.NotAllowedShiftCategories.Clear();
			turnOffCalculateMinMaxCacheIfNeeded(schedulingOptions);
			AdvanceLoggingService.LogOptimizationInfo(optimizerPreferences, scheduleDays.Select(x => x.Person).Distinct().Count(),
				dateOnlyList.Count, () => runBackgroupWorkerOptimization(e));
			_undoRedo.CommitBatch();
		}

		private void runBackgroupWorkerOptimization(DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument) e.Argument;
			var dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(argument.DaysOffPreferences);

			if (argument.OptimizationMethod == OptimizationMethod.BackToLegalState)
			{
				_container.Resolve<BackToLegalStateExecuter>().Execute(
					_optimizerOriginalPreferences,
					new BackgroundWorkerWrapper(_backgroundWorkerOptimization),
					_schedulerState,
					argument.SelectedScheduleDays,
					_optimizationPreferences,
					dayOffOptimizationPreferenceProvider);
			}
			else
			{
				_container.Resolve<OptimizationExecuter>().Execute(
					new BackgroundWorkerWrapper(_backgroundWorkerOptimization),
					_schedulerState,
					argument.SelectedScheduleDays,
					_optimizationPreferences, 
					dayOffOptimizationPreferenceProvider);
			}
		}

		private void checkPastePermissions()
		{
			_permissionHelper.CheckPastePermissions(toolStripMenuItemPaste, toolStripMenuItemPasteSpecial);
		}

		private void setPermissionOnControls()
		{
			_permissionHelper.SetPermissionOnClipboardControl(_clipboardControl, _clipboardControlRestrictions);
			_permissionHelper.SetPermissionOnEditControl(_editControl, _editControlRestrictions);
			_permissionHelper.SetPermissionOnContextMenuItems(toolStripMenuItemInsertAbsence, toolStripMenuItemInsertDayOff,
				toolStripMenuItemDelete, toolStripMenuItemDeleteSpecial, toolStripMenuItemWriteProtectSchedule,
				toolStripMenuItemWriteProtectSchedule2, toolStripMenuItemAddStudentAvailabilityRestriction,
				toolStripMenuItemAddStudentAvailability,
				toolStripMenuItemAddPreferenceRestriction, toolStripMenuItemAddPreference, toolStripMenuItemViewReport,
				toolStripMenuItemScheduledTimePerActivity);
			_permissionHelper.SetPermissionOnMenuButtons(toolStripButtonRequestView, backStageButtonOptions,
				toolStripButtonFilterOvertimeAvailability, ToolStripMenuItemScheduleOvertime,
				toolStripButtonFilterStudentAvailability);
			toolStripExActions.Enabled = true;
			if (_scheduleView != null) enableSwapButtons(_scheduleView.SelectedSchedules());
		}

		private void loadAndOptimizeData(DoWorkEventArgs e)
		{
			var optimizerHelper = new OptimizerHelperHelper();
			IList<LoaderMethod> methods = new List<LoaderMethod>();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ILoaderDeciderResult deciderResult = null;
				var setDeciderResult = new Action<ILoaderDeciderResult>(r => deciderResult = r);
				var getDeciderResult = new Func<ILoaderDeciderResult>(() => deciderResult);

				uow.Reassociate(_scenario);
				methods.Add(new LoaderMethod(loadCommonStateHolder, LanguageResourceHelper.Translate("XXLoadingDataTreeDots")));
				methods.Add(new LoaderMethod(loadSkills, null));
				methods.Add(new LoaderMethod(loadSettings, null));
				methods.Add(new LoaderMethod(loadAuditingSettings, null));
				methods.Add(new LoaderMethod(loadPeople, LanguageResourceHelper.Translate("XXLoadingPeopleTreeDots")));
				methods.Add(new LoaderMethod(filteringPeopleAndSkills, null));
				methods.Add(new LoaderMethod(loadSchedules, LanguageResourceHelper.Translate("XXLoadingSchedulesTreeDots")));
                if(PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _loadRequsts)
				    methods.Add(new LoaderMethod(loadRequests, LanguageResourceHelper.Translate("XXLoadingRequestsThreeDots")));

				methods.Add(new LoaderMethod(loadSkillDays, LanguageResourceHelper.Translate("XXLoadingSkillDataTreeDots")));
				methods.Add(new LoaderMethod(loadDefinitionSets, null));
				methods.Add(new LoaderMethod(loadAccounts, null));
				methods.Add(new LoaderMethod(loadSeniorityWorkingDays, null));

				using (PerformanceOutput.ForOperation("Loading all data for scheduler"))
				{
					foreach (var method in methods)
					{
						backgroundWorkerLoadData.ReportProgress(1, method.StatusStripString);
						method.Action.Invoke(uow, SchedulerState, setDeciderResult, getDeciderResult);
						if (backgroundWorkerLoadData.CancellationPending)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				var period = new ScheduleDateTimePeriod(SchedulerState.RequestedPeriod.Period(),
					SchedulerState.SchedulingResultState.PersonsInOrganization);
				if (!_teamLeaderMode)
				{
					ISchedulingOptions options = new SchedulingOptions();
					optimizerHelper.SetConsiderShortBreaks(SchedulerState.SchedulingResultState.PersonsInOrganization,
						SchedulerState.RequestedPeriod.DateOnlyPeriod, options, _container.Resolve<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>());
					SchedulerState.ConsiderShortBreaks = options.ConsiderShortBreaks;
				}
				else
				{
					SchedulerState.ConsiderShortBreaks = false;
				}
				initMessageBroker(period.LoadedPeriod());
			}

			_scheduleOptimizerHelper = _container.Resolve<ScheduleOptimizerHelper>();

			if (!_schedulerState.SchedulingResultState.SkipResourceCalculation && !_teamLeaderMode)
			{
				backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXCalculatingResourcesDotDotDot"));
				var requestPeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;
				using (_container.Resolve<SharedResourceContextOldSchedulingScreenBehavior>().MakeSureExists(requestPeriod))
				{
					_optimizationHelperExtended.ResourceCalculateAllDays(new BackgroundWorkerWrapper(backgroundWorkerLoadData), true);
				}
			}

			if (e.Cancel)
				return;

			_schedulerState.ClearDaysToRecalculate();

			if (_validation)
				validation();

			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXValidations"));
			////TODO move into the else clause above
			_detectedTimeZoneInfos.Add(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				validatePersonAccounts(permittedPerson);
				_detectedTimeZoneInfos.Add(permittedPerson.PermissionInformation.DefaultTimeZone());
			}

			SchedulerState.SchedulingResultState.Schedules.ModifiedPersonAccounts.Clear();
			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXInitializingTreeDots"));
			backgroundWorkerLoadData.ReportProgress(1);

			foreach (var tag in _schedulerState.CommonStateHolder.ActiveScheduleTags)
			{
				if (tag.Id != _currentSchedulingScreenSettings.DefaultScheduleTag) continue;
				_defaultScheduleTag = tag;
				break;
			}

			foreach (var skillDay in SchedulerState.SchedulingResultState.AllSkillDays())
			{
				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					skillStaffPeriod.CalculateEstimatedServiceLevel();
				}
			}

			var agentsDictionary = _schedulerState.FilteredCombinedAgentsDictionary;
			if (agentsDictionary.Count == 0)
			{
				_schedulerState.ResetFilteredPersons();
				agentsDictionary = _schedulerState.FilteredCombinedAgentsDictionary;
			}
			schedulerSplitters1.RefreshTabInfoPanels(agentsDictionary.Values);

			GridHelper.GridlockWriteProtected(_schedulerState, LockManager);

			_lastSaved = DateTime.Now;
			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXLoadingFormThreeDots"));
		}


		private void validatePersonAccounts(IPerson person)
		{
			IScheduleRange range = SchedulerState.SchedulingResultState.Schedules[person];
			var rule = new NewPersonAccountRule(SchedulerState.SchedulingResultState,
				SchedulerState.SchedulingResultState.AllPersonAccounts);
			IList<IBusinessRuleResponse> toRemove = new List<IBusinessRuleResponse>();
			IList<IBusinessRuleResponse> exposedBusinessRuleResponseCollection =
				((ScheduleRange) range).ExposedBusinessRuleResponseCollection();
			foreach (var businessRuleResponse in exposedBusinessRuleResponseCollection)
			{
				if (businessRuleResponse.TypeOfRule == rule.GetType())
				{
					toRemove.Add(businessRuleResponse);
				}
			}
			foreach (var businessRuleResponse in toRemove)
			{
				exposedBusinessRuleResponseCollection.Remove(businessRuleResponse);
			}

			DateOnlyPeriod reqPeriod = SchedulerState.RequestedPeriod.DateOnlyPeriod;
			IEnumerable<IScheduleDay> allScheduleDays = range.ScheduledDayCollection(reqPeriod);
			IDictionary<IPerson, IScheduleRange> dic = new Dictionary<IPerson, IScheduleRange>();
			dic.Add(person, range);
			//TODO need to make the call twice, ugly fix for now /MD
			rule.Validate(dic, allScheduleDays);
		}

		private void validation()
		{
			backgroundWorkerLoadData.ReportProgress(1,
				string.Format(CultureInfo.CurrentCulture, LanguageResourceHelper.Translate("XXValidatingPersons"),
					SchedulerState.AllPermittedPersons.Count));
			_personsToValidate.Clear();
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				_personsToValidate.Add(permittedPerson);
			}
			var rulesToRun = _schedulerState.SchedulingResultState.GetRulesToRun();
			var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;

			if (_container.Resolve<IToggleManager>().IsEnabled(Toggles.SchedulingScreen_BatchAgentValidation_41552))
			{
				var resolvedTranslatedString = LanguageResourceHelper.Translate("XXValidatingPersons2");
				var validatedCount = 0;
				foreach (var persons in _personsToValidate.Batch(100))
				{
					var batchedPeople = persons as IList<IPerson> ?? persons.ToList();
					validatedCount += batchedPeople.Count;
					backgroundWorkerLoadData.ReportProgress(0,
					string.Format(CultureInfo.CurrentCulture, resolvedTranslatedString, validatedCount, SchedulerState.AllPermittedPersons.Count));
					_schedulerState.Schedules.ValidateBusinessRulesOnPersons(batchedPeople, loggedOnCulture, rulesToRun);
				}
			}
			else
			{
				_schedulerState.Schedules.ValidateBusinessRulesOnPersons(_personsToValidate, loggedOnCulture, rulesToRun);
			}
			_personsToValidate.Clear();
		}

		private void setupRequestPresenter()
		{
			_handleBusinessRuleResponse = new HandleBusinessRuleResponse();
			_requestPresenter = new RequestPresenter(_personRequestAuthorizationChecker);
			_requestPresenter.SetUndoRedoContainer(_undoRedo);
		}

		private void loadAccounts(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			var rep = new PersonAbsenceAccountRepository(uow);
			SchedulerState.SchedulingResultState.AllPersonAccounts = rep.LoadAllAccounts();
		}

		private void loadDefinitionSets(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository =
				new MultiplicatorDefinitionSetRepository(uow);
			MultiplicatorDefinitionSet = multiplicatorDefinitionSetRepository.FindAllDefinitions();
		}

		private void filteringPeopleAndSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			using (PerformanceOutput.ForOperation("Executing and filtering loader decider"))
			{
				ICollection<IPerson> peopleInOrg = SchedulerState.SchedulingResultState.PersonsInOrganization;
				int peopleCountFromBeginning = peopleInOrg.Count;
				var decider = _teamLeaderMode ? new PeopleAndSkillLoaderDeciderForTeamLeaderMode() : _peopleAndSkillLoaderDecider;
				var result = decider.Execute(_schedulerState.RequestedScenario, _schedulerState.RequestedPeriod.Period(),
					SchedulerState.AllPermittedPersons);
				setDeciderResult(result);

				int removedPeople = result.FilterPeople(peopleInOrg);
				log.Info("Removed " + removedPeople + " people when filtering (original: " + peopleCountFromBeginning +
						 ")");

				//RK: jag tycker detta är fel, men det rättar en bugg för nu. 
				//Filtereringen gör rätt utan detta, men jag vet inte vilken lista som egentligen används för
				//visning, db-sparning, resursberäkning, visning etc. Så det blir såhär tills större häv

				peopleInOrg = new HashSet<IPerson>(peopleInOrg);
				SchedulerState.AllPermittedPersons.ForEach(peopleInOrg.Add);
				SchedulerState.SchedulingResultState.PersonsInOrganization = peopleInOrg;
				log.Info("No, changed my mind... Removed " + (peopleCountFromBeginning - peopleInOrg.Count) + " people.");
				var skills = stateHolder.SchedulingResultState.Skills;
				int orgSkills = skills.Length;
				int removedSkills = result.FilterSkills(skills,stateHolder.SchedulingResultState.RemoveSkill,s => stateHolder.SchedulingResultState.AddSkills(s));			
				log.Info("Removed " + removedSkills + " skill when filtering (original: " + orgSkills + ")");
			}
		}

		private void loadSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			ICollection<ISkill> skills = new SkillRepository(uow).FindAllWithSkillDays(stateHolder.RequestedPeriod.DateOnlyPeriod);
			foreach (ISkill skill in skills)
			{
				if (skill.SkillType is SkillTypePhone)
					skill.SkillType.StaffingCalculatorService = new StaffingCalculatorServiceFacade();
				stateHolder.SchedulingResultState.AddSkills(skill);
			}
		}
		
		private void loadSettings(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			using (PerformanceOutput.ForOperation("Loading settings"))
			{
				_schedulerState.LoadSettings(uow, new RepositoryFactory());
			}
		}

		private void loadAuditingSettings(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			var repository = new AuditSettingRepository(new ThisUnitOfWork(uow));
			var auditSetting = repository.Read();
			_isAuditingSchedules = auditSetting.IsScheduleEnabled;
		}

		private void loadSchedules(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			var period = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(),
				stateHolder.SchedulingResultState.PersonsInOrganization);
			using (PerformanceOutput.ForOperation("Loading schedules " + period.LoadedPeriod()))
			{
				IPersonProvider personsInOrganizationProvider =
					new PersonsInOrganizationProvider(stateHolder.SchedulingResultState.PersonsInOrganization);
				// If the people in organization is filtered out to 70% or less of all people then flag 
				// so that a criteria for that is used later when loading schedules.
				var loaderSpecification = new LoadScheduleByPersonSpecification();
				personsInOrganizationProvider.DoLoadByPerson = loaderSpecification.IsSatisfiedBy(getDeciderResult());
				IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true)
				{
					LoadDaysAfterLeft = true
				};
				stateHolder.LoadSchedules(_container.Resolve<IFindSchedulesForPersons>(), personsInOrganizationProvider, scheduleDictionaryLoadOptions, period);
				_schedulerState.Schedules.SetUndoRedoContainer(_undoRedo);
			}
			SchedulerState.Schedules.PartModified += schedulesPartModified;
		}

		private void initMessageBroker(DateTimePeriod period)
		{
			_schedulerMessageBrokerHandler.Listen(period);
			_schedulerMessageBrokerHandler.RequestDeletedFromBroker += schedulerMessageBrokerHandlerRequestDeletedFromBroker;
			_schedulerMessageBrokerHandler.RequestInsertedFromBroker += schedulerMessageBrokerHandlerRequestInsertedFromBroker;
			_schedulerMessageBrokerHandler.SchedulesUpdatedFromBroker += schedulerMessageBrokerHandlerSchedulesUpdatedFromBroker;
		}

		private void schedulerMessageBrokerHandlerRequestInsertedFromBroker(object sender, CustomEventArgs<IPersonRequest> e)
		{
			IPersonRequest personRequestInserted = e.Value;
			if (_requestView != null && personRequestInserted != null)
			{
				_requestView.InsertPersonRequestViewModel(personRequestInserted);
			}
		}

		private void schedulerMessageBrokerHandlerRequestDeletedFromBroker(object sender, CustomEventArgs<IPersonRequest> e)
		{
			IPersonRequest personRequestDeleted = e.Value;
			if (_requestView != null && personRequestDeleted != null)
			{
				_requestView.DeletePersonRequestViewModel(personRequestDeleted);
			}
		}

		private void schedulesPartModified(object sender, ModifyEventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<ModifyEventArgs>(schedulesPartModified), sender, e);
			}
			else
			{
				if (IsDisposed)
					return;


				if (_selectedPeriod.Contains(e.ModifiedPeriod))
					_totalScheduled++;

				_restrictionPersonsToReload.Add(e.ModifiedPerson);

				if (e.Modifier == ScheduleModifier.UndoRedo || e.Modifier == ScheduleModifier.Request)
					_personsToValidate.Add(e.ModifiedPerson);

				_lastModifiedPart = e;

				if (!_backgroundWorkerRunning)
				{
					if (_scheduleView != null)
						_scheduleView.RefreshRangeForAgentPeriod(e.ModifiedPerson, e.ModifiedPeriod);
					if (e.Modifier == ScheduleModifier.UndoRedo)
					{
						selectCellFromPersonDate(e.ModifiedPerson, e.ModifiedPart.DateOnlyAsPeriod.DateOnly);
					}
					if (e.Modifier != ScheduleModifier.MessageBroker)
						enableSave();
					if (_scheduleView != null && _scheduleView.HasOneScheduleDaySelected())
						updateShiftEditor();
				}
			}
		}

		private void loadPeople(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			using (PerformanceOutput.ForOperation("Loading people"))
			{
				SchedulerState.SchedulingResultState.OptionalColumns =
					new OptionalColumnRepository(uow).GetOptionalColumns<Person>();
				var personRep = new PersonRepository(new ThisUnitOfWork(uow));
				IPeopleLoader loader;
				if (_teamLeaderMode)
				{
					loader = new PeopleLoaderForTeamLeaderMode(uow, SchedulerState,
						new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView,
							_schedulerState.RequestedPeriod.DateOnlyPeriod), new RepositoryFactory());
				}
				else
				{
					loader = new PeopleLoader(personRep, new ContractRepository(uow), SchedulerState,
						new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView,
							_schedulerState.RequestedPeriod.DateOnlyPeriod), new SkillRepository(uow));
				}

				loader.Initialize();
			}
			// part of the workaround because we can't press cancel before this / Ola
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, true);
		}

		private void loadRequests(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			using (PerformanceOutput.ForOperation("Loading requests"))
			{
				string numberOfDaysToShowNonPendingRequests;
				stateHolder.LoadPersonRequests(uow, new RepositoryFactory(), _personRequestAuthorizationChecker,
					StateHolderReader.Instance.StateReader.ApplicationScopeData.AppSettings.TryGetValue(
						"NumberOfDaysToShowNonPendingRequests", out numberOfDaysToShowNonPendingRequests)
						? Convert.ToInt32(numberOfDaysToShowNonPendingRequests)
						: 14);
			}
		}

		private void loadSeniorityWorkingDays(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			var result = new SeniorityWorkDayRanksRepository(uow).LoadAll();
			var workDayRanks = result.IsEmpty() ? new SeniorityWorkDayRanks() : result.First();
			stateHolder.SchedulingResultState.SeniorityWorkDayRanks = workDayRanks;
		}

		private static void loadCommonStateHolder(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			stateHolder.LoadCommonState(uow, new RepositoryFactory());
			if (!stateHolder.CommonStateHolder.DayOffs.Any())
				throw new StateHolderException("You must create at least one Day Off in Options!");
		}

		private void disableSave()
		{
			toolStripButtonSaveLarge.Enabled = false;
		}

		private void enableSave()
		{
			toolStripButtonSaveLarge.Enabled = true;
			enableUndoRedoButtons();
		}

		private void enableUndoRedoButtons()
		{
			if (_grid.Enabled)
			{
				toggleQuickButtonEnabledState(toolStripButtonQuickAccessRedo, _undoRedo.CanRedo());
				toggleQuickButtonEnabledState(toolStripSplitButtonQuickAccessUndo, _undoRedo.CanUndo());
			}
		}

		//fix for sunkfusion clickevent fires twice in this method, will be some issues when debugging
		private bool save()
		{
			if (_lastSaved.AddSeconds(2) > DateTime.Now)
				return false;

			if (_scheduleView != null)
			{
				if (notesEditor.NotesIsAltered || notesEditor.PublicNotesIsAltered)
				{
					NotesEditor.RemoveFocus();
					notesEditor.LoadNote(notesEditor.SchedulePart);
				}
				_scheduleView.Presenter.UpdateFromEditor();
			}

			try
			{
				doSaveProcess();
				_lastSaved = DateTime.Now;
				return true;
			}
			catch (TooManyActiveAgentsException e)
			{
				string explanation;
				if (e.LicenseType.Equals(LicenseType.Seat))
				{
					explanation = string.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats, e.NumberOfLicensed);
				}
				else
				{
					explanation = String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents,
						e.NumberOfAttemptedActiveAgents, e.NumberOfLicensed);
				}
				ShowErrorMessage(explanation, Resources.ErrorMessage);
				return false;
			}
		}

		private void doSaveProcess()
		{
			Cursor = Cursors.WaitCursor;

			if (_personAbsenceAccountPersistValidationBusinessRuleResponses != null)
				_personAbsenceAccountPersistValidationBusinessRuleResponses.Clear();

			try
			{
				var persister = _container.Resolve<ISchedulingScreenPersister>();
				IEnumerable<PersistConflict> foundConflicts;
				bool success = persister.TryPersist(_schedulerState.Schedules,
					_schedulerState.PersonRequests,
					_modifiedWriteProtections,
					_schedulerState.CommonStateHolder.ModifiedWorkflowControlSets,
					out foundConflicts);

				if (!success && foundConflicts != null)
				{
					handleConflicts(new List<IPersistableScheduleData>(), foundConflicts);
					doSaveProcess();
				}

				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var accountConflictCollector = new PersonAccountConflictCollector(new DatabaseVersion(new ThisUnitOfWork(uow)));
					var accountConflicts = accountConflictCollector.GetConflicts(_schedulerState.Schedules.ModifiedPersonAccounts);
					if (accountConflicts != null && accountConflicts.Any()) refreshData();
				}

				var personAccountPersister = _container.Resolve<IPersonAccountPersister>();
				personAccountPersister.Persist(_schedulerState.Schedules.ModifiedPersonAccounts);

				//Denna sätts i längre inne i save-loopen. fixa på annat sätt!
				if (_personAbsenceAccountPersistValidationBusinessRuleResponses != null &&
					_personAbsenceAccountPersistValidationBusinessRuleResponses.Any())
				{
					BusinessRuleResponseDialog.ShowDialogFromWinForms(_personAbsenceAccountPersistValidationBusinessRuleResponses);
				}

			}
			catch (CouldNotCreateTransactionException ex)
			{
				using (var view = new SimpleExceptionHandlerView(ex, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
			}
			catch (OptimisticLockException ex)
			{
				using (
					var view = new SimpleExceptionHandlerView(ex, Resources.OpenTeleoptiCCC,
						Resources.SomeoneChangedTheSameDataBeforeYouDot + " " + Resources.PleaseTryAgainLater))
				{
					view.ShowDialog();
				}
			}
			finally
			{
				if (_undoRedo != null) _undoRedo.Clear();
				Cursor = Cursors.Default;
				updateRequestCommandsAvailability();
				updateShiftEditor();
				RecalculateResources();
			}
		}

		private void updateRibbon(ControlType controlType)
		{
			_controlType = controlType;
			switch (controlType)
			{
				case ControlType.SchedulerGridMain:
					enableRibbonForMainGrid(true);
					break;
				case ControlType.ShiftEditor:
				case ControlType.SchedulerGridSkillData:
					break;
				case ControlType.Request:
					enableRibbonForRequests(true);
					break;
			}
			enableChartControls(true);
		}

		private void disableAllExceptCancelInRibbon()
		{
			_uIEnabled = false;
			using (PerformanceOutput.ForOperation("disableAllExceptCancelInRibbon"))
			{
				toolStripTabItemHome.Panel.Enabled = false;
				toolStripTabItemChart.Panel.Enabled = false;
				toolStripTabItem1.Panel.Enabled = false;
				toolStripMenuItemQuickAccessUndo.ShortcutKeys = Keys.None;
				ControlBox = false;
				toggleQuickButtonEnabledState(false);
				contextMenuViews.Enabled = false;
				toolStripButtonShrinkage.Enabled = false;
				toolStripButtonValidation.Enabled = false;
				toolStripButtonCalculation.Enabled = false;
				_grid.Cursor = Cursors.WaitCursor;
				_grid.Enabled = false;
				_grid.Cursor = Cursors.WaitCursor;
				schedulerSplitters1.DisableViewShiftCategoryDistribution();
				schedulerSplitters1.ElementHost1.Enabled = false; //shifteditor
				toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, true);
				ribbonControlAdv1.Cursor = Cursors.AppStarting;
				if (toolStripSpinningProgressControl1.SpinningProgressControl == null)
					toolStripSpinningProgressControl1 = new ToolStripSpinningProgressControl();
				toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = true;
				disableSave();
				toolStripStatusLabelContractTime.Enabled = false;
			}
		}

		private void enableAllExceptCancelInRibbon()
		{
			_uIEnabled = true;
			toolStripTabItemHome.Panel.Enabled = true;
			toolStripTabItemChart.Panel.Enabled = true;
			toolStripTabItem1.Panel.Enabled = true;
			toolStripButtonShrinkage.Enabled = true;
			toolStripButtonValidation.Enabled = true;
			toolStripButtonCalculation.Enabled = true;
			toolStripMenuItemQuickAccessUndo.ShortcutKeys = Keys.Control | Keys.Z;
			ControlBox = true;
			contextMenuViews.Enabled = true;
			toggleQuickButtonEnabledState(true);
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, false);
			enableUndoRedoButtons();
			ribbonControlAdv1.Cursor = Cursors.Default;
			updateRibbon(ControlType.SchedulerGridMain);
			//av nån #%¤#¤%#¤% anledning tänds alla knappar i toggleQuick... ovan. Måste explicit tända/släcka igen.
			_schedulerMessageBrokerHandler.NotifyMessageQueueSizeChange();
			disableButtonsIfTeamLeaderMode();
			schedulerSplitters1.EnableViewShiftCategoryDistribution();
			toolStripStatusLabelContractTime.Enabled = true;
		}

		private void disableButtonsIfTeamLeaderMode()
		{
			if (!_teamLeaderMode) return;
			toolStripButtonShowGraph.Enabled = false;
			toolStripButtonShowResult.Enabled = false;
			toolStripButtonShrinkage.Enabled = false;
			toolStripButtonCalculation.Enabled = false;
			toolStripMenuItemSwapAndReschedule.Enabled = false;
		}

		private void enableRibbonForMainGrid(bool value)
		{
			foreach (Control control1 in toolStripTabItemHome.Panel.Controls)
			{
				if (control1.Name != "toolStripExClipboard")
					enableControl(control1, value);
			}
			if (!_scenario.DefaultScenario)
				toolStripButtonRequestView.Enabled = false;
		}

		private void enableChartControls(bool value)
		{
			foreach (Control control in toolStripTabItemHome.Panel.Controls)
			{
				if (control.Name == "toolStripExGridRowInChartButtons")
					enableControl(control, value);
			}
		}

		private void enableRibbonForRequests(bool value)
		{
			toolStripTabItem1.Enabled = value;
			toolStripTabItemHome.Enabled = !value;

			var textInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture.TextInfo;
			var home = textInfo.ToUpper(Resources.Home);
			var requests = textInfo.ToUpper(Resources.RequestsMenu);

			if (value)
			{
				updateRequestCommandsAvailability();
				toolStripTabItem1.Checked = true;
				toolStripTabItem1.Text = requests;
				toolStripTabItem1.Visible = true;
				toolStripTabItemHome.Text = string.Empty;
			}
			else
			{
				toolStripTabItemHome.Text = home;
				toolStripTabItem1.Text = string.Empty;
				toolStripTabItem1.Visible = false;
				toolStripExHandleRequests.Enabled = false;
			}
		}

		private static void enableControl(Control control, bool value)
		{
			if (control.HasChildren)
			{
				foreach (Control control1 in control.Controls)
				{
					enableControl(control1, value);
				}
			}
			else
			{
				control.Enabled = value;
			}
		}

		private void setupToolbarButtonsChartViews()
		{
			if (toolStripButtonChartIntradayView != null)
				toolStripButtonChartIntradayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			if (toolStripButtonChartDayView != null)
				toolStripButtonChartDayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			if (toolStripButtonChartPeriodView != null)
				toolStripButtonChartPeriodView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			if (toolStripButtonChartMonthView != null)
				toolStripButtonChartMonthView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			if (toolStripButtonChartWeekView != null)
				toolStripButtonChartWeekView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week);
		}

		private void setupContextMenuSkillGrid()
		{

			var skillGridMenuItem = new ToolStripMenuItem(Resources.Period)
			{
				Name = "Period",
				Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period)
			};
			skillGridMenuItem.Click += skillGridMenuItemPeriodClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Month)
			{
				Name = "Month",
				Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month)
			};
			skillGridMenuItem.Click += skillGridMenuItemMonthClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Week)
			{
				Name = "Week",
				Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week)
			};
			skillGridMenuItem.Click += skillGridMenuItemWeekClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Day)
			{
				Name = "Day",
				Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day)
			};
			skillGridMenuItem.Click += skillGridMenuItemDayClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Intraday)
			{
				Name = "Intraday",
				Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday)
			};
			skillGridMenuItem.Click += skillGridMenuItemIntraDayClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.UseShrinkage);
			skillGridMenuItem.Click += toolStripMenuItemUseShrinkage_Click;
			skillGridMenuItem.Checked = _shrinkage;
			skillGridMenuItem.Name = "UseShrinkage";
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			var skillGridMenuSeparator = new ToolStripSeparator();
			_contextMenuSkillGrid.Items.Add(skillGridMenuSeparator);
			skillGridMenuItem = new ToolStripMenuItem(Resources.CreateSkillSummery);
			skillGridMenuItem.Click += skillGridMenuItemClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			skillGridMenuItem = new ToolStripMenuItem(Resources.EditSkillSummery) {Name = "Edit", Enabled = false};
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			skillGridMenuItem = new ToolStripMenuItem(Resources.DeleteSkillSummery) {Name = "Delete", Enabled = false};
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			_skillDayGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillIntradayGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillWeekGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillMonthGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillFullPeriodGridControl.ContextMenuStrip = _contextMenuSkillGrid;
		}

		private void skillGridMenuItemAnalyzeResorceChangesClick(object sender, EventArgs e)
		{
			var selectedDate = _scheduleView.SelectedDateLocal();
			TimeSpan? selectedTime = null;
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				selectedTime = _skillIntradayGridControl.Presenter.SelectedIntervalTime();
			}
			var model = new ResourceCalculationAnalyzerModel(_schedulerState, _container, _optimizationHelperExtended, selectedDate, selectedTime, _shrinkage);
			using (var resourceChanges = new ResourceCalculationAnalyzerView(model))
			{
				resourceChanges.ShowDialog(this);
			}
		}

		private void skillGridMenuItemShovelAnalyzerClick(object sender, EventArgs e)
		{
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				using (var resourceChanges = new ShovelingAnalyzerView(_container.Resolve<IResourceCalculation>(), _container.Resolve<ITimeZoneGuard>()))
				{
					resourceChanges.FillForm(_schedulerState.SchedulingResultState,
						_skillIntradayGridControl.Presenter.Skill, 
						_scheduleView.SelectedDateLocal(), 
						_skillIntradayGridControl.Presenter.SelectedIntervalTime().GetValueOrDefault());
					resourceChanges.ShowDialog(this);
				}
			}
		}

		private void skillGridMenuItemAgentSkillAnalyser_Click(object sender, EventArgs e)
		{
			using (var analyzer = new AgentSkillAnalyzer(SchedulerState.SchedulingResultState.PersonsInOrganization,
				SchedulerState.SchedulingResultState.Skills, SchedulerState.SchedulingResultState.SkillDays,
				SchedulerState.RequestedPeriod.DateOnlyPeriod, _container.Resolve<CreateIslands>()))
			{
				analyzer.LoadData();
				analyzer.ShowDialog(this);
			}
		}

		private void setUpZomMenu()
		{
			toolStripButtonDayView.Tag = ZoomLevel.DayView;
			toolStripButtonWeekView.Tag = ZoomLevel.WeekView;
			toolStripButtonPeriodView.Tag = ZoomLevel.PeriodView;
			toolStripButtonSummaryView.Tag = ZoomLevel.Overview;
			toolStripButtonRequestView.Tag = ZoomLevel.RequestView;
			toolStripButtonRestrictions.Tag = ZoomLevel.RestrictionView;
		}

		#region ribbon

		private void setUpRibbon()
		{
			foreach (ToolStripItem tsi in ribbonControlAdv1.Header.QuickItems)
			{
				if (tsi.GetType() == typeof (QuickButtonReflectable))
				{
					var toolStripButton = (QuickButtonReflectable) tsi;
					tsi.ToolTipText = toolStripButton.ReflectedButton.ToolTipText;
				}
				else if (tsi.GetType() == typeof (QuickDropDownButtonReflectable))
				{
					var toolStripDropDownButton = (QuickDropDownButtonReflectable) tsi;
					tsi.ToolTipText = toolStripDropDownButton.ReflectedDropDownButton.ToolTipText;
				}
			}
			ribbonControlAdv1.TabGroups[0].Name = Resources.ShiftEditor;
			ribbonControlAdv1.TabGroups[1].Name = Resources.MainGrid;
			_gridrowInChartSettingButtons = new GridRowInChartSettingButtons();
			var chartsetteinghost = new ToolStripControlHost(_gridrowInChartSettingButtons);
			toolStripExGridRowInChartButtons.Items.Add(chartsetteinghost);
			_gridrowInChartSettingButtons.SetButtons();
			_gridChartManager = new GridChartManager(_chartControlSkillData, true, true, true);
			_gridChartManager.Create();
			ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdv1);
			ribbonControlAdv1.MenuButtonText = Resources.File;
		}

		private void ribbonTemplatePanelsClose()
		{
			foreach (ToolStripTabGroup group in ribbonControlAdv1.TabGroups)
			{
				group.Visible = false;
			}
		}

		#endregion

		private void setupSkillTabs()
		{
			_currentIntraDayDate = _schedulerState.RequestedPeriod.DateOnlyPeriod.StartDate;
			_tabSkillData.TabPages.Clear();
			_tabSkillData.ImageList = imageListSkillTypeIcons;
			foreach (
				ISkill virtualSkill in
					_virtualSkillHelper.LoadVirtualSkills(_schedulerState.SchedulingResultState.VisibleSkills).OrderBy(s => s.Name))
			{
				TabPageAdv tab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
				tab.Tag = virtualSkill;
				tab.ImageIndex = 4;
				_tabSkillData.TabPages.Add(tab);
				enableEditVirtualSkill(virtualSkill);
				enableDeleteVirtualSkill(virtualSkill);
			}

			foreach (ISkill skill in _schedulerState.SchedulingResultState.VisibleSkills.OrderBy(s => s.Name))
			{
				TabPageAdv tab = ColorHelper.CreateTabPage(skill.Name, skill.Description);
				tab.Tag = skill;
				tab.ImageIndex = GuiHelper.ImageIndexSkillType(skill.SkillType.ForecastSource);

				_tabSkillData.TabPages.Add(tab);
			}
			schedulerSplitters1.PinSavedSkills(_currentSchedulingScreenSettings);
		}

		private void setupInfoTabs()
		{
			var requestedPeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;
			var outerPeriod = new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-7), requestedPeriod.EndDate.AddDays(7));

			_agentInfoControl = new AgentInfoControl(_workShiftWorkTime, _groupPagesProvider, _container, outerPeriod,
				requestedPeriod, _restrictionExtractor, _schedulerState);
			schedulerSplitters1.InsertAgentInfoControl(_agentInfoControl, _schedulerState,
				_container.Resolve<IEffectiveRestrictionCreator>(), maxCalculatMinMaxCacheEnries);

			//container can fix this to one row
			ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson =
				new CachedNumberOfEachCategoryPerPerson(_schedulerState.Schedules, _schedulerState.RequestedPeriod.DateOnlyPeriod);
			ICachedNumberOfEachCategoryPerDate cachedNumberOfEachCategoryPerDate =
				new CachedNumberOfEachCategoryPerDate(_schedulerState.Schedules, _schedulerState.RequestedPeriod.DateOnlyPeriod);
			var allowedSc = new List<IShiftCategory>();
			foreach (var shiftCategory in _schedulerState.CommonStateHolder.ShiftCategories)
			{
				var sc = shiftCategory as IDeleteTag;
				if (sc != null && !sc.IsDeleted)
					allowedSc.Add(shiftCategory);
			}
			ICachedShiftCategoryDistribution cachedShiftCategoryDistribution =
				new CachedShiftCategoryDistribution(_schedulerState.Schedules, _schedulerState.RequestedPeriod.DateOnlyPeriod,
					cachedNumberOfEachCategoryPerPerson,
					allowedSc);
			_shiftCategoryDistributionModel = new ShiftCategoryDistributionModel(cachedShiftCategoryDistribution,
				cachedNumberOfEachCategoryPerDate,
				cachedNumberOfEachCategoryPerPerson,
				_schedulerState.RequestedPeriod.DateOnlyPeriod,
				_schedulerState);
			_shiftCategoryDistributionModel.SetFilteredPersons(_schedulerState.FilteredCombinedAgentsDictionary.Values);
			schedulerSplitters1.InsertShiftCategoryDistributionModel(_shiftCategoryDistributionModel);
			schedulerSplitters1.InsertValidationAlertsModel(new	ValidationAlertsModel(_schedulerState.Schedules, NameOrderOption.LastNameFirstName, _schedulerState.RequestedPeriod.DateOnlyPeriod));
			schedulerSplitters1.ToggelPropertyPanel(!toolStripButtonShowPropertyPanel.Checked);
		}

		private PersonsFilterView _cachedPersonsFilterView;

		private PersonsFilterView getCachedPersonsFilterView()
		{
			if (_cachedPersonsFilterView == null || _cachedPersonsFilterView.IsDisposed)
			{
				var permittedPersons = SchedulerState.AllPermittedPersons.Select(p => p.Id.Value).ToList();

				_cachedPersonsFilterView =
					new PersonsFilterView(SchedulerState.RequestedPeriod.DateOnlyPeriod,
						SchedulerState.FilteredCombinedAgentsDictionary,
						_container,
						ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory()
							.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage),
						string.Empty,
						permittedPersons, true);
			}
			
			_cachedPersonsFilterView.SetCurrentFilter(SchedulerState.FilteredCombinedAgentsDictionary);
			return _cachedPersonsFilterView;
		}

		private void showFilterDialog()
		{
			var scheduleFilterView = getCachedPersonsFilterView();

			scheduleFilterView.StartPosition = FormStartPosition.Manual;

			//TODO: Please come up with a better solution!
			Point pointToScreen = toolStripExFilter.PointToScreen(
				new Point(toolStripButtonFilterAgents.Bounds.X + 63, toolStripButtonFilterAgents.Bounds.Y +
																	 toolStripButtonFilterAgents.Height));
			scheduleFilterView.Location = pointToScreen;
			scheduleFilterView.AutoLocate();

			if (scheduleFilterView.ShowDialog() == DialogResult.OK)
			{
				_schedulerState.FilterPersons(scheduleFilterView.SelectedAgentGuids());

				toolStripButtonFilterAgents.Checked = SchedulerState.AgentFilter();

				if (_scheduleView != null)
				{
					if (_scheduleView.Presenter.SortCommand == null || _scheduleView.Presenter.SortCommand is NoSortCommand)
						_scheduleView.Presenter.ApplyGridSort();
					else
						_scheduleView.Sort(_scheduleView.Presenter.SortCommand);

					_grid.Refresh();
					GridHelper.GridlockWriteProtected(_schedulerState, LockManager);
					_grid.Refresh();
				}
				if (_requestView != null)
					_requestView.FilterPersons(_schedulerState.FilteredCombinedAgentsDictionary.Keys);
				drawSkillGrid();
			}
		}

		private void prepareAgentRestrictionView(IScheduleDay schedulePart, ScheduleViewBase detailView,
			IList<IPerson> persons)
		{
			if (persons.Count == 0) return;
			var selectedPerson = persons.FirstOrDefault();
			if (schedulePart != null) selectedPerson = schedulePart.Person;

			var schedulingOptions = schedulerSplitters1.SchedulingOptions;
			var view = (AgentRestrictionsDetailView) detailView;
			_splitContainerLessIntellegentRestriction.SplitterDistance = 300;
			schedulerSplitters1.AgentRestrictionGrid.MergeHeaders();
			schedulerSplitters1.AgentRestrictionGrid.LoadData(SchedulerState, persons, schedulingOptions, _workShiftWorkTime,
				selectedPerson, view, schedulePart, _container);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void zoom(ZoomLevel level)
		{
			schedulerSplitters1.SuspendLayout();
			IList<IScheduleDay> scheduleParts = null;
			IScheduleDay selectedPart = null;
			IScheduleSortCommand sortCommand = null;
			IList<IPerson> selectedPersons = null;
			int currentSortColumn = 0;
			bool isAscendingSort = false;

			if (_scheduleView != null)
			{
				_grid.ContextMenuStrip = null;
				scheduleParts = _scheduleView.SelectedSchedules();
				selectedPersons = new List<IPerson>(_scheduleView.AllSelectedPersons(scheduleParts));
				sortCommand = _scheduleView.Presenter.SortCommand;
				currentSortColumn = _scheduleView.Presenter.CurrentSortColumn;
				isAscendingSort = _scheduleView.Presenter.IsAscendingSort;
				selectedPart =
					_scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, _scheduleView.ViewGrid.CurrentCell.ColIndex]
						.CellValue as IScheduleDay;
				_scheduleView.RefreshSelectionInfo -= scheduleViewRefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor -= scheduleViewRefreshShiftEditor;
				_scheduleView.Dispose();
				_scheduleView = null;
			}

			enableRibbonForRequests(false);
			var isRestrictionView = level == ZoomLevel.RestrictionView;
			SchedulerRibbonHelper.EnableRibbonControls(toolStripExClipboard, toolStripExEdit2, toolStripExActions,
				toolStripExLocks, toolStripButtonFilterAgents, toolStripMenuItemLock,
				isRestrictionView);

			var callback = _container.Resolve<IScheduleDayChangeCallback>();
			switch (level)
			{
				case ZoomLevel.DayView:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new DayViewNew(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_scheduleView.SetSelectedDateLocal(_dateNavigateControl.SelectedDate);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.WeekView:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new WeekView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.PeriodView:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new PeriodView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.Overview:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new OverviewView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.RequestView:
					restrictionViewMode(false);
					_scheduleView = new PeriodView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_elementHostRequests.BringToFront();
					_elementHostRequests.ContextMenuStrip = contextMenuStripRequests;
					enableRibbonForRequests(true);
					ActiveControl = _elementHostRequests;
					break;
				case ZoomLevel.RestrictionView:
					//restriction view
					Cursor = Cursors.WaitCursor;
					_grid.BringToFront();
					_scheduleView = new AgentRestrictionsDetailView(schedulerSplitters1.AgentRestrictionGrid, _grid, SchedulerState,
						_gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback,
						_defaultScheduleTag, _workShiftWorkTime);
					_scheduleView.TheGrid.ContextMenuStrip = contextMenuStripRestrictionView;
					prepareAgentRestrictionView(selectedPart, _scheduleView, selectedPersons);
					if (scheduleParts != null)
					{
						if (!scheduleParts.IsEmpty())
						{
							_dateNavigateControl.SetSelectedDateNoInvoke(scheduleParts[0].DateOnlyAsPeriod.DateOnly);
						}
					}

					ActiveControl = _grid;
					restrictionViewMode(true);
					Cursor = Cursors.Default;

					break;
				default:
					throw new InvalidEnumArgumentException("level", (int) level, typeof (ZoomLevel));
			}
			_previousZoomLevel = _currentZoomLevel;
			_currentZoomLevel = level;

			if (_currentZoomLevel == ZoomLevel.RequestView)
				reloadRequestView();

			foreach (ToolStripItem item in toolStripPanelItemViews2.Items)
			{
				var t = item as ToolStripButton;
				if (t != null && t.Tag != null)
					t.Checked = ((ZoomLevel) t.Tag == level) ? true : false;
			}

			if (_scheduleView != null)
			{
				if (sortCommand != null) _scheduleView.Presenter.SortCommand = sortCommand;
				_scheduleView.Presenter.CurrentSortColumn = currentSortColumn;
				_scheduleView.Presenter.IsAscendingSort = isAscendingSort;
				_scheduleView.RefreshSelectionInfo += scheduleViewRefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor += scheduleViewRefreshShiftEditor;
				_scheduleView.ViewPasteCompleted += currentViewViewPasteCompleted;
				_scheduleView.LoadScheduleViewGrid();

				if (scheduleParts != null)
				{
					_scheduleView.SetSelectionFromParts(scheduleParts);
					updateShiftEditor();
				}
			}
			schedulerSplitters1.ResumeLayout(true);

			if (level == ZoomLevel.DayView)
			{
				_grid.Model.Selections.Clear(true);
				_grid.Model.Selections.SelectRange(
					GridRangeInfo.Cell(_grid.CurrentCell.RowIndex, _grid.CurrentCell.ColIndex), true);

				if (_scheduleView != null)
				{
					_scheduleView.SetSelectionFromParts(scheduleParts);
				}
			}
		}

		private void agentRestrictionGridSelectedAgentIsReady(object sender, EventArgs e)
		{
			AgentRestrictionsDetailView view = _scheduleView as AgentRestrictionsDetailView;
			if (view == null)
				return;

			if (view.TheGrid.InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(agentRestrictionGridSelectedAgentIsReady), sender, e);
			}
			else
			{
				_scheduleView.TheGrid.Refresh();
				view.InitializeGrid();
				var args = e as AgentDisplayRowEventArgs;
				if (args == null) return;
				if (args.MoveToDate) view.SelectDateIfExists(_dateNavigateControl.SelectedDate);
				if (args.UpdateShiftEditor) updateShiftEditor();
			}
		}

		private void scheduleViewRefreshShiftEditor(object sender, EventArgs e)
		{
			updateShiftEditor();
		}

		private void scheduleViewRefreshSelectionInfo(object sender, EventArgs e)
		{
			updateSelectionInfo(_scheduleView.SelectedSchedules());
		}

		private void updateRequestCommandsAvailability()
		{
			if (_requestView != null)
			{
				toolStripExHandleRequests.Enabled = _requestView.IsSelectionEditable() &&
													_permissionHelper.IsPermittedApproveRequest(_requestView.SelectedAdapters());
			}
		}

		private void _requestView_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			enableSave();
		}

		private void loadScenarioMenuItems()
		{
			IList<IScenario> scenarios;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IScenarioRepository scenarioRepository = new ScenarioRepository(uow);
				scenarios = scenarioRepository.FindAllSorted(); // Ascending or Descending ?
			}
			var authorization = PrincipalAuthorization.Current();

			for (var i = scenarios.Count - 1; i > -1; i--)
			{
				if (scenarios[i].Restricted &&
					!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
					scenarios.RemoveAt(i);
			}

			if (RightToLeftLayout) flowLayoutExportToScenario.ReverseRows = true;

			foreach (var scenario in scenarios)
			{
				if (_scenario.Description.Name == scenario.Description.Name) continue;
				var button = new ButtonAdv
				{
					Text = scenario.Description.Name,
					Width = 300,
					Height = 80,
					Appearance = ButtonAppearance.Metro,
					UseVisualStyle = true,
					Tag = scenario
				};

				button.Font.ChangeToBold();
				button.Click += menuItemClick;
				flowLayoutExportToScenario.ContainerControl.Controls.Add(button);
			}
		}

		private void menuItemClick(object sender, EventArgs e)
		{
			var buttonAdv = sender as ButtonAdv;
			var toolStripMenuItem = sender as ToolStripMenuItem;
			IScenario scenario = null;

			if (buttonAdv != null) scenario = (IScenario) (buttonAdv).Tag;
			if (toolStripMenuItem != null) scenario = (IScenario) (toolStripMenuItem).Tag;

			if (scenario == null) return;

			var allNewRules = _schedulerState.SchedulingResultState.GetRulesToRun();
			var selectedSchedules = _scheduleView.SelectedSchedules();
			var uowFactory = UnitOfWorkFactory.Current;
			var toggleManager = _container.Resolve<IToggleManager>();
			var scheduleRepository = new ScheduleStorage(new FromFactory(() => uowFactory), new RepositoryFactory(), new PersistableScheduleDataPermissionChecker(), _container.Resolve<IScheduleStorageRepositoryWrapper>());
			var exportToScenarioAccountPersister = new ExportToScenarioAccountPersister(_container.Resolve<IPersonAccountPersister>());
			var exportToScenarioAbsenceFinder = new ExportToScenarioAbsenceFinder();
			using (
				var exportForm = new ExportToScenarioResultView(uowFactory, scheduleRepository,
					new MoveDataBetweenSchedules(allNewRules, _container.Resolve<IScheduleDayChangeCallback>()),
					_schedulerMessageBrokerHandler,
					_scheduleView.AllSelectedPersons(selectedSchedules),
					selectedSchedules,
					scenario,
					_container.Resolve<IScheduleDictionaryPersister>(),
					exportToScenarioAccountPersister,
					exportToScenarioAbsenceFinder,
					SchedulerState.SchedulingResultState.AllPersonAccounts,
					_scheduleView.AllSelectedDates(selectedSchedules)))
			{
				exportForm.ShowDialog(this);
			}
		}

		private void loadLockMenues()
		{
			if (_scheduleView == null) return;

			var lockAbsencesMenuBuilder = new LockAbsencesMenuBuilder();
			lockAbsencesMenuBuilder.Build(_schedulerState.CommonStateHolder.Absences, toolStripMenuItemLockAbsenceDaysClick,
				toolStripMenuItemLockAbsenceDaysMouseUp, toolStripMenuItemLockAbsence,
				toolStripMenuItemLockAbsencesRM, toolStripMenuItemLockAbsencesClick,
				toolStripMenuItemAbsenceLockRmMouseUp);

			var lockDaysOffMenuBuilder = new LockDaysOffMenuBuilder();
			lockDaysOffMenuBuilder.Build(_schedulerState.CommonStateHolder.DayOffs, toolStripMenuItemLockFreeDaysClick,
				toolStripMenuItemLockSpecificDayOffClick, toolStripMenuItemDayOffLockRmMouseUp,
				toolStripMenuItemLockDayOff, toolStripMenuItemLockFreeDaysRM);

			var lockShiftCategoriesMenuBuilder = new LockShiftCategoriesMenuBuilder();
			lockShiftCategoriesMenuBuilder.Build(_schedulerState.CommonStateHolder.ShiftCategories,
				toolStripMenuItemLockShiftCategoryDaysClick,
				toolStripMenuItemLockShiftCategoryDaysMouseUp,
				toolStripMenuItemLockShiftCategory, toolStripMenuItemLockShiftCategoriesRM,
				toolStripMenuItemLockShiftCategoriesClick,
				toolStripMenuItemLockShiftCategoriesMouseUp);

			var tagsMenuLoader = new TagsMenuLoader(toolStripMenuItemLockTags, toolStripMenuItemLockTagsRM,
				_schedulerState.CommonStateHolder.ScheduleTags, toolStripMenuItemLockTag,
				toolStripSplitButtonChangeTag, toolStripMenuItemChangeTag,
				toolStripComboBoxAutoTag, _defaultScheduleTag, toolStripMenuItemChangeTagRM);
			tagsMenuLoader.LoadTags();
		}

		private void enableSwapButtons(IList<IScheduleDay> selectedSchedules)
		{
			SchedulerRibbonHelper.EnableSwapButtons(selectedSchedules, _scheduleView, toolStripMenuItemSwap,
				toolStripMenuItemSwapAndReschedule,
				ToolStripMenuItemSwapRaw, toolStripDropDownButtonSwap, _permissionHelper, _teamLeaderMode,
				_temporarySelectedEntitiesFromTreeView, _grid);
		}

		private void updateSelectionInfo(IList<IScheduleDay> selectedSchedules)
		{
			var updater = new UpdateSelectionForAgentInfo(toolStripStatusLabelContractTime, toolStripStatusLabelScheduleTag);
			updater.Update(selectedSchedules, _scheduleView, _schedulerState, _agentInfoControl, _scheduleTimeType,
				_showInfoPanel);
		}

		private void drawSkillGrid()
		{
			if (_teamLeaderMode || _scheduleView == null)
				return;

			if (_tabSkillData.SelectedIndex >= 0)
			{
				_currentIntraDayDate = _scheduleView.SelectedDateLocal();
				TabPageAdv tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
				var skill = (ISkill) tab.Tag;
				IAggregateSkill aggregateSkillSkill = skill;
				_chartDescription = skill.Name;

				var skillGridControl = resolveControlFromSkillResultViewSetting();
				if (skillGridControl is SkillIntradayGridControl)
				{
					drawIntraday(skill, aggregateSkillSkill);
					return;
				}

				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return;

				if (selectedSkillGridControl is SkillFullPeriodGridControl)
				{
					if (TestMode.Micke)
					{
						positionControl(_skillFullPeriodGridControl, SkillFullPeriodGridControl.PreferredGridWidth);
						TabPageAdv thisTab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
						thisTab.Controls.Add(_skillResultHighlightGridControl);
						_skillResultHighlightGridControl.DrawGridContents(_schedulerState, skill);
						_skillResultHighlightGridControl.Left = SkillFullPeriodGridControl.PreferredGridWidth + 5;
						_skillResultHighlightGridControl.Top = 0;
						_skillResultHighlightGridControl.Width = thisTab.Width - _skillResultHighlightGridControl.Left;
						_skillResultHighlightGridControl.Height = thisTab.Height;
						_skillResultHighlightGridControl.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top |
																  AnchorStyles.Left;
					}
					else
					{
						positionControl(skillGridControl);
					}

					ActiveControl = skillGridControl;
					selectedSkillGridControl.DrawDayGrid(_schedulerState, skill);
					selectedSkillGridControl.DrawDayGrid(_schedulerState, skill);
					return;
				}

				positionControl(skillGridControl);
				ActiveControl = skillGridControl;
				selectedSkillGridControl.DrawDayGrid(_schedulerState, skill);
				selectedSkillGridControl.DrawDayGrid(_schedulerState, skill);
			}
		}

		private TeleoptiGridControl resolveControlFromSkillResultViewSetting()
		{
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
				return _skillIntradayGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Day))
				return _skillDayGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week))
				return _skillWeekGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month))
				return _skillMonthGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period))
				return _skillFullPeriodGridControl;

			return null;
		}

		private void refreshSummarySkillIfActive()
		{
			if (_tabSkillData.SelectedIndex < 0) return;
			var tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
			var skill = (ISkill) tab.Tag;
			IAggregateSkill aggregateSkillSkill = skill;
			if (!aggregateSkillSkill.IsVirtual)
				return;

			var skillGridControl = resolveControlFromSkillResultViewSetting();
			if (skillGridControl is SkillIntradayGridControl)
			{
				var skillStaffPeriods = SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					aggregateSkillSkill,
					TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
						_currentIntraDayDate.AddDays(1).Date, _schedulerState.TimeZoneInfo));
				if (_skillIntradayGridControl.Presenter.RowManager != null)
					_skillIntradayGridControl.Presenter.RowManager.SetDataSource(skillStaffPeriods);
			}
			else
			{
				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return;

				selectedSkillGridControl.SetDataSource(_schedulerState, skill);
			}

			skillGridControl.Refresh();
		}

		private void drawIntraday(ISkill skill, IAggregateSkill aggregateSkillSkill)
		{
			IList<ISkillStaffPeriod> skillStaffPeriods;
			var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
				_currentIntraDayDate.AddDays(1).Date, _schedulerState.TimeZoneInfo);
			if (aggregateSkillSkill.IsVirtual)
			{
				SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, periodToFind);
				skillStaffPeriods =
					SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, periodToFind);
			}
			else
			{
				skillStaffPeriods =
					SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> {skill},
						periodToFind);
			}
			if (skillStaffPeriods.Count >= 0)
			{
				_chartDescription = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", skill.Name,
					_currentIntraDayDate.ToShortDateString());
				_skillIntradayGridControl.SetupDataSource(skillStaffPeriods, skill, _schedulerState);
				_skillIntradayGridControl.SetRowsAndCols();
				positionControl(_skillIntradayGridControl);
			}
		}

		private void loadSkillDays(IUnitOfWork uow, ISchedulerStateHolder stateHolder,
			Action<ILoaderDeciderResult> setDeciderResult, Func<ILoaderDeciderResult> getDeciderResult)
		{
			if (_teamLeaderMode) return;
			using (PerformanceOutput.ForOperation("Loading skill days"))
			{
				stateHolder.SchedulingResultState.SkillDays = _skillDayLoadHelper.LoadSchedulerSkillDays(
					new DateOnlyPeriod(stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8),
						stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8)), stateHolder.SchedulingResultState.Skills,
					stateHolder.RequestedScenario);

				_container.Resolve<IInitMaxSeatForStateHolder>().Execute(stateHolder.SchedulingResultState.MinimumSkillIntervalLength());

					IList<ISkillStaffPeriod> skillStaffPeriods =
					stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						stateHolder.SchedulingResultState.Skills, stateHolder.LoadedPeriod.Value);

				foreach (ISkillStaffPeriod period in skillStaffPeriods)
				{
					period.Payload.UseShrinkage = _shrinkage;
				}
			}
		}

		private void positionControl(Control control)
		{
			//remove control from all tabPages
			foreach (TabPageAdv tabPage in _tabSkillData.TabPages)
			{
				tabPage.Controls.Clear();
			}

			TabPageAdv tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
			tab.Controls.Add(control);

			//position _grid
			control.Dock = DockStyle.Fill;
		}

		private void positionControl(Control control, int width)
		{
			//remove control from all tabPages
			foreach (TabPageAdv tabPage in _tabSkillData.TabPages)
			{
				tabPage.Controls.Clear();
			}
			TabPageAdv tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
			tab.Controls.Add(control);
			tab.BackColor = control.BackColor;

			//position _grid
			control.Dock = DockStyle.Left;
			control.Width = width;
		}

		public void RefreshSelection()
		{
			if (_scheduleView != null)
			{
				GridRangeInfoList rangeList = GridHelper.GetGridSelectedRanges(_scheduleView.ViewGrid, true);
				foreach (GridRangeInfo rangeInfo in rangeList)
				{
					_scheduleView.ViewGrid.RefreshRange(rangeInfo, true);
				}
			}
		}

		private int checkIfUserWantsToSaveUnsavedData()
		{
			if (_schedulerState.Schedules == null)
				return 0;

			if (!_schedulerState.Schedules.DifferenceSinceSnapshot().IsEmpty() || _schedulerState.ChangedRequests() ||
				!_modifiedWriteProtections.IsEmpty() || !_schedulerState.CommonStateHolder.ModifiedWorkflowControlSets.IsEmpty())
			{
				DialogResult res = ShowConfirmationMessage(Resources.DoYouWantToSaveChangesYouMade, Resources.Save);
				switch (res)
				{
					case DialogResult.Cancel:
						return -1;
					case DialogResult.No:
						return 1;
					case DialogResult.Yes:
						var savedOk = save();
						if (savedOk)
							return 1;
						return -1;
				}
			}
			return 1;
		}

		private void selectCellFromPersonDate(IPerson person, DateOnly localDate)
		{
			if (_scheduleView != null)
			{
				Point point = _scheduleView.GetCellPositionForAgentDay(person, localDate);
				if (point.X != -1 && point.Y != -1)
				{
					_scheduleView.ViewGrid.Selections.Clear(true);
					_grid.CurrentCell.MoveTo(point.Y, point.X, GridSetCurrentCellOptions.None);
					_grid.Selections.SelectRange(GridRangeInfo.Cell(point.Y, point.X), true);
				}
			}
		}

		private void reloadChart()
		{
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Week, _chartDescription);
				_gridChartManager.ReloadChart(_skillWeekGridControl, description);
				_chartInIntradayMode = false;
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Month, _chartDescription);
				_gridChartManager.ReloadChart(_skillMonthGridControl, description);
				_chartInIntradayMode = false;
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Period, _chartDescription);
				_gridChartManager.ReloadChart(_skillFullPeriodGridControl, description);
				_chartInIntradayMode = false;
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Intraday, _chartDescription);
				_gridChartManager.ReloadChart(_skillIntradayGridControl, description);
				_chartInIntradayMode = true;
			}
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Day))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Day, _chartDescription);
				_gridChartManager.ReloadChart(_skillDayGridControl, description);
				_chartInIntradayMode = false;
			}
			_chartControlSkillData.Visible = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void setEventHandlers()
		{
			schedulerSplitters1.ValidationAlertsAgentDoubleClick += schedulerSplitters1ValidationAlertsAgentDoubleClick;
			_schedulerMeetingHelper.ModificationOccured += schedulerMeetingHelperModificationOccured;
			_tmpTimer.Tick += tmpTimerTick;
			schedulerSplitters1.TabSkillData.SelectedIndexChanged += tabSkillDataSelectedIndexChanged;
			_grid.CurrentCellKeyDown += gridCurrentCellKeyDown;
			_grid.GotFocus += gridGotFocus;
			_grid.SelectionChanged += gridSelectionChanged;
			_grid.Click += grid_Click;
			_grid.ScrollControlMouseUp += _grid_ScrollControlMouseUp;
			_grid.StartAutoScrolling += _grid_StartAutoScrolling;

			wpfShiftEditor1.ShiftUpdated += wpfShiftEditor1ShiftUpdated;
			wpfShiftEditor1.CommitChanges += wpfShiftEditor1CommitChanges;
			wpfShiftEditor1.EditMeeting += wpfShiftEditor1_EditMeeting;
			wpfShiftEditor1.RemoveParticipant += wpfShiftEditor1_RemoveParticipant;
			wpfShiftEditor1.DeleteMeeting += wpfShiftEditor1_DeleteMeeting;
			wpfShiftEditor1.CreateMeeting += wpfShiftEditor1_CreateMeeting;
			wpfShiftEditor1.AddAbsence += wpfShiftEditor_AddAbsence;
			wpfShiftEditor1.AddActivity += wpfShiftEditor_AddActivity;
			wpfShiftEditor1.AddOvertime += wpfShiftEditor_AddOvertime;
			wpfShiftEditor1.AddPersonalShift += wpfShiftEditor_AddPersonalShift;
			wpfShiftEditor1.Undo += wpfShiftEditor_Undo;
			wpfShiftEditor1.ShowLayers += wpfShiftEditor1ShowLayers;

			notesEditor.NotesChanged += notesEditor_NotesChanged;
			notesEditor.PublicNotesChanged += notesEditor_PublicNotesChanged;

			_skillDayGridControl.GotFocus += skillGridControlGotFucus;
			_skillIntradayGridControl.GotFocus += skillGridControlGotFucus;
			_skillWeekGridControl.GotFocus += skillGridControlGotFucus;
			_skillMonthGridControl.GotFocus += skillGridControlGotFucus;
			_skillFullPeriodGridControl.GotFocus += skillGridControlGotFucus;

			_skillDayGridControl.SelectionChanged += skillGridControlSelectionChanged;
			_skillIntradayGridControl.SelectionChanged += skillIntradayGridControlSelectionChanged;
			_skillWeekGridControl.SelectionChanged += skillGridControlSelectionChanged;
			_skillMonthGridControl.SelectionChanged += skillGridControlSelectionChanged;
			_skillFullPeriodGridControl.SelectionChanged += skillGridControlSelectionChanged;
			_skillResultHighlightGridControl.GoToDate += _skillResultHighlightGridControl_GoToDate;

			_gridrowInChartSettingButtons.LineInChartSettingsChanged += gridlinesInChartSettingsLineInChartSettingsChanged;
			_gridrowInChartSettingButtons.LineInChartEnabledChanged += gridrowInChartSettingLineInChartEnabledChanged;
			_chartControlSkillData.ChartRegionMouseHover += chartControlSkillDataChartRegionMouseHover;
			_chartControlSkillData.ChartRegionClick += chartControlSkillDataChartRegionClick;
			_undoRedo.ChangedHandler += undoRedo_Changed;

			#region eventaggregator

			_eventAggregator.GetEvent<GenericEvent<HandlePersonRequestSelectionChanged>>().Subscribe(requestSelectionChanged);
			_eventAggregator.GetEvent<GenericEvent<ShowRequestDetailsView>>().Subscribe(showRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ApproveRequestFromRequestDetailsView>>()
				.Subscribe(approveRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<DenyRequestFromRequestDetailsView>>()
				.Subscribe(denyRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyRequestFromRequestDetailsView>>()
				.Subscribe(replyRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyAndApproveRequestFromRequestDetailsView>>()
				.Subscribe(replyAndApproveRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyAndDenyRequestFromRequestDetailsView>>()
				.Subscribe(replyAndDenyRequestFromRequestDetailsView);

			#endregion
		}

		private void schedulerSplitters1ValidationAlertsAgentDoubleClick(object sender, ValidationViewAgentDoubleClickEvenArgs e)
		{
			var row = _scheduleView.GetRowForAgent(e.Person);
			int column = _scheduleView.GetColumnForDate(e.Date);
			GridRangeInfo info = GridRangeInfo.Cells(row, column, row, column);
			_scheduleView.TheGrid.Selections.Clear(true);
			_scheduleView.TheGrid.CurrentCell.Activate(row, column, GridSetCurrentCellOptions.SetFocus);
			_scheduleView.TheGrid.Selections.ChangeSelection(info, info, true);
			_scheduleView.TheGrid.CurrentCell.MoveTo(row, column, GridSetCurrentCellOptions.ScrollInView);
			_scheduleView.SetSelectedDateLocal(e.Date);
			updateShiftEditor();
		}

		private void wpfShiftEditor1ShowLayers(object sender, EventArgs e)
		{
			RunActionWithDelay(updateShiftEditor, 50);
		}

		private void _skillResultHighlightGridControl_GoToDate(object sender, GoToDateEventArgs e)
		{
			_scheduleView.SetSelectedDateLocal(e.Date);
		}

		private void _grid_StartAutoScrolling(object sender, StartAutoScrollingEventArgs e)
		{
			if (e.Reason == AutoScrollReason.MouseDragging)
				_grid.SupportsPrepareViewStyleInfo = false;
		}

		private void _grid_ScrollControlMouseUp(object sender, CancelMouseEventArgs e)
		{
			_grid.SupportsPrepareViewStyleInfo = true;
			_grid.Invalidate();
		}

		private void replyAndDenyRequestFromRequestDetailsView(EventParameters<ReplyAndDenyRequestFromRequestDetailsView> obj)
		{
			var denyCommand = new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker);
			IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>() {obj.Value.Request};
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList, denyCommand))
			{
				dialog.ShowDialog();
			}
			recalculateResourcesForRequests(selectedRequestList);
		}

		private void replyAndApproveRequestFromRequestDetailsView(
			EventParameters<ReplyAndApproveRequestFromRequestDetailsView> obj)
		{

			var businessRules = _schedulerState.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var approveRequestCommand = new ApprovePersonRequestCommand(this, _schedulerState.Schedules,
					_schedulerState.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse, _personRequestAuthorizationChecker, businessRules,
					_overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository);

				IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>() {obj.Value.Request};
				using (
					var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList,
						approveRequestCommand))
				{
					dialog.ShowDialog();
				}
				recalculateResourcesForRequests(selectedRequestList);
			}
			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();

		}

		private void replyRequestFromRequestDetailsView(EventParameters<ReplyRequestFromRequestDetailsView> eventParameters)
		{
			IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>()
			{
				eventParameters.Value.Request
			};
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList))
			{
				dialog.ShowDialog();
			}
		}

		private void denyRequestFromRequestDetailsView(EventParameters<DenyRequestFromRequestDetailsView> eventParameters)
		{
			IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>()
			{
				eventParameters.Value.Request
			};

			changeRequestStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker),
				selectedRequestList);

		}

		private void approveRequestFromRequestDetailsView(
			EventParameters<ApproveRequestFromRequestDetailsView> eventParameters)
		{
			var allNewBusinessRules = _schedulerState.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var approvePersonRequestCommand = new ApprovePersonRequestCommand(this, _schedulerState.Schedules,
					_schedulerState.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse,
					_personRequestAuthorizationChecker, allNewBusinessRules, _overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository);

				var selectedAdapters = new List<PersonRequestViewModel>() {eventParameters.Value.Request};

				changeRequestStatus(approvePersonRequestCommand, selectedAdapters);
			}

			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void setEventHandlersOff()
		{
			//Request tab
			toolStripTabItem1.Click -= toolStripTabItem1_Click;
			schedulerSplitters1.ValidationAlertsAgentDoubleClick -= schedulerSplitters1ValidationAlertsAgentDoubleClick;
			toolStripButtonRequestBack.Click -= toolStripButtonRequestBackClick;
			toolStripButtonFilterAgentsRequestView.Click -= toolStripButtonFilterAgentsClick;
			ToolStripMenuItemViewDetails.Click -= ToolStripMenuItemViewDetails_Click;
			toolStripButtonViewAllowance.Click -= toolStripItemViewAllowanceClick;
			toolStripButtonViewRequestHistory.Click -= toolStripViewRequestHistoryClick;
			toolStripButtonApproveRequest.Click -= toolStripButtonApproveRequestClick;
			toolStripButtonDenyRequest.Click -= toolStripButtonDenyRequestClick;
			toolStripButtonEditNote.Click -= toolStripButtonEditNote_Click;
			toolStripButtonReplyAndApprove.Click -= toolStripButtonReplyAndApprove_Click;
			toolStripButtonReplyAndDeny.Click -= toolStripButtonReplyAndDeny_Click;
			//Chart tab
			toolStripTabItemChart.Click -= toolStripTabItemChart_Click;
			toolStripButtonGridInChart.Click -= toolStripButtonGridInChartClick;
			toolStripButtonChartPeriodView.Click -= toolStripButtonChartPeriodViewClick;
			toolStripButtonChartMonthView.Click -= toolStripButtonChartPeriodViewClick;
			toolStripButtonChartWeekView.Click -= toolStripButtonChartPeriodViewClick;
			toolStripButtonChartDayView.Click -= toolStripButtonChartPeriodViewClick;
			toolStripButtonChartIntradayView.Click -= toolStripButtonChartPeriodViewClick;
			//
			toolStripButtonFilterStudentAvailability.Click -= toolStripButtonFilterStudentAvailabilityClick;
			toolStripButtonFilterOvertimeAvailability.Click -= toolStripButtonFilterOvertimeAvailabilityClick;
			toolStripButtonFilterAgents.Click -= toolStripButtonFilterAgentsClick;
			toolStripTabItemHome.Click -= toolStripTabItemHome_Click;
			toolStripTabItemChart.Click -= toolStripTabItemChart_Click;
			toolStripTabItem1.Click -= toolStripTabItem1_Click;
			if (_schedulerMeetingHelper != null)
				_schedulerMeetingHelper.ModificationOccured -= schedulerMeetingHelperModificationOccured;
			if (_tmpTimer != null)
				_tmpTimer.Tick -= tmpTimerTick;
			if (_dateNavigateControl != null)
			{
				_dateNavigateControl.SelectedDateChanged -= dateNavigateControlSelectedDateChanged;
				_dateNavigateControl.ClosedPopup -= dateNavigateControlClosedPopup;
			}
			backStageButtonMainMenuSave.Click -= toolStripButtonMainMenuSaveClick;
			backStageButtonMainMenuHelp.Click -= toolStripButtonMainMenuHelpClick;
			backStageButtonMainMenuClose.Click -= toolStripButtonMainMenuCloseClick;
			backStageButtonOptions.Click -= toolStripButtonOptions_Click;
			backStageButtonSystemExit.Click -= toolStripButtonSystemExitClick;
			backStage1.VisibleChanged -= backStage1VisibleChanged;

			if (flowLayoutExportToScenario != null && flowLayoutExportToScenario.ContainerControl != null)
			{
				foreach (var control in flowLayoutExportToScenario.ContainerControl.Controls)
				{
					ButtonAdv button = control as ButtonAdv;
					if (button != null)
						button.Click -= menuItemClick;
				}
			}
			toolStripButtonSaveLarge.Click -= toolStripButtonSaveLargeClick;
			toolStripButtonRefreshLarge.Click -= toolStripButtonRefreshLargeClick;
			toolStripButtonQuickAccessCancel.Click -= toolStripButtonQuickAccessCancelClick;
			toolStripButtonQuickAccessRedo.MouseUp -= toolStripButtonQuickAccessRedo_Click_1;
			toolStripMenuItemQuickAccessUndo.Click -= toolStripSplitButtonQuickAccessUndo_ButtonClick;
			toolStripMenuItemQuickAccessUndoAll.Click -= toolStripMenuItemQuickAccessUndoAll_Click_1;
			toolStripButtonShowTexts.Click -= toolStripButtonShowTexts_Click;

			if (_permissionHelper != null)
				_permissionHelper = null;

			if (_schedulerMessageBrokerHandler != null)
			{
				_schedulerMessageBrokerHandler.RequestDeletedFromBroker -= schedulerMessageBrokerHandlerRequestDeletedFromBroker;
				_schedulerMessageBrokerHandler.RequestInsertedFromBroker -= schedulerMessageBrokerHandlerRequestInsertedFromBroker;
				_schedulerMessageBrokerHandler.SchedulesUpdatedFromBroker -= schedulerMessageBrokerHandlerSchedulesUpdatedFromBroker;

				_schedulerMessageBrokerHandler.Dispose();
				_schedulerMessageBrokerHandler = null; // referens till SchedulingScreen
			}
			_requestPresenter = null; // referens till SchedulingScreen

			if (backgroundWorkerLoadData != null)
			{
				backgroundWorkerLoadData.DoWork -= backgroundWorkerLoadDataDoWork;
				backgroundWorkerLoadData.RunWorkerCompleted -= backgroundWorkerLoadDataRunWorkerCompleted;
				backgroundWorkerLoadData.ProgressChanged -= backgroundWorkerLoadDataProgressChanged;
			}

			if (_backgroundWorkerDelete != null)
			{
				_backgroundWorkerDelete.DoWork -= backgroundWorkerDeleteDoWork;
				_backgroundWorkerDelete.RunWorkerCompleted -= backgroundWorkerDeleteRunWorkerCompleted;
			}

			if (_backgroundWorkerResourceCalculator != null)
			{
				_backgroundWorkerResourceCalculator.DoWork -= backgroundWorkerResourceCalculatorDoWork;
				_backgroundWorkerResourceCalculator.ProgressChanged -= backgroundWorkerResourceCalculatorProgressChanged;
				_backgroundWorkerResourceCalculator.RunWorkerCompleted -= backgroundWorkerResourceCalculatorRunWorkerCompleted;
			}

			if (_backgroundWorkerValidatePersons != null)
			{
				_backgroundWorkerValidatePersons.RunWorkerCompleted -= backgroundWorkerValidatePersonsRunWorkerCompleted;
				_backgroundWorkerValidatePersons.DoWork -= backgroundWorkerValidatePersonsDoWork;
			}

			if (_backgroundWorkerScheduling != null)
			{
				_backgroundWorkerScheduling.DoWork -= backgroundWorkerSchedulingDoWork;
				_backgroundWorkerScheduling.ProgressChanged -= backgroundWorkerSchedulingProgressChanged;
				_backgroundWorkerScheduling.RunWorkerCompleted -= backgroundWorkerSchedulingRunWorkerCompleted;
			}

			if (_backgroundWorkerOvertimeScheduling != null)
			{
				_backgroundWorkerOvertimeScheduling.DoWork -= backgroundWorkerOvertimeSchedulingDoWork;
				_backgroundWorkerOvertimeScheduling.ProgressChanged -= backgroundWorkerOvertimeSchedulingProgressChanged;
				_backgroundWorkerOvertimeScheduling.RunWorkerCompleted -= backgroundWorkerOvertimeSchedulingRunWorkerCompleted;
			}

			if (_backgroundWorkerOptimization != null)
			{
				_backgroundWorkerOptimization.DoWork -= backgroundWorkerOptimizationDoWork;
				_backgroundWorkerOptimization.ProgressChanged -= backgroundWorkerOptimizationProgressChanged;
			}

			if (toolStripComboBoxAutoTag != null)
				toolStripComboBoxAutoTag.SelectedIndexChanged -= toolStripComboBoxAutoTagSelectedIndexChanged;

			if (SchedulerState != null && SchedulerState.Schedules != null)
				SchedulerState.Schedules.PartModified -= schedulesPartModified;

			if (_schedulerMeetingHelper != null)
				_schedulerMeetingHelper.ModificationOccured -= schedulerMeetingHelperModificationOccured;
			if (schedulerSplitters1 != null)
			{
				schedulerSplitters1.HandlePersonRequestView1.RemoveEvents();
				schedulerSplitters1.TabSkillData.SelectedIndexChanged -= tabSkillDataSelectedIndexChanged;
			}
			if (_grid != null)
			{
				_grid.CurrentCellKeyDown -= gridCurrentCellKeyDown;
				_grid.GotFocus -= gridGotFocus;
				_grid.SelectionChanged -= gridSelectionChanged;
				_grid.StartAutoScrolling -= _grid_StartAutoScrolling;
				_grid.ScrollControlMouseUp -= _grid_ScrollControlMouseUp;
				_grid.Click -= grid_Click;
			}

			if (wpfShiftEditor1 != null)
			{
				wpfShiftEditor1.ShiftUpdated -= wpfShiftEditor1ShiftUpdated;
				wpfShiftEditor1.CommitChanges -= wpfShiftEditor1CommitChanges;
				wpfShiftEditor1.EditMeeting -= wpfShiftEditor1_EditMeeting;
				wpfShiftEditor1.RemoveParticipant -= wpfShiftEditor1_RemoveParticipant;
				wpfShiftEditor1.DeleteMeeting -= wpfShiftEditor1_DeleteMeeting;
				wpfShiftEditor1.CreateMeeting -= wpfShiftEditor1_CreateMeeting;

				wpfShiftEditor1.AddAbsence -= wpfShiftEditor_AddAbsence;
				wpfShiftEditor1.AddActivity -= wpfShiftEditor_AddActivity;
				wpfShiftEditor1.AddOvertime -= wpfShiftEditor_AddOvertime;
				wpfShiftEditor1.AddPersonalShift -= wpfShiftEditor_AddPersonalShift;
				wpfShiftEditor1.ShowLayers -= wpfShiftEditor1ShowLayers;
			}

			if (notesEditor != null)
			{
				notesEditor.NotesChanged -= notesEditor_NotesChanged;
				notesEditor.PublicNotesChanged -= notesEditor_PublicNotesChanged;
			}
			if (_requestView != null)
			{
				_requestView.PropertyChanged -= _requestView_PropertyChanged;
				_requestView.SelectionChanged -= requestViewSelectionChanged;
			}

			if (_skillResultHighlightGridControl != null)
				_skillResultHighlightGridControl.GoToDate -= _skillResultHighlightGridControl_GoToDate;

			if (_skillDayGridControl != null)
				_skillDayGridControl.GotFocus -= skillGridControlGotFucus;
			if (_skillIntradayGridControl != null)
				_skillIntradayGridControl.GotFocus -= skillGridControlGotFucus;

			if (_skillWeekGridControl != null)
				_skillWeekGridControl.GotFocus -= skillGridControlGotFucus;

			if (_skillMonthGridControl != null)
				_skillMonthGridControl.GotFocus -= skillGridControlGotFucus;

			if (_skillFullPeriodGridControl != null)
				_skillFullPeriodGridControl.GotFocus -= skillGridControlGotFucus;

			if (_skillDayGridControl != null)
				_skillDayGridControl.SelectionChanged -= skillGridControlSelectionChanged;

			if (_skillIntradayGridControl != null)
				_skillIntradayGridControl.SelectionChanged -= skillIntradayGridControlSelectionChanged;

			if (_skillWeekGridControl != null)
				_skillWeekGridControl.SelectionChanged -= skillGridControlSelectionChanged;

			if (_skillMonthGridControl != null)
				_skillMonthGridControl.SelectionChanged -= skillGridControlSelectionChanged;

			if (_skillFullPeriodGridControl != null)
				_skillFullPeriodGridControl.SelectionChanged -= skillGridControlSelectionChanged;

			if (_gridrowInChartSettingButtons != null)
			{
				_gridrowInChartSettingButtons.LineInChartSettingsChanged -= gridlinesInChartSettingsLineInChartSettingsChanged;
				_gridrowInChartSettingButtons.LineInChartEnabledChanged -= gridrowInChartSettingLineInChartEnabledChanged;
			}

			if (_chartControlSkillData != null)
			{
				_chartControlSkillData.ChartRegionMouseHover -= chartControlSkillDataChartRegionMouseHover;
				_chartControlSkillData.ChartRegionClick -= chartControlSkillDataChartRegionClick;
			}

			if (_clipboardControl != null)
			{
				_clipboardControl.CutSpecialClicked -= clipboardControlCutSpecialClicked;
				_clipboardControl.CutClicked -= clipboardControlCutClicked;
				_clipboardControl.PasteSpecialClicked -= clipboardControlPasteSpecialClicked;
				_clipboardControl.PasteClicked -= clipboardControlPasteClicked;
				_clipboardControl.CopySpecialClicked -= clipboardControlCopySpecialClicked;
				_clipboardControl.CopyClicked -= clipboardControlCopyClicked;
			}

			if (_clipboardControlRestrictions != null)
			{
				_clipboardControlRestrictions.CopyClicked -= toolStripMenuItemRestrictionCopyClick;
				_clipboardControlRestrictions.PasteClicked -= toolStripMenuItemRestrictionPasteClick;
			}

			if (_editControl != null)
			{
				_editControl.NewSpecialClicked -= editControlNewSpecialClicked;
				_editControl.DeleteClicked -= editControlDeleteClicked;
				_editControl.DeleteSpecialClicked -= editControlDeleteSpecialClicked;
				_editControl.NewClicked -= editControlNewClicked;
			}

			if (_editControlRestrictions != null)
			{
				_editControlRestrictions.NewClicked -= editControlRestrictionsNewClicked;
				_editControlRestrictions.NewSpecialClicked -= editControlRestrictionsNewSpecialClicked;
				_editControlRestrictions.DeleteClicked -= toolStripMenuItemRestrictionDeleteClick;
			}

			if (toolStripMenuItemDeleteSpecial != null)
			{
				toolStripMenuItemDeleteSpecial.Click -= toolStripMenuItemDeleteSpecial2Click;
			}

			if (_undoRedo != null) _undoRedo.ChangedHandler -= undoRedo_Changed;

			if (contextMenuViews != null)
			{
				contextMenuViews.Opened -= contextMenuViewsOpened;
				contextMenuViews.Opening -= contextMenuViewsOpening;
			}

			if (_eventAggregator != null)
			{
				#region eventaggregator

				_eventAggregator.GetEvent<GenericEvent<HandlePersonRequestSelectionChanged>>().Unsubscribe(requestSelectionChanged);
				_eventAggregator.GetEvent<GenericEvent<ShowRequestDetailsView>>().Unsubscribe(showRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ApproveRequestFromRequestDetailsView>>()
					.Unsubscribe(approveRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<DenyRequestFromRequestDetailsView>>()
					.Unsubscribe(denyRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyRequestFromRequestDetailsView>>()
					.Unsubscribe(replyRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyAndApproveRequestFromRequestDetailsView>>()
					.Unsubscribe(replyAndApproveRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyAndDenyRequestFromRequestDetailsView>>()
					.Unsubscribe(replyAndDenyRequestFromRequestDetailsView);

				#endregion
			}
		}

		private void requestSelectionChanged(EventParameters<HandlePersonRequestSelectionChanged> eventParameters)
		{
			toolStripExHandleRequests.Enabled = eventParameters.Value.SelectionIsEditable &&
												_permissionHelper.IsPermittedApproveRequest(_requestView.SelectedAdapters());
			ToolStripMenuItemViewDetails.Enabled =
				toolStripButtonViewDetails.Enabled = _permissionHelper.IsViewRequestDetailsAvailable(_requestView);
		}

		private void wpfShiftEditor1_DeleteMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			_schedulerMeetingHelper.MeetingRemove(e.Value.BelongsToMeeting, _scheduleView);
		}

		private void wpfShiftEditor1_RemoveParticipant(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			IList<IPersonMeeting> meetings = new List<IPersonMeeting> {e.Value};
			_schedulerMeetingHelper.MeetingRemoveAttendees(meetings);
		}

		private void wpfShiftEditor1_EditMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool editPermission = _permissionHelper.IsPermittedToEditMeeting(_scheduleView,
				_temporarySelectedEntitiesFromTreeView, _scenario);
			bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
			_schedulerMeetingHelper.MeetingComposerStart(e.Value.BelongsToMeeting, _scheduleView, editPermission,
				viewSchedulesPermission, _container.Resolve<IToggleManager>());
		}

		private void wpfShiftEditor1_CreateMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
			_schedulerMeetingHelper.MeetingComposerStart(null, _scheduleView, true, viewSchedulesPermission,
				_container.Resolve<IToggleManager>());
		}

		private void notesEditor_NotesChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.LastUnsavedSchedulePart = notesEditor.SchedulePart;
				_scheduleView.Presenter.UpdateNoteFromEditor();
			}
			enableSave();
		}

		private void notesEditor_PublicNotesChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.LastUnsavedSchedulePart = notesEditor.SchedulePart;
				_scheduleView.Presenter.UpdatePublicNoteFromEditor();
			}
			enableSave();
		}

		private void grid_Click(object sender, EventArgs e)
		{
			//updateShiftEditor();
		}

		private void undoRedo_Changed(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(undoRedo_Changed), sender, e);
			}
			else
				enableUndoRedoButtons();
		}

		private void wpfShiftEditor_AddActivity(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;

			IActivity selectedActivity = null;
			if(e.SelectedLayer != null)
				selectedActivity = e.SelectedLayer.Payload as IActivity;

			_scheduleView.Presenter.AddActivity(new List<IScheduleDay> {e.SchedulePart}, e.Period, selectedActivity);
			RecalculateResources();
		}

		private void wpfShiftEditor_AddPersonalShift(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddPersonalShift(new List<IScheduleDay> {e.SchedulePart}, e.Period);
			RecalculateResources();
		}

		private void wpfShiftEditor_AddOvertime(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddOvertime(new List<IScheduleDay> {e.SchedulePart}, e.Period,
				MultiplicatorDefinitionSet.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime).ToList());
			RecalculateResources();
		}

		private void wpfShiftEditor_AddAbsence(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddAbsence(new List<IScheduleDay> {e.SchedulePart}, e.Period);
			RecalculateResources();
		}

		private void wpfShiftEditor_Undo(object sender, EventArgs e)
		{
			undoKeyDown();
		}

		private void setColor()
		{
			_grid.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
			for (int i = 0; i < ribbonControlAdv1.TabGroups.Count; i++)
			{
				ribbonControlAdv1.TabGroups[i].Color = ColorHelper.RibbonContextTabColor();
			}
		}

		#endregion

		// becaused called on another thread sometimes
		private delegate void ToggleQuickButtonEnabledState(ToolStripItem button, bool enable);

		private void toggleQuickButtonEnabledState(ToolStripItem button, bool enable)
		{
			if (InvokeRequired)
			{
				var paramsList = new object[] {button, enable};
				BeginInvoke(new ToggleQuickButtonEnabledState(toggleQuickButtonEnabledState), paramsList);
				return;
			}

			foreach (var quickItem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripButton>())
			{
				if (((QuickButtonReflectable) quickItem).ReflectedButton.Name == button.Name)
				{
					quickItem.Enabled = enable;
					return;
				}
			}

			foreach (var quickItem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripSplitButton>())
			{
				if (((QuickSplitButtonReflectable) quickItem).ReflectedSplitButton.Name == button.Name)
				{
					quickItem.Enabled = enable;
					return;
				}
			}
		}

		private void toggleQuickButtonEnabledState(bool enable)
		{

			foreach (var quickitem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripButton>())
			{
				quickitem.Enabled = enable;
			}

			foreach (var quickitem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripSplitButton>())
			{
				quickitem.Enabled = enable;
			}

			foreach (var quickitem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripDropDownButton>())
			{
				quickitem.Enabled = enable;
			}
		}

		private void toolStripButtonApproveRequestClick(object sender, EventArgs e)
		{
			var allNewBusinessRules = _schedulerState.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				changeRequestStatus(
					new ApprovePersonRequestCommand(this, _schedulerState.Schedules, _schedulerState.RequestedScenario,
						_requestPresenter, _handleBusinessRuleResponse,
						_personRequestAuthorizationChecker, allNewBusinessRules, _overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
						globalSettingRepository, personAbsenceAccountRepository), _requestView.SelectedAdapters());
			}

			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();
		}

		private void toolStripButtonDenyRequestClick(object sender, EventArgs e)
		{
			changeRequestStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker),
				_requestView.SelectedAdapters());
		}

		private void toolStripButtonEditNote_Click(object sender, EventArgs e)
		{
			IList<PersonRequestViewModel> selectedRequestList = _requestView.SelectedAdapters();
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList))
			{
				dialog.ShowDialog();
			}
		}

		private void toolStripButtonReplyAndApprove_Click(object sender, EventArgs e)
		{
			var businessRules = _schedulerState.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				replyAndChangeStatus(new ApprovePersonRequestCommand(this, _schedulerState.Schedules,
					_schedulerState.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse, _personRequestAuthorizationChecker, businessRules,
					_overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository));
			}
			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();
		}

		private void replyAndChangeStatus(IHandlePersonRequestCommand command)
		{
			IList<PersonRequestViewModel> selectedRequestList = _requestView.SelectedAdapters();
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList, command))
			{
				dialog.ShowDialog();
			}
			recalculateResourcesForRequests(selectedRequestList);
		}

		private void recalculateResourcesForRequests(IList<PersonRequestViewModel> selectedRequestList)
		{
			foreach (var personRequestViewModel in selectedRequestList)
			{
				var days =
					personRequestViewModel.PersonRequest.Request.Period.ToDateOnlyPeriod(
						TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).DayCollection();
				days.ForEach(_schedulerState.MarkDateToBeRecalculated);
			}
			RecalculateResources();
		}

		private void toolStripButtonReplyAndDeny_Click(object sender, EventArgs e)
		{
			replyAndChangeStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker));
		}

		private void toolStripButtonOptions_Click(object sender, EventArgs e)
		{
			var toggleManager = _container.Resolve<IToggleManager>();

			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(toggleManager, _container.Resolve<IBusinessRuleConfigProvider>())));
				settings.Show();
				settings.BringToFront();
			}
			catch (CouldNotCreateTransactionException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void ToolStripMenuItemScheduleHourlyEmployees_Click(object sender, EventArgs e)
		{
			scheduleHourlyEmployees();
		}

		private void SchedulingScreen_FormClosed(object sender, FormClosedEventArgs e)
		{
			// bug 28705 hide it so we don't get strange paint events
			Hide();
			_mainWindow.Activate();
			if (_schedulerState != null && _schedulerState.Schedules != null)
			{
				_schedulerState.Schedules.Clear();
			}
			setEventHandlersOff();
			_container.Dispose();
			if (_scheduleView != null)
			{
				_scheduleView.ViewPasteCompleted -= currentViewViewPasteCompleted;
				_scheduleView.RefreshSelectionInfo -= scheduleViewRefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor -= scheduleViewRefreshShiftEditor;
				_scheduleView.Dispose();
				_scheduleView = null;
			}
			_schedulerState = null;
			_schedulerMeetingHelper = null;

			if (wpfShiftEditor1 != null) wpfShiftEditor1.LoadSchedulePart(null);
			if (notesEditor != null) notesEditor.LoadNote(null);
			if (wpfShiftEditor1 != null) wpfShiftEditor1.Unload();
			wpfShiftEditor1 = null;
			if (_undoRedo != null) _undoRedo.Clear();

			notesEditor = null;

			if (_elementHostRequests != null && _elementHostRequests.Child != null) _elementHostRequests.Child = null;
			if (_grid != null) _grid.ContextMenu = null;
			if (contextMenuViews != null) contextMenuViews.Dispose();
			if (schedulerSplitters1 != null) schedulerSplitters1.Dispose();

			if (backStageView != null)
			{
				backStageView.HideBackStage();
				backStageView.BackStage = null;
				backStageView.HostControl = null;
				backStageView.HostForm = null;
				backStageView = null;
			}

			ribbonControlAdv1.BackStageView = null;
			backStage1.Parent = null;

			if (!Disposing)
			{
				Dispose(true);
			}
		}

		private void toolStripButtonQuickAccessRedo_Click_1(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Redo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void toolStripSplitButtonQuickAccessUndo_ButtonClick(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Undo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void toolStripMenuItemQuickAccessUndoAll_Click_1(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.UndoAll();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void undoRedoSelectAndRefresh()
		{
			if (_lastModifiedPart != null && _lastModifiedPart.ModifiedPart != null)
			{
				selectCellFromPersonDate(_lastModifiedPart.ModifiedPerson,_lastModifiedPart.ModifiedPart.DateOnlyAsPeriod.DateOnly);
			}

			RecalculateResources();
			if (_requestView != null)
				updateShiftEditor();
		}

		private void toolStripTabItem1_Click(object sender, EventArgs e)
		{
			ActiveControl = _elementHostRequests;
		}

		private void toolStripTabItemChart_Click(object sender, EventArgs e)
		{
			ActiveControl = _chartControlSkillData;
		}

		private void toolStripTabItemHome_Click(object sender, EventArgs e)
		{
			ActiveControl = null;
		}

		private void toolStripMenuItemUseShrinkage_Click(object sender, EventArgs e)
		{
			bool useShrinkage = !((ToolStripMenuItem) _contextMenuSkillGrid.Items["UseShrinkage"]).Checked;
			toggleShrinkage(useShrinkage);
		}

		private void toolStripMenuItemWriteProtectSchedule2_Click(object sender, EventArgs e)
		{
			writeProtectSchedule();
		}

		private void ToolstripMenuRemoveWriteProtectionMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection)) return;
			Cursor = Cursors.WaitCursor;
			var removeCommand = new WriteProtectionRemoveCommand(_scheduleView.SelectedSchedules(), _modifiedWriteProtections);
			removeCommand.Execute();
			GridHelper.GridlockWriteProtected(_schedulerState, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void writeProtectSchedule()
		{
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
				return;
			GridHelper.WriteProtectPersonSchedule(_grid).ForEach(_modifiedWriteProtections.Add);
			GridHelper.GridlockWriteProtected(_schedulerState, LockManager);
			_grid.Refresh();
			enableSave();
		}

		private void setupAvailTimeZones()
		{
			TimeZoneGuard.Instance.TimeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
			_schedulerState.TimeZoneInfo = TimeZoneGuard.Instance.TimeZone;
			wpfShiftEditor1.SetTimeZone(_schedulerState.TimeZoneInfo);

			foreach (TimeZoneInfo info in _detectedTimeZoneInfos)
			{
				var timeZoneItem = new ToolStripMenuItem(info.DisplayName) {Tag = info};
				timeZoneItem.Click += timeZoneItemClick;
				toolStripSplitButtonTimeZone.DropDownItems.Add(timeZoneItem);			
			}

			displayTimeZoneInfo();
		}

		private void timeZoneItemClick(object sender, EventArgs e)
		{
			var item = (ToolStripMenuItem)sender;
			var timeZone = (TimeZoneInfo)item.Tag;
			changeTimeZone(timeZone);
		}

		private void toolStripMenuItemExportToPdfGraphicalMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			var exporter = new ExportToPdfGraphical(_scheduleView, this, _schedulerState,
				TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
				TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.TextInfo.IsRightToLeft);
			exporter.Export();
		}

		private void exportToPdf(bool shiftsPerDay)
		{
			var exporter = new ExportToPdf(_scheduleView, this, _schedulerState,
				TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
				TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.TextInfo.IsRightToLeft);
			exporter.Export(shiftsPerDay);
		}

		private void toolStripMenuItemExportToPdfMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			exportToPdf(false);
		}

		private void toolStripButtonFilterAgentsClick(object sender, EventArgs e)
		{
			showFilterDialog();
			reloadFilteredPeople();
		}

		private void refreshEntitiesUsingMessageBroker()
		{
			var conflictsBuffer = new List<PersistConflict>();
			var refreshedEntitiesBuffer = new List<IPersistableScheduleData>();
			refreshEntitiesUsingMessageBroker(refreshedEntitiesBuffer, conflictsBuffer);
			handleConflicts(refreshedEntitiesBuffer, conflictsBuffer);
		}

		private void handleConflicts(IEnumerable<IPersistableScheduleData> refreshedEntities,
			IEnumerable<PersistConflict> conflicts)
		{
			var modifiedDataFromConflictResolution = new List<IPersistableScheduleData>(refreshedEntities);

			if (conflicts.Any())
				showPersistConflictView(modifiedDataFromConflictResolution, conflicts);

			_undoRedo.Clear(); //see if this can be removed later... Should undo/redo work after refresh?
			foreach (var data in modifiedDataFromConflictResolution)
			{
				_schedulerState.MarkDateToBeRecalculated(new DateOnly(data.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo)));
				_personsToValidate.Add(data.Person);
			}
		}

		private void showPersistConflictView(List<IPersistableScheduleData> modifiedData,
			IEnumerable<PersistConflict> conflicts)
		{
			using (
				var conflictForm = new PersistConflictView(_schedulerState.Schedules, conflicts, modifiedData,
					_schedulerMessageBrokerHandler))
			{
				conflictForm.ShowDialog();
			}
		}

		private void refreshEntitiesUsingMessageBroker(ICollection<IPersistableScheduleData> refreshedEntitiesBuffer,
			ICollection<PersistConflict> conflictsBuffer)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_schedulerMessageBrokerHandler.Refresh(refreshedEntitiesBuffer, conflictsBuffer, _loadRequsts);
			}
		}

		public void SizeOfMessageBrokerQueue(int count)
		{
			toolStripButtonRefreshLarge.Enabled = count != 0;
		}

		private void refreshData()
		{
			try
			{
				refreshEntitiesUsingMessageBroker();
				_schedulerState.Schedules.ForEach(p => p.Value.ForceRecalculationOfTargetTimeContractTimeAndDaysOff());
				RecalculateResources();
				updateShiftEditor();
				var selectedSchedules = _scheduleView.SelectedSchedules();
				updateSelectionInfo(selectedSchedules);
			}
			catch (CouldNotCreateTransactionException dataSourceException)
			{
				//rk - dont like this but cannot easily find "the spot" to catch these exception in current design
				using (
					var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC,
						Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
			}
		}

		private void SchedulingScreen_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
				wpfShiftEditor1.EnableMoveAllLayers(true);
		}

		private void SchedulingScreen_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
				wpfShiftEditor1.EnableMoveAllLayers(false);
		}

		private SplitterManagerRestrictionView SplitterManager
		{
			get
			{
				if (_splitterManager == null)
				{
					_splitterManager = new SplitterManagerRestrictionView
					{
						MainSplitter = _splitContainerAdvMain,
						LeftMainSplitter = schedulerSplitters1.SplitContainerAdvMain,
						GraphResultSplitter = _splitContainerAdvResultGraph,
						GridEditorSplitter = _splitContainerLessIntellegentEditor,
						RestrictionViewSplitter = _splitContainerLessIntellegentRestriction
					};
				}
				return _splitterManager;
			}
		}

		private void toolStripButtonShowGraph_Click(object sender, EventArgs e)
		{
			toolStripButtonShowGraph.Checked = !toolStripButtonShowGraph.Checked;
			SplitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
			_showGraph = toolStripButtonShowGraph.Checked;
		}

		private void toolStripButtonShowResult_Click(object sender, EventArgs e)
		{
			toolStripButtonShowResult.Checked = !toolStripButtonShowResult.Checked;
			SplitterManager.ShowResult = toolStripButtonShowResult.Checked;
			_showResult = toolStripButtonShowResult.Checked;
		}

		private void toolStripButtonShowEditor_Click(object sender, EventArgs e)
		{
			toolStripButtonShowEditor.Checked = !toolStripButtonShowEditor.Checked;
			SplitterManager.ShowEditor = toolStripButtonShowEditor.Checked;
			_showEditor = toolStripButtonShowEditor.Checked;
			if (_showEditor)
				updateShiftEditor();
		}

		private DateTime _lastclickLabels;

		private void toolStripButtonShowTexts_Click(object sender, EventArgs e)
		{
			// fix for bug in syncfusion that shoots click event twice on buttons in quick access
			if (_lastclickLabels.AddSeconds(1) > DateTime.Now) return;
			_lastclickLabels = DateTime.Now;

			toolStripButtonShowTexts.Checked = !toolStripButtonShowTexts.Checked;
			_showRibbonTexts = toolStripButtonShowTexts.Checked;
			setShowRibbonTexts();
		}

		private void restrictionViewMode(bool show)
		{
			if (show)
			{
				toolStripButtonShowResult.Checked = false;
				SplitterManager.ShowResult = toolStripButtonShowResult.Checked;
				toolStripButtonShowGraph.Checked = false;
				SplitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
				_splitterManager.ShowRestrictionView = true;
				toolStripButtonShowEditor.Checked = true;
				SplitterManager.ShowEditor = true;
				toolStripMenuItemLock.Enabled = false;
				toolStripMenuItemUnlock.Enabled = false;
				toolStripSplitButtonLock.Enabled = false;
				toolStripSplitButtonUnlock.Enabled = false;
				toolStripMenuItemSortBy.Enabled = false;
			}
			else
			{
				toolStripButtonShowResult.Checked = _showResult;
				SplitterManager.ShowResult = toolStripButtonShowResult.Checked;
				toolStripButtonShowGraph.Checked = _showGraph;
				SplitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
				SplitterManager.ShowEditor = _showEditor;
				toolStripButtonShowEditor.Checked = _showEditor;
				_splitterManager.ShowRestrictionView = false;
				toolStripMenuItemLock.Enabled = true;
				toolStripMenuItemUnlock.Enabled = true;
				toolStripSplitButtonLock.Enabled = true;
				toolStripSplitButtonUnlock.Enabled = true;
				toolStripMenuItemSortBy.Enabled = true;
				if (_teamLeaderMode)
				{
					SplitterManager.ShowGraph = false;
					SplitterManager.ShowResult = false;
				}
			}


		}

		private void toolStripButtonShrinkage_Click(object sender, EventArgs e)
		{
			disableAllExceptCancelInRibbon();
			bool useShrinkage = !toolStripButtonShrinkage.Checked;
			toggleShrinkage(useShrinkage);
			enableAllExceptCancelInRibbon();
		}

		private void toolStripButtonValidation_Click(object sender, EventArgs e)
		{
			if (_validation)
			{
				_schedulerState.SchedulingResultState.UseValidation = false;
				validateAllPersons();
			}
			_validation = !toolStripButtonValidation.Checked;
			toolStripButtonValidation.Checked = _validation;
			_schedulerState.SchedulingResultState.UseValidation = _validation;
			applyValidation();
		}

		private void applyValidation()
		{
			if (_scheduleView != null)
			{
				if (_validation)
				{
					validateAllPersons();
				}
				else
				{
					_scheduleView.TheGrid.Invalidate();
				}
			}
		}

		private void toolStripButtonCalculation_Click(object sender, EventArgs e)
		{
			toggleCalculation();
		}

		private void toggleShrinkage(bool useShrinkage)
		{
			Cursor = Cursors.WaitCursor;
			IList<ISkillStaffPeriod> skillStaffPeriods =
				_schedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					_schedulerState.SchedulingResultState.Skills, _schedulerState.LoadedPeriod.Value);
			foreach (ISkillStaffPeriod period in skillStaffPeriods)
			{
				period.Payload.UseShrinkage = useShrinkage;
				_schedulerState.MarkDateToBeRecalculated(new DateOnly(period.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo)));
			}

			RecalculateResources();
			((ToolStripMenuItem) _contextMenuSkillGrid.Items["UseShrinkage"]).Checked = useShrinkage;
			toolStripButtonShrinkage.Checked = useShrinkage;
			Cursor = Cursors.Default;

			refreshSummarySkillIfActive();
		}

		private void ToolStripMenuItemScheduledTimePerActivityMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor.Current = Cursors.WaitCursor;
			if (_scheduleView.SelectedSchedules().Count > 0)
			{
				var reportDetail =
					ReportHandler.CreateReportDetail(
						ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
							DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport));
				ReportHandler.ShowReport(reportDetail, _scheduleView, SchedulerState.RequestedScenario, CultureInfo.CurrentCulture);
			}
			Cursor.Current = Cursors.Default;
		}

		private void ToolStripMenuItemSearch_Click(object sender, EventArgs e)
		{
			displaySearch();
		}

		private void displaySearch()
		{
			IList<IPerson> persons = new List<IPerson>(SchedulerState.FilteredCombinedAgentsDictionary.Values);

			using (var searchForm = new SearchPerson(persons))
			{
				if (searchForm.ShowDialog(this) != DialogResult.OK) return;
				if (searchForm.SelectedPerson == null) return;

				int row = _scheduleView.GetRowForAgent(searchForm.SelectedPerson);
				GridRangeInfo info = GridRangeInfo.Cells(row, 0, row, 0);
				_scheduleView.TheGrid.Selections.Clear(true);
				_scheduleView.TheGrid.CurrentCell.Activate(row, 0, GridSetCurrentCellOptions.SetFocus);
				_scheduleView.TheGrid.Selections.ChangeSelection(info, info, true);
				_scheduleView.TheGrid.CurrentCell.MoveTo(row, 0, GridSetCurrentCellOptions.ScrollInView);
			}
		}

		private void ToolStripMenuItemViewDetails_Click(object sender, EventArgs e)
		{
			showRequestDetailsView(null);
		}

		private void showRequestDetailsView(EventParameters<ShowRequestDetailsView> eventParameters)
		{
			if (!_permissionHelper.IsViewRequestDetailsAvailable(_requestView)) return;
			var requestDetailsView = new RequestDetailsView(_eventAggregator, _requestView.SelectedAdapters().First(),
				_schedulerState.Schedules);
			requestDetailsView.Show(this);
		}

		private void ToolStripMenuItemStartAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByStartAscendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemStartTimeDescMouseUp(object sender, MouseEventArgs e)
		{

			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByStartDescendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemEndTimeAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByEndAscendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemEndTimeDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByEndDescendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemUnlockSelectionRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			GridHelper.GridUnlockSelection(_grid, LockManager);
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemUnlockAllRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			LockManager.Clear();
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemLockSelectionRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			GridHelper.GridlockSelection(_grid, LockManager);
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemWriteProtectScheduleMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			writeProtectSchedule();
		}

		private void toolStripMenuItemCreateMeetingMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
			_schedulerMeetingHelper.MeetingComposerStart(null, _scheduleView, true, viewSchedulesPermission,
				_container.Resolve<IToggleManager>());

		}

		private void toolStripMenuItemEditMeetingMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			editMeeting();
		}

		private void toolStripMenuItemRemoveParticipantMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (_scheduleView != null)
				_schedulerMeetingHelper.MeetingRemoveAttendees(GridHelper.MeetingsFromSelection(_scheduleView.ViewGrid));
		}

		private void toolStripMenuItemDeleteMeetingMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			deleteMeeting();
		}

		private void toolStripSplitButtonUnlockButtonClick(object sender, EventArgs e)
		{
			LockManager.Clear();
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemSwapRawClick(object sender, EventArgs e)
		{
			if (_scheduleView != null)
				swapRaw();
		}

		private void toolStripMenuItemFindMatchingClick(object sender, EventArgs e)
		{
			IScheduleDay selected;
			if (tryGetFirstSelectedSchedule(_scheduleView.SelectedSchedules(), out selected))
			{
				findMatching(selected);
			}
		}

		private bool tryGetFirstSelectedSchedule(IEnumerable<IScheduleDay> selectedSchedules, out IScheduleDay scheduleDay)
		{
			scheduleDay = null;
			if (!selectedSchedules.Any()) 
				return false;

			scheduleDay = selectedSchedules.First();
			return true;
		}

		private void toolStripMenuItemFindMatching2Click(object sender, EventArgs e)
		{
			var selectedAdapters = _requestView.SelectedAdapters();
			if (selectedAdapters.Count != 1) return;

			var selectedRequest = selectedAdapters.First();
			if (!selectedRequest.IsEditable) return;
			if (!selectedRequest.IsPending) return;
			if (!selectedRequest.IsWithinSchedulePeriod) return;

			var request = selectedRequest.PersonRequest.Request as IAbsenceRequest;
			if (request == null) return;
			DateTimePeriod period = request.Period;
			IPerson person = request.Person;
			DateOnlyPeriod dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

			if (dateOnlyPeriod.DayCount() > 1) return;
			DateOnly dateOnly = dateOnlyPeriod.StartDate;
			IScheduleDay selected = _schedulerState.Schedules[person].ScheduledDay(dateOnly);
			findMatching(selected);
		}

		private void findMatching(IScheduleDay selected)
		{
			using (
				var form = new FindMatchingNew(_restrictionExtractor, selected.Person, selected.DateOnlyAsPeriod.DateOnly,
					new ScheduleDayForPerson(() => new ScheduleRangeForPerson(() => _schedulerState.SchedulingResultState)),
					_schedulerState.FilteredCombinedAgentsDictionary.Values)
				)
			{
				form.ShowDialog(this);
				if (form.DialogResult == DialogResult.OK)
				{
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> {selected});
					_scheduleView.GridClipboardCopy(false);
					if (form.Selected() == null)
						return;
					IScheduleDay target = _schedulerState.Schedules[form.Selected()].ScheduledDay(selected.DateOnlyAsPeriod.DateOnly);
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> {target});
					_cutPasteHandlerFactory.For(_controlType).Paste();
					updateShiftEditor();
				}
			}
		}

		private void toolStripMenuItemViewHistoryClick(object sender, EventArgs e)
		{
			if (!_scenario.DefaultScenario || !_isAuditingSchedules) return;
			IScheduleDay selected;
			if (!tryGetFirstSelectedSchedule(_scheduleView.SelectedSchedules(), out selected)) return;

			bool isLocked = _gridLockManager.HasLocks && _gridLockManager.Gridlocks(selected) != null;

			using (var auditHistoryView = new AuditHistoryView(selected, this))
			{
				auditHistoryView.ShowDialog(this);
				if (auditHistoryView.DialogResult != DialogResult.OK || auditHistoryView.SelectedScheduleDay == null ||
					isLocked) return;

				var historyDay = auditHistoryView.SelectedScheduleDay;

				var scheduleRange = SchedulerState.Schedules[historyDay.Person];
				var currentDay = scheduleRange.ScheduledDay(historyDay.DateOnlyAsPeriod.DateOnly);
				//schedule day can apperently have person absences from "other" day due to nightshifts and consecutive absence so we neet to add those to history day
				foreach (
					var data in
						currentDay.PersistableScheduleDataCollection()
							.OfType<PersonAbsence>()
							.Where(data => !data.Period.Intersect(historyDay.Period)))
				{
					historyDay.Add(data);
				}

				var schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(SchedulerState.SchedulingResultState,
				_container.Resolve<IScheduleDayChangeCallback>(),
						new ScheduleTagSetter(_defaultScheduleTag));
				schedulePartModifyAndRollbackService.Modify(historyDay);
				updateShiftEditor();
			}
		}

		private void toolStripItemViewAllowanceClick(object sender, EventArgs e)
		{
			_requestView.ShowRequestAllowanceView(this);
		}

		private void toolStripViewRequestHistoryClick(object sender, EventArgs e)
		{
			var id = Guid.Empty;
			var defaultRequest = _requestView.SelectedAdapters().Count > 0
				? _requestView.SelectedAdapters().First().PersonRequest
				: _schedulerState.PersonRequests.FirstOrDefault(r => r.Request is AbsenceRequest);
			if (defaultRequest != null)
				id = defaultRequest.Person.Id.GetValueOrDefault();
			var presenter = _container.BeginLifetimeScope().Resolve<IRequestHistoryViewPresenter>();
			presenter.ShowHistory(id, _schedulerState.FilteredCombinedAgentsDictionary.Values);
		}

		private void toolStripExTagsSizeChanged(object sender, EventArgs e)
		{
			toolStripSplitButtonChangeTag.Width = toolStripComboBoxAutoTag.Width;
		}

		private void toolStripMenuItemContractTimeAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeAscendingCommand(SchedulerState));
		}

		private void toolStripMenuItemContractTimeDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeDescendingCommand(SchedulerState));
		}

		private void toolStripMenuItemSeniorityRankDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				_scheduleView.Sort(new SortBySeniorityRankingDescendingCommand(SchedulerState,
					_container.Resolve<IRankedPersonBasedOnStartDate>()));
		}

		private void toolStripMenuItemSeniorityRankAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				_scheduleView.Sort(new SortBySeniorityRankingAscendingCommand(SchedulerState,
					_container.Resolve<IRankedPersonBasedOnStartDate>()));
		}

		private void toolStripMenuItemExportToPdfShiftsPerDayMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			exportToPdf(true);
		}

		private void toolStripMenuItemRestrictionCopyClick(object sender, EventArgs e)
		{
			toolStripMenuItemCopyClick(sender, e);
		}

		private void toolStripMenuItemRestrictionPasteClick(object sender, EventArgs e)
		{
			((AgentRestrictionsDetailView) _scheduleView).PasteSelectedRestrictions(_undoRedo);
		}

		private void toolStripMenuItemRestrictionDeleteClick(object sender, EventArgs e)
		{
			((AgentRestrictionsDetailView) _scheduleView).DeleteSelectedRestrictions(_undoRedo, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
		}

		private void editControlRestrictionsNewClicked(object sender, EventArgs e)
		{
			addPreferenceToolStripMenuItemClick(sender, e);
		}

		private void editControlRestrictionsNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			_editControlRestrictions.CloseDropDown();
			if ((ClipboardItems) e.ClickedItem.Tag == ClipboardItems.StudentAvailability)
				addStudentAvailabilityToolStripMenuItemClick(sender, e);
			if ((ClipboardItems) e.ClickedItem.Tag == ClipboardItems.Preference)
				addPreferenceToolStripMenuItemClick(sender, e);
		}

		private void addPreferenceToolStripMenuItemClick(object sender, EventArgs e)
		{
			IScheduleDay selectedDay;
			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (!tryGetFirstSelectedSchedule(selectedSchedules, out selectedDay)) return;

			using (var view = new AgentPreferenceView(selectedDay, _schedulerState, _container.Resolve<IScheduleDayChangeCallback>()))
			{
				view.ShowDialog(this);
				updateRestrictions(selectedSchedules.First());
			}
		}

		private void addStudentAvailabilityToolStripMenuItemClick(object sender, EventArgs e)
		{
			IScheduleDay selectedDay;
			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (!tryGetFirstSelectedSchedule(selectedSchedules, out selectedDay)) return;

			using (var view = new AgentStudentAvailabilityView(selectedDay, _schedulerState.SchedulingResultState, _container.Resolve<IScheduleDayChangeCallback>()))
			{
				view.ShowDialog(this);
				updateRestrictions(selectedSchedules.First());
			}
		}

		private void updateRestrictions(IScheduleDay scheduleDay)
		{
			if (_scheduleView == null || scheduleDay == null) return;
			if (_scheduleView is AgentRestrictionsDetailView)
			{
				schedulerSplitters1.RecalculateRestrictions();
				schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions);
			}
			updateSelectionInfo(new List<IScheduleDay> {scheduleDay});
			enableSave();
		}

		private void addOvertimeAvailabilityToolStripMenuItemClick(object sender, EventArgs e)
		{
			IScheduleDay selectedDay;
			if (!tryGetFirstSelectedSchedule(_scheduleView.SelectedSchedules(), out selectedDay)) return;

			using (var view = new AgentOvertimeAvailabilityView(selectedDay, _schedulerState.SchedulingResultState, _container.Resolve<IScheduleDayChangeCallback>()))
			{
				view.ShowDialog(this);
				updateOvertimeAvailability();
			}
		}

		private void updateOvertimeAvailability()
		{
			if (_scheduleView == null) return;
			enableSave();
		}

		private void toolStripButtonFilterOvertimeAvailabilityClick(object sender, EventArgs e)
		{
			if (toolStripButtonFilterOvertimeAvailability.Checked)
			{
				toolStripButtonFilterOvertimeAvailability.Checked = false;

				_schedulerState.ResetFilteredPersonsOvertimeAvailability();
				reloadFilteredPeople();
				return;
			}
			var defaultDate = _scheduleView.SelectedDateLocal();

			using (var view = new FilterOvertimeAvailabilityView(defaultDate, _schedulerState))
			{
				if (view.ShowDialog() == DialogResult.OK)
				{
					toolStripButtonFilterOvertimeAvailability.Checked = true;
					reloadFilteredPeople();
				}
			}
		}

		private void toolStripButtonFilterStudentAvailabilityClick(object sender, EventArgs e)
		{
			if (toolStripButtonFilterStudentAvailability.Checked)
			{
				toolStripButtonFilterStudentAvailability.Checked = false;
				_schedulerState.ResetFilteredPersonsHourlyAvailability();
				reloadFilteredPeople();
				return;
			}

			var defaultDate = _scheduleView.SelectedDateLocal();
			using (var view = new FilterHourlyAvailabilityView(defaultDate, _schedulerState))
			{
				if (view.ShowDialog() == DialogResult.OK)
				{
					toolStripButtonFilterStudentAvailability.Checked = true;
					reloadFilteredPeople();
				}
			}
		}

		private void reloadFilteredPeople()
		{
			toolStripButtonFilterAgents.Checked = SchedulerState.AgentFilter();

			if (_scheduleView != null)
			{
				if (_scheduleView.Presenter.SortCommand == null || _scheduleView.Presenter.SortCommand is NoSortCommand)
					_scheduleView.Presenter.ApplyGridSort();
				else
					_scheduleView.Sort(_scheduleView.Presenter.SortCommand);

				_grid.Refresh();
				GridHelper.GridlockWriteProtected(_schedulerState, LockManager);
				_scheduleView.ResizeGridColumnsToFit();
				_grid.Refresh();
			}
			_requestView?.FilterPersons(_schedulerState.FilteredCombinedAgentsDictionary.Keys);
			drawSkillGrid();

			_shiftCategoryDistributionModel.SetFilteredPersons(_schedulerState.FilteredCombinedAgentsDictionary.Values);
			schedulerSplitters1.RefreshTabInfoPanels(_schedulerState.FilteredCombinedAgentsDictionary.Values);
			updateShiftEditor();
			toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXAgentsColon") + " " +
													  _schedulerState.FilteredCombinedAgentsDictionary.Count;
		}

		private void toolStripMenuItemSwitchViewPointToTimeZoneOfSelectedAgentClick(object sender, EventArgs e)
		{
			IScheduleDay scheduleDay;
			if (tryGetFirstSelectedSchedule(_scheduleView.SelectedSchedules(), out scheduleDay))
			{
				var timeZone = scheduleDay.Person.PermissionInformation.DefaultTimeZone();

				changeTimeZone(timeZone);
			}
		}

		private void changeTimeZone(TimeZoneInfo timeZone)
		{
			TimeZoneGuard.Instance.TimeZone = timeZone;
			_schedulerState.TimeZoneInfo = TimeZoneGuard.Instance.TimeZone;
			wpfShiftEditor1.SetTimeZone(TimeZoneGuard.Instance.TimeZone);
			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (_scheduleView != null && _scheduleView.HelpId == "AgentRestrictionsDetailView")
			{
				prepareAgentRestrictionView(null, _scheduleView, new List<IPerson>(_scheduleView.AllSelectedPersons(selectedSchedules)));
			}
			displayTimeZoneInfo();
			_scheduleView.SetSelectedDateLocal(_dateNavigateControl.SelectedDate);
			_grid.Invalidate();
			_grid.Refresh();
			updateSelectionInfo(selectedSchedules);
			updateShiftEditor();
			drawSkillGrid();
			reloadChart();
		}

		private void displayTimeZoneInfo()
		{
			bool show = _detectedTimeZoneInfos.Count > 1;
			if(!show)
				toolStripStatusLabelScheduleTag.BorderSides = ToolStripStatusLabelBorderSides.Right;
			toolStripMenuItemSwitchToViewPointOfSelectedAgent.Visible = show;
			toolStripSplitButtonTimeZone.Visible = show;
			toolStripSplitButtonTimeZone.Text = string.Concat(LanguageResourceHelper.Translate("XXViewPointTimeZone"),
				Resources.Colon,
				TimeZoneGuard.Instance.TimeZone.DisplayName);
		}

		private void toolStripMenuItemScheduleOvertimeClick(object sender, EventArgs e)
		{
			if (_backgroundWorkerOvertimeScheduling.IsBusy) return;
			if (_scheduleView == null) return;
			if (!_scheduleView.HasSelectedSchedules()) return;

			IList<IRuleSetBag> ruleSetBags;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new WorkShiftRuleSetRepository(new ThisUnitOfWork(uow)).FindAllWithLimitersAndExtenders();
				var bags = new RuleSetBagRepository(new ThisUnitOfWork(uow)).LoadAllWithRuleSets();
				ruleSetBags = bags.OrderBy(r => r.Description.Name).ToList();
			}

				try
				{
					var definitionSets =
						MultiplicatorDefinitionSet.Where(set => set.MultiplicatorType == MultiplicatorType.Overtime).ToList();
					var resolution = 15;
					IScheduleDay scheduleDay;
					var selectedSchedules = _scheduleView.SelectedSchedules();
					if (tryGetFirstSelectedSchedule(selectedSchedules, out scheduleDay))
					{
						var person = scheduleDay.Person;
						if (scheduleDay.DateOnlyAsPeriod.DateOnly < person.TerminalDate)
						{
							var skills = aggregateSkills(person, scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
							if (skills.Count > 0)
							{
								var skillResolutionProvider = _container.Resolve<ISkillResolutionProvider>();
								resolution = skillResolutionProvider.MinimumResolution(skills);
							}
						}
					}

					var showUseSkills = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_CascadingSkillsGUI_40018)
										&& _schedulerState.SchedulingResultState.Skills.Any(x => x.IsCascading());
					
					using (var options = new OvertimePreferencesDialog(_schedulerState.CommonStateHolder.ActiveScheduleTags,
																	"OvertimePreferences", 
																	_schedulerState.CommonStateHolder.ActiveActivities, 
																	resolution, 
																	definitionSets,
																	ruleSetBags,
																	showUseSkills))
					{
						if (options.ShowDialog(this) != DialogResult.OK) return;
						options.Refresh();
						startBackgroundScheduleWork(_backgroundWorkerOvertimeScheduling,
							new SchedulingAndOptimizeArgument(selectedSchedules) { OvertimePreferences = options.Preferences },
							true);
					}
				}
				catch (CouldNotCreateTransactionException dataSourceException)
				{
					using (
						var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC,
							Resources.ServerUnavailable))
					{
						view.ShowDialog();
					}
				}
		}

		private static IEnumerable<ISkill> aggregateSkills(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod.PersonSkillCollection.Where(s => s.Active).Select(s => s.Skill).Distinct();
		}

		private void toolStripMenuItemContractTimeClick(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem) sender;
			_scheduleTimeType = (ScheduleTimeType) item.Tag;
			updateSelectionInfo(_scheduleView.SelectedSchedules());
		}

		private void toolStripButtonShowPropertyPanelClick(object sender, EventArgs e)
		{
			toolStripButtonShowPropertyPanel.Checked = !toolStripButtonShowPropertyPanel.Checked;
			schedulerSplitters1.ToggelPropertyPanel(!toolStripButtonShowPropertyPanel.Checked);
			_showInfoPanel = toolStripButtonShowPropertyPanel.Checked;
			updateSelectionInfo(_scheduleView.SelectedSchedules());
		}

		private void toolStripButtonRequestBackClick(object sender, EventArgs e)
		{
			toolStripTabItemHome.Checked = true;
			zoom(_previousZoomLevel);
		}

		private void backStage1VisibleChanged(object sender, EventArgs e)
		{
			if (!backStage1.Visible && RightToLeftLayout) _tmpTimer.Enabled = true;
		}

		private void toolStripMenuItemShiftBackToLegalClick(object sender, EventArgs e)
		{
			scheduleSelected(true);
		}

		private void publishToolStripMenuItemClick(object sender, EventArgs e)
		{
			using (var view = new PublishScheduleDateView(_scheduleView.SelectedSchedules()))
			{
				if (view.ShowDialog(this) != DialogResult.OK) return;
				var workflowControlSets = view.WorkflowControlSets;
				var publishToDate = view.PublishScheduleTo;
				var publishCommand = new PublishScheduleCommand(workflowControlSets, publishToDate,
					_schedulerState.CommonStateHolder);
				publishCommand.Execute();
				enableSave();
			}
		}

		private void toolStripButtonSaveLargeClick(object sender, EventArgs e)
		{
			save();
		}

		private void toolStripButtonRefreshLargeClick(object sender, EventArgs e)
		{
			refreshData();
		}
	}
}

//Cake-in-the-kitchen if* this reaches 5000! 
//bigmac-in-the-kitchen if* this reaces 6000!
//new-mailfooter-in-the-kitchen if* this reaces 7000!
//naken-kebab-in-the-kitchen if* this reaces 8000!
//*when
