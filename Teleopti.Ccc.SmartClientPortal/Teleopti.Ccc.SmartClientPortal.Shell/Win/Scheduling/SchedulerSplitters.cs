using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public partial class SchedulerSplitters : BaseUserControl
    {
        private readonly PinnedSkillHelper _pinnedSkillHelper;
		IEnumerable<IPerson> _filteredPersons = new List<IPerson>();

		public SchedulerSplitters()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            _pinnedSkillHelper = new PinnedSkillHelper();
				tabSkillData.TabStyle = typeof(SkillTabRenderer);
				tabSkillData.TabPanelBackColor = Color.FromArgb(199, 216, 237);
			validationAlertsView1.AgentDoubleClick += validationAlertsView1AgentDoubleClick;
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
        public TeleoptiLessIntelligentSplitContainer SplitContainerAdvMain
        {
            get { return lessIntellegentSplitContainerAdvMain; }
        }
		
		public TeleoptiLessIntelligentSplitContainer SplitContainerAdvMainContainer
        {
            get { return lessIntellegentSplitContainerAdvMainContainer; }
        }
        public TeleoptiLessIntelligentSplitContainer SplitContainerAdvResultGraph
        {
            get { return lessIntellegentSplitContainerAdvResultGraph; }
        }
        public TeleoptiLessIntelligentSplitContainer SplitContainerLessIntelligent1
        {
            get { return teleoptiLessIntelligentSplitContainerLessIntelligent1; }
        }
        public TeleoptiLessIntelligentSplitContainer SplitContainerView
        {
            get { return teleoptiLessIntellegentSplitContainerView; }
        }
        public ChartControl ChartControlSkillData
        {
            get { return chartControlSkillData; }
        }
        public TabControlAdv TabSkillData
        {
            get { return tabSkillData; }
        }
		
		public TabControlAdv TabInfoPanels
        {
            get { return tabInfoPanels; }
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

		private TabPageAdv PinnedPage
		{
			get { return _pinnedSkillHelper.PinnedPage(); }
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

			if (PinnedPage != null)
				TabSkillData.SelectedTab = PinnedPage;
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
					TabSkillData.TabPages.Add(tab);
					AddVirtualSkill(virtualSkill);
					SortSkills();

					return virtualSkill;
				}
			}

			return null;
		}

		public void SortSkills()
        {
            _pinnedSkillHelper.SortSkills();
        }

        public void AddVirtualSkill(ISkill virtualSkill)
        {
            _pinnedSkillHelper.AddVirtualSkill(virtualSkill);
        }

        public void RemoveVirtualSkill(ISkill virtualSkill)
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

	    public void InsertAgentInfoControl(AgentInfoControl agentInfoControl)
	    {
		    tabInfoPanels.TabPages[0].Controls.Add(agentInfoControl);
		    agentInfoControl.Dock = DockStyle.Fill;
		    tabInfoPanels.Refresh();
	    }

	    public void InsertShiftCategoryDistributionModel(IShiftCategoryDistributionModel model)
		{
			var shiftCategoryDistributionControl = (ShiftCategoryDistributionControl)tabInfoPanels.TabPages[1].Controls[0];
			shiftCategoryDistributionControl.SetModel(model);
		}

		public void InsertValidationAlertsModel(ValidationAlertsModel model)
		{
			var validationAlertsControl = (ValidationAlertsView)tabInfoPanels.TabPages[2].Controls[0];
			validationAlertsControl.SetModel(model);
		}

		public void ReselectSelectedAgentNotPossibleToSchedule()
		{
			agentsNotPossibleToSchedule1.ReselectSelected();
		}
		public void SetSelectedAgentsOnAgentsNotPossibleToSchedule(IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedDates, AgentRestrictionsDetailView detailView)
		{
			agentsNotPossibleToSchedule1.SetSelected(selectedPersons, selectedDates, detailView);
		}

		public void InsertRestrictionNotAbleToBeScheduledReportModel(RestrictionNotAbleToBeScheduledReport reportModel)
		{
			agentsNotPossibleToSchedule1.InitAgentsNotPossibleToSchedule(reportModel, this);
		}

		public void DisableViewShiftCategoryDistribution()
		{
			var shiftCategoryDistributionControl = (ShiftCategoryDistributionControl)tabInfoPanels.TabPages[1].Controls[0];
			shiftCategoryDistributionControl.DisableViewShiftCategoryDistribution();
		}

		public void EnableViewShiftCategoryDistribution()
		{
			var shiftCategoryDistributionControl = (ShiftCategoryDistributionControl)tabInfoPanels.TabPages[1].Controls[0];
			shiftCategoryDistributionControl.EnableViewShiftCategoryDistribution();
		}

		public void RefreshTabInfoPanels(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons;
			if(tabInfoPanels.SelectedIndex == 2)
			{
				var validationAlertsControl = (ValidationAlertsView)tabInfoPanels.TabPages[2].Controls[0];
				validationAlertsControl.ReDraw(_filteredPersons);
			}
			tabInfoPanels.Refresh();
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
			RefreshTabInfoPanels(_filteredPersons);
		}

		private void gridResize(object sender, EventArgs e)
		{
			Grid.Invalidate();
		}
	}
}
