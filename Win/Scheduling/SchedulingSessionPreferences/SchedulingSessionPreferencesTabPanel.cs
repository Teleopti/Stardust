﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesTabPanel : BaseUserControl, IDataExchange
    {
         private ISchedulingOptions _localSchedulingOptions;
        private ISchedulingOptions _schedulingOptions;
        private IEnumerable<IShiftCategory> _shiftCategories;
        private bool _dataLoaded;
    	private IList<IGroupPageLight> _groupPages;
        private IEnumerable<IActivity> _availableActivity;
		private IList<IGroupPageLight> _groupPagesFairness;
        private IEnumerable<IScheduleTag> _scheduleTags;
    	private ISchedulerGroupPagesProvider _groupPagesProvider;
        private GroupPageLight _singleAgentEntry;
	    private IGroupPageLight _noTeamsGroupPage;

	    public SchedulingSessionPreferencesTabPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Initialize(ISchedulingOptions schedulingOptions, IEnumerable<IShiftCategory> shiftCategories, bool reschedule, bool backToLegal, ISchedulerGroupPagesProvider groupPagesProvider,
            IEnumerable<IScheduleTag> scheduleTags, IEnumerable<IActivity> availableActivity)
        {
			_groupPagesProvider = groupPagesProvider;
            _availableActivity = availableActivity;

            if(!reschedule)
            {

            }
            else
            {
            	checkBoxMustHaves.Text = Resources.UsePreferenceMustHavesOnly1;
            }
            if (backToLegal)
            {
                pnlBlockTeamScheduling .Visible = false;
            }


			labelResourceCalculateEveryColon.Visible = true;
			numericUpDownResourceCalculateEvery.Visible = true;
			labelScheduleOrSchedules1.Visible = true;

			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff)
			{
				labelResourceCalculateEveryColon.Visible = false;
				numericUpDownResourceCalculateEvery.Visible = false;
				labelScheduleOrSchedules1.Visible = false;
			}

            _schedulingOptions = schedulingOptions;
            _shiftCategories = shiftCategories;
            _scheduleTags = scheduleTags;
			// inga skill
			_groupPages = _groupPagesProvider.GetGroups(false);
			_groupPagesFairness = _groupPages.ToList();
			_noTeamsGroupPage = new GroupPageLight { Key = "NoTeam", Name = Resources.NoTeam  };
			_singleAgentEntry = new GroupPageLight {Key = "SingleAgentTeam", Name = Resources.SingleAgentTeam};
         ExchangeData(ExchangeDataOption.DataSourceToControls);
         _dataLoaded = true;
        }

		public override string HelpId
		{
			get
			{
				return Tag.ToString();
			}
		}

        public bool ScheduleOnlyRotationDaysVisible
        {
            get { return checkBoxOnlyRotationDays.Visible; }
            set { checkBoxOnlyRotationDays.Visible = value; }
        }

        public bool ScheduleOnlyAvailableDaysVisible
        {
            get { return checkBoxOnlyAvailabilityDays.Visible; }
            set { checkBoxOnlyAvailabilityDays.Visible = value; }
        }
        
        public bool ScheduleOnlyPreferenceDaysVisible
        {
            get { return checkBoxOnlyPreferenceDays.Visible; }
            set { checkBoxOnlyPreferenceDays.Visible = value; }
        }

        public bool ShiftCategoryVisible
        {
            get { return pnlShiftCategory .Visible; }
            set { pnlShiftCategory.Visible = value; }
        }

        public bool UseGroupSchedulingCommonStart
        {
            get { return checkBoxTeamSameStartTime.Checked ; }
            set { checkBoxTeamSameStartTime.Checked = value; }
        }

        public bool UseGroupSchedulingCommonEnd
        {
            get { return checkBoxTeamSameEndTime.Checked ; }
            set { checkBoxTeamSameEndTime.Checked = value; }
        }

        public bool UseGroupSchedulingCommonCategory
        {
            get { return checkBoxTeamSameShiftCategory.Checked ; }
            set { checkBoxTeamSameShiftCategory.Checked = value; }
        }

        public bool UseCommonActivity
        {
            get { return checkBoxTeamSameActivity.Checked; }
            set { checkBoxTeamSameActivity.Checked = value; }
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
                dataOffline();
                initShiftCategories();
                initGroupPages();
                initBlockType();
                initCommonActivity();
				initGroupPagesFairness();
                initTags();
                setDataInControls();
            }
            else
            {
                getDataFromControls();
                dataOnline();
            }
        }

        #endregion

        private void initShiftCategories()
        {
            if (_shiftCategories != null)
            {
                comboBoxAdvShiftCategory.DataSource = null;
                var shiftCategoryBindingList = new TypedBindingCollection<IShiftCategory>();
                foreach (var item in _shiftCategories)
                {
                    shiftCategoryBindingList.Add(item);
                }
                comboBoxAdvShiftCategory.DataSource = shiftCategoryBindingList;
                comboBoxAdvShiftCategory.DisplayMember = "Description";
                comboBoxAdvShiftCategory.SelectedIndex = 0;
            }
        }

        private void initBlockType()
        {
            comboBoxBlockType.DataSource = BlockFinderTypeCreator.GetBlockFinderTypes;
            comboBoxBlockType.DisplayMember = "Name";
            comboBoxBlockType.ValueMember = "Key";
            if (_localSchedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.None)
                comboBoxBlockType.SelectedValue = BlockFinderType.SingleDay.ToString( );
            else
                comboBoxBlockType.SelectedValue = _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling;
        }

		private void initGroupPages()
		{
			var tempGroupPages = _groupPages;
			tempGroupPages.Add(_noTeamsGroupPage);
			comboBoxTeamGroupPage.DataSource = tempGroupPages;
			comboBoxTeamGroupPage.DisplayMember = "Name";
			comboBoxTeamGroupPage.ValueMember  = "Key";
			if(_localSchedulingOptions.GroupOnGroupPage != null)
			{
                comboBoxTeamGroupPage.SelectedValue  = _localSchedulingOptions.GroupOnGroupPage.Key ;
			}
			else
			{
				comboBoxTeamGroupPage.SelectedValue = _noTeamsGroupPage.Key;
			}
           
		}
        private void initCommonActivity()
		{
            comboBoxTeamActivity.DataSource = _availableActivity ;
            comboBoxTeamActivity.DisplayMember = "Name";
            comboBoxTeamActivity.ValueMember = "Name";
            if (_localSchedulingOptions.CommonActivity != null)
            {
                comboBoxTeamActivity.SelectedValue = _localSchedulingOptions.CommonActivity.Name ;
            }
		}

        private void initTags()
        {
            comboBoxAdvTag.DataSource = _scheduleTags;
            comboBoxAdvTag.DisplayMember = "Description";
			comboBoxAdvTag.SelectedItem = _localSchedulingOptions.TagToUseOnScheduling; 
        }

		private void initGroupPagesFairness()
		{
			comboBoxGroupingFairness.DataSource = _groupPagesFairness;
			comboBoxGroupingFairness.DisplayMember = "Name";
            comboBoxGroupingFairness.ValueMember  = "Key";

			if (_localSchedulingOptions.GroupPageForShiftCategoryFairness != null)
			{
				comboBoxGroupingFairness.SelectedValue  = _localSchedulingOptions.GroupPageForShiftCategoryFairness.Key  ;
			}
		}

        private void dataOffline()
        {
            _localSchedulingOptions = (ISchedulingOptions)_schedulingOptions.Clone();
        }

        private void dataOnline()
        {
            _schedulingOptions.UseRotations = _localSchedulingOptions.UseRotations;
            _schedulingOptions.RotationDaysOnly = _localSchedulingOptions.RotationDaysOnly;
            _schedulingOptions.AvailabilityDaysOnly = _localSchedulingOptions.AvailabilityDaysOnly;
            _schedulingOptions.PreferencesDaysOnly = _localSchedulingOptions.PreferencesDaysOnly;
            _schedulingOptions.UseMaximumPersons = _localSchedulingOptions.UseMaximumPersons;
            _schedulingOptions.UseMinimumPersons = _localSchedulingOptions.UseMinimumPersons;
            _schedulingOptions.UseAvailability = _localSchedulingOptions.UseAvailability;
            _schedulingOptions.UseStudentAvailability = _localSchedulingOptions.UseStudentAvailability;
            _schedulingOptions.UsePreferences = _localSchedulingOptions.UsePreferences;
            _schedulingOptions.ShiftCategory = _localSchedulingOptions.ShiftCategory;
            _schedulingOptions.RefreshRate = _localSchedulingOptions.RefreshRate;
            _schedulingOptions.Fairness = _localSchedulingOptions.Fairness;
            _schedulingOptions.UseShiftCategoryLimitations = _localSchedulingOptions.UseShiftCategoryLimitations;
            _schedulingOptions.UsePreferencesMustHaveOnly = _localSchedulingOptions.UsePreferencesMustHaveOnly;
            _schedulingOptions.BlockFinderTypeForAdvanceScheduling =
                _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling;
				_schedulingOptions.UseTeam = _localSchedulingOptions.UseTeam;
        	_schedulingOptions.GroupOnGroupPage = _localSchedulingOptions.GroupOnGroupPage;
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _localSchedulingOptions.GroupOnGroupPageForTeamBlockPer;
            _schedulingOptions.DoNotBreakMaxStaffing = _localSchedulingOptions.DoNotBreakMaxStaffing;
			_schedulingOptions.GroupPageForShiftCategoryFairness = _localSchedulingOptions.GroupPageForShiftCategoryFairness;
        	_schedulingOptions.UseMaxSeats = _localSchedulingOptions.UseMaxSeats;
        	_schedulingOptions.DoNotBreakMaxSeats = _localSchedulingOptions.DoNotBreakMaxSeats;
            _schedulingOptions.UseSameDayOffs = _localSchedulingOptions.UseSameDayOffs;
            _schedulingOptions.TagToUseOnScheduling = _localSchedulingOptions.TagToUseOnScheduling;
        	_schedulingOptions.ResourceCalculateFrequency = _localSchedulingOptions.ResourceCalculateFrequency;
			_schedulingOptions.ShowTroubleshot = _localSchedulingOptions.ShowTroubleshot;
            _schedulingOptions.TeamSameShiftCategory =
                _localSchedulingOptions.TeamSameShiftCategory;
            _schedulingOptions.TeamSameEndTime = _localSchedulingOptions.TeamSameEndTime;
            _schedulingOptions.TeamSameStartTime = _localSchedulingOptions.TeamSameStartTime;
            _schedulingOptions.TeamSameActivity = _localSchedulingOptions.TeamSameActivity;
            _schedulingOptions.CommonActivity = _localSchedulingOptions.CommonActivity;
        	_schedulingOptions.UseAverageShiftLengths = _localSchedulingOptions.UseAverageShiftLengths;
            _schedulingOptions.BlockSameEndTime = _localSchedulingOptions.BlockSameEndTime;
				_schedulingOptions.BlockSameShift = _localSchedulingOptions.BlockSameShift;
            _schedulingOptions.BlockSameShiftCategory = _localSchedulingOptions.BlockSameShiftCategory;
            _schedulingOptions.BlockSameStartTime = _localSchedulingOptions.BlockSameStartTime;
				_schedulingOptions.UseBlock = _localSchedulingOptions.UseBlock;
            
        }

        private void getDataFromControls()
        {
            _localSchedulingOptions.RefreshRate = (int)numericUpDownRefreshRate.Value;
            _localSchedulingOptions.UseRotations = checkBoxUseRotations.Checked;
            _localSchedulingOptions.RotationDaysOnly = checkBoxOnlyRotationDays.Checked;
            _localSchedulingOptions.UseAvailability = checkBoxUseAvailability.Checked;
            _localSchedulingOptions.AvailabilityDaysOnly = checkBoxOnlyAvailabilityDays.Checked;
            _localSchedulingOptions.UseStudentAvailability = checkBoxUseStudentAvailability.Checked;
            _localSchedulingOptions.UseMaximumPersons = checkBoxUseMaximumPersons.Checked;
            _localSchedulingOptions.UseMinimumPersons = checkBoxUseMinimumPersons.Checked;
			  _localSchedulingOptions.UsePreferences = checkBoxUsePreferences.Checked;
            _localSchedulingOptions.PreferencesDaysOnly = checkBoxOnlyPreferenceDays.Checked;
            _localSchedulingOptions.UsePreferencesMustHaveOnly = checkBoxMustHaves.Checked;
			  if (checkBoxUseShiftCategory.Checked)
                _localSchedulingOptions.ShiftCategory = (IShiftCategory)comboBoxAdvShiftCategory.SelectedItem;
            else
                _localSchedulingOptions.ShiftCategory = null;
			  _localSchedulingOptions.Fairness = new Percent(trackBar1.Value / 100d);
            _localSchedulingOptions.UseShiftCategoryLimitations = checkBoxUseShiftCategoryRestrictions.Checked;
			  _localSchedulingOptions.GroupPageForShiftCategoryFairness = (IGroupPageLight)comboBoxGroupingFairness.SelectedItem;
			  _localSchedulingOptions.DoNotBreakMaxStaffing = checkBoxDoNotBreakMaxSeats.Checked;
        	_localSchedulingOptions.UseMaxSeats = checkBoxUseMaxSeats.Checked;
        	_localSchedulingOptions.DoNotBreakMaxSeats = checkBoxDoNotBreakMaxSeats.Checked;
            _localSchedulingOptions.TagToUseOnScheduling = (IScheduleTag)comboBoxAdvTag.SelectedItem;
            _localSchedulingOptions.ResourceCalculateFrequency = (int)numericUpDownResourceCalculateEvery.Value;
			_localSchedulingOptions.ShowTroubleshot = checkBoxShowTroubleShot.Checked;
            
            if (checkBoxTeamSameActivity.Checked)
                _localSchedulingOptions.CommonActivity = (IActivity) comboBoxTeamActivity.SelectedItem;
        	_localSchedulingOptions.UseAverageShiftLengths = checkBoxUseAverageShiftLengths.Checked;
            getTeamBlockDataToSave();
        }

        private void setDataInControls()
        {
            numericUpDownRefreshRate.Value = _localSchedulingOptions.RefreshRate;

            checkBoxUseRotations.Checked = _localSchedulingOptions.UseRotations;

            checkBoxOnlyRotationDays.Checked = _localSchedulingOptions.RotationDaysOnly;
            checkBoxOnlyRotationDays.Enabled = _localSchedulingOptions.UseRotations;

            checkBoxUseAvailability.Checked = _localSchedulingOptions.UseAvailability;

            checkBoxOnlyAvailabilityDays.Checked = _localSchedulingOptions.AvailabilityDaysOnly;
            checkBoxOnlyAvailabilityDays.Enabled = _localSchedulingOptions.UseAvailability;

            checkBoxUseStudentAvailability.Checked = _localSchedulingOptions.UseStudentAvailability;
            
            checkBoxUsePreferences.Checked = _localSchedulingOptions.UsePreferences;

            checkBoxOnlyPreferenceDays.Checked = _localSchedulingOptions.PreferencesDaysOnly;
            checkBoxOnlyPreferenceDays.Enabled = _localSchedulingOptions.UsePreferences;
            
            checkBoxMustHaves.Checked = _localSchedulingOptions.UsePreferencesMustHaveOnly;
            checkBoxMustHaves.Enabled = _localSchedulingOptions.UsePreferences;

            checkBoxUseMaximumPersons.Checked = _localSchedulingOptions.UseMaximumPersons;
            checkBoxUseMinimumPersons.Checked = _localSchedulingOptions.UseMinimumPersons;

            checkBoxUseShiftCategoryRestrictions.Checked = _localSchedulingOptions.UseShiftCategoryLimitations;

            if (_localSchedulingOptions.ShiftCategory != null)
                checkBoxUseShiftCategory.Checked = true;
            else
                checkBoxUseShiftCategory.Checked = false;

            if (_localSchedulingOptions.ShiftCategory != null)
                comboBoxAdvShiftCategory.SelectedItem = _localSchedulingOptions.ShiftCategory;

            comboBoxAdvShiftCategory.Enabled = checkBoxUseShiftCategory.Checked;
            
            trackBar1.Value = (int)(_localSchedulingOptions.Fairness.Value*100);

        	
			checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxStaffing;
        	checkBoxUseMaxSeats.Checked = _localSchedulingOptions.UseMaxSeats;
        	checkBoxDoNotBreakMaxSeats.Enabled = checkBoxUseMaxSeats.Checked;
        	checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxSeats;
        	numericUpDownResourceCalculateEvery.Value = _localSchedulingOptions.ResourceCalculateFrequency;
			checkBoxShowTroubleShot.Checked = _localSchedulingOptions.ShowTroubleshot;
            
        	checkBoxUseAverageShiftLengths.Checked = _localSchedulingOptions.UseAverageShiftLengths;

			
			  setTeamBlockPerDataToSave();
        }

        private void checkBoxUseRotationsCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxOnlyRotationDaysCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxUseAvailabilityCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }
        private void checkBoxUseStudentAvailabilityCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxUsePreferencesCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void comboBoxAdvShiftCategorySelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxUseShiftCategoryCheckedChanged(object sender, EventArgs e)
        {
            comboBoxAdvShiftCategory.Enabled = checkBoxUseShiftCategory.Checked;
        }

        private void trackBar1ValueChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
            }
        }

       

        private void checkBoxOnlyAvailabilityDaysCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxOnlyPreferenceDaysCheckedChanged(object sender, EventArgs e)
        {
			if (checkBoxOnlyPreferenceDays.Checked) checkBoxMustHaves.Checked = false;

            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxUseShiftCategoryRestrictionsCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxMustHavesCheckedChanged(object sender, EventArgs e)
        {
			if (checkBoxMustHaves.Checked) checkBoxOnlyPreferenceDays.Checked = false;

            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void changeGrpSchedulingCommonOptionState(bool value)
        {

            if (value)
            {
                checkBoxTeamSameShiftCategory.Checked = true;
            }
            else
            {
                checkBoxTeamSameShiftCategory.Checked = false;
            }
            checkBoxTeamSameEndTime.Checked = false;
            checkBoxTeamSameStartTime.Checked = false;
            checkBoxTeamSameActivity.Checked = false;
            
            checkBoxTeamSameShiftCategory.Enabled = value;
            checkBoxTeamSameEndTime.Enabled = value;
            checkBoxTeamSameStartTime.Enabled = value;
            checkBoxTeamSameActivity.Enabled = value;
        }

		private void comboBoxGroupingSelectedIndexChanged(object sender, EventArgs e)
		{
			bool isTeamEnabled = (string) comboBoxTeamGroupPage.SelectedValue != _noTeamsGroupPage.Key;
			changeGrpSchedulingCommonOptionState(isTeamEnabled);

			if (_dataLoaded)
			{
				getDataFromControls();
				setDataInControls();
			}
		}
		private void comboBoxGroupingFairnessSelectedIndexChanged(object sender, EventArgs e)
        {

        }

		private void checkBoxUseMaxSeatsCheckedChanged(object sender, EventArgs e)
		{
			if (!checkBoxUseMaxSeats.Checked)
				checkBoxDoNotBreakMaxSeats.Checked = false;
			checkBoxDoNotBreakMaxSeats.Enabled = checkBoxUseMaxSeats.Checked;
		}

		private void tabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl1.SelectedIndex == 0) Tag = "Main";
			if (tabControl1.SelectedIndex == 1) Tag = "Extra";
			if (tabControl1.SelectedIndex == 2) Tag = "Advanced";
		}

        public bool ValidateTeamSchedulingOption()
        {
			  if ((string) comboBoxTeamGroupPage.SelectedValue != _noTeamsGroupPage.Key)
            {
                if (!(checkBoxTeamSameShiftCategory.Checked || checkBoxTeamSameStartTime.Checked || checkBoxTeamSameEndTime.Checked || checkBoxTeamSameActivity.Checked ))
                    return false;
            }
            return true;
        }

        public bool ValidateBlockOption()
        {
            if ((string) comboBoxBlockType.SelectedValue != BlockFinderType.SingleDay.ToString( )  )
            {
                if (!(checkBoxBlockSameShiftCategory .Checked || checkBoxBlockSameStartTime .Checked || checkBoxBlockSameShift .Checked ))
                    return false;
            }
            return true;
        }

        public void HideTeamAndBlockSchedulingOptions()
        {
            var row = tableLayoutPanel2.GetRow(pnlBlockTeamScheduling);
            tableLayoutPanel2.Controls.Remove(pnlBlockTeamScheduling);
            tableLayoutPanel2.RowStyles.RemoveAt(row);
            foreach (Control control in tableLayoutPanel2.Controls)
            {
                var rowIndex = tableLayoutPanel2.GetRow(control);
                if (rowIndex > row)
                    tableLayoutPanel2.SetRow(control, rowIndex - 1);
            }
        }

		private void checkBoxCommonActivity_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxTeamActivity.Enabled = checkBoxTeamSameActivity.Checked;
		}

       
        private void getTeamBlockDataToSave()
        {
			  //team
			  _localSchedulingOptions.UseTeam = (string) comboBoxTeamGroupPage.SelectedValue != _noTeamsGroupPage.Key;
			  _localSchedulingOptions.GroupOnGroupPage = (IGroupPageLight)comboBoxTeamGroupPage.SelectedItem;
			  _localSchedulingOptions.TeamSameShiftCategory = checkBoxTeamSameShiftCategory.Checked;
			  _localSchedulingOptions.TeamSameStartTime = checkBoxTeamSameStartTime.Checked;
			  _localSchedulingOptions.TeamSameEndTime = checkBoxTeamSameEndTime.Checked;
			  _localSchedulingOptions.TeamSameActivity = checkBoxTeamSameActivity.Checked;

			  //block
	        if ((string) comboBoxBlockType.SelectedValue == BlockFinderType.SingleDay.ToString())
		        _localSchedulingOptions.UseBlock = false;
	        else
				  _localSchedulingOptions.UseBlock = true;
	        
			  if ((string) comboBoxBlockType.SelectedValue == BlockFinderType.BetweenDayOff.ToString())
                _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
            else if ((string) comboBoxBlockType.SelectedValue == BlockFinderType.SchedulePeriod.ToString())
                _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			  if ((string) comboBoxTeamGroupPage.SelectedValue == _noTeamsGroupPage.Key)
                _localSchedulingOptions.GroupOnGroupPageForTeamBlockPer = _singleAgentEntry;
            else
                _localSchedulingOptions.GroupOnGroupPageForTeamBlockPer = (IGroupPageLight)comboBoxTeamGroupPage.SelectedItem;
            _localSchedulingOptions.BlockSameEndTime = false;
            _localSchedulingOptions.BlockSameShiftCategory = checkBoxBlockSameShiftCategory.Checked ;
            _localSchedulingOptions.BlockSameStartTime = checkBoxBlockSameStartTime.Checked ;
				_localSchedulingOptions.BlockSameShift = checkBoxBlockSameShift.Checked;

        }

        private void setTeamBlockPerDataToSave()
        {
			  if (_localSchedulingOptions.UseTeam)
			  {
				  checkBoxTeamSameShiftCategory.Checked = _localSchedulingOptions.TeamSameShiftCategory;
				  checkBoxTeamSameEndTime.Checked = _localSchedulingOptions.TeamSameEndTime;
				  checkBoxTeamSameStartTime.Checked = _localSchedulingOptions.TeamSameStartTime;
				  checkBoxTeamSameActivity.Checked = _localSchedulingOptions.TeamSameActivity;
				  comboBoxTeamActivity.Enabled = _localSchedulingOptions.TeamSameActivity;
			  }

	        if (!_localSchedulingOptions.UseBlock)
		        comboBoxBlockType.SelectedValue = BlockFinderType.SingleDay.ToString();
	        else
	        {
				  if (_localSchedulingOptions.BlockFinderTypeForAdvanceScheduling != BlockFinderType.None)
					  comboBoxBlockType.SelectedValue = _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling.ToString();
	        }
			  checkBoxBlockSameShiftCategory.Checked = _localSchedulingOptions.BlockSameShiftCategory;
           checkBoxBlockSameStartTime.Checked = _localSchedulingOptions.BlockSameStartTime;
			  checkBoxBlockSameShift.Checked = _localSchedulingOptions.BlockSameShift;
        }

		  private void comboBoxBlockType_SelectedValueChanged(object sender, EventArgs e)
		  {
			  var isEnabled = false;
			  if ((string) comboBoxBlockType.SelectedValue != BlockFinderType.SingleDay.ToString())
			  {
				  checkBoxBlockSameShiftCategory.Checked = true;
				  isEnabled = true;
			  }
			  else
			  {
				  checkBoxBlockSameShiftCategory.Checked = false;
				  checkBoxBlockSameShift.Checked = false;
				  checkBoxBlockSameStartTime.Checked = false;
			  }
			  checkBoxBlockSameStartTime.Enabled = isEnabled ;
			  checkBoxBlockSameShift.Enabled = isEnabled;
			  checkBoxBlockSameShiftCategory.Enabled = isEnabled;
		  }
    }
    
}
