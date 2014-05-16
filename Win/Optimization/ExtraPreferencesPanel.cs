using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {
        private IList<IGroupPageLight> _groupPageOnTeams;
        private IList<IGroupPageLight> _groupPageOnCompareWith;
        private IEnumerable<IActivity> _availableActivity;
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
			ISchedulerGroupPagesProvider groupPagesProvider, IEnumerable<IActivity> availableActivity)
        {
            Preferences = extraPreferences;
		    _availableActivity = availableActivity;
			_groupPageOnCompareWith = groupPagesProvider.GetGroups(false);
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
			_singleAgentEntry = new GroupPageLight { Key = "SingleAgentTeam", Name = Resources.NoTeam };
			_groupPageOnTeams.Insert(0, _singleAgentEntry);
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
			comboBoxTeamGroupPage.DisplayMember = "Name";
			comboBoxTeamGroupPage.ValueMember = "Key";
			comboBoxTeamGroupPage.DataSource = _groupPageOnTeams;

			comboBoxGroupPageOnCompareWith.DataSource = _groupPageOnCompareWith;
			comboBoxGroupPageOnCompareWith.DisplayMember = "Name";
			comboBoxGroupPageOnCompareWith.ValueMember = "Key";
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
			Preferences.FairnessValue = (double)trackBar1.Value / 100;
			Preferences.GroupPageOnCompareWith = (IGroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;
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

			if (Preferences.TeamGroupPage != null)
				comboBoxTeamGroupPage.SelectedValue = Preferences.TeamGroupPage.Key;
			if (comboBoxTeamGroupPage.SelectedValue == null)
				comboBoxTeamGroupPage.SelectedIndex = 0;

			trackBar1.Value = (int) (Preferences.FairnessValue*100);

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

			if ((BlockFinderType)comboBoxBlockType.SelectedValue != BlockFinderType.BetweenDayOff)
				return true;

			if (checkBoxTeamSameDaysOff.Checked)
				return true;

			return false;
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

        private void checkBoxCommonActivity_CheckedChanged(object sender, System.EventArgs e)
        {
            comboBoxTeamActivity.Enabled = checkBoxTeamSameActivity.Checked;
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
			Preferences.TeamGroupPage = (IGroupPageLight) comboBoxTeamGroupPage.SelectedItem;

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

    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
