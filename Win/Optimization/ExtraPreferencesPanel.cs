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
			comboBoxTeamBlockType.DisplayMember = "Value"; 
			comboBoxTeamBlockType.ValueMember = "Key";
			comboBoxTeamBlockType.DataSource = LanguageResourceHelper.TranslateEnumToList<BlockFinderType>();

            comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization;
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
			comboBoxGroupPageOnTeams.DisplayMember = "Name";
			comboBoxGroupPageOnTeams.ValueMember = "Key";
			comboBoxGroupPageOnTeams.DataSource = _groupPageOnTeams;

			comboBoxGroupPageOnCompareWith.DataSource = _groupPageOnCompareWith;
			comboBoxGroupPageOnCompareWith.DisplayMember = "Name";
			comboBoxGroupPageOnCompareWith.ValueMember = "Key";
		}

		private void bindActivityList()
        {
            comboBoxActivity.DataSource = _availableActivity ;
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

			if (Preferences.GroupPageOnTeam != null)
				comboBoxGroupPageOnTeams.SelectedValue = Preferences.GroupPageOnTeam.Key;
			if (comboBoxGroupPageOnTeams.SelectedValue == null)
				comboBoxGroupPageOnTeams.SelectedIndex = 0;

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

			if ((BlockFinderType)comboBoxTeamBlockType.SelectedValue != BlockFinderType.BetweenDayOff)
				return true;

			if (checkBoxKeepWeekEndsTogether.Checked)
				return true;

			return false;
		}

		public bool ValidateDefaultValuesForTeam()
		{
			if (isTeamEnabled())
			{
				//check if none of the options are not checked. Set the default values
				if (
					!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked ||
					  checkBoxCommonActivity.Checked))
					return false;
			}
			return true;
		}

		public bool ValidateDefaultValuesForBlock()
		{
			if (isBlockEnabled())
			{
				if (!(checkBoxTeamBlockSameShift.Checked || checkBoxSameStartTime.Checked || checkBoxTeamBlockSameShiftCategory.Checked))
					return false;
			}
			return true;
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

			comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization;
        }

		private void getTeamBlockPerDataToSave()
		{
			Preferences.UseTeamBlockOption = isBlockEnabled();
			Preferences.BlockFinderTypeForAdvanceOptimization = (BlockFinderType) comboBoxTeamBlockType.SelectedValue;

			Preferences.UseTeams = isTeamEnabled();
			Preferences.GroupPageOnTeam = (IGroupPageLight) comboBoxGroupPageOnTeams.SelectedItem;

			Preferences.UseTeamBlockSameEndTime = false;
			Preferences.UseTeamBlockSameShift = checkBoxTeamBlockSameShift.Checked;
			Preferences.UseTeamBlockSameShiftCategory = checkBoxTeamBlockSameShiftCategory.Checked;
			Preferences.UseTeamBlockSameStartTime = checkBoxSameStartTime.Checked;
		}

		private void comboBoxTeamBlockType_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = isBlockEnabled();
			  checkBoxTeamBlockSameShiftCategory.Enabled = isEnabled;
			  checkBoxSameStartTime.Enabled = isEnabled;
			  checkBoxTeamBlockSameShift.Enabled = isEnabled;
		  }

		  private void comboBoxGroupPageOnTeams_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = isTeamEnabled();
			  checkBoxKeepWeekEndsTogether.Enabled = isEnabled;
			  checkBoxCommonCategory.Enabled = isEnabled;
			  checkBoxCommonStart.Enabled = isEnabled;
			  checkBoxCommonEnd.Enabled = isEnabled;
			  checkBoxCommonActivity.Enabled = isEnabled;
		  }

		private bool isTeamEnabled()
		{
			if (comboBoxGroupPageOnTeams.SelectedValue == null)
				return false;

			return comboBoxGroupPageOnTeams.SelectedValue.ToString() != _singleAgentEntry.Key;
		}

		private bool isBlockEnabled()
		{
			if (comboBoxTeamBlockType.SelectedValue == null)
				return false;

			return (BlockFinderType)comboBoxTeamBlockType.SelectedValue != BlockFinderType.SingleDay;
		}

    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
