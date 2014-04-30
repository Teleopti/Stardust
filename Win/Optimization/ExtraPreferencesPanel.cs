﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
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
        private IEnumerable<IActivity> _availableActivity;
        private GroupPageLight _singleAgentEntry;
		private IGroupPageLight _noTeamsGroupPage;

		public IExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IExtraPreferences extraPreferences,
			ISchedulerGroupPagesProvider groupPagesProvider, IEnumerable<IActivity> availableActivity)
        {
            Preferences = extraPreferences;
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
        	
		    _availableActivity = availableActivity;
			_groupPageOnCompareWith = groupPagesProvider.GetGroups(false);
			_groupPageOnTeamsTeamBlockPer  = groupPagesProvider.GetGroups(false);
			_noTeamsGroupPage = new GroupPageLight { Key = "NoTeam", Name = Resources.NoTeam };
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
                comboBoxTeamBlockType.SelectedValue = BlockFinderType.SingleDay.ToString();
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
			  var tempGroupPages = _groupPageOnTeams;
			  tempGroupPages.Add(_noTeamsGroupPage);

			  comboBoxGroupPageOnTeams.DataSource = tempGroupPages;
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
	        
        	Preferences.KeepSameDaysOffInTeam = checkBoxKeepWeekEndsTogether.Checked;
			Preferences.UseGroupSchedulingCommonCategory = checkBoxCommonCategory.Checked;
			Preferences.UseGroupSchedulingCommonStart = checkBoxCommonStart.Checked;
			Preferences.UseGroupSchedulingCommonEnd = checkBoxCommonEnd.Checked;
            Preferences.UseCommonActivity = checkBoxCommonActivity.Checked;
            Preferences.CommonActivity = (IActivity)comboBoxActivity.SelectedItem;
			Preferences.FairnessValue = (double)trackBar1.Value / 100;
			Preferences.GroupPageOnCompareWith = (IGroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;
            getTeamBlockPerDataToSave();
        }

        private void setDataToControls()
        {
            
        	checkBoxKeepWeekEndsTogether.Checked = Preferences.KeepSameDaysOffInTeam;
        	checkBoxCommonCategory.Checked = Preferences.UseGroupSchedulingCommonCategory;
        	checkBoxCommonStart.Checked = Preferences.UseGroupSchedulingCommonStart;
        	checkBoxCommonEnd.Checked = Preferences.UseGroupSchedulingCommonEnd;
            checkBoxCommonActivity.Checked = Preferences.UseCommonActivity;
            comboBoxActivity.Enabled = checkBoxCommonActivity.Checked;
            if (Preferences.CommonActivity != null)
                comboBoxActivity.SelectedValue = Preferences.CommonActivity.Name;
	        if (Preferences.UseTeams)
	        {
				  if (Preferences.GroupPageOnTeam != null)
					  comboBoxGroupPageOnTeams.SelectedValue = Preferences.GroupPageOnTeam.Key;
				  if (comboBoxGroupPageOnTeams.SelectedValue == null)
					  comboBoxGroupPageOnTeams.SelectedIndex = 0;
	        }
	        else
	        {
				  comboBoxGroupPageOnTeams.SelectedValue = _noTeamsGroupPage.Key;
	        }
			  

            trackBar1.Value = (int)(Preferences.FairnessValue * 100);

            if (Preferences.GroupPageOnCompareWith != null)
                comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
            if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
                comboBoxGroupPageOnCompareWith.SelectedIndex = 0;
			  setTeamBlockPerData();
        }

		public bool ValidateTeamBlockCombination()
		{
			if (!isTeamEnabled() || !isBlockEnabled())
				return true;

			if ((string)comboBoxTeamBlockType.SelectedValue != BlockFinderType.BetweenDayOff.ToString())
				return true;

			if (checkBoxKeepWeekEndsTogether.Checked)
				return true;

			return false;
		}

        public bool  ValidateDefaultValuesForTeam()
        {
			  if (isTeamEnabled())
	        {
				  //check if none of the options are not checked. Set the default values
				  if (!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked || checkBoxCommonActivity.Checked))
					  return false;
	        }
	        return true;
        }

		public bool ValidateDefaultValuesForBlock()
		{
			if (isBlockEnabled())
			{
				if (!(checkBoxTeamBlockSameShift.Checked || checkBoxSameStartTime.Checked || checkBoxTeamBlockSameShiftCategory.Checked))
				{
					return false;
				}
			}
			return true;
		}


        private void setSubItemsOnTeamOptimizationStatus()
        {
			  var isEnabled = isTeamEnabled();
			  checkBoxKeepWeekEndsTogether.Enabled = isEnabled;
			  checkBoxCommonCategory.Enabled = isEnabled;
			  checkBoxCommonStart.Enabled = isEnabled;
			  checkBoxCommonEnd.Enabled = isEnabled;
			  checkBoxCommonActivity.Enabled = isEnabled;
        }

      

        private void setInitialControlStatus()
        {
            setSubItemsOnTeamOptimizationStatus();
        }

        private void checkBoxCommonActivity_CheckedChanged(object sender, System.EventArgs e)
        {
            comboBoxActivity.Enabled = checkBoxCommonActivity.Checked;
        }


        private void setTeamBlockPerData()
        {
	        
            checkBoxTeamBlockSameShiftCategory.Checked = Preferences.UseTeamBlockSameShiftCategory;
            checkBoxSameStartTime.Checked = Preferences.UseTeamBlockSameStartTime;
	        checkBoxTeamBlockSameShift.Checked = Preferences.UseTeamBlockSameShift;
	        if (Preferences.BlockFinderTypeForAdvanceOptimization == BlockFinderType.None)
		        comboBoxTeamBlockType.SelectedValue = BlockFinderType.SingleDay.ToString();
			  else
				  comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization.ToString();
        }
        private void getTeamBlockPerDataToSave()
        {
			  if (!isBlockEnabled())
		        Preferences.UseTeamBlockOption  = false;
	        else
				  Preferences.UseTeamBlockOption = true;
			  if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.BetweenDayOff.ToString( ) )
				  Preferences.BlockFinderTypeForAdvanceOptimization  =  BlockFinderType.BetweenDayOff ;
				else if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.SchedulePeriod.ToString( ) )
				  Preferences.BlockFinderTypeForAdvanceOptimization = BlockFinderType.SchedulePeriod;
				else
				  Preferences.BlockFinderTypeForAdvanceOptimization = BlockFinderType.None;

	        if (!isTeamEnabled())
	        {
				  Preferences.GroupPageOnTeam = _singleAgentEntry;
		        Preferences.UseTeams = false;
	        }
	        else
	        {
				  Preferences.GroupPageOnTeam = (IGroupPageLight)comboBoxGroupPageOnTeams.SelectedItem;
		        Preferences.UseTeams = true;
	        }
				  
			  Preferences.UseTeamBlockSameEndTime = false;
	        Preferences.UseTeamBlockSameShift = checkBoxTeamBlockSameShift.Checked;
           Preferences.UseTeamBlockSameShiftCategory = checkBoxTeamBlockSameShiftCategory.Checked;
           Preferences.UseTeamBlockSameStartTime = checkBoxSameStartTime.Checked;
        }

		  private void comboBoxTeamBlockType_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = isBlockEnabled();

			  if (isEnabled)
			  {
				  checkBoxTeamBlockSameShiftCategory.Checked = true;
			  }
			  else
			  {
				  checkBoxTeamBlockSameShiftCategory.Checked = false;
				  checkBoxSameStartTime.Checked = false;
				  checkBoxTeamBlockSameShift.Checked = false;
			  }

			  checkBoxTeamBlockSameShiftCategory.Enabled = isEnabled;
			  checkBoxSameStartTime.Enabled = isEnabled;
			  checkBoxTeamBlockSameShift.Enabled = isEnabled;
		  }

		  private void comboBoxGroupPageOnTeams_SelectedValueChanged(object sender, EventArgs e)
		  {
			  setSubItemsOnTeamOptimizationStatus();
			  var isEnabled = isTeamEnabled();
			  if (isEnabled)
			  {
				  checkBoxCommonCategory.Checked = true;
			  }
			  else
			  {
				  checkBoxCommonCategory.Checked = false;
				  checkBoxCommonStart.Checked = false;
				  checkBoxCommonEnd.Checked = false;
				  checkBoxCommonActivity.Checked = false;
				  checkBoxKeepWeekEndsTogether.Checked = false;
			  }
		  }

		private bool isTeamEnabled()
		{
			return (string) comboBoxGroupPageOnTeams.SelectedValue != _noTeamsGroupPage.Key;
		}

		private bool isBlockEnabled()
		{
			return (string) comboBoxTeamBlockType.SelectedValue != BlockFinderType.SingleDay.ToString();
		}

		
    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
