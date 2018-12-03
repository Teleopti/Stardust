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
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public partial class SchedulerSplitters : BaseUserControl
    {
        private readonly PinnedSkillHelper _pinnedSkillHelper;
		private IList<IPerson> _filteredPersons = new List<IPerson>();
		private IVirtualSkillHelper _virtualSkillHelper;
		private readonly ContextMenuStrip _contextMenuSkillGrid;
		private SplitterManagerRestrictionView _splitterManager;
		private ISchedulerStateHolder _schedulerStateHolder;
		private TeleoptiGridControl _skillGridControl;
		private DateOnly _currentIntradayDate;

		public SchedulerSplitters()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
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
		}

		public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> RestrictionsNotAbleToBeScheduledProgress;
		public event EventHandler<ValidationViewAgentDoubleClickEvenArgs> ValidationAlertsAgentDoubleClick;

		private void validationAlertsView1AgentDoubleClick(object sender, ValidationViewAgentDoubleClickEvenArgs e)
		{
			OnValidationAlertsAgentDoubleClick(e);
		}

		public MultipleHostControl MultipleHostControl3
        {
            get { return multipleHostControl1; }
        }

		public AgentInfoControl AgentInfoControl
		{
			get { return tabInfoPanels.TabPages[0].Controls[0] as AgentInfoControl;}
		}

		public TeleoptiLessIntelligentSplitContainer SplitContainerAdvMainContainer
        {
            get { return lessIntellegentSplitContainerAdvMainContainer; }
        }

        public TeleoptiLessIntelligentSplitContainer SplitContainerView
        {
            get { return teleoptiLessIntellegentSplitContainerView; }
        }
        public ChartControl ChartControlSkillData
        {
            get { return chartControlSkillData; }
        }

		public ContextMenuStrip ContextMenuSkillGrid
		{
			get { return _contextMenuSkillGrid; }
		}

		public TabControlAdv TabSkillData
		{
			get { return tabSkillData; }
		}

		public IVirtualSkillHelper VirtualSkillHelper
		{
			get { return _virtualSkillHelper; }
		}

        public ElementHost ElementHostRequests
        {
            get { return elementHostRequests; }
        }
        public HandlePersonRequestView HandlePersonRequestView1
        {
            get { return handlePersonRequestView1; }
        }
        public ElementHost ElementHost1
        {
            get { return elementHost1; }
        }
        public GridControl Grid
        {
            get { return grid; }
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

		private void pinnedToolStripMenuItemClick(object sender, EventArgs e)
        {
            var tab = tabSkillData.SelectedTab;
            
            if (tab!=null)
                _pinnedSkillHelper.PinSlashUnpinTab(tab);
        }

        public void PinSavedSkills(ISchedulingScreenSettings currentSchedulingScreenSettings)
        {
            _pinnedSkillHelper.InitialSetup(tabSkillData, currentSchedulingScreenSettings);

			if (_pinnedSkillHelper.PinnedPage() != null)
				tabSkillData.SelectedTab = _pinnedSkillHelper.PinnedPage();
		}

		public ISkill CreateSkillSummery(IList<ISkill> allSkills)
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
					AddVirtualSkill(virtualSkill);
					SortSkills();

					return virtualSkill;
				}
			}

			return null;
		}

		private bool editSkillSummary(IList<ISkill> allSkills, ISkill skill, ToolStripMenuItem menuItem)
		{
			var ret = false;
			using (var skillSummery = new SkillSummary(skill, allSkills))
			{
				skillSummery.ShowDialog();

				if (skillSummery.DialogResult == DialogResult.OK)
				{
					IAggregateSkill newSkill = handleSummeryEditMenuItems(_contextMenuSkillGrid, menuItem, skillSummery);

					if (newSkill.AggregateSkills.Count != 0)
					{
						VirtualSkillHelper.EditAndRenameVirtualSkill(newSkill, skill.Name);
						ReplaceOldWithNew((ISkill)newSkill, skill);
						SortSkills();
						ret = true;
					}
					else
					{
						RemoveVirtualSkill(newSkill);
					}
				}
			}

			return ret;
		}

		public void RemoveVirtualSkill(IAggregateSkill virtualSkill)
		{
			virtualSkill.ClearAggregateSkill();
			removeVirtualSkill((Skill)virtualSkill);
			foreach (TabPageAdv tabPage in tabSkillData.TabPages)
			{
				if (tabPage.Tag == virtualSkill)
				{
					removeVirtualSkillToolStripMenuItem(_contextMenuSkillGrid, tabPage, virtualSkill, "Delete");
					removeVirtualSkillToolStripMenuItem(_contextMenuSkillGrid, tabPage, virtualSkill, "Edit");
					break;
				}
			}
			VirtualSkillHelper.SaveVirtualSkill(virtualSkill);
		}

		private IAggregateSkill handleSummeryEditMenuItems(ContextMenuStrip contextMenuSkillGrid, ToolStripMenuItem menuItem, SkillSummary skillSummary)
		{
			var virtualSkill = (ISkill)skillSummary.AggregateSkillSkill;
			tabSkillData.SelectedTab = ColorHelper.CreateTabPage(virtualSkill.Name, virtualSkill.Description);
			foreach (TabPageAdv tabPage in tabSkillData.TabPages)
			{
				handleTabsAndMenuItemsVirtualSkill(contextMenuSkillGrid, skillSummary, virtualSkill, tabPage, menuItem);
			}
			return virtualSkill;
		}

		private void handleTabsAndMenuItemsVirtualSkill(ContextMenuStrip contextMenuSkillGrid, SkillSummary skillSummary, ISkill virtualSkill, TabPageAdv tabPage,
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

		private void removeVirtualSkillToolStripMenuItem(ContextMenuStrip contextMenuSkillGrid, TabPageAdv tabPage, IAggregateSkill virtualSkill, string action)
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

		public void SortSkills()
        {
            _pinnedSkillHelper.SortSkills();
        }

        public void AddVirtualSkill(ISkill virtualSkill)
        {
            _pinnedSkillHelper.AddVirtualSkill(virtualSkill);
        }

        private void removeVirtualSkill(ISkill virtualSkill)
        {
            _pinnedSkillHelper.RemoveVirtualSkill(virtualSkill);
        }

        public void ReplaceOldWithNew(ISkill newSkill, ISkill oldSkill)
        {
            _pinnedSkillHelper.ReplaceOldWithNew(newSkill, oldSkill);
        }

        public void ToggelPropertyPanel(bool value)
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

		public void Initialize(ILifetimeScope container, ISchedulerStateHolder schedulerStateHolder, SchedulerGroupPagesProvider schedulerGroupPagesProvider, IEnumerable<IOptionalColumn> optionalColumns)
		{
			Grid.VScrollPixel = false;
			Grid.HScrollPixel = false;
			_virtualSkillHelper = container.Resolve<IVirtualSkillHelper>();
			_schedulerStateHolder = schedulerStateHolder;

			var requestedPeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			var outerPeriod = new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-7), requestedPeriod.EndDate.AddDays(7));
			var agentInfoControl = new AgentInfoControl(schedulerGroupPagesProvider, container, outerPeriod,
				requestedPeriod, schedulerStateHolder, optionalColumns);

			tabInfoPanels.TabPages[0].Controls.Add(agentInfoControl);
			agentInfoControl.Dock = DockStyle.Fill;
			tabInfoPanels.Refresh();

			//container can fix this to one row
			ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson =
				new CachedNumberOfEachCategoryPerPerson(schedulerStateHolder.Schedules, schedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			ICachedNumberOfEachCategoryPerDate cachedNumberOfEachCategoryPerDate =
				new CachedNumberOfEachCategoryPerDate(schedulerStateHolder.Schedules, schedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
			var allowedSc = new List<IShiftCategory>();
			foreach (var shiftCategory in schedulerStateHolder.CommonStateHolder.ShiftCategories)
			{
				var sc = shiftCategory as IDeleteTag;
				if (sc != null && !sc.IsDeleted)
					allowedSc.Add(shiftCategory);
			}
			ICachedShiftCategoryDistribution cachedShiftCategoryDistribution =
				new CachedShiftCategoryDistribution(schedulerStateHolder.Schedules, schedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
					cachedNumberOfEachCategoryPerPerson,
					allowedSc);
			var shiftCategoryDistributionModel = new ShiftCategoryDistributionModel(cachedShiftCategoryDistribution,
				cachedNumberOfEachCategoryPerDate,
				cachedNumberOfEachCategoryPerPerson,
				schedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
				schedulerStateHolder);
			shiftCategoryDistributionModel.SetFilteredPersons(schedulerStateHolder.FilteredCombinedAgentsDictionary.Values);
			shiftCategoryDistributionControl1.SetModel(shiftCategoryDistributionModel);
			agentsNotPossibleToSchedule1.InitAgentsNotPossibleToSchedule(container.Resolve<RestrictionNotAbleToBeScheduledReport>(), this);
			validationAlertsView1.SetModel(new ValidationAlertsModel(
				schedulerStateHolder.Schedules, NameOrderOption.LastNameFirstName,
				schedulerStateHolder.RequestedPeriod.DateOnlyPeriod));

			_contextMenuSkillGrid.Items["CreateSkillSummery"].Click += skillGridMenuItemClick;
		}

		public void RefreshSummarySkillIfActive(TeleoptiGridControl skillGridControl, DateOnly currentIntraDayDate)
		{
			if (TabSkillData.SelectedIndex < 0) return;
			var tab = TabSkillData.TabPages[TabSkillData.SelectedIndex];
			var skill = (ISkill)tab.Tag;
			IAggregateSkill aggregateSkillSkill = skill;
			if (!aggregateSkillSkill.IsVirtual)
				return;

			if (skillGridControl is SkillIntradayGridControl)
			{
				var skillStaffPeriods = _schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					aggregateSkillSkill,
					TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentIntraDayDate.Date,
						currentIntraDayDate.AddDays(1).Date, _schedulerStateHolder.TimeZoneInfo));
				((SkillIntradayGridControl) skillGridControl).Presenter.RowManager?.SetDataSource(skillStaffPeriods);
			}
			else
			{
				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return;

				selectedSkillGridControl.SetDataSource(_schedulerStateHolder, skill);
			}

			skillGridControl.Refresh();
		}

		public void SetupSkillTabs(SchedulingScreenSettings currentSchedulingScreenSettings)
		{
			TabSkillData.TabPages.Clear();
			TabSkillData.ImageList = imageListSkillTypeIcons;
			foreach (
				ISkill virtualSkill in
				VirtualSkillHelper.LoadVirtualSkills(_schedulerStateHolder.SchedulingResultState.VisibleSkills).OrderBy(s => s.Name))
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
			PinSavedSkills(currentSchedulingScreenSettings);
		}

		private void skillGridMenuItemClick(object sender, EventArgs e)
		{
			var virtualSkill = CreateSkillSummery(_schedulerStateHolder.SchedulingResultState.Skills);
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
			var subItem = new ToolStripMenuItem(virtualSkill.Name);
			subItem.Tag = virtualSkill;
			subItem.Click += skillGridMenuItemDeleteClick;
			skillGridMenuItem.DropDownItems.Add(subItem);
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

		private void skillGridMenuItemDeleteClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			var virtualSkill = (IAggregateSkill)menuItem.Tag;
			RemoveVirtualSkill(virtualSkill);
		}

		private void skillGridMenuItemEditClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			var skill = (ISkill)menuItem.Tag;

			var validData = editSkillSummary(_schedulerStateHolder.SchedulingResultState.Skills, skill, menuItem);
			if (validData)
			{
				DrawSkillGrid(_skillGridControl, _currentIntradayDate);
			}
		}

		public string DrawSkillGrid(TeleoptiGridControl skillGridControl, DateOnly currentIntradayDate)
		{
			_skillGridControl = skillGridControl;
			_currentIntradayDate = currentIntradayDate;
			var chartDescription = string.Empty;
			if (TabSkillData.SelectedIndex >= 0)
			{
				TabPageAdv tab = TabSkillData.TabPages[TabSkillData.SelectedIndex];
				var skill = (ISkill)tab.Tag;
				IAggregateSkill aggregateSkillSkill = skill;
				chartDescription = skill.Name;

				if (skillGridControl is SkillIntradayGridControl control)
				{
					chartDescription = drawIntraday(control, skill, aggregateSkillSkill, currentIntradayDate, chartDescription);
					return chartDescription;
				}

				var selectedSkillGridControl = skillGridControl as SkillResultGridControlBase;
				if (selectedSkillGridControl == null)
					return chartDescription;

				positionControl(skillGridControl);
				selectedSkillGridControl.DrawDayGrid(_schedulerStateHolder, skill);
				selectedSkillGridControl.DrawDayGrid(_schedulerStateHolder, skill);
			}

			return chartDescription;
		}

		private string drawIntraday(SkillIntradayGridControl skillIntradayGridControl, ISkill skill,
			IAggregateSkill aggregateSkillSkill, DateOnly currentIntradayDate, string chartDescription)
		{
			IList<ISkillStaffPeriod> skillStaffPeriods;
			var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentIntradayDate.Date,
				currentIntradayDate.AddDays(1).Date, _schedulerStateHolder.TimeZoneInfo);
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
						new List<ISkill> {skill},
						periodToFind);
			}

			if (skillStaffPeriods.Count >= 0)
			{
				chartDescription = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", skill.Name,
					currentIntradayDate.ToShortDateString());
				skillIntradayGridControl.SetupDataSource(skillStaffPeriods, skill, _schedulerStateHolder);
				skillIntradayGridControl.SetRowsAndCols();
				positionControl(skillIntradayGridControl);
			}

			return chartDescription;
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

		public void ReselectSelectedAgentNotPossibleToSchedule()
		{
			agentsNotPossibleToSchedule1.ReselectSelected();
		}

		public void SetSelectedAgentsOnAgentsNotPossibleToSchedule(IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedDates, AgentRestrictionsDetailView detailView)
		{
			agentsNotPossibleToSchedule1.SetSelected(selectedPersons, selectedDates, detailView);
		}

		public void DisableViewShiftCategoryDistribution()
		{
			shiftCategoryDistributionControl1.DisableViewShiftCategoryDistribution();
		}

		public void EnableViewShiftCategoryDistribution()
		{
			shiftCategoryDistributionControl1.EnableViewShiftCategoryDistribution();
		}

		public void RefreshFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons.ToList();
			if(tabInfoPanels.SelectedIndex == 2)
			{
				validationAlertsView1.ReDraw(_filteredPersons);
			}
			tabInfoPanels.Refresh();

			shiftCategoryDistributionControl1.Model.SetFilteredPersons(_filteredPersons);
		}

	    protected virtual void OnValidationAlertsAgentDoubleClick(ValidationViewAgentDoubleClickEvenArgs e)
	    {
		    ValidationAlertsAgentDoubleClick?.Invoke(this, e);
	    }

		public virtual void OnRestrictionsNotAbleToBeScheduledProgress(System.ComponentModel.ProgressChangedEventArgs e)
		{
			RestrictionsNotAbleToBeScheduledProgress?.Invoke(this, e);
		}

		private void tabInfoPanelsSelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshFilteredPersons(_filteredPersons);
		}

		private void gridResize(object sender, EventArgs e)
		{
			Grid.Invalidate();
		}

	}
}
