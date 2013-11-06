using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IList<IGroupPageLight> _groupPageOnTeams;
        private IList<IGroupPageLight> _groupPageOnCompareWith;
        private IList<IGroupPageLight> _groupPageOnTeamsTeamBlockPer;
        private IList<IActivity> _availableActivity;
        private GroupPageLight _singleAgentEntry;

        public IExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IExtraPreferences extraPreferences,
			ISchedulerGroupPagesProvider groupPagesProvider, IList<IActivity> availableActivity)
        {
            Preferences = extraPreferences;
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
		    _availableActivity = availableActivity;
			_groupPageOnCompareWith = groupPagesProvider.GetGroups(false);
			_groupPageOnTeamsTeamBlockPer  = groupPagesProvider.GetGroups(false);
            //adding the single agent Team
            _singleAgentEntry = new GroupPageLight();
            _singleAgentEntry.Key = "SingleAgentTeam";
            _singleAgentEntry.Name = "Single Agent Team";
            _groupPageOnTeamsTeamBlockPer.Add(_singleAgentEntry);
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
		    initBlockFinderType();
        }

        #region IDataExchange Members

        private void initBlockFinderType()
        {
            comboBoxTeamBlockType.DataSource = BlockFinderTypeCreator.GetBlockFinderTypes;
            comboBoxTeamBlockType.DisplayMember = "Name";
            comboBoxTeamBlockType.ValueMember = "Key";
            if (Preferences.BlockFinderTypeForAdvanceOptimization == BlockFinderType.None)
                comboBoxTeamBlockType.SelectedValue = BlockFinderType.BetweenDayOff.ToString();
            else
                comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization.ToString();
        }

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.DataSourceToControls)
            {
                bindGroupPages();
                bindActivityList();
                setDataToControls();
            }
            else
            {
                getDataFromControls();
            }
        }

        #endregion

        private void bindGroupPages()
        {
            comboBoxGroupPageOnTeams.DataSource = _groupPageOnTeams;
            comboBoxGroupPageOnTeams.DisplayMember = "Name";
            comboBoxGroupPageOnTeams.ValueMember = "Key";

            comboBoxGroupPageOnCompareWith.DataSource = _groupPageOnCompareWith;
            comboBoxGroupPageOnCompareWith.DisplayMember = "Name";
            comboBoxGroupPageOnCompareWith.ValueMember = "Key";
        }

        private void bindActivityList()
        {
            comboBoxActivity .DataSource = _availableActivity ;
            comboBoxActivity.DisplayMember = "Name";
            comboBoxActivity.ValueMember = "Name";
        }
    
        private void getDataFromControls()
        {
			Preferences.UseTeams = checkBoxTeams.Checked;
        	Preferences.KeepSameDaysOffInTeam = checkBoxKeepWeekEndsTogether.Checked;
			Preferences.UseGroupSchedulingCommonCategory = checkBoxCommonCategory.Checked;
			Preferences.UseGroupSchedulingCommonStart = checkBoxCommonStart.Checked;
			Preferences.UseGroupSchedulingCommonEnd = checkBoxCommonEnd.Checked;
            Preferences.UseCommonActivity = checkBoxCommonActivity.Checked;
            Preferences.CommonActivity = (IActivity)comboBoxActivity.SelectedItem;
			Preferences.GroupPageOnTeam = (IGroupPageLight)comboBoxGroupPageOnTeams.SelectedItem;
			Preferences.FairnessValue = (double)trackBar1.Value / 100;
			Preferences.GroupPageOnCompareWith = (IGroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;
            Preferences.UseTeamBlockOption = checkBoxBlock.Checked;
            getTeamBlockPerDataToSave();
        }

        private void setDataToControls()
        {
            checkBoxBlock.Checked = Preferences.UseTeamBlockOption;
			checkBoxTeams.Checked = Preferences.UseTeams;
        	checkBoxKeepWeekEndsTogether.Checked = Preferences.KeepSameDaysOffInTeam;
        	checkBoxCommonCategory.Checked = Preferences.UseGroupSchedulingCommonCategory;
        	checkBoxCommonStart.Checked = Preferences.UseGroupSchedulingCommonStart;
        	checkBoxCommonEnd.Checked = Preferences.UseGroupSchedulingCommonEnd;
            checkBoxCommonActivity.Checked = Preferences.UseCommonActivity;
            comboBoxActivity.Enabled = checkBoxCommonActivity.Checked;
            if (Preferences.CommonActivity != null)
                comboBoxActivity.SelectedValue = Preferences.CommonActivity.Name;
            if (Preferences.GroupPageOnTeam != null)
                comboBoxGroupPageOnTeams.SelectedValue = Preferences.GroupPageOnTeam.Key;
            if (comboBoxGroupPageOnTeams.SelectedValue == null)
                comboBoxGroupPageOnTeams.SelectedIndex = 0;

            trackBar1.Value = (int)(Preferences.FairnessValue * 100);

            if (Preferences.GroupPageOnCompareWith != null)
                comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
            if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
                comboBoxGroupPageOnCompareWith.SelectedIndex = 0;

            checkBoxTeams.Checked = Preferences.UseTeams;
            checkBoxBlock.Checked = Preferences.UseTeamBlockOption;
            setTeamBlockPerData();
        }

        public bool  ValidateDefaultValuesForTeam()
        {
            if (checkBoxTeams.Checked)
            {
				//check if none of the options are not checked. Set the default values
				if (!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked || checkBoxCommonActivity.Checked))
				{
					return false;
				}
            }
	        return true;
        }

		public bool ValidateDefaultValuesForBlock()
		{
			if (checkBoxBlock.Checked)
			{
				if (!(checkBoxTeamBlockSameShift.Checked || checkBoxSameStartTime.Checked || checkBoxTeamBlockSameShiftCategory.Checked))
				{
					return false;
				}
			}
			return true;
		}

        private void checkBoxTeams_CheckedChanged(object sender, System.EventArgs e)
        {
        	setSubItemsOnTeamOptimizationStatus();
        }

        private void setSubItemsOnTeamOptimizationStatus()
        {
            comboBoxGroupPageOnTeams.Enabled = checkBoxTeams.Checked;
			checkBoxKeepWeekEndsTogether.Enabled = checkBoxTeams.Checked;
			checkBoxCommonCategory.Enabled = checkBoxTeams.Checked;
			checkBoxCommonStart.Enabled = checkBoxTeams.Checked;
			checkBoxCommonEnd.Enabled = checkBoxTeams.Checked;
            checkBoxCommonActivity.Enabled = checkBoxTeams.Checked;
        }

      

        private void setInitialControlStatus()
        {
            setSubItemsOnTeamOptimizationStatus();
        }

        private void checkBoxCommonActivity_CheckedChanged(object sender, System.EventArgs e)
        {
            comboBoxActivity.Enabled = checkBoxCommonActivity.Checked;
        }

        private void checkBoxTeamBlockPerBlockScheduling_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxTeamBlockSameShiftCategory.Enabled = checkBoxBlock.Checked;
            checkBoxSameStartTime.Enabled = checkBoxBlock.Checked;
            comboBoxTeamBlockType.Enabled = checkBoxBlock.Checked;
	        checkBoxTeamBlockSameShift.Enabled = checkBoxBlock.Checked;
        }

        private void setTeamBlockPerData()
        {
            checkBoxTeamBlockSameShiftCategory.Checked = Preferences.UseTeamBlockSameShiftCategory;
            checkBoxSameStartTime.Checked = Preferences.UseTeamBlockSameStartTime;
	        checkBoxTeamBlockSameShift.Checked = Preferences.UseTeamBlockSameShift;
            comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization.ToString();
        }
        private void getTeamBlockPerDataToSave()
        {
            if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.BetweenDayOff.ToString( ) )
                Preferences.BlockFinderTypeForAdvanceOptimization  =  BlockFinderType.BetweenDayOff ;
            else if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.SchedulePeriod.ToString( ) )
                Preferences.BlockFinderTypeForAdvanceOptimization = BlockFinderType.SchedulePeriod;
			if (!checkBoxTeams.Checked)
				Preferences.GroupPageOnTeamBlockPer = _singleAgentEntry;
			else
				Preferences.GroupPageOnTeamBlockPer = (IGroupPageLight)comboBoxGroupPageOnTeams.SelectedItem;
	        Preferences.UseTeamBlockSameEndTime = false;
	        Preferences.UseTeamBlockSameShift = checkBoxTeamBlockSameShift.Checked;
            Preferences.UseTeamBlockSameShiftCategory = checkBoxTeamBlockSameShiftCategory.Checked;
            Preferences.UseTeamBlockSameStartTime = checkBoxSameStartTime.Checked;
        }
    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
