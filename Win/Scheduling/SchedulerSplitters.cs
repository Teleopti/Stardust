﻿using System;
using System.Drawing;
using System.Windows.Forms.Integration;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WpfControls.Controls.Requests.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class SchedulerSplitters : BaseUserControl
    {
        private bool _useAvailability = true;
        private bool _useStudentAvailability;
        private bool _useRotation;
        private bool _usePreference = true;
        private bool _useSchedules = true;
        private readonly PinnedSkillHelper _pinnedSkillHelper;

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
        }

        public WpfControls.Common.Interop.MultipleHostControl MultipleHostControl3
        {
            get { return multipleHostControl1; }
        }
        public TeleoptiLessIntelligentSplitContainer SplitContainerAdvMain
        {
            get { return lessIntellegentSplitContainerAdvMain; }
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

        private void chbAvailability_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _useAvailability = chbAvailability.Checked;
            RecalculateRestrictions();
        }

        private void chbRotations_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _useRotation = chbRotations.Checked;
            RecalculateRestrictions();
        }

        private void chbPreferences_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _usePreference = chbPreferences.Checked;
            RecalculateRestrictions();
        }

        private void chbStudenAvailability_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _useStudentAvailability = chbStudenAvailability.Checked;
            RecalculateRestrictions();
        }

        private void chbSchedules_CheckedChanged(object sender, CheckedChangedEventArgs e)
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
    }
}
