using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Editor;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Notes;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.UserTexts;

using Cursors = System.Windows.Forms.Cursors;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class IntradayViewContent : BaseUserControl
	{
		private IntradayScheduleView _scheduleView;
		private PinnedLayerView pinnedLayerView;
		private WpfShiftEditor wpfShiftEditor1;
		private NotesEditor notesEditor1;
		private IntradayPresenter _presenter;
		private GridChartManager _gridChartManager;
		private SkillIntraDayGridControl _skillIntradayGridControl;
		private IIntradayView _owner;
		private readonly GridlockManager _gridLockManager = new GridlockManager();
		private readonly ClipHandler<IScheduleDay> _clipHandlerSchedule = new ClipHandler<IScheduleDay>();
		private readonly BackgroundWorker _skillGridBackgroundLoader = new BackgroundWorker();
		private readonly BackgroundWorker _backgroundWorkerResources = new BackgroundWorker();
		private IScheduleDay _lastSchedulePartInEditor;
		private readonly IEventAggregator _eventAggregator;
		private readonly IntradaySettingManager _settingManager;
		private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private ISchedulerStateHolder _schedulerStateHolder;
		private bool _userWantsToCloseIntraday;
		private MultipleHostControl shiftEditorHost;
		private readonly IResourceOptimizationHelperExtended _resourceOptimizationHelperExtended;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ISkillPriorityProvider _skillPriorityProvider;

		public IntradayViewContent(IntradayPresenter presenter, IIntradayView owner, IEventAggregator eventAggregator, ISchedulerStateHolder schedulerStateHolder,
			 IntradaySettingManager settingManager, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, IResourceOptimizationHelperExtended resourceOptimizationHelperExtended,
			 CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IScheduleDayChangeCallback scheduleDayChangeCallback, ISkillPriorityProvider skillPriorityProvider)
		{
			if (presenter == null) throw new ArgumentNullException("presenter");
			if (owner == null) throw new ArgumentNullException("owner");
			if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");

			_eventAggregator = eventAggregator;
			_settingManager = settingManager;
			_overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_skillPriorityProvider = skillPriorityProvider;
			_presenter = presenter;
			_schedulerStateHolder = schedulerStateHolder;
			_owner = owner;

			InitializeComponent();
			if (DesignMode) return;
			SetTexts();
		}

		private void dockingManager1AutoHideAnimationStart(object sender, AutoHideAnimationEventArgs arg)
		{
			if (arg.DockBorder == DockStyle.Left || arg.DockBorder == DockStyle.Right)
				DockingManager.AnimationStep = arg.Bounds.Width;
			else
				DockingManager.AnimationStep = arg.Bounds.Height;
		}

		private void dockingManager1DockStateChanged(object sender, DockStateChangeEventArgs arg)
		{
			_settingManager.HasUnsavedLayout = true;
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		private void _backgroundWorkerResources_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
		}

		private void backgroundWorkerResourcesRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing || IsDisposed) return;
			DrawSkillGrid(true);
			if (dataSourceExceptionOccurred(e))
				_owner.FinishProgress();
		}

		private void backgroundWorkerResourcesDoWork(object sender, DoWorkEventArgs e)
		{
			//Claes & Roger: we don't know if intrainterval calc needs to be done. We keep this as before
			if(!_schedulerStateHolder.SchedulingResultState.Skills.Any()) return;

			var period = new DateOnlyPeriod(_schedulerStateHolder.DaysToRecalculate.Min(), _schedulerStateHolder.DaysToRecalculate.Max());
			using (_resourceCalculationContextFactory.Create(_schedulerStateHolder.SchedulingResultState, false, period))
			{
				_resourceOptimizationHelperExtended.ResourceCalculateMarkedDays(new BackgroundWorkerWrapper(_backgroundWorkerResources), true, true);
			}
		}

		private void calculateResources()
		{
			if (Disposing || IsDisposed) return;
			if (!_backgroundWorkerResources.IsBusy)
				_backgroundWorkerResources.RunWorkerAsync();
		}

		private void dockingManager1NewDockStateEndLoad(object sender, EventArgs e)
		{
			if (Disposing || IsDisposed) return;

			authorizeFunctions();
			//This is done here to make sure that texts are set in RTL cultures as well
			dockingManager1.SetDockLabel(elementHostDayLayerView, Resources.DayView);
			dockingManager1.SetDockLabel(elementHostShiftEditor, Resources.ShiftEditor);
			dockingManager1.SetDockLabel(panelSkillGrid, Resources.SkillData);
			dockingManager1.SetDockLabel(panelSkillChart, Resources.Chart);
			dockingManager1.SetDockLabel(elementHostPinnedLayerView, Resources.PinnedUpWindow);
			dockingManager1.SetDockLabel(elementHostAgentState, Resources.StategroupOverview);
			dockingManager1.SetDockLabel(elementHostStaffingEffect, Resources.StaffingEffect);
		}

		private void intradayViewContentLoad(object sender, EventArgs e)
		{
			shiftEditorHost = new MultipleHostControl();
			wpfShiftEditor1 = new WpfShiftEditor(_eventAggregator, new CreateLayerViewModelService(), false) { MinHeight = 80 };
			notesEditor1 = new NotesEditor(true) { MinHeight = 80 };
			shiftEditorHost.AddItem(Resources.ShiftEditor, wpfShiftEditor1);
			shiftEditorHost.AddItem(Resources.Note, notesEditor1);
			pinnedLayerView = new PinnedLayerView();
			elementHostShiftEditor.Child = shiftEditorHost;
			elementHostPinnedLayerView.Child = pinnedLayerView;

			_settingManager.Initialize(dockingManager1);
			_backgroundWorkerResources.WorkerReportsProgress = true;
			_backgroundWorkerResources.DoWork += backgroundWorkerResourcesDoWork;
			_backgroundWorkerResources.RunWorkerCompleted += backgroundWorkerResourcesRunWorkerCompleted;
			_backgroundWorkerResources.ProgressChanged += _backgroundWorkerResources_ProgressChanged;

			_skillIntradayGridControl = new SkillIntraDayGridControl(_settingManager.ChartSetting, _skillPriorityProvider);
			_skillIntradayGridControl.SelectionChanged += skillIntradayGridControlSelectionChanged;
			InitializeIntradayViewContent();
		}

		public void InitializeIntradayViewContent()
		{
			setupDayView();
			setupChart();
			SetupSkillTabs();
			SelectSkillTab(SelectedSkill);
			DrawSkillGrid(false);

			Visible = true;

			//_defaultSetting = SaveDockingState();
		}

		private void setupDayView()
		{
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.InitializingDayViewThreeDots);

			RefreshRealTimeScheduleControls();

			_skillGridBackgroundLoader.DoWork += skillGridBackgroundLoaderDoWork;
			_skillGridBackgroundLoader.RunWorkerCompleted += skillGridBackgroundLoaderRunWorkerCompleted;
			chartControlSkillData.Name = "IntradayChartView";
			tabSkillData.Name = "SkillView";
			_skillIntradayGridControl.Name = "SkillView";
			_owner.AddControlHelpContext(chartControlSkillData);
			_owner.AddControlHelpContext(tabSkillData);

			_scheduleView = new IntradayScheduleView(_skillIntradayGridControl, _owner, _presenter.SchedulerStateHolder, _gridLockManager, SchedulePartFilter.None, _clipHandlerSchedule,
				 _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance, new UndoRedoContainer());

			wpfShiftEditor1.LoadFromStateHolder(_presenter.SchedulerStateHolder.CommonStateHolder);
			wpfShiftEditor1.CommitChanges += shiftEditorCommitChanges;
			wpfShiftEditor1.ShiftUpdated += wpfShiftEditor1ShiftUpdated;
			wpfShiftEditor1.AddAbsence += wpfShiftEditorAddAbsence;
			wpfShiftEditor1.AddActivity += wpfShiftEditorAddActivity;
			wpfShiftEditor1.AddOvertime += wpfShiftEditorAddOvertime;
			wpfShiftEditor1.AddPersonalShift += wpfShiftEditorAddPersonalShift;
			wpfShiftEditor1.SetTimeZone(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			dayLayerView1.UpdateShiftEditor += dayLayerView1UpdateShiftEditor;
			notesEditor1.NotesChanged += notesEditor1NotesChanged;
			notesEditor1.PublicNotesChanged += notesEditor1NotesChanged;


			ToggleSchedulePartModified(true);
			timerRefreshSchedule.Enabled = _presenter.RealTimeAdherenceEnabled;
			Cursor = Cursors.Default;
		}

		public void RefreshRealTimeScheduleControls()
		{
			if (_presenter.RealTimeAdherenceEnabled || _presenter.EarlyWarningEnabled)
			{
				var model = _presenter.CreateDayLayerViewModel();
				model.InitializeRows();
				dayLayerView1.SetDayLayerViewModel(model);
				pinnedLayerView.SetDayLayerViewCollection(model);

				dayLayerView1.Model.Period = _presenter.SchedulerStateHolder.RequestedPeriod.Period();

				if (_presenter.RealTimeAdherenceEnabled)
				{
					staffingEffectView1.SetDayLayerViewAdapterCollection(model);
					agentStateLayerView1.SetAgentStateViewAdapterCollection(
						 _presenter.CreateAgentStateViewAdapterCollection(model));
				}
			}
		}

		private void skillGridBackgroundLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing || IsDisposed) return;
			if (_userWantsToCloseIntraday) return;
			if (dataSourceExceptionOccurred(e))
			{
				_owner.FinishProgress();
				return;
			}

			_skillIntradayGridControl.SetRowsAndCols();
			positionControl(_skillIntradayGridControl);
			reloadChart();

			_owner.FinishProgress();
		}

		private static bool dataSourceExceptionOccurred(RunWorkerCompletedEventArgs e)
		{
			if (e.Error == null) return false;

			var dataSourceException = e.Error as DataSourceException;
			if (dataSourceException == null)
				return false;

			return showDataSourceException(dataSourceException);
		}

		private static bool showDataSourceException(DataSourceException dataSourceException)
		{
			using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
			{
				view.ShowDialog();
			}
			return true;
		}

		private void skillGridBackgroundLoaderDoWork(object sender, DoWorkEventArgs e)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;

			IList<ISkillStaffPeriod> periods = _presenter.PrepareSkillIntradayCollection();
			_skillIntradayGridControl.SetupDataSource(periods, SelectedSkill, true, _presenter.SchedulerStateHolder);

			e.Result = e.Argument;
		}

		private void backgroundWorkerFetchDataDoWork(object sender, DoWorkEventArgs e)
		{
			_scheduleView.Presenter.Now = DateTime.UtcNow;
		}

		private void backgroundWorkerFetchDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing || IsDisposed) return;

			if (dataSourceExceptionOccurred(e))
			{
				_owner.FinishProgress();
				return;
			}
			using (PerformanceOutput.ForOperation("Updating GUI"))
			{
				dayLayerView1.Model.NowPeriod = new DateTimePeriod(_scheduleView.Presenter.Now, _scheduleView.Presenter.Now.AddMinutes(1));
				agentStateLayerView1.NowPeriod = dayLayerView1.Model.NowPeriod;
			}
		}

		private void authorizeFunctions()
		{
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.AuthorizingFunctionsThreeDots);

			var control = dockingManager1.ActiveControl;

			dockingManager1.ActivateControl(elementHostDayLayerView);
			dockingManager1.ActivateControl(elementHostShiftEditor);
			dockingManager1.ActivateControl(panelSkillGrid);
			dockingManager1.ActivateControl(elementHostPinnedLayerView);
			dockingManager1.ActivateControl(elementHostAgentState);
			dockingManager1.ActivateControl(elementHostStaffingEffect);
			dockingManager1.ActivateControl(panelSkillChart);

			dockingManager1.ActivateControl(control);

			var rtaOrEwEnabled = _presenter.RealTimeAdherenceEnabled || _presenter.EarlyWarningEnabled;
			dockingManager1.SetHiddenOnLoad(panelSkillChart, !_presenter.EarlyWarningEnabled);
			dockingManager1.SetHiddenOnLoad(panelSkillGrid, !_presenter.EarlyWarningEnabled);
			dockingManager1.SetHiddenOnLoad(elementHostShiftEditor, !rtaOrEwEnabled);
			dockingManager1.SetHiddenOnLoad(elementHostDayLayerView, !rtaOrEwEnabled);
			dockingManager1.SetHiddenOnLoad(elementHostAgentState, !_presenter.RealTimeAdherenceEnabled);
			dockingManager1.SetHiddenOnLoad(elementHostPinnedLayerView, !_presenter.RealTimeAdherenceEnabled);
			dockingManager1.SetHiddenOnLoad(elementHostStaffingEffect, !_presenter.RealTimeAdherenceEnabled);
			dockingManager1.SetDockVisibility(panelSkillChart, _presenter.EarlyWarningEnabled);
			dockingManager1.SetDockVisibility(panelSkillGrid, _presenter.EarlyWarningEnabled);
			dockingManager1.SetDockVisibility(elementHostShiftEditor, rtaOrEwEnabled);
			dockingManager1.SetDockVisibility(elementHostDayLayerView, rtaOrEwEnabled);
			dockingManager1.SetDockVisibility(elementHostAgentState, _presenter.RealTimeAdherenceEnabled);
			dockingManager1.SetDockVisibility(elementHostPinnedLayerView, _presenter.RealTimeAdherenceEnabled);
			dockingManager1.SetDockVisibility(elementHostStaffingEffect, _presenter.RealTimeAdherenceEnabled);
		}

		private void setupChart()
		{
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.InitializingChartThreeDots);
			_gridChartManager = new GridChartManager(chartControlSkillData, false);
			_gridChartManager.Create();
		}

		public void SetupSkillTabs()
		{
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.InitializingSkillTabsThreeDots);

			tabSkillData.SelectedIndexChanged -= tabSkillDataSelectedIndexChanged;
			tabSkillData.TabPages.Clear();

			foreach (var skill in _presenter.SchedulerStateHolder.SchedulingResultState.Skills.OrderBy(s => s.Name))
			{
				var tab = ColorHelper.CreateTabPage(skill.Name, skill.Description);
				tab.Tag = skill;
				tabSkillData.TabPages.Add(tab);
			}

			tabSkillData.SelectedIndexChanged += tabSkillDataSelectedIndexChanged;
		}

		private void tabSkillDataSelectedIndexChanged(object sender, EventArgs e)
		{
			DrawSkillGrid(true);
		}

		public void DrawSkillGrid(bool handleProgressSelf)
		{
			if (SelectedSkill == null || _skillGridBackgroundLoader.IsBusy)
			{
				// No tab selected
				return;
			}
			if (Disposing || IsDisposed) return;

			if (handleProgressSelf)
				_owner.StartProgress();

			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.ReloadingSkillGridThreeDots);
			_skillGridBackgroundLoader.RunWorkerAsync(handleProgressSelf);
		}

		public ISkill SelectedSkill
		{
			get { return tabSkillData.SelectedTab == null ? null : tabSkillData.SelectedTab.Tag as ISkill; }
		}

		public void SelectSkillTab(ISkill skill)
		{
			if (skill == null) return;

			int index = 0;
			TabPageAdv tabPage =
				 tabSkillData.TabPages.OfType<TabPageAdv>().FirstOrDefault(t => skill.Equals(t.Tag as ISkill));
			if (tabPage != null)
				index = tabSkillData.TabPages.IndexOf(tabPage);

			tabSkillData.SelectedIndex = index;
		}

		private void positionControl(Control control)
		{
			//remove control from all tabPages
			foreach (TabPageAdv tabPage in tabSkillData.TabPages)
			{
				tabPage.Controls.Clear();
			}

			TabPageAdv tab = tabSkillData.TabPages[tabSkillData.SelectedIndex];
			tab.Controls.Add(control);

			//position grid
			control.Dock = DockStyle.Fill;
		}

		public void ReloadScheduleDayInEditor(IPerson person)
		{
			if (_lastSchedulePartInEditor != null)
			{
				if (person == null)
				{
					person = _lastSchedulePartInEditor.Person;
				}
				if (_lastSchedulePartInEditor.Person.Equals(person))
				{
					var currentPart =
						 _presenter.SchedulerStateHolder.Schedules[person].ReFetch(
							  _lastSchedulePartInEditor);
					wpfShiftEditor1.LoadSchedulePart(currentPart);
					notesEditor1.LoadNote(currentPart);
				}
			}
		}

		public void UpdateShiftEditor(IList<IScheduleDay> scheduleParts)
		{
			if (_scheduleView.Presenter.LastUnsavedSchedulePart != null) UpdateFromEditor();
			IScheduleDay currentPart = null;

			if (scheduleParts.Count == 0 &&
				 _lastSchedulePartInEditor != null)
			{
				currentPart = _lastSchedulePartInEditor;
			}

			wpfShiftEditor1.LoadSchedulePart(null);
			notesEditor1.LoadNote(null);
			if (scheduleParts.Count == 1)
			{
				currentPart = scheduleParts[0];

			}
			if (currentPart != null)
			{
				currentPart =
					 _presenter.SchedulerStateHolder.Schedules[currentPart.Person].ReFetch(currentPart);
			}
			wpfShiftEditor1.LoadSchedulePart(currentPart);
			notesEditor1.LoadNote(currentPart);
		}

		private void reloadChart()
		{
			if (Disposing || IsDisposed) return;

			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.ReloadingChartThreeDots);

			_gridChartManager.ReloadChart(_skillIntradayGridControl,
													IntradayPresenter.PrepareChartDescription("{0} - {1}",
																											Resources.Intraday,
																											_presenter.ChartIntradayDescription));

			chartControlSkillData.Visible = true;
		}

		public void UpdateFromEditor()
		{
			if (_scheduleView != null)
			{
				_scheduleView.Presenter.UpdateFromEditor();
			}
		}

		private void wpfShiftEditorAddActivity(object sender, ShiftEditorEventArgs e)
		{
			_scheduleView.Presenter.AddActivity(new List<IScheduleDay> { e.SchedulePart }, e.Period, null);
			invalidateScheduleView(e.SchedulePart);
		}

		private void invalidateScheduleView(IScheduleDay schedulePart)
		{
			if (schedulePart == null) return;
			UpdateShiftEditor(new List<IScheduleDay> { schedulePart });
			dayLayerView1.RefreshProjection(schedulePart.Person);
			_owner.EnableSave();
		}

		private void wpfShiftEditorAddPersonalShift(object sender, ShiftEditorEventArgs e)
		{
			_scheduleView.Presenter.AddPersonalShift(new List<IScheduleDay> { e.SchedulePart }, e.Period);
			invalidateScheduleView(e.SchedulePart);
		}

		private void wpfShiftEditorAddOvertime(object sender, ShiftEditorEventArgs e)
		{
			_scheduleView.Presenter.AddOvertime(new List<IScheduleDay> { e.SchedulePart }, null, _presenter.MultiplicatorDefinitionSets.ToList());
			invalidateScheduleView(e.SchedulePart);
		}

		private void wpfShiftEditorAddAbsence(object sender, ShiftEditorEventArgs e)
		{
			_scheduleView.Presenter.AddAbsence(new List<IScheduleDay> { e.SchedulePart }, e.Period);
			invalidateScheduleView(e.SchedulePart);
		}

		private void wpfShiftEditor1ShiftUpdated(object sender, ShiftEditorEventArgs e)
		{
			_scheduleView.Presenter.LastUnsavedSchedulePart = e.SchedulePart;
		}

		void notesEditor1NotesChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			_scheduleView.Presenter.LastUnsavedSchedulePart = notesEditor1.SchedulePart;
			UpdateFromEditor();
			invalidateScheduleView(notesEditor1.SchedulePart);
		}

		private void shiftEditorCommitChanges(object sender, ShiftEditorEventArgs e)
		{
			UpdateFromEditor();
			invalidateScheduleView(e.SchedulePart);
		}

		private void skillIntradayGridControlSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (_skillIntradayGridControl.CurrentSelectedGridRow != null)
			{
				IChartSeriesSetting chartSeriesSettings = _skillIntradayGridControl.CurrentSelectedGridRow.ChartSeriesSettings;
				_owner.SetChartButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation,
											  chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
			}
		}

		public void RefreshPerson(IPerson person)
		{
			if (person != null)
				dayLayerView1.RefreshProjection(person);
		}

		private void dayLayerView1UpdateShiftEditor(object sender, ShiftEditorEventArgs e)
		{
			_lastSchedulePartInEditor = e.SchedulePart;
			UpdateShiftEditor(new List<IScheduleDay> { e.SchedulePart });
			shiftEditorHost.UpdateItems();
		}

		private void schedulesPartModified(object sender, ModifyEventArgs e)
		{
			_presenter.SchedulerStateHolder.MarkDateToBeRecalculated(new DateOnly(e.ModifiedPeriod.StartDateTimeLocal(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone)));
			if (e.Modifier != ScheduleModifier.MessageBroker)
				_owner.EnableSave();

			calculateResources();
		}

		private void chartControlSkillDataChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			var column2 = (int)Math.Round(GridChartManager.GetIntervalValueForChartPoint(chartControlSkillData, e.Point));
			if (column2 > 0)
			{
				_skillIntradayGridControl.Model.ScrollCellInView(GridRangeInfo.Col(column2), GridScrollCurrentCellReason.MoveTo);
				_skillIntradayGridControl.Model.Selections.SelectRange(GridRangeInfo.Cell(0, column2), true);
			}
		}

		private void chartControlSkillDataChartRegionMouseEnter(object sender, ChartRegionMouseEventArgs e)
		{
			GridChartManager.SetChartToolTip(e.Region, chartControlSkillData);
		}

		public void SelectChartView(string chartView)
		{
			_settingManager.SetChartView(chartView);
			_skillIntradayGridControl.ReloadChartSettings(_settingManager.ChartSetting);
			reloadChart();
		}

		public void LineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_skillIntradayGridControl.CurrentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis, e.LineColor);
			UpdateChartSettings(_skillIntradayGridControl.CurrentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis, e.LineColor);
		}

		public void LineInChartEnabledChanged(GridlineInChartButtonEventArgs e, GridRowInChartSettingButtons setting)
		{
			if (_skillIntradayGridControl.CurrentSelectedGridRow == null)
				return;
			_gridChartManager.UpdateChartSettings(_skillIntradayGridControl.CurrentSelectedGridRow, setting, e.Enabled);
			SetRowVisibility(_skillIntradayGridControl.CurrentSelectedGridRow.ChartSeriesSettings.DisplayKey, e.Enabled);
			reloadChart();
		}

		public void UpdateChartSettings(GridRow row, bool enabled, ChartSeriesDisplayType type, AxisLocation axis, Color color)
		{
			if (row == null) return;
			IChartSeriesSetting chartSeriesSetting = _settingManager.CurrentIntradaySetting.ChartSetting.DefinedSetting(
																															row.DisplayMember,
																															new ChartSettingsManager().ChartSettingsDefault);
			chartSeriesSetting.Enabled = enabled;
			chartSeriesSetting.Color = color;
			chartSeriesSetting.SeriesType = type;
			chartSeriesSetting.AxisLocation = axis;
		}

		public void SetRowVisibility(string key, bool enabled)
		{

			if (enabled)
			{
				if (!_settingManager.CurrentIntradaySetting.ChartSetting.SelectedRows.Contains(key))
					_settingManager.CurrentIntradaySetting.ChartSetting.SelectedRows.Add(key);
			}
			else
			{
				_settingManager.CurrentIntradaySetting.ChartSetting.SelectedRows.Remove(key);
			}
		}

		public void ToggleSchedulePartModified(bool enable)
		{
			if (_schedulerStateHolder == null || _schedulerStateHolder.Schedules == null) return;
			if (enable)
				_schedulerStateHolder.Schedules.PartModified += schedulesPartModified;
			else
				_schedulerStateHolder.Schedules.PartModified -= schedulesPartModified;
		}

		public void Close()
		{
			_userWantsToCloseIntraday = true;
		}

		private void cleanUp()
		{
			ToggleSchedulePartModified(false);
			dayLayerView1.ReleaseManagedResources();
			dayLayerView1.UnregisterMessageBrokerEvents();

			_schedulerStateHolder = null;
		}

		private void timerRefreshScheduleTick(object sender, EventArgs e)
		{
			if (Disposing || IsDisposed) return;
			if (!backgroundWorkerFetchData.IsBusy)
			{
				backgroundWorkerFetchData.RunWorkerAsync();
			}
		}
	}
}
