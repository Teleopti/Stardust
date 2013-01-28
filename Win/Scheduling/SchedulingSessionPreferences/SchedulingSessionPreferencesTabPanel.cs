﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesTabPanel : BaseUserControl, IDataExchange
    {
         private ISchedulingOptions _localSchedulingOptions;
        private ISchedulingOptions _schedulingOptions;
        private IList<IShiftCategory> _shiftCategories;
        private bool _dataLoaded;
    	private IList<IGroupPageLight> _groupPages;
        private IList<IActivity> _availableActivity;
		private IList<IGroupPageLight> _groupPagesFairness;
        private IList<IScheduleTag> _scheduleTags;
    	private ISchedulerGroupPagesProvider _groupPagesProvider;

    	public SchedulingSessionPreferencesTabPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Initialize(ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories, bool reschedule, bool backToLegal, ISchedulerGroupPagesProvider groupPagesProvider,
            IList<IScheduleTag> scheduleTags, IList<IActivity> availableActivity)
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
            _shiftCategories = (from s in shiftCategories where ((IDeleteTag)s).IsDeleted == false select s).ToList();
            _scheduleTags = scheduleTags;
			// inga skill
			_groupPages = _groupPagesProvider.GetGroups(false);
			_groupPagesFairness = _groupPages.ToList();
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

        public bool UseBlockSchedulingVisible
        {
            get { return checkBoxUseBlockScheduling.Visible; }
            set { checkBoxUseBlockScheduling.Visible = value; }
        }

        public bool BetweenDayOffVisible
        {
            get { return radioButtonBetweenDayOff.Visible; }
            set { radioButtonBetweenDayOff.Visible = value; }
        }

        public bool SchedulePeriodVisible
        {
            get { return radioButtonSchedulePeriod.Visible; }
            set { radioButtonSchedulePeriod.Visible = value; }
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
            get { return checkBoxCommonStart.Checked ; }
            set { checkBoxCommonStart.Checked = value; }
        }

        public bool UseGroupSchedulingCommonEnd
        {
            get { return checkBoxCommonEnd.Checked ; }
            set { checkBoxCommonEnd.Checked = value; }
        }

        public bool UseGroupSchedulingCommonCategory
        {
            get { return checkBoxCommonCategory.Checked ; }
            set { checkBoxCommonCategory.Checked = value; }
        }

        public bool UseCommonActivity
        {
            get { return checkBoxCommonActivity.Checked; }
            set { checkBoxCommonActivity.Checked = value; }
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

		private void initGroupPages()
		{
			comboBoxGrouping.DataSource = _groupPages;
            comboBoxGrouping.DisplayMember = "Name";
		    comboBoxGrouping.ValueMember  = "Key";
			if(_localSchedulingOptions.GroupOnGroupPage != null)
			{
                comboBoxGrouping.SelectedValue  = _localSchedulingOptions.GroupOnGroupPage.Key ;
			}
		}
        private void initCommonActivity()
		{
            comboBoxActivity.DataSource = _availableActivity ;
            comboBoxActivity.DisplayMember = "Name";
            comboBoxActivity.ValueMember = "Name";
            if (_localSchedulingOptions.CommonActivity != null)
            {
                comboBoxActivity.SelectedValue = _localSchedulingOptions.CommonActivity.Name ;
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
            _schedulingOptions.UseBlockScheduling = _localSchedulingOptions.UseBlockScheduling;
            _schedulingOptions.BlockFinderTypeForAdvanceScheduling =
                _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling;
        	_schedulingOptions.UseGroupScheduling = _localSchedulingOptions.UseGroupScheduling;
        	_schedulingOptions.GroupOnGroupPage = _localSchedulingOptions.GroupOnGroupPage;
            _schedulingOptions.DoNotBreakMaxStaffing = _localSchedulingOptions.DoNotBreakMaxStaffing;
			_schedulingOptions.GroupPageForShiftCategoryFairness = _localSchedulingOptions.GroupPageForShiftCategoryFairness;
        	_schedulingOptions.UseMaxSeats = _localSchedulingOptions.UseMaxSeats;
        	_schedulingOptions.DoNotBreakMaxSeats = _localSchedulingOptions.DoNotBreakMaxSeats;
            _schedulingOptions.UseSameDayOffs = _localSchedulingOptions.UseSameDayOffs;
            _schedulingOptions.UseBlockOptimizing = _localSchedulingOptions.UseBlockScheduling;
            _schedulingOptions.TagToUseOnScheduling = _localSchedulingOptions.TagToUseOnScheduling;
        	_schedulingOptions.ResourceCalculateFrequency = _localSchedulingOptions.ResourceCalculateFrequency;
			_schedulingOptions.ShowTroubleshot = _localSchedulingOptions.ShowTroubleshot;
            _schedulingOptions.UseGroupSchedulingCommonCategory =
                _localSchedulingOptions.UseGroupSchedulingCommonCategory;
            _schedulingOptions.UseGroupSchedulingCommonEnd = _localSchedulingOptions.UseGroupSchedulingCommonEnd;
            _schedulingOptions.UseGroupSchedulingCommonStart = _localSchedulingOptions.UseGroupSchedulingCommonStart;
            _schedulingOptions.UseCommonActivity = _localSchedulingOptions.UseCommonActivity;
            _schedulingOptions.CommonActivity = _localSchedulingOptions.CommonActivity;
        	_schedulingOptions.UseAverageShiftLengths = _localSchedulingOptions.UseAverageShiftLengths;
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

            
            if (!checkBoxUseBlockScheduling.Checked)
                _localSchedulingOptions.UseBlockScheduling = BlockFinderType.None;
            else
            {
                if (radioButtonBetweenDayOff.Checked)
                    _localSchedulingOptions.UseBlockScheduling = BlockFinderType.BetweenDayOff;
                else
                    _localSchedulingOptions.UseBlockScheduling = BlockFinderType.SchedulePeriod;
            }

            if (!checkBoxLevellingPerBlockScheduling.Checked)
                _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling  = BlockFinderType.None;
            else
            {
                if (radioButtonBetweenDaysOffAdvScheduling.Checked)
                    _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
                else
                    _localSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
            }

            _localSchedulingOptions.Fairness = new Percent(trackBar1.Value / 100d);
            _localSchedulingOptions.UseShiftCategoryLimitations = checkBoxUseShiftCategoryRestrictions.Checked;
			_localSchedulingOptions.UseGroupScheduling = checkBoxUseGroupScheduling.Checked;
        	_localSchedulingOptions.GroupOnGroupPage = (IGroupPageLight)comboBoxGrouping.SelectedItem;
			_localSchedulingOptions.GroupPageForShiftCategoryFairness = (IGroupPageLight)comboBoxGroupingFairness.SelectedItem;
			_localSchedulingOptions.DoNotBreakMaxStaffing = checkBoxDoNotBreakMaxSeats.Checked;
        	_localSchedulingOptions.UseMaxSeats = checkBoxUseMaxSeats.Checked;
        	_localSchedulingOptions.DoNotBreakMaxSeats = checkBoxDoNotBreakMaxSeats.Checked;
            _localSchedulingOptions.TagToUseOnScheduling = (IScheduleTag)comboBoxAdvTag.SelectedItem;
            _localSchedulingOptions.ResourceCalculateFrequency = (int)numericUpDownResourceCalculateEvery.Value;
			_localSchedulingOptions.ShowTroubleshot = checkBoxShowTroubleShot.Checked;
            _localSchedulingOptions.UseGroupSchedulingCommonCategory = checkBoxCommonCategory.Checked;
            _localSchedulingOptions.UseGroupSchedulingCommonStart = checkBoxCommonStart.Checked;
            _localSchedulingOptions.UseGroupSchedulingCommonEnd = checkBoxCommonEnd.Checked;
            _localSchedulingOptions.UseCommonActivity = checkBoxCommonActivity.Checked;
            if (checkBoxCommonActivity.Checked)
                _localSchedulingOptions.CommonActivity = (IActivity) comboBoxActivity.SelectedItem;
        	_localSchedulingOptions.UseAverageShiftLengths = checkBoxUseAverageShiftLengths.Checked;
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

            if (mustHaveSetAndOnlyPreferenceDaysVisible())
            {
                checkBoxOnlyPreferenceDays.Checked = true;
                checkBoxOnlyPreferenceDays.Enabled = false;
            }

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

            switch (_localSchedulingOptions.UseBlockScheduling)
            {
                case BlockFinderType.None :
                    radioButtonBetweenDayOff.Enabled = false;
                    radioButtonSchedulePeriod.Enabled = false;
                    break;

                case BlockFinderType.BetweenDayOff :
                    checkBoxUseBlockScheduling.Checked = true;
                    radioButtonBetweenDayOff.Enabled = true;
                    radioButtonBetweenDayOff.Checked = true;
                    radioButtonSchedulePeriod.Enabled = true;
                    break;

                case BlockFinderType.SchedulePeriod :
                    checkBoxUseBlockScheduling.Checked = true;
                    radioButtonBetweenDayOff.Enabled = true;
                    radioButtonSchedulePeriod.Enabled = true;
                    radioButtonSchedulePeriod.Checked = true;
                    break;

            }

            switch (_localSchedulingOptions.BlockFinderTypeForAdvanceScheduling )
            {
                case BlockFinderType.None:
                    radioButtonBetweenDaysOffAdvScheduling .Enabled = false;
                    radioButtonSchedulePeriodAdvScheduling.Enabled = false;
                    break;

                case BlockFinderType.BetweenDayOff:
                    checkBoxLevellingPerBlockScheduling .Checked = true;
                    radioButtonBetweenDaysOffAdvScheduling.Enabled = true;
                    radioButtonBetweenDaysOffAdvScheduling.Checked = true;
                    radioButtonSchedulePeriodAdvScheduling.Enabled = true;
                    break;

                case BlockFinderType.SchedulePeriod:
                    checkBoxLevellingPerBlockScheduling .Checked = true;
                    radioButtonBetweenDaysOffAdvScheduling.Enabled = true;
                    radioButtonSchedulePeriodAdvScheduling.Checked = true;
                    radioButtonSchedulePeriodAdvScheduling.Enabled = true;
                    break;

            }

        	checkBoxUseGroupScheduling.Checked = _localSchedulingOptions.UseGroupScheduling;
        	comboBoxGrouping.Enabled = checkBoxUseGroupScheduling.Checked;
			checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxStaffing;
        	checkBoxUseMaxSeats.Checked = _localSchedulingOptions.UseMaxSeats;
        	checkBoxDoNotBreakMaxSeats.Enabled = checkBoxUseMaxSeats.Checked;
        	checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxSeats;
        	numericUpDownResourceCalculateEvery.Value = _localSchedulingOptions.ResourceCalculateFrequency;
			checkBoxShowTroubleShot.Checked = _localSchedulingOptions.ShowTroubleshot;
            if(_localSchedulingOptions.UseGroupScheduling )
            {
                checkBoxCommonCategory.Checked = _localSchedulingOptions.UseGroupSchedulingCommonCategory;
                checkBoxCommonEnd.Checked = _localSchedulingOptions.UseGroupSchedulingCommonEnd;
                checkBoxCommonStart.Checked = _localSchedulingOptions.UseGroupSchedulingCommonStart;
                checkBoxCommonActivity.Checked = _localSchedulingOptions.UseCommonActivity;
                comboBoxActivity.Enabled = _localSchedulingOptions.UseCommonActivity;
            }
        	checkBoxUseAverageShiftLengths.Checked = _localSchedulingOptions.UseAverageShiftLengths;
        }

        private bool mustHaveSetAndOnlyPreferenceDaysVisible()
        {
            return checkBoxMustHaves.Checked && checkBoxOnlyPreferenceDays.Visible && checkBoxOnlyRotationDays.Enabled;
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
            if (checkBoxUseShiftCategory.Checked)
                checkBoxUseBlockScheduling.Checked = false;
            checkBoxUseBlockScheduling.Enabled = !checkBoxUseShiftCategory.Checked;
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
            if (_dataLoaded)
            {
                getDataFromControls();
                setDataInControls();
            }
        }

        private void checkBoxUseBlockSchedulingCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseBlockScheduling.Checked)
            {
                checkBoxUseShiftCategory.Checked = false;
                radioButtonBetweenDayOff.Checked = true;
            }
            checkBoxLevellingPerBlockScheduling.Enabled = !checkBoxUseBlockScheduling.Checked;
            checkBoxUseShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            comboBoxAdvShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            radioButtonSchedulePeriod.Enabled = checkBoxUseBlockScheduling.Checked;
            radioButtonBetweenDayOff.Enabled = checkBoxUseBlockScheduling.Checked;
			checkBoxUseGroupScheduling.Enabled = !checkBoxUseBlockScheduling.Checked;
			if (checkBoxUseGroupScheduling.Checked && checkBoxUseBlockScheduling.Checked)
				checkBoxUseGroupScheduling.Checked = false;
        }

        private void radioButtonBetweenDayOffCheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonBetweenDayOff.Checked)
                radioButtonSchedulePeriod.Checked = false;
        }

        private void radioButtonSchedulePeriodCheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonSchedulePeriod.Checked)
                radioButtonBetweenDayOff.Checked = false;
        }

		private void checkBoxUseGroupSchedulingCheckedChanged(object sender, EventArgs e)
		{
			comboBoxGrouping.Enabled = checkBoxUseGroupScheduling.Checked;


			checkBoxUseBlockScheduling.Enabled = !checkBoxUseGroupScheduling.Checked;
            checkBoxLevellingPerBlockScheduling.Enabled = !checkBoxUseGroupScheduling.Checked;
			if (checkBoxUseGroupScheduling.Checked && checkBoxUseBlockScheduling.Checked)
				checkBoxUseBlockScheduling.Checked = false;

		    changeGrpSchedulingCommonOptionState(checkBoxUseGroupScheduling.Checked);
		}

        private void changeGrpSchedulingCommonOptionState(bool value)
        {

            if (value)
            {
                checkBoxCommonCategory.Checked = true;
            }
            else
            {
                checkBoxCommonCategory.Checked = false;
            }
            checkBoxCommonEnd.Checked = false;
            checkBoxCommonStart.Checked = false;
            checkBoxCommonActivity.Checked = false;
            
            checkBoxCommonCategory.Enabled = value;
            checkBoxCommonEnd.Enabled = value;
            checkBoxCommonStart.Enabled = value;
            checkBoxCommonActivity.Enabled = value;
        }

		private void comboBoxGroupingSelectedIndexChanged(object sender, EventArgs e)
		{
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
            if(checkBoxUseGroupScheduling.Checked )
            {
                if (!(checkBoxCommonCategory.Checked || checkBoxCommonStart.Checked || checkBoxCommonEnd.Checked || checkBoxCommonActivity.Checked ))
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
            if (checkBoxCommonActivity.Checked)
                comboBoxActivity .Enabled = true;
            else
            {
                comboBoxActivity.Enabled = false;
            }
        }

        private void checkBoxLevellingPerBlockScheduling_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLevellingPerBlockScheduling.Checked)
            {
                radioButtonSchedulePeriodAdvScheduling .Checked = true ;
                radioButtonBetweenDaysOffAdvScheduling .Checked = false ;
                checkBoxUseGroupScheduling.Checked = false;
                checkBoxUseBlockScheduling.Checked = false;

            }
            else
            {
                radioButtonSchedulePeriodAdvScheduling.Checked = false ;
                radioButtonBetweenDaysOffAdvScheduling.Checked = false;
            }
            radioButtonSchedulePeriodAdvScheduling.Enabled = checkBoxLevellingPerBlockScheduling.Checked;
            radioButtonBetweenDaysOffAdvScheduling.Enabled = checkBoxLevellingPerBlockScheduling.Checked;
            checkBoxUseGroupScheduling.Enabled  = !checkBoxLevellingPerBlockScheduling.Checked;
            checkBoxUseBlockScheduling.Enabled = !checkBoxLevellingPerBlockScheduling.Checked;
            //checkBoxUseShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            //comboBoxAdvShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            //radioButtonSchedulePeriod.Enabled = checkBoxUseBlockScheduling.Checked;
            //radioButtonBetweenDayOff.Enabled = checkBoxUseBlockScheduling.Checked;
            //checkBoxUseGroupScheduling.Enabled = !checkBoxUseBlockScheduling.Checked;
            
                
        }
    }
    
}
