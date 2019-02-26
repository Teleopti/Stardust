using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
{
	public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IList<GroupPageLight> _groupPageOnTeams;
        private IEnumerable<IActivity> _availableActivity;
        private GroupPageLight _singleAgentEntry;

		public ExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
        }

		public void Initialize(ExtraPreferences extraPreferences, SchedulerGroupPagesProvider groupPagesProvider, IEnumerable<IActivity> availableActivity, bool useRightToLeft)
        {
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
			Preferences = extraPreferences;
		    _availableActivity = availableActivity;
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
			_singleAgentEntry = GroupPageLight.SingleAgentGroup(Resources.NoTeam);
			_groupPageOnTeams.Insert(0, _singleAgentEntry);
			tableLayoutPanel2.RowStyles[1].Height = 0;
			tableLayoutPanel2.RowStyles[0].SizeType = SizeType.Percent;
			tableLayoutPanel2.RowStyles[0].Height = 100;

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
		}

		private void bindActivityList()
        {
            comboBoxTeamActivity.DataSource = _availableActivity ;
			comboBoxTeamActivity.DisplayMember = "Name";
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

			setTeamBlockPerData();
		}

		public bool ValidateTeamBlockSameDaysOffCombination()
		{
			if (!isTeamSelected() || !isBlockSelected())
				return true;

			if ((BlockFinderType)comboBoxBlockType.SelectedValue != BlockFinderType.BetweenDayOff)
				return true;

			if (checkBoxTeamSameDaysOff.Checked)
				return true;

			return false;
		}

	    public bool IsTeamOrBlockSelected()
	    {
			if (isBlockSelected() || isTeamSelected())
                return true;

            return false;
	    }

		public bool IsSameShiftChecked()
		{
			return BlockFinderType.SingleDay != (BlockFinderType) comboBoxBlockType.SelectedValue && checkBoxBlockSameShift.Checked;
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
			Preferences.UseTeamBlockOption = isBlockSelected();
			Preferences.BlockTypeValue = (BlockFinderType) comboBoxBlockType.SelectedValue;

			Preferences.UseTeams = isTeamSelected();
			Preferences.TeamGroupPage = (GroupPageLight) comboBoxTeamGroupPage.SelectedItem;

			Preferences.UseBlockSameEndTime = false;
			Preferences.UseBlockSameShift = checkBoxBlockSameShift.Checked;
			Preferences.UseBlockSameShiftCategory = checkBoxBlockSameShiftCategory.Checked;
			Preferences.UseBlockSameStartTime = checkBoxBlockSameStartTime.Checked;
		}

		private void comboBoxTeamBlockType_SelectedValueChanged(object sender, EventArgs e)
		{
			  var isEnabled = isBlockSelected();
			  checkBoxBlockSameShiftCategory.Enabled = isEnabled;
			  checkBoxBlockSameStartTime.Enabled = isEnabled;
			  checkBoxBlockSameShift.Enabled = isEnabled;
			if(isBlockSelected() &&  !(checkBoxBlockSameShiftCategory.Checked  || checkBoxBlockSameStartTime.Checked || checkBoxBlockSameShift.Checked  ))
				checkBoxBlockSameShiftCategory.Checked = true;
		}

		  private void comboBoxGroupPageOnTeams_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = isTeamSelected();
			  checkBoxTeamSameDaysOff.Enabled = isEnabled;
			  checkBoxTeamSameShiftCategory.Enabled = isEnabled;
			  checkBoxTeamSameStartTime.Enabled = isEnabled;
			  checkBoxTeamSameEndTime.Enabled = isEnabled;
			  checkBoxTeamSameActivity.Enabled = isEnabled;
			  if (isTeamSelected() && !(checkBoxTeamSameShiftCategory.Checked || checkBoxTeamSameStartTime.Checked || checkBoxTeamSameEndTime.Checked || checkBoxTeamSameActivity.Checked ))
				  checkBoxTeamSameShiftCategory.Checked = true;
		  }

		private bool isTeamSelected()
		{
			if (comboBoxTeamGroupPage.SelectedValue == null)
				return false;

			return comboBoxTeamGroupPage.SelectedValue.ToString() != _singleAgentEntry.Key;
		}

		private bool isBlockSelected()
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
