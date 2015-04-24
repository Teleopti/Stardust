﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IList<GroupPageLight> _groupPageOnTeams;
        private IList<GroupPageLight> _groupPageOnCompareWith;
        private IEnumerable<IActivity> _availableActivity;
        private GroupPageLight _singleAgentEntry;
		private IToggleManager _toggleManager;

		public IExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		public void Initialize(
            IExtraPreferences extraPreferences,
			ISchedulerGroupPagesProvider groupPagesProvider, 
			IEnumerable<IActivity> availableActivity, 
			IToggleManager toggleManager)
        {
            Preferences = extraPreferences;
			_toggleManager = toggleManager;
		    _availableActivity = availableActivity;
			_groupPageOnCompareWith = groupPagesProvider.GetGroups(false);
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
			_singleAgentEntry = GroupPageLight.SingleAgentGroup(Resources.NoTeam);
			_groupPageOnTeams.Insert(0, _singleAgentEntry);

			if (_toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317))
			{
				tableLayoutPanel2.RowStyles[1].Height = 0;
				tableLayoutPanel2.RowStyles[0].SizeType = SizeType.Percent;
				tableLayoutPanel2.RowStyles[0].Height = 100;
			}

            ExchangeData(ExchangeDataOption.DataSourceToControls);	    
        }

        #region IDataExchange Members

        private void bindBlockFinderType()
        {
			comboBoxBlockType.DisplayMember = "Value"; 
			comboBoxBlockType.ValueMember = "Key";
			comboBoxBlockType.DataSource = LanguageResourceHelper.TranslateEnumToList<BlockFinderType>();

            comboBoxBlockType.SelectedValue = Preferences.BlockTypeValue;
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
				bindBlockFinderType();
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
			comboBoxTeamGroupPage.DisplayMember = "DisplayName";
			comboBoxTeamGroupPage.ValueMember = "Key";
			comboBoxTeamGroupPage.DataSource = _groupPageOnTeams;

			comboBoxGroupPageOnCompareWith.DataSource = _groupPageOnCompareWith;
			comboBoxGroupPageOnCompareWith.DisplayMember = "Name";
			comboBoxGroupPageOnCompareWith.ValueMember = "Key";
		}

		private void bindActivityList()
        {
            comboBoxTeamActivity.DataSource = _availableActivity ;
			comboBoxTeamActivity.DisplayMember = "DisplayName";
            comboBoxTeamActivity.ValueMember = "Name";
        }
    
        private void getDataFromControls()
        {       
        	Preferences.UseTeamSameDaysOff = checkBoxTeamSameDaysOff.Checked;
			Preferences.UseTeamSameShiftCategory = checkBoxTeamSameShiftCategory.Checked;
			Preferences.UseTeamSameStartTime = checkBoxTeamSameStartTime.Checked;
			Preferences.UseTeamSameEndTime = checkBoxTeamSameEndTime.Checked;
            Preferences.UseTeamSameActivity = checkBoxTeamSameActivity.Checked;
            Preferences.TeamActivityValue = (IActivity)comboBoxTeamActivity.SelectedItem;
			Preferences.GroupPageOnCompareWith = (GroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;
	        Preferences.FairnessValue = _toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317)
		        ? 0d
		        : (double) trackBar1.Value/100;
	        getTeamBlockPerDataToSave();
        }

		private void setDataToControls()
		{
			checkBoxTeamSameDaysOff.Checked = Preferences.UseTeamSameDaysOff;
			checkBoxTeamSameShiftCategory.Checked = Preferences.UseTeamSameShiftCategory;
			checkBoxTeamSameStartTime.Checked = Preferences.UseTeamSameStartTime;
			checkBoxTeamSameEndTime.Checked = Preferences.UseTeamSameEndTime;
			checkBoxTeamSameActivity.Checked = Preferences.UseTeamSameActivity;
			comboBoxTeamActivity.Enabled = checkBoxTeamSameActivity.Checked;
			if (Preferences.TeamActivityValue != null)
				comboBoxTeamActivity.SelectedValue = Preferences.TeamActivityValue.Name;

			comboBoxTeamGroupPage.SelectedValue = Preferences.TeamGroupPage.Key;
			if (comboBoxTeamGroupPage.SelectedValue == null)
				comboBoxTeamGroupPage.SelectedIndex = 0;

			trackBar1.Value = (int) (Preferences.FairnessValue*100);

			comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
			if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
				comboBoxGroupPageOnCompareWith.SelectedIndex = 0;
			setTeamBlockPerData();
		}

		public bool ValidateTeamBlockCombination()
		{
			if (!isTeamEnabled() || !isBlockEnabled())
				return true;

			if ((BlockFinderType)comboBoxBlockType.SelectedValue != BlockFinderType.BetweenDayOff)
				return true;

			if (checkBoxTeamSameDaysOff.Checked)
				return true;

			return false;
		}

	    public bool IsTeamOrBlockChecked()
	    {
            if (BlockFinderType.SingleDay != (BlockFinderType)comboBoxBlockType.SelectedValue || isTeamEnabled())
                return true;
            return false;
	    }

		public bool IsSameShiftChecked()
		{
			return BlockFinderType.SingleDay != (BlockFinderType) comboBoxBlockType.SelectedValue && checkBoxBlockSameShift.Checked;
		}

		public bool ValidateDefaultValuesForTeam()
		{
			if (isTeamEnabled())
			{
				//check if none of the options are not checked. Set the default values
				if (
					!(checkBoxTeamSameShiftCategory.Checked || checkBoxTeamSameStartTime.Checked || checkBoxTeamSameEndTime.Checked ||
					  checkBoxTeamSameActivity.Checked))
					return false;
			}
			return true;
		}

		public bool ValidateDefaultValuesForBlock()
		{
			if (isBlockEnabled())
			{
				if (!(checkBoxBlockSameShift.Checked || checkBoxBlockSameStartTime.Checked || checkBoxBlockSameShiftCategory.Checked))
					return false;
			}
			return true;
		}

        private void setTeamBlockPerData()
        {
        	  checkBoxBlockSameShiftCategory.Checked = Preferences.UseBlockSameShiftCategory;
			  checkBoxBlockSameStartTime.Checked = Preferences.UseBlockSameStartTime;
	        checkBoxBlockSameShift.Checked = Preferences.UseBlockSameShift;
			  comboBoxBlockType.SelectedValue = Preferences.BlockTypeValue;
        }

		private void getTeamBlockPerDataToSave()
		{
			Preferences.UseTeamBlockOption = isBlockEnabled();
			Preferences.BlockTypeValue = (BlockFinderType) comboBoxBlockType.SelectedValue;

			Preferences.UseTeams = isTeamEnabled();
			Preferences.TeamGroupPage = (GroupPageLight) comboBoxTeamGroupPage.SelectedItem;

			Preferences.UseBlockSameEndTime = false;
			Preferences.UseBlockSameShift = checkBoxBlockSameShift.Checked;
			Preferences.UseBlockSameShiftCategory = checkBoxBlockSameShiftCategory.Checked;
			Preferences.UseBlockSameStartTime = checkBoxBlockSameStartTime.Checked;
		}

		private void comboBoxTeamBlockType_SelectedValueChanged(object sender, EventArgs e)
		{
			  var isEnabled = isBlockEnabled();
			  checkBoxBlockSameShiftCategory.Enabled = isEnabled;
			  checkBoxBlockSameStartTime.Enabled = isEnabled;
			  checkBoxBlockSameShift.Enabled = isEnabled;
			if(isBlockEnabled() &&  !(checkBoxBlockSameShiftCategory.Checked  || checkBoxBlockSameStartTime.Checked || checkBoxBlockSameShift.Checked  ))
				checkBoxBlockSameShiftCategory.Checked = true;
		}

		  private void comboBoxGroupPageOnTeams_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = isTeamEnabled();
			  checkBoxTeamSameDaysOff.Enabled = isEnabled;
			  checkBoxTeamSameShiftCategory.Enabled = isEnabled;
			  checkBoxTeamSameStartTime.Enabled = isEnabled;
			  checkBoxTeamSameEndTime.Enabled = isEnabled;
			  checkBoxTeamSameActivity.Enabled = isEnabled;
			  if (isTeamEnabled() && !(checkBoxTeamSameShiftCategory.Checked || checkBoxTeamSameStartTime.Checked || checkBoxTeamSameEndTime.Checked || checkBoxTeamSameActivity.Checked ))
				  checkBoxTeamSameShiftCategory.Checked = true;
		  }

		private bool isTeamEnabled()
		{
			if (comboBoxTeamGroupPage.SelectedValue == null)
				return false;

			return comboBoxTeamGroupPage.SelectedValue.ToString() != _singleAgentEntry.Key;
		}

		private bool isBlockEnabled()
		{
			if (comboBoxBlockType.SelectedValue == null)
				return false;

			return (BlockFinderType)comboBoxBlockType.SelectedValue != BlockFinderType.SingleDay;
		}

		private void checkBoxTeamSameShiftCategory_CheckedChanged(object sender, EventArgs e)
		{
			teamOptimizingSessionOptionsHandler();
		}

		private void checkBoxTeamSameStartTime_CheckedChanged(object sender, EventArgs e)
		{
			teamOptimizingSessionOptionsHandler();
		}

		private void checkBoxTeamSameEndTime_CheckedChanged(object sender, EventArgs e)
		{
			teamOptimizingSessionOptionsHandler();
		}

		private void checkBoxCommonActivity_CheckedChanged(object sender, System.EventArgs e)
		{
			comboBoxTeamActivity.Enabled = checkBoxTeamSameActivity.Checked;
			teamOptimizingSessionOptionsHandler();
		}

		private void teamOptimizingSessionOptionsHandler()
		{
			if (!checkBoxTeamSameEndTime.Checked &&
				!checkBoxTeamSameShiftCategory.Checked &&
				!checkBoxTeamSameStartTime.Checked &&
				!checkBoxTeamSameActivity.Checked)
				comboBoxTeamGroupPage.SelectedIndex = 0;
		}

		private void checkBoxBlockSameShiftCategory_CheckedChanged(object sender, EventArgs e)
		{
			blockOptimizingSessionOptionsHandler();
		}

		private void checkBoxBlockSameStartTime_CheckedChanged(object sender, EventArgs e)
		{
			blockOptimizingSessionOptionsHandler();
		}

		private void checkBoxBlockSameShift_CheckedChanged(object sender, EventArgs e)
		{
			blockOptimizingSessionOptionsHandler();
		}

		private void blockOptimizingSessionOptionsHandler()
		{
			if (!checkBoxBlockSameShift.Checked &&
				!checkBoxBlockSameShiftCategory.Checked &&
				!checkBoxBlockSameStartTime.Checked)
				comboBoxBlockType.SelectedIndex = 0;
		}

    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
