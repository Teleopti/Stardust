using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	

    public partial class ExtraPreferencesPanel : BaseUserControl, IDataExchange
    {

        private IList<IGroupPageLight> _groupPageOnTeams;
        private IList<IGroupPageLight> _groupPageOnCompareWith;
    	private IEventAggregator _eventAggregator;

        public IExtraPreferences Preferences { get; private set; }

        public ExtraPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IExtraPreferences extraPreferences,
			ISchedulerGroupPagesProvider groupPagesProvider,
			IEventAggregator eventAggregator)
        {
            Preferences = extraPreferences;
			_groupPageOnTeams = groupPagesProvider.GetGroups(false);
        	_eventAggregator = eventAggregator;
			_groupPageOnCompareWith = groupPagesProvider.GetGroups(false);
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
        }

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.DataSourceToControls)
            {
                bindGroupPages();
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
    
        private void getDataFromControls()
        {
            Preferences.UseBlockScheduling = checkBoxBlock.Checked;

            Preferences.BlockFinderTypeValue = 
                radioButtonBetweenDayOff.Checked 
                ? BlockFinderType.BetweenDayOff 
                : BlockFinderType.SchedulePeriod;


            Preferences.UseTeams = checkBoxTeams.Checked;
        	Preferences.KeepSameDaysOffInTeam = checkBoxKeepWeekEndsTogether.Checked;
			Preferences.UseGroupSchedulingCommonCategory = checkBoxCommonCategory.Checked;
			Preferences.UseGroupSchedulingCommonStart = checkBoxCommonStart.Checked;
			Preferences.UseGroupSchedulingCommonEnd = checkBoxCommonEnd.Checked;

            Preferences.GroupPageOnTeam = (IGroupPageLight)comboBoxGroupPageOnTeams.SelectedItem;

            Preferences.FairnessValue = (double)trackBar1.Value / 100;

            Preferences.GroupPageOnCompareWith = (IGroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;

            
        }

        private void setDataToControls()
        {
            checkBoxBlock.Checked = Preferences.UseBlockScheduling;

            switch(Preferences.BlockFinderTypeValue)
            {
                case BlockFinderType.BetweenDayOff:
                    radioButtonBetweenDayOff.Checked = true;
                    break;
                case BlockFinderType.SchedulePeriod:
                    radioButtonSchedulePeriod.Checked = true;
                    break;
            }

			if (checkBoxBlock.Checked) Preferences.UseTeams = false;
			checkBoxTeams.Checked = Preferences.UseTeams;
        	checkBoxKeepWeekEndsTogether.Checked = Preferences.KeepSameDaysOffInTeam;
        	checkBoxCommonCategory.Checked = Preferences.UseGroupSchedulingCommonCategory;
        	checkBoxCommonStart.Checked = Preferences.UseGroupSchedulingCommonStart;
        	checkBoxCommonEnd.Checked = Preferences.UseGroupSchedulingCommonEnd;

            if (Preferences.GroupPageOnTeam != null)
                comboBoxGroupPageOnTeams.SelectedValue = Preferences.GroupPageOnTeam.Key;
            if (comboBoxGroupPageOnTeams.SelectedValue == null)
                comboBoxGroupPageOnTeams.SelectedIndex = 0;

            trackBar1.Value = (int)(Preferences.FairnessValue * 100);


            if (Preferences.GroupPageOnCompareWith != null)
                comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
            if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
                comboBoxGroupPageOnCompareWith.SelectedIndex = 0;


        }

        private void checkBoxBlock_CheckedChanged(object sender, System.EventArgs e)
        {
			new ExtraPreferencesPanelUseBlockScheduling { Use = checkBoxBlock.Checked }.PublishEvent("ExtraPreferencesPanelUseBlockScheduling", _eventAggregator);
        	checkBoxTeams.Enabled = !checkBoxBlock.Checked;
            setRadioButtonsStatus();
        }

        private void setRadioButtonsStatus()
        {
            radioButtonBetweenDayOff.Enabled = checkBoxBlock.Checked;
            radioButtonSchedulePeriod.Enabled = checkBoxBlock.Checked;
        }

        public bool  ValidateDefaultValuesForTeam()
        {
            if (checkBoxTeams.Checked)
                //check if none of the options are not checked. Set the default values
                if (!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked ))
                {
                    return false ;
                }
            return true;
        }

        private void checkBoxTeams_CheckedChanged(object sender, System.EventArgs e)
        {
        	checkBoxBlock.Enabled = !checkBoxTeams.Checked;
            setSubItemsOnTeamOptimizationStatus();
        }

        private void setSubItemsOnTeamOptimizationStatus()
        {
            comboBoxGroupPageOnTeams.Enabled = checkBoxTeams.Checked;
			checkBoxKeepWeekEndsTogether.Enabled = checkBoxTeams.Checked;
			checkBoxCommonCategory.Enabled = checkBoxTeams.Checked;
			checkBoxCommonStart.Enabled = checkBoxTeams.Checked;
			checkBoxCommonEnd.Enabled = checkBoxTeams.Checked;
        }

      

        private void setInitialControlStatus()
        {
            setRadioButtonsStatus();
            setSubItemsOnTeamOptimizationStatus();
        }
    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
