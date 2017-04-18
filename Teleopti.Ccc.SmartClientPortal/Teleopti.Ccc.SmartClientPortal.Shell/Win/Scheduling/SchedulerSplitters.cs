using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public partial class SchedulerSplitters : BaseUserControl
    {
        private bool _useAvailability = true;
        private bool _useStudentAvailability;
        private bool _useRotation;
        private bool _usePreference = true;
        private bool _useSchedules = true;
        private readonly PinnedSkillHelper _pinnedSkillHelper;
		IEnumerable<IPerson> _filteredPersons = new List<IPerson>();


		public SchedulerSplitters()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            chbStudenAvailability.Checked = false;
            chbAvailability.Checked = true;
            chbRotations.Checked = false;
            chbPreferences.Checked = true;
            chbSchedules.Checked = true;
            _pinnedSkillHelper = new PinnedSkillHelper();
				tabSkillData.TabStyle = typeof(SkillTabRenderer);
				tabSkillData.TabPanelBackColor = Color.FromArgb(199, 216, 237);
			validationAlertsView1.AgentDoubleClick += ValidationAlertsView1AgentDoubleClick;
        }

		public event EventHandler<ValidationViewAgentDoubleClickEvenArgs> ValidationAlertsAgentDoubleClick;

		private void ValidationAlertsView1AgentDoubleClick(object sender, ValidationViewAgentDoubleClickEvenArgs e)
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

		public TabPageAdv PinnedPage
		{
			get { return _pinnedSkillHelper.PinnedPage(); }
		}

    	public AgentRestrictionGrid AgentRestrictionGrid
    	{
			get { return agentRestrictionGrid1; }
    	}

		public RestrictionSchedulingOptions SchedulingOptions
		{
			get
			{
				var options = new RestrictionSchedulingOptions
				{
					UseAvailability = _useAvailability,
					UsePreferences = _usePreference,
					UseStudentAvailability = _useStudentAvailability,
					UseRotations = _useRotation,
					UseScheduling = _useSchedules
				};
				return options;
			}
		}

        private void chbAvailability_CheckedChanged(object sender, EventArgs e)
        {
            _useAvailability = chbAvailability.Checked;
            RecalculateRestrictions();
        }

        private void chbRotations_CheckedChanged(object sender, EventArgs e)
        {
            _useRotation = chbRotations.Checked;
            RecalculateRestrictions();
        }

        private void chbPreferences_CheckedChanged(object sender, EventArgs e)
        {
            _usePreference = chbPreferences.Checked;
            RecalculateRestrictions();
        }

        private void chbStudenAvailability_CheckedChanged(object sender, EventArgs e)
        {
            _useStudentAvailability = chbStudenAvailability.Checked;
            RecalculateRestrictions();
        }

        private void chbSchedules_CheckedChanged(object sender, EventArgs e)
        {
            _useSchedules = chbSchedules.Checked;
            RecalculateRestrictions();
        }

        public void RecalculateRestrictions()
        {
			agentRestrictionGrid1.LoadData(SchedulingOptions);
        }

        private void PinnedToolStripMenuItemClick(object sender, EventArgs e)
        {
            var tab = tabSkillData.SelectedTab;
            
            if (tab!=null)
                _pinnedSkillHelper.PinSlashUnpinTab(tab);
        }

        public void PinSavedSkills(ISchedulingScreenSettings currentSchedulingScreenSettings)
        {
            _pinnedSkillHelper.InitialSetup(tabSkillData, currentSchedulingScreenSettings);
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
	        if (!value)
	        {
		        if (lessIntellegentSplitContainerAdvMainContainer.SplitterDistance >
		            lessIntellegentSplitContainerAdvMainContainer.Width - 300)
			        lessIntellegentSplitContainerAdvMainContainer.SplitterDistance =
				        lessIntellegentSplitContainerAdvMainContainer.Width - 300;
	        }
        }

	    public void InsertAgentInfoControl(AgentInfoControl agentInfoControl, ISchedulerStateHolder schedulerState,
		    IEffectiveRestrictionCreator effectiveRestrictionCreator, int maxCalculatedMinMaxCacheEntries)
	    {
		    var schedulingOptions = new SchedulingOptions
		    {
			    UseAvailability = true,
			    UsePreferences = true,
			    UseRotations = true,
			    UseStudentAvailability = true
		    };
		    var calculateMinMaxCacheDecider = new CalculateMinMaxCacheDecider();
		    agentInfoControl.MbCacheDisabled = calculateMinMaxCacheDecider.ShouldCacheBeDisabled(schedulerState, schedulingOptions,
			    effectiveRestrictionCreator, maxCalculatedMinMaxCacheEntries);
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

		private void tabInfoPanelsSelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshTabInfoPanels(_filteredPersons);
		}
	}
}
