using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences;
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
        private IList<IGroupPageLight> _groupPageOnTeamsTeamBlockPer;
        //private IEventAggregator _eventAggregator;
        private IList<IActivity> _availableActivity;
        //private TeamBlockPerConfiguration _TeamBlockConfiguartion;
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
        	//_eventAggregator = eventAggregator;
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

            //comboBoxGroupPageOnTeamsTeamBlockPer.DataSource = _groupPageOnTeamsTeamBlockPer;
            //comboBoxGroupPageOnTeamsTeamBlockPer.DisplayMember = "Name";
            //comboBoxGroupPageOnTeamsTeamBlockPer.ValueMember = "Key";
        }

        private void bindActivityList()
        {
            comboBoxActivity .DataSource = _availableActivity ;
            comboBoxActivity.DisplayMember = "Name";
            comboBoxActivity.ValueMember = "Name";

        }
    
        private void getDataFromControls()
        {
            //Preferences.UseBlockScheduling = checkBoxBlock.Checked;

            //Preferences.BlockFinderTypeValue = 
            //    radioButtonBetweenDayOff.Checked 
            //    ? BlockFinderType.BetweenDayOff 
            //    : BlockFinderType.SchedulePeriod;


            Preferences.UseTeams = checkBoxTeams.Checked;
        	Preferences.KeepSameDaysOffInTeam = checkBoxKeepWeekEndsTogether.Checked;
			Preferences.UseGroupSchedulingCommonCategory = checkBoxCommonCategory.Checked;
			Preferences.UseGroupSchedulingCommonStart = checkBoxCommonStart.Checked;
			Preferences.UseGroupSchedulingCommonEnd = checkBoxCommonEnd.Checked;
            Preferences.UseCommonActivity = checkBoxCommonActivity.Checked;
            Preferences.CommonActivity = (IActivity)comboBoxActivity.SelectedItem;

            Preferences.GroupPageOnTeam = (IGroupPageLight)comboBoxGroupPageOnTeams.SelectedItem;

            //if ((string) comboBoxGroupPageOnTeamsTeamBlockPer.SelectedValue == "SingleAgentTeam")
            //    Preferences.GroupPageOnTeamBlockPer = null;
            //else
            //Preferences.GroupPageOnTeamBlockPer = (IGroupPageLight)comboBoxGroupPageOnTeamsTeamBlockPer .SelectedItem;

            Preferences.FairnessValue = (double)trackBar1.Value / 100;

            Preferences.GroupPageOnCompareWith = (IGroupPageLight)comboBoxGroupPageOnCompareWith.SelectedItem;

            //if(checkBoxTeamBlockPerBlockScheduling.Checked )
            //    Preferences.BlockFinderTypeForAdvanceOptimization = radioButtonBetweenDaysOffAdvOptimization .Checked
            //        ? BlockFinderType.BetweenDayOff
            //        : BlockFinderType.SchedulePeriod;
                
            //else
            //    Preferences.BlockFinderTypeForAdvanceOptimization = BlockFinderType.None;

            Preferences.UseTeamBlockOption = checkBoxTeamBlockPerBlockScheduling.Checked;
            getTeamBlockPerDataToSave();
        }

        private void setDataToControls()
        {
            //checkBoxBlock.Checked = Preferences.UseBlockScheduling;
            checkBoxTeamBlockPerBlockScheduling.Checked = Preferences.UseTeamBlockOption;

            //switch(Preferences.BlockFinderTypeValue)
            //{
            //    case BlockFinderType.BetweenDayOff:
            //        radioButtonBetweenDayOff.Checked = true;
            //        break;
            //    case BlockFinderType.SchedulePeriod:
            //        radioButtonSchedulePeriod.Checked = true;
            //        break;
            //}

			//if (checkBoxBlock.Checked) Preferences.UseTeams = false;
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

            //comboBoxGroupPageOnTeamsTeamBlockPer.SelectedValue = "SingleAgentTeam";
            //if (Preferences.GroupPageOnTeamBlockPer != null)
            //    comboBoxGroupPageOnTeamsTeamBlockPer.SelectedValue = Preferences.GroupPageOnTeamBlockPer;

            trackBar1.Value = (int)(Preferences.FairnessValue * 100);


            if (Preferences.GroupPageOnCompareWith != null)
                comboBoxGroupPageOnCompareWith.SelectedValue = Preferences.GroupPageOnCompareWith.Key;
            if (comboBoxGroupPageOnCompareWith.SelectedValue == null)
                comboBoxGroupPageOnCompareWith.SelectedIndex = 0;

            switch (Preferences.BlockFinderTypeForAdvanceOptimization)
            {
                case BlockFinderType.BetweenDayOff:
                    //radioButtonBetweenDaysOffAdvOptimization.Checked = true;
                    checkBoxTeamBlockPerBlockScheduling.Checked = true;
                    break;
                case BlockFinderType.SchedulePeriod:
                    //radioButtonSchedulePeriodAdvOptimization.Checked = true;
                    checkBoxTeamBlockPerBlockScheduling.Checked = true;
                    break;
                case BlockFinderType.None:
                    checkBoxTeamBlockPerBlockScheduling.Checked = false;
                    checkBoxTeamBlockPerBlockScheduling.Enabled = false;
                    //radioButtonBetweenDaysOffAdvOptimization.Enabled = false;
                    //radioButtonSchedulePeriodAdvOptimization.Enabled = false;
                    //comboBoxGroupPageOnTeamsTeamBlockPer.Enabled = false;
                    break;
            }
            checkBoxTeams.Checked = Preferences.UseTeams;
            checkBoxTeamBlockPerBlockScheduling.Checked = Preferences.UseTeamBlockOption;
            setLellingPerData();
        }

        //private void checkBoxBlock_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    new ExtraPreferencesPanelUseBlockScheduling { Use = checkBoxBlock.Checked }.PublishEvent("ExtraPreferencesPanelUseBlockScheduling", _eventAggregator);
        //    checkBoxTeams.Enabled = !checkBoxBlock.Checked;
        //    checkBoxTeamBlockPerBlockScheduling.Enabled = !checkBoxBlock.Checked;
        //    setRadioButtonsStatus();
        //}

        //private void setRadioButtonsStatus()
        //{
        //    radioButtonBetweenDayOff.Enabled = checkBoxBlock.Checked;
        //    radioButtonSchedulePeriod.Enabled = checkBoxBlock.Checked;
        //}

        public bool  ValidateDefaultValuesForTeam()
        {
            if (checkBoxTeams.Checked)
                //check if none of the options are not checked. Set the default values
                if (!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked || checkBoxCommonActivity.Checked  ))
                {
                    return false ;
                }
            return true;
        }

        private void checkBoxTeams_CheckedChanged(object sender, System.EventArgs e)
        {
        	//checkBoxBlock.Enabled = !checkBoxTeams.Checked;
            checkBoxTeamBlockPerBlockScheduling.Enabled = !checkBoxTeams.Checked;
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
            //setRadioButtonsStatus();
            setSubItemsOnTeamOptimizationStatus();
        }

        private void checkBoxCommonActivity_CheckedChanged(object sender, System.EventArgs e)
        {
            comboBoxActivity.Enabled = checkBoxCommonActivity.Checked;
        }

        private void checkBoxTeamBlockPerBlockScheduling_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxTeams.Enabled = !checkBoxTeamBlockPerBlockScheduling.Checked;
            //checkBoxBlock.Enabled = !checkBoxTeamBlockPerBlockScheduling.Checked;
            //radioButtonSchedulePeriodAdvOptimization.Enabled = checkBoxTeamBlockPerBlockScheduling.Checked;
            //radioButtonBetweenDaysOffAdvOptimization.Enabled = checkBoxTeamBlockPerBlockScheduling.Checked;
            //comboBoxGroupPageOnTeamsTeamBlockPer.Enabled = checkBoxTeamBlockPerBlockScheduling.Checked;
            //btnTeamBlockPer.Enabled = checkBoxTeamBlockPerBlockScheduling.Checked;
            //if (checkBoxTeamBlockPerBlockScheduling.Checked)
            //{
            //    if(Preferences.BlockFinderTypeForAdvanceOptimization != BlockFinderType.BetweenDayOff )
            //        radioButtonBetweenDaysOffAdvOptimization.Checked = false;
            //    radioButtonSchedulePeriodAdvOptimization.Checked = true;
            //    checkBoxTeams.Checked = false ;
            //    checkBoxBlock.Checked = false;
            //}
            //else
            //{
            //    radioButtonSchedulePeriodAdvOptimization.Checked = false;
            //    radioButtonBetweenDaysOffAdvOptimization.Checked = false;
            //}
            
        }

        //private void btnTeamBlockPer_Click(object sender, EventArgs e)
        //{
        //    TeamBlockPerConfiguration TeamBlockPerConfiguration = new TeamBlockPerConfiguration();
        //    TeamBlockPerConfiguration.SelectedBlockFinderType = Preferences.BlockFinderTypeForAdvanceOptimization;
        //    TeamBlockPerConfiguration.SelectedGroupPage = Preferences.GroupPageOnTeamBlockPer;
        //    TeamBlockPerConfiguration.UseSameEndTime = Preferences.UseTeamBlockSameEndTime;
        //    TeamBlockPerConfiguration.UseSameShiftCategory = Preferences.UseTeamBlockSameShiftCategory;
        //    TeamBlockPerConfiguration.UseSameStartTime = Preferences.UseTeamBlockSameStartTime;
        //    TeamBlockPerConfiguration.UserSameShift = Preferences.UseTeamBlockSameShift;

        //    var TeamBlockPerPrefrences = new TeamBlockPerPrefrences(TeamBlockPerConfiguration, _groupPageOnTeamsTeamBlockPer);
        //    TeamBlockPerPrefrences.ShowDialog();
        //    _TeamBlockConfiguartion = TeamBlockPerPrefrences.TeamBlockConfiguration;
        //    GetTeamBlockPerDataToSave();

        //}

        private void setLellingPerData()
        {
            checkBoxTeamBlockSameShiftCategory.Checked = Preferences.UseTeamBlockSameShiftCategory;
            checkBoxSameStartTime.Checked = Preferences.UseTeamBlockSameStartTime;
            comboBoxTeamBlockType.SelectedValue = Preferences.BlockFinderTypeForAdvanceOptimization.ToString();
        }
        private void getTeamBlockPerDataToSave()
        {
            if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.BetweenDayOff.ToString( ) )
                Preferences.BlockFinderTypeForAdvanceOptimization  =  BlockFinderType.BetweenDayOff ;
            else if((string) comboBoxTeamBlockType.SelectedValue == BlockFinderType.SchedulePeriod.ToString( ) )
                Preferences.BlockFinderTypeForAdvanceOptimization = BlockFinderType.SchedulePeriod;
            Preferences.GroupPageOnTeamBlockPer = _singleAgentEntry;
            Preferences.UseTeamBlockSameEndTime =false ;
            Preferences.UseTeamBlockSameShift = false;
            Preferences.UseTeamBlockSameShiftCategory = checkBoxTeamBlockSameShiftCategory.Checked ;
            Preferences.UseTeamBlockSameStartTime = checkBoxSameStartTime.Checked ;

        }
    }
    public class ExtraPreferencesPanelUseBlockScheduling
    {
        public bool Use { get; set; }
    }
}
