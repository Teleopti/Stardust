
#region wohoo!! 51 usings in one form
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Win.Meetings;
using Teleopti.Ccc.Win.Optimization;
using Teleopti.Ccc.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Grouping;
using log4net;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.Reporting;
using Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences;
using Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Ccc.WinCode.Scheduling.GridlockCommands;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Ccc.WpfControls.Controls.Editor;
using Teleopti.Ccc.WpfControls.Controls.Notes;
using Teleopti.Ccc.WpfControls.Controls.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

#endregion

namespace Teleopti.Ccc.Win.Scheduling
{
	[CLSCompliant(true)]
	public partial class SchedulingScreen : BaseRibbonForm
	{
		#region Fields
		private readonly ILifetimeScope _container;
		private IScheduleScreenPersister _persister;
		private static readonly ILog Log = LogManager.GetLogger(typeof(SchedulingScreen));
		private ISchedulerStateHolder _schedulerState;
		private readonly ClipHandler<IScheduleDay> _clipHandlerSchedule;
		private readonly IList<IPerson> _selectedPersons = new List<IPerson>();
		private readonly SkillDayGridControl _skillDayGridControl;
		private readonly SkillIntradayGridControl _skillIntradayGridControl;
		private readonly SkillWeekGridControl _skillWeekGridControl;
		private readonly SkillMonthGridControl _skillMonthGridControl;
		private readonly SkillFullPeriodGridControl _skillFullPeriodGridControl;
		private readonly SkillResultHighlightGridControl _skillResultHighlightGridControl;
		private DateOnly _currentIntraDayDate;
		private DockingManager _dockingManager;
		private FormAgentInfo _agentInfo;
		private ScheduleViewBase _scheduleView;
		private RequestView _requestView;
		private ResourceOptimizationHelperWin _optimizationHelperWin;
		private ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private GroupDayOffOptimizerHelper _groupDayOffOptimizerHelper;
		//private BlockOptimizerHelper _blockOptimizerHelper;
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
		private readonly WpfControls.Controls.Requests.Views.HandlePersonRequestView _handlePersonRequestView1;
		private SingleAgentRestrictionPresenter _singleAgentRestrictionPresenter;
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
		private bool _showEditor = true;
		private bool _showResult = true;
		private bool _showGraph = true;
		private bool _showRibbonTexts = true;
		#endregion
		private ControlType _controlType;
		private SchedulerMessageBrokerHandler _schedulerMessageBrokerHandler;
		private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
		private readonly ContextMenuStrip _contextMenuSkillGrid = new ContextMenuStrip();
		private readonly IOptimizerOriginalPreferences _optimizerOriginalPreferences;
		private readonly IOptimizationPreferences _optimizationPreferences;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly IBudgetPermissionService _budgetPermissionService;
		private readonly ICollection<IPerson> _personsToValidate = new HashSet<IPerson>();
		private readonly ICollection<IPerson> _restrictionPersonsToReload = new HashSet<IPerson>();
		private readonly BackgroundWorker _backgroundWorkerValidatePersons = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerResourceCalculator = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerDelete = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerScheduling = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerOptimization = new BackgroundWorker();
		private readonly IUndoRedoContainer _undoRedo = new UndoRedoContainer(500);
		private IDayOffTemplate _dayOffTemplate;
		private readonly ICollection<IPersonWriteProtectionInfo> _modifiedWriteProtections = new HashSet<IPersonWriteProtectionInfo>();
		private SchedulingScreenSettings _currentSchedulingScreenSettings;
		private ZoomLevel _currentZoomLevel;
		private SplitterManagerRestrictionView _splitterManager;
		private IWorkShiftWorkTime _workShiftWorkTime;
		private DateOnly _defaultFilterDate;
		private bool _inUpdate;
		private int _totalScheduled;
		private readonly IPersonRequestCheckAuthorization _personRequestAuthorizationChecker;
		private bool _forceClose;
		private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private readonly DateNavigateControl _dateNavigateControl;
		private bool _isAuditingSchedules;
		private IScheduleTag _defaultScheduleTag = NullScheduleTag.Instance;
		private System.Windows.Forms.Timer _tmpTimer = new System.Windows.Forms.Timer();
		private ISchedulerGroupPagesProvider _groupPagesProvider;
		public IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSet { get; private set; }
		private SkillResultViewSetting _skillResultViewSetting;
    	private ISingleSkillDictionary _singleSkillDictionary;
		private const int maxCalculatMinMaxCacheEnries = 100000;
		public IList<IWorkflowControlSet> WorkflowControlSets { get; private set; }
		private DateTimePeriod _selectedPeriod;
	    private bool isWindowLoaded = false;

		#region enums
		private enum ZoomLevel
		{
			Level1,
			Level2,
			Level3,
			Level4,
			Level5,
			Level6,
			Level7
		}
		private enum ControlType
		{
			ShiftEditor,
			SchedulerGridMain,
			SchedulerGridSkillData,
			Request
		}
		private enum OptimizationMethod
		{
			BackToLegalState,
			ReOptimize
		}
		#endregion
		#region private classes
		private class LoaderMethod
		{
			private readonly Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> _action;
			private readonly string _statusStripString;

			public LoaderMethod(Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> action,
								string statusStripString)
			{
				_action = action;
				_statusStripString = statusStripString;
			}

			public Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> Action
			{
				get { return _action; }
			}

			public string StatusStripString
			{
				get { return _statusStripString; }
			}
		}
		#endregion
		#region Constructors

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected SchedulingScreen()
		{
			InitializeComponent();
			_dateNavigateControl = new DateNavigateControl();

			var hostDatePicker = new ToolStripControlHost(_dateNavigateControl);
			toolStripExScheduleViews.Items.Add(hostDatePicker);
			_grid = schedulerSplitters1.Grid;
			_chartControlSkillData = schedulerSplitters1.ChartControlSkillData;
			_splitContainerAdvMain = schedulerSplitters1.SplitContainerAdvMain;
			_splitContainerAdvResultGraph = schedulerSplitters1.SplitContainerAdvResultGraph;
			_splitContainerLessIntellegentEditor = schedulerSplitters1.SplitContainerLessIntelligent1;
			_splitContainerLessIntellegentRestriction = schedulerSplitters1.SplitContainerView;
			_elementHostRequests = schedulerSplitters1.ElementHostRequests;
			_handlePersonRequestView1 = schedulerSplitters1.HandlePersonRequestView1;
			_tabSkillData = schedulerSplitters1.TabSkillData;
			wpfShiftEditor1 = new WpfShiftEditor(_eventAggregator, new CreateLayerViewModelService(), true);
			notesEditor = new NotesEditor(PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
			schedulerSplitters1.MultipleHostControl3.AddItem(Resources.ShiftEditor, wpfShiftEditor1);
			schedulerSplitters1.MultipleHostControl3.AddItem(Resources.Note, notesEditor);
			toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = false;
			toolStripSpinningProgressControl1.Visible = true;
			if (!DesignMode) SetTexts();
			if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
				Icon = Properties.Resources.scheduler;

			// this timer is just for fixing bug 17948 regarding dateNavigationControl
			_tmpTimer.Interval = 50;
			_tmpTimer.Enabled = false;
			_tmpTimer.Tick += _tmpTimer_Tick;
		}

		private void _tmpTimer_Tick(object sender, EventArgs e)
		{
			_tmpTimer.Enabled = false;
			updateShiftEditor();
			_grid.Focus();
		}

		private void dateNavigateControlSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
		{
			_scheduleView.SetSelectedDateLocal(e.Value);
			_grid.Invalidate();
			//if (_intradayMode && _scheduleView is DayViewNew)
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday) && _scheduleView is DayViewNew)
			{
				drawSkillGrid();
				reloadChart();
			}

			_tmpTimer.Enabled = true;

		}

		private void formatRibbonControls()
		{
			//Load Options
			setAllToolStripItemsToLongestWidth(toolStripPanelItemLoadOptions);
			//Show
			setAllToolStripItemsToLongestWidth(toolStripPanelItem1);
			//Locks
			setAllToolStripItemsToLongestWidth(toolStripPanelItemLocks);
			//Actions
			setAllToolStripItemsToLongestWidth(toolStripPanelItemAssignments);
			//Views
			setAllToolStripItemsToLongestWidth(toolStripPanelItemViews);
			setAllToolStripItemsToLongestWidth(toolStripPanelItemViews2);
			//Edit
			setAllToolStripItemsToLongestWidth(_editControl.PanelItem);
		}

		private void setShowRibbonTexts()
		{
			//HOME
			//Load Options
			toggleRibbonTexts(toolStripPanelItemLoadOptions.Items);
			//Show
			toggleRibbonTexts(toolStripPanelItem1.Items);
			//Locks
			toggleRibbonTexts(toolStripPanelItemLocks.Items);
			//Actions
			toggleRibbonTexts(toolStripPanelItemAssignments.Items);
			//Views
			toggleRibbonTexts(toolStripPanelItemViews.Items);
			toggleRibbonTexts(toolStripPanelItemViews2.Items);
			//Edit
			toggleRibbonTexts(_editControl.PanelItem.Items);
			toggleRibbonTexts(_editControlRestrictions.PanelItem.Items);
			//Clipboard
			toggleRibbonTexts(_clipboardControl.PanelItem.Items);
			toggleRibbonTexts(_clipboardControlRestrictions.PanelItem.Items);
			//REQUESTS
			toggleRibbonTexts(toolStripExHandleRequests.Items);

			if (_showRibbonTexts)
				formatRibbonControls();

		}

		private static void setAllToolStripItemsToLongestWidth(ToolStripPanelItem panelItem)
		{
			int maxWidth = 0;

			foreach (ToolStripItem item in panelItem.Items)
			{
				if (item.Width > maxWidth)
					maxWidth = item.Width;
			}

			foreach (ToolStripItem item in panelItem.Items)
			{
				item.ImageAlign = ContentAlignment.MiddleLeft;
				item.TextAlign = ContentAlignment.MiddleLeft;
				item.AutoSize = false;
				item.Width = maxWidth;
			}
		}

		private void toggleRibbonTexts(ToolStripItemCollection items)
		{
			if (_showRibbonTexts)
			{
				foreach (ToolStripItem item in items)
				{
					item.Owner.ShowItemToolTips = false;
					item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
				}
			}
			else
			{
				foreach (ToolStripItem item in items)
				{
					item.Owner.ShowItemToolTips = true;
					item.DisplayStyle = ToolStripItemDisplayStyle.Image;
					item.AutoSize = true;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public SchedulingScreen(IComponentContext componentContext, DateOnlyPeriod loadingPeriod, IScenario loadScenario, bool shrinkage, bool calculation, bool validation, bool teamLeaderMode, IList<IEntity> allSelectedEntities)
			: this()
		{
			Application.DoEvents();
			loadSchedulingScreenSettings();
			_skillDayGridControl = new SkillDayGridControl { ContextMenu = contextMenuStripResultView.ContextMenu };
			_skillIntradayGridControl = new SkillIntradayGridControl("SchedulerSkillIntradayGridAndChart") { ContextMenu = contextMenuStripResultView.ContextMenu };
			_skillWeekGridControl = new SkillWeekGridControl { ContextMenu = contextMenuStripResultView.ContextMenu };
			_skillMonthGridControl = new SkillMonthGridControl { ContextMenu = contextMenuStripResultView.ContextMenu };
			_skillFullPeriodGridControl = new SkillFullPeriodGridControl { ContextMenu = contextMenuStripResultView.ContextMenu };
			_skillResultHighlightGridControl = new SkillResultHighlightGridControl();

			setUpZomMenu();
			var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
			_container = lifetimeScope.BeginLifetimeScope();
			_optimizerOriginalPreferences = new OptimizerOriginalPreferences(new SchedulingOptions());
			_optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
			_overriddenBusinessRulesHolder = _container.Resolve<IOverriddenBusinessRulesHolder>();
			_workShiftWorkTime = _container.Resolve<IWorkShiftWorkTime>();
			_temporarySelectedEntitiesFromTreeView = allSelectedEntities;
			_virtualSkillHelper = _container.Resolve<IVirtualSkillHelper>();
			_budgetPermissionService = _container.Resolve<IBudgetPermissionService>();
			_groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
			_schedulerState = _container.Resolve<ISchedulerStateHolder>();
			_groupPagesProvider = _container.Resolve<ISchedulerGroupPagesProvider>();
			_schedulerState.SetRequestedScenario(loadScenario);
			_schedulerState.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(loadingPeriod, TeleoptiPrincipal.Current.Regional.TimeZone);
			_defaultFilterDate = _schedulerState.RequestedPeriod.DateOnlyPeriod.StartDate;
			_schedulerState.UndoRedoContainer = _undoRedo;
			_schedulerMessageBrokerHandler = new SchedulerMessageBrokerHandler(this, _container);
			_schedulerMeetingHelper = new SchedulerMeetingHelper(_schedulerMessageBrokerHandler, _schedulerState);
			//Using the same module id when saving meeting changes to avoid getting them via MB as well
			_schedulerMeetingHelper.ModificationOccured += _schedulerMeetingHelper_ModificationOccured;
			toolStripMenuItemLoggedOnUserTimeZone.Text = _schedulerState.TimeZoneInfo.DisplayName;
			toolStripButtonRefresh.Text = Resources.Refresh;
			toolStripMenuItemLoggedOnUserTimeZone.Tag = _schedulerState.TimeZoneInfo;
			_clipHandlerSchedule = new ClipHandler<IScheduleDay>();

			_scenario = loadScenario;
			_shrinkage = shrinkage;
			_schedulerState.SchedulingResultState.SkipResourceCalculation = !calculation;
			_validation = validation;
			_schedulerState.SchedulingResultState.UseValidation = validation;
			_teamLeaderMode = teamLeaderMode;
			_schedulerState.SchedulingResultState.TeamLeaderMode = teamLeaderMode;
			_skillResultViewSetting = _currentSchedulingScreenSettings.SkillResultViewSetting;
			initializeDocking();
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

			displayOptionsFromSetting();
			_dateNavigateControl.SetAvailableTimeSpan(loadingPeriod);
			_dateNavigateControl.SetSelectedDateNoInvoke(loadingPeriod.StartDate);
			_dateNavigateControl.SelectedDateChanged += dateNavigateControlSelectedDateChanged;

			_backgroundWorkerDelete.WorkerSupportsCancellation = true;
			_backgroundWorkerDelete.DoWork += _backgroundWorkerDelete_DoWork;
			_backgroundWorkerDelete.RunWorkerCompleted += _backgroundWorkerDelete_RunWorkerCompleted;

			_backgroundWorkerResourceCalculator.DoWork += _backgroundWorkerResourceCalculator_DoWork;
			_backgroundWorkerResourceCalculator.ProgressChanged += _backgroundWorkerResourceCalculator_ProgressChanged;
			_backgroundWorkerResourceCalculator.RunWorkerCompleted += _backgroundWorkerResourceCalculator_RunWorkerCompleted;

			_backgroundWorkerValidatePersons.RunWorkerCompleted += _backgroundWorkerValidatePersons_RunWorkerCompleted;
			_backgroundWorkerValidatePersons.DoWork += _backgroundWorkerValidatePersons_DoWork;

			_backgroundWorkerScheduling.WorkerReportsProgress = true;
			_backgroundWorkerScheduling.WorkerSupportsCancellation = true;
			_backgroundWorkerScheduling.DoWork += _backgroundWorkerScheduling_DoWork;
			_backgroundWorkerScheduling.ProgressChanged += _backgroundWorkerScheduling_ProgressChanged;
			_backgroundWorkerScheduling.RunWorkerCompleted += _backgroundWorkerScheduling_RunWorkerCompleted;

			_backgroundWorkerOptimization.WorkerReportsProgress = true;
			_backgroundWorkerOptimization.WorkerSupportsCancellation = true;
			_backgroundWorkerOptimization.DoWork += _backgroundWorkerOptimization_DoWork;
			_backgroundWorkerOptimization.ProgressChanged += _backgroundWorkerOptimization_ProgressChanged;
			_backgroundWorkerOptimization.RunWorkerCompleted += _backgroundWorkerOptimization_RunWorkerCompleted;
			setPermissionOnControls();
			setInitialClipboardControlState();
			setupContextMenuSkillGrid();
			setupToolbarButtonsChartViews();
			contextMenuViews.Opened += contextMenuViews_Opened;
			setHeaderText(loadingPeriod.StartDate, loadingPeriod.EndDate);
			setLoadingOptions();
			setShowRibbonTexts();

			_personRequestAuthorizationChecker = new PersonRequestCheckAuthorization();

			//todo: move more of this stuff to ioc
			var scheduleDictionaryBatchingPersister = _container.Resolve<IScheduleDictionaryBatchPersister>(
				TypedParameter.From<IMessageBrokerModule>(_schedulerMessageBrokerHandler),
				TypedParameter.From<IReassociateData>(_schedulerMessageBrokerHandler)
				);
			_persister = _container.Resolve<IScheduleScreenPersister>(
				TypedParameter.From<IPersonRequestPersister>(
					new PersonRequestPersister((IClearReferredShiftTradeRequests)_schedulerState)),
				TypedParameter.From<IPersonAbsenceAccountRefresher>(
					new PersonAbsenceAccountRefresher(new RepositoryFactory(), _scenario)),
				TypedParameter.From<IPersonAbsenceAccountValidator>(
					new AnonymousPersonAbsenceAccountValidator(a =>
																   {
																	   foreach (var account in a.AccountCollection())
																	   {
																		   var response =
																			   validatePersonAccounts(
																				   account.Owner.Person);
																		   if (response != null)
																		   {
																			   _personAbsenceAccountPersistValidationBusinessRuleResponses
																				   .Add(response);
																		   }
																	   }
																   })),
				TypedParameter.From<IMessageBrokerModule>(_schedulerMessageBrokerHandler),
				TypedParameter.From(scheduleDictionaryBatchingPersister),
				TypedParameter.From<IOwnMessageQueue>(_schedulerMessageBrokerHandler)
				);

		}

		private void loadSchedulingScreenSettings()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingRepository = new PersonalSettingDataRepository(uow);
					_currentSchedulingScreenSettings = settingRepository.FindValueByKey("SchedulingScreen", new SchedulingScreenSettings());
				}
			}
			catch (DataSourceException ex)
			{
				Log.Error("An error occurred while trying to load settings.", ex);
				_currentSchedulingScreenSettings = new SchedulingScreenSettings();
			}
		}

		private readonly List<IBusinessRuleResponse> _personAbsenceAccountPersistValidationBusinessRuleResponses = new List<IBusinessRuleResponse>();

		//TODO: Should probably be refactored away with the new validation refact. And if not, we should do it anyway... =P
		private class AnonymousPersonAbsenceAccountValidator : IPersonAbsenceAccountValidator
		{
			private readonly Action<IPersonAbsenceAccount> _validateMethod;

			public AnonymousPersonAbsenceAccountValidator(Action<IPersonAbsenceAccount> validateMethod)
			{
				_validateMethod = validateMethod;
			}

			public void Validate(IPersonAbsenceAccount personAbsenceAccount)
			{
				_validateMethod.Invoke(personAbsenceAccount);
			}
		}

		private void setLoadingOptions()
		{
			toolStripButtonShrinkage.Checked = _shrinkage;
			toolStripButtonCalculation.Checked = !_schedulerState.SchedulingResultState.SkipResourceCalculation;
			toolStripButtonValidation.Checked = _validation;
		}

		private void setHeaderText(DateOnly start, DateOnly end)
		{
			CultureInfo currentCultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			string startDate = start.Date.ToString("d", currentCultureInfo);
			string endDate = end.Date.ToString("d", currentCultureInfo);
			Text = string.Format(currentCultureInfo, Resources.TeleoptiCCCColonModuleColonFromToDateScenarioColon, Resources.Schedules, startDate, endDate, _scenario.Description.Name);
		}

		private void _schedulerMeetingHelper_ModificationOccured(object sender, Meetings.ModifyMeetingEventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(_schedulerState.SchedulingResultState.PersonsInOrganization);
				_schedulerMessageBrokerHandler.HandleMeetingChange(e.ModifiedMeeting, e.Delete);
			}
			if (_scheduleView != null &&
				_scheduleView.ViewGrid != null)
			{
				_scheduleView.ViewGrid.InvalidateRange(_scheduleView.ViewGrid.ViewLayout.VisibleCellsRange);
			    updateScheduleParts(e);
                RecalculateResources();
			}
		}

	    private void updateScheduleParts(ModifyMeetingEventArgs e)
	    {
	        var startDate = e.ModifiedMeeting.StartDate;
	        var affectedScheduleDays = new List<IScheduleDay>();
	        while (startDate <= e.ModifiedMeeting.EndDate)
	        {
	            affectedScheduleDays.AddRange(
	                e.ModifiedMeeting.MeetingPersons.Where(meetingPerson => SchedulerState.SchedulingResultState.PersonsInOrganization.Contains(meetingPerson.Person))
	                 .Select(meetingPerson => SchedulerState.SchedulingResultState.Schedules[meetingPerson.Person])
	                 .Select(range => range.ScheduledDay(startDate)));
	            startDate = startDate.AddDays(1);
	        }
	        _scheduleView.Presenter.ModifySchedulePart(affectedScheduleDays, true);
	    }

	    private void permittedPersonsToSelectedList()
		{
			foreach (IPerson person in SchedulerState.AllPermittedPersons)
			{
				_selectedPersons.Add(person);
			}
		}

		private void contextMenuViews_Opened(object sender, EventArgs e)
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
			_editControl.NewClicked += _editControl_NewClicked;
			_editControl.NewSpecialClicked += _editControl_NewSpecialClicked;
			_editControl.DeleteClicked += _editControl_DeleteClicked;
			_editControl.DeleteSpecialClicked += _editControl_DeleteSpecialClicked;
			var editControlHost = new ToolStripControlHost(_editControl);
			toolStripExEdit2.Items.Add(editControlHost);
		}

		private void instantiateEditControlRestrictions()
		{
			_editControlRestrictions = new EditControl();
			SchedulerRibbonHelper.InstantiateEditControlRestrictions(_editControlRestrictions);
			_editControlRestrictions.NewClicked += editControlRestrictionsNewClicked;
			_editControlRestrictions.NewSpecialClicked += editControlRestrictionsNewSpecialClicked;
			_editControlRestrictions.DeleteClicked += toolStripMenuItemRestrictionDelete_Click;
			var editControlHostRestrictions = new ToolStripControlHost(_editControlRestrictions);
			toolStripExEdit2.Items.Add(editControlHostRestrictions);
			editControlHostRestrictions.Visible = false;
		}

		private void setPermissionOnEditControl()
		{
			var permissionSetter = new PermissionEditControl(_editControl);
			var permissionSetterRestriction = new PermissionEditRestrictionControl(_editControlRestrictions);
			permissionSetter.SetPermission();
			permissionSetterRestriction.SetPermission();
		}

		private void _editControl_DeleteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems)e.ClickedItem.Tag)
			{
				case ClipboardItems.Special:
					deleteSpecialSwitch();
					break;

			}
		}

		private void _editControl_DeleteClicked(object sender, EventArgs e)
		{
			deleteSwitch();
		}

		private void _editControl_NewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
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
						var definitionSets = from set in MultiplicatorDefinitionSet
											 where set.MultiplicatorType == MultiplicatorType.Overtime
											 select set;
						_scheduleView.Presenter.AddOvertime(definitionSets.ToList());
						break;
					case ClipboardItems.Absence:
						if (!(from a in SchedulerState.CommonStateHolder.Absences
							  where !((IDeleteTag)a).IsDeleted
							  select a).Any())
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
				updateShiftEditor();
			}
		}

		private void _editControl_NewClicked(object sender, EventArgs e)
		{
			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
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
				StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode = !StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode;
				toolStripMenuItemFindMatching.Visible = StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode;
				toolStripMenuItemFindMatching2.Visible = StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode;
				Refresh();
				drawSkillGrid();	
			}
			if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
			{
				var options = new PasteOptions();
				options.PersonalShifts = true;
				_scheduleView.GridClipboardPaste(options, _undoRedo);
				checkCutMode();
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
			//numpad+ and alt and shift and ctrl
			if (e.KeyValue == 107 && e.Alt && e.Shift && e.Control)
				nonBlendSkills();

			if(e.KeyCode == Keys.Q && e.Control && e.Shift)
			{
				if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
				{
					using(var agentSkillExplorer = new AgentSkillExplorer())
					{
						agentSkillExplorer.Setup(_schedulerState, _singleSkillDictionary);
						agentSkillExplorer.ShowDialog(this);
					}
				}
			}

			if (e.KeyCode == Keys.Enter && toolStripTextBoxFilter.Focused)
			{
                _requestView.FilterGrid(toolStripTextBoxFilter.Text.Split(' '), SchedulerState.FilteredPersonDictionary);
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
			SelectAndRefresh();
		}

		private void undoKeyDown()
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Undo();
			_backgroundWorkerRunning = false;
			SelectAndRefresh();
		}

		private void toggleCalculation()
		{
			_schedulerState.SchedulingResultState.SkipResourceCalculation = !_schedulerState.SchedulingResultState.SkipResourceCalculation;
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
				statusStrip1.BackColor = SystemColors.Control;
			}
		}

		private void clipboardMessage(string message)
		{
			string s = (message + " " + _skillDayGridControl.Name);
			Trace.WriteLine(s);
		}

		#region Clipboardcontrol

		private void instantiateClipboardControl()
		{
			_clipboardControl = new ClipboardControl();
			SchedulerRibbonHelper.InstantiateClipboardControl(_clipboardControl);
			_clipboardControl.CopyClicked += _clipboardControl_CopyClicked;
			_clipboardControl.CutSpecialClicked += _clipboardControl_CutSpecialClicked;
			_clipboardControl.CutClicked += _clipboardControl_CutClicked;
			_clipboardControl.PasteSpecialClicked += _clipboardControl_PasteSpecialClicked;
			_clipboardControl.PasteClicked += _clipboardControl_PasteClicked;
			var clipboardhost = new ToolStripControlHost(_clipboardControl);
			toolStripExClipboard.Items.Add(clipboardhost);	
		}

		private void instantiateClipboardControlRestrictions()
		{
			_clipboardControlRestrictions = new ClipboardControl();
			SchedulerRibbonHelper.InstantiateClipboardControlRestrictions(_clipboardControlRestrictions);
			_clipboardControlRestrictions.CopyClicked += toolStripMenuItemRestrictionCopy_Click;
			_clipboardControlRestrictions.PasteClicked += toolStripMenuItemRestrictionPaste_Click;
			var clipboardhost = new ToolStripControlHost(_clipboardControlRestrictions);
			toolStripExClipboard.Items.Add(clipboardhost);
			clipboardhost.Visible = false;	
		}

		/// <summary>
		/// Disable the paste clipboard control at the beginning.
		/// </summary>
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

		private void setPermissionOnClipboardControl()
		{
			var permissionSetter = new PermissionClipboardControl(_clipboardControl);
			var permissionSetterRestriction = new PermissionClipboardRestrictionControl(_clipboardControlRestrictions);
			permissionSetter.SetPermission();
			permissionSetterRestriction.SetPermission();
		}

		private void _clipboardControl_CutClicked(object sender, EventArgs e)
		{
			cutSwitch();
		}

		private void _clipboardControl_PasteClicked(object sender, EventArgs e)
		{
			pasteSwitch();
		}

		private void _clipboardControl_CopyClicked(object sender, EventArgs e)
		{
			copySwitch();
		}

		private void _clipboardControl_PasteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems)e.ClickedItem.Tag)
			{
				case ClipboardItems.Shift:
					pasteAssignmentSwitch();
					break;
				case ClipboardItems.Absence:
					pasteAbsenceSwitch();
					break;
				case ClipboardItems.DayOff:
					pasteDayOffSwitch();
					break;
				case ClipboardItems.PersonalShift:
					pastePersonalShiftSwitch();
					break;
				case ClipboardItems.Special:
					pasteSpecialSwitch();
					break;
				case ClipboardItems.ShiftFromShifts:
					pasteShiftFromShiftsSwitch();
					break;
				default:
					break;
			}
		}

		private void _clipboardControl_CutSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems)e.ClickedItem.Tag)
			{
				case ClipboardItems.Shift:
					cutAssignmentSwitch();
					break;
				case ClipboardItems.Absence:
					cutAbsenceSwitch();
					break;
				case ClipboardItems.DayOff:
					cutDayOffSwitch();
					break;
				case ClipboardItems.PersonalShift:
					cutPersonalShiftSwitch();
					break;
				case ClipboardItems.Special:
					cutSpecialSwitch();
					break;
			}
		}

		private void _clipboardControl_CopySpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch ((ClipboardItems)e.ClickedItem.Tag)
			{
				case ClipboardItems.Special:
					copySpecialSwitch();
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

		private void SchedulingScreen_Load(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			_splitContainerAdvMain.Visible = false;
			_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
			_clipboardControlRestrictions.SetButtonState(ClipboardAction.Paste, true);
			Show();
			Application.DoEvents();
			loadQuickAccessState();
			disableAllExceptCancelInRibbon();
			toolStripStatusLabelStatus.Text = Resources.LoadingThreeDots;

			if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
			{
				toolStripMenuItemFindMatching2.Visible = true;
				toolStripMenuItemFindMatching.Visible = true;
			}

			backgroundWorkerLoadData.WorkerSupportsCancellation = true;
			backgroundWorkerLoadData.DoWork += backgroundWorkerLoadData_DoWork;
			backgroundWorkerLoadData.RunWorkerCompleted += backgroundWorkerLoadData_RunWorkerCompleted;
			backgroundWorkerLoadData.ProgressChanged += backgroundWorkerLoadData_ProgressChanged;
			toolStripProgressBar1.Value = 0;
			toolStripProgressBar1.Maximum = _schedulerState.RequestedPeriod.DateOnlyPeriod.DayCollection().Count + 19;

			var authorization = PrincipalAuthorization.Instance();
			toolStripMenuItemMeetingOrganizer.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
			toolStripMenuItemWriteProtectSchedule.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			schedulerSplitters1.AgentRestrictionGrid.SelectedAgentIsReady += AgentRestrictionGridSelectedAgentIsReady;

			_backgroundWorkerRunning = true;
			backgroundWorkerLoadData.RunWorkerAsync();
		}

		private void loadQuickAccessState()
		{
			var loader = new QuickAccessState(_currentSchedulingScreenSettings, ribbonControlAdv1);
			loader.Load(toolStripButtonQuickAccessSave, toolStripSplitButtonQuickAccessUndo, toolStripButtonQuickAccessRedo, toolStripButtonQuickAccessCancel, toolStripButtonShowTexts);
		}

		private void saveQuickAccessState()
		{
			var saver = new QuickAccessState(_currentSchedulingScreenSettings, ribbonControlAdv1);
			saver.Save();
		}

		private void SchedulingScreen_FormClosing(object sender, FormClosingEventArgs e)
		{
			cancelAllBackgroundWorkers();

			if (_forceClose || _schedulerState == null)
				return;

			if(_cachedPersonsFilterView != null && _cachedPersonsFilterView.Disposing == false)
				_cachedPersonsFilterView.Dispose();

			int res = checkIfUserWantsToSaveUnsavedData();

			if (res == -1)
				e.Cancel = true;

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
					_currentSchedulingScreenSettings.HideRibbonTexts = !_showRibbonTexts;
					_currentSchedulingScreenSettings.DefaultScheduleTag = _defaultScheduleTag.Id;
					_currentSchedulingScreenSettings.SkillResultViewSetting = _skillResultViewSetting;

					if (_scheduleView != null)
					{
						var mapper = new SchedulerSortCommandMapper(SchedulerState, SchedulerSortCommandSetting.NoSortCommand);
						var sortSetting = mapper.GetSettingFromCommand(_scheduleView.Presenter.SortCommand);
						_currentSchedulingScreenSettings.SortCommandSetting = sortSetting;
					}

					using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						var settingDataRepository = new PersonalSettingDataRepository(uow);
						OpenScenarioForPeriodSetting openScenarioForPeriodSetting = settingDataRepository.FindValueByKey("OpenScheduler", new OpenScenarioForPeriodSetting());
						openScenarioForPeriodSetting.NoShrinkage = !toolStripButtonShrinkage.Checked;
						openScenarioForPeriodSetting.NoCalculation = !toolStripButtonCalculation.Checked;
						openScenarioForPeriodSetting.NoValidation = !toolStripButtonValidation.Checked;
						settingDataRepository.PersistSettingValue(openScenarioForPeriodSetting);
						settingDataRepository.PersistSettingValue(_currentSchedulingScreenSettings);
						uow.PersistAll(_schedulerMessageBrokerHandler);
					}
				}
				catch (DataSourceException dataSourceException)
				{
					Log.Error("An error occurred when trying to save settings on closing scheduler.", dataSourceException);
				}

				if (_agentInfo != null)
					_agentInfo.Close();
			}

			Cursor.Current = Cursors.Default;
		}

		#endregion

		#region Schedules RibbonBar Events

		#region Toolstrip events

		private IList<IActivity> GetNonDeletedActivty()
		{
			var finalAvailableActivity = new List<IActivity>();
			foreach (var activity in SchedulerState.CommonStateHolder.Activities.Where(activity => !activity.IsDeleted))
				finalAvailableActivity.Add(activity);
			return finalAvailableActivity;
		}

		private void toolStripMenuItemBackToLegalState_Click(object sender, EventArgs e)
		{
			if (_backgroundWorkerRunning)
				return;

			if (_scheduleView == null)
				return;

			if (_scheduleView.AllSelectedDates().Count == 0)
				return;

			IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
			using (
				var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions, daysOffPreferences, _schedulerState.CommonStateHolder.ShiftCategories, false,
														   true, _groupPagesProvider, _schedulerState.CommonStateHolder.ScheduleTagsNotDeleted, "SchedulingOptions", GetNonDeletedActivty()))
			{
				if (options.ShowDialog(this) == DialogResult.OK)
				{
					Cursor = Cursors.WaitCursor;
					disableAllExceptCancelInRibbon();
					_backgroundWorkerRunning = true;
					_backgroundWorkerOptimization.RunWorkerAsync(new SchedulingAndOptimizeArgument(_scheduleView.SelectedSchedules())
							 {
								 OptimizationMethod = OptimizationMethod.BackToLegalState,
								 DaysOffPreferences = daysOffPreferences
							 });
				}
			}
		}

		private void toolStripMenuItemReOptimize_Click(object sender, EventArgs e)
		{
			if (_backgroundWorkerRunning) return;

			if (_scheduleView != null)
			{
				if (_scheduleView.AllSelectedDates().Count == 0)
					return;

				using (var optimizationPreferencesDialog =
					new OptimizationPreferencesDialog(_optimizationPreferences, _groupPagesProvider, _schedulerState.CommonStateHolder.ScheduleTagsNotDeleted, GetNonDeletedActivty(), SchedulerState.DefaultSegmentLength))
				{
					if (optimizationPreferencesDialog.ShowDialog(this) == DialogResult.OK)
					{
						var optimizationPreferences = new SchedulingAndOptimizeArgument(_scheduleView.SelectedSchedules())
						{
							OptimizationMethod = OptimizationMethod.ReOptimize
						};

						startBackgroundScheduleWork(_backgroundWorkerOptimization, optimizationPreferences, false);
					}
				}
			}
		}

		private void toolStripButtonChartPeriodView_Click(object sender, EventArgs e)
		{
			var button = sender as ToolStripButton;
			if (button == toolStripButtonChartPeriodView) _skillResultViewSetting = SkillResultViewSetting.Period;
			if (button == toolStripButtonChartMonthView) _skillResultViewSetting = SkillResultViewSetting.Month;
			if (button == toolStripButtonChartWeekView) _skillResultViewSetting = SkillResultViewSetting.Week;
			if (button == toolStripButtonChartDayView) _skillResultViewSetting = SkillResultViewSetting.Day;
			if (button == toolStripButtonChartIntradayView) _skillResultViewSetting = SkillResultViewSetting.Intraday;

			((ToolStripMenuItem)_contextMenuSkillGrid.Items["IntraDay"]).Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Day"]).Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Period"]).Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Month"]).Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["Week"]).Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week);

			if (toolStripButtonChartIntradayView != null) toolStripButtonChartIntradayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			if (toolStripButtonChartDayView != null) toolStripButtonChartDayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			if (toolStripButtonChartPeriodView != null) toolStripButtonChartPeriodView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			if (toolStripButtonChartMonthView != null) toolStripButtonChartMonthView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			if (toolStripButtonChartWeekView != null) toolStripButtonChartWeekView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week);

			_currentSelectedGridRow = null;

			drawSkillGrid();
			reloadChart();
		}

		private void toolStripButtonZoom_Click(object sender, EventArgs e)
		{
			var button = sender as ToolStripButton;
			if (button != null && button.Tag != null) zoom((ZoomLevel)button.Tag);
			else
			{
				var quickButton = sender as QuickButtonReflectable;
				if (quickButton != null && quickButton.ReflectedButton.Tag != null) zoom((ZoomLevel)quickButton.ReflectedButton.Tag);
			}
		}

		private void toolStripButtonQuickAccessSave_Click(object sender, EventArgs e)
		{
			if (_forceClose) return;

			save();
		}

		private void toolStripButtonAgentInfo_Click(object sender, EventArgs e)
		{
			if (_scheduleView == null)
				return;
			if (_agentInfo == null)
			{
				_agentInfo = new FormAgentInfo(_workShiftWorkTime, _container, _groupPagesProvider);
				_agentInfo.FormClosed += _agentInfo_FormClosed;
				_agentInfo.Show(this);
			}
			else
			{
				if (_agentInfo.WindowState.Equals(FormWindowState.Minimized))
					_agentInfo.WindowState = FormWindowState.Normal;
			}

			updateSelectionInfo(_scheduleView.SelectedSchedules());
		}

		private void _agentInfo_FormClosed(object sender, FormClosedEventArgs e)
		{
			_agentInfo = null;
		}

		private void changeRequestStatus(IHandlePersonRequestCommand command, IList<PersonRequestViewModel> requestViewAdapters)
		{
			_requestPresenter.ApproveOrDeny(requestViewAdapters, command, string.Empty);
			recalculateResourcesForRequests(requestViewAdapters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void toolStripMenuItemSwapAndReschedule_Click(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				swapSelectedSchedules();

				// TODO show dialog here too??? Not yet, rotation can not be followed when swapping
				ISchedulingOptions schedulingOptions = new SchedulingOptions { GroupPageForShiftCategoryFairness = _groupPagesProvider.GetGroups(false)[0], UseRotations = false };

				_groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _container.Resolve<IGroupPageCreator>()
					.CreateGroupPagePerDate(_scheduleView, _container.Resolve<IGroupScheduleGroupPageDataProvider>(),
					schedulingOptions.GroupPageForShiftCategoryFairness);

				var finderService = _container.Resolve<IWorkShiftFinderService>();
				// This is not working now I presume (SelectedSchedules is probably not correct)
				foreach (IScheduleDay schedulePart in _scheduleView.SelectedSchedules())
				{
					if (schedulePart.PersonDayOffCollection().Count == 0)
					{
						IMainShift selectedShift = _scheduleOptimizerHelper.PrepareAndChooseBestShift(schedulePart, schedulingOptions, finderService);
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

		private void toolStripMenuItemSwap_Click(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				swapSelectedSchedules();
				Refresh();
				RefreshSelection();
			}
		}

		private void SwapRaw()
		{
			var swapper = new Swapper(_scheduleView, _undoRedo, _schedulerState, _gridLockManager, this, _defaultScheduleTag);
			swapper.SwapRaw();
		}

		private void swapSelectedSchedules()
		{
			var swapper = new Swapper(_scheduleView, _undoRedo, _schedulerState, _gridLockManager, this, _defaultScheduleTag);
			swapper.SwapSelectedSchedules(_handleBusinessRuleResponse, _overriddenBusinessRulesHolder);
		}

		private void toolStripMenuItemSchedule_Click(object sender, EventArgs e)
		{
			scheduleSelected();
		}

		private void toolStripMenuItemScheduleSelected_Click(object sender, EventArgs e)
		{
			scheduleSelected();
		}

		private void toolStripButtonMainMenuSave_Click(object sender, EventArgs e)
		{
			save();
		}

		private void toolStripButtonMainMenuHelp_Click(object sender, EventArgs e)
		{
			ShowHelp(true);
		}

		private void toolStripButtonMainMenuClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripButtonSystemExit_Click(object sender, EventArgs e)
		{
			if (!CloseAllOtherForms(this))
				return; // a form was canceled

			Close();
			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

		private void toolStripButtonQuickAccessCancel_Click(object sender, EventArgs e)
		{
			cancelAllBackgroundWorkers();
			toolStripButtonQuickAccessCancel.Enabled = false;
		}

		// the loop is used so the form would not close before all backgroundworkers are canceled
		// have to use Application.DoEvents(); here. Else the loop will continue for ever
		private void cancelAllBackgroundWorkers()
		{
			toolStripStatusLabelStatus.Text = Resources.CancellingThreeDots;
			if (backgroundWorkerLoadData.IsBusy)
			{
				toolStripStatusLabelStatus.Text = Resources.CancelingLoadAndCloseProgram;
				backgroundWorkerLoadData.CancelAsync();
				while (backgroundWorkerLoadData.IsBusy)
				{
					Application.DoEvents();
					Thread.Sleep(10);
				}
			}

			if (_backgroundWorkerDelete.IsBusy)
			{
				_backgroundWorkerDelete.CancelAsync();
				while (_backgroundWorkerDelete.IsBusy)
				{
					Application.DoEvents();
					Thread.Sleep(10);
				}
			}

			if (_backgroundWorkerScheduling.IsBusy)
			{
				_backgroundWorkerScheduling.CancelAsync();
				while (_backgroundWorkerScheduling.IsBusy)
				{
					Application.DoEvents();
					Thread.Sleep(10);
				}
			}
			cancelBackgroundWorkerOptimization();
		}

		private void cancelBackgroundWorkerOptimization()
		{
			if (_backgroundWorkerOptimization.IsBusy)
			{
				_backgroundWorkerOptimization.CancelAsync();
				while (_backgroundWorkerOptimization.IsBusy)
				{
					Application.DoEvents();
					Thread.Sleep(10);
				}
			}
		}

		#region Insert

		private void toolStripButtonInsertDayOff_Click(object sender, EventArgs e)
		{
			addDayOff();
		}

		private void ToolStripMenuItemAddActivity_Click(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Shift);
		}

		private void toolStripMenuItemAddOverTime_Click(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Overtime);
		}

		private void toolStripMenuItemInsertAbsence_Click(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.Absence);
		}

		private void ToolStripMenuItemAddPersonalActivity_Click(object sender, EventArgs e)
		{
			addNewLayer(ClipboardItems.PersonalShift);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void addDayOff()
		{
			if (_scheduleView != null)
			{
				if (SchedulerState.FilteredPersonDictionary.Count > 0)
				{
					IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
														  where ((IDeleteTag)item).IsDeleted == false
														  select item).ToList();
					if (displayList.Count > 0)
					{
						// todo: remove comment when the user warning is ready for the other activities(delete, paste, swap etc.)
						var clone =
							(IScheduleDay)
							SchedulerState.Schedules[SchedulerState.FilteredPersonDictionary.ElementAt(0).Value].
								ScheduledDay(new DateOnly(DateTime.MinValue.AddDays(1))).Clone();
						var selectedSchedules = _scheduleView.SelectedSchedules();
						if (selectedSchedules.Count() == 0)
							return;

						var sortedList = (from d in ScheduleViewBase.AllSelectedDates(selectedSchedules)
										  orderby d.Date
										  select d).ToList();

						var first = sortedList.FirstOrDefault();
						var last = sortedList.LastOrDefault();
						var period = new DateOnlyPeriod(first, last).ToDateTimePeriod(_schedulerState.TimeZoneInfo);
						var addDayOffDialog = _scheduleView.CreateAddDayOffViewModel(displayList, _schedulerState.TimeZoneInfo, period);

						if (!addDayOffDialog.Result)
							return;

						var dayOffTemplate = addDayOffDialog.SelectedItem;
						var personDayOff = new PersonDayOff(clone.Person, _schedulerState.RequestedScenario, dayOffTemplate, new DateOnly().AddDays(1), clone.TimeZone);
						clone.Add(personDayOff);
						_scheduleView.Presenter.ClipHandlerSchedule.Clear();
						_scheduleView.Presenter.ClipHandlerSchedule.AddClip(1, 1, clone);
						_externalExceptionHandler.AttemptToUseExternalResource(() => Clipboard.SetData("PersistableScheduleData", new int()));
						pasteDayOff();
						_scheduleView.Presenter.ClipHandlerSchedule.Clear();
					}
				}
			}
		}

		#endregion

		#region Copy

		private void toolStripMenuItemCopy_Click(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.GridClipboardCopy(false);
				checkPastePermissions();
				_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
				_clipboardControlRestrictions.SetButtonState(ClipboardAction.Paste, true);
			}
		}

		private void copySwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					if (_scheduleView != null)
					{
						_scheduleView.GridClipboardCopy(false);
						checkPastePermissions();
						_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
					}
					break;
				case ControlType.SchedulerGridSkillData:
					var guiHelper = new ColorHelper();
					Control activeControl = guiHelper.GetActiveControl(this);
					var control = (GridControl)activeControl;
					control.CutPaste.Copy();
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor copy");
					break;
			}
		}

		private void copySpecialSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					clipboardMessage("Main copy special");
					break;
				case ControlType.SchedulerGridSkillData:
					var guiHelper = new ColorHelper();
					Control activeControl = guiHelper.GetActiveControl(this);
					var control = (GridControl)activeControl;
					GridHelper.CopySelectedValuesAndHeadersToPublicClipboard(control);
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor copy special");
					break;
			}
		}

		#endregion

		#region Delete

		private void toolStripButtonDelete_Click(object sender, EventArgs e)
		{
			if (_scheduleView != null)
			{
				var deleteOptions = new PasteOptions();
				deleteOptions.Default = true;
				deleteInMainGrid(deleteOptions);
			}
		}

		private void toolStripMenuItemDeleteSpecial2_Click(object sender, EventArgs e)
		{
			deleteSpecial();
		}

		#endregion

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
			var scheduleTag = (IScheduleTag)(((ToolStripMenuItem)(sender)).Tag);
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayTagExtractor scheduleDayTagExtractor = new ScheduleDayTagExtractor(gridSchedulesExtractor.Extract());
			var gridlockTagCommand = new GridlockTagCommand(LockManager, scheduleDayTagExtractor, scheduleTag);
			gridlockTagCommand.Execute();
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripComboBoxAutoTag_SelectedIndexChanged(object sender, EventArgs e)
		{
			_defaultScheduleTag = (IScheduleTag)toolStripComboBoxAutoTag.SelectedItem;
			_scheduleView.Presenter.DefaultScheduleTag = _defaultScheduleTag;
		}

		private void toolStripMenuItemChangeTag(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var scheduleTag = (IScheduleTag)(((ToolStripMenuItem)(sender)).Tag);
			var gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			var setTagCommand = new SetTagCommand(_undoRedo, gridSchedulesExtractor, _scheduleView.Presenter, _scheduleView, scheduleTag, LockManager);
			_backgroundWorkerRunning = true;
			toolStripStatusLabelStatus.Text = Resources.ScheduleTags;
			statusStrip1.Refresh();
			setTagCommand.Execute();
			toolStripStatusLabelStatus.Text = Resources.Ready;
			statusStrip1.Refresh();
			_backgroundWorkerRunning = false;
			updateSelectionInfo(gridSchedulesExtractor.ExtractSelected());
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripSplitButtonLock_ButtonClick(object sender, EventArgs e)
		{
			GridHelper.GridlockSelection(_grid, LockManager);
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemLockSelection_Click(object sender, EventArgs e)
		{
			GridHelper.GridlockSelection(_grid, LockManager);
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemLockAbsenceDays_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllAbsences(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemLockAbsenceDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllAbsences(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockFreeDays_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockFreeDays(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemDayOffLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockFreeDays(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockSpecificDayOff_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var dayOffTemplate = (IDayOffTemplate)(((ToolStripMenuItem)(sender)).Tag);
			GridHelper.GridlockSpecificDayOff(_grid, LockManager, dayOffTemplate);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		//lock all with specified absencens
		private void toolStripMenuItemLockAbsences_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var absence = (Absence)(((ToolStripMenuItem)(sender)).Tag);
			GridHelper.GridlockAbsences(_grid, LockManager, absence);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemAbsenceLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var absence = (Absence)(((ToolStripMenuItem)(sender)).Tag);
			GridHelper.GridlockAbsences(_grid, LockManager, absence);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		//lock all with specified shiftcategory
		private void toolStripMenuItemLockShiftCategories_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var shiftCategory = (ShiftCategory)(((ToolStripMenuItem)(sender)).Tag);
			GridHelper.GridlockShiftCategories(_grid, LockManager, shiftCategory);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemLockShiftCategoriesMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			var shiftCategory = (ShiftCategory)(((ToolStripMenuItem)(sender)).Tag);
			GridHelper.GridlockShiftCategories(_grid, LockManager, shiftCategory);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemLockShiftCategoryDays_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllShiftCategories(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemLockShiftCategoryDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllShiftCategories(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void ToolStripMenuItemLockAllRestrictionsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.LockAllRestrictions(e.Button);
		}

		private void ToolStripMenuItemAllPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllPreferences(e.Button);
		}

		private void ToolStripMenuItemAllDaysOffMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllDaysOff(e.Button);
		}

		private void ToolStripMenuItemAllShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllShiftsPreferences(e.Button);
		}

		private void ToolStripMenuItemAllMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllMustHave(e.Button);
		}

		private void ToolStripMenuItemAllFulfilledMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledMustHave(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledPreferences(e.Button);
		}

		private void ToolStripMenuItemAllAbsencePreferenceMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllAbsencePreference(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledAbsencesPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledAbsencesPreferences(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledDaysOffPreferences(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledShiftsPreferences(e.Button);
		}

		private void ToolStripMenuItemAllRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllRotations(e.Button);
		}

		private void ToolStripMenuItemAllDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllDaysOffRotations(e.Button);
		}

		private void ToolStripMenuItemAllShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllShiftsRotations(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledRotations(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledDaysOffRotations(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledShiftsRotations(e.Button);
		}

		private void ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllUnavailableStudentAvailability(e.Button);
		}

		private void ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllAvailableStudentAvailability(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledStudentAvailability(e.Button);
		}

		private void ToolStripMenuItemAllUnavailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllUnavailableAvailability(e.Button);
		}

		private void ToolStripMenuItemAllAvailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllAvailableAvailability(e.Button);
		}

		private void ToolStripMenuItemAllFulFilledAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _schedulerState, LockManager, this);
			executer.AllFulfilledAvailability(e.Button);
		}

		#endregion

		#region paste

		private void ToolStripMenuItemPaste_Click(object sender, EventArgs e)
		{
			paste();
			updateShiftEditor();
		}

		private void toolStripMenuItemPasteSpecial2_Click(object sender, EventArgs e)
		{
			pasteSpecial();
			updateShiftEditor();
		}

		private void toolStripMenuItemPasteShiftFromShiftsClick(object sender, EventArgs e)
		{
			pasteShiftFromShiftsSwitch();
		}

		private void paste()
		{
			if (_scheduleView != null)
			{
				var options = new PasteOptions();
				options.Default = true;

				if (ClipsHandlerSchedule.IsInCutMode)
					options = ClipsHandlerSchedule.CutMode;

				_backgroundWorkerRunning = true;
				_scheduleView.GridClipboardPaste(options, _undoRedo);
				_backgroundWorkerRunning = false;
				RecalculateResources();
				checkCutMode();
			}
		}


		private void pasteAssignment()
		{
			if (_scheduleView != null)
			{
				var options = new PasteOptions { MainShift = true };
				_scheduleView.GridClipboardPaste(options, _undoRedo);
				checkCutMode();
			}
		}

		private void pasteAbsence()
		{
			if (_scheduleView != null)
			{
				var options = new PasteOptions { Absences = PasteAction.Add };
				_scheduleView.GridClipboardPaste(options, _undoRedo);

				checkCutMode();
			}
		}

		private void pasteDayOff()
		{
			if (_scheduleView != null)
			{
				var options = new PasteOptions { DayOff = true };
				_scheduleView.GridClipboardPaste(options, _undoRedo);

				checkCutMode();
			}
		}

		private void pastePersonalShift()
		{
			if (_scheduleView != null)
			{
				var options = new PasteOptions { PersonalShifts = true };
				_scheduleView.GridClipboardPaste(options, _undoRedo);

				checkCutMode();
			}
		}

		private void pasteSpecial()
		{
			var options = new PasteOptions();
			bool showRestrictions = _scheduleView is RestrictionSummaryView;
			var pasteSpecial = new FormClipboardSpecial(false, showRestrictions, options) { Text = Resources.PasteSpecial };
			pasteSpecial.ShowDialog();

			if (_scheduleView != null)
			{
				if (!pasteSpecial.Cancel())
				{
					_scheduleView.GridClipboardPaste(options, _undoRedo);
					checkCutMode();
				}
			}

			pasteSpecial.Close();
		}

		private void pasteSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					paste();
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor paste");
					break;
			}
		}

		private void pasteAssignmentSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					pasteAssignment();
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("Shifteditor paste ass");
					break;
			}
		}

		private void pasteAbsenceSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					pasteAbsence();
					break;
				case ControlType.SchedulerGridSkillData:
					// do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("Shifteditor paste absence");
					break;
			}
		}

		private void pasteDayOffSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					pasteDayOff();
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor paste day off");
					break;
			}
		}

		private void pastePersonalShiftSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					pastePersonalShift();
					break;
				case ControlType.SchedulerGridSkillData:
					// do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor paste personal shift");
					break;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void pasteShiftFromShiftsSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					IWorkShift workShift = StateHolderReader.Instance.StateReader.SessionScopeData.Clip as WorkShift;
					if (workShift == null)
						return;
					if (WorkShiftMasterActivityChecker.DoesContainMasterActivity(workShift))
					{
						ShowErrorMessage(Resources.CannotPasteAShiftWithMasterActivity, Resources.PasteError);
						return;
					}
					IList<IScheduleDay> lst = _scheduleView.SelectedSchedules();
					if (lst.Count == 0)
						return;

					var part = (IScheduleDay)_schedulerState.Schedules[lst[0].Person].ReFetch(lst[0]).Clone();

					part.Clear<IScheduleData>();
					IMainShift mainShift = workShift.ToMainShift(part.DateOnlyAsPeriod.DateOnly, part.Person.PermissionInformation.DefaultTimeZone());

					foreach (var cat in _schedulerState.CommonStateHolder.ShiftCategories.Where(cat => cat.Id.Equals(workShift.ShiftCategory.Id)))
					{
						mainShift.ShiftCategory = cat;
					}

					part.AddMainShift(mainShift);
					_clipHandlerSchedule.Clear();
					_clipHandlerSchedule.AddClip(0, 0, part);
					_externalExceptionHandler.AttemptToUseExternalResource(() => Clipboard.SetData("PersistableScheduleData", new int()));
					paste();
					break;
				case ControlType.SchedulerGridSkillData:
					// do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("Shifteditor paste shift from Shifts");
					break;
			}
		}

		private void pasteSpecialSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					pasteSpecial();
					break;
				case ControlType.SchedulerGridSkillData:
					// do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("Shifteditor paste special");
					break;
			}
		}

		#endregion

		#region Next assignment

		private void toolStripMenuItemShowNextAssignment_Click(object sender, EventArgs e) //used by context
		{
			getAssignmentZOrder(false, true);
		}

		private void toolStripMenuItemShowAssignmentBefore_Click(object sender, EventArgs e) //used by context
		{
			getAssignmentZOrder(true, true);
		}

		#endregion //assignment

		#endregion

		#region Context menu events

		private void contextMenuViews_Opening(object sender, CancelEventArgs e)
		{
			if (_scheduleView == null)
				e.Cancel = true;

			if (getAssignmentZOrder(true, false) != null)
				toolStripMenuItemShowAssignmentBefore.Enabled = true;
			else
				toolStripMenuItemShowAssignmentBefore.Enabled = false;

			if (getAssignmentZOrder(false, false) != null)
				toolStripMenuItemNextAssignment.Enabled = true;
			else
				toolStripMenuItemNextAssignment.Enabled = false;

			ToolStripMenuItemCreateMeeting.Enabled = toolStripMenuItemDeleteMeeting.Enabled = toolStripMenuItemRemoveParticipant.Enabled = isPermittedToEditMeeting();
			toolStripMenuItemMeetingOrganizer.Enabled = toolStripMenuItemEditMeeting.Enabled = isPermittedToViewMeeting();

			toolStripMenuItemWriteProtectSchedule.Enabled = toolStripMenuItemWriteProtectSchedule2.Enabled = isPermittedToWriteProtect();

			//ToolStripMenuItemRequests.Enabled = _scenario.DefaultScenario;

			toolStripMenuItemViewHistory.Enabled = false;
			if (_scenario.DefaultScenario)
				toolStripMenuItemViewHistory.Enabled = _isAuditingSchedules;
		}

		private static bool hasFunctionPermissionForTeams(IEnumerable<ITeam> teams, string functionPath)
		{
			var authorization = PrincipalAuthorization.Instance();
			foreach (ITeam team in teams)
			{
				if (!authorization.IsPermitted(functionPath, DateOnly.Today, team))
				{
					return false;
				}
			}
			return true;
		}

		private bool isPermittedToViewMeeting()
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.ModifyMeetings;
			return CheckPermission(functionPath);
		}
		private bool isPermittedToEditMeeting()
		{
			if (!isPermittedToViewMeeting())
				return false;
			return !_scenario.Restricted || PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
		}

		private bool isPermittedToWriteProtect()
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.SetWriteProtection;
			return CheckPermission(functionPath);
		}

		private bool CheckPermission(string functionPath)
		{
			var schedulePart = _scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, _scheduleView.ViewGrid.CurrentCell.ColIndex].CellValue as IScheduleDay;
			if (schedulePart != null)
			{
				if (schedulePart.Person.PersonPeriodCollection.Count > 0)
				{
					IList<ITeam> teams = (from personPeriod in schedulePart.Person.PersonPeriodCollection
										  where personPeriod.Period.Contains(schedulePart.DateOnlyAsPeriod.DateOnly)
										  select personPeriod.Team).ToList();
					var permission = hasFunctionPermissionForTeams(teams, functionPath);
					return permission;
				}
			}

			return hasFunctionPermissionForTeams(_temporarySelectedEntitiesFromTreeView.OfType<ITeam>(), functionPath);
		}

		private bool isPermittedToViewSchedules()
		{
			var viewSchedulesFunction = DefinedRaptorApplicationFunctionPaths.ViewSchedules;
			if (hasFunctionPermissionForTeams(_temporarySelectedEntitiesFromTreeView.OfType<ITeam>(), viewSchedulesFunction)) return true;
			return false;
		}


		private static bool isPermittedApproveRequest(IEnumerable<PersonRequestViewModel> models)
		{
			foreach (var model in models)
			{
				if (!isThisPermittedApproveRequest(model))
					return false;
			}
			return true;
		}

		private static bool isThisPermittedApproveRequest(PersonRequestViewModel model)
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Instance()).IsPermittedRequestApprove(model.RequestType);
		}

		private static bool isPermittedViewRequest()
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Instance()).IsPermittedRequestView();
		}

		private bool isViewRequestDetailsAvailable()
		{
			if (_requestView.SelectedAdapters() == null || _requestView.SelectedAdapters().Count != 1)
				return false;
			var selectedModel = _requestView.SelectedAdapters().First();
			if (!selectedModel.IsWithinSchedulePeriod)
				return false;
			if (!isPermittedViewRequest())
				return false;
			return true;
		}

		#region Virtual skill handling

		private void SkillGridMenuItemPeriodClick(object sender, EventArgs e)
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, SkillResultViewSetting.Period, toolStripButtonChartPeriodView, toolStripButtonChartMonthView, toolStripButtonChartWeekView, toolStripButtonChartDayView, toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_skillResultViewSetting = SkillResultViewSetting.Period;
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void SkillGridMenuItemMonthClick(object sender, EventArgs e)
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, SkillResultViewSetting.Month, toolStripButtonChartPeriodView, toolStripButtonChartMonthView, toolStripButtonChartWeekView, toolStripButtonChartDayView, toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_skillResultViewSetting = SkillResultViewSetting.Month;
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void SkillGridMenuItemWeekClick(object sender, EventArgs e)
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, SkillResultViewSetting.Week, toolStripButtonChartPeriodView, toolStripButtonChartMonthView, toolStripButtonChartWeekView, toolStripButtonChartDayView, toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_skillResultViewSetting = SkillResultViewSetting.Week;
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void skillGridMenuItemDay_Click(object sender, EventArgs e)
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, SkillResultViewSetting.Day, toolStripButtonChartPeriodView, toolStripButtonChartMonthView, toolStripButtonChartWeekView, toolStripButtonChartDayView, toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_skillResultViewSetting = SkillResultViewSetting.Day;
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void skillGridMenuItemIntraDay_Click(object sender, EventArgs e)
		{
			var menuUpdater = new SkillGridMenuItemUpdate(_contextMenuSkillGrid, SkillResultViewSetting.Intraday, toolStripButtonChartPeriodView, toolStripButtonChartMonthView, toolStripButtonChartWeekView, toolStripButtonChartDayView, toolStripButtonChartIntradayView);
			menuUpdater.Update();
			_skillResultViewSetting = SkillResultViewSetting.Intraday;
			_currentSelectedGridRow = null;
			drawSkillGrid();
			reloadChart();
		}

		private void skillGridMenuItem_Click(object sender, EventArgs e)
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

		private void skillGridMenuItemEdit_Click(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			var skill = (ISkill)menuItem.Tag;

			using (var skillSummery = new SkillSummary(skill, _schedulerState.SchedulingResultState.Skills))
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

		private void skillGridMenuItemDelete_Click(object sender, EventArgs e)
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
			subItem.Click += skillGridMenuItemEdit_Click;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private void enableDeleteVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items["Delete"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemDelete_Click;
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

		private void handleTabsAndMenuItemsVirtualSkill(SkillSummary skillSummary, ISkill virtualSkill, TabPageAdv tabPage, ToolStripMenuItem menuItem)
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
				return;
			}
		}

		#endregion//Virtual skill handling

		#endregion//other

		#endregion

		#region Gridevents

		private void grid_CurrentCellKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				if(!(_scheduleView is AgentRestrictionsDetailView))
				{
					GridHelper.HandleSelectAllSchedulingView((GridControl)sender);
					return;
				}
			}

			GridHelper.HandleSelectionKeys((GridControl)sender, e);
		}

		private void _currentView_viewPasteCompleted(object sender, EventArgs e)
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
				Refresh();
				return;
			}

			disableAllExceptCancelInRibbon();

			if (toolStripProgressBar1.ProgressBar == null) //Somone knows why this is null in some cases?
				toolStripProgressBar1 = new ToolStripProgressBar();

			toolStripProgressBar1.Maximum = numberOfDaysToRecalculate;
			toolStripProgressBar1.Value = 0;

			toolStripProgressBar1.Visible = true;
			toolStripStatusLabelStatus.Text = Resources.CalculatingResourcesDotDotDot;

			_backgroundWorkerResourceCalculator.WorkerReportsProgress = true;
			_backgroundWorkerRunning = true;
			_backgroundWorkerResourceCalculator.RunWorkerAsync();
		}

		private void _backgroundWorkerResourceCalculator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;

			_backgroundWorkerRunning = false;

			if (rethrowBackgroundException(e))
				return;

			toolStripProgressBar1.Visible = false;
			if (e.Cancelled)
			{
				toolStripStatusLabelStatus.Text = Resources.Cancel;
				releaseUserInterface(e.Cancelled);
				return;
			}
			toolStripStatusLabelStatus.Text = Resources.Ready;

			if (_personsToValidate.IsEmpty())
			{
				afterBackgroundWorkersCompleted(e.Cancelled);
				return;
			}

			validatePersons();
		}

		private void _backgroundWorkerResourceCalculator_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;
			toolStripProgressBar1.PerformStep();
		}

		private void _backgroundWorkerResourceCalculator_DoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			_optimizationHelperWin.ResourceCalculateMarkedDays(e, _backgroundWorkerResourceCalculator, SchedulerState.ConsiderShortBreaks, true);
		}

		private void validateAllPersons()
		{
			_personsToValidate.Clear();
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				_personsToValidate.Add(permittedPerson);
			}
			validatePersons();
		}

		private void validatePersons()
		{
			if (_backgroundWorkerRunning) return;
			disableAllExceptCancelInRibbon();
			toolStripStatusLabelStatus.Text = string.Format(CultureInfo.CurrentCulture, Resources.ValidatingPersons, _personsToValidate.Count);
			_backgroundWorkerRunning = true;
			_backgroundWorkerValidatePersons.RunWorkerAsync();
			Application.DoEvents();
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "result")]
		private void _backgroundWorkerValidatePersons_DoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			if (_scheduleView != null)
			{
				if (!_validation)
					_personsToValidate.Clear();
				_scheduleView.ValidatePersons(_personsToValidate);
			}
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.Current.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.Current.Regional.UICulture;
		}

		private void _backgroundWorkerValidatePersons_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;

			_backgroundWorkerRunning = false;

			if (rethrowBackgroundException(e))
				return;

			afterBackgroundWorkersCompleted(e.Cancelled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void afterBackgroundWorkersCompleted(bool canceled)
		{
			_personsToValidate.Clear();

			using (PerformanceOutput.ForOperation("After validate"))
			{
				updateShiftEditor();
				if (_requestView != null)
					_requestView.NeedUpdate = true;
				reloadRequestView();
				if (_currentZoomLevel == ZoomLevel.Level7)
				{
					schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions, _restrictionPersonsToReload);
					_restrictionPersonsToReload.Clear();
				}

				drawSkillGrid();
			}
			releaseUserInterface(canceled);
			if (!_scheduleOptimizerHelper.WorkShiftFinderResultHolder.LastResultIsSuccessful)
			{
				if (_optimizerOriginalPreferences.SchedulingOptions.ShowTroubleshot)
					new SchedulingResult(_scheduleOptimizerHelper.WorkShiftFinderResultHolder, true).Show(this);
				else
					ViewBase.ShowInformationMessage(this, string.Format(CultureInfo.CurrentCulture, Resources.NoOfAgentDaysCouldNotBeScheduled,
						_scheduleOptimizerHelper.WorkShiftFinderResultHolder.GetResults(false, true).Count)
						, Resources.SchedulingResult);
			}
			_scheduleOptimizerHelper.ResetWorkShiftFinderResults();
		}

		private void disableScheduleButtonsOnNonCoherentSelection(IEnumerable<IScheduleDay> selectedSchedules)
		{
			IScheduleDay scheduleDay = null;
			var sortedList = selectedSchedules.OrderBy(d => d.DateOnlyAsPeriod.DateOnly).ToList();

			foreach (var selectedSchedule in sortedList)
			{
				if (scheduleDay != null)
				{
					if (scheduleDay.DateOnlyAsPeriod.DateOnly.Equals(selectedSchedule.DateOnlyAsPeriod.DateOnly)) continue;
					if (!scheduleDay.DateOnlyAsPeriod.DateOnly.AddDays(1).Equals(selectedSchedule.DateOnlyAsPeriod.DateOnly))
					{
						toolStripSplitButtonSchedule.Enabled = false;
						break;
					}
				}

				scheduleDay = selectedSchedule;
			}
		}

		private void grid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (e.Reason == GridSelectionReason.Clear) return;
			if (_scheduleView == null) return;
			using (PerformanceOutput.ForOperation("Changing selection in view"))
			{
				if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.AutomaticScheduling))
				{
					toolStripSplitButtonSchedule.Enabled = _scheduleView.TheGrid.Selections.Count == 1;
					if (_splitterManager.ShowRestrictionView) disableScheduleButtonsOnNonCoherentSelection(_scheduleView.SelectedSchedules());
				}

				disableButtonsIfTeamLeaderMode();
				if (_scheduleView != null && (e.Reason == GridSelectionReason.SetCurrentCell || e.Reason == GridSelectionReason.MouseUp) || e.Reason == GridSelectionReason.ArrowKey)
				{
					_scheduleView.Presenter.UpdateFromEditor();
					updateShiftEditor();
					var currentCell = _scheduleView.ViewGrid.CurrentCell;
					var selectedCols = _scheduleView.ViewGrid.Model.Selections.Ranges.ActiveRange.Width;
					if (!(_scheduleView is AgentRestrictionsDetailView) && currentCell.RowIndex == 0 && selectedCols == 1 && currentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
					{
						_scheduleView.AddWholeWeekAsSelected(currentCell.RowIndex, currentCell.ColIndex);
					}
					var selectedSchedules = _scheduleView.SelectedSchedules();
					updateSelectionInfo(selectedSchedules);
					enableSwapButtons(selectedSchedules);

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

					if (selectedSchedules.Count > 0)
						_dateNavigateControl.SetSelectedDateNoInvoke(selectedSchedules[0].DateOnlyAsPeriod.DateOnly);
				}
			}
		}

		private void saveAllChartSetting()
		{
			_skillIntradayGridControl.SaveSetting();
			_skillDayGridControl.SaveSetting();
			_skillWeekGridControl.SaveSetting();
			_skillMonthGridControl.SaveSetting();
			_skillFullPeriodGridControl.SaveSetting();
		}

		private void updateShiftEditor()
		{
			if (_scheduleView == null) return;

			using (PerformanceOutput.ForOperation("Updating shift editor"))
			{
				notesEditor.LoadNote(null);
				IScheduleDay scheduleDay = _scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, _scheduleView.ViewGrid.CurrentCell.ColIndex].CellValue as IScheduleDay;

				if (scheduleDay == null)
					return;

				scheduleDay = _schedulerState.Schedules[scheduleDay.Person].ReFetch(scheduleDay);

				if (_showEditor)
					schedulePartToEditor(scheduleDay);

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

			checkModifyPermissions();
		}

		private void checkModifyPermissions()
		{
			var authorization = PrincipalAuthorization.Instance();
			ToolStripMenuItemAddActivity.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			toolStripMenuItemAddOverTime.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			toolStripMenuItemInsertAbsence.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
			toolStripMenuItemInsertDayOff.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff);
		}

		#endregion

		#region BackgroundWorkerLoadData events

		private void backgroundWorkerLoadData_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;
			toolStripProgressBar1.PerformStep();
			var status = e.UserState as string;
			if (status != null)
				toolStripStatusLabelStatus.Text = status;
		}

		private void backgroundWorkerLoadData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_backgroundWorkerRunning = false;

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
			setupContextMenuAvailTimeZones();
			zoom(ZoomLevel.Level4);
			DateOnly dateOnly = SchedulerState.RequestedPeriod.DateOnlyPeriod.StartDate;
			_scheduleView.SetSelectedDateLocal(dateOnly);
			_scheduleView.ViewPasteCompleted += _currentView_viewPasteCompleted;
			schedulerSplitters1.ElementHost1.Enabled = true;
			_splitContainerAdvMain.Visible = true;
			_grid.Cursor = Cursors.WaitCursor;
			wpfShiftEditor1.LoadFromStateHolder(_schedulerState.CommonStateHolder);
			IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
												  where ((IDeleteTag)item).IsDeleted == false
												  select item).ToList();

			((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
			_dayOffTemplate = displayList[0];
			wpfShiftEditor1.Interval = _currentSchedulingScreenSettings.EditorSnapToResolution;

			loadAbsencesMenu();
			loadShiftCategoriesMenu();
			loadDayOffMenu();
			loadScenarioMenuItems();
			loadTagsMenu();

			setupSkillTabs();

			if (schedulerSplitters1.PinnedPage != null)
				schedulerSplitters1.TabSkillData.SelectedTab = schedulerSplitters1.PinnedPage;
			toolStripStatusLabelStatus.Text = Resources.ReadyThreeDots;

			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
			{
				using (PerformanceOutput.ForOperation("Creating new RequestView"))
				{
					_requestView = new RequestView(_handlePersonRequestView1, _schedulerState, _undoRedo, SchedulerState.SchedulingResultState.AllPersonAccounts, _eventAggregator);
				}
				toolStripComboBoxExFilterDays.SelectedIndex = toolStripComboBoxExFilterDays.Items.Count - 1;
				_requestView.PropertyChanged += _requestView_PropertyChanged;
			}
			else
			{
				toolStripTabItem1.Visible = false;
			}

			_grid.VScrollPixel = false;
			_grid.HScrollPixel = false;
			_grid.Selections.Clear(true);
			var point = new Point((int)ColumnType.StartScheduleColumns, _grid.Rows.HeaderCount + 1);
			_grid.CurrentCell.MoveTo(point.Y, point.X, GridSetCurrentCellOptions.None);
			_grid.Selections.SelectRange(GridRangeInfo.Cell(point.Y, point.X), true);
			_grid.Select();
			var schedulerSortCommandSetting = _currentSchedulingScreenSettings.SortCommandSetting;
			var sortCommandMapper = new SchedulerSortCommandMapper(SchedulerState, SchedulerSortCommandSetting.NoSortCommand);
			var sortCommand = sortCommandMapper.GetCommandFromSetting(schedulerSortCommandSetting);
			_scheduleView.Sort(sortCommand);

			GridHelper.GridlockWriteProtected(_grid, LockManager);
			drawSkillGrid();
			reloadChart();
			setupRequestPresenter();
			setupRequestViewButtonStates();
			releaseUserInterface(e.Cancelled);
			ResumeLayout(true);

			Cursor = Cursors.Default;
		}

		private void setupRequestViewButtonStates()
		{
			toolStripButtonViewAllowance.Available = _budgetPermissionService.IsAllowancePermitted;
			toolStripMenuItemViewAllowance.Visible = _budgetPermissionService.IsAllowancePermitted;
			toolStripMenuItemViewAllowance.Enabled = _budgetPermissionService.IsAllowancePermitted;
		}

		private bool stateHolderExceptionOccurred(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var sourceException = e.Error as StateHolderException;
				if (sourceException == null)
					return false;

				using (var view = new SimpleExceptionHandlerView(sourceException, Resources.OpenTeleoptiCCC, sourceException.Message))
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
				var dataSourceException = e.Error as DataSourceException;
				if (dataSourceException == null)
					return false;

				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				_forceClose = true;
				Close();
				return true;
			}
			return false;
		}

		private void displayOptionsFromSetting()
		{
			SplitterManager.ShowResult = !_currentSchedulingScreenSettings.HideResult;
			toolStripButtonShowResult.Checked = !_currentSchedulingScreenSettings.HideResult;
			_showResult = !_currentSchedulingScreenSettings.HideResult;
			SplitterManager.ShowGraph = !_currentSchedulingScreenSettings.HideGraph;
			toolStripButtonShowGraph.Checked = !_currentSchedulingScreenSettings.HideGraph;
			_showGraph = !_currentSchedulingScreenSettings.HideGraph;
			SplitterManager.ShowEditor = !_currentSchedulingScreenSettings.HideEditor;
			toolStripButtonShowEditor.Checked = !_currentSchedulingScreenSettings.HideEditor;
			_showEditor = !_currentSchedulingScreenSettings.HideEditor;

			toolStripButtonShowTexts.Checked = !_currentSchedulingScreenSettings.HideRibbonTexts;
			_showRibbonTexts = !_currentSchedulingScreenSettings.HideRibbonTexts;
			if (_teamLeaderMode)
			{
				SplitterManager.ShowGraph = false;
				SplitterManager.ShowResult = false;
			}
		}

		private void _schedulerMessageBrokerHandler_SchedulesUpdatedFromBroker(object sender, EventArgs e)
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
			if (_currentZoomLevel == ZoomLevel.Level6)
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

		private void backgroundWorkerLoadData_DoWork(object sender, DoWorkEventArgs e)
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
					IMeeting meeting = _schedulerMeetingHelper.MeetingFromList(dest.Person, dest.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo), dest.PersonMeetingCollection());
					if (meeting != null)
					{
						meeting = meeting.EntityClone();
						//We don't want to work with the actual meeting, that will be a bad idea!
						IList<ITeam> meetingPersonsTeams = getDistinctTeamList(meeting);
						bool editPermission = hasFunctionPermissionForTeams(meetingPersonsTeams, DefinedRaptorApplicationFunctionPaths.ModifyMeetings) && isPermittedToEditMeeting();
						bool viewSchedulesPermission = isPermittedToViewSchedules();
						_schedulerMeetingHelper.MeetingComposerStart(meeting, _scheduleView, editPermission, viewSchedulesPermission);
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
				int rowIndex = _scheduleView.ViewGrid.CurrentCell.RowIndex;
				int colIndex = _scheduleView.ViewGrid.CurrentCell.ColIndex;
				var dest = _scheduleView.ViewGrid[rowIndex, colIndex].CellValue as IScheduleDay;

				if (dest != null)
				{
					_scheduleView.Presenter.LastUnsavedSchedulePart = dest;
					IMeeting meeting = _schedulerMeetingHelper.MeetingFromList(dest.Person, dest.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo), dest.PersonMeetingCollection());
					if (meeting != null)
						_schedulerMeetingHelper.MeetingRemove(meeting, _scheduleView);
				}
			}
		}

		private bool _updating;

		private void grid_GotFocus(object sender, EventArgs e)
		{
			if (_updating)
			{
				return;
			}
			_updating = true;
			updateRibbon(ControlType.SchedulerGridMain);

			_updating = false;
		}

		private void skillDayGridControl_GotFocus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void skillIntradayGridControl_GotFocus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void skillWeekGridControl_GotFocus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void skillMonthGridControl_GotFocus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void skillFullPeriodGridControl_GotFocus(object sender, EventArgs e)
		{
			updateRibbon(ControlType.SchedulerGridSkillData);
		}

		private void wpfShiftEditor1_ShiftUpdated(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView != null)
			{
				//Spara undan AgentDayInformatiom
				//Vid byte av cell i vy eller vid stäng eller lostfocus, så utför ändringarna.
				_scheduleView.Presenter.LastUnsavedSchedulePart = e.SchedulePart;
				//Trace.WriteLine("AgentDayInformation changed");
			}
		}

		private void wpfShiftEditor1_CommitChanges(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.UpdateFromEditor();
				if (_currentZoomLevel == ZoomLevel.Level7)
					schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions);

				if (_currentZoomLevel == ZoomLevel.Level1 && !(_scheduleView.Presenter.SortCommand is NoSortCommand))
					_scheduleView.SetSelectionFromParts(new List<IScheduleDay> { e.SchedulePart });

				updateShiftEditor();
			}
		}

		private void deleteFromSchedulePart(DeleteOption deleteOption)
		{
			if (_scheduleView != null)
			{
				if (_backgroundWorkerRunning) return;
				if (_backgroundWorkerDelete.IsBusy)
					return;

				disableAllExceptCancelInRibbon();
				var clipHandler = new ClipHandler<IScheduleDay>();
				GridHelper.GridCopySelection(_scheduleView.ViewGrid, clipHandler, true);
				var list = _scheduleView.DeleteList(clipHandler);
				IGridlockRemoverForDelete gridlockRemoverForDelete = new GridlockRemoverForDelete(_gridLockManager);
				list = gridlockRemoverForDelete.RemoveLocked(list);
				toolStripStatusLabelStatus.Text = string.Format(CultureInfo.CurrentCulture, Resources.DeletingSchedules, list.Count);
				_deleteOption = deleteOption;
				Cursor = Cursors.WaitCursor;
				_backgroundWorkerDelete.WorkerReportsProgress = true;
				_backgroundWorkerRunning = true;
				_backgroundWorkerDelete.RunWorkerAsync(list);
			}
		}

		private void _backgroundWorkerDelete_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;

			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();

			_backgroundWorkerRunning = false;

			if (rethrowBackgroundException(e))
				return;

			_grid.Refresh();
			if (e.Cancelled)
			{
				toolStripStatusLabelStatus.Text = Resources.Cancel;
				releaseUserInterface(e.Cancelled);
				return;
			}

			if (_schedulerState.SchedulingResultState.SkipResourceCalculation)
				releaseUserInterface(e.Cancelled);

			updateShiftEditor();
			RecalculateResources();
		}

		private DeleteOption _deleteOption;

		private void _backgroundWorkerDelete_DoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			var list = (IList<IScheduleDay>)e.Argument;
			_undoRedo.CreateBatch(string.Format(CultureInfo.CurrentCulture, Resources.UndoRedoDeleteSchedules, list.Count));
			var deleteService = new DeleteSchedulePartService(SchedulerState.SchedulingResultState);
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(SchedulerState.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), SchedulerState), new ScheduleTagSetter(_defaultScheduleTag));
			if (!list.IsEmpty())
			{
				deleteService.Delete(list, _deleteOption, rollbackService, _backgroundWorkerDelete, NewBusinessRuleCollection.AllForDelete(SchedulerState.SchedulingResultState));
			}

			_undoRedo.CommitBatch();
		}

		private void tabSkillData_SelectedIndexChanged(object sender, EventArgs e)
		{
			drawSkillGrid();
			reloadChart();
		}

		#endregion

		#region Chart Events

		private void gridrowInChartSetting_LineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGridRow, _gridrowInChartSettingButtons, e.Enabled);
		}

		private void gridlinesInChartSettings_LineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis, e.LineColor);
		}

		private void skillWeekGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillWeekGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillWeekGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillWeekGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void skillMonthGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillMonthGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillMonthGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillMonthGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void skillFullPeriodGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillFullPeriodGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillFullPeriodGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillFullPeriodGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void skillIntradayGridControl_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillIntradayGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillIntradayGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillIntradayGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void skillDayGridControl_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillDayGridControl.CurrentSelectedGridRow != null)
			{
				_currentSelectedGridRow = _skillDayGridControl.CurrentSelectedGridRow;
				IChartSeriesSetting chartSeriesSettings = _skillDayGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_gridrowInChartSettingButtons.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		private void toolStripButtonGridInChart_Click(object sender, EventArgs e)
		{
			reloadChart();
		}

		private static void chartControlSkillData_ChartRegionMouseEnter(object sender, ChartRegionMouseEventArgs e)
		{
			//GridChartManager.SetChartToolTip(e.Region, _chartControlSkillData);
		}

		private void chartControlSkillData_ChartRegionMouseHover(object sender, ChartRegionMouseEventArgs e)
		{
			GridChartManager.SetChartToolTip(e.Region, _chartControlSkillData);
		}

		private void chartControlSkillData_ChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = Math.Max(1, (int)GridChartManager.GetIntervalValueForChartPoint(_chartControlSkillData, e.Point));
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

		#region Cut

		private void cutAssignmentSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					var deleteOptions = new PasteOptions();
					deleteOptions.MainShift = true;
					setCutMode(deleteOptions);
					deleteInMainGrid(deleteOptions);
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut ass");
					break;
			}
		}

		private void cutAbsenceSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					var deleteOptions = new PasteOptions();
					deleteOptions.Absences = PasteAction.Replace;
					setCutMode(deleteOptions);
					deleteInMainGrid(deleteOptions);
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut abs");
					break;
			}
		}

		private void cutDayOffSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					var deleteOptions = new PasteOptions();
					deleteOptions.DayOff = true;
					setCutMode(deleteOptions);
					deleteInMainGrid(deleteOptions);
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut off");
					break;
			}
		}

		private void cutPersonalShiftSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					var deleteOptions = new PasteOptions();
					deleteOptions.PersonalShifts = true;
					setCutMode(deleteOptions);
					deleteInMainGrid(deleteOptions);
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut personal shift");
					break;
			}
		}

		private void cutSpecialSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					cutSpecial();
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut special");
					break;
			}
		}

		private void cutSwitch()
		{
			switch (_controlType)
			{
				case ControlType.SchedulerGridMain:
					var deleteOptions = new PasteOptions();
					deleteOptions.Default = true;
					setCutMode(deleteOptions);
					deleteInMainGrid(deleteOptions);
					break;
				case ControlType.SchedulerGridSkillData:
					//do nothing
					break;
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor cut");
					break;
			}
		}

		private void toolStripMenuItemCut_Click(object sender, EventArgs e)
		{
			var deleteOptions = new PasteOptions();
			deleteOptions.Default = true;
			setCutMode(deleteOptions);
			deleteInMainGrid(deleteOptions);
		}

		private void toolStripMenuItemCutSpecial2_Click(object sender, EventArgs e)
		{
			cutSpecial();
		}

		private void cutSpecial()
		{
			var options = new PasteOptions();
			bool showRestrictions = _scheduleView is RestrictionSummaryView;
			var cutSpecial = new FormClipboardSpecial(true, showRestrictions, options) { Text = Resources.CutSpecial };
			cutSpecial.ShowDialog();

			if (_scheduleView != null)
			{
				if (!cutSpecial.Cancel())
				{
					setCutMode(options);
					deleteInMainGrid(options);
				}
			}

			cutSpecial.Close();
		}

		#endregion

		#region Docking

		private void dockStateChanged(object sender, DockStateChangeEventArgs arg)
		{
			if (_dockingManager.IsFloating(_agentInfo))
			{
				var dhost = _agentInfo.Parent as DockHost;
				if (dhost != null)
				{
					var frmfloat = dhost.ParentForm as FloatingForm;
					if (frmfloat != null)
					{
						frmfloat.Opacity = 1.0;
					}
				}
			}
		}

		private void dockVisibilityChanged(object sender, DockVisibilityChangedEventArgs arg)
		{
			arg.Control.Dispose();
			Controls.Remove(arg.Control);
			_agentInfo = null;
		}

		#endregion

		#region Delete

		private void deleteSpecial()
		{
			var options = new PasteOptions();
			bool showRestrictions = _scheduleView is RestrictionSummaryView;
			using (var deleteSpecial = new FormClipboardSpecial(true, showRestrictions, options))
			{
				deleteSpecial.Text = Resources.DeleteSpecial;
				deleteSpecial.ShowDialog();

				if (_scheduleView != null)
				{
					if (!deleteSpecial.Cancel())
					{
						deleteInMainGrid(options);
					}
				}
			}
		}

		private void deleteSpecialSwitch()
		{
			switch (_controlType)
			{
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor delete special");
					break;
				case ControlType.SchedulerGridMain:
					deleteSpecial();
					break;
				case ControlType.SchedulerGridSkillData:
					//not possible
					break;
			}
		}

		private void deleteSwitch()
		{
			switch (_controlType)
			{
				case ControlType.ShiftEditor:
					clipboardMessage("ShiftEditor delete");
					break;

				case ControlType.SchedulerGridMain:
					if (_scheduleView != null)
					{
						var deleteOptions = new PasteOptions();
						deleteOptions.Default = true;
						deleteInMainGrid(deleteOptions); //, clipHandler);
					}
					break;

				case ControlType.SchedulerGridSkillData:
					//not possible
					break;
			}
		}

		#endregion//delete

		private void scheduleSelected()
		{
			if (_backgroundWorkerScheduling.IsBusy) return;

			if (_scheduleView != null)
			{
				if (_scheduleView.AllSelectedDates().Count == 0)
					return;

				_optimizerOriginalPreferences.SchedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff;
				_optimizerOriginalPreferences.SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
				IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
				try
				{
					using (var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions, daysOffPreferences,
																			_schedulerState.CommonStateHolder.ShiftCategories,
																			 false, false, _groupPagesProvider, _schedulerState.CommonStateHolder.ScheduleTagsNotDeleted, "SchedulingOptions", GetNonDeletedActivty()))
					{
						if (options.ShowDialog(this) == DialogResult.OK)
						{
							options.Refresh();
							startBackgroundScheduleWork(_backgroundWorkerScheduling, new SchedulingAndOptimizeArgument(_scheduleView.SelectedSchedules()), true);
						}
					}
				}
				catch (DataSourceException dataSourceException)
				{
					using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
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
				if (_scheduleView.AllSelectedDates().Count == 0)
					return;

				_optimizerOriginalPreferences.SchedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.HourlyStaff;
				_optimizerOriginalPreferences.SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free;
				IDaysOffPreferences daysOffPreferences = new DaysOffPreferences();
				using (var options = new SchedulingSessionPreferencesDialog(_optimizerOriginalPreferences.SchedulingOptions, daysOffPreferences, _schedulerState.CommonStateHolder.ShiftCategories,
						false, false, _groupPagesProvider, _schedulerState.CommonStateHolder.ScheduleTagsNotDeleted, "SchedulingOptionsActivities", GetNonDeletedActivty()))
				{
					if (options.ShowDialog(this) == DialogResult.OK)
					{
						_optimizerOriginalPreferences.SchedulingOptions.OnlyShiftsWhenUnderstaffed = true;
						Refresh();
						startBackgroundScheduleWork(_backgroundWorkerScheduling, new SchedulingAndOptimizeArgument(_scheduleView.SelectedSchedules()), true);
					}
				}
			}
		}

		private void startBackgroundScheduleWork(BackgroundWorker backgroundWorker, object argument, bool showProgressBar)
		{
			if (_backgroundWorkerRunning) return;

			var scheduleDays = ((SchedulingAndOptimizeArgument) argument).ScheduleDays;
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

			toolStripStatusLabelStatus.Text = string.Format(CultureInfo.CurrentCulture, Resources.SchedulingDays, selectedScheduleCount);
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

		private void _backgroundWorkerScheduling_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing)
				return;
			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();
			_backgroundWorkerRunning = false;
			if (rethrowBackgroundException(e))
				return;

			//Next line will start work on another background thread.
			//No code after next line please.
			RecalculateResources();
			//afterBackgroundWorkersCompleted(false);
		}

		private void _backgroundWorkerScheduling_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;

			if (InvokeRequired)
				BeginInvoke(new EventHandler<ProgressChangedEventArgs>(_backgroundWorkerScheduling_ProgressChanged), sender, e);
			else
			{
				if (e.UserState is TeleoptiProgressChangeMessage)
				{
					var arg = (TeleoptiProgressChangeMessage)e.UserState;
					scheduleStatusBarUpdate(arg.Message);
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

		private class SchedulingAndOptimizeArgument
		{
			public IList<IScheduleDay> ScheduleDays { get; private set; }
			public OptimizationMethod OptimizationMethod { get; set; }
			public IDaysOffPreferences DaysOffPreferences { get; set; }

			public SchedulingAndOptimizeArgument(IList<IScheduleDay> scheduleDays)
			{
				ScheduleDays = scheduleDays;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void _backgroundWorkerScheduling_DoWork(object sender, DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var scheduleDays = argument.ScheduleDays;
			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var selectedPeriod =
			new DateOnlyPeriod(
				OptimizerHelperHelper.GetStartDateInSelectedDays(scheduleDays, currentPersonTimeZone),
				OptimizerHelperHelper.GetEndDateInSelectedDays(scheduleDays, currentPersonTimeZone));
			var dateOnlyList = selectedPeriod.DayCollection();
			_schedulerState.SchedulingResultState.SkillDaysOnDateOnly(dateOnlyList);
			AdvanceLoggingService.LogSchedulingInfo(_optimizerOriginalPreferences.SchedulingOptions, scheduleDays.Select(x => x.Person).Distinct().Count(), dateOnlyList.Count(), () => runBackgroundWorkerScheduling(e));
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void runBackgroundWorkerScheduling(DoWorkEventArgs e)
		{
			setThreadCulture();
			var schedulingOptions = _optimizerOriginalPreferences.SchedulingOptions;
			schedulingOptions.DayOffTemplate = _dayOffTemplate;
			bool lastCalculationState = _schedulerState.SchedulingResultState.SkipResourceCalculation;
			_schedulerState.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
				_optimizationHelperWin.ResourceCalculateAllDays(e, null, true);

			_totalScheduled = 0;
			var argument = (SchedulingAndOptimizeArgument)e.Argument;

			turnOffCalculateMinMaxCacheIfNeeded(schedulingOptions);

			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;
			var scheduleDays = argument.ScheduleDays;

			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var selectedPeriod =
				new DateOnlyPeriod(
					OptimizerHelperHelper.GetStartDateInSelectedDays(scheduleDays, currentPersonTimeZone),
					OptimizerHelperHelper.GetEndDateInSelectedDays(scheduleDays, currentPersonTimeZone));

            IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = OptimizerHelperHelper.CreateMatrixList(scheduleDays, _schedulerState.SchedulingResultState, _container, selectedPeriod);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			var allScheduleDays = new List<IScheduleDay>();

			foreach (var scheduleMatrixPro in matrixesOfSelectedScheduleDays)
			{
				allScheduleDays.AddRange(_schedulerState.Schedules[scheduleMatrixPro.Person].ScheduledDayCollection(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod).ToList());
			}

			var allMatrixesOfSelectedPersons = OptimizerHelperHelper.CreateMatrixList(allScheduleDays, _schedulerState.SchedulingResultState, _container, selectedPeriod);

			_undoRedo.CreateBatch(Resources.UndoRedoScheduling);

			//Extend period with 10 days to handle block scheduling
			DateOnlyPeriod groupPagePeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;
            //if (schedulingOptions.UseBlockScheduling != BlockFinderType.None)
            //    groupPagePeriod = new DateOnlyPeriod(groupPagePeriod.StartDate.AddDays(-10), groupPagePeriod.EndDate.AddDays(10));

			_groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _container.Resolve<IGroupPageCreator>()
				   .CreateGroupPagePerDate(groupPagePeriod.DayCollection(), _container.Resolve<IGroupScheduleGroupPageDataProvider>(), _optimizerOriginalPreferences.SchedulingOptions.GroupPageForShiftCategoryFairness);

			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				schedulingOptions.OnlyShiftsWhenUnderstaffed = false;

				if (!schedulingOptions.UseTeamBlockPerOption)
				{
					if (schedulingOptions.UseGroupScheduling)
					{
						var allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
						_scheduleOptimizerHelper.GroupSchedule(_backgroundWorkerScheduling, scheduleDays, matrixesOfSelectedScheduleDays,
						                                       allMatrixesOfSelectedPersons, schedulingOptions,
						                                       _container.Resolve<IGroupPageHelper>(), allMatrixes);
					}
					else
					{
						var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);

						if (daysOnlyHelper.DaysOnly)
						{
							if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
								_scheduleOptimizerHelper.ScheduleSelectedPersonDays(scheduleDays, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, _backgroundWorkerScheduling, daysOnlyHelper.PreferenceOnlyOptions);

							if (schedulingOptions.RotationDaysOnly)
								_scheduleOptimizerHelper.ScheduleSelectedPersonDays(scheduleDays, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, _backgroundWorkerScheduling, daysOnlyHelper.RotationOnlyOptions);

							if (schedulingOptions.AvailabilityDaysOnly)
								_scheduleOptimizerHelper.ScheduleSelectedPersonDays(scheduleDays, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, _backgroundWorkerScheduling, daysOnlyHelper.AvailabilityOnlyOptions);

							if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly || daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
								_scheduleOptimizerHelper.ScheduleSelectedPersonDays(scheduleDays, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, _backgroundWorkerScheduling, daysOnlyHelper.NoOnlyOptions);

						}
						else
							_scheduleOptimizerHelper.ScheduleSelectedPersonDays(scheduleDays, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, _backgroundWorkerScheduling, schedulingOptions);
					}
						
				}
				else
				{
					//when the advance scheduling is required
					_groupPagePerDateHolder.GroupPersonGroupPagePerDate = _groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate;

					var allVisibleMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
					_scheduleOptimizerHelper.BlockTeamScheduleSelected(matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons,
					                                                   allVisibleMatrixes, _backgroundWorkerScheduling,
					                                                   schedulingOptions, scheduleDays);
				}

			}
			else
			{
				_scheduleOptimizerHelper.ScheduleSelectedStudents(scheduleDays, _backgroundWorkerScheduling, schedulingOptions);
			}

			//shiftcategorylimitations
			if (!_backgroundWorkerScheduling.CancellationPending)
			{
				schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
				if (schedulingOptions.UseShiftCategoryLimitations)
				{
					IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();

					if (schedulingOptions.UseGroupScheduling)
					{
						allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
					}

					_scheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays, _backgroundWorkerScheduling,
																				 _optimizationPreferences,
																				 schedulingOptions,
																				 selectedPeriod, allMatrixes);
				}
			}
			_schedulerState.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
			_undoRedo.CommitBatch();
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

		private void _backgroundWorkerOptimization_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//statusStripButtonShowOptimizationProgress.Visible = false;
			if (Disposing)
				return;
			if (_undoRedo.InUndoRedo)
				_undoRedo.CommitBatch();
			_backgroundWorkerRunning = false;
			if (rethrowBackgroundException(e))
				return;

			//Next line will start work on another background thread.
			//No code after next line please.
			RecalculateResources();
			//afterBackgroundWorkersCompleted(false);
		}

		private void _backgroundWorkerOptimization_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (Disposing)
				return;
			if (InvokeRequired)
				BeginInvoke(new EventHandler<ProgressChangedEventArgs>(_backgroundWorkerOptimization_ProgressChanged), sender, e);
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

			string statusText = string.Format(CultureInfo.CurrentCulture, Resources.SchedulingProgress, _totalScheduled, toolStripProgressBar1.Maximum);
			toolStripStatusLabelStatus.Text = statusText;
			_grid.Invalidate();
			refreshSummarySkillIfActive();
			_skillIntradayGridControl.RefreshGrid();
			_skillDayGridControl.RefreshGrid();
			_skillWeekGridControl.RefreshGrid();
			_skillMonthGridControl.RefreshGrid();
			_skillFullPeriodGridControl.RefreshGrid();
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
				if (_scheduleCounter >= _optimizerOriginalPreferences.SchedulingOptions.RefreshRate)
				{
					_grid.Invalidate();
					refreshSummarySkillIfActive();
					_skillIntradayGridControl.RefreshGrid();
					_skillDayGridControl.RefreshGrid();
					_skillWeekGridControl.RefreshGrid();
					_skillMonthGridControl.RefreshGrid();
					_skillFullPeriodGridControl.RefreshGrid();
					refreshChart();
					_scheduleCounter = 0;
				}
				if (!string.IsNullOrEmpty(progress.Message))
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
				_skillIntradayGridControl.RefreshGrid();
				_skillDayGridControl.RefreshGrid();
				_skillWeekGridControl.RefreshGrid();
				_skillMonthGridControl.RefreshGrid();
				_skillFullPeriodGridControl.RefreshGrid();
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
					toolStripStatusLabelStatus.Text = Resources.Cancel;
					return;
				}
				toolStripStatusLabelStatus.Text = Resources.Ready;
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
				LogManager.GetLogger(typeof(SchedulingScreen)).Error(ex.ToString());
			}
		}

		private void _backgroundWorkerOptimization_DoWork(object sender, DoWorkEventArgs e)
		{
			var argument = (SchedulingAndOptimizeArgument)e.Argument;
			var scheduleDays = argument.ScheduleDays;
			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var selectedPeriod =
				new DateOnlyPeriod(
					OptimizerHelperHelper.GetStartDateInSelectedDays(scheduleDays, currentPersonTimeZone),
					OptimizerHelperHelper.GetEndDateInSelectedDays(scheduleDays, currentPersonTimeZone));
			var dateOnlyList = selectedPeriod.DayCollection();
			_schedulerState.SchedulingResultState.SkillDaysOnDateOnly(dateOnlyList);
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			AdvanceLoggingService.LogOptimizationInfo(optimizerPreferences, scheduleDays.Select(x => x.Person).Distinct().Count(), dateOnlyList.Count(), () => runBackgroupWorkerOptimization(e));
			
		}

		private void runBackgroupWorkerOptimization(DoWorkEventArgs e)
		{
			setThreadCulture();
			var options = (SchedulingAndOptimizeArgument)e.Argument;
			_undoRedo.CreateBatch(Resources.UndoRedoReOptimize);

			bool lastCalculationState = _schedulerState.SchedulingResultState.SkipResourceCalculation;
			_schedulerState.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
				_optimizationHelperWin.ResourceCalculateAllDays(e, null, true);
			var selectedSchedules = options.ScheduleDays;
			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var selectedPeriod =
				new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedSchedules, currentPersonTimeZone),
				                   OptimizerHelperHelper.GetEndDateInSelectedDays(selectedSchedules, currentPersonTimeZone));
			var scheduleMatrixOriginalStateContainers = _scheduleOptimizerHelper.CreateScheduleMatrixOriginalStateContainers(selectedSchedules, selectedPeriod);
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			DateOnlyPeriod groupPagePeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;

			IGroupPageLight selectedGroupPage;
			// ***** temporary cope
			if (options.OptimizationMethod == OptimizationMethod.BackToLegalState)
			{
				selectedGroupPage = _optimizerOriginalPreferences.SchedulingOptions.GroupPageForShiftCategoryFairness;
			}
			else
			{
				selectedGroupPage = _optimizationPreferences.Extra.GroupPageOnCompareWith;
			}

			_groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _container.Resolve<IGroupPageCreator>().CreateGroupPagePerDate(groupPagePeriod.DayCollection(), _container.Resolve<IGroupScheduleGroupPageDataProvider>(), selectedGroupPage);
			

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
			turnOffCalculateMinMaxCacheIfNeeded(schedulingOptions);
			IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
			switch (options.OptimizationMethod)
			{

				case OptimizationMethod.BackToLegalState:
					IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
					                                      where ((IDeleteTag) item).IsDeleted == false
					                                      select item).ToList();
					_scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
					                                                 _backgroundWorkerOptimization, displayList[0], false,
					                                                 _optimizerOriginalPreferences.SchedulingOptions,
					                                                 options.DaysOffPreferences);
					_optimizationHelperWin.ResourceCalculateMarkedDays(e, null,
					                                                   _optimizerOriginalPreferences.SchedulingOptions
					                                                                                .ConsiderShortBreaks, true);
					IList<IScheduleMatrixPro> matrixList = OptimizerHelperHelper.CreateMatrixList(selectedSchedules,
					                                                                              _schedulerState.SchedulingResultState,
					                                                                              _container, selectedPeriod);


					if (optimizerPreferences.Extra.UseTeams)
					{
						allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
					}

					_scheduleOptimizerHelper.GetBackToLegalState(matrixList, _schedulerState, _backgroundWorkerOptimization,
					                                             _optimizerOriginalPreferences.SchedulingOptions, selectedPeriod,
					                                             allMatrixes);
					break;
				case OptimizationMethod.ReOptimize:
					if (optimizerPreferences.Extra.UseTeams)
					{
						allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
						_groupDayOffOptimizerHelper.ReOptimize(_backgroundWorkerOptimization, selectedSchedules, allMatrixes);
						break;
					}

					if (optimizerPreferences.Extra.UseTeamBlockOption)
					{
						IList<IPerson> selectedPersons =
							new PersonListExtractorFromScheduleParts(selectedSchedules).ExtractPersons().ToList();
						_groupDayOffOptimizerHelper.TeamGroupReOptimize(_backgroundWorkerOptimization, selectedPeriod, selectedPersons,
						                                                _container.Resolve<IOptimizationPreferences>());
						break;
					}

					// we need it here for fairness opt. for example
					_groupPagePerDateHolder.GroupPersonGroupPagePerDate = _groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate;
					_scheduleOptimizerHelper.ReOptimize(_backgroundWorkerOptimization, selectedSchedules);

					break;
			}

			_undoRedo.CommitBatch();
			_schedulerState.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private void checkCutMode()
		{
			if (ClipsHandlerSchedule.IsInCutMode)
			{
				ClipsHandlerSchedule.IsInCutMode = false;
			}
		}

		private void setCutMode(PasteOptions cutMode)
		{
			if (_scheduleView != null)
			{
				_scheduleView.GridClipboardCopy(true);
				ClipsHandlerSchedule.IsInCutMode = true;
				ClipsHandlerSchedule.CutMode = cutMode;
				checkPastePermissions();
			}
			else
				ClipsHandlerSchedule.IsInCutMode = false;
		}

		private void setPermissionOnControls()
		{
			setPermissionOnClipboardControl();
			setPermissionOnEditControl();
			setPermissionOnContextMenuItems();
			setPermissionOnMenuButtons();
			setPermissionOnScheduleControl();
		}

		private void checkPastePermissions()
		{
			bool permitted = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			toolStripMenuItemPaste.Enabled = permitted;
			toolStripMenuItemPasteSpecial.Enabled = permitted;
		}

		private void setPermissionOnScheduleControl()
		{
			toolStripExActions.Enabled = true;
			toolStripSplitButtonSchedule.Enabled = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.AutomaticScheduling);
			if (_scheduleView != null)
				enableSwapButtons(_scheduleView.SelectedSchedules());
		}

		private void setPermissionOnContextMenuItems()
		{
			var authorization = PrincipalAuthorization.Instance();
			toolStripMenuItemInsertAbsence.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
			toolStripMenuItemInsertDayOff.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff);
			toolStripMenuItemDelete.Enabled = toolStripMenuItemDeleteSpecial.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			toolStripMenuItemWriteProtectSchedule.Enabled = toolStripMenuItemWriteProtectSchedule2.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			toolStripMenuItemAddStudentAvailabilityRestriction.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			toolStripMenuItemAddStudentAvailability.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			toolStripMenuItemAddPreferenceRestriction.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			toolStripMenuItemAddPreference.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			//reports
			toolStripMenuItemViewReport.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports);
			toolStripMenuItemScheduledTimePerActivity.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport);
		}

		private void setPermissionOnMenuButtons()
		{
			var authorization = PrincipalAuthorization.Instance();
			toolStripButtonRequestView.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler);
			toolStripButtonOptions.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
		}

		private void loadAndOptimizeData(DoWorkEventArgs e)
		{
			IList<LoaderMethod> methods = new List<LoaderMethod>();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IPeopleAndSkillLoaderDecider decider;
				if (_teamLeaderMode)
				{
					decider = new PeopleAndSkillLoaderDeciderForTeamLeaderMode();
				}
				else
				{
					decider = new PeopleAndSkillLoaderDecider(new PersonRepository(uow));
				}
				methods.Add(new LoaderMethod(loadCommonStateHolder, Resources.LoadingDataTreeDots));
				methods.Add(new LoaderMethod(loadSkills, null));
				methods.Add(new LoaderMethod(loadSettings, null));
				methods.Add(new LoaderMethod(loadAuditingSettings, null));
				methods.Add(new LoaderMethod(loadPeople, Resources.LoadingPeopleTreeDots));
				methods.Add(new LoaderMethod(filteringPeopleAndSkills, null));
				methods.Add(new LoaderMethod(loadSchedules, Resources.LoadingSchedulesTreeDots));
				methods.Add(new LoaderMethod(loadRequests, null));
				methods.Add(new LoaderMethod(loadSkillDays, Resources.LoadingSkillDataTreeDots));
				methods.Add(new LoaderMethod(loadDefinitionSets, null));
				methods.Add(new LoaderMethod(loadContractSchedule, null));
				methods.Add(new LoaderMethod(loadAccounts, null));
				methods.Add(new LoaderMethod(loadWorkflowControlSets, null));

				using (PerformanceOutput.ForOperation("Loading all data for scheduler"))
				{
					foreach (var method in methods)
					{
						backgroundWorkerLoadData.ReportProgress(1, method.StatusStripString);
						method.Action.Invoke(uow, SchedulerState, decider);
						if (backgroundWorkerLoadData.CancellationPending)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				var period = new ScheduleDateTimePeriod(SchedulerState.RequestedPeriod.Period(), SchedulerState.SchedulingResultState.PersonsInOrganization);
				ISchedulingOptions options = new SchedulingOptions();
				OptimizerHelperHelper.SetConsiderShortBreaks(SchedulerState.SchedulingResultState.PersonsInOrganization, SchedulerState.RequestedPeriod.DateOnlyPeriod, options, _container);
				SchedulerState.ConsiderShortBreaks = options.ConsiderShortBreaks;
				initMessageBroker(period.LoadedPeriod());
			}

        	_singleSkillDictionary = _container.Resolve<ISingleSkillDictionary>();
			_singleSkillDictionary.Create(SchedulerState.SchedulingResultState.PersonsInOrganization.ToList(), SchedulerState.RequestedPeriod.DateOnlyPeriod);

            _optimizationHelperWin = new ResourceOptimizationHelperWin(SchedulerState, _singleSkillDictionary);
			_scheduleOptimizerHelper = new ScheduleOptimizerHelper(_container);

			_groupDayOffOptimizerHelper = new GroupDayOffOptimizerHelper(_container);
			//_blockOptimizerHelper = new BlockOptimizerHelper(_container, _scheduleOptimizerHelper);

			if (!_schedulerState.SchedulingResultState.SkipResourceCalculation)
				backgroundWorkerLoadData.ReportProgress(1, Resources.CalculatingResourcesDotDotDot);
			_optimizationHelperWin.ResourceCalculateAllDays(e, backgroundWorkerLoadData, true);

			if (e.Cancel)
				return;

			_schedulerState.ClearDaysToRecalculate();

			if (_validation)
				validation();

			////TODO move into the else clause above
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				validatePersonAccounts(permittedPerson);
			}
			SchedulerState.SchedulingResultState.Schedules.ModifiedPersonAccounts.Clear();
			permittedPersonsToSelectedList();

			foreach (var tag in _schedulerState.CommonStateHolder.ScheduleTagsNotDeleted)
			{
				if (tag.Id != _currentSchedulingScreenSettings.DefaultScheduleTag) continue;
				_defaultScheduleTag = tag;
				break;
			}
		}

		private void createMaxSeatSkills(ISkillDayRepository skillDayRepository)
		{
			var extendedPeriod = new DateOnlyPeriod(SchedulerState.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8), SchedulerState.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8));
			var maxSeatSitesExtractor = new MaxSeatSitesExtractor(SchedulerState.AllPermittedPersons);
			var createSkillsFromMaxSeatSites = new CreateSkillsFromMaxSeatSites(SchedulerState.SchedulingResultState);
			var schedulerSkillDayHelper = new SchedulerSkillDayHelper(SchedulerState.SchedulingResultState, extendedPeriod, skillDayRepository, SchedulerState.RequestedScenario);
			var createPersonalSkillsFromMaxSeatSites = new CreatePersonalSkillsFromMaxSeatSites();
			var maxSeatSkillCreator = new MaxSeatSkillCreator(maxSeatSitesExtractor, createSkillsFromMaxSeatSites, createPersonalSkillsFromMaxSeatSites, schedulerSkillDayHelper, SchedulerState.SchedulingResultState.PersonsInOrganization);
			maxSeatSkillCreator.CreateMaxSeatSkills(SchedulerState.RequestedPeriod.DateOnlyPeriod);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void nonBlendSkills()
		{
			if (_scheduleView == null) return;
			var selectedDates = _scheduleView.AllSelectedDates();
			if (selectedDates.Count == 0) return;
			IActivity selectedActivity;
			int demand;
			using (var options = new TempNonBlendSchedulingPreferences(_optimizerOriginalPreferences.SchedulingOptions, _groupPagesProvider, SchedulerState.CommonStateHolder.Activities))
			{
				options.ShowDialog();
				selectedActivity = options.SelectedActivity();
				demand = options.Demand();
			}
			ISkillDayRepository skillDayRepository = new SkillDayRepository(UnitOfWorkFactory.Current);
			var extendedPeriod = new DateOnlyPeriod(SchedulerState.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8), SchedulerState.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8));
			var schedulerSkillDayHelper = new SchedulerSkillDayHelper(SchedulerState.SchedulingResultState, extendedPeriod, skillDayRepository, SchedulerState.RequestedScenario);
			var schedulerHelper = new SchedulerSkillHelper(schedulerSkillDayHelper);
			var nonBlendPersonSkillFromGroupingCreator = new NonBlendPersonSkillFromGroupingCreator();
			var nonBlendSkillFromGroupingCreator = new NonBlendSkillFromGroupingCreator(SchedulerState.SchedulingResultState, nonBlendPersonSkillFromGroupingCreator, selectedActivity);
			IGroupPageDataProvider groupPageDataProvider = new GroupScheduleGroupPageDataProvider(_schedulerState, new RepositoryFactory(), UnitOfWorkFactory.Current);
			schedulerHelper.CreateNonBlendSkillsFromGrouping(groupPageDataProvider, _optimizerOriginalPreferences.SchedulingOptions.GroupOnGroupPage, selectedDates.First(), nonBlendSkillFromGroupingCreator, demand);

			foreach (ISkill skill in _schedulerState.SchedulingResultState.VisibleSkills.OrderBy(s => s.Name))
			{
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
					continue;

				TabPageAdv tab = ColorHelper.CreateTabPage(skill.Name, skill.Description);
				tab.Tag = skill;
				tab.ImageIndex = GuiHelper.ImageIndexSkillType(skill.SkillType.ForecastSource);

				_tabSkillData.TabPages.Add(tab);
			}
		}

		private IBusinessRuleResponse validatePersonAccounts(IPerson person)
		{
			IScheduleRange range = SchedulerState.SchedulingResultState.Schedules[person];
			var rule = new NewPersonAccountRule(SchedulerState.SchedulingResultState, SchedulerState.SchedulingResultState.AllPersonAccounts);
			IList<IBusinessRuleResponse> toRemove = new List<IBusinessRuleResponse>();
			IList<IBusinessRuleResponse> exposedBusinessRuleResponseCollection = ((ScheduleRange)range).ExposedBusinessRuleResponseCollection();
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

			return null;
		}

		private void validation()
		{
			backgroundWorkerLoadData.ReportProgress(1, string.Format(CultureInfo.CurrentCulture, Resources.ValidatingPersons, SchedulerState.AllPermittedPersons.Count));
			_personsToValidate.Clear();
			foreach (IPerson permittedPerson in SchedulerState.AllPermittedPersons)
			{
				_personsToValidate.Add(permittedPerson);
			}
			_schedulerState.Schedules.ValidateBusinessRulesOnPersons(_personsToValidate, TeleoptiPrincipal.Current.Regional.Culture, _schedulerState.SchedulingResultState.GetRulesToRun());
			_personsToValidate.Clear();
		}

		private void setupRequestPresenter()
		{
			_handleBusinessRuleResponse = new HandleBusinessRuleResponse();
			_requestPresenter = new RequestPresenter(_personRequestAuthorizationChecker);
			_requestPresenter.SetUndoRedoContainer(_undoRedo);
		}

		private void loadAccounts(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			var rep = new PersonAbsenceAccountRepository(uow);
			SchedulerState.SchedulingResultState.AllPersonAccounts = rep.LoadAllAccounts();
		}

		private void loadDefinitionSets(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(uow);
			MultiplicatorDefinitionSet = multiplicatorDefinitionSetRepository.FindAllDefinitions();
		}
		
		private void loadWorkflowControlSets(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			IWorkflowControlSetRepository workflowControlSetRepository = new WorkflowControlSetRepository(uow);
			WorkflowControlSets = workflowControlSetRepository.LoadAllSortByName();	
		}

		private void filteringPeopleAndSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			using (PerformanceOutput.ForOperation("Executing and filtering loader decider"))
			{
				ICollection<IPerson> peopleInOrg = SchedulerState.SchedulingResultState.PersonsInOrganization;
				int peopleCountFromBeginning = peopleInOrg.Count;
				decider.Execute(_schedulerState.RequestedScenario, _schedulerState.RequestedPeriod.Period(), SchedulerState.AllPermittedPersons);
				int removedPeople = decider.FilterPeople(peopleInOrg);
				Log.Info("Removed " + removedPeople + " people when filtering (original: " + peopleCountFromBeginning +
						 ")");

				//RK: jag tycker detta är fel, men det rättar en bugg för nu. 
				//Filtereringen gör rätt utan detta, men jag vet inte vilken lista som egentligen används för
				//visning, db-sparning, resursberäkning, visning etc. Så det blir såhär tills större häv

				peopleInOrg = new HashSet<IPerson>(peopleInOrg);
				SchedulerState.AllPermittedPersons.ForEach(peopleInOrg.Add);
				SchedulerState.SchedulingResultState.PersonsInOrganization = peopleInOrg;
				Log.Info("No, changed my mind... Removed " + (peopleCountFromBeginning - peopleInOrg.Count) + " people.");
				ICollection<ISkill> skills = stateHolder.SchedulingResultState.Skills;
				int orgSkills = skills.Count;
				int removedSkills = decider.FilterSkills(skills);
				Log.Info("Removed " + removedSkills + " skill when filtering (original: " + orgSkills + ")");
			}
		}

		private static void loadSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			ICollection<ISkill> skills = new SkillRepository(uow).FindAllWithSkillDays(stateHolder.RequestedPeriod.DateOnlyPeriod);
			foreach (ISkill skill in skills)
			{
				stateHolder.SchedulingResultState.Skills.Add(skill);
			}
		}

		private static void loadContractSchedule(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			using (PerformanceOutput.ForOperation("Loading contract schedule"))
			{
				new ContractScheduleRepository(uow).LoadAllAggregate();
			}
		}

		private void loadSettings(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			using (PerformanceOutput.ForOperation("Loading settings"))
			{
				_schedulerState.LoadSettings(uow, new RepositoryFactory());
			}
		}

		private void loadAuditingSettings(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			var repository = new AuditSettingRepository(uow);
			var auditSetting = repository.Read();
			_isAuditingSchedules = auditSetting.IsScheduleEnabled;
		}

		private void loadSchedules(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			var period = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(), stateHolder.SchedulingResultState.PersonsInOrganization);
			using (PerformanceOutput.ForOperation("Loading schedules " + period.LoadedPeriod()))
			{
				IPersonProvider personsInOrganizationProvider = new PersonsInOrganizationProvider(stateHolder.SchedulingResultState.PersonsInOrganization);
				// If the people in organization is filtered out to 70% or less of all people then flag 
				// so that a criteria for that is used later when loading schedules.
				var loaderSpecification = new LoadScheduleByPersonSpecification();
				personsInOrganizationProvider.DoLoadByPerson = loaderSpecification.IsSatisfiedBy(decider);
				IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
				stateHolder.LoadSchedules(new ScheduleRepository(uow), personsInOrganizationProvider, scheduleDictionaryLoadOptions, period);
				_schedulerState.Schedules.SetUndoRedoContainer(_undoRedo);
			}
			SchedulerState.Schedules.PartModified += _schedules_PartModified;
		}

		private void initMessageBroker(DateTimePeriod period)
		{
			_schedulerMessageBrokerHandler.Listen(period);
			_schedulerMessageBrokerHandler.RequestDeletedFromBroker += _schedulerMessageBrokerHandler_RequestDeletedFromBroker;
			_schedulerMessageBrokerHandler.RequestInsertedFromBroker += _schedulerMessageBrokerHandler_RequestInsertedFromBroker;
			_schedulerMessageBrokerHandler.SchedulesUpdatedFromBroker += _schedulerMessageBrokerHandler_SchedulesUpdatedFromBroker;
		}

		private void _schedulerMessageBrokerHandler_RequestInsertedFromBroker(object sender, CustomEventArgs<IPersonRequest> e)
		{
			IPersonRequest personRequestInserted = e.Value;
			if (_requestView != null && personRequestInserted != null)
			{
				_requestView.InsertPersonRequestViewModel(personRequestInserted);
			}
		}

		private void _schedulerMessageBrokerHandler_RequestDeletedFromBroker(object sender, CustomEventArgs<IPersonRequest> e)
		{
			IPersonRequest personRequestDeleted = e.Value;
			if (_requestView != null && personRequestDeleted != null)
			{
				_requestView.DeletePersonRequestViewModel(personRequestDeleted);
			}
		}

		private void _schedules_PartModified(object sender, ModifyEventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<ModifyEventArgs>(_schedules_PartModified), sender, e);
			}
			else
			{
				if (IsDisposed)
					return;

				
				if(_selectedPeriod.Contains(e.ModifiedPeriod))
					_totalScheduled++;

				var localDate = new DateOnly(e.ModifiedPeriod.StartDateTimeLocal(_schedulerState.TimeZoneInfo));

				if (e.Modifier != ScheduleModifier.Scheduler)
				{
					_schedulerState.MarkDateToBeRecalculated(localDate);
					//add date after for night shifts
					_schedulerState.MarkDateToBeRecalculated(localDate.AddDays(1));
				}

				_restrictionPersonsToReload.Add(e.ModifiedPerson);

				if (e.Modifier == ScheduleModifier.UndoRedo)
					_personsToValidate.Add(e.ModifiedPerson);

				_lastModifiedPart = e;

				if (!_backgroundWorkerRunning)
				{
					if (_scheduleView != null)
						_scheduleView.RefreshRangeForAgentPeriod(e.ModifiedPerson, e.ModifiedPeriod);
					if (e.Modifier == ScheduleModifier.UndoRedo)
					{
						selectCellFromPersonDate(e.ModifiedPerson, localDate);
					}
					if (e.Modifier != ScheduleModifier.MessageBroker)
						enableSave();
					if (_scheduleView != null && _scheduleView.SelectedSchedules().Count == 1)
						updateShiftEditor();
				}
			}
		}

		private void loadPeople(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			using (PerformanceOutput.ForOperation("Loading people"))
			{
				SchedulerState.SchedulingResultState.OptionalColumns = new OptionalColumnRepository(uow).GetOptionalColumns<Person>();
				var personRep = new PersonRepository(uow);
				IPeopleLoader loader;
				if (_teamLeaderMode)
				{
					loader = new PeopleLoaderForTeamLeaderMode(uow, SchedulerState, new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView, _schedulerState.RequestedPeriod.DateOnlyPeriod), new RepositoryFactory());
				}
				else
				{
					loader = new PeopleLoader(personRep, new ContractRepository(uow), SchedulerState, new SelectedEntitiesForPeriod(_temporarySelectedEntitiesFromTreeView, _schedulerState.RequestedPeriod.DateOnlyPeriod), new SkillRepository(uow));
				}

				loader.Initialize();
			}
			// part of the workaround because we can't press cancel before this / Ola
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, true);
		}

		private void loadRequests(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			using (PerformanceOutput.ForOperation("Loading requests"))
			{
				stateHolder.LoadPersonRequests(uow, new RepositoryFactory(), _personRequestAuthorizationChecker);
			}
		}

		private static void loadCommonStateHolder(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			stateHolder.LoadCommonState(uow, new RepositoryFactory());
			if (stateHolder.CommonStateHolder.DayOffs.Count == 0)
				throw new StateHolderException("You must create at least one Day Off in Options!");
		}

		private void disableSave()
		{
			toolStripButtonMainMenuSave.Enabled = false;
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessSave, false);
		}

		private void enableSave()
		{
			toolStripButtonMainMenuSave.Enabled = true;
			toggleQuickButtonEnabledState(toolStripButtonQuickAccessSave, true);
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

		private bool save()
		{
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
				return true;
			}
			catch (TooManyActiveAgentsException e)
			{
				string explanation;
				if (e.LicenseType.Equals(LicenseType.Seat))
				{
					explanation = String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats, e.NumberOfLicensed);
				}
				else
				{
					explanation = String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents, e.NumberOfAttemptedActiveAgents, e.NumberOfLicensed);
				}
				ShowErrorMessage(explanation, Resources.ErrorMessage);
				return false;
			}
			catch (DataSourceException dataSourceException)
			{
				//rk - dont like this but cannot easily find "the spot" to catch these exception in current design
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
				return false;
			}
		}

		private void doSaveProcess()
		{
			var refreshResult = refreshEntitiesUsingMessageBroker();
			if (refreshResult.ConflictsFound)
			{
				if (refreshResult.DialogResult == PersistConflictDialogResult.OK)
					showPleaseSaveAgainDialog();
			}
			else
			{
				//if conflicts don't exist - do the saving
				Cursor = Cursors.WaitCursor;
				using (PerformanceOutput.ForOperation("Persisting changes"))
				{
					using (new DenormalizerContext(new SendDenormalizeNotificationToSdk(_container.Resolve<ISendCommandToSdk>())))
					{
						_personAbsenceAccountPersistValidationBusinessRuleResponses.Clear();
						var result = _persister.TryPersist(_schedulerState.Schedules, _modifiedWriteProtections, _schedulerState.PersonRequests, _schedulerState.Schedules.ModifiedPersonAccounts);
						if (result.ScheduleDictionaryConflicts != null && result.ScheduleDictionaryConflicts.Any())
						{
							var conflictHandlingResult = handleConflicts(result.ScheduleDictionaryConflicts);
							if (conflictHandlingResult.DialogResult == PersistConflictDialogResult.Overwrite)
								doSaveProcess();
							if (conflictHandlingResult.DialogResult == PersistConflictDialogResult.OK)
								showPleaseSaveAgainDialog();
							return;
						}
						if (!result.Saved)
						{
							appologizeAndClose();
							return;
						}
						if (_personAbsenceAccountPersistValidationBusinessRuleResponses.Any())
						{
							BusinessRuleResponseDialog.ShowDialogFromWinForms(_personAbsenceAccountPersistValidationBusinessRuleResponses);
						}
					}
				}
				_undoRedo.Clear();
				enableUndoRedoButtons();
				Cursor = Cursors.Default;
			}
			updateRequestCommandsAvailability();
			updateShiftEditor();
			RecalculateResources();
		}

		private void showPleaseSaveAgainDialog()
		{
			ShowInformationMessage(Resources.PleaseSaveAgainWhenYouHaveReviewedTheAppliedChanges, Resources.NotSaved);
		}

		private void appologizeAndClose()
		{
			ShowInformationMessage(Resources.SorryForTheInconvenianceButSchedulerHasToCloseNowEtc, "  ");
			_forceClose = true;
			Close();
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
				toolStripButtonSystemExit.Enabled = false;
				toolStripTabItemHome.Panel.Enabled = false;
				toolStripTabItemChart.Panel.Enabled = false;
				toolStripTabItem1.Panel.Enabled = false;
				toolStripMenuItemQuickAccessUndo.ShortcutKeys = Keys.None;
				ControlBox = false;
				toggleQuickButtonEnabledState(false);
				toolStripButtonMainMenuClose.Enabled = false;
				toolStripButtonMainMenuSave.Enabled = false;
				contextMenuViews.Enabled = false;
				toolStripButtonShrinkage.Enabled = false;
				toolStripButtonValidation.Enabled = false;
				toolStripButtonCalculation.Enabled = false;
				_grid.Cursor = Cursors.WaitCursor;
				_grid.Enabled = false;
				_grid.Cursor = Cursors.WaitCursor;
				schedulerSplitters1.ElementHost1.Enabled = false; //shifteditor
				toggleQuickButtonEnabledState(toolStripButtonQuickAccessCancel, true);
				ribbonControlAdv1.Cursor = Cursors.AppStarting;
				if (toolStripSpinningProgressControl1.SpinningProgressControl == null)
					toolStripSpinningProgressControl1 = new Common.Controls.SpinningProgress.ToolStripSpinningProgressControl();
				toolStripSpinningProgressControl1.SpinningProgressControl.Enabled = true;
				disableSave();
			}
		}

		private void enableAllExceptCancelInRibbon()
		{
			_uIEnabled = true;
			toolStripButtonSystemExit.Enabled = true;
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
			toolStripButtonMainMenuClose.Enabled = true;
			enableUndoRedoButtons();
			ribbonControlAdv1.Cursor = Cursors.Default;
			updateRibbon(ControlType.SchedulerGridMain);
			//av nån #%¤#¤%#¤% anledning tänds alla knappar i toggleQuick... ovan. Måste explicit tända/släcka igen.
			_schedulerMessageBrokerHandler.NotifyMessageQueueSize();
			disableButtonsIfTeamLeaderMode();
		}

		private void disableButtonsIfTeamLeaderMode()
		{
			if (!_teamLeaderMode) return;
			toolStripButtonShowGraph.Enabled = false;
			toolStripButtonShowResult.Enabled = false;
			toolStripButtonShrinkage.Enabled = false;
			toolStripButtonCalculation.Enabled = false;
			toolStripMenuItemSwapAndReschedule.Enabled = false;
			toolStripSplitButtonSchedule.Enabled = false;
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
			toolStripTabItem1.Visible = value;
			if (value)
			{
				updateRequestCommandsAvailability();
			}
			else
			{
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
			if (toolStripButtonChartIntradayView != null) toolStripButtonChartIntradayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday);
			if (toolStripButtonChartDayView != null) toolStripButtonChartDayView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day);
			if (toolStripButtonChartPeriodView != null) toolStripButtonChartPeriodView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period);
			if (toolStripButtonChartMonthView != null) toolStripButtonChartMonthView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month);
			if (toolStripButtonChartWeekView != null) toolStripButtonChartWeekView.Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week);
		}

		private void setupContextMenuSkillGrid()
		{
			var skillGridMenuItem = new ToolStripMenuItem(Resources.Period) { Name = "Period", Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Period) };
			skillGridMenuItem.Click += SkillGridMenuItemPeriodClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Month) { Name = "Month", Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Month) };
			skillGridMenuItem.Click += SkillGridMenuItemMonthClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Week) { Name = "Week", Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Week) };
			skillGridMenuItem.Click += SkillGridMenuItemWeekClick;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Day) { Name = "Day", Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Day) };
			skillGridMenuItem.Click += skillGridMenuItemDay_Click;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Intraday) { Name = "Intraday", Checked = _skillResultViewSetting.Equals(SkillResultViewSetting.Intraday) };
			skillGridMenuItem.Click += skillGridMenuItemIntraDay_Click;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.UseShrinkage);
			skillGridMenuItem.Click += toolStripMenuItemUseShrinkage_Click;
			skillGridMenuItem.Checked = _shrinkage;
			skillGridMenuItem.Name = "UseShrinkage";
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			var skillGridMenuSeparator = new ToolStripSeparator();
			_contextMenuSkillGrid.Items.Add(skillGridMenuSeparator);
			skillGridMenuItem = new ToolStripMenuItem(Resources.CreateSkillSummery);
			skillGridMenuItem.Click += skillGridMenuItem_Click;
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
            skillGridMenuItem = new ToolStripMenuItem(Resources.EditSkillSummery) {Name = "Edit", Enabled = false};	
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			skillGridMenuItem = new ToolStripMenuItem(Resources.DeleteSkillSummery) { Name = "Delete", Enabled = false };
			_contextMenuSkillGrid.Items.Add(skillGridMenuItem);
			_skillDayGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillIntradayGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillWeekGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillMonthGridControl.ContextMenuStrip = _contextMenuSkillGrid;
			_skillFullPeriodGridControl.ContextMenuStrip = _contextMenuSkillGrid;
		}

		private void setUpZomMenu()
		{
			toolStripButtonDayView.Tag = ZoomLevel.Level1;
			toolStripButtonWeekView.Tag = ZoomLevel.Level2;
            toolStripButtonFilterAgents.Tag = ZoomLevel.Level3;
			toolStripButtonPeriodView.Tag = ZoomLevel.Level4;
			toolStripButtonSummaryView.Tag = ZoomLevel.Level5;
			toolStripButtonRequestView.Tag = ZoomLevel.Level6;
			toolStripButtonRestrictions.Tag = ZoomLevel.Level7;
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
			_gridChartManager = new GridChartManager(_chartControlSkillData, true, true, true);
			_gridChartManager.Create();
			ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdv1);
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
			foreach (ISkill virtualSkill in _virtualSkillHelper.LoadVirtualSkills(_schedulerState.SchedulingResultState.VisibleSkills).OrderBy(s => s.Name))
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

		private PersonsFilterView _cachedPersonsFilterView;
		private PersonsFilterView getCachedPersonsFilterView()
		{
			if (_cachedPersonsFilterView == null || _cachedPersonsFilterView.IsDisposed)
			{
				var permittedPersons = SchedulerState.AllPermittedPersons.Select(p => p.Id.Value).ToList();

				_cachedPersonsFilterView =
					new PersonsFilterView(SchedulerState.RequestedPeriod.DateOnlyPeriod, 
										  SchedulerState.FilteredPersonDictionary.Keys,
										  _container,
										  ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory()
											.ApplicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage),
										  string.Empty, 
										  permittedPersons);
			}
			return _cachedPersonsFilterView;
		}

		private void showFilterDialog()
		{
			var scheduleFilterView = getCachedPersonsFilterView();

			scheduleFilterView.StartPosition = FormStartPosition.Manual;

			//TODO: Please come up with a better solution!
			Point pointToScreen = toolStripExScheduleViews.PointToScreen(
				new Point(toolStripButtonFilterAgents.Bounds.X + 63, toolStripButtonFilterAgents.Bounds.Y +
				toolStripButtonFilterAgents.Height));
			scheduleFilterView.Location = pointToScreen;
			scheduleFilterView.AutoLocate();

			if (scheduleFilterView.ShowDialog() == DialogResult.OK)
			{
				_schedulerState.FilterPersons(scheduleFilterView.SelectedAgentGuids());
				toolStripButtonFilterAgents.Checked = scheduleFilterView.SelectedAgentGuids().Count != SchedulerState.AllPermittedPersons.Count;

				if (_scheduleView != null)
				{
					if (_scheduleView.Presenter.SortCommand == null || _scheduleView.Presenter.SortCommand is NoSortCommand)
						_scheduleView.Presenter.ApplyGridSort();
					else
						_scheduleView.Sort(_scheduleView.Presenter.SortCommand);

					_grid.Refresh();
					GridHelper.GridlockWriteProtected(_grid, LockManager);
					_grid.Refresh();
				}
				if (_requestView != null)
					_requestView.FilterPersons(_schedulerState.FilteredPersonDictionary.Select(kvp => kvp.Key));
				drawSkillGrid();
			}
		}

		private void prepareAgentRestrictionView(IScheduleDay schedulePart, ScheduleViewBase detailView, IList<IPerson> persons)
		{
			if (persons.Count == 0) return;
			var selectedPerson = persons.FirstOrDefault();
			if (schedulePart != null) selectedPerson = schedulePart.Person;

			var schedulingOptions = schedulerSplitters1.SchedulingOptions;
			var view = (AgentRestrictionsDetailView)detailView;
			_splitContainerLessIntellegentRestriction.SplitterDistance = 300;
			schedulerSplitters1.AgentRestrictionGrid.MergeHeaders();
			schedulerSplitters1.AgentRestrictionGrid.LoadData(SchedulerState, persons, schedulingOptions, _workShiftWorkTime, selectedPerson, view, schedulePart, _container);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
				selectedPersons = new List<IPerson>(_scheduleView.AllSelectedPersons());
				sortCommand = _scheduleView.Presenter.SortCommand;
				currentSortColumn = _scheduleView.Presenter.CurrentSortColumn;
				isAscendingSort = _scheduleView.Presenter.IsAscendingSort;
				selectedPart = _scheduleView.ViewGrid[_scheduleView.ViewGrid.CurrentCell.RowIndex, _scheduleView.ViewGrid.CurrentCell.ColIndex].CellValue as IScheduleDay;
				_scheduleView.RefreshSelectionInfo -= _scheduleView_RefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor -= _scheduleView_RefreshShiftEditor;
				_scheduleView.Dispose();
				_scheduleView = null;
			}

			enableRibbonForRequests(false);
			var isRestrictionView = level == ZoomLevel.Level7;
			SchedulerRibbonHelper.EnableRibbonControls(toolStripExClipboard, toolStripExEdit2, toolStripExActions, toolStripExLocks, toolStripButtonFilterAgents, toolStripMenuItemLock, toolStripMenuItemLoggedOnUserTimeZone, isRestrictionView);

			var callback = new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), SchedulerState);
			switch (level)
			{
				case ZoomLevel.Level1:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new DayViewNew(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_scheduleView.SetSelectedDateLocal(_dateNavigateControl.SelectedDate);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.Level2:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new WeekView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.Level4:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new PeriodView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.Level5:
					restrictionViewMode(false);
					_grid.BringToFront();
					_scheduleView = new OverviewView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_grid.ContextMenuStrip = contextMenuViews;
					ActiveControl = _grid;
					break;
				case ZoomLevel.Level6:
					restrictionViewMode(false);
					_scheduleView = new PeriodView(_grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag);
					_elementHostRequests.BringToFront();
					_elementHostRequests.ContextMenuStrip = contextMenuStripRequests;
					enableRibbonForRequests(true);
					ActiveControl = _elementHostRequests;
					break;
				case ZoomLevel.Level7:
					//restriction view
					Cursor = Cursors.WaitCursor;
					_grid.BringToFront();
					_scheduleView = new AgentRestrictionsDetailView(schedulerSplitters1.AgentRestrictionGrid, _grid, SchedulerState, _gridLockManager, SchedulePartFilter, _clipHandlerSchedule, _overriddenBusinessRulesHolder, callback, _defaultScheduleTag, _workShiftWorkTime);
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
					throw new InvalidEnumArgumentException("level", (int)level, typeof(ZoomLevel));
			}
			_currentZoomLevel = level;

			if (_currentZoomLevel == ZoomLevel.Level6)
				reloadRequestView();

			foreach (ToolStripItem item in toolStripPanelItemViews.Items)
			{
				var t = item as ToolStripButton;
				if (t != null)
					t.Checked = ((ZoomLevel)t.Tag == level) ? true : false;
			}
			foreach (ToolStripItem item in toolStripPanelItemViews2.Items)
			{
				var t = item as ToolStripButton;
				if (t != null && t.Tag != null)
					t.Checked = ((ZoomLevel)t.Tag == level) ? true : false;
			}

			if (_scheduleView != null)
			{
				if (sortCommand != null) _scheduleView.Presenter.SortCommand = sortCommand;
				_scheduleView.Presenter.CurrentSortColumn = currentSortColumn;
				_scheduleView.Presenter.IsAscendingSort = isAscendingSort;
				_scheduleView.RefreshSelectionInfo += _scheduleView_RefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor += _scheduleView_RefreshShiftEditor;
				_scheduleView.ViewPasteCompleted += _currentView_viewPasteCompleted;
				_scheduleView.LoadScheduleViewGrid();

				if (scheduleParts != null)
				{
					_scheduleView.SetSelectionFromParts(scheduleParts);
					updateShiftEditor();
				}
			}
			schedulerSplitters1.ResumeLayout(true);

			if (level == ZoomLevel.Level1)
			{

				_grid.Model.Selections.Clear(true);
				_grid.Model.Selections.SelectRange(
					GridRangeInfo.Cell(_grid.CurrentCell.RowIndex, _grid.CurrentCell.ColIndex), true);

				if (_scheduleView != null)
				{
					scheduleParts = _scheduleView.SelectedSchedules();
					_scheduleView.SetSelectionFromParts(scheduleParts);
				}
			}
		}

		void AgentRestrictionGridSelectedAgentIsReady(object sender, EventArgs e)
		{
			AgentRestrictionsDetailView view = _scheduleView as AgentRestrictionsDetailView;
			if (view == null)
				return;

			if (view.TheGrid.InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(AgentRestrictionGridSelectedAgentIsReady), sender, e);
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

		private void _scheduleView_RefreshShiftEditor(object sender, EventArgs e)
		{
			updateShiftEditor();
		}

		private void _scheduleView_RefreshSelectionInfo(object sender, EventArgs e)
		{
			updateSelectionInfo(_scheduleView.SelectedSchedules());
		}

		private void updateRequestCommandsAvailability()
		{
			if (_requestView != null)
			{
				toolStripExHandleRequests.Enabled = _requestView.IsSelectionEditable() && isPermittedApproveRequest(_requestView.SelectedAdapters());
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
			var authorization = PrincipalAuthorization.Instance();

			for (var i = scenarios.Count - 1; i > -1; i--)
			{
				if (scenarios[i].Restricted && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
					scenarios.RemoveAt(i);
			}
			officeDropDownButtonMainMenuExportTo.DropDownItems.Clear();
			officeDropDownButtonMainMenuExportTo.DropDownText = Resources.ExportToScenario;

			foreach (IScenario scenario in scenarios)
			{
				if (_scenario.Description.Name != scenario.Description.Name)
				{
					var scenarioMenuItem = new ToolStripMenuItem(scenario.Description.Name);
					scenarioMenuItem.TextAlign = ContentAlignment.MiddleLeft;
					scenarioMenuItem.Click += scenarioMenuItem_Click;
					scenarioMenuItem.Tag = scenario;
					officeDropDownButtonMainMenuExportTo.DropDownItems.Insert(0, scenarioMenuItem);
				}
			}
		}

		private void scenarioMenuItem_Click(object sender, EventArgs e)
		{
			var scenario = (IScenario)((ToolStripMenuItem)sender).Tag;

			var allNewRules = _schedulerState.SchedulingResultState.GetRulesToRun();
			var selectedSchedules = _scheduleView.SelectedSchedules();
			var uowFactory = UnitOfWorkFactory.Current;
			var scheduleRepository = new ScheduleRepository(uowFactory);
			using (var exportForm = new ExportToScenarioResultView(uowFactory, scheduleRepository, new MoveDataBetweenSchedules(allNewRules, new EmptyScheduleDayChangeCallback()),
															_schedulerMessageBrokerHandler,
															ScheduleViewBase.AllSelectedPersons(selectedSchedules),
															selectedSchedules,
															scenario,
															new ScheduleDictionaryBatchPersister(UnitOfWorkFactory.CurrentUnitOfWorkFactory(), scheduleRepository, new ScheduleDictionarySaver(), new DifferenceEntityCollectionService<IPersistableScheduleData>(), _schedulerMessageBrokerHandler, null, null)))
			{
				exportForm.ShowDialog(this);
			}
		}

		private void loadTagsMenu()
		{
			var tags = _schedulerState.CommonStateHolder.ScheduleTags;
			var tagsMenuLoader = new TagsMenuLoader(toolStripMenuItemLockTags, toolStripMenuItemLockTagsRM, tags, toolStripMenuItemLockTag, toolStripSplitButtonChangeTag, toolStripMenuItemChangeTag, toolStripComboBoxAutoTag, _defaultScheduleTag, toolStripMenuItemChangeTagRM);
			tagsMenuLoader.LoadTags();
		}

		private void loadAbsencesMenu()
		{
			if (_scheduleView != null)
			{
				var toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
				var toolStripMenuItemDeletedAbsenceLockRibbon = new ToolStripMenuItem();
				var toolStripMenuItemAbsenceLockRM = new ToolStripMenuItem();
				var toolStripMenuItemDeletedAbsenceLockRM = new ToolStripMenuItem();
				var sortedAbsences = from a in _schedulerState.CommonStateHolder.Absences
									 orderby a.Description.ShortName, a.Description.Name
									 select a;

				if (sortedAbsences.Any())
				{
					toolStripMenuItemAbsenceLockRibbon.Text = Resources.All;
					toolStripMenuItemAbsenceLockRM.Text = Resources.All;
					toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsenceDays_Click;
					toolStripMenuItemAbsenceLockRM.MouseUp += ToolStripMenuItemLockAbsenceDaysMouseUp;
					toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
					toolStripMenuItemLockAbsencesRM.DropDownItems.Add(toolStripMenuItemAbsenceLockRM);
				}
				foreach (IAbsence abs in sortedAbsences)
				{
					if (((IDeleteTag)abs).IsDeleted)
						continue;
					toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemAbsenceLockRM = new ToolStripMenuItem();

					toolStripMenuItemAbsenceLockRibbon.Text = abs.Description.ToString();
					toolStripMenuItemAbsenceLockRM.Text = abs.Description.ToString();

					toolStripMenuItemAbsenceLockRibbon.Tag = abs;
					toolStripMenuItemAbsenceLockRM.Tag = abs;

					toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsences_Click;
					toolStripMenuItemAbsenceLockRM.MouseUp += ToolStripMenuItemAbsenceLockRmMouseUp;

					toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
					toolStripMenuItemLockAbsencesRM.DropDownItems.Add(toolStripMenuItemAbsenceLockRM);
				}
				var deleted = from a in sortedAbsences
							  where ((IDeleteTag)a).IsDeleted
							  select a;
				if (deleted.Any())
				{
					toolStripMenuItemDeletedAbsenceLockRM.Text = Resources.Deleted;
					toolStripMenuItemDeletedAbsenceLockRibbon.Text = Resources.Deleted;
					toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemDeletedAbsenceLockRibbon);
					toolStripMenuItemLockAbsencesRM.DropDownItems.Add(toolStripMenuItemDeletedAbsenceLockRM);

					foreach (IAbsence abs in deleted)
					{
						toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
						toolStripMenuItemAbsenceLockRM = new ToolStripMenuItem();
						toolStripMenuItemAbsenceLockRibbon.Text = abs.Description.ToString();
						toolStripMenuItemAbsenceLockRM.Text = abs.Description.ToString();
						toolStripMenuItemAbsenceLockRibbon.Tag = abs;
						toolStripMenuItemAbsenceLockRM.Tag = abs;
						toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsences_Click;
						toolStripMenuItemAbsenceLockRM.MouseUp += ToolStripMenuItemAbsenceLockRmMouseUp;
						toolStripMenuItemDeletedAbsenceLockRibbon.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
						toolStripMenuItemDeletedAbsenceLockRM.DropDownItems.Add(toolStripMenuItemAbsenceLockRM);
					}
				}
			}
		}

		private void loadDayOffMenu()
		{
			if (_scheduleView != null)
			{
				IList<Description> descriptions = new List<Description>();
				ToolStripMenuItem toolStripMenuItemDayOffLockRibbon;
				ToolStripMenuItem toolStripMenuItemDayOffLockRm;
				ToolStripMenuItem toolStripMenuItemDeletedDayOffLockRibbon;
				ToolStripMenuItem toolStripMenuDeletedItemDayOffLockRm;

				IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
													  orderby item.Description.ShortName, item.Description.Name
													  select item).ToList();
				if (displayList.Count > 0)
				{
					toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRibbon.Text = Resources.All;
					toolStripMenuItemDayOffLockRm.Text = Resources.All;
					toolStripMenuItemDayOffLockRibbon.Click += toolStripMenuItemLockFreeDays_Click;
					toolStripMenuItemDayOffLockRm.MouseUp += ToolStripMenuItemDayOffLockRmMouseUp;
					toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
					toolStripMenuItemLockFreeDaysRM.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
				}
				foreach (IDayOffTemplate dayOff in displayList)
				{
					if (((IDeleteTag)dayOff).IsDeleted)
						continue;
					if (descriptions.Count > 0)
					{
						if (descriptions.Contains(dayOff.Description))
							continue;
					}
					toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRibbon.Text = dayOff.Description.ToString();
					toolStripMenuItemDayOffLockRm.Text = dayOff.Description.ToString();
					toolStripMenuItemDayOffLockRibbon.Tag = dayOff;
					toolStripMenuItemDayOffLockRm.Tag = dayOff;
					toolStripMenuItemDayOffLockRibbon.Click += toolStripMenuItemLockSpecificDayOff_Click;
					toolStripMenuItemDayOffLockRm.Click += toolStripMenuItemLockSpecificDayOff_Click;
					toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
					toolStripMenuItemLockFreeDaysRM.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
					descriptions.Add(dayOff.Description);
				}
				var deleted = from a in displayList
							  where ((IDeleteTag)a).IsDeleted
							  select a;
				if (deleted.Any())
				{
					toolStripMenuItemDeletedDayOffLockRibbon = new ToolStripMenuItem();
					toolStripMenuDeletedItemDayOffLockRm = new ToolStripMenuItem();
					toolStripMenuItemDeletedDayOffLockRibbon.Text = Resources.Deleted;
					toolStripMenuDeletedItemDayOffLockRm.Text = Resources.Deleted;
					toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDeletedDayOffLockRibbon);
					toolStripMenuItemLockFreeDaysRM.DropDownItems.Add(toolStripMenuDeletedItemDayOffLockRm);

					foreach (IDayOffTemplate dayOff in deleted)
					{
						if (descriptions.Count > 0)
						{
							if (descriptions.Contains(dayOff.Description))
								continue;
						}

						toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
						toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
						toolStripMenuItemDayOffLockRibbon.Text = dayOff.Description.ToString();
						toolStripMenuItemDayOffLockRm.Text = dayOff.Description.ToString();
						toolStripMenuItemDayOffLockRibbon.Tag = dayOff;
						toolStripMenuItemDayOffLockRm.Tag = dayOff;
						toolStripMenuItemDayOffLockRibbon.Click += toolStripMenuItemLockSpecificDayOff_Click;
						toolStripMenuItemDayOffLockRm.Click += toolStripMenuItemLockSpecificDayOff_Click;
						toolStripMenuItemDeletedDayOffLockRibbon.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
						toolStripMenuDeletedItemDayOffLockRm.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
						descriptions.Add(dayOff.Description);
					}
				}
			}
		}

		private void loadShiftCategoriesMenu()
		{
			if (_scheduleView != null)
			{
				var toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
				var toolStripMenuItemShiftCategoryLockRM = new ToolStripMenuItem();
				var toolStripMenuItemDeletedShiftCategoryLockRibbon = new ToolStripMenuItem();
				var toolStripMenuItemDeletedShiftCategoryLockRM = new ToolStripMenuItem();
				var sortedCategories = from c in _schedulerState.CommonStateHolder.ShiftCategories
									   orderby c.Description.ShortName, c.Description.Name
									   select c;
				if (sortedCategories.Any())
				{
					toolStripMenuItemShiftCategoryLockRibbon.Text = Resources.All;
					toolStripMenuItemShiftCategoryLockRM.Text = Resources.All;
					toolStripMenuItemShiftCategoryLockRibbon.Click += toolStripMenuItemLockShiftCategoryDays_Click;
					toolStripMenuItemShiftCategoryLockRM.MouseUp += ToolStripMenuItemLockShiftCategoryDaysMouseUp;
					toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
					toolStripMenuItemLockShiftCategoriesRM.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRM);
				}
				foreach (IShiftCategory shiftCategory in sortedCategories)
				{
					if (((IDeleteTag)shiftCategory).IsDeleted) continue;
					toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemShiftCategoryLockRM = new ToolStripMenuItem();
					toolStripMenuItemShiftCategoryLockRibbon.Text = shiftCategory.Description.ToString();
					toolStripMenuItemShiftCategoryLockRM.Text = shiftCategory.Description.ToString();
					toolStripMenuItemShiftCategoryLockRibbon.Tag = shiftCategory;
					toolStripMenuItemShiftCategoryLockRM.Tag = shiftCategory;
					toolStripMenuItemShiftCategoryLockRibbon.Click += toolStripMenuItemLockShiftCategories_Click;
					toolStripMenuItemShiftCategoryLockRM.MouseUp += ToolStripMenuItemLockShiftCategoriesMouseUp;
					toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
					toolStripMenuItemLockShiftCategoriesRM.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRM);
				}
				var deleted = from a in sortedCategories
							  where ((IDeleteTag)a).IsDeleted
							  select a;
				if (deleted.Any())
				{
					toolStripMenuItemDeletedShiftCategoryLockRibbon.Text = Resources.Deleted;
					toolStripMenuItemDeletedShiftCategoryLockRM.Text = Resources.Deleted;
					toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemDeletedShiftCategoryLockRibbon);
					toolStripMenuItemLockShiftCategoriesRM.DropDownItems.Add(toolStripMenuItemDeletedShiftCategoryLockRM);

					foreach (IShiftCategory category in deleted)
					{
						toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
						toolStripMenuItemShiftCategoryLockRM = new ToolStripMenuItem();
						toolStripMenuItemShiftCategoryLockRibbon.Text = category.Description.ToString();
						toolStripMenuItemShiftCategoryLockRM.Text = category.Description.ToString();
						toolStripMenuItemShiftCategoryLockRibbon.Tag = category;
						toolStripMenuItemShiftCategoryLockRM.Tag = category;
						toolStripMenuItemShiftCategoryLockRibbon.Click += toolStripMenuItemLockShiftCategories_Click;
						toolStripMenuItemShiftCategoryLockRM.MouseUp += ToolStripMenuItemLockShiftCategoriesMouseUp;
						toolStripMenuItemDeletedShiftCategoryLockRibbon.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
						toolStripMenuItemDeletedShiftCategoryLockRM.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRM);
					}
				}
			}
		}

		private void initializeDocking()
		{
			_dockingManager = new DockingManager(components);
			_dockingManager.HostForm = this;
			_dockingManager.ThemesEnabled = true;
			DockingManager.AnimationStep = 5;
			_dockingManager.VisualStyle = VisualStyle.Office2007;
			_dockingManager.PersistState = false;
			_dockingManager.EnableContextMenu = false;
			_dockingManager.CloseEnabled = true;
			var button = new Syncfusion.Windows.Forms.Tools.CaptionButton(CaptionButtonType.Close);
			_dockingManager.CaptionButtons.Add(button);
			_dockingManager.DockStateChanged += dockStateChanged;
			_dockingManager.DockVisibilityChanged += dockVisibilityChanged;
		}

		private void enableSwapButtons(IList<IScheduleDay> selectedSchedules)
		{
			if (_scheduleView == null) return;
			toolStripMenuItemSwap.Enabled = false;
			toolStripMenuItemSwapAndReschedule.Enabled = false;
			ToolStripMenuItemSwapRaw.Enabled = false;
			toolStripDropDownButtonSwap.Enabled = false;
			var gridRangeInfoList = GridHelper.GetGridSelectedRanges(_grid, true);

			if (gridRangeInfoList.Count == 2 && GridHelper.IsRangesSameSize(gridRangeInfoList[0], gridRangeInfoList[1]))
			{
				if (!GridHelper.SelectionContainsOnlyHeadersAndScheduleDays(_grid, gridRangeInfoList)) return;
				toolStripDropDownButtonSwap.Enabled = true;
				ToolStripMenuItemSwapRaw.Enabled = true;
			}
			if (selectedSchedules.Count <= 1 || ScheduleViewBase.AllSelectedPersons(selectedSchedules).Count() != 2)
				return;

			toolStripDropDownButtonSwap.Enabled = true;
			toolStripMenuItemSwap.Enabled = true;
			var automaticScheduleFunction = DefinedRaptorApplicationFunctionPaths.AutomaticScheduling;

			toolStripMenuItemSwapAndReschedule.Enabled = hasFunctionPermissionForTeams(_temporarySelectedEntitiesFromTreeView.OfType<ITeam>(), automaticScheduleFunction);
			if (_teamLeaderMode)
				toolStripMenuItemSwapAndReschedule.Enabled = false;

			if (!toolStripMenuItemSwapAndReschedule.Enabled) return;
			var schedulesWithinValidPeriod = ScheduleHelper.SchedulesWithinValidSchedulePeriod(selectedSchedules);
			toolStripMenuItemSwapAndReschedule.Enabled = schedulesWithinValidPeriod.Count == selectedSchedules.Count;
		}

		private void updateSelectionInfo(IList<IScheduleDay> selectedSchedules)
		{
			var updater = new UpdateSelectionInfo(toolStripStatusLabelContractTime, toolStripStatusLabelScheduleTag);
			updater.Update(selectedSchedules, _scheduleView, _schedulerState, _agentInfo);
		}

		private void deleteInMainGrid(PasteOptions deleteOptions)
		{
			if (_scheduleView != null)
			{
				var deleteOption = new DeleteOption();
				deleteOption.MainShift = deleteOptions.MainShift;
				deleteOption.DayOff = deleteOptions.DayOff;
				deleteOption.PersonalShift = deleteOptions.PersonalShifts;
				deleteOption.Overtime = deleteOptions.Overtime;
				deleteOption.Preference = deleteOptions.Preference;
				deleteOption.StudentAvailability = deleteOptions.StudentAvailability;
				PasteAction pasteAction = deleteOptions.Absences;
				if (pasteAction == PasteAction.Replace)
					deleteOption.Absence = true;

				deleteOption.Default = deleteOptions.Default;
				deleteFromSchedulePart(deleteOption);
			}
		}

		private void drawSkillGrid()
		{
			if (_teamLeaderMode) return;
			if (_scheduleView != null)
			{
				if (_tabSkillData.SelectedIndex >= 0)
				{
					_currentIntraDayDate = _scheduleView.SelectedDateLocal();
					TabPageAdv tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
					var skill = (ISkill)tab.Tag;
					IAggregateSkill aggregateSkillSkill = skill;
					_chartDescription = skill.Name;

					if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week))
					{
						positionControl(_skillWeekGridControl);
						ActiveControl = _skillWeekGridControl;
						_skillWeekGridControl.DrawDayGrid(_schedulerState, skill);
						_skillWeekGridControl.DrawDayGrid(_schedulerState, skill);
					}

					if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month))
					{
						positionControl(_skillMonthGridControl);
						ActiveControl = _skillMonthGridControl;
						_skillMonthGridControl.DrawDayGrid(_schedulerState, skill);
						_skillMonthGridControl.DrawDayGrid(_schedulerState, skill);
					}

					if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period))
					{
						if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
						{
							positionControl(_skillFullPeriodGridControl, SkillFullPeriodGridControl.PreferredGridWidth);
							TabPageAdv thisTab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
							thisTab.Controls.Add(_skillResultHighlightGridControl);
							_skillResultHighlightGridControl.DrawGridContents(_schedulerState, skill);
							_skillResultHighlightGridControl.Left = SkillFullPeriodGridControl.PreferredGridWidth + 5;
							_skillResultHighlightGridControl.Top = 0;
							_skillResultHighlightGridControl.Width = thisTab.Width - _skillResultHighlightGridControl.Left;
							_skillResultHighlightGridControl.Height = thisTab.Height;
							_skillResultHighlightGridControl.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left;
						}
						else
						{
							positionControl(_skillFullPeriodGridControl);
						}

						ActiveControl = _skillFullPeriodGridControl;
						_skillFullPeriodGridControl.DrawDayGrid(_schedulerState, skill);
						_skillFullPeriodGridControl.DrawDayGrid(_schedulerState, skill);
					}

					if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
					{
						drawIntraday(skill, aggregateSkillSkill);
					}
					if (_skillResultViewSetting.Equals(SkillResultViewSetting.Day))
					{
						positionControl(_skillDayGridControl);
						ActiveControl = _skillDayGridControl;
						_skillDayGridControl.DrawDayGrid(_schedulerState, skill);
						_skillDayGridControl.DrawDayGrid(_schedulerState, skill);
					}
				}
			}
		}

		private void refreshSummarySkillIfActive()
		{
			if (_tabSkillData.SelectedIndex < 0) return;
			var tab = _tabSkillData.TabPages[_tabSkillData.SelectedIndex];
			var skill = (ISkill)tab.Tag;
			IAggregateSkill aggregateSkillSkill = skill;
			if (!aggregateSkillSkill.IsVirtual)
				return;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week))
			{
				_skillWeekGridControl.SetDataSource(_schedulerState, skill);
				_skillWeekGridControl.Refresh();
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month))
			{
				_skillMonthGridControl.SetDataSource(_schedulerState, skill);
				_skillMonthGridControl.Refresh();
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period))
			{
				_skillFullPeriodGridControl.SetDataSource(_schedulerState, skill);
				_skillFullPeriodGridControl.Refresh();
			}

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				var skillStaffPeriods = SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					aggregateSkillSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate, _currentIntraDayDate.AddDays(1), _schedulerState.TimeZoneInfo));
				if (_skillIntradayGridControl.Presenter.RowManager != null)
				{
					_skillIntradayGridControl.Presenter.RowManager.SetDataSource(skillStaffPeriods);
					_skillFullPeriodGridControl.Refresh();
				}
			}
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Day))
			{
				_skillDayGridControl.SetDataSource(_schedulerState, skill);
				_skillDayGridControl.Refresh();
			}
		}

		private void drawIntraday(ISkill skill, IAggregateSkill aggregateSkillSkill)
		{
			IList<ISkillStaffPeriod> skillStaffPeriods;
			if (aggregateSkillSkill.IsVirtual)
			{
				SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate, _currentIntraDayDate.AddDays(1), _schedulerState.TimeZoneInfo));
				skillStaffPeriods = SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate, _currentIntraDayDate.AddDays(1), _schedulerState.TimeZoneInfo));
			}
			else
			{
				DateTimePeriod periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate, _currentIntraDayDate.AddDays(1), _schedulerState.TimeZoneInfo);
				skillStaffPeriods = SchedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, periodToFind);
			}
			if (skillStaffPeriods.Count >= 0)
			{
				_chartDescription = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", skill.Name, _currentIntraDayDate.ToShortDateString());
				var statistics = new SkillStaffPeriodStatisticsForSkillIntraday(skillStaffPeriods);
				statistics.Analyze();
				_skillIntradayGridControl.SetupDataSource(skillStaffPeriods, skill, _schedulerState);
				_skillIntradayGridControl.SetRowsAndCols();
				positionControl(_skillIntradayGridControl);
			}
		}

		private void loadSkillDays(IUnitOfWork uow, ISchedulerStateHolder stateHolder, IPeopleAndSkillLoaderDecider decider)
		{
			if (_teamLeaderMode) return;
			using (PerformanceOutput.ForOperation("Loading skill days"))
			{
				ISkillDayRepository skillDayRepository = new SkillDayRepository(uow);
				IMultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(uow);
				stateHolder.SchedulingResultState.SkillDays = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository).
					LoadSchedulerSkillDays(
						new DateOnlyPeriod(stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8), stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8)), stateHolder.SchedulingResultState.Skills, stateHolder.RequestedScenario);

				createMaxSeatSkills(skillDayRepository);
				IList<ISkillStaffPeriod> skillStaffPeriods = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(stateHolder.SchedulingResultState.Skills, stateHolder.LoadedPeriod.Value);

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

		private IPersonAssignment getAssignmentZOrder(bool before, bool move)
		{
			if (_scheduleView != null)
			{
				IList<IScheduleDay> selectedSchedules = _scheduleView.SelectedSchedules();
				foreach (IScheduleDay schedule in selectedSchedules)
				{
					IPersonAssignment highZOrder = schedule.AssignmentHighZOrder();
					IPersonAssignment newHighZOrder = null;

					var personAssignments = schedule.PersonAssignmentCollection();
					if (personAssignments.Count > 1)
					{
						int num = 0;
						foreach (IPersonAssignment pa in personAssignments)
						{
							if (highZOrder == null)
							{
								newHighZOrder = pa;
								break;
							}
							if (before)
								newHighZOrder = getAssignmentZOrderBefore(highZOrder, pa, num, personAssignments);
							else
								newHighZOrder = getAssignmentZOrderNext(highZOrder, pa, num, personAssignments);

							if (newHighZOrder != null)
								break;
							num++;
						}
					}
					if (newHighZOrder != null && move)
					{
						newHighZOrder.ZOrder = DateTime.Now;
						_scheduleView.Presenter.ModifySchedulePart(new List<IScheduleDay> { schedule });
					}
					return newHighZOrder;
				}
			}
			return null;
		}

		private static IPersonAssignment getAssignmentZOrderBefore(IPersonAssignment highZOrder, IPersonAssignment pa, int num, IList<IPersonAssignment> personAssignments)
		{
			if (pa == highZOrder && num > 0)
				return personAssignments[num - 1];
			return null;
		}

		private static IPersonAssignment getAssignmentZOrderNext(IPersonAssignment highZOrder, IPersonAssignment pa, int num, IList<IPersonAssignment> personAssignments)
		{
			if (pa == highZOrder)
			{
				if (num < personAssignments.Count - 1)
				{
					return personAssignments[num + 1];
				}
			}
			return null;
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

			if (!_schedulerState.Schedules.DifferenceSinceSnapshot().IsEmpty() || _schedulerState.ChangedRequests() || !_modifiedWriteProtections.IsEmpty())
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

		private void selectCellFromPersonDate(IPerson person, DateTime localDate)
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
			schedulerSplitters1.TabSkillData.SelectedIndexChanged += tabSkillData_SelectedIndexChanged;
			_grid.CurrentCellKeyDown += grid_CurrentCellKeyDown;
			_grid.GotFocus += grid_GotFocus;
			_grid.SelectionChanged += grid_SelectionChanged;
			_grid.Click += grid_Click;
			_grid.ScrollControlMouseUp += _grid_ScrollControlMouseUp;
			_grid.StartAutoScrolling += _grid_StartAutoScrolling;

			wpfShiftEditor1.ShiftUpdated += wpfShiftEditor1_ShiftUpdated;
			wpfShiftEditor1.CommitChanges += wpfShiftEditor1_CommitChanges;
			wpfShiftEditor1.EditMeeting += wpfShiftEditor1_EditMeeting;
			wpfShiftEditor1.RemoveParticipant += wpfShiftEditor1_RemoveParticipant;
			wpfShiftEditor1.DeleteMeeting += wpfShiftEditor1_DeleteMeeting;
			wpfShiftEditor1.CreateMeeting += wpfShiftEditor1_CreateMeeting;
			wpfShiftEditor1.AddAbsence += wpfShiftEditor_AddAbsence;
			wpfShiftEditor1.AddActivity += wpfShiftEditor_AddActivity;
			wpfShiftEditor1.AddOvertime += wpfShiftEditor_AddOvertime;
			wpfShiftEditor1.AddPersonalShift += wpfShiftEditor_AddPersonalShift;
			wpfShiftEditor1.Undo += wpfShiftEditor_Undo;

			notesEditor.NotesChanged += notesEditor_NotesChanged;
			notesEditor.PublicNotesChanged += notesEditor_PublicNotesChanged;

			_skillDayGridControl.GotFocus += skillDayGridControl_GotFocus;
			_skillIntradayGridControl.GotFocus += skillIntradayGridControl_GotFocus;
			_skillWeekGridControl.GotFocus += skillWeekGridControl_GotFocus;
			_skillMonthGridControl.GotFocus += skillMonthGridControl_GotFocus;
			_skillFullPeriodGridControl.GotFocus += skillFullPeriodGridControl_GotFocus;

			_skillDayGridControl.SelectionChanged += skillDayGridControl_SelectionChanged;
			_skillIntradayGridControl.SelectionChanged += skillIntradayGridControl_SelectionChanged;
			_skillWeekGridControl.SelectionChanged += skillWeekGridControlSelectionChanged;
			_skillMonthGridControl.SelectionChanged += skillMonthGridControlSelectionChanged;
			_skillFullPeriodGridControl.SelectionChanged += skillFullPeriodGridControlSelectionChanged;
			_skillResultHighlightGridControl.GoToDate += _skillResultHighlightGridControl_GoToDate;

			_gridrowInChartSettingButtons.LineInChartSettingsChanged += gridlinesInChartSettings_LineInChartSettingsChanged;
			_gridrowInChartSettingButtons.LineInChartEnabledChanged += gridrowInChartSetting_LineInChartEnabledChanged;
			_chartControlSkillData.ChartRegionMouseEnter += chartControlSkillData_ChartRegionMouseEnter;
			_chartControlSkillData.ChartRegionMouseHover += chartControlSkillData_ChartRegionMouseHover;
			_chartControlSkillData.ChartRegionClick += chartControlSkillData_ChartRegionClick;
			_undoRedo.ChangedHandler += undoRedo_Changed;

			#region eventaggregator
			_eventAggregator.GetEvent<GenericEvent<HandlePersonRequestSelectionChanged>>().Subscribe(requestSelectionChanged);
			_eventAggregator.GetEvent<GenericEvent<ShowRequestDetailsView>>().Subscribe(showRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ApproveRequestFromRequestDetailsView>>().Subscribe(approveRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<DenyRequestFromRequestDetailsView>>().Subscribe(denyRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyRequestFromRequestDetailsView>>().Subscribe(replyRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyAndApproveRequestFromRequestDetailsView>>().Subscribe(replyAndApproveRequestFromRequestDetailsView);
			_eventAggregator.GetEvent<GenericEvent<ReplyAndDenyRequestFromRequestDetailsView>>().Subscribe(replyAndDenyRequestFromRequestDetailsView);
			#endregion
		}

		void _skillResultHighlightGridControl_GoToDate(object sender, GoToDateEventArgs e)
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
			toolStripButtonReplyAndDeny_Click(null, null);
		}

		private void replyAndApproveRequestFromRequestDetailsView(EventParameters<ReplyAndApproveRequestFromRequestDetailsView> obj)
		{
			toolStripButtonReplyAndApprove_Click(null, null);
		}

		private void replyRequestFromRequestDetailsView(EventParameters<ReplyRequestFromRequestDetailsView> eventParameters)
		{
			toolStripButtonEditNote_Click(null, null);
		}

		private void denyRequestFromRequestDetailsView(EventParameters<DenyRequestFromRequestDetailsView> eventParameters)
		{
			toolStripButtonDenyRequestClick(null, null);
		}

		private void approveRequestFromRequestDetailsView(EventParameters<ApproveRequestFromRequestDetailsView> eventParameters)
		{
			toolStripButtonApproveRequestClick(null, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void setEventHandlersOff()
		{
			_dateNavigateControl.SelectedDateChanged -= dateNavigateControlSelectedDateChanged;
			if (_schedulerMessageBrokerHandler != null)
			{
				_schedulerMessageBrokerHandler.StopListen();
				_schedulerMessageBrokerHandler.RequestDeletedFromBroker -= _schedulerMessageBrokerHandler_RequestDeletedFromBroker;
				_schedulerMessageBrokerHandler.SchedulesUpdatedFromBroker -= _schedulerMessageBrokerHandler_SchedulesUpdatedFromBroker;

				_schedulerMessageBrokerHandler.Dispose();
				_schedulerMessageBrokerHandler = null; // referens till SchedulingScreen
			}
			_requestPresenter = null; // referens till SchedulingScreen
			_optimizationHelperWin = null;

			if (backgroundWorkerLoadData != null)
			{
				backgroundWorkerLoadData.DoWork -= backgroundWorkerLoadData_DoWork;
				backgroundWorkerLoadData.RunWorkerCompleted -= backgroundWorkerLoadData_RunWorkerCompleted;
				backgroundWorkerLoadData.ProgressChanged -= backgroundWorkerLoadData_ProgressChanged;
			}

			if (_backgroundWorkerDelete != null)
			{
				_backgroundWorkerDelete.DoWork -= _backgroundWorkerDelete_DoWork;
				_backgroundWorkerDelete.RunWorkerCompleted -= _backgroundWorkerDelete_RunWorkerCompleted;
			}

			if (_backgroundWorkerResourceCalculator != null)
			{
				_backgroundWorkerResourceCalculator.DoWork -= _backgroundWorkerResourceCalculator_DoWork;
				_backgroundWorkerResourceCalculator.ProgressChanged -= _backgroundWorkerResourceCalculator_ProgressChanged;
				_backgroundWorkerResourceCalculator.RunWorkerCompleted -= _backgroundWorkerResourceCalculator_RunWorkerCompleted;
			}

			if (_backgroundWorkerValidatePersons != null)
			{
				_backgroundWorkerValidatePersons.RunWorkerCompleted -= _backgroundWorkerValidatePersons_RunWorkerCompleted;
				_backgroundWorkerValidatePersons.DoWork -= _backgroundWorkerValidatePersons_DoWork;
			}

			if (_backgroundWorkerScheduling != null)
			{
				_backgroundWorkerScheduling.DoWork -= _backgroundWorkerScheduling_DoWork;
				_backgroundWorkerScheduling.ProgressChanged -= _backgroundWorkerScheduling_ProgressChanged;
				_backgroundWorkerScheduling.RunWorkerCompleted -= _backgroundWorkerScheduling_RunWorkerCompleted;
			}

			if (_backgroundWorkerOptimization != null)
			{
				_backgroundWorkerOptimization.DoWork -= _backgroundWorkerOptimization_DoWork;
				_backgroundWorkerOptimization.ProgressChanged -= _backgroundWorkerOptimization_ProgressChanged;
			}

			if (toolStripComboBoxExFilterDays != null)
				toolStripComboBoxExFilterDays.SelectedIndexChanged -= toolStripComboBoxExFilterDays_SelectedIndexChanged;

			if (toolStripComboBoxAutoTag != null)
				toolStripComboBoxAutoTag.SelectedIndexChanged -= toolStripComboBoxAutoTag_SelectedIndexChanged;

			if (SchedulerState != null && SchedulerState.Schedules != null)
				SchedulerState.Schedules.PartModified -= _schedules_PartModified;

			if (_schedulerMeetingHelper != null)
				_schedulerMeetingHelper.ModificationOccured -= _schedulerMeetingHelper_ModificationOccured;
			if (schedulerSplitters1 != null)
			{
				schedulerSplitters1.HandlePersonRequestView1.RemoveEvents();
				schedulerSplitters1.TabSkillData.SelectedIndexChanged -= tabSkillData_SelectedIndexChanged;
			}
			if (_grid != null)
			{
				_grid.CurrentCellKeyDown -= grid_CurrentCellKeyDown;
				_grid.GotFocus -= grid_GotFocus;
				_grid.SelectionChanged -= grid_SelectionChanged;
				_grid.StartAutoScrolling -= _grid_StartAutoScrolling;
				_grid.ScrollControlMouseUp -= _grid_ScrollControlMouseUp;
				_grid.Click -= grid_Click;
			}

			if (wpfShiftEditor1 != null)
			{
				wpfShiftEditor1.ShiftUpdated -= wpfShiftEditor1_ShiftUpdated;
				wpfShiftEditor1.CommitChanges -= wpfShiftEditor1_CommitChanges;
				wpfShiftEditor1.EditMeeting -= wpfShiftEditor1_EditMeeting;
				wpfShiftEditor1.RemoveParticipant -= wpfShiftEditor1_RemoveParticipant;
				wpfShiftEditor1.DeleteMeeting -= wpfShiftEditor1_DeleteMeeting;
				wpfShiftEditor1.CreateMeeting -= wpfShiftEditor1_CreateMeeting;

				wpfShiftEditor1.AddAbsence -= wpfShiftEditor_AddAbsence;
				wpfShiftEditor1.AddActivity -= wpfShiftEditor_AddActivity;
				wpfShiftEditor1.AddOvertime -= wpfShiftEditor_AddOvertime;
				wpfShiftEditor1.AddPersonalShift -= wpfShiftEditor_AddPersonalShift;
			}

			if (notesEditor != null)
			{
				notesEditor.NotesChanged -= notesEditor_NotesChanged;
				notesEditor.PublicNotesChanged -= notesEditor_PublicNotesChanged;
			}
			if (_requestView != null)
				_requestView.PropertyChanged -= _requestView_PropertyChanged;

			if (_skillResultHighlightGridControl != null) _skillResultHighlightGridControl.GoToDate -= _skillResultHighlightGridControl_GoToDate;

			if (_skillDayGridControl != null) _skillDayGridControl.GotFocus -= skillDayGridControl_GotFocus;
			if (_skillIntradayGridControl != null)
				_skillIntradayGridControl.GotFocus -= skillIntradayGridControl_GotFocus;

			if (_skillWeekGridControl != null)
				_skillWeekGridControl.GotFocus -= skillWeekGridControl_GotFocus;

			if (_skillMonthGridControl != null)
				_skillMonthGridControl.GotFocus -= skillMonthGridControl_GotFocus;

			if (_skillFullPeriodGridControl != null)
				_skillFullPeriodGridControl.GotFocus -= skillFullPeriodGridControl_GotFocus;

			if (_skillDayGridControl != null)
				_skillDayGridControl.SelectionChanged -= skillDayGridControl_SelectionChanged;

			if (_skillIntradayGridControl != null)
				_skillIntradayGridControl.SelectionChanged -= skillIntradayGridControl_SelectionChanged;

			if (_skillWeekGridControl != null)
				_skillWeekGridControl.SelectionChanged -= skillWeekGridControlSelectionChanged;

			if (_skillMonthGridControl != null)
				_skillMonthGridControl.SelectionChanged -= skillMonthGridControlSelectionChanged;

			if (_skillFullPeriodGridControl != null)
				_skillFullPeriodGridControl.SelectionChanged -= skillFullPeriodGridControlSelectionChanged;

			if (_gridrowInChartSettingButtons != null)
			{
				_gridrowInChartSettingButtons.LineInChartSettingsChanged -= gridlinesInChartSettings_LineInChartSettingsChanged;
				_gridrowInChartSettingButtons.LineInChartEnabledChanged -= gridrowInChartSetting_LineInChartEnabledChanged;
			}

			if (_chartControlSkillData != null)
			{
				_chartControlSkillData.ChartRegionMouseEnter -= chartControlSkillData_ChartRegionMouseEnter;
				_chartControlSkillData.ChartRegionMouseHover -= chartControlSkillData_ChartRegionMouseHover;
				_chartControlSkillData.ChartRegionClick -= chartControlSkillData_ChartRegionClick;
			}

			if (_clipboardControl != null)
			{
				_clipboardControl.CutSpecialClicked -= _clipboardControl_CutSpecialClicked;
				_clipboardControl.CutClicked -= _clipboardControl_CutClicked;
				_clipboardControl.PasteSpecialClicked -= _clipboardControl_PasteSpecialClicked;
				_clipboardControl.PasteClicked -= _clipboardControl_PasteClicked;
				_clipboardControl.CopySpecialClicked -= _clipboardControl_CopySpecialClicked;
				_clipboardControl.CopyClicked -= _clipboardControl_CopyClicked;
			}

			if (_clipboardControlRestrictions != null)
			{
				_clipboardControlRestrictions.CopyClicked -= toolStripMenuItemRestrictionCopy_Click;
				_clipboardControlRestrictions.PasteClicked -= toolStripMenuItemRestrictionPaste_Click;
			}

			if (_editControl != null)
			{
				_editControl.NewSpecialClicked -= _editControl_NewSpecialClicked;
				_editControl.DeleteClicked -= _editControl_DeleteClicked;
				_editControl.DeleteSpecialClicked -= _editControl_DeleteSpecialClicked;
				_editControl.NewClicked -= _editControl_NewClicked;
			}

			if (_editControlRestrictions != null)
			{
				_editControlRestrictions.NewClicked -= editControlRestrictionsNewClicked;
				_editControlRestrictions.NewSpecialClicked -= editControlRestrictionsNewSpecialClicked;
				_editControlRestrictions.DeleteClicked -= toolStripMenuItemRestrictionDelete_Click;	
			}

			if (toolStripMenuItemDeleteSpecial != null)
			{
				toolStripMenuItemDeleteSpecial.Click -= toolStripMenuItemDeleteSpecial2_Click;
			}

			if (_undoRedo != null) _undoRedo.ChangedHandler -= undoRedo_Changed;

			if (contextMenuViews != null)
			{
				contextMenuViews.Opened -= contextMenuViews_Opened;
				contextMenuViews.Opening -= contextMenuViews_Opening;
			}

			if (_eventAggregator != null)
			{
				#region eventaggregator
				_eventAggregator.GetEvent<GenericEvent<HandlePersonRequestSelectionChanged>>().Unsubscribe(requestSelectionChanged);
				_eventAggregator.GetEvent<GenericEvent<ShowRequestDetailsView>>().Unsubscribe(showRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ApproveRequestFromRequestDetailsView>>().Unsubscribe(approveRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<DenyRequestFromRequestDetailsView>>().Unsubscribe(denyRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyRequestFromRequestDetailsView>>().Unsubscribe(replyRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyAndApproveRequestFromRequestDetailsView>>().Unsubscribe(replyAndApproveRequestFromRequestDetailsView);
				_eventAggregator.GetEvent<GenericEvent<ReplyAndDenyRequestFromRequestDetailsView>>().Unsubscribe(replyAndDenyRequestFromRequestDetailsView);
				#endregion
			}
		}

		private void requestSelectionChanged(EventParameters<HandlePersonRequestSelectionChanged> eventParameters)
		{
			toolStripExHandleRequests.Enabled = eventParameters.Value.SelectionIsEditable && isPermittedApproveRequest(_requestView.SelectedAdapters());
			ToolStripMenuItemViewDetails.Enabled = toolStripButtonViewDetails.Enabled = isViewRequestDetailsAvailable();
		}

		private void wpfShiftEditor1_DeleteMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			_schedulerMeetingHelper.MeetingRemove(e.Value.BelongsToMeeting, _scheduleView);
		}

		private void wpfShiftEditor1_RemoveParticipant(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			IList<IPersonMeeting> meetings = new List<IPersonMeeting> { e.Value };
			_schedulerMeetingHelper.MeetingRemoveAttendees(meetings);
		}

		private void wpfShiftEditor1_EditMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool editPermission = isPermittedToEditMeeting();
			bool viewSchedulesPermission = isPermittedToViewSchedules();
			_schedulerMeetingHelper.MeetingComposerStart(e.Value.BelongsToMeeting, _scheduleView, editPermission, viewSchedulesPermission);
		}

		private void wpfShiftEditor1_CreateMeeting(object sender, CustomEventArgs<IPersonMeeting> e)
		{
			bool viewSchedulesPermission = isPermittedToViewSchedules();
			_schedulerMeetingHelper.MeetingComposerStart(null, _scheduleView, true, viewSchedulesPermission);
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
			updateShiftEditor();
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
			_scheduleView.Presenter.AddActivity(new List<IScheduleDay> { e.SchedulePart }, e.Period);
			RecalculateResources();
		}

		private void wpfShiftEditor_AddPersonalShift(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddPersonalShift(new List<IScheduleDay> { e.SchedulePart }, e.Period);
		}

		private void wpfShiftEditor_AddOvertime(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddOvertime(new List<IScheduleDay> { e.SchedulePart }, e.Period, MultiplicatorDefinitionSet.Where(m => m.MultiplicatorType == MultiplicatorType.Overtime).ToList());
			RecalculateResources();
		}

		private void wpfShiftEditor_AddAbsence(object sender, ShiftEditorEventArgs e)
		{
			if (_scheduleView == null) return;
			_scheduleView.Presenter.AddAbsence(new List<IScheduleDay> { e.SchedulePart }, e.Period);
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
			var allNewBusinessRules = _schedulerState.SchedulingResultState.GetRulesToRun();
			
			changeRequestStatus(
				new ApprovePersonRequestCommand(this, _schedulerState.Schedules, _schedulerState.RequestedScenario, _requestPresenter, _handleBusinessRuleResponse,
												_personRequestAuthorizationChecker, allNewBusinessRules, _overriddenBusinessRulesHolder,
												new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), SchedulerState)), _requestView.SelectedAdapters());
			if (_requestView != null)
				_requestView.NeedUpdate = true;

			reloadRequestView();
		}

		private void toolStripButtonDenyRequestClick(object sender, EventArgs e)
		{
			changeRequestStatus(new DenyPersonRequestCommand(_requestPresenter, _personRequestAuthorizationChecker), _requestView.SelectedAdapters());
		}

		private void toolStripButtonFindRequestOnClick(object sender, EventArgs eventArgs)
		{
			_requestView.FilterGrid(toolStripTextBoxFilter.Text.Split(' '), SchedulerState.FilteredPersonDictionary);
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
			replyAndChangeStatus(new ApprovePersonRequestCommand(this, _schedulerState.Schedules, _schedulerState.RequestedScenario, _requestPresenter,
																 _handleBusinessRuleResponse, _personRequestAuthorizationChecker, businessRules, _overriddenBusinessRulesHolder,
																 new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), SchedulerState)));
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
				var days = personRequestViewModel.PersonRequest.Request.Period.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone).DayCollection();
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
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider()));
				settings.Show();
				settings.BringToFront();
			}
			catch (DataSourceException ex)
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
			if (_schedulerState != null && _schedulerState.Schedules != null)
			{
				_schedulerState.Schedules.Clear();
			}
			setEventHandlersOff();
			_container.Dispose();
			if (_scheduleView != null)
			{
				_scheduleView.ViewPasteCompleted -= _currentView_viewPasteCompleted;
				_scheduleView.RefreshSelectionInfo -= _scheduleView_RefreshSelectionInfo;
				_scheduleView.RefreshShiftEditor -= _scheduleView_RefreshShiftEditor;
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
			if (_singleAgentRestrictionPresenter != null)
			{
				_singleAgentRestrictionPresenter.Dispose();
				_singleAgentRestrictionPresenter = null;
			}

			if (_elementHostRequests != null && _elementHostRequests.Child != null) _elementHostRequests.Child = null;
			if (_grid != null) _grid.ContextMenu = null;
			if (contextMenuViews != null) contextMenuViews.Dispose();
			if (schedulerSplitters1 != null) schedulerSplitters1.Dispose();

			if (!Disposing)
			{
				if (_dockingManager != null)
				{
					_dockingManager.DockStateChanged -= dockStateChanged;
					_dockingManager.DockVisibilityChanged -= dockVisibilityChanged;
				}
				Dispose(true);
			}
		}

		private void toolStripButtonQuickAccessRedo_Click_1(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Redo();
			_backgroundWorkerRunning = false;
			SelectAndRefresh();
		}

		private void toolStripSplitButtonQuickAccessUndo_ButtonClick(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.Undo();
			_backgroundWorkerRunning = false;
			SelectAndRefresh();
		}

		private void toolStripMenuItemQuickAccessUndoAll_Click_1(object sender, EventArgs e)
		{
			_backgroundWorkerRunning = true;
			_undoRedo.UndoAll();
			_backgroundWorkerRunning = false;
			SelectAndRefresh();
		}

		private void SelectAndRefresh()
		{
			if (_lastModifiedPart != null)
			{
				selectCellFromPersonDate(_lastModifiedPart.ModifiedPerson, _lastModifiedPart.ModifiedPeriod.StartDateTimeLocal(_schedulerState.TimeZoneInfo));
			}
			refreshView();
		}

		private void refreshView()
		{
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
			bool useShrinkage = !((ToolStripMenuItem)_contextMenuSkillGrid.Items["UseShrinkage"]).Checked;
			toggleShrinkage(useShrinkage);
		}

		private void toolStripMenuItemWriteProtectSchedule2_Click(object sender, EventArgs e)
		{
			writeProtectSchedule();
		}

		private void ToolstripMenuRemoveWriteProtectionMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection)) return;
			Cursor = Cursors.WaitCursor;
			var removeCommand = new WriteProtectionRemoveCommand(_scheduleView.SelectedSchedules(), _modifiedWriteProtections);
			removeCommand.Execute();
			GridHelper.GridlockWriteProtected(_grid, LockManager);
			Refresh();
			RefreshSelection();
			Cursor = Cursors.Default;
		}

		private void writeProtectSchedule()
		{
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
				return;
			GridHelper.WriteProtectPersonSchedule(_grid).ForEach(_modifiedWriteProtections.Add);
			GridHelper.GridlockWriteProtected(_grid, LockManager);
			_grid.Refresh();
			enableSave();
		}

		private void toolStripMenuItemLoggedOnUserTimeZone_Click(object sender, EventArgs e)
		{
			foreach (ToolStripMenuItem downItem in toolStripMenuItemViewPointTimeZone.DropDownItems)
			{
				downItem.Checked = (_schedulerState.TimeZoneInfo == downItem.Tag);
			}
		}

		private void setupContextMenuAvailTimeZones()
		{
			_schedulerState.TimeZoneInfo = TeleoptiPrincipal.Current.Regional.TimeZone;
			toolStripMenuItemLoggedOnUserTimeZone.Tag = _schedulerState.TimeZoneInfo;
			wpfShiftEditor1.SetTimeZone(_schedulerState.TimeZoneInfo);
			IList<TimeZoneInfo> otherList = new List<TimeZoneInfo> { _schedulerState.TimeZoneInfo };
			foreach (IPerson person in _schedulerState.AllPermittedPersons)
			{
				if (!otherList.Contains(person.PermissionInformation.DefaultTimeZone()))
					otherList.Add(person.PermissionInformation.DefaultTimeZone());
			}

			foreach (TimeZoneInfo info in otherList)
			{
				if (info != _schedulerState.TimeZoneInfo)
				{
					var item = new ToolStripMenuItem(info.DisplayName);
					item.Tag = info;
					item.Click += toolStripMenuItemLoggedOnUserTimeZone_Click;
					item.MouseUp += toolStripMenuItemLoggedOnUserTimeZoneMouseUp;
					toolStripMenuItemViewPointTimeZone.DropDownItems.Add(item);
				}
			}
		}

		private void ToolStripMenuItemExportToPdfGraphicalMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			var exporter = new ExportToPdfGraphical(_scheduleView, this, _schedulerState, TeleoptiPrincipal.Current.Regional.Culture, TeleoptiPrincipal.Current.Regional.UICulture.TextInfo.IsRightToLeft);
			exporter.Export();
		}

		private void ExportToPdf(bool shiftsPerDay)
		{
			var exporter = new ExportToPdf(_scheduleView, this, _schedulerState, TeleoptiPrincipal.Current.Regional.Culture, TeleoptiPrincipal.Current.Regional.UICulture.TextInfo.IsRightToLeft);
			exporter.Export(shiftsPerDay);
		}

		private void ToolStripMenuItemExportToPdfMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			ExportToPdf(false);
		}

		private void setRestrictionUsage()
		{
			var view = _scheduleView as RestrictionView;
			if (view == null)
				return;

        	var restrictionPresenter = (RestrictionPresenter) view.Presenter;
            restrictionPresenter.UseStudent = toolStripMenuItemUseStudentAvailability.Checked;
            restrictionPresenter.UseSchedule = toolStripMenuItemUseSchedule.Checked;
            restrictionPresenter.UseRotation = toolStripMenuItemUseRotation.Checked;
            restrictionPresenter.UsePreference = toolStripMenuItemUsePreference.Checked;
            restrictionPresenter.UseAvailability = toolStripMenuItemUseAvailability.Checked;
			disableAllExceptCancelInRibbon();
			_grid.Enabled = false;
			validateAllPersons();
		}

		private void toolStripMenuItemUseStudentAvailability_Click(object sender, EventArgs e)
		{
			setRestrictionUsage();
		}

		private void toolStripMenuItemUseSchedule_Click(object sender, EventArgs e)
		{
			setRestrictionUsage();
		}

		private void toolStripMenuItemUseRotation_Click(object sender, EventArgs e)
		{
			setRestrictionUsage();
		}

		private void toolStripMenuItemUsePreference_Click(object sender, EventArgs e)
		{
			setRestrictionUsage();
		}

		private void toolStripMenuItemUseAvailability_Click(object sender, EventArgs e)
		{
			setRestrictionUsage();
		}

		private void toolStripButtonFilterAgents_Click(object sender, EventArgs e)
		{
			showFilterDialog();
		}

		private class ConflictHandlingResult
		{
			public bool ConflictsFound { get; set; }
			public PersistConflictDialogResult DialogResult { get; set; }
		}

		private ConflictHandlingResult refreshEntitiesUsingMessageBroker()
		{
			var conflictsBuffer = new List<PersistConflictMessageState>();
			var refreshedEntitiesBuffer = new List<IPersistableScheduleData>();
			refreshEntitiesUsingMessageBroker(refreshedEntitiesBuffer, conflictsBuffer);
            var result = handleConflicts(refreshedEntitiesBuffer, conflictsBuffer);
			return result;
		}

		private ConflictHandlingResult handleConflicts(IEnumerable<IPersistConflict> conflicts)
		{
			return handleConflicts(null, conflicts);
		}

		private ConflictHandlingResult handleConflicts(IEnumerable<IPersistableScheduleData> refreshedEntities, IEnumerable<IPersistConflict> conflicts)
		{
			List<IPersistableScheduleData> changedEntitiesBuffer;
			if (refreshedEntities == null)
				changedEntitiesBuffer = new List<IPersistableScheduleData>();
			else
				changedEntitiesBuffer = new List<IPersistableScheduleData>(refreshedEntities);

			var result = new ConflictHandlingResult { ConflictsFound = false, DialogResult = PersistConflictDialogResult.None };
			result.ConflictsFound = conflicts.Any();
			if (result.ConflictsFound)
				result.DialogResult = showPersistConflictView(changedEntitiesBuffer, conflicts);

			_undoRedo.Clear(); //see if this can be removed later... Should undo/redo work after refresh?
			foreach (var data in changedEntitiesBuffer)
			{
				_schedulerState.MarkDateToBeRecalculated(new DateOnly(data.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo)));
				_personsToValidate.Add(data.Person);
			}
			return result;
		}

		private PersistConflictDialogResult showPersistConflictView(List<IPersistableScheduleData> modifiedData, IEnumerable<IPersistConflict> conflicts)
		{
			PersistConflictDialogResult dialogResult;
			using (var conflictForm = new PersistConflictView(_schedulerState.Schedules, conflicts, modifiedData))
			{
				conflictForm.ShowDialog();
				dialogResult = conflictForm.DialogResult;
			}
			return dialogResult;
		}

		private void refreshEntitiesUsingMessageBroker(ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_schedulerMessageBrokerHandler.Refresh(refreshedEntitiesBuffer, conflictsBuffer);
			}
		}

		public void SizeOfMessageBrokerQueue(int count)
		{
			toolStripButtonRefresh.Enabled = count != 0;
		}

		private void toolStripComboBoxExFilterDays_SelectedIndexChanged(object sender, EventArgs e)
		{
			TimeSpan span = TimeSpan.FromDays(Convert.ToDouble(toolStripComboBoxExFilterDays.Text, CultureInfo.CurrentCulture));
			_requestView.FilterDays(span);
		}

		private void toolStripButtonRefresh_Click(object sender, EventArgs e)
		{
			try
			{
				refreshEntitiesUsingMessageBroker();
				_schedulerState.Schedules.ForEach(p => p.Value.ForceRecalculationOfContractTimeAndDaysOff());
				RecalculateResources();
			}
			catch (DataSourceException dataSourceException)
			{
				//rk - dont like this but cannot easily find "the spot" to catch these exception in current design
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
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
			IList<ISkillStaffPeriod> skillStaffPeriods = _schedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(_schedulerState.SchedulingResultState.Skills, _schedulerState.LoadedPeriod.Value);
			foreach (ISkillStaffPeriod period in skillStaffPeriods)
			{
				period.Payload.UseShrinkage = useShrinkage;
				_schedulerState.MarkDateToBeRecalculated(new DateOnly(period.Period.StartDateTimeLocal(_schedulerState.TimeZoneInfo)));
			}

			RecalculateResources();
			((ToolStripMenuItem)_contextMenuSkillGrid.Items["UseShrinkage"]).Checked = useShrinkage;
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
				var reportDetail = ReportHandler.CreateReportDetail(ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport));
				ReportHandler.ShowReport(reportDetail, _scheduleView, SchedulerState.RequestedScenario, CultureInfo.CurrentCulture);
			}
			Cursor.Current = Cursors.Default;
		}

		private void ToolStripMenuItemSearch_Click(object sender, EventArgs e)
		{
			DisplaySearch();
		}

		public void DisplaySearch()
		{
			IList<IPerson> persons = new List<IPerson>(SchedulerState.FilteredPersonDictionary.Values);

			using (SearchPerson searchForm = new SearchPerson(persons))
			{
				searchForm.ShowDialog(this);

				if (searchForm.DialogResult == DialogResult.OK)
				{
					if (searchForm.SelectedPerson != null)
					{
						int row = _scheduleView.GetRowForAgent(searchForm.SelectedPerson);
						GridRangeInfo info = GridRangeInfo.Cells(row, 0, row, 0);
						_scheduleView.TheGrid.Selections.Clear(true);
						_scheduleView.TheGrid.CurrentCell.Activate(row, 0, GridSetCurrentCellOptions.SetFocus);
						_scheduleView.TheGrid.Selections.ChangeSelection(info, info, true);
						_scheduleView.TheGrid.CurrentCell.MoveTo(row, 0, GridSetCurrentCellOptions.ScrollInView);
					}
				}
			}
		}

		private void ToolStripMenuItemViewDetails_Click(object sender, EventArgs e)
		{
			showRequestDetailsView(null);
		}

		private void showRequestDetailsView(EventParameters<ShowRequestDetailsView> eventParameters)
		{
			if (!isViewRequestDetailsAvailable()) return;
			var requestDetailsView = new RequestDetailsView(_eventAggregator, _requestView.SelectedAdapters().First(), _schedulerState.Schedules);
			requestDetailsView.Show(this);
		}

		private void toolStripMenuItemLoggedOnUserTimeZoneMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			var item = (ToolStripMenuItem)sender;
			_schedulerState.TimeZoneInfo = (TimeZoneInfo)item.Tag;
			wpfShiftEditor1.SetTimeZone(_schedulerState.TimeZoneInfo);
			foreach (ToolStripMenuItem downItem in toolStripMenuItemViewPointTimeZone.DropDownItems)
			{
				downItem.Checked = (_schedulerState.TimeZoneInfo == downItem.Tag);
			}
			if (_scheduleView != null && _scheduleView.HelpId == "AgentRestrictionsDetailView")
			{
				prepareAgentRestrictionView(null, _scheduleView, new List<IPerson>(_scheduleView.AllSelectedPersons()));
			}
			_grid.Invalidate();
			_grid.Refresh();
			updateSelectionInfo(_scheduleView.SelectedSchedules());
			updateShiftEditor();
			drawSkillGrid();
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
			bool viewSchedulesPermission = isPermittedToViewSchedules();
			_schedulerMeetingHelper.MeetingComposerStart(null, _scheduleView, true, viewSchedulesPermission);
		    
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

		private void toolStripSplitButtonUnlock_ButtonClick(object sender, EventArgs e)
		{
			LockManager.Clear();
			Refresh();
			RefreshSelection();
		}

		private void toolStripMenuItemSwapRawClick(object sender, EventArgs e)
		{
			if (_scheduleView != null)
				SwapRaw();
		}

		private void toolStripMenuItemFindMatching_Click(object sender, EventArgs e)
		{
			IScheduleDay selected = _scheduleView.SelectedSchedules()[0];
			findMatching(selected);
		}

		private void toolStripMenuItemFindMatching2_Click(object sender, EventArgs e)
		{
			if (_requestView.SelectedAdapters().Count == 0)
				return;
			if (_requestView.SelectedAdapters().Count > 1)
				return;
			var selectedRequest = _requestView.SelectedAdapters().First();
			if (!selectedRequest.IsEditable)
				return;
			if (!selectedRequest.IsPending)
				return;
			if (!selectedRequest.IsWithinSchedulePeriod)
				return;

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
				var form = new FindMatchingNew(selected.Person, selected.DateOnlyAsPeriod.DateOnly,
				                               _schedulerState.SchedulingResultState, _schedulerState.FilteredPersonDictionary.Values)
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
					paste();
					updateShiftEditor();
				}
			}
		}

		private void toolStripMenuItemViewHistory_Click(object sender, EventArgs e)
		{
			var selected = _scheduleView.SelectedSchedules()[0];
        	bool isLocked = _gridLockManager.HasLocks && _gridLockManager.Gridlocks(selected) != null;

        	using (var auditHistoryView = new AuditHistoryView(selected, this))
			{
				auditHistoryView.ShowDialog(this);
				if (auditHistoryView.DialogResult != DialogResult.OK || auditHistoryView.SelectedScheduleDay == null ||
					isLocked) return;
				var schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(SchedulerState.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), SchedulerState), new ScheduleTagSetter(_defaultScheduleTag));
				var restoreSchedulePartService = new RestoreSchedulePartService(schedulePartModifyAndRollbackService, selected, auditHistoryView.SelectedScheduleDay);
				restoreSchedulePartService.Restore();
				updateShiftEditor();
			}
		}

		private void toolStripButtonViewAllowance_Click(object sender, EventArgs e)
		{
			showRequestAllowanceView();
		}

		private void toolStripMenuItemViewAllowance_Click(object sender, EventArgs e)
		{
            isWindowLoaded = false;
			showRequestAllowanceView();
		}

		private void showRequestAllowanceView()
		{
			var defaultRequest = _requestView.SelectedAdapters().Count > 0
				                     ? _requestView.SelectedAdapters().First().PersonRequest
				                     : _schedulerState.PersonRequests.FirstOrDefault(
					                     r =>
					                     r.Request is AbsenceRequest &&
					                     _schedulerState.RequestedPeriod.Period().Contains(r.Request.Period));

			if (defaultRequest == null)
			{
				var allowanceView = new RequestAllowanceView(null, _defaultFilterDate);
			    
                if (!isWindowLoaded)
                {
                    allowanceView.Show(this);
                    isWindowLoaded = true;
                    allowanceView.FormClosed += allowanceView_FormClosed;
                }
                else
                {
                    isWindowLoaded = false;
                }
			}
			else
			{
				var requestDate = new DateOnly(defaultRequest.RequestedDate);
                var personPeriod = defaultRequest.Person.PersonPeriodCollection.FirstOrDefault(p => p.Period.Contains(requestDate));
				if (personPeriod != null)
				{
					var allowanceView = new RequestAllowanceView(personPeriod.BudgetGroup, requestDate);

                    if (!isWindowLoaded)
                    {
                        allowanceView.Show(this);
                        isWindowLoaded = true;
                        allowanceView.FormClosed += allowanceView_FormClosed;
                    }
                    else
                    {
                        isWindowLoaded = false;
                    }
				}
			}
		}

	    private void allowanceView_FormClosed(object sender, FormClosedEventArgs e)
	    {
	        isWindowLoaded = false;
	    }

	    private void toolStripViewRequestHistory_Click(object sender, EventArgs e)
		{
            var id = Guid.Empty;
			var defaultRequest = _requestView.SelectedAdapters().Count > 0 ? _requestView.SelectedAdapters().First().PersonRequest : _schedulerState.PersonRequests.FirstOrDefault(r => r.Request is AbsenceRequest);
			if (defaultRequest != null)
                id = defaultRequest.Person.Id.GetValueOrDefault();
			var presenter = _container.BeginLifetimeScope().Resolve<IRequestHistoryViewPresenter>();
			presenter.ShowHistory(id, _schedulerState.FilteredPersonDictionary.Values);
		}

		private void toolStripExTags_SizeChanged(object sender, EventArgs e)
		{
			toolStripSplitButtonChangeTag.Width = toolStripComboBoxAutoTag.Width;
		}

		private void ToolStripMenuItemContractTimeAscMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeAscendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemContractTimeDescMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) _scheduleView.Sort(new SortByContractTimeDescendingCommand(SchedulerState));
		}

		private void ToolStripMenuItemExportToPDFShiftsPerDay_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			ExportToPdf(true);
		}

		private void toolStripMenuItemRestrictionCopy_Click(object sender, EventArgs e)
		{
			toolStripMenuItemCopy_Click(sender, e);
		}

		private void toolStripMenuItemRestrictionPaste_Click(object sender, EventArgs e)
		{
			((AgentRestrictionsDetailView)_scheduleView).PasteSelectedRestrictions(_undoRedo);
		}

		private void toolStripMenuItemRestrictionDelete_Click(object sender, EventArgs e)
		{
			((AgentRestrictionsDetailView)_scheduleView).DeleteSelectedRestrictions(_undoRedo, _defaultScheduleTag);
		}

		void editControlRestrictionsNewClicked(object sender, EventArgs e)
		{
			addPreferenceToolStripMenuItemClick(sender, e);
		}

		void editControlRestrictionsNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			_editControlRestrictions.CloseDropDown();
			if ((ClipboardItems)e.ClickedItem.Tag == ClipboardItems.StudentAvailability)
				addStudentAvailabilityToolStripMenuItemClick(sender, e);
			if ((ClipboardItems)e.ClickedItem.Tag == ClipboardItems.Preference)
				addPreferenceToolStripMenuItemClick(sender, e);
		}

		private void addPreferenceToolStripMenuItemClick(object sender, EventArgs e)
		{
			var selectedDay = _scheduleView.SelectedSchedules()[0];

			using (var view = new AgentPreferenceView(selectedDay, _schedulerState))
			{
				view.ShowDialog(this);
				updateRestrictions(view.ScheduleDay);
			}
		}

		private void addStudentAvailabilityToolStripMenuItemClick(object sender, EventArgs e)
		{
			var selectedDay = _scheduleView.SelectedSchedules()[0];
			using (var view = new AgentStudentAvailabilityView(selectedDay))
			{
				view.ShowDialog(this);
				updateRestrictions(view.ScheduleDay);
			}
		}

		private void updateRestrictions(IScheduleDay scheduleDay)
		{
			if (_scheduleView == null || scheduleDay == null) return;
			_scheduleView.Presenter.LastUnsavedSchedulePart = scheduleDay;
			_scheduleView.Presenter.UpdateRestriction();
			if (_scheduleView is AgentRestrictionsDetailView)
			{
				schedulerSplitters1.RecalculateRestrictions();
				schedulerSplitters1.AgentRestrictionGrid.LoadData(schedulerSplitters1.SchedulingOptions);
				updateSelectionInfo(new List<IScheduleDay>{scheduleDay});
			}

			enableSave();
		}
	}
}
//Cake-in-the-kitchen if* this reaches 5000! 
//bigmac-in-the-kitchen if* this reaces 6000!
//new-mailfooter-in-the-kitchen if* this reaces 7000!
//naken-kebab-in-the-kitchen if* this reaces 8000!
//*when
