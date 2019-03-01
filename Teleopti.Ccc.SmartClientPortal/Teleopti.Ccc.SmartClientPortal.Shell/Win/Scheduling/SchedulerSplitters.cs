using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autofac;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class SchedulerSplitters : BaseUserControl
	{
		//Most of those needs to be properties with backend field, otherwise can't work in the designer for this control
		private readonly PinnedSkillHelper _pinnedSkillHelper;
		private IList<IPerson> _filteredPersons = new List<IPerson>();
		private readonly ContextMenuStrip _contextMenuSkillGrid;
		private readonly SplitterManagerRestrictionView _splitterManager;
		private ISchedulerStateHolder _schedulerStateHolder;
		private SkillDayGridControl _skillDayGridControl;
		private SkillIntraDayGridControl _skillIntraDayGridControl;
		private SkillWeekGridControl _skillWeekGridControl;
		private SkillMonthGridControl _skillMonthGridControl;
		private SkillFullPeriodGridControl _skillFullPeriodGridControl;
		private SkillResultViewSetting _skillResultViewSetting;
		private DateOnly _currentIntraDayDate;
		private string _chartDescription;
		private IVirtualSkillHelper _virtualSkillHelper;
		private ChartControl _chartControlSkillData;
		private TeleoptiLessIntelligentSplitContainer _splitContainerView;
		private GridControl _grid;
		private ElementHost _elementHostRequests;
		private HandlePersonRequestView _handlePersonRequestView1;
		private ElementHost _elementHost1;
		private MultipleHostControl _multipleHostControl3;

		public event EventHandler<System.ComponentModel.ProgressChangedEventArgs>
			RestrictionsNotAbleToBeScheduledProgress;

		public event EventHandler<ValidationViewAgentDoubleClickEvenArgs> ValidationAlertsAgentDoubleClick;

		public SchedulerSplitters()
		{
			InitializeComponent();
			_pinnedSkillHelper = new PinnedSkillHelper();
			tabSkillData.TabStyle = typeof(SkillTabRenderer);
			tabSkillData.TabPanelBackColor = Color.FromArgb(199, 216, 237);
			validationAlertsView1.AgentDoubleClick += validationAlertsView1AgentDoubleClick;
			_contextMenuSkillGrid = new ContextMenuStrip();
			_splitterManager = new SplitterManagerRestrictionView
			{
				MainSplitter = SplitContainerAdvMainContainer,
				LeftMainSplitter = lessIntellegentSplitContainerAdvMain,
				GraphResultSplitter = lessIntellegentSplitContainerAdvResultGraph,
				GridEditorSplitter = teleoptiLessIntelligentSplitContainerLessIntelligent1,
				RestrictionViewSplitter = SplitContainerView
			};

			GridChartManager = new GridChartManager(ChartControlSkillData, true, true, true);
			GridChartManager.Create();
			ChartControlSkillData.ChartRegionClick += chartControlSkillDataChartRegionClick;
			ChartControlSkillData.ChartRegionMouseHover += chartControlSkillDataChartRegionMouseHover;
		}

		public void InitializeSkillResultGrids(ILifetimeScope container)
		{
			_skillDayGridControl = new SkillDayGridControl(container.Resolve<ISkillPriorityProvider>(), container.Resolve<ITimeZoneGuard>());
			_skillWeekGridControl = new SkillWeekGridControl();
			_skillMonthGridControl = new SkillMonthGridControl();
			_skillFullPeriodGridControl = new SkillFullPeriodGridControl();
			_skillIntraDayGridControl = new SkillIntraDayGridControl("SchedulerSkillIntradayGridAndChart",
				container.Resolve<ISkillPriorityProvider>());

			DayGridControl.ContextMenuStrip = ContextMenuSkillGrid;
			IntraDayGridControl.ContextMenuStrip = ContextMenuSkillGrid;
			WeekGridControl.ContextMenuStrip = ContextMenuSkillGrid;
			MonthGridControl.ContextMenuStrip = ContextMenuSkillGrid;
			FullPeriodGridControl.ContextMenuStrip = ContextMenuSkillGrid;
		}

		public void Initialize(ILifetimeScope container, ISchedulerStateHolder schedulerStateHolder,
			SchedulerGroupPagesProvider schedulerGroupPagesProvider, IEnumerable<IOptionalColumn> optionalColumns, SchedulingScreenSettings currentSchedulingScreenSettings)
		{
			if (container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_PrepareToRemoveRightToLeft_81112))
			{
				if (!DesignMode) SetTextsNoRightToLeft();
				shiftCategoryDistributionControl1.PrepareToRemoveRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
				
			Grid.VScrollPixel = false;
			Grid.HScrollPixel = false;
			_virtualSkillHelper = container.Resolve<IVirtualSkillHelper>();
			_schedulerStateHolder = schedulerStateHolder;

			var requestedPeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			CurrentIntraDayDate = requestedPeriod.StartDate;
			var outerPeriod =
				new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-7), requestedPeriod.EndDate.AddDays(7));
			var agentInfoControl = new AgentInfoControl(schedulerGroupPagesProvider, container, outerPeriod,
				requestedPeriod, schedulerStateHolder, optionalColumns);

			tabInfoPanels.TabPages[0].Controls.Add(agentInfoControl);
			agentInfoControl.Dock = DockStyle.Fill;
			tabInfoPanels.Refresh();

			//container can fix this to one row
			ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson =
				new CachedNumberOfEachCategoryPerPerson(schedulerStateHolder.Schedules,
					schedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			ICachedNumberOfEachCategoryPerDate cachedNumberOfEachCategoryPerDate =
				new CachedNumberOfEachCategoryPerDate(schedulerStateHolder.Schedules,
					schedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			var allowedSc = new List<IShiftCategory>();
			foreach (var shiftCategory in schedulerStateHolder.CommonStateHolder.ShiftCategories)
			{
				if (shiftCategory is IDeleteTag sc && !sc.IsDeleted)
					allowedSc.Add(shiftCategory);
			}

			ICachedShiftCategoryDistribution cachedShiftCategoryDistribution =
				new CachedShiftCategoryDistribution(schedulerStateHolder.Schedules,
					schedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
					cachedNumberOfEachCategoryPerPerson,
					allowedSc);
			var shiftCategoryDistributionModel = new ShiftCategoryDistributionModel(cachedShiftCategoryDistribution,
				cachedNumberOfEachCategoryPerDate,
				cachedNumberOfEachCategoryPerPerson,
				schedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
				schedulerStateHolder.CommonNameDescription);
			shiftCategoryDistributionModel.SetFilteredPersons(schedulerStateHolder.FilteredCombinedAgentsDictionary
				.Values);
			shiftCategoryDistributionControl1.SetModel(shiftCategoryDistributionModel);
			agentsNotPossibleToSchedule1.InitAgentsNotPossibleToSchedule(
				container.Resolve<RestrictionNotAbleToBeScheduledReport>(), this);
			validationAlertsView1.SetModel(new ValidationAlertsModel(
				schedulerStateHolder.Schedules, NameOrderOption.LastNameFirstName,
				schedulerStateHolder.RequestedPeriod.DateOnlyPeriod));

			_contextMenuSkillGrid.Items["CreateSkillSummery"].Click += skillGridMenuItemClick;
			setupSkillTabs(currentSchedulingScreenSettings);
		}

		public void RefreshSummarySkillIfActive()
		{
			if (TabSkillData.SelectedIndex < 0) return;
			var tab = TabSkillData.TabPages[TabSkillData.SelectedIndex];
			var skill = (ISkill)tab.Tag;
			IAggregateSkill aggregateSkillSkill = skill;
			if (!aggregateSkillSkill.IsVirtual)
				return;

			var skillGridControl = resolveControlFromSkillResultViewSetting();
			if (skillGridControl is SkillIntraDayGridControl control)
			{
				var skillStaffPeriods =
					_schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						aggregateSkillSkill,
						TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
							_currentIntraDayDate.AddDays(1).Date, TimeZoneGuardForDesktop_DONOTUSE.Instance_DONTUSE.CurrentTimeZone()));
				control.Presenter.RowManager?.SetDataSource(skillStaffPeriods);
			}
			else
			{
				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return;

				selectedSkillGridControl.SetDataSource(_schedulerStateHolder, skill);
			}

			InvalidateSkillResultGrids();
			skillGridControl.Refresh();
		}

		public MultipleHostControl MultipleHostControl3 => _multipleHostControl3;

		public AgentInfoControl AgentInfoControl => tabInfoPanels.TabPages[0].Controls[0] as AgentInfoControl;

		public TeleoptiLessIntelligentSplitContainer SplitContainerAdvMainContainer => lessIntellegentSplitContainerAdvMainContainer;

		public TeleoptiLessIntelligentSplitContainer SplitContainerView { get => _splitContainerView; }

		public ChartControl ChartControlSkillData { get => _chartControlSkillData; }

		public ContextMenuStrip ContextMenuSkillGrid => _contextMenuSkillGrid;

		public TabControlAdv TabSkillData => tabSkillData;

		public ElementHost ElementHostRequests { get => _elementHostRequests; }

		public HandlePersonRequestView HandlePersonRequestView1 { get => _handlePersonRequestView1; }

		public ElementHost ElementHost1 { get => _elementHost1; }

		public GridControl Grid { get => _grid; }

		public SkillDayGridControl DayGridControl => _skillDayGridControl;

		public SkillIntraDayGridControl IntraDayGridControl => _skillIntraDayGridControl;

		public SkillWeekGridControl WeekGridControl => _skillWeekGridControl;

		public SkillMonthGridControl MonthGridControl => _skillMonthGridControl;

		public SkillFullPeriodGridControl FullPeriodGridControl => _skillFullPeriodGridControl;

		public SkillResultViewSetting SkillResultViewSetting
		{
			get => _skillResultViewSetting;
			set
			{
				_skillResultViewSetting = value;
				foreach (var item in ContextMenuSkillGrid.Items)
				{
					if (!((item as ToolStripMenuItem)?.Tag is SkillResultViewSetting)) continue;
					var itemTag = ((ToolStripMenuItem)item).Tag;
					(item as ToolStripMenuItem).Checked = ((SkillResultViewSetting)itemTag).Equals(_skillResultViewSetting);
				}
			}
		}

		public GridChartManager GridChartManager { get; }

		public DateOnly CurrentIntraDayDate
		{
			get => _currentIntraDayDate;
			set => _currentIntraDayDate = value;
		}

		public void ShowGraph(bool show)
		{
			_splitterManager.ShowGraph = show;
		}

		public void ShowEditor(bool show)
		{
			_splitterManager.ShowEditor = show;
		}

		public void ShowResult(bool show)
		{
			_splitterManager.ShowResult = show;
		}

		public void ShowRestrictionView(bool show)
		{
			_splitterManager.ShowRestrictionView = show;
		}

		public void EnableShiftEditor(bool enable)
		{
			if (enable)
			{
				_splitterManager.EnableShiftEditor();
			}
			else
			{
				_splitterManager.DisableShiftEditor();
			}
		}

		public void ReselectSelectedAgentNotPossibleToSchedule()
		{
			agentsNotPossibleToSchedule1.ReselectSelected();
		}

		public void SetSelectedAgentsOnAgentsNotPossibleToSchedule(IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedDates, AgentRestrictionsDetailView detailView)
		{
			agentsNotPossibleToSchedule1.SetSelected(selectedPersons, selectedDates, detailView);
		}

		public void EnableOrDisableViewShiftCategoryDistribution(bool enable)
		{
			shiftCategoryDistributionControl1.EnableOrDisableViewShiftCategoryDistribution(enable);
		}

		public void RefreshFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons.ToList();
			if (tabInfoPanels.SelectedIndex == 2)
				validationAlertsView1.ReDraw(_filteredPersons);

			if (tabInfoPanels.SelectedIndex == 1)
				shiftCategoryDistributionControl1.Model.SetFilteredPersons(_filteredPersons);

			tabInfoPanels.Refresh();
		}

		public void TogglePropertyPanel(bool value)
		{
			//fix to solve right to left cultures not showing panel on start
			lessIntellegentSplitContainerAdvMainContainer.Panel2Collapsed = !value;
			lessIntellegentSplitContainerAdvMainContainer.Panel2Collapsed = value;
			var defaultDistance = lessIntellegentSplitContainerAdvMainContainer.Width - 350;

			if (!value)
			{
				if (lessIntellegentSplitContainerAdvMainContainer.SplitterDistance >
					defaultDistance)
					lessIntellegentSplitContainerAdvMainContainer.SplitterDistance =
						defaultDistance;

				if (lessIntellegentSplitContainerAdvMainContainer.SplitterDistance <
					lessIntellegentSplitContainerAdvMainContainer.Width - 700)
					lessIntellegentSplitContainerAdvMainContainer.SplitterDistance =
						defaultDistance;
			}
		}

		public void SaveAllChartSetting()
		{
			IntraDayGridControl.SaveSetting();
			DayGridControl.SaveSetting();
			WeekGridControl.SaveSetting();
			MonthGridControl.SaveSetting();
			FullPeriodGridControl.SaveSetting();
		}

		public void InvalidateSkillResultGrids()
		{
			IntraDayGridControl.Invalidate(true);
			DayGridControl.Invalidate(true);
			WeekGridControl.Invalidate(true);
			MonthGridControl.Invalidate(true);
			FullPeriodGridControl.Invalidate(true);
		}

		public void ReloadChart()
		{
			if (SkillResultViewSetting.Equals(SkillResultViewSetting.Week))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Week, _chartDescription);
				GridChartManager.ReloadChart(WeekGridControl, description);
			}

			if (SkillResultViewSetting.Equals(SkillResultViewSetting.Month))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Month, _chartDescription);
				GridChartManager.ReloadChart(MonthGridControl, description);
			}

			if (SkillResultViewSetting.Equals(SkillResultViewSetting.Period))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Period, _chartDescription);
				GridChartManager.ReloadChart(FullPeriodGridControl, description);
			}

			if (SkillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Intraday, _chartDescription);
				GridChartManager.ReloadChart(IntraDayGridControl, description);
			}
			if (SkillResultViewSetting.Equals(SkillResultViewSetting.Day))
			{
				string description = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Resources.Day, _chartDescription);
				GridChartManager.ReloadChart(DayGridControl, description);
			}
			ChartControlSkillData.Visible = true;
		}

		public void DrawSkillGrid()
		{
			var skillGridControl = resolveControlFromSkillResultViewSetting();
			_chartDescription = string.Empty;
			if (TabSkillData.SelectedIndex >= 0)
			{
				TabPageAdv tab = TabSkillData.TabPages[TabSkillData.SelectedIndex];
				var skill = (ISkill)tab.Tag;
				IAggregateSkill aggregateSkillSkill = skill;
				_chartDescription = skill.Name;

				if (skillGridControl is SkillIntraDayGridControl control)
				{
					_chartDescription = drawIntraDay(control, skill, aggregateSkillSkill);
				}

				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;

				positionControl(skillGridControl);
				selectedSkillGridControl?.DrawDayGrid(_schedulerStateHolder, skill);
				selectedSkillGridControl?.DrawDayGrid(_schedulerStateHolder, skill);
			}
		}

		#region Private

		private TeleoptiGridControl resolveControlFromSkillResultViewSetting()
		{
			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Intraday))
				return IntraDayGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Day))
				return DayGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Week))
				return WeekGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Month))
				return MonthGridControl;

			if (_skillResultViewSetting.Equals(SkillResultViewSetting.Period))
				return FullPeriodGridControl;

			return null;
		}

		private void pinnedToolStripMenuItemClick(object sender, EventArgs e)
		{
			var tab = tabSkillData.SelectedTab;

			if (tab != null)
				_pinnedSkillHelper.PinSlashUnpinTab(tab);
		}

		private void pinSavedSkills(ISchedulingScreenSettings currentSchedulingScreenSettings)
		{
			_pinnedSkillHelper.InitialSetup(tabSkillData, currentSchedulingScreenSettings);

			if (_pinnedSkillHelper.PinnedPage() != null)
				tabSkillData.SelectedTab = _pinnedSkillHelper.PinnedPage();
		}

		private ISkill createSkillSummery(IEnumerable<ISkill> allSkills)
		{
			using (var skillSummery = new SkillSummary(allSkills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					var virtualSkill = (ISkill)skillSummery.AggregateSkillSkill;
					virtualSkill.SetId(Guid.NewGuid());
					TabPageAdv tab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
					tab.ImageIndex = 4;
					tab.Tag = skillSummery.AggregateSkillSkill;
					tabSkillData.TabPages.Add(tab);
					_virtualSkillHelper.SaveVirtualSkill(virtualSkill);
					_pinnedSkillHelper.AddVirtualSkill(virtualSkill);
					_pinnedSkillHelper.SortSkills();

					return virtualSkill;
				}
			}

			return null;
		}

		private bool editSkillSummary(IEnumerable<ISkill> allSkills, ISkill skill, ToolStripMenuItem menuItem)
		{
			var ret = false;
			using (var skillSummery = new SkillSummary(skill, allSkills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					IAggregateSkill newSkill =
						handleSummeryEditMenuItems(_contextMenuSkillGrid, menuItem, skillSummery);

					if (newSkill.AggregateSkills.Count != 0)
					{
						_virtualSkillHelper.EditAndRenameVirtualSkill(newSkill, skill.Name);
						_pinnedSkillHelper.ReplaceOldWithNew((ISkill)newSkill, skill);
						_pinnedSkillHelper.SortSkills();
						ret = true;
					}
					else
					{
						removeVirtualSkill(newSkill);
					}
				}
			}

			return ret;
		}

		private void removeVirtualSkill(IAggregateSkill virtualSkill)
		{
			virtualSkill.ClearAggregateSkill();
			_pinnedSkillHelper.RemoveVirtualSkill((ISkill)virtualSkill);
			foreach (TabPageAdv tabPage in tabSkillData.TabPages)
			{
				if (tabPage.Tag == virtualSkill)
				{
					removeVirtualSkillToolStripMenuItem(_contextMenuSkillGrid, tabPage, virtualSkill, "Delete");
					removeVirtualSkillToolStripMenuItem(_contextMenuSkillGrid, tabPage, virtualSkill, "Edit");
					break;
				}
			}

			_virtualSkillHelper.SaveVirtualSkill(virtualSkill);
		}

		private IAggregateSkill handleSummeryEditMenuItems(ContextMenuStrip contextMenuSkillGrid,
			ToolStripMenuItem menuItem, SkillSummary skillSummary)
		{
			var virtualSkill = (ISkill)skillSummary.AggregateSkillSkill;
			tabSkillData.SelectedTab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
			foreach (TabPageAdv tabPage in tabSkillData.TabPages)
			{
				handleTabsAndMenuItemsVirtualSkill(contextMenuSkillGrid, skillSummary, virtualSkill, tabPage, menuItem);
			}

			return virtualSkill;
		}

		private void handleTabsAndMenuItemsVirtualSkill(ContextMenuStrip contextMenuSkillGrid,
			SkillSummary skillSummary, ISkill virtualSkill, TabPageAdv tabPage,
			ToolStripMenuItem menuItem)
		{
			if (tabPage.Tag == virtualSkill)
			{
				if (skillSummary.AggregateSkillSkill.AggregateSkills.Count == 0)
				{
					removeVirtualSkillToolStripMenuItem(contextMenuSkillGrid, tabPage, virtualSkill, "Edit");
					removeVirtualSkillToolStripMenuItem(contextMenuSkillGrid, tabPage, virtualSkill, "Delete");
					return;
				}

				tabPage.Text = virtualSkill.Name;
				menuItem.Name = virtualSkill.Name;
				menuItem.Text = virtualSkill.Name;
				var skillGridMenuItem = (ToolStripMenuItem)contextMenuSkillGrid.Items["Delete"];
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

		private void removeVirtualSkillToolStripMenuItem(ContextMenuStrip contextMenuSkillGrid, TabPageAdv tabPage,
			IAggregateSkill virtualSkill, string action)
		{
			var skillGridMenuItem = (ToolStripMenuItem)contextMenuSkillGrid.Items[action];
			tabSkillData.TabPages.Remove(tabPage);
			foreach (ToolStripMenuItem subItem in skillGridMenuItem.DropDownItems)
			{
				if (subItem.Tag == virtualSkill)
				{
					skillGridMenuItem.DropDownItems.Remove(subItem);
					if (skillGridMenuItem.DropDownItems.Count == 0)
						contextMenuSkillGrid.Items[action].Enabled = false;
					break;
				}
			}
		}

		private void setupSkillTabs(SchedulingScreenSettings currentSchedulingScreenSettings)
		{
			_currentIntraDayDate = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate;
			TabSkillData.TabPages.Clear();
			TabSkillData.ImageList = imageListSkillTypeIcons;
			foreach (
				ISkill virtualSkill in
				_virtualSkillHelper.LoadVirtualSkills(_schedulerStateHolder.SchedulingResultState.VisibleSkills)
					.OrderBy(s => s.Name))
			{
				TabPageAdv tab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
				tab.Tag = virtualSkill;
				tab.ImageIndex = 4;
				TabSkillData.TabPages.Add(tab);
				enableEditVirtualSkill(virtualSkill);
				enableDeleteVirtualSkill(virtualSkill);
			}

			foreach (ISkill skill in _schedulerStateHolder.SchedulingResultState.VisibleSkills.OrderBy(s => s.Name))
			{
				TabPageAdv tab = ColorHelper.CreateTabPage(skill.Name, skill.Description);
				tab.Tag = skill;
				tab.ImageIndex = GuiHelper.ImageIndexSkillType(skill.SkillType.ForecastSource);

				TabSkillData.TabPages.Add(tab);
			}

			pinSavedSkills(currentSchedulingScreenSettings);
		}

		private void skillGridMenuItemClick(object sender, EventArgs e)
		{
			var virtualSkill = createSkillSummery(_schedulerStateHolder.SchedulingResultState.Skills);
			if (virtualSkill != null)
			{
				enableEditVirtualSkill(virtualSkill);
				enableDeleteVirtualSkill(virtualSkill);
			}
		}

		private void enableDeleteVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem)ContextMenuSkillGrid.Items["Delete"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name) { Tag = virtualSkill };
			subItem.Click += skillGridMenuItemDeleteClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private void enableEditVirtualSkill(ISkill virtualSkill)
		{
			var skillGridMenuItem = (ToolStripMenuItem)_contextMenuSkillGrid.Items["Edit"];
			skillGridMenuItem.Enabled = true;
			var subItem = new ToolStripMenuItem(virtualSkill.Name) { Tag = virtualSkill };
			subItem.Click += skillGridMenuItemEditClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
		}

		private void skillGridMenuItemDeleteClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			var virtualSkill = (IAggregateSkill)menuItem.Tag;
			removeVirtualSkill(virtualSkill);
		}

		private void skillGridMenuItemEditClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			var skill = (ISkill)menuItem.Tag;

			var validData = editSkillSummary(_schedulerStateHolder.SchedulingResultState.Skills, skill, menuItem);
			if (validData)
			{
				DrawSkillGrid();
			}
		}

		private string drawIntraDay(SkillIntraDayGridControl skillIntraDayGridControl, ISkill skill,
			IAggregateSkill aggregateSkillSkill)
		{
			_chartDescription = string.Empty;
			IList<ISkillStaffPeriod> skillStaffPeriods;
			var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_currentIntraDayDate.Date,
				_currentIntraDayDate.AddDays(1).Date, TimeZoneGuardForDesktop_DONOTUSE.Instance_DONTUSE.CurrentTimeZone());
			if (aggregateSkillSkill.IsVirtual)
			{
				_schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					aggregateSkillSkill, periodToFind);
				skillStaffPeriods =
					_schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						aggregateSkillSkill, periodToFind);
			}
			else
			{
				skillStaffPeriods =
					_schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						new List<ISkill> { skill },
						periodToFind);
			}

			if (skillStaffPeriods.Count >= 0)
			{
				_chartDescription = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", skill.Name,
					_currentIntraDayDate.ToShortDateString());
				skillIntraDayGridControl.SetupDataSource(skillStaffPeriods, skill, _schedulerStateHolder);
				skillIntraDayGridControl.SetRowsAndCols();
				positionControl(skillIntraDayGridControl);
			}

			return _chartDescription;
		}

		private void positionControl(Control control)
		{
			//remove control from all tabPages
			foreach (TabPageAdv tabPage in TabSkillData.TabPages)
			{
				tabPage.Controls.Clear();
			}

			TabPageAdv tab = TabSkillData.TabPages[TabSkillData.SelectedIndex];
			tab.Controls.Add(control);

			//position _grid
			control.Dock = DockStyle.Fill;
		}
		#endregion

		#region Virtual

		protected virtual void OnValidationAlertsAgentDoubleClick(ValidationViewAgentDoubleClickEvenArgs e)
		{
			ValidationAlertsAgentDoubleClick?.Invoke(this, e);
		}

		public virtual void OnRestrictionsNotAbleToBeScheduledProgress(System.ComponentModel.ProgressChangedEventArgs e)
		{
			RestrictionsNotAbleToBeScheduledProgress?.Invoke(this, e);
		}

		#endregion

		#region Events

		private void chartControlSkillDataChartRegionMouseHover(object sender, ChartRegionMouseEventArgs e)
		{
			GridChartManager.SetChartToolTip(e.Region, ChartControlSkillData);
		}

		private void chartControlSkillDataChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = Math.Max(1, (int)GridChartManager.GetIntervalValueForChartPoint(ChartControlSkillData, e.Point));
			var skillGridControl = resolveControlFromSkillResultViewSetting();
			skillGridControl.ScrollCellInView(0, column);

			if (skillGridControl is SkillDayGridControl)
				Grid.ScrollCellInView(0, column + 1);
		}

		private void validationAlertsView1AgentDoubleClick(object sender, ValidationViewAgentDoubleClickEvenArgs e)
		{
			OnValidationAlertsAgentDoubleClick(e);
		}

		private void tabInfoPanelsSelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshFilteredPersons(_filteredPersons);
		}

		private void gridResize(object sender, EventArgs e)
		{
			Grid.Invalidate();
		}
		#endregion
	}
}
