﻿#region wohoo!! 95 usings in one form

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using log4net;
using MbCache.Core;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.AgentInfo;
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
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.Security.Authentication;
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
using Teleopti.Ccc.SmartClientPortal.Shell.Common;
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
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{

	public partial class SchedulingScreen : BaseRibbonForm
	{
		private readonly HashSet<TimeZoneInfo> _detectedTimeZoneInfos = new HashSet<TimeZoneInfo>();
		private readonly ILifetimeScope _container;
		private static readonly ILog log = LogManager.GetLogger(typeof(SchedulingScreen));
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
		private readonly IVirtualSkillHelper _virtualSkillHelper;
		private SchedulerMeetingHelper _schedulerMeetingHelper;
		private readonly IList<IEntity> _temporarySelectedEntitiesFromTreeView;
		private GridChartManager _gridChartManager;
		private string _chartDescription;
		private GridRowInChartSettingButtons _gridrowInChartSettingButtons;
		private GridRow _currentSelectedGridRow;
		private readonly TabControlAdv _tabSkillData;
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
		private readonly ContextMenuStrip _contextMenuSkillGrid = new ContextMenuStrip();
		private readonly SchedulingOptions _schedulingOptions;
		private readonly IOptimizationPreferences _optimizationPreferences;
		private readonly IResourceOptimizationHelperExtended _optimizationHelperExtended;
		private readonly ICollection<IPerson> _personsToValidate = new HashSet<IPerson>();
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
		private readonly System.Windows.Forms.Timer _tmpTimer = new System.Windows.Forms.Timer();
		private readonly SchedulerGroupPagesProvider _groupPagesProvider;
		public IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSet { get; private set; }
		private SkillResultViewSetting _skillResultViewSetting;
		private DateTimePeriod _selectedPeriod;
		private ScheduleTimeType _scheduleTimeType;
		private DateTime _lastSaved = DateTime.Now;
		private SchedulingScreenPermissionHelper _permissionHelper;
		private readonly CutPasteHandlerFactory _cutPasteHandlerFactory;
		private Form _mainWindow;
		private bool _cancelButtonPressed;
		private IDaysOffPreferences _daysOffPreferences;
		private IEnumerable<IOptionalColumn> _optionalColumns;
		private ReplaceActivityParameters _replaceActivityParameters;

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
			foreach (ToolStripTabItem ribbonTabItem in ribbonControlAdv1.Header.MainItems)
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
			schedulerSplitters1.Grid.Focus();
		}

		private void dateNavigateControlSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
		{
			_scheduleView.SetSelectedDateLocal(e.Value);
			schedulerSplitters1.Grid.Invalidate();

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
			//SHOULD NOT BE HERE. If we need special registrations for scheduler, use _configuration.Args().IsFatClient instead!
			var updater = new ContainerBuilder();

			updater.RegisterType<SchedulingScreenPersister>().As<ISchedulingScreenPersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleRangePersister>().As<IScheduleRangePersister>().InstancePerLifetimeScope();
			updater.RegisterType<ScheduleRangeConflictCollector>()
				.As<IScheduleRangeConflictCollector>()
				.InstancePerLifetimeScope();
			updater.Register(c => _schedulerMessageBrokerHandler)
				.As<IInitiatorIdentifier>()
				.As<IReassociateDataForSchedules>()
				.ExternallyOwned();

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

#pragma warning disable 618
			updater.Update(_container.ComponentRegistry);
#pragma warning restore 618
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

			_skillDayGridControl = new SkillDayGridControl(_container.Resolve<ISkillPriorityProvider>())
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
			};
			_skillWeekGridControl = new SkillWeekGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
			};
			_skillMonthGridControl = new SkillMonthGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
			};
			_skillFullPeriodGridControl = new SkillFullPeriodGridControl
			{
				ContextMenu = contextMenuStripResultView.ContextMenu,
			};
			_skillResultHighlightGridControl = new SkillResultHighlightGridControl();

			setUpZomMenu();

			_skillIntradayGridControl = new SkillIntradayGridControl("SchedulerSkillIntradayGridAndChart", _container.Resolve<ISkillPriorityProvider>())
			{
				ContextMenu = contextMenuStripResultView.ContextMenu
			};
			_schedulingOptions = new SchedulingOptions();
			_optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
			_daysOffPreferences = _container.Resolve<IDaysOffPreferences>();
			_overriddenBusinessRulesHolder = _container.Resolve<IOverriddenBusinessRulesHolder>();
			_workShiftWorkTime = _container.Resolve<IWorkShiftWorkTime>();
			_temporarySelectedEntitiesFromTreeView = allSelectedEntities;
			_virtualSkillHelper = _container.Resolve<IVirtualSkillHelper>();
			SchedulerState = new SchedulingScreenState(_container.Resolve<IDisableDeletedFilter>(), _container.Resolve<ISchedulerStateHolder>());
			_groupPagesProvider = _container.Resolve<SchedulerGroupPagesProvider>();
			_optimizationHelperExtended = _container.Resolve<IResourceOptimizationHelperExtended>();
			SchedulerState.SchedulerStateHolder.SetRequestedScenario(loadScenario);
			SchedulerState.SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(loadingPeriod,
				TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

			_schedulerMeetingHelper = new SchedulerMeetingHelper(_schedulerMessageBrokerHandler,
																SchedulerState.SchedulerStateHolder,
																_container.Resolve<IResourceCalculation>(),
																_container.Resolve<ISkillPriorityProvider>(),
																_container.Resolve<IScheduleStorageFactory>(),
																_container.Resolve<CascadingResourceCalculationContextFactory>(),
																_container.Resolve<IStaffingCalculatorServiceFacade>());
			//Using the same module id when saving meeting changes to avoid getting them via MB as well

			ClipsHandlerSchedule = new ClipHandler<IScheduleDay>();

			_scenario = loadScenario;
			_shrinkage = shrinkage;
			SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation = !calculation;
			_validation = validation;
			SchedulerState.SchedulerStateHolder.SchedulingResultState.UseValidation = validation;
			_teamLeaderMode = teamLeaderMode;
			_loadRequsts = loadRequsts;
			SchedulerState.SchedulerStateHolder.SchedulingResultState.TeamLeaderMode = teamLeaderMode;

			toolStripProgressBar1.Visible = true;
			toolStripProgressBar1.Maximum = loadingPeriod.DayCount() + 5;
			toolStripProgressBar1.Step = 1;


			GridHelper.GridStyle(schedulerSplitters1.Grid);

			setColor();
			setUpRibbon();
			ribbonTemplatePanelsClose();

			LockManager = _container.Resolve<IGridlockManager>();

			setEventHandlers();
			instantiateClipboardControl();
			instantiateEditControl();
			instantiateEditControlRestrictions();
			instantiateClipboardControlRestrictions();

			AddControlHelpContext(schedulerSplitters1.Grid);
			AddControlHelpContext(schedulerSplitters1.ChartControlSkillData);
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
			toolStripButtonCalculation.Checked = !SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			toolStripButtonValidation.Checked = _validation;
		}

		private void setHeaderText(DateOnly start, DateOnly end, DateOnly? outerStart, DateOnly? outerEnd)
		{
			var currentCultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var startDate = start.Date.ToString("d", currentCultureInfo);
			var endDate = end.Date.ToString("d", currentCultureInfo);

			if (outerStart.HasValue && outerEnd.HasValue)
			{
				if (start.AddDays(-7) != outerStart || end.AddDays(7) != outerEnd)
				{
					startDate = start.Date.ToString("d", currentCultureInfo) + "-" + end.Date.ToString("d", currentCultureInfo);
					endDate = "(" + outerStart.Value.Date.ToString("d", currentCultureInfo) + "-" +
							  outerEnd.Value.Date.ToString("d", currentCultureInfo) + ")";
				}
			}

			Text = string.Format(currentCultureInfo, Resources.TeleoptiCCCColonModuleColonFromToDateScenarioColon, Resources.Schedules, startDate, endDate, _scenario.Description.Name);
		}

		private void schedulerMeetingHelperModificationOccured(object sender, ModifyMeetingEventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{

				uow.Reassociate(SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents);
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
			var changeProvider = (IProvideCustomChangeInfo)e.ModifiedMeeting;
			var tracker = new MeetingChangeTracker();
			tracker.TakeSnapshot((IMeeting)changeProvider.BeforeChanges());
			var changes =
				tracker.CustomChanges(e.ModifiedMeeting, e.Delete ? DomainUpdateType.Delete : DomainUpdateType.Update)
					.Select(r => (MeetingChangedEntity)r.Root);
			var meetingChangedEntities = changes as MeetingChangedEntity[] ?? changes.ToArray();
			meetingChangedEntities.ForEach(c =>
			{
				var period = c.Period.ToDateOnlyPeriod(SchedulerState.SchedulerStateHolder.TimeZoneInfo);
				period = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
				period.DayCollection().ForEach(SchedulerState.SchedulerStateHolder.MarkDateToBeRecalculated);
			});
			SchedulerState.SchedulerStateHolder.Schedules.Where(s => meetingChangedEntities.Any(c => s.Key.Equals(c.MainRoot)))
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
			switch ((ClipboardItems)e.ClickedItem.Tag)
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
			if ((ClipboardItems)e.ClickedItem.Tag == ClipboardItems.DayOff)
				addDayOff();
			else
				addNewLayer((ClipboardItems)e.ClickedItem.Tag);
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
						if (!SchedulerState.SchedulerStateHolder.CommonStateHolder.Absences.NonDeleted().Any())
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
				replaceActivityToolStripMenuItem.Visible = TestMode.Micke;
				Refresh();
				drawSkillGrid();
				if (TestMode.Micke)
				{
					bool found = false;
					foreach (var item in _contextMenuSkillGrid.Items)
					{
						if (item is ToolStripMenuItem menuItem && menuItem.Name == "agentSkillAnalyzer")
							found = true;
					}

					if (!found)
					{
						var skillGridMenuItem1 = new ToolStripMenuItem("Analyze primary/shoveled resources...");
						skillGridMenuItem1.Click += skillGridMenuItemAnalyzeResorceChangesClick;
						_contextMenuSkillGrid.Items.Add(skillGridMenuItem1);
						var skillGridMenuItem2 = new ToolStripMenuItem("Analyze shoveling...");
						skillGridMenuItem2.Click += skillGridMenuItemShovelAnalyzerClick;
						_contextMenuSkillGrid.Items.Add(skillGridMenuItem2);
						var skillGridMenuItem = new ToolStripMenuItem("Agent Skill Analyzer...");
						skillGridMenuItem.Name = "agentSkillAnalyzer";
						skillGridMenuItem.Click += skillGridMenuItemAgentSkillAnalyserClick;
						_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
						var skillGridMenuItem3 = new ToolStripMenuItem("Find skill changes in period...");
						skillGridMenuItem3.Click += skillGridMenuItemSkillChangeFinderClick;
						_contextMenuSkillGrid.Items.Add(skillGridMenuItem3);
					}
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
						agentSkillExplorer.Setup(SchedulerState.SchedulerStateHolder, _container);
						agentSkillExplorer.ShowDialog(this);
					}
				}
			}

			if (e.KeyCode == Keys.Enter && toolStripTextBoxFilter.Focused)
			{
				_requestView.FilterGrid(toolStripTextBoxFilter.Text.Split(' '), SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary);
				e.Handled = true;
				e.SuppressKeyPress = true;
			}

			base.OnKeyDown(e);
		}

		private void skillGridMenuItemSkillChangeFinderClick(object sender, EventArgs e)
		{
			var result = new PersonsThatChangedPersonSkillsDuringPeriodFinder().Find(
				SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod, _scheduleView.AllSelectedPersons(_scheduleView.SelectedSchedules()));
			using (var analyzer = new PersonsThatChangedPersonSkills(result))
			{
				analyzer.ShowDialog(this);
			}
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
			SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation =
				!SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			toolStripButtonCalculation.Checked = !SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			if (SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation)
			{
				statusStrip1.BackColor = Color.Salmon;
			}
			else
			{
				foreach (var date in SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection())
				{
					SchedulerState.SchedulerStateHolder.MarkDateToBeRecalculated(date);
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
			switch ((ClipboardItems)e.ClickedItem.Tag)
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
			switch ((ClipboardItems)e.ClickedItem.Tag)
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
			switch ((ClipboardItems)e.ClickedItem.Tag)
			{
				case ClipboardItems.Special:
					_cutPasteHandlerFactory.For(_controlType).CopySpecial();
					break;
			}
		}

		#endregion

		#region Interface

		public SchedulingScreenState SchedulerState { get; private set; }
		
		public ClipHandler<IScheduleDay> ClipsHandlerSchedule { get; }

		public IGridlockManager LockManager { get; }

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
			toolStripProgressBar1.Maximum = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.DayCount() + 19;
			toolStripProgressBar1.Visible = true;

			schedulerSplitters1.SplitContainerAdvMainContainer.Visible = false;
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

			backStageButtonManiMenuImport.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportSchedule);
			backStageButtonMainMenuCopy.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.CopySchedule);

			setPermissionOnControls();
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

			if (_forceClose || SchedulerState == null)
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
						var mapper = new SchedulerSortCommandMapper(SchedulerState.SchedulerStateHolder, SchedulerSortCommandSetting.NoSortCommand, _container);
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
						SchedulerState.ScheduleTags.NonDeleted(),
						SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted(),
						SchedulerState.SchedulerStateHolder.DefaultSegmentLength, SchedulerState.SchedulerStateHolder.Schedules,
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

			((ToolStripMenuItem)_contextMenuSkillGrid.Items["IntraDay"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Day"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Period"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Month"]).Checked =
				_skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Week"]).Checked =
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
			if (button != null && button.Tag != null) zoom((ZoomLevel)button.Tag);
			else
			{
				var quickButton = sender as QuickButtonReflectable;
				if (quickButton != null && quickButton.ReflectedButton.Tag != null)
					zoom((ZoomLevel)quickButton.ReflectedButton.Tag);
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
				var schedulingOptions = new SchedulingOptions { UseRotations = false };
				var finderService = _container.Resolve<WorkShiftFinderService>();
				// This is not working now I presume (SelectedSchedules is probably not correct)
				foreach (IScheduleDay schedulePart in _scheduleView.SelectedSchedules())
				{
					if (!schedulePart.HasDayOff())
					{
						IEditableShift selectedShift = bestShiftChooser.PrepareAndChooseBestShift(schedulePart, schedulingOptions, finderService);
						if (selectedShift != null)
						{
							schedulePart.AddMainShift(selectedShift);
							_scheduleView.Presenter.ModifySchedulePart(new List<IScheduleDay> { schedulePart });
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
			var swapper = new Swapper(_scheduleView, _undoRedo, SchedulerState.SchedulerStateHolder, LockManager, this, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
			swapper.SwapRaw();
		}

		private void swapSelectedSchedules()
		{
			var swapper = new Swapper(_scheduleView, _undoRedo, SchedulerState.SchedulerStateHolder, LockManager, this, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
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
				if (SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Count > 0)
				{
					IList<IDayOffTemplate> displayList = SchedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs.NonDeleted().ToList();
					if (displayList.Count <= 0)
						return;

					// todo: remove comment when the user warning is ready for the other activities(delete, paste, swap etc.)
					var clone =
						(IScheduleDay)
						SchedulerState.SchedulerStateHolder.Schedules[SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.ElementAt(0).Value].
							ScheduledDay(new DateOnly(DateTime.MinValue.AddDays(1))).Clone();
					var selectedSchedules = _scheduleView.SelectedSchedules();
					if (!selectedSchedules.Any())
						return;

					var sortedList = selectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly.Date).OrderBy(d => d).ToArray();

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
					new ExternalExceptionHandler().AttemptToUseExternalResource(
						() => Clipboard.SetData("PersistableScheduleData", new int()));
					_cutPasteHandlerFactory.For(_controlType).PasteDayOff();
					_scheduleView.Presenter.ClipHandlerSchedule.Clear();
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

			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(schedulerSplitters1.Grid);
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
			var scheduleTag = (IScheduleTag)((ToolStripMenuItem)sender).Tag;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(schedulerSplitters1.Grid);
			IScheduleDayTagExtractor scheduleDayTagExtractor = new ScheduleDayTagExtractor(gridSchedulesExtractor.Extract());
			var gridlockTagCommand = new GridlockTagCommand(LockManager, scheduleDayTagExtractor, scheduleTag);
			gridlockTagCommand.Execute();
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripComboBoxAutoTagSelectedIndexChanged(object sender, EventArgs e)
		{
			_defaultScheduleTag = (IScheduleTag)toolStripComboBoxAutoTag.SelectedItem;
			_scheduleView.Presenter.DefaultScheduleTag = _defaultScheduleTag;
		}

		private void toolStripMenuItemChangeTag(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var scheduleTag = (IScheduleTag)((ToolStripMenuItem)sender).Tag;
			var gridSchedulesExtractor = new GridSchedulesExtractor(schedulerSplitters1.Grid);
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
			GridHelper.GridlockSelection(schedulerSplitters1.Grid, LockManager);
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
			GridHelper.GridlockAllAbsences(schedulerSplitters1.Grid, LockManager);
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
			GridHelper.GridlockFreeDays(schedulerSplitters1.Grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockSpecificDayOffClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var dayOffTemplate = (IDayOffTemplate)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockSpecificDayOff(schedulerSplitters1.Grid, LockManager, dayOffTemplate);
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
			var absence = (Absence)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockAbsences(schedulerSplitters1.Grid, LockManager, absence);
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
			var shiftCategory = (ShiftCategory)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockShiftCategories(schedulerSplitters1.Grid, LockManager, shiftCategory);
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
			GridHelper.GridlockAllShiftCategories(schedulerSplitters1.Grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemNotifyAgentClick(object sender, EventArgs e)
		{
			var selectedPersons = _scheduleView.AllSelectedPersons(_scheduleView.SelectedSchedules()).ToArray();
			if (!selectedPersons.Any())
				return;
			var agents = string.Join(",", selectedPersons.Select(a => a.Id.ToString()));
			var url = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle") + "Messages#" + agents;
			if (url.IsAnUrl())
				Process.Start(url);
		}

		private void toolStripMenuItemLockAllRestrictionsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.LockAllRestrictions(e.Button);
		}

		private void toolStripMenuItemAllPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllPreferences(e.Button);
		}

		private void toolStripMenuItemAllDaysOffMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllDaysOff(e.Button);
		}

		private void toolStripMenuItemAllShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllShiftsPreferences(e.Button);
		}

		private void toolStripMenuItemAllMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllMustHave(e.Button);
		}

		private void toolStripMenuItemAllFulfilledMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledMustHave(e.Button);
		}

		private void toolStripMenuItemAllFulFilledPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledPreferences(e.Button);
		}

		private void toolStripMenuItemAllAbsencePreferenceMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllAbsencePreference(e.Button);
		}

		private void toolStripMenuItemAllFulFilledAbsencesPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledAbsencesPreferences(e.Button);
		}

		private void toolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledDaysOffPreferences(e.Button);
		}

		private void toolStripMenuItemAllFulFilledShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledShiftsPreferences(e.Button);
		}

		private void toolStripMenuItemAllRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllRotations(e.Button);
		}

		private void toolStripMenuItemAllDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllDaysOffRotations(e.Button);
		}

		private void toolStripMenuItemAllShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllShiftsRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledDaysOffRotations(e.Button);
		}

		private void toolStripMenuItemAllFulFilledShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledShiftsRotations(e.Button);
		}

		private void toolStripMenuItemAllUnavailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllUnavailableStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllAvailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllAvailableStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllFulFilledStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllFulfilledStudentAvailability(e.Button);
		}

		private void toolStripMenuItemAllUnavailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllUnavailableAvailability(e.Button);
		}

		private void toolStripMenuItemAllAvailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
			executer.AllAvailableAvailability(e.Button);
		}

		private void toolStripMenuItemAllFulFilledAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(schedulerSplitters1.Grid, _container.Resolve<IRestrictionExtractor>(), LockManager, this);
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

			toolStripMenuItemViewReport.Visible = !_container.Resolve<IToggleManager>()
				.IsEnabled(Toggles.Report_Remove_Scheduled_Time_Per_Activity_From_Scheduler_45640);
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
			using (var skillSummery = new SkillSummary(SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills))
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
			var menuItem = (ToolStripMenuItem)sender;
			var skill = (ISkill)menuItem.Tag;

			using (var skillSummery = new SkillSummary(skill, SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					IAggregateSkill newSkill = handleSummeryEditMenuItems(menuItem, skillSummery);

					if (newSkill.AggregateSkills.Count != 0)
					{
						_virtualSkillHelper.EditAndRenameVirtualSkill(newSkill, skill.Name);
						schedulerSplitters1.ReplaceOldWithNew((ISkill)newSkill, skill);
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
			var menuItem = (ToolStripMenuItem)sender;
			var virtualSkill = (IAggregateSkill)menuItem.Tag;
			removeVirtualSkill(virtualSkill);
		}

		private void removeVirtualSkill(IAggregateSkill virtualSkill)
		{
			virtualSkill.ClearAggregateSkill();
			schedulerSplitters1.RemoveVirtualSkill((Skill)virtualSkill);
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
			var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items[action];
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
			var virtualSkill = (ISkill)skillSummary.AggregateSkillSkill;
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
			var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items["Edit"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemEditClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private void enableDeleteVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items["Delete"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemDeleteClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private IAggregateSkill handleSummeryEditMenuItems(ToolStripMenuItem menuItem, SkillSummary skillSummary)
		{
			var virtualSkill = (ISkill)skillSummary.AggregateSkillSkill;
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
				var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items["Delete"];
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
					GridHelper.HandleSelectAllSchedulingView((GridControl)sender);
					return;
				}
			}

			GridHelper.HandleSelectionKeys((GridControl)sender, e);
		}

		private void currentViewViewPasteCompleted(object sender, EventArgs e)
		{
			RecalculateResources();
			schedulerSplitters1.Grid.Invalidate();
		}

		public void RecalculateResources()
		{
			if (_backgroundWorkerRunning) return;

			if (_backgroundWorkerResourceCalculator.IsBusy)
				return;

			var daysToRecalculate = SchedulerState.SchedulerStateHolder.DaysToRecalculate;
			var numberOfDaysToRecalculate = daysToRecalculate.Count();
			if (numberOfDaysToRecalculate == 0 && _uIEnabled)
				return;

			if ((SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation || _teamLeaderMode) && _uIEnabled)
			{
				validatePersons();
				SchedulerState.SchedulerStateHolder.ClearDaysToRecalculate();
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
			if (!SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills.Any()) return;
			if (!SchedulerState.SchedulerStateHolder.DaysToRecalculate.Any()) return;
			if (SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillDays == null) return;

			using (_container.Resolve<CascadingResourceCalculationContextFactory>().Create(SchedulerState.SchedulerStateHolder.SchedulingResultState, false, new DateOnlyPeriod(SchedulerState.SchedulerStateHolder.DaysToRecalculate.Min(), SchedulerState.SchedulerStateHolder.DaysToRecalculate.Max())))
			{
				_optimizationHelperExtended.ResourceCalculateMarkedDays(new BackgroundWorkerWrapper(_backgroundWorkerResourceCalculator), SchedulerState.SchedulerStateHolder.ConsiderShortBreaks, true);
			}
		}

		private void validateAllPersons()
		{
			_personsToValidate.Clear();
			SchedulerState.SchedulerStateHolder.ChoosenAgents.ForEach(_personsToValidate.Add);
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
				_scheduleView.ValidatePersons(_personsToValidate, _validation);
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

		private void afterBackgroundWorkersCompleted(bool canceled)
		{
			_personsToValidate.Clear();

			using (PerformanceOutput.ForOperation("After validate"))
			{
				updateShiftEditor();
				if (_requestView != null)
					_requestView.NeedUpdate = true;
				reloadRequestView();
				drawSkillGrid();
			}
			releaseUserInterface(canceled);

			if (_schedulingOptions.ShowTroubleshot && _schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				var scheduleDays = _scheduleView.SelectedSchedules();
				if (scheduleDays.Any())
				{
					var startDay = scheduleDays.First();
					var endDay = scheduleDays.Last();
					var selectedPeriod = new DateOnlyPeriod(startDay.DateOnlyAsPeriod.DateOnly, endDay.DateOnlyAsPeriod.DateOnly);
					var validationResult = _container.Resolve<CheckScheduleHints>()
						.Execute(new SchedulePostHintInput(SchedulerState.SchedulerStateHolder.Schedules, _scheduleView.AllSelectedPersons(_scheduleView.SelectedSchedules()),
							selectedPeriod, new FixedBlockPreferenceProvider(_schedulingOptions), _schedulingOptions.UsePreferences));

					var specificTimeZone = new SpecificTimeZone(SchedulerState.SchedulerStateHolder.TimeZoneInfo);
					foreach (var result in validationResult.InvalidResources)
					{
						HintsHelper.BuildErrorMessages(result.ValidationErrors,specificTimeZone);
					}

					if (validationResult.InvalidResources.Any())
						new AgentValidationResult(validationResult).Show(this);
				}
			}

			_schedulingOptions.ShowTroubleshot = false;
			if (SikuliHelper.InteractiveMode)
			{
				var skillTabPage = _tabSkillData.TabPages[0];
				var totalSkill = skillTabPage.Tag as IAggregateSkill;
				var currentValidator = SikuliHelper.CurrentValidator;

				if (currentValidator != null)
					SikuliHelper.Validate(currentValidator, this, new SchedulerTestData(SchedulerState.SchedulerStateHolder, totalSkill));
				else
					SikuliHelper.ShowTaskDoneView(this);
			}

			var agentsDictionary = SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary;
			if (agentsDictionary.Count == 0)
			{
				SchedulerState.SchedulerStateHolder.ResetFilteredPersons();
				agentsDictionary = SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary;
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
					updateShiftEditor();
					RunActionWithDelay(updateShiftEditor, 50);
					var currentCell = _scheduleView.ViewGrid.CurrentCell;
					var selectedCols = _scheduleView.ViewGrid.Model.Selections.Ranges.ActiveRange.Width;
					if (!(_scheduleView is AgentRestrictionsDetailView) && currentCell.RowIndex == 0 && selectedCols == 1 &&
						currentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
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

			using (PerformanceOutput.ForOperation(@"Updating shift editor"))
			{
				toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXSelectedAgentsColon") + @" " +
													  _scheduleView.NumberOfSelectedPersons() + @" " +
													  LanguageResourceHelper.Translate("XXAgentsColon") + @" " +
													  SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Count + @" " +
													  LanguageResourceHelper.Translate("XXLoadedColon") + @" " + 
														  SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents.Count;

				if (!_showEditor)
					return;

				notesEditor.LoadNote(null);

				var scheduleDay = getSelectedScheduleDayForShiftEditor();

				if (scheduleDay == null)
				{
					splitterManager.DisableShiftEditor();
					return;
				}

				splitterManager.EnableShiftEditor();

				scheduleDay = SchedulerState.SchedulerStateHolder.Schedules[scheduleDay.Person].ReFetch(scheduleDay);

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

			if (e.ProgressPercentage > 0)
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
			DateOnly dateOnly = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate;
			_scheduleView.SetSelectedDateLocal(dateOnly);
			_scheduleView.ViewPasteCompleted += currentViewViewPasteCompleted;
			schedulerSplitters1.ElementHost1.Enabled = true;

			schedulerSplitters1.Grid.Cursor = Cursors.WaitCursor;
			wpfShiftEditor1.LoadFromStateHolder(SchedulerState.SchedulerStateHolder.CommonStateHolder);
			wpfShiftEditor1.Interval = _currentSchedulingScreenSettings.EditorSnapToResolution;

			loadLockMenues();
			loadScenarioMenuItems();

			toolStripStatusLabelStatus.Text = @"SETTING UP SKILL TABS...";
			ResumeLayout(true);
			Refresh();
			SuspendLayout();
			setupSkillTabs();

			toolStripStatusLabelStatus.Text = @"SETTING UP INFO TABS...";
			Refresh();
			setupInfoTabs();
			toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXLoadingFormThreeDots");
			ResumeLayout(true);
			Refresh();
			SuspendLayout();

			if (schedulerSplitters1.PinnedPage != null)
				schedulerSplitters1.TabSkillData.SelectedTab = schedulerSplitters1.PinnedPage;

			schedulerSplitters1.SplitContainerAdvMainContainer.Visible = true;
			toolStripStatusLabelScheduleTag.Visible = true;
			toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXAgentsColon") + @" " +
													  SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Count + @" " +
													  LanguageResourceHelper.Translate("XXLoadedColon") +
													  @" " + SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents.Count;
			toolStripStatusLabelNumberOfAgents.Visible = true;

			var loadedPeriod = SchedulerState.SchedulerStateHolder.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			setHeaderText(SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate, SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.EndDate, loadedPeriod.StartDate, loadedPeriod.EndDate);

			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _loadRequsts)
			{
				using (PerformanceOutput.ForOperation("Creating new RequestView"))
				{
					_requestView = new RequestView(schedulerSplitters1.HandlePersonRequestView1, SchedulerState, _undoRedo,
						SchedulerState.SchedulerStateHolder.SchedulingResultState.AllPersonAccounts, _eventAggregator);
				}

				_requestView.PropertyChanged += requestViewPropertyChanged;
				_requestView.SelectionChanged += requestViewSelectionChanged;
			}
			else
			{
				toolStripTabItem1.Visible = false;
				toolStripButtonRequestView.Visible = false;
			}

			schedulerSplitters1.Grid.VScrollPixel = false;
			schedulerSplitters1.Grid.HScrollPixel = false;
			schedulerSplitters1.Grid.Selections.Clear(true);
			var point = new Point((int)ColumnType.StartScheduleColumns, schedulerSplitters1.Grid.Rows.HeaderCount + 1);
			schedulerSplitters1.Grid.CurrentCell.MoveTo(point.Y, point.X, GridSetCurrentCellOptions.None);
			schedulerSplitters1.Grid.Selections.SelectRange(GridRangeInfo.Cell(point.Y, point.X), true);
			schedulerSplitters1.Grid.Select();
			var schedulerSortCommandSetting = _currentSchedulingScreenSettings.SortCommandSetting;
			var sortCommandMapper = new SchedulerSortCommandMapper(SchedulerState.SchedulerStateHolder, SchedulerSortCommandSetting.NoSortCommand,
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

			var localDate = TimeZoneHelper.ConvertFromUtc(request.FirstDateInRequest, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			selectCellFromPersonDate(request.PersonRequest.Person, new DateOnly(localDate));
		}

		private void setupRequestViewButtonStates()
		{
			var budgetPermissionService = _container.Resolve<IBudgetPermissionService>();
			toolStripButtonViewAllowance.Available = budgetPermissionService.IsAllowancePermitted ||
													 PrincipalAuthorization.Current()
														 .IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance);
			toolStripMenuItemViewAllowance.Visible = budgetPermissionService.IsAllowancePermitted ||
													 PrincipalAuthorization.Current()
														 .IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance);
			toolStripMenuItemViewAllowance.Enabled = budgetPermissionService.IsAllowancePermitted ||
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
				var dataSourceException = e.Error; //as CouldNotCreateTransactionException;
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
			splitterManager.ShowResult = !settings.HideResult;
			toolStripButtonShowResult.Checked = !settings.HideResult;
			_showResult = !settings.HideResult;
			splitterManager.ShowGraph = !settings.HideGraph;
			toolStripButtonShowGraph.Checked = !settings.HideGraph;
			_showGraph = !settings.HideGraph;
			splitterManager.ShowEditor = !settings.HideEditor;
			toolStripButtonShowEditor.Checked = !settings.HideEditor;
			_showEditor = !settings.HideEditor;
			_showInfoPanel = !settings.HideInfoPanel;
			toolStripButtonShowPropertyPanel.Checked = _showInfoPanel;

			toolStripButtonShowTexts.Checked = !settings.HideRibbonTexts;
			_showRibbonTexts = !settings.HideRibbonTexts;
			if (_teamLeaderMode)
			{
				splitterManager.ShowGraph = false;
				splitterManager.ShowResult = false;
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
					_requestView.CreatePersonRequestViewModels(SchedulerState, schedulerSplitters1.HandlePersonRequestView1);
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
						dest.Period.StartDateTimeLocal(SchedulerState.SchedulerStateHolder.TimeZoneInfo), dest.PersonMeetingCollection());
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
				bool quit = !SchedulerState.SchedulerStateHolder.ChoosenAgents.Contains(meetingPerson.Person);

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
				_scheduleView.Presenter.UpdateFromEditor();

				if (_currentZoomLevel == ZoomLevel.DayView && !(_scheduleView.Presenter.SortCommand is NoSortCommand))
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> { e.SchedulePart });

				updateShiftEditor();
			}
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

			schedulerSplitters1.Grid.Refresh();
			if (e.Cancelled)
			{
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXCancel");
				releaseUserInterface(e.Cancelled);
				return;
			}

			if (SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation)
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
				IGridlockRemoverForDelete gridlockRemoverForDelete = new GridlockRemoverForDelete(LockManager);
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
			var argument = (Tuple<DeleteOption, IList<IScheduleDay>>)e.Argument;
			var list = argument.Item2;
			_undoRedo.CreateBatch(string.Format(CultureInfo.CurrentCulture, Resources.UndoRedoDeleteSchedules, list.Count));
			var deleteService = new DeleteSchedulePartService();
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(SchedulerState.SchedulerStateHolder.SchedulingResultState,
					_container.Resolve<IScheduleDayChangeCallback>(),
					new ScheduleTagSetter(_defaultScheduleTag));
			if (!list.IsEmpty())
			{
				deleteService.Delete(list, argument.Item1, rollbackService, new BackgroundWorkerWrapper(_backgroundWorkerDelete),
					NewBusinessRuleCollection.AllForDelete(SchedulerState.SchedulerStateHolder.SchedulingResultState));
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
			var skillResultGridControlBase = (SkillResultGridControlBase)sender;
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
			GridChartManager.SetChartToolTip(e.Region, schedulerSplitters1.ChartControlSkillData);
		}

		private void chartControlSkillDataChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = Math.Max(1, (int)GridChartManager.GetIntervalValueForChartPoint(schedulerSplitters1.ChartControlSkillData, e.Point));
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
				schedulerSplitters1.Grid.ScrollCellInView(0, column + 1);
			}

			if (!_chartInIntradayMode)
			{
				schedulerSplitters1.Grid.ScrollCellInView(0, column + 1);
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

				_schedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff;
				_schedulingOptions.WorkShiftLengthHintOption =
					WorkShiftLengthHintOption.AverageWorkTime;
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

					using (var options = new SchedulingSessionPreferencesDialog(_schedulingOptions,
							SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories.NonDeleted(),
							_groupPagesProvider, SchedulerState.ScheduleTags.NonDeleted(),
							"SchedulingOptions", SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted()))
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

				_schedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.HourlyStaff;
				_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free;
				using (
					var options = new SchedulingSessionPreferencesDialog(_schedulingOptions,
						SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories.NonDeleted(),
						_groupPagesProvider, SchedulerState.ScheduleTags.NonDeleted(), "SchedulingOptionsActivities",
						SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted()))
				{
					if (options.ShowDialog(this) == DialogResult.OK)
					{
						_schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
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

			var scheduleDays = ((SchedulingAndOptimizeArgument)argument).SelectedScheduleDays;
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

			_personsToValidate.Clear();
			if(_scheduleView != null && _validation)
				_scheduleView.AllSelectedPersons(scheduleDays).ForEach(_personsToValidate.Add);

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
					var args = (BackToLegalShiftArgs)e.UserState;
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
					var arg = (TeleoptiProgressChangeMessage)e.UserState;
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

					if (progress != null)
					{
						var part = progress.SchedulePart;
						if (part != null)
						{
							scheduleStatusBarUpdate(string.Format(CultureInfo.CurrentCulture, "{0} {1}",
								SchedulerState.SchedulerStateHolder.CommonAgentName(part.Person),
								part.DateOnlyAsPeriod.DateOnly.ToShortDateString()));
						}
					}
					else
					{
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
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void backgroundWorkerSchedulingDoWork(object sender, DoWorkEventArgs e)
		{
			_totalScheduled = 0;
			_undoRedo.CreateBatch(Resources.UndoRedoScheduling);
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var scheduleDays = argument.SelectedScheduleDays;
			var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(scheduleDays).Value;
			_schedulingOptions.NotAllowedShiftCategories.Clear();

			AdvanceLoggingService.LogSchedulingInfo(_schedulingOptions,
				scheduleDays.Select(x => x.Person).Distinct().Count(),
				selectedPeriod.DayCount(),
				() => runBackgroundWorkerScheduling(e));
			_undoRedo.CommitBatch();

		}

		private void runBackgroundWorkerScheduling(DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			if (argument.OptimizationMethod == OptimizationMethod.BackToLegalShift)
			{
				var command = _container.Resolve<BackToLegalShiftCommand>();
				command.Execute(new BackgroundWorkerWrapper(_backgroundWorkerScheduling), argument.SelectedScheduleDays,
					SchedulerState.SchedulerStateHolder.SchedulingResultState, SchedulerState.SchedulerStateHolder.ChoosenAgents);
			}
			else
			{
				var desktopScheduling = _container.Resolve<DesktopScheduling>();
				var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(argument.SelectedScheduleDays).Value;
				var selectedAgents = argument.SelectedScheduleDays.Select(x => x.Person).Distinct();
				var backgroundWrapper = new BackgroundWorkerWrapper(_backgroundWorkerScheduling);
				desktopScheduling.Execute(new SchedulingCallbackForDesktop(backgroundWrapper, _schedulingOptions), _schedulingOptions, backgroundWrapper, selectedAgents, selectedPeriod);
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
			if (_inUpdate) return;
			_inUpdate = true;
			toolStripStatusLabelStatus.Text = message;
			statusStrip1.Refresh();
			Application.DoEvents();
			_inUpdate = false;
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
			schedulerSplitters1.Grid.Invalidate();
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
					schedulerSplitters1.Grid.Invalidate();
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
					toolStripStatusLabelStatus.Text = @"Period value = " +
													  progress.Value + @" (" + progress.Delta + @")";
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
				schedulerSplitters1.Grid.Cursor = Cursors.Default;
				schedulerSplitters1.Grid.Enabled = true;
				schedulerSplitters1.Grid.Cursor = Cursors.Default;
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
				if (SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation)
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
				LogManager.GetLogger(typeof(SchedulingScreen)).Error(ex.ToString());
			}
		}

		private void backgroundWorkerOvertimeSchedulingDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			var schedulingOptions = _schedulingOptions;
			schedulingOptions.DayOffTemplate = SchedulerState.SchedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			bool lastCalculationState = SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			_totalScheduled = 0;
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var scheduleDays = argument.SelectedScheduleDays;

			var matrixesOfSelectedScheduleDays = _container.Resolve<MatrixListFactory>().CreateMatrixListForSelection(SchedulerState.SchedulerStateHolder.Schedules, scheduleDays);
			if (!matrixesOfSelectedScheduleDays.Any())
				return;

			_undoRedo.CreateBatch(Resources.UndoRedoScheduling);

			_container.Resolve<ScheduleOvertime>()
				.Execute(argument.OvertimePreferences, new BackgroundWorkerWrapper(_backgroundWorkerOvertimeScheduling),
								LockManager.UnlockedDays(scheduleDays));

			SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
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
			foreach (IPerson permittedPerson in SchedulerState.SchedulerStateHolder.ChoosenAgents)
			{
				_personsToValidate.Add(permittedPerson);
			}

			RecalculateResources();
		}

		private void backgroundWorkerOptimizationDoWork(object sender, DoWorkEventArgs e)
		{
			_undoRedo.CreateBatch(Resources.UndoRedoReOptimize);
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var scheduleDays = argument.SelectedScheduleDays;
			var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(scheduleDays).Value;
			var dateOnlyList = selectedPeriod.DayCollection();
			SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillDaysOnDateOnly(dateOnlyList);
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var schedulingOptions = _container.Resolve<ISchedulingOptionsCreator>().CreateSchedulingOptions(optimizerPreferences);
			schedulingOptions.NotAllowedShiftCategories.Clear();
			AdvanceLoggingService.LogOptimizationInfo(optimizerPreferences, scheduleDays.Select(x => x.Person).Distinct().Count(),
				dateOnlyList.Count, () => runBackgroupWorkerOptimization(e));
			_undoRedo.CommitBatch();
		}

		private void runBackgroupWorkerOptimization(DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(argument.DaysOffPreferences);
			var selectedPeriod = new PeriodExtractorFromScheduleParts().ExtractPeriod(argument.SelectedScheduleDays).Value;
			var selectedAgents = argument.SelectedScheduleDays.Select(x => x.Person).Distinct();
			_container.Resolve<OptimizationDesktopExecuter>().Execute(
				new BackgroundWorkerWrapper(_backgroundWorkerOptimization),
				SchedulerState.SchedulerStateHolder,
				selectedAgents,
				selectedPeriod,
				_optimizationPreferences,
				dayOffOptimizationPreferenceProvider);

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
			_container.Resolve<IMbCacheFactory>().Invalidate();
			IList<LoaderMethod> methods = new List<LoaderMethod>();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(_scenario);
				methods.Add(new LoaderMethod(loadSchedulingScreenState, null));
				methods.Add(new LoaderMethod(loadCommonStateHolder, LanguageResourceHelper.Translate("XXLoadingDataTreeDots")));
				methods.Add(new LoaderMethod(loadSkills, null));
				methods.Add(new LoaderMethod(loadSettings, null));
				methods.Add(new LoaderMethod(loadAuditingSettings, null));
				methods.Add(new LoaderMethod(loadPeople, LanguageResourceHelper.Translate("XXLoadingPeopleTreeDots")));
				methods.Add(new LoaderMethod(filteringPeopleAndSkills, null));
				methods.Add(new LoaderMethod(loadSchedules, LanguageResourceHelper.Translate("XXLoadingSchedulesTreeDots")));
				if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _loadRequsts)
					methods.Add(new LoaderMethod(loadRequests, LanguageResourceHelper.Translate("XXLoadingRequestsThreeDots")));

				methods.Add(new LoaderMethod(loadSkillDays, LanguageResourceHelper.Translate("XXLoadingSkillDataTreeDots")));
				methods.Add(new LoaderMethod(loadBpos, null));
				methods.Add(new LoaderMethod(loadDefinitionSets, null));
				methods.Add(new LoaderMethod(loadAccounts, null));
				methods.Add(new LoaderMethod(loadSeniorityWorkingDays, null));

				using (PerformanceOutput.ForOperation("Loading all data for scheduler"))
				{
					foreach (var method in methods)
					{
						backgroundWorkerLoadData.ReportProgress(1, method.StatusStripString);
						method.Action.Invoke(uow, SchedulerState);
						if (backgroundWorkerLoadData.CancellationPending)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				var period = new ScheduleDateTimePeriod(SchedulerState.SchedulerStateHolder.RequestedPeriod.Period(),
					SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents);
				if (!_teamLeaderMode)
				{
					var options = new SchedulingOptions();
					options.ConsiderShortBreaks = _container.Resolve<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>()
						.CanHaveShortBreak(SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents,
							SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
					SchedulerState.SchedulerStateHolder.ConsiderShortBreaks = options.ConsiderShortBreaks;
				}
				else
				{
					SchedulerState.SchedulerStateHolder.ConsiderShortBreaks = false;
				}
				initMessageBroker(period.LoadedPeriod());
			}

			if (!SchedulerState.SchedulerStateHolder.SchedulingResultState.SkipResourceCalculation && !_teamLeaderMode)
			{
				backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXCalculatingResourcesDotDotDot"));
				var requestPeriod = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
				using (_container.Resolve<CascadingResourceCalculationContextFactory>().Create(SchedulerState.SchedulerStateHolder.SchedulingResultState, false, requestPeriod))
				{
					_optimizationHelperExtended.ResourceCalculateAllDays(new BackgroundWorkerWrapper(backgroundWorkerLoadData), true);
				}

				backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXInitializingTreeDots"));
				foreach (var skillDay in SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillDays.ToSkillDayEnumerable())
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						skillStaffPeriod.CalculateEstimatedServiceLevel();
					}
				}

				var workShiftWorkTime = _container.Resolve<IWorkShiftWorkTime>();
				var shiftBags = new HashSet<IRuleSetBag>();
				var ruleSets = new HashSet<IWorkShiftRuleSet>();
				foreach (var person in SchedulerState.SchedulerStateHolder.ChoosenAgents)
				{
					if (person.Period(requestPeriod.StartDate) != null &&
						person.Period(requestPeriod.StartDate).RuleSetBag != null)
					{
						shiftBags.Add(person.Period(requestPeriod.StartDate).RuleSetBag);
					}
				}

				foreach (var ruleSetBag in shiftBags)
				{
					foreach (var workShiftRuleSet in ruleSetBag.RuleSetCollection)
					{
						ruleSets.Add(workShiftRuleSet);
					}
				}

				foreach (var ruleSet in ruleSets)
				{
					workShiftWorkTime.CalculateMinMax(ruleSet, new EffectiveRestriction());
				}
			}

			if (e.Cancel)
				return;

			SchedulerState.SchedulerStateHolder.ClearDaysToRecalculate();

			if (_validation)
				validation();

			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXValidations"));
			////TODO move into the else clause above
			_detectedTimeZoneInfos.Add(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			foreach (IPerson permittedPerson in SchedulerState.SchedulerStateHolder.ChoosenAgents)
			{
				validatePersonAccounts(permittedPerson);
				_detectedTimeZoneInfos.Add(permittedPerson.PermissionInformation.DefaultTimeZone());
			}

			SchedulerState.SchedulerStateHolder.SchedulingResultState.Schedules.ModifiedPersonAccounts.Clear();
			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXInitializingTreeDots"));

			foreach (var tag in SchedulerState.ScheduleTags.NonDeleted())
			{
				if (tag.Id != _currentSchedulingScreenSettings.DefaultScheduleTag) continue;
				_defaultScheduleTag = tag;
				break;
			}

			var agentsDictionary = SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary;
			if (agentsDictionary.Count == 0)
			{
				SchedulerState.SchedulerStateHolder.ResetFilteredPersons();
				agentsDictionary = SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary;
			}
			schedulerSplitters1.RefreshTabInfoPanels(agentsDictionary.Values);

			GridHelper.GridlockWriteProtected(SchedulerState.SchedulerStateHolder, LockManager);

			_lastSaved = DateTime.Now;
			backgroundWorkerLoadData.ReportProgress(1, LanguageResourceHelper.Translate("XXLoadingFormThreeDots"));
		}


		private void validatePersonAccounts(IPerson person)
		{
			IScheduleRange range = SchedulerState.SchedulerStateHolder.SchedulingResultState.Schedules[person];
			var rule = new NewPersonAccountRule(SchedulerState.SchedulerStateHolder.SchedulingResultState.Schedules,
				SchedulerState.SchedulerStateHolder.SchedulingResultState.AllPersonAccounts);
			IList<IBusinessRuleResponse> toRemove = new List<IBusinessRuleResponse>();
			IList<IBusinessRuleResponse> exposedBusinessRuleResponseCollection =
				((ScheduleRange)range).ExposedBusinessRuleResponseCollection();
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

			DateOnlyPeriod reqPeriod = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
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
					SchedulerState.SchedulerStateHolder.ChoosenAgents.Count));
			_personsToValidate.Clear();
			foreach (IPerson permittedPerson in SchedulerState.SchedulerStateHolder.ChoosenAgents)
			{
				_personsToValidate.Add(permittedPerson);
			}
			var rulesToRun = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();

			var resolvedTranslatedString = LanguageResourceHelper.Translate("XXValidatingPersons2");
			var validatedCount = 0;
			foreach (var persons in _personsToValidate.Batch(100))
			{
				var batchedPeople = persons as IList<IPerson> ?? persons.ToList();
				validatedCount += batchedPeople.Count;
				backgroundWorkerLoadData.ReportProgress(0,
					string.Format(CultureInfo.CurrentCulture, resolvedTranslatedString, validatedCount,
						SchedulerState.SchedulerStateHolder.ChoosenAgents.Count));
				SchedulerState.SchedulerStateHolder.Schedules.ValidateBusinessRulesOnPersons(batchedPeople, rulesToRun);
			}

			_personsToValidate.Clear();
		}

		private void setupRequestPresenter()
		{
			_handleBusinessRuleResponse = new HandleBusinessRuleResponse();
			_requestPresenter = new RequestPresenter(_personRequestAuthorizationChecker);
			_requestPresenter.SetUndoRedoContainer(_undoRedo);
		}

		private void loadAccounts(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			var rep = new PersonAbsenceAccountRepository(uow);
			if (_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_LoadLessPersonAccountsWhenOpeningScheduler_78487))
			{
				SchedulerState.SchedulerStateHolder.SchedulingResultState.AllPersonAccounts = rep.LoadByUsers(SchedulerState.SchedulerStateHolder.ChoosenAgents);
			}
			else
			{
				SchedulerState.SchedulerStateHolder.SchedulingResultState.AllPersonAccounts = rep.LoadAllAccounts();
			}
		}

		private void loadDefinitionSets(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository =
				new MultiplicatorDefinitionSetRepository(uow);
			MultiplicatorDefinitionSet = multiplicatorDefinitionSetRepository.FindAllDefinitions();
		}

		private void filteringPeopleAndSkills(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			using (PerformanceOutput.ForOperation("Executing and filtering loader decider"))
			{
				ICollection<IPerson> peopleInOrg = SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents;
				int peopleCountFromBeginning = peopleInOrg.Count;
				var decider = _teamLeaderMode ? new PeopleAndSkillLoaderDeciderForTeamLeaderMode() : _container.Resolve<IPeopleAndSkillLoaderDecider>();
				var result = decider.Execute(SchedulerState.SchedulerStateHolder.RequestedScenario, SchedulerState.SchedulerStateHolder.RequestedPeriod.Period(),
					SchedulerState.SchedulerStateHolder.ChoosenAgents);

				int removedPeople = result.FilterPeople(peopleInOrg);
				log.Info("Removed " + removedPeople + " people when filtering (original: " + peopleCountFromBeginning +
						 ")");

				//RK: jag tycker detta är fel, men det rättar en bugg för nu. 
				//Filtereringen gör rätt utan detta, men jag vet inte vilken lista som egentligen används för
				//visning, db-sparning, resursberäkning, visning etc. Så det blir såhär tills större häv

				peopleInOrg = new HashSet<IPerson>(peopleInOrg);
				SchedulerState.SchedulerStateHolder.ChoosenAgents.ForEach(peopleInOrg.Add);
				SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents = peopleInOrg;
				log.Info("No, changed my mind... Removed " + (peopleCountFromBeginning - peopleInOrg.Count) + " people.");
				var skills = stateHolder.SchedulerStateHolder.SchedulingResultState.Skills;
				int orgSkills = skills.Length;
				int removedSkills = result.FilterSkills(skills, stateHolder.SchedulerStateHolder.SchedulingResultState.RemoveSkill, s => stateHolder.SchedulerStateHolder.SchedulingResultState.AddSkills(s));
				log.Info("Removed " + removedSkills + " skill when filtering (original: " + orgSkills + ")");
			}
		}

		private void loadSkills(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			var staffingCalculatorServiceFacade = _container.Resolve<IStaffingCalculatorServiceFacade>();
			ICollection<ISkill> skills = new SkillRepository(uow).FindAllWithSkillDays(stateHolder.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			foreach (ISkill skill in skills)
			{
				skill.SkillType.StaffingCalculatorService = staffingCalculatorServiceFacade;
				stateHolder.SchedulerStateHolder.SchedulingResultState.AddSkills(skill);
			}
		}

		private void loadSettings(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			using (PerformanceOutput.ForOperation("Loading settings"))
			{
				SchedulerState.SchedulerStateHolder.LoadSettings(uow, new RepositoryFactory());
			}
		}

		private void loadAuditingSettings(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			var repository = new AuditSettingRepository(new ThisUnitOfWork(uow));
			var auditSetting = repository.Read();
			_isAuditingSchedules = auditSetting.IsScheduleEnabled;
		}

		private void loadSchedules(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			var period = stateHolder.SchedulerStateHolder.RequestedPeriod.Period();
			using (PerformanceOutput.ForOperation("Loading schedules " + period))
			{
				var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true)
				{
					LoadDaysAfterLeft = true
				};
				stateHolder.SchedulerStateHolder.LoadSchedules(_container.Resolve<IFindSchedulesForPersons>(), stateHolder.SchedulerStateHolder.SchedulingResultState.LoadedAgents, scheduleDictionaryLoadOptions, period);
				SchedulerState.SchedulerStateHolder.Schedules.SetUndoRedoContainer(_undoRedo);
			}
			SchedulerState.SchedulerStateHolder.Schedules.PartModified += schedulesPartModified;
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

				if (_scheduleView is AgentRestrictionsDetailView)
					return;

				if (_selectedPeriod.Contains(e.ModifiedPeriod))
					_totalScheduled++;

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

		private void loadPeople(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			using (PerformanceOutput.ForOperation("Loading people"))
			{
				_optionalColumns = new OptionalColumnRepository(uow).GetOptionalColumns<Person>();
				var personRep = new PersonRepository(new ThisUnitOfWork(uow));
				IPeopleLoader loader;
				if (_teamLeaderMode)
				{
					loader = new PeopleLoaderForTeamLeaderMode(uow, SchedulerState.SchedulerStateHolder,
						new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView,
							SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod), new RepositoryFactory());
				}
				else
				{
					loader = new PeopleLoader(personRep, new ContractRepository(uow), SchedulerState.SchedulerStateHolder,
						new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView,
							SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod), new SkillRepository(uow));
				}

				loader.Initialize();

				if(_container.Resolve<IToggleManager>().IsEnabled(Toggles.SchedulePeriod_HideChineseMonth_78424))
				{
					foreach (var schedulerStateChoosenAgent in SchedulerState.SchedulerStateHolder.ChoosenAgents)
					{
						foreach (var schedulePeriod in schedulerStateChoosenAgent.PersonSchedulePeriodCollection)
						{
							((SchedulePeriod) schedulePeriod).Toggle78424 = true;
						}
					}
				}
			}
			// part of the workaround because we can't press cancel before this / Ola
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, true);
		}

		private void loadRequests(IUnitOfWork uow, SchedulingScreenState stateHolder)
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

		private void loadSeniorityWorkingDays(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			var result = new SeniorityWorkDayRanksRepository(uow).LoadAll();
			var seniorityWorkDayRankses = result as ISeniorityWorkDayRanks[] ?? result.ToArray();
			var workDayRanks = seniorityWorkDayRankses.IsEmpty() ? new SeniorityWorkDayRanks() : seniorityWorkDayRankses.First();
			stateHolder.SchedulerStateHolder.SchedulingResultState.SeniorityWorkDayRanks = workDayRanks;
		}

		private static void loadCommonStateHolder(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			stateHolder.SchedulerStateHolder.LoadCommonState(uow, new RepositoryFactory());
			if (!stateHolder.SchedulerStateHolder.CommonStateHolder.DayOffs.Any())
				throw new StateHolderException("You must create at least one Day Off in Options!");
		}

		private void loadSchedulingScreenState(IUnitOfWork uow, SchedulingScreenState state)
		{
			state.Fill(uow);
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
			if (schedulerSplitters1.Grid.Enabled)
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
				bool success = persister.TryPersist(SchedulerState.SchedulerStateHolder.Schedules,
					SchedulerState.PersonRequests,
					_modifiedWriteProtections,
					SchedulerState.ModifiedWorkflowControlSets,
					out foundConflicts);

				if (!success && foundConflicts != null)
				{
					handleConflicts(new List<IPersistableScheduleData>(), foundConflicts);
					doSaveProcess();
				}

				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var accountConflictCollector = new PersonAccountConflictCollector(new DatabaseVersion(new ThisUnitOfWork(uow)));
					var accountConflicts = accountConflictCollector.GetConflicts(SchedulerState.SchedulerStateHolder.Schedules.ModifiedPersonAccounts);
					if (accountConflicts != null && accountConflicts.Any()) refreshData();
				}

				var personAccountPersister = _container.Resolve<IPersonAccountPersister>();
				personAccountPersister.Persist(SchedulerState.SchedulerStateHolder.Schedules.ModifiedPersonAccounts, SchedulerState.SchedulerStateHolder.Schedules);

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
				schedulerSplitters1.Grid.Cursor = Cursors.WaitCursor;
				schedulerSplitters1.Grid.Enabled = false;
				schedulerSplitters1.Grid.Cursor = Cursors.WaitCursor;
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
			if (toolStripSpinningProgressControl1.SpinningProgressControl == null)
				toolStripSpinningProgressControl1 = new ToolStripSpinningProgressControl();
			toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = false;
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
			skillGridMenuItem.Click += toolStripMenuItemUseShrinkageClick;
			skillGridMenuItem.Checked = _shrinkage;
			skillGridMenuItem.Name = "UseShrinkage";
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			var skillGridMenuSeparator = new ToolStripSeparator();
			_contextMenuSkillGrid.Items.Add(skillGridMenuSeparator);
			skillGridMenuItem = new ToolStripMenuItem(Resources.CreateSkillSummery);
			skillGridMenuItem.Click += skillGridMenuItemClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			skillGridMenuItem = new ToolStripMenuItem(Resources.EditSkillSummery) { Name = "Edit", Enabled = false };
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			skillGridMenuItem = new ToolStripMenuItem(Resources.DeleteSkillSummery) { Name = "Delete", Enabled = false };
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
			var model = new ResourceCalculationAnalyzerModel(SchedulerState.SchedulerStateHolder, _container, _optimizationHelperExtended, selectedDate, selectedTime, _shrinkage);
			using (var resourceChanges = new ResourceCalculationAnalyzerView(model))
			{
				resourceChanges.ShowDialog(this);
			}
		}

		private void skillGridMenuItemShovelAnalyzerClick(object sender, EventArgs e)
		{
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				using (_container.Resolve<CascadingResourceCalculationContextFactory>().Create(SchedulerState.SchedulerStateHolder.SchedulingResultState, false, _scheduleView.SelectedDateLocal().ToDateOnlyPeriod()))
				{
					using (var resourceChanges = new ShovelingAnalyzerView(_container.Resolve<IResourceCalculation>(), _container.Resolve<ITimeZoneGuard>()))
					{
						resourceChanges.FillForm(SchedulerState.SchedulerStateHolder.SchedulingResultState,
							_skillIntradayGridControl.Presenter.Skill,
							_scheduleView.SelectedDateLocal(),
							_skillIntradayGridControl.Presenter.SelectedIntervalTime().GetValueOrDefault());
						resourceChanges.ShowDialog(this);
					}
				}
			}
		}

		private void skillGridMenuItemAgentSkillAnalyserClick(object sender, EventArgs e)
		{
			using (var analyzer = new AgentSkillAnalyzer(SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents,
				SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills, SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillDays,
				SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod, _container.Resolve<CreateIslands>(), _container.Resolve<DesktopSchedulingContext>(), SchedulerState.SchedulerStateHolder))
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
				if (tsi.GetType() == typeof(QuickButtonReflectable))
				{
					var toolStripButton = (QuickButtonReflectable)tsi;
					tsi.ToolTipText = toolStripButton.ReflectedButton.ToolTipText;
				}
				else if (tsi.GetType() == typeof(QuickDropDownButtonReflectable))
				{
					var toolStripDropDownButton = (QuickDropDownButtonReflectable)tsi;
					tsi.ToolTipText = toolStripDropDownButton.ReflectedDropDownButton.ToolTipText;
				}
			}
			ribbonControlAdv1.TabGroups[0].Name = Resources.ShiftEditor;
			ribbonControlAdv1.TabGroups[1].Name = Resources.MainGrid;
			_gridrowInChartSettingButtons = new GridRowInChartSettingButtons();
			var chartsetteinghost = new ToolStripControlHost(_gridrowInChartSettingButtons);
			toolStripExGridRowInChartButtons.Items.Add(chartsetteinghost);
			_gridrowInChartSettingButtons.SetButtons();
			_gridChartManager = new GridChartManager(schedulerSplitters1.ChartControlSkillData, true, true, true);
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
			_currentIntraDayDate = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate;
			_tabSkillData.TabPages.Clear();
			_tabSkillData.ImageList = imageListSkillTypeIcons;
			foreach (
				ISkill virtualSkill in
					_virtualSkillHelper.LoadVirtualSkills(SchedulerState.SchedulerStateHolder.SchedulingResultState.VisibleSkills).OrderBy(s => s.Name))
			{
				TabPageAdv tab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
				tab.Tag = virtualSkill;
				tab.ImageIndex = 4;
				_tabSkillData.TabPages.Add(tab);
				enableEditVirtualSkill(virtualSkill);
				enableDeleteVirtualSkill(virtualSkill);
			}

			foreach (ISkill skill in SchedulerState.SchedulerStateHolder.SchedulingResultState.VisibleSkills.OrderBy(s => s.Name))
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
			var requestedPeriod = SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			var outerPeriod = new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-7), requestedPeriod.EndDate.AddDays(7));

			_agentInfoControl = new AgentInfoControl(_groupPagesProvider, _container, outerPeriod,
				requestedPeriod, _container.Resolve<IRestrictionExtractor>(), SchedulerState.SchedulerStateHolder, _optionalColumns);
			schedulerSplitters1.InsertAgentInfoControl(_agentInfoControl);

			//container can fix this to one row
			ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson =
				new CachedNumberOfEachCategoryPerPerson(SchedulerState.SchedulerStateHolder.Schedules, SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			ICachedNumberOfEachCategoryPerDate cachedNumberOfEachCategoryPerDate =
				new CachedNumberOfEachCategoryPerDate(SchedulerState.SchedulerStateHolder.Schedules, SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			var allowedSc = new List<IShiftCategory>();
			foreach (var shiftCategory in SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories)
			{
				var sc = shiftCategory as IDeleteTag;
				if (sc != null && !sc.IsDeleted)
					allowedSc.Add(shiftCategory);
			}
			ICachedShiftCategoryDistribution cachedShiftCategoryDistribution =
				new CachedShiftCategoryDistribution(SchedulerState.SchedulerStateHolder.Schedules, SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
					cachedNumberOfEachCategoryPerPerson,
					allowedSc);
			_shiftCategoryDistributionModel = new ShiftCategoryDistributionModel(cachedShiftCategoryDistribution,
				cachedNumberOfEachCategoryPerDate,
				cachedNumberOfEachCategoryPerPerson,
				SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
				SchedulerState.SchedulerStateHolder);
			_shiftCategoryDistributionModel.SetFilteredPersons(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values);
			schedulerSplitters1.InsertShiftCategoryDistributionModel(_shiftCategoryDistributionModel);
			schedulerSplitters1.InsertRestrictionNotAbleToBeScheduledReportModel(
				_container.Resolve<RestrictionNotAbleToBeScheduledReport>());
			schedulerSplitters1.InsertValidationAlertsModel(new ValidationAlertsModel(SchedulerState.SchedulerStateHolder.Schedules, NameOrderOption.LastNameFirstName, SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod));
			schedulerSplitters1.ToggelPropertyPanel(!toolStripButtonShowPropertyPanel.Checked);
		}

		private PersonsFilterView _cachedPersonsFilterView;

		private PersonsFilterView getCachedPersonsFilterView()
		{
			if (_cachedPersonsFilterView == null || _cachedPersonsFilterView.IsDisposed)
			{
				var permittedPersons = SchedulerState.SchedulerStateHolder.ChoosenAgents.Select(p => p.Id.Value).ToList();

				_cachedPersonsFilterView =
					new PersonsFilterView(SchedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
						SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary,
						_container,
						ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory()
							.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage),
						string.Empty,
						permittedPersons, true);
			}

			_cachedPersonsFilterView.SetCurrentFilter(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary);
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
				SchedulerState.SchedulerStateHolder.FilterPersons(scheduleFilterView.SelectedAgentGuids());

				toolStripButtonFilterAgents.Checked = SchedulerState.SchedulerStateHolder.AgentFilter();

				if (_scheduleView != null)
				{
					if (_scheduleView.Presenter.SortCommand == null || _scheduleView.Presenter.SortCommand is NoSortCommand)
						_scheduleView.Presenter.ApplyGridSort();
					else
						_scheduleView.Sort(_scheduleView.Presenter.SortCommand);

					schedulerSplitters1.Grid.Refresh();
					GridHelper.GridlockWriteProtected(SchedulerState.SchedulerStateHolder, LockManager);
					schedulerSplitters1.Grid.Refresh();
				}
				if (_requestView != null)
					_requestView.FilterPersons(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Keys);
				drawSkillGrid();
			}
		}

		private void prepareAgentRestrictionView(ScheduleViewBase detailView,
			IList<IPerson> persons, DateOnlyPeriod selectedPeriod)
		{
			if (persons.Count == 0) return;
			var view = (AgentRestrictionsDetailView)detailView;
			schedulerSplitters1.SplitContainerView.SplitterDistance = 300;
			schedulerSplitters1.SetSelectedAgentsOnAgentsNotPossibleToSchedule(persons, selectedPeriod, view);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void zoom(ZoomLevel level)
		{
			schedulerSplitters1.SuspendLayout();
			IList<IScheduleDay> scheduleParts = null;
			IScheduleSortCommand sortCommand = null;
			IList<IPerson> selectedPersons = null;
			var selectedPeriod = new DateOnlyPeriod();
			int currentSortColumn = 0;
			bool isAscendingSort = false;

			if (_scheduleView != null)
			{
				schedulerSplitters1.Grid.ContextMenuStrip = null;
				scheduleParts = _scheduleView.SelectedSchedules();
				selectedPersons = new List<IPerson>(_scheduleView.AllSelectedPersons(scheduleParts));
				IEnumerable<DateOnly> selectedDates = _scheduleView.AllSelectedDates(scheduleParts);
				if(selectedDates.Any())
					selectedPeriod = new DateOnlyPeriod(selectedDates.Min(), selectedDates.Max());
				sortCommand = _scheduleView.Presenter.SortCommand;
				currentSortColumn = _scheduleView.Presenter.CurrentSortColumn;
				isAscendingSort = _scheduleView.Presenter.IsAscendingSort;
				_scheduleView.RefreshSelectionInfo -= scheduleViewRefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor -= scheduleViewRefreshShiftEditor;
				_scheduleView.ViewPasteCompleted -= currentViewViewPasteCompleted;
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
					_scheduleView = new DayViewNew(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder, LockManager, SchedulePartFilter, ClipsHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _undoRedo);
					_scheduleView.SetSelectedDateLocal(_dateNavigateControl.SelectedDate);
					break;
				case ZoomLevel.WeekView:
					_scheduleView = new WeekView(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder, LockManager, SchedulePartFilter, ClipsHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _undoRedo);
					break;
				case ZoomLevel.PeriodView:
					_scheduleView = new PeriodView(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder, LockManager, SchedulePartFilter, ClipsHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _undoRedo);
					break;
				case ZoomLevel.Overview:
					_scheduleView = new OverviewView(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder, LockManager, SchedulePartFilter, ClipsHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _undoRedo);
					break;
				case ZoomLevel.RequestView:
					restrictionViewMode(false);
					_scheduleView = new PeriodView(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder, LockManager, SchedulePartFilter, ClipsHandlerSchedule,
						_overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _undoRedo);
					schedulerSplitters1.ElementHostRequests.BringToFront();
					schedulerSplitters1.ElementHostRequests.ContextMenuStrip = contextMenuStripRequests;
					enableRibbonForRequests(true);
					ActiveControl = schedulerSplitters1.ElementHostRequests;
					break;
				case ZoomLevel.RestrictionView:
					//restriction view
					Cursor = Cursors.WaitCursor;
					schedulerSplitters1.Grid.BringToFront();
					_scheduleView = new AgentRestrictionsDetailView(schedulerSplitters1.Grid, SchedulerState.SchedulerStateHolder,
						LockManager, SchedulePartFilter, ClipsHandlerSchedule, _overriddenBusinessRulesHolder, callback,
						_defaultScheduleTag, _workShiftWorkTime, _undoRedo);
					_scheduleView.TheGrid.ContextMenuStrip = contextMenuStripRestrictionView;
					prepareAgentRestrictionView(_scheduleView, selectedPersons, selectedPeriod);

					if (scheduleParts != null)
					{
						if (!scheduleParts.IsEmpty())
						{
							_dateNavigateControl.SetSelectedDateNoInvoke(scheduleParts[0].DateOnlyAsPeriod.DateOnly);
						}
					}

					ActiveControl = schedulerSplitters1.Grid;
					restrictionViewMode(true);
					Cursor = Cursors.Default;

					break;
				default:
					throw new InvalidEnumArgumentException(nameof(level), (int)level, typeof(ZoomLevel));
			}
			_previousZoomLevel = _currentZoomLevel;
			_currentZoomLevel = level;

			if (_currentZoomLevel != ZoomLevel.RequestView && _currentZoomLevel != ZoomLevel.RestrictionView)
			{
				restrictionViewMode(false);
				schedulerSplitters1.Grid.BringToFront();
				schedulerSplitters1.Grid.ContextMenuStrip = contextMenuViews;
				ActiveControl = schedulerSplitters1.Grid;
			}

			if (_currentZoomLevel == ZoomLevel.RequestView)
				reloadRequestView();

			foreach (ToolStripItem item in toolStripPanelItemViews2.Items)
			{
				var t = item as ToolStripButton;
				if (t?.Tag != null)
					t.Checked = (ZoomLevel)t.Tag == level;
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
				schedulerSplitters1.Grid.Model.Selections.Clear(true);
				schedulerSplitters1.Grid.Model.Selections.SelectRange(
					GridRangeInfo.Cell(schedulerSplitters1.Grid.CurrentCell.RowIndex, schedulerSplitters1.Grid.CurrentCell.ColIndex), true);

				_scheduleView?.SetSelectionFromParts(scheduleParts);
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

		private void requestViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			enableSave();
		}

		[RemoveMeWithToggle("function + flowLayoutExportToScenario from designer", Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576)]
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

			if (_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576))
			{
				var exportLimitedTime = new Label
				{
					Text = Resources.ExportAvailableLimitedTime,
					Width = 300,
					Height = 80,
					BackColor = Color.Green,
					ForeColor = Color.White,
					TextAlign = ContentAlignment.MiddleCenter
				};
				exportLimitedTime.Font.ChangeToBold();
				flowLayoutExportToScenario.ContainerControl.Controls.Add(exportLimitedTime);
			}

			backStageButtonManiMenuImport.Visible = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576);
			backStageButtonMainMenuCopy.Visible = _container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576);

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

		[RemoveMeWithToggle(Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576)]
		private void menuItemClick(object sender, EventArgs e)
		{
			var buttonAdv = sender as ButtonAdv;
			var toolStripMenuItem = sender as ToolStripMenuItem;
			IScenario scenario = null;

			if (buttonAdv != null) scenario = (IScenario)(buttonAdv).Tag;
			if (toolStripMenuItem != null) scenario = (IScenario)(toolStripMenuItem).Tag;

			if (scenario == null) return;

			var allNewRules = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();
			var selectedSchedules = _scheduleView.SelectedSchedules();
			var uowFactory = UnitOfWorkFactory.Current;
			var currentAuthorization = CurrentAuthorization.Make();
			var scheduleRepository = new ScheduleStorage(new FromFactory(() => uowFactory), new RepositoryFactory(),
				new PersistableScheduleDataPermissionChecker(currentAuthorization),
				_container.Resolve<IScheduleStorageRepositoryWrapper>(), currentAuthorization);
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
					SchedulerState.SchedulerStateHolder.SchedulingResultState.AllPersonAccounts,
					_scheduleView.AllSelectedDates(selectedSchedules)))
			{
				exportForm.ShowDialog(this);
			}
		}

		private void loadLockMenues()
		{
			if (_scheduleView == null) return;

			var lockAbsencesMenuBuilder = new LockAbsencesMenuBuilder();
			lockAbsencesMenuBuilder.Build(SchedulerState.SchedulerStateHolder.CommonStateHolder.Absences, toolStripMenuItemLockAbsenceDaysClick,
				toolStripMenuItemLockAbsenceDaysMouseUp, toolStripMenuItemLockAbsence,
				toolStripMenuItemLockAbsencesRM, toolStripMenuItemLockAbsencesClick,
				toolStripMenuItemAbsenceLockRmMouseUp);

			var lockDaysOffMenuBuilder = new LockDaysOffMenuBuilder();
			lockDaysOffMenuBuilder.Build(SchedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs, toolStripMenuItemLockFreeDaysClick,
				toolStripMenuItemLockSpecificDayOffClick, toolStripMenuItemDayOffLockRmMouseUp,
				toolStripMenuItemLockDayOff, toolStripMenuItemLockFreeDaysRM);

			var lockShiftCategoriesMenuBuilder = new LockShiftCategoriesMenuBuilder();
			lockShiftCategoriesMenuBuilder.Build(SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories,
				toolStripMenuItemLockShiftCategoryDaysClick,
				toolStripMenuItemLockShiftCategoryDaysMouseUp,
				toolStripMenuItemLockShiftCategory, toolStripMenuItemLockShiftCategoriesRM,
				toolStripMenuItemLockShiftCategoriesClick,
				toolStripMenuItemLockShiftCategoriesMouseUp);

			var tagsMenuLoader = new TagsMenuLoader(toolStripMenuItemLockTags, toolStripMenuItemLockTagsRM,
				SchedulerState.ScheduleTags, toolStripMenuItemLockTag,
				toolStripSplitButtonChangeTag, toolStripMenuItemChangeTag,
				toolStripComboBoxAutoTag, _defaultScheduleTag, toolStripMenuItemChangeTagRM);
			tagsMenuLoader.LoadTags();
		}

		private void enableSwapButtons(IList<IScheduleDay> selectedSchedules)
		{
			SchedulerRibbonHelper.EnableSwapButtons(selectedSchedules, _scheduleView, toolStripMenuItemSwap,
				toolStripMenuItemSwapAndReschedule,
				ToolStripMenuItemSwapRaw, toolStripDropDownButtonSwap, _permissionHelper, _teamLeaderMode,
				_temporarySelectedEntitiesFromTreeView, schedulerSplitters1.Grid);
		}

		private void updateSelectionInfo(IList<IScheduleDay> selectedSchedules)
		{
			var updater = new UpdateSelectionForAgentInfo(toolStripStatusLabelContractTime, toolStripStatusLabelScheduleTag);
			updater.Update(selectedSchedules, _scheduleView, SchedulerState.SchedulerStateHolder, _agentInfoControl, _scheduleTimeType,
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
				var skill = (ISkill)tab.Tag;
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
						_skillResultHighlightGridControl.DrawGridContents(SchedulerState.SchedulerStateHolder, skill);
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
					selectedSkillGridControl.DrawDayGrid(SchedulerState.SchedulerStateHolder, skill);
					selectedSkillGridControl.DrawDayGrid(SchedulerState.SchedulerStateHolder, skill);
					return;
				}

				positionControl(skillGridControl);
				ActiveControl = skillGridControl;
				selectedSkillGridControl.DrawDayGrid(SchedulerState.SchedulerStateHolder, skill);
				selectedSkillGridControl.DrawDayGrid(SchedulerState.SchedulerStateHolder, skill);
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
			var skill = (ISkill)tab.Tag;
			IAggregateSkill aggregateSkillSkill = skill;
			if (!aggregateSkillSkill.IsVirtual)
				return;

			var skillGridControl = resolveControlFromSkillResultViewSetting();
			if (skillGridControl is SkillIntradayGridControl)
			{
				var skillStaffPeriods = SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					aggregateSkillSkill,
					TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
						_currentIntraDayDate.AddDays(1).Date, SchedulerState.SchedulerStateHolder.TimeZoneInfo));
				if (_skillIntradayGridControl.Presenter.RowManager != null)
					_skillIntradayGridControl.Presenter.RowManager.SetDataSource(skillStaffPeriods);
			}
			else
			{
				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return;

				selectedSkillGridControl.SetDataSource(SchedulerState.SchedulerStateHolder, skill);
			}

			skillGridControl.Refresh();
		}

		private void drawIntraday(ISkill skill, IAggregateSkill aggregateSkillSkill)
		{
			IList<ISkillStaffPeriod> skillStaffPeriods;
			var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
				_currentIntraDayDate.AddDays(1).Date, SchedulerState.SchedulerStateHolder.TimeZoneInfo);
			if (aggregateSkillSkill.IsVirtual)
			{
				SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, periodToFind);
				skillStaffPeriods =
					SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, periodToFind);
			}
			else
			{
				skillStaffPeriods =
					SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill },
						periodToFind);
			}
			if (skillStaffPeriods.Count >= 0)
			{
				_chartDescription = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", skill.Name,
					_currentIntraDayDate.ToShortDateString());
				_skillIntradayGridControl.SetupDataSource(skillStaffPeriods, skill, SchedulerState.SchedulerStateHolder);
				_skillIntradayGridControl.SetRowsAndCols();
				positionControl(_skillIntradayGridControl);
			}
		}

		private void loadBpos(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			stateHolder.SchedulerStateHolder.SchedulingResultState.ExternalStaff = _container.Resolve<ExternalStaffProvider>().Fetch(stateHolder.SchedulerStateHolder.SchedulingResultState.Skills, stateHolder.SchedulerStateHolder.RequestedPeriod.Period());
		}

		private void loadSkillDays(IUnitOfWork uow, SchedulingScreenState stateHolder)
		{
			if (_teamLeaderMode) return;
			using (PerformanceOutput.ForOperation("Loading skill days"))
			{
				stateHolder.SchedulerStateHolder.SchedulingResultState.SkillDays = _container.Resolve<ISkillDayLoadHelper>().LoadSchedulerSkillDays(
					new DateOnlyPeriod(stateHolder.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8),
						stateHolder.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8)), stateHolder.SchedulerStateHolder.SchedulingResultState.Skills,
					stateHolder.SchedulerStateHolder.RequestedScenario);

				_container.Resolve<InitMaxSeatForStateHolder>().Execute(stateHolder.SchedulerStateHolder.SchedulingResultState.MinimumSkillIntervalLength());

				IList<ISkillStaffPeriod> skillStaffPeriods =
				stateHolder.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					stateHolder.SchedulerStateHolder.SchedulingResultState.Skills, stateHolder.SchedulerStateHolder.Schedules.Period.LoadedPeriod());

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
			if (SchedulerState.SchedulerStateHolder.Schedules == null)
				return 0;

			if (!SchedulerState.SchedulerStateHolder.Schedules.DifferenceSinceSnapshot().IsEmpty() || SchedulerState.ChangedRequests() ||
				!_modifiedWriteProtections.IsEmpty() || !SchedulerState.ModifiedWorkflowControlSets.IsEmpty())
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
					schedulerSplitters1.Grid.CurrentCell.MoveTo(point.Y, point.X, GridSetCurrentCellOptions.None);
					schedulerSplitters1.Grid.Selections.SelectRange(GridRangeInfo.Cell(point.Y, point.X), true);
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
			schedulerSplitters1.ChartControlSkillData.Visible = true;
		}

		private void schedulerSplitters1RestrictionsNotAbleToBeScheduledProgress(object sender,
			ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage == 0)
			{
				disableAllExceptCancelInRibbon();
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate((string)e.UserState) + @" " + e.ProgressPercentage + @" %";
				toolStripStatusLabelStatus.Owner.Refresh();
			}

			if (e.ProgressPercentage > 0 && e.ProgressPercentage < 100)
			{
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate((string)e.UserState) + @" " + e.ProgressPercentage + @" %";
				toolStripStatusLabelStatus.Owner.Refresh();
			}

			if (e.ProgressPercentage == 100)
			{
				enableAllExceptCancelInRibbon();
				toolStripStatusLabelStatus.Text = LanguageResourceHelper.Translate("XXReady");
				toolStripStatusLabelStatus.Owner.Refresh();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void setEventHandlers()
		{
			schedulerSplitters1.ValidationAlertsAgentDoubleClick += schedulerSplitters1ValidationAlertsAgentDoubleClick;
			schedulerSplitters1.RestrictionsNotAbleToBeScheduledProgress += schedulerSplitters1RestrictionsNotAbleToBeScheduledProgress;
			_schedulerMeetingHelper.ModificationOccured += schedulerMeetingHelperModificationOccured;
			_tmpTimer.Tick += tmpTimerTick;
			schedulerSplitters1.TabSkillData.SelectedIndexChanged += tabSkillDataSelectedIndexChanged;
			schedulerSplitters1.Grid.CurrentCellKeyDown += gridCurrentCellKeyDown;
			schedulerSplitters1.Grid.GotFocus += gridGotFocus;
			schedulerSplitters1.Grid.SelectionChanged += gridSelectionChanged;
			schedulerSplitters1.Grid.ScrollControlMouseUp += gridScrollControlMouseUp;
			schedulerSplitters1.Grid.StartAutoScrolling += gridStartAutoScrolling;

			wpfShiftEditor1.ShiftUpdated += wpfShiftEditor1ShiftUpdated;
			wpfShiftEditor1.CommitChanges += wpfShiftEditor1CommitChanges;
			wpfShiftEditor1.EditMeeting += wpfShiftEditor1EditMeeting;
			wpfShiftEditor1.RemoveParticipant += wpfShiftEditor1RemoveParticipant;
			wpfShiftEditor1.DeleteMeeting += wpfShiftEditor1DeleteMeeting;
			wpfShiftEditor1.CreateMeeting += wpfShiftEditor1CreateMeeting;
			wpfShiftEditor1.AddAbsence += wpfShiftEditorAddAbsence;
			wpfShiftEditor1.AddActivity += wpfShiftEditorAddActivity;
			wpfShiftEditor1.AddOvertime += wpfShiftEditorAddOvertime;
			wpfShiftEditor1.AddPersonalShift += wpfShiftEditorAddPersonalShift;
			wpfShiftEditor1.Undo += wpfShiftEditorUndo;
			wpfShiftEditor1.ShowLayers += wpfShiftEditor1ShowLayers;

			notesEditor.NotesChanged += notesEditorNotesChanged;
			notesEditor.PublicNotesChanged += notesEditorPublicNotesChanged;

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
			_skillResultHighlightGridControl.GoToDate += skillResultHighlightGridControlGoToDate;

			_gridrowInChartSettingButtons.LineInChartSettingsChanged += gridlinesInChartSettingsLineInChartSettingsChanged;
			_gridrowInChartSettingButtons.LineInChartEnabledChanged += gridrowInChartSettingLineInChartEnabledChanged;
			schedulerSplitters1.ChartControlSkillData.ChartRegionMouseHover += chartControlSkillDataChartRegionMouseHover;
			schedulerSplitters1.ChartControlSkillData.ChartRegionClick += chartControlSkillDataChartRegionClick;
			_undoRedo.ChangedHandler += undoRedoChanged;

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

		private void skillResultHighlightGridControlGoToDate(object sender, GoToDateEventArgs e)
		{
			_scheduleView.SetSelectedDateLocal(e.Date);
		}

		private void gridStartAutoScrolling(object sender, StartAutoScrollingEventArgs e)
		{
			if (e.Reason == AutoScrollReason.MouseDragging)
				schedulerSplitters1.Grid.SupportsPrepareViewStyleInfo = false;
		}

		private void gridScrollControlMouseUp(object sender, CancelMouseEventArgs e)
		{
			schedulerSplitters1.Grid.SupportsPrepareViewStyleInfo = true;
			schedulerSplitters1.Grid.Invalidate();
		}

		private void replyAndDenyRequestFromRequestDetailsView(EventParameters<ReplyAndDenyRequestFromRequestDetailsView> obj)
		{
			var denyCommand = new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker, SchedulerState.SchedulerStateHolder.RequestedScenario, this);
			IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>() { obj.Value.Request };
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList, denyCommand))
			{
				dialog.ShowDialog();
			}
			recalculateResourcesForRequests(selectedRequestList);
		}

		private void replyAndApproveRequestFromRequestDetailsView(
			EventParameters<ReplyAndApproveRequestFromRequestDetailsView> obj)
		{

			var businessRules = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var personRequestRepository = new PersonRequestRepository(uow);
				var approveRequestCommand = new ApprovePersonRequestCommand(this, SchedulerState.SchedulerStateHolder.Schedules,
					SchedulerState.SchedulerStateHolder.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse, _personRequestAuthorizationChecker, businessRules,
					_overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository, personRequestRepository);

				IList<PersonRequestViewModel> selectedRequestList = new List<PersonRequestViewModel>() { obj.Value.Request };
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

			changeRequestStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker, SchedulerState.SchedulerStateHolder.RequestedScenario, this),
				selectedRequestList);

		}

		private void approveRequestFromRequestDetailsView(
			EventParameters<ApproveRequestFromRequestDetailsView> eventParameters)
		{
			var allNewBusinessRules = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var personRequestRepository = new PersonRequestRepository(uow);
				var approvePersonRequestCommand = new ApprovePersonRequestCommand(this, SchedulerState.SchedulerStateHolder.Schedules,
					SchedulerState.SchedulerStateHolder.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse,
					_personRequestAuthorizationChecker, allNewBusinessRules, _overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository, personRequestRepository);

				var selectedAdapters = new List<PersonRequestViewModel>() { eventParameters.Value.Request };

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
			toolStripTabItem1.Click -= toolStripTabItem1Click;
			schedulerSplitters1.ValidationAlertsAgentDoubleClick -= schedulerSplitters1ValidationAlertsAgentDoubleClick;
			schedulerSplitters1.RestrictionsNotAbleToBeScheduledProgress -= schedulerSplitters1RestrictionsNotAbleToBeScheduledProgress;
			toolStripButtonRequestBack.Click -= toolStripButtonRequestBackClick;
			toolStripButtonFilterAgentsRequestView.Click -= toolStripButtonFilterAgentsClick;
			ToolStripMenuItemViewDetails.Click -= toolStripMenuItemViewDetailsClick;
			toolStripButtonViewAllowance.Click -= toolStripItemViewAllowanceClick;
			toolStripButtonViewRequestHistory.Click -= toolStripViewRequestHistoryClick;
			toolStripButtonApproveRequest.Click -= toolStripButtonApproveRequestClick;
			toolStripButtonDenyRequest.Click -= toolStripButtonDenyRequestClick;
			toolStripButtonEditNote.Click -= toolStripButtonEditNoteClick;
			toolStripButtonReplyAndApprove.Click -= toolStripButtonReplyAndApproveClick;
			toolStripButtonReplyAndDeny.Click -= toolStripButtonReplyAndDenyClick;

			//Chart tab
			toolStripTabItemChart.Click -= toolStripTabItemChartClick;
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
			toolStripTabItemHome.Click -= toolStripTabItemHomeClick;
			toolStripTabItemChart.Click -= toolStripTabItemChartClick;
			toolStripTabItem1.Click -= toolStripTabItem1Click;
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
			backStageButtonOptions.Click -= toolStripButtonOptionsClick;
			backStageButtonSystemExit.Click -= toolStripButtonSystemExitClick;
			backStageButtonManiMenuImport.Click -= backStageButtonManiMenuImportClick;
			backStageButtonMainMenuCopy.Click -= backStageButtonMainMenuCopyClick;
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
			toolStripButtonQuickAccessRedo.MouseUp -= toolStripButtonQuickAccessRedoClick1;
			toolStripMenuItemQuickAccessUndo.Click -= toolStripSplitButtonQuickAccessUndoButtonClick;
			toolStripMenuItemQuickAccessUndoAll.Click -= toolStripMenuItemQuickAccessUndoAllClick1;
			toolStripButtonShowTexts.Click -= toolStripButtonShowTextsClick;
			toolStripButtonShowResult.Click -= toolStripButtonShowResultClick;
			toolStripButtonShowGraph.Click -= toolStripButtonShowGraphClick;
			toolStripButtonShowEditor.Click -= toolStripButtonShowEditorClick;

			toolStripButtonDayView.Click -= toolStripButtonZoomClick;
			toolStripButtonWeekView.Click -= toolStripButtonZoomClick;
			toolStripButtonPeriodView.Click -= toolStripButtonZoomClick;
			toolStripButtonSummaryView.Click -= toolStripButtonZoomClick;
			toolStripButtonRequestView.Click -= toolStripButtonZoomClick;
			toolStripButtonRestrictions.Click -= toolStripButtonZoomClick;
			toolStripButtonShowPropertyPanel.Click -= toolStripButtonShowPropertyPanelClick;

			toolStripSplitButtonUnlock.ButtonClick -= toolStripSplitButtonUnlockButtonClick;
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
				_backgroundWorkerOptimization.RunWorkerCompleted -= backgroundWorkerOptimizationRunWorkerCompleted;
			}

			if (toolStripComboBoxAutoTag != null)
			{
				toolStripComboBoxAutoTag.SelectedIndexChanged -= toolStripComboBoxAutoTagSelectedIndexChanged;
			}

			if (SchedulerState?.SchedulerStateHolder.Schedules != null)
			{
				SchedulerState.SchedulerStateHolder.Schedules.PartModified -= schedulesPartModified;
			}

			if (_schedulerMeetingHelper != null)
			{
				_schedulerMeetingHelper.ModificationOccured -= schedulerMeetingHelperModificationOccured;
			}
			if (schedulerSplitters1 != null)
			{
				schedulerSplitters1.HandlePersonRequestView1.RemoveEvents();
				schedulerSplitters1.TabSkillData.SelectedIndexChanged -= tabSkillDataSelectedIndexChanged;
			}
			if (schedulerSplitters1.Grid != null)
			{
				schedulerSplitters1.Grid.CurrentCellKeyDown -= gridCurrentCellKeyDown;
				schedulerSplitters1.Grid.GotFocus -= gridGotFocus;
				schedulerSplitters1.Grid.SelectionChanged -= gridSelectionChanged;
				schedulerSplitters1.Grid.StartAutoScrolling -= gridStartAutoScrolling;
				schedulerSplitters1.Grid.ScrollControlMouseUp -= gridScrollControlMouseUp;
			}

			if (wpfShiftEditor1 != null)
			{
				wpfShiftEditor1.ShiftUpdated -= wpfShiftEditor1ShiftUpdated;
				wpfShiftEditor1.CommitChanges -= wpfShiftEditor1CommitChanges;
				wpfShiftEditor1.EditMeeting -= wpfShiftEditor1EditMeeting;
				wpfShiftEditor1.RemoveParticipant -= wpfShiftEditor1RemoveParticipant;
				wpfShiftEditor1.DeleteMeeting -= wpfShiftEditor1DeleteMeeting;
				wpfShiftEditor1.CreateMeeting -= wpfShiftEditor1CreateMeeting;

				wpfShiftEditor1.AddAbsence -= wpfShiftEditorAddAbsence;
				wpfShiftEditor1.AddActivity -= wpfShiftEditorAddActivity;
				wpfShiftEditor1.AddOvertime -= wpfShiftEditorAddOvertime;
				wpfShiftEditor1.AddPersonalShift -= wpfShiftEditorAddPersonalShift;
				wpfShiftEditor1.ShowLayers -= wpfShiftEditor1ShowLayers;
				wpfShiftEditor1.Undo -= wpfShiftEditorUndo;
			}

			toolStripMenuItemStartAsc.MouseUp -= toolStripMenuItemStartAscMouseUp;
			toolStripMenuItemStartTimeDesc.MouseUp -= toolStripMenuItemStartTimeDescMouseUp;
			toolStripMenuItemEndTimeAsc.MouseUp -= toolStripMenuItemEndTimeAscMouseUp;
			toolStripMenuItemEndTimeDesc.MouseUp -= toolStripMenuItemEndTimeDescMouseUp;
			toolStripMenuItemContractTimeAsc.MouseUp -= toolStripMenuItemContractTimeAscMouseUp;
			toolStripMenuItemContractTimeDesc.MouseUp -= toolStripMenuItemContractTimeDescMouseUp;
			toolStripMenuItemSeniorityRankAsc.MouseUp -= toolStripMenuItemSeniorityRankAscMouseUp;
			toolStripMenuItemSeniorityRankDesc.MouseUp -= toolStripMenuItemSeniorityRankDescMouseUp;
			toolStripMenuItemUnlockSelectionRM.MouseUp -= toolStripMenuItemUnlockSelectionRmMouseUp;
			toolStripMenuItemUnlockAllRM.MouseUp -= toolStripMenuItemUnlockAllRmMouseUp;
			toolStripMenuItemLockSelectionRM.MouseUp -= toolStripMenuItemLockSelectionRmMouseUp;
			ToolStripMenuItemAllRM.MouseUp -= toolStripMenuItemLockAllRestrictionsMouseUp;
			ToolStripMenuItemAllPreferencesRM.MouseUp -= toolStripMenuItemAllPreferencesMouseUp;
			ToolStripMenuItemAllAbsencePreferenceRM.MouseUp -= toolStripMenuItemAllAbsencePreferenceMouseUp;
			ToolStripMenuItemAllDaysOffPreferencesRM.MouseUp -= toolStripMenuItemAllDaysOffMouseUp;
			ToolStripMenuItemAllShiftsPreferencesRM.MouseUp -= toolStripMenuItemAllShiftsPreferencesMouseUp;
			ToolStripMenuItemAllMustHaveRM.MouseUp -= toolStripMenuItemAllMustHaveMouseUp;
			ToolStripMenuItemAllFulFilledPreferencesRM.MouseUp -= toolStripMenuItemAllFulFilledPreferencesMouseUp;
			ToolStripMenuItemAllFulFilledAbsencesPreferencesRM.MouseUp -= toolStripMenuItemAllFulFilledAbsencesPreferencesMouseUp;
			ToolStripMenuItemAllFulFilledDaysOffPreferencesRM.MouseUp -= toolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp;
			ToolStripMenuItemAllFulFilledShiftsPreferencesRM.MouseUp -= toolStripMenuItemAllFulFilledShiftsPreferencesMouseUp;
			ToolStripMenuItemAllFulfilledMustHaveRM.MouseUp -= toolStripMenuItemAllFulfilledMustHaveMouseUp;
			ToolStripMenuItemAllRotationsRM.MouseUp -= toolStripMenuItemAllRotationsMouseUp;
			ToolStripMenuItemAllDaysOffRotationsRM.MouseUp -= toolStripMenuItemAllDaysOffRotationsMouseUp;
			ToolStripMenuItemAllShiftsRotationsRM.MouseUp -= toolStripMenuItemAllShiftsRotationsMouseUp;
			ToolStripMenuItemAllFulFilledRotationsRM.MouseUp -= toolStripMenuItemAllFulFilledRotationsMouseUp;
			ToolStripMenuItemAllFulFilledDaysOffRotationsRM.MouseUp -= toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			ToolStripMenuItemAllFulFilledShiftsRotationsRM.MouseUp -= toolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			ToolStripMenuItemAllUnavailableStudentAvailabilityRM.MouseUp -= toolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			ToolStripMenuItemAllAvailableStudentAvailabilityRM.MouseUp -= toolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			ToolStripMenuItemAllFulFilledStudentAvailabilityRM.MouseUp -= toolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			ToolStripMenuItemAllUnAvailableAvailabilityRM.MouseUp -= toolStripMenuItemAllUnavailableAvailabilityMouseUp;
			ToolStripMenuItemAllAvailableAvailabilityRM.MouseUp -= toolStripMenuItemAllAvailableAvailabilityMouseUp;
			ToolStripMenuItemAllFulFilledAvailabilityRM.MouseUp -= toolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAllTagsRM.MouseUp -= toolStripMenuItemLockAllTagsMouseUp;
			toolStripMenuItemWriteProtectSchedule.MouseUp -= toolStripMenuItemWriteProtectScheduleMouseUp;
			toolstripMenuRemoveWriteProtection.MouseUp -= toolstripMenuRemoveWriteProtectionMouseUp;
			ToolStripMenuItemCreateMeeting.MouseUp -= toolStripMenuItemCreateMeetingMouseUp;
			toolStripMenuItemEditMeeting.MouseUp -= toolStripMenuItemEditMeetingMouseUp;
			toolStripMenuItemRemoveParticipant.MouseUp -= toolStripMenuItemRemoveParticipantMouseUp;
			toolStripMenuItemDeleteMeeting.MouseUp -= toolStripMenuItemDeleteMeetingMouseUp;
			toolStripMenuItemScheduledTimePerActivity.MouseUp -= toolStripMenuItemScheduledTimePerActivityMouseUp;
			toolStripMenuItemExportToPDF.MouseUp -= toolStripMenuItemExportToPdfMouseUp;
			toolStripMenuItemExportToPDFGraphical.MouseUp -= toolStripMenuItemExportToPdfGraphicalMouseUp;
			ToolStripMenuItemExportToPDFShiftsPerDay.MouseUp -= toolStripMenuItemExportToPdfShiftsPerDayMouseUp;
			ToolStripMenuItemLockAllRestrictions.MouseUp -= toolStripMenuItemLockAllRestrictionsMouseUp;
			ToolStripMenuItemAllPreferences.MouseUp -= toolStripMenuItemAllPreferencesMouseUp;
			ToolStripMenuItemAllAbsencePreference.MouseUp -= toolStripMenuItemAllAbsencePreferenceMouseUp;
			ToolStripMenuItemAllDaysOff.MouseUp -= toolStripMenuItemAllDaysOffMouseUp;
			ToolStripMenuItemAllShiftsPreferences.MouseUp -= toolStripMenuItemAllShiftsPreferencesMouseUp;
			ToolStripMenuItemAllMustHave.MouseUp -= toolStripMenuItemAllMustHaveMouseUp;
			ToolStripMenuItemAllFulFilledPreferences.MouseUp -= toolStripMenuItemAllFulFilledPreferencesMouseUp;
			ToolStripMenuItemAllFulFilledDaysOffPreferences.MouseUp -= toolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp;
			ToolStripMenuItemAllFulFilledShiftsPreferences.MouseUp -= toolStripMenuItemAllFulFilledShiftsPreferencesMouseUp;
			ToolStripMenuItemAllFulfilledMustHave.MouseUp -= toolStripMenuItemAllFulfilledMustHaveMouseUp;
			ToolStripMenuItemAllRotations.MouseUp -= toolStripMenuItemAllRotationsMouseUp;
			ToolStripMenuItemAllDaysOffRotations.MouseUp -= toolStripMenuItemAllDaysOffRotationsMouseUp;
			ToolStripMenuItemAllFulFilledRotations.MouseUp -= toolStripMenuItemAllFulFilledRotationsMouseUp;
			ToolStripMenuItemAllFulFilledDaysOffRotations.MouseUp -= toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			ToolStripMenuItemAllFulFilledShiftsRotations.MouseUp -= toolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			ToolStripMenuItemAllShiftsRotations.MouseUp -= toolStripMenuItemAllShiftsRotationsMouseUp;
			ToolStripMenuItemAllUnavailableStudentAvailability.MouseUp -= toolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			ToolStripMenuItemAllAvailableStudentAvailability.MouseUp -= toolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			ToolStripMenuItemAllFulFilledStudentAvailability.MouseUp -= toolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			ToolStripMenuItemAllUnavailableAvailability.MouseUp -= toolStripMenuItemAllUnavailableAvailabilityMouseUp;
			ToolStripMenuItemAllAvailableAvailability.MouseUp -= toolStripMenuItemAllAvailableAvailabilityMouseUp;
			ToolStripMenuItemAllFulFilledAvailability.MouseUp -= toolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAllTags.MouseUp -= toolStripMenuItemLockAllTagsMouseUp;
			ToolStripMenuItemRemoveWriteProtectionToolBar.MouseUp -= toolstripMenuRemoveWriteProtectionMouseUp;
			toolStripButtonQuickAccessRedo.MouseUp -= toolStripButtonQuickAccessRedoClick1;

			toolStripButtonShrinkage.Click -= toolStripButtonShrinkageClick;
			toolStripButtonCalculation.Click -= toolStripButtonCalculationClick;
			toolStripButtonValidation.Click -= toolStripButtonValidationClick;

			if (notesEditor != null)
			{
				notesEditor.NotesChanged -= notesEditorNotesChanged;
				notesEditor.PublicNotesChanged -= notesEditorPublicNotesChanged;
			}
			if (_requestView != null)
			{
				_requestView.PropertyChanged -= requestViewPropertyChanged;
				_requestView.SelectionChanged -= requestViewSelectionChanged;
			}

			if (_skillResultHighlightGridControl != null)
				_skillResultHighlightGridControl.GoToDate -= skillResultHighlightGridControlGoToDate;

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

			if (schedulerSplitters1 != null && schedulerSplitters1.ChartControlSkillData != null)
			{
				schedulerSplitters1.ChartControlSkillData.ChartRegionMouseHover -= chartControlSkillDataChartRegionMouseHover;
				schedulerSplitters1.ChartControlSkillData.ChartRegionClick -= chartControlSkillDataChartRegionClick;
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

			if (_undoRedo != null) _undoRedo.ChangedHandler -= undoRedoChanged;

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

		private void wpfShiftEditor1DeleteMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			_schedulerMeetingHelper.MeetingRemove(e.Value.BelongsToMeeting, _scheduleView);
		}

		private void wpfShiftEditor1RemoveParticipant(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			IList<IPersonMeeting> meetings = new List<IPersonMeeting> { e.Value };
			_schedulerMeetingHelper.MeetingRemoveAttendees(meetings);
		}

		private void wpfShiftEditor1EditMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool editPermission = _permissionHelper.IsPermittedToEditMeeting(_scheduleView,
				_temporarySelectedEntitiesFromTreeView, _scenario);
			bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
			_schedulerMeetingHelper.MeetingComposerStart(e.Value.BelongsToMeeting, _scheduleView, editPermission,
				viewSchedulesPermission, _container.Resolve<IToggleManager>());
		}

		private void wpfShiftEditor1CreateMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool viewSchedulesPermission = _permissionHelper.IsPermittedToViewSchedules(_temporarySelectedEntitiesFromTreeView);
			_schedulerMeetingHelper.MeetingComposerStart(null, _scheduleView, true, viewSchedulesPermission,
				_container.Resolve<IToggleManager>());
		}

		private void notesEditorNotesChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.LastUnsavedSchedulePart = notesEditor.SchedulePart;
				_scheduleView.Presenter.UpdateNoteFromEditor();
			}
			enableSave();
		}

		private void notesEditorPublicNotesChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.LastUnsavedSchedulePart = notesEditor.SchedulePart;
				_scheduleView.Presenter.UpdatePublicNoteFromEditor();
			}
			enableSave();
		}

		private void undoRedoChanged(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(undoRedoChanged), sender, e);
			}
			else
				enableUndoRedoButtons();
		}

		private void wpfShiftEditorAddActivity(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;

			IActivity selectedActivity = null;
			if (e.SelectedLayer != null)
				selectedActivity = e.SelectedLayer.Payload as IActivity;

			_scheduleView.Presenter.AddActivity(new List<IScheduleDay> { e.SchedulePart }, e.Period, selectedActivity);
			RecalculateResources();
		}

		private void wpfShiftEditorAddPersonalShift(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddPersonalShift(new List<IScheduleDay> { e.SchedulePart }, e.Period);
			RecalculateResources();
		}

		private void wpfShiftEditorAddOvertime(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddOvertime(new List<IScheduleDay> { e.SchedulePart }, e.Period,
				MultiplicatorDefinitionSet.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime).ToList());
			RecalculateResources();
		}

		private void wpfShiftEditorAddAbsence(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddAbsence(new List<IScheduleDay> { e.SchedulePart }, e.Period);
			RecalculateResources();
		}

		private void wpfShiftEditorUndo(object sender, EventArgs e)
		{
			undoKeyDown();
		}

		private void setColor()
		{
			schedulerSplitters1.Grid.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
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
				var paramsList = new object[] { button, enable };
				BeginInvoke(new ToggleQuickButtonEnabledState(toggleQuickButtonEnabledState), paramsList);
				return;
			}

			foreach (var quickItem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripButton>())
			{
				if (((QuickButtonReflectable)quickItem).ReflectedButton.Name == button.Name)
				{
					quickItem.Enabled = enable;
					return;
				}
			}

			foreach (var quickItem in ribbonControlAdv1.Header.QuickItems.OfType<ToolStripSplitButton>())
			{
				if (((QuickSplitButtonReflectable)quickItem).ReflectedSplitButton.Name == button.Name)
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
			var allNewBusinessRules = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var personRequestRepository = new PersonRequestRepository(uow);
				changeRequestStatus(
					new ApprovePersonRequestCommand(this, SchedulerState.SchedulerStateHolder.Schedules, SchedulerState.SchedulerStateHolder.RequestedScenario,
						_requestPresenter, _handleBusinessRuleResponse,
						_personRequestAuthorizationChecker, allNewBusinessRules, _overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
						globalSettingRepository, personAbsenceAccountRepository, personRequestRepository), _requestView.SelectedAdapters());
			}

			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();
		}

		private void toolStripButtonDenyRequestClick(object sender, EventArgs e)
		{
			changeRequestStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker, SchedulerState.SchedulerStateHolder.RequestedScenario, this),
				_requestView.SelectedAdapters());
		}

		private void toolStripButtonEditNoteClick(object sender, EventArgs e)
		{
			IList<PersonRequestViewModel> selectedRequestList = _requestView.SelectedAdapters();
			using (var dialog = new RequestReplyStatusChangeDialog(_requestPresenter, selectedRequestList))
			{
				dialog.ShowDialog();
			}
		}

		private void toolStripButtonReplyAndApproveClick(object sender, EventArgs e)
		{
			var businessRules = SchedulerState.SchedulerStateHolder.SchedulingResultState.GetRulesToRun();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var globalSettingRepository = new GlobalSettingDataRepository(uow);
				var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
				var personRequestRepository = new PersonRequestRepository(uow);
				replyAndChangeStatus(new ApprovePersonRequestCommand(this, SchedulerState.SchedulerStateHolder.Schedules,
					SchedulerState.SchedulerStateHolder.RequestedScenario, _requestPresenter,
					_handleBusinessRuleResponse, _personRequestAuthorizationChecker, businessRules,
					_overriddenBusinessRulesHolder,
				_container.Resolve<IScheduleDayChangeCallback>(),
					globalSettingRepository, personAbsenceAccountRepository, personRequestRepository));
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
				days.ForEach(SchedulerState.SchedulerStateHolder.MarkDateToBeRecalculated);
			}
			RecalculateResources();
		}

		private void toolStripButtonReplyAndDenyClick(object sender, EventArgs e)
		{
			replyAndChangeStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker, SchedulerState.SchedulerStateHolder.RequestedScenario, this));
		}

		private void toolStripButtonOptionsClick(object sender, EventArgs e)
		{
			var toggleManager = _container.Resolve<IToggleManager>();

			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(toggleManager, _container.Resolve<IBusinessRuleConfigProvider>(), _container.Resolve<IConfigReader>())));
				settings.Show();
				settings.BringToFront();
			}
			catch (CouldNotCreateTransactionException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void toolStripMenuItemScheduleHourlyEmployeesClick(object sender, EventArgs e)
		{
			scheduleHourlyEmployees();
		}

		private void schedulingScreenFormClosed(object sender, FormClosedEventArgs e)
		{
			// bug 28705 hide it so we don't get strange paint events
			Hide();
			_mainWindow.Activate();
			SchedulerState?.SchedulerStateHolder.Schedules?.Clear();
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
			SchedulerState = null;
			_schedulerMeetingHelper = null;

			wpfShiftEditor1?.LoadSchedulePart(null);
			notesEditor?.LoadNote(null);
			wpfShiftEditor1?.Unload();
			wpfShiftEditor1 = null;
			_undoRedo?.Clear();

			notesEditor = null;

			if (schedulerSplitters1 != null && schedulerSplitters1.ElementHostRequests != null && schedulerSplitters1.ElementHostRequests.Child != null) schedulerSplitters1.ElementHostRequests.Child = null;
			if (schedulerSplitters1.Grid != null) schedulerSplitters1.Grid.ContextMenu = null;
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

		private void toolStripButtonQuickAccessRedoClick1(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Redo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void toolStripSplitButtonQuickAccessUndoButtonClick(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Undo();
			_backgroundWorkerRunning = false;
			undoRedoSelectAndRefresh();
		}

		private void toolStripMenuItemQuickAccessUndoAllClick1(object sender, EventArgs e)
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
				selectCellFromPersonDate(_lastModifiedPart.ModifiedPerson, _lastModifiedPart.ModifiedPart.DateOnlyAsPeriod.DateOnly);
			}

			RecalculateResources();
			if (_requestView != null)
				updateShiftEditor();
		}

		private void toolStripTabItem1Click(object sender, EventArgs e)
		{
			ActiveControl = schedulerSplitters1.ElementHostRequests;
		}

		private void toolStripTabItemChartClick(object sender, EventArgs e)
		{
			ActiveControl = schedulerSplitters1.ChartControlSkillData;
		}

		private void toolStripTabItemHomeClick(object sender, EventArgs e)
		{
			ActiveControl = null;
		}

		private void toolStripMenuItemUseShrinkageClick(object sender, EventArgs e)
		{
			bool useShrinkage = !((ToolStripMenuItem)_contextMenuSkillGrid.Items["UseShrinkage"]).Checked;
			toggleShrinkage(useShrinkage);
		}

		private void toolStripMenuItemWriteProtectSchedule2Click(object sender, EventArgs e)
		{
			writeProtectSchedule();
		}

		private void toolstripMenuRemoveWriteProtectionMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection)) return;
			Cursor = Cursors.WaitCursor;
			var removeCommand = new WriteProtectionRemoveCommand(_scheduleView.SelectedSchedules(), _modifiedWriteProtections);
			removeCommand.Execute();
			GridHelper.GridlockWriteProtected(SchedulerState.SchedulerStateHolder, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void writeProtectSchedule()
		{
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
				return;
			GridHelper.WriteProtectPersonSchedule(schedulerSplitters1.Grid).ForEach(_modifiedWriteProtections.Add);
			GridHelper.GridlockWriteProtected(SchedulerState.SchedulerStateHolder, LockManager);
			schedulerSplitters1.Grid.Refresh();
			enableSave();
		}

		private void setupAvailTimeZones()
		{
			TimeZoneGuard.Instance.TimeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
			SchedulerState.SchedulerStateHolder.TimeZoneInfo = TimeZoneGuard.Instance.TimeZone;
			wpfShiftEditor1.SetTimeZone(SchedulerState.SchedulerStateHolder.TimeZoneInfo);

			foreach (TimeZoneInfo info in _detectedTimeZoneInfos)
			{
				var timeZoneItem = new ToolStripMenuItem(info.DisplayName) { Tag = info };
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
			var exporter = new ExportToPdfGraphical(_scheduleView, this, SchedulerState,
				TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
				TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.TextInfo.IsRightToLeft);
			exporter.Export();
		}

		private void exportToPdf(bool shiftsPerDay)
		{
			var exporter = new ExportToPdf(_scheduleView, this, SchedulerState,
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

			var persistConflicts = conflicts as PersistConflict[] ?? conflicts.ToArray();
			if (persistConflicts.Any())
				showPersistConflictView(modifiedDataFromConflictResolution, persistConflicts);

			_undoRedo.Clear(); //see if this can be removed later... Should undo/redo work after refresh?
			foreach (var data in modifiedDataFromConflictResolution)
			{
				SchedulerState.SchedulerStateHolder.MarkDateToBeRecalculated(new DateOnly(data.Period.StartDateTimeLocal(SchedulerState.SchedulerStateHolder.TimeZoneInfo)));
				_personsToValidate.Add(data.Person);
			}
		}

		private void showPersistConflictView(List<IPersistableScheduleData> modifiedData,
			IEnumerable<PersistConflict> conflicts)
		{
			using (
				var conflictForm = new PersistConflictView(SchedulerState.SchedulerStateHolder.Schedules, conflicts, modifiedData,
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
				SchedulerState.SchedulerStateHolder.Schedules.ForEach(p => p.Value.ForceRecalculationOfTargetTimeContractTimeAndDaysOff());
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

		private void schedulingScreenKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
				wpfShiftEditor1.EnableMoveAllLayers(true);
		}

		private void schedulingScreenKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
				wpfShiftEditor1.EnableMoveAllLayers(false);
		}

		private SplitterManagerRestrictionView splitterManager
		{
			get
			{
				if (_splitterManager == null)
				{
					_splitterManager = new SplitterManagerRestrictionView
					{
						MainSplitter = schedulerSplitters1.SplitContainerAdvMainContainer,
						LeftMainSplitter = schedulerSplitters1.SplitContainerAdvMain,
						GraphResultSplitter = schedulerSplitters1.SplitContainerAdvResultGraph,
						GridEditorSplitter = schedulerSplitters1.SplitContainerLessIntelligent1,
						RestrictionViewSplitter = schedulerSplitters1.SplitContainerView
					};
				}
				return _splitterManager;
			}
		}

		private void toolStripButtonShowGraphClick(object sender, EventArgs e)
		{
			toolStripButtonShowGraph.Checked = !toolStripButtonShowGraph.Checked;
			splitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
			_showGraph = toolStripButtonShowGraph.Checked;
		}

		private void toolStripButtonShowResultClick(object sender, EventArgs e)
		{
			toolStripButtonShowResult.Checked = !toolStripButtonShowResult.Checked;
			splitterManager.ShowResult = toolStripButtonShowResult.Checked;
			_showResult = toolStripButtonShowResult.Checked;
		}

		private void toolStripButtonShowEditorClick(object sender, EventArgs e)
		{
			toolStripButtonShowEditor.Checked = !toolStripButtonShowEditor.Checked;
			splitterManager.ShowEditor = toolStripButtonShowEditor.Checked;
			_showEditor = toolStripButtonShowEditor.Checked;
			updateShiftEditor();
		}

		private DateTime _lastclickLabels;

		private void toolStripButtonShowTextsClick(object sender, EventArgs e)
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
				splitterManager.ShowResult = toolStripButtonShowResult.Checked;
				toolStripButtonShowGraph.Checked = false;
				splitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
				_splitterManager.ShowRestrictionView = true;
				toolStripButtonShowEditor.Checked = true;
				splitterManager.ShowEditor = true;
				toolStripMenuItemLock.Enabled = false;
				toolStripMenuItemUnlock.Enabled = false;
				toolStripSplitButtonLock.Enabled = false;
				toolStripSplitButtonUnlock.Enabled = false;
				toolStripMenuItemSortBy.Enabled = false;
			}
			else
			{
				toolStripButtonShowResult.Checked = _showResult;
				splitterManager.ShowResult = toolStripButtonShowResult.Checked;
				toolStripButtonShowGraph.Checked = _showGraph;
				splitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
				splitterManager.ShowEditor = _showEditor;
				toolStripButtonShowEditor.Checked = _showEditor;
				_splitterManager.ShowRestrictionView = false;
				toolStripMenuItemLock.Enabled = true;
				toolStripMenuItemUnlock.Enabled = true;
				toolStripSplitButtonLock.Enabled = true;
				toolStripSplitButtonUnlock.Enabled = true;
				toolStripMenuItemSortBy.Enabled = true;
				if (_teamLeaderMode)
				{
					splitterManager.ShowGraph = false;
					splitterManager.ShowResult = false;
				}
			}


		}

		private void toolStripButtonShrinkageClick(object sender, EventArgs e)
		{
			disableAllExceptCancelInRibbon();
			bool useShrinkage = !toolStripButtonShrinkage.Checked;
			toggleShrinkage(useShrinkage);
			enableAllExceptCancelInRibbon();
		}

		private void toolStripButtonValidationClick(object sender, EventArgs e)
		{
			if (_validation)
			{
				SchedulerState.SchedulerStateHolder.SchedulingResultState.UseValidation = false;
				validateAllPersons();
			}
			_validation = !toolStripButtonValidation.Checked;
			toolStripButtonValidation.Checked = _validation;
			SchedulerState.SchedulerStateHolder.SchedulingResultState.UseValidation = _validation;
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

		private void toolStripButtonCalculationClick(object sender, EventArgs e)
		{
			toggleCalculation();
		}

		private void toggleShrinkage(bool useShrinkage)
		{
			Cursor = Cursors.WaitCursor;
			IList<ISkillStaffPeriod> skillStaffPeriods =
				SchedulerState.SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills, SchedulerState.SchedulerStateHolder.Schedules.Period.LoadedPeriod());
			foreach (ISkillStaffPeriod period in skillStaffPeriods)
			{
				period.Payload.UseShrinkage = useShrinkage;
				SchedulerState.SchedulerStateHolder.MarkDateToBeRecalculated(new DateOnly(period.Period.StartDateTimeLocal(SchedulerState.SchedulerStateHolder.TimeZoneInfo)));
			}

			RecalculateResources();
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["UseShrinkage"]).Checked = useShrinkage;
			toolStripButtonShrinkage.Checked = useShrinkage;
			Cursor = Cursors.Default;

			refreshSummarySkillIfActive();
		}

		private void toolStripMenuItemScheduledTimePerActivityMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor.Current = Cursors.WaitCursor;
			if (_scheduleView.SelectedSchedules().Count > 0)
			{
				var reportDetail =
					ReportHandler.CreateReportDetail(
						ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
							DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport));
				ReportHandler.ShowReport(reportDetail, _scheduleView, SchedulerState.SchedulerStateHolder.RequestedScenario, CultureInfo.CurrentCulture);
			}
			Cursor.Current = Cursors.Default;
		}

		private void toolStripMenuItemSearchClick(object sender, EventArgs e)
		{
			displaySearch();
		}

		private void displaySearch()
		{
			IList<IPerson> persons = new List<IPerson>(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values);

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

		private void toolStripMenuItemViewDetailsClick(object sender, EventArgs e)
		{
			showRequestDetailsView(null);
		}

		private void showRequestDetailsView(EventParameters<ShowRequestDetailsView> eventParameters)
		{
			if (!_permissionHelper.IsViewRequestDetailsAvailable(_requestView)) return;
			var requestDetailsView = new RequestDetailsView(_eventAggregator, _requestView.SelectedAdapters().First(),
				SchedulerState.SchedulerStateHolder.Schedules);
			requestDetailsView.Show(this);
		}

		private void toolStripMenuItemStartAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByStartAscendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemStartTimeDescMouseUp(object sender, MouseEventArgs e)
		{

			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByStartDescendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemEndTimeAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByEndAscendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemEndTimeDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByEndDescendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemUnlockSelectionRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			GridHelper.GridUnlockSelection(schedulerSplitters1.Grid, LockManager);
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
			GridHelper.GridlockSelection(schedulerSplitters1.Grid, LockManager);
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
			var scheduleDays = selectedSchedules as IScheduleDay[] ?? selectedSchedules.ToArray();
			if (!scheduleDays.Any())
				return false;

			scheduleDay = scheduleDays.First();
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
			IScheduleDay selected = SchedulerState.SchedulerStateHolder.Schedules[person].ScheduledDay(dateOnly);
			findMatching(selected);
		}

		private void findMatching(IScheduleDay selected)
		{
			using (
				var form = new FindMatchingNew(_container.Resolve<IRestrictionExtractor>(), selected.Person, selected.DateOnlyAsPeriod.DateOnly,
					new ScheduleDayForPerson(() => new ScheduleRangeForPerson(() => SchedulerState.SchedulerStateHolder.SchedulingResultState)),
					SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values)
				)
			{
				form.ShowDialog(this);
				if (form.DialogResult == DialogResult.OK)
				{
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> { selected });
					_scheduleView.GridClipboardCopy(false);
					if (form.Selected() == null)
						return;
					IScheduleDay target = SchedulerState.SchedulerStateHolder.Schedules[form.Selected()].ScheduledDay(selected.DateOnlyAsPeriod.DateOnly);
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> { target });
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

			bool isLocked = LockManager.HasLocks && LockManager.Gridlocks(selected) != null;

			using (var auditHistoryView = new AuditHistoryView(selected, this))
			{
				auditHistoryView.ShowDialog(this);
				if (auditHistoryView.DialogResult != DialogResult.OK || auditHistoryView.SelectedScheduleDay == null ||
					isLocked) return;

				var historyDay = auditHistoryView.SelectedScheduleDay;

				var scheduleRange = SchedulerState.SchedulerStateHolder.Schedules[historyDay.Person];
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
					new SchedulePartModifyAndRollbackService(SchedulerState.SchedulerStateHolder.SchedulingResultState,
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
				: SchedulerState.PersonRequests.FirstOrDefault(r => r.Request is AbsenceRequest);
			if (defaultRequest != null)
				id = defaultRequest.Person.Id.GetValueOrDefault();
			var presenter = _container.BeginLifetimeScope().Resolve<IRequestHistoryViewPresenter>();
			presenter.ShowHistory(id, SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values);
		}

		private void toolStripExTagsSizeChanged(object sender, EventArgs e)
		{
			toolStripSplitButtonChangeTag.Width = toolStripComboBoxAutoTag.Width;
		}

		private void toolStripMenuItemContractTimeAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeAscendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemContractTimeDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeDescendingCommand(SchedulerState.SchedulerStateHolder));
		}

		private void toolStripMenuItemSeniorityRankDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				_scheduleView.Sort(new SortBySeniorityRankingDescendingCommand(SchedulerState.SchedulerStateHolder,
					_container.Resolve<IRankedPersonBasedOnStartDate>()));
		}

		private void toolStripMenuItemSeniorityRankAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				_scheduleView.Sort(new SortBySeniorityRankingAscendingCommand(SchedulerState.SchedulerStateHolder,
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
			((AgentRestrictionsDetailView)_scheduleView).PasteSelectedRestrictions(_undoRedo);
		}

		private void toolStripMenuItemRestrictionDeleteClick(object sender, EventArgs e)
		{
			((AgentRestrictionsDetailView)_scheduleView).DeleteSelectedRestrictions(_undoRedo, _defaultScheduleTag, _container.Resolve<IScheduleDayChangeCallback>());
		}

		private void editControlRestrictionsNewClicked(object sender, EventArgs e)
		{
			addPreferenceToolStripMenuItemClick(sender, e);
		}

		private void editControlRestrictionsNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			_editControlRestrictions.CloseDropDown();
			if ((ClipboardItems)e.ClickedItem.Tag == ClipboardItems.StudentAvailability)
				addStudentAvailabilityToolStripMenuItemClick(sender, e);
			if ((ClipboardItems)e.ClickedItem.Tag == ClipboardItems.Preference)
				addPreferenceToolStripMenuItemClick(sender, e);
		}

		private void addPreferenceToolStripMenuItemClick(object sender, EventArgs e)
		{
			IScheduleDay selectedDay;
			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (!tryGetFirstSelectedSchedule(selectedSchedules, out selectedDay)) return;

			using (var view = new AgentPreferenceView(selectedDay, SchedulerState.SchedulerStateHolder, _container.Resolve<IScheduleDayChangeCallback>()))
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

			using (var view = new AgentStudentAvailabilityView(selectedDay, SchedulerState.SchedulerStateHolder.SchedulingResultState, _container.Resolve<IScheduleDayChangeCallback>()))
			{
				view.ShowDialog(this);
				updateRestrictions(selectedSchedules.First());
			}
		}

		private void updateRestrictions(IScheduleDay scheduleDay)
		{
			if (_scheduleView == null || scheduleDay == null) return;
			if (_scheduleView is AgentRestrictionsDetailView)
				schedulerSplitters1.ReselectSelectedAgentNotPossibleToSchedule();

			updateSelectionInfo(new List<IScheduleDay> { scheduleDay });
			enableSave();
		}

		private void addOvertimeAvailabilityToolStripMenuItemClick(object sender, EventArgs e)
		{
			IScheduleDay selectedDay;
			if (!tryGetFirstSelectedSchedule(_scheduleView.SelectedSchedules(), out selectedDay)) return;

			using (var view = new AgentOvertimeAvailabilityView(selectedDay, SchedulerState.SchedulerStateHolder.SchedulingResultState, _container.Resolve<IScheduleDayChangeCallback>()))
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

				SchedulerState.SchedulerStateHolder.ResetFilteredPersonsOvertimeAvailability();
				reloadFilteredPeople();
				return;
			}
			var defaultDate = _scheduleView.SelectedDateLocal();

			using (var view = new FilterOvertimeAvailabilityView(defaultDate, SchedulerState.SchedulerStateHolder))
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
				SchedulerState.SchedulerStateHolder.ResetFilteredPersonsHourlyAvailability();
				reloadFilteredPeople();
				return;
			}

			var defaultDate = _scheduleView.SelectedDateLocal();
			using (var view = new FilterHourlyAvailabilityView(defaultDate, SchedulerState.SchedulerStateHolder))
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
			toolStripButtonFilterAgents.Checked = SchedulerState.SchedulerStateHolder.AgentFilter();

			if (_scheduleView != null)
			{
				if (_scheduleView.Presenter.SortCommand == null || _scheduleView.Presenter.SortCommand is NoSortCommand)
					_scheduleView.Presenter.ApplyGridSort();
				else
					_scheduleView.Sort(_scheduleView.Presenter.SortCommand);

				schedulerSplitters1.Grid.Refresh();
				GridHelper.GridlockWriteProtected(SchedulerState.SchedulerStateHolder, LockManager);
				_scheduleView.ResizeGridColumnsToFit();
				schedulerSplitters1.Grid.Refresh();
			}
			_requestView?.FilterPersons(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Keys);
			drawSkillGrid();

			_shiftCategoryDistributionModel.SetFilteredPersons(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values);
			schedulerSplitters1.RefreshTabInfoPanels(SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values);
			updateShiftEditor();
			toolStripStatusLabelNumberOfAgents.Text = LanguageResourceHelper.Translate("XXAgentsColon") + @" " +
													  SchedulerState.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Count;
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
			SchedulerState.SchedulerStateHolder.TimeZoneInfo = TimeZoneGuard.Instance.TimeZone;
			wpfShiftEditor1.SetTimeZone(TimeZoneGuard.Instance.TimeZone);
			var selectedSchedules = _scheduleView.SelectedSchedules();
			if (_scheduleView != null && _scheduleView.HelpId == "AgentRestrictionsDetailView")
			{
				IEnumerable<DateOnly> selectedDates = _scheduleView.AllSelectedDates(selectedSchedules);
				prepareAgentRestrictionView(_scheduleView, new List<IPerson>(_scheduleView.AllSelectedPersons(selectedSchedules)), new DateOnlyPeriod(selectedDates.Min(), selectedDates.Max()));
			}
			displayTimeZoneInfo();
			_scheduleView.SetSelectedDateLocal(_dateNavigateControl.SelectedDate);
			schedulerSplitters1.Grid.Invalidate();
			schedulerSplitters1.Grid.Refresh();
			updateSelectionInfo(selectedSchedules);
			updateShiftEditor();
			drawSkillGrid();
			reloadChart();
		}

		private void displayTimeZoneInfo()
		{
			bool show = _detectedTimeZoneInfos.Count > 1;
			if (!show)
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
							resolution = skills.Min(x => x.DefaultResolution);
						}
					}
				}

				var showUseSkills = SchedulerState.SchedulerStateHolder.SchedulingResultState.Skills.Any(x => x.IsCascading());

				using (var options = new OvertimePreferencesDialog(SchedulerState.ScheduleTags.NonDeleted(),
																"OvertimePreferences",
																SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted(),
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
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			_scheduleTimeType = (ScheduleTimeType)item.Tag;
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
				var publishCommand = new PublishScheduleCommand(workflowControlSets, publishToDate, SchedulerState);
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

		private void backStageButtonManiMenuImportClick(object sender, EventArgs e)
		{
			openResourcePlannerInWfm("importschedule");
		}

		private void backStageButtonMainMenuCopyClick(object sender, EventArgs e)
		{
			openResourcePlannerInWfm("copyschedule");
		}

		private void openResourcePlannerInWfm(string what)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("WebApp");
			var url = $"{wfmPath}WFM/#/resourceplanner/{what}";
			if (url.IsAnUrl())
				Process.Start(url);
		}

		private void replaceActivityToolStripMenuItemClick(object sender, EventArgs e)
		{
			replaceActivity();
		}

		private void replaceActivity()
		{
			if (!TestMode.Micke) return;
			var schedules = _scheduleView.SelectedSchedules();
			if (schedules.IsEmpty()) return;
			var replaceActivityService = _container.Resolve<ReplaceActivityService>();
			var defaultPeriod = AddActivityCommand.GetDefaultPeriodFromPart(schedules[0]);
			using (var view = new ReplaceActivityView(SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities.ToList(), defaultPeriod.TimePeriod(TimeZoneGuard.Instance.CurrentTimeZone()), _replaceActivityParameters))
			{
				var result = view.ShowDialog(this);
				if (result != DialogResult.OK)
					return;

				_replaceActivityParameters = view.ActivityParameters;
				var activity = view.ActivityParameters.Activity;
				var replaceWithActivity = view.ActivityParameters.ReplaceWithActivity;
				var timePeriod = new TimePeriod(view.ActivityParameters.FromTimeSpan, view.ActivityParameters.ToTimeSpan);
				replaceActivityService.Replace(schedules, activity, replaceWithActivity, timePeriod, TimeZoneGuard.Instance.CurrentTimeZone());
				_scheduleView.Presenter.ModifySchedulePart(schedules);
				foreach (var part in schedules)
				{
					_scheduleView.RefreshRangeForAgentPeriod(part.Person, defaultPeriod);
				}

				RecalculateResources();
				RunActionWithDelay(updateShiftEditor, 50);
			}	
		}
	}
}

//Cake-in-the-kitchen if* this reaches 5000! 
//bigmac-in-the-kitchen if* this reaces 6000!
//new-mailfooter-in-the-kitchen if* this reaces 7000!
//naken-kebab-in-the-kitchen if* this reaces 8000!
//*when
