using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class SchedulingSessionPreferencesPanel : BaseUserControl, IDataExchange
    {
        private ISchedulingOptions _localSchedulingOptions;
        private ISchedulingOptions _schedulingOptions;
        private IList<IShiftCategory> _shiftCategories;
        private bool _dataLoaded;
    	private IList<IGroupPage> _groupPages;
		private IList<IGroupPage> _groupPagesFairness;
        private bool _optimize;
        private IList<IScheduleTag> _scheduleTags;

        public SchedulingSessionPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Initialize(ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
			bool reschedule, bool backToLegal, IList<IGroupPage> groupPages, 
            bool optimize, IList<IScheduleTag> scheduleTags)
        {
            if(!reschedule)
            {

            }
            else
            {
            	checkBoxMustHaves.Text = Resources.UsePreferenceMustHavesOnly1;
            }
            if (backToLegal)
            {
                groupBox3.Visible = false;
            }

            _optimize = optimize;

            if (optimize)
            {
                checkBoxUseGroupScheduling.Text = Resources.UseTeam;
                groupBox3.Text = Resources.Team;
            }

            _schedulingOptions = schedulingOptions;
            _shiftCategories = (from s in shiftCategories where ((IDeleteTag)s).IsDeleted == false select s).ToList();
            _scheduleTags = scheduleTags;
            var specification = new NotSkillGroupSpecification();
        	_groupPages = new List<IGroupPage>(groupPages).FindAll(specification.IsSatisfiedBy);
			_groupPagesFairness = _groupPages.ToList();
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            _dataLoaded = true;
            
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

        public bool UseSameDayOffsVisible
        {
            get { return checkBoxUseSameDayOffs.Visible; }
            set { checkBoxUseSameDayOffs.Visible = value; }
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
            get { return groupBoxShiftCategory.Visible; }
            set { groupBoxShiftCategory.Visible = value; }
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
                DataOffline();
                InitShiftCategories();
                InitGroupPages();
				InitGroupPagesFairness();
                initTags();
                SetDataInControls();
            }
            else
            {
                GetDataFromControls();
                DataOnline();
            }
        }

        #endregion

        private void InitShiftCategories()
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

		private void InitGroupPages()
		{
			comboBoxGrouping.DataSource = _groupPages;
			comboBoxGrouping.DisplayMember = "Description";

			if(_localSchedulingOptions.GroupOnGroupPage != null)
			{
				comboBoxGrouping.SelectedItem = _localSchedulingOptions.GroupOnGroupPage;
			}
		}

        private void initTags()
        {
            comboBoxAdvTag.DataSource = _scheduleTags;
            comboBoxAdvTag.DisplayMember = "Description";

            if (!_optimize)
                comboBoxAdvTag.SelectedItem = _localSchedulingOptions.TagToUseOnScheduling;
            else
                comboBoxAdvTag.SelectedItem = _localSchedulingOptions.TagToUseOnOptimize;
            
        }

		private void InitGroupPagesFairness()
		{
			comboBoxGroupingFairness.DataSource = _groupPagesFairness;
			comboBoxGroupingFairness.DisplayMember = "Description";

			if (_localSchedulingOptions.GroupPageForShiftCategoryFairness != null)
			{
				comboBoxGroupingFairness.SelectedItem = _localSchedulingOptions.GroupPageForShiftCategoryFairness;
			}
		}

        private void DataOffline()
        {
            _localSchedulingOptions = (ISchedulingOptions)_schedulingOptions.Clone();
        }

        private void DataOnline()
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
            _schedulingOptions.RescheduleOptions = _localSchedulingOptions.RescheduleOptions;
            _schedulingOptions.Fairness = _localSchedulingOptions.Fairness;
            _schedulingOptions.UseShiftCategoryLimitations = _localSchedulingOptions.UseShiftCategoryLimitations;
            _schedulingOptions.UsePreferencesMustHaveOnly = _localSchedulingOptions.UsePreferencesMustHaveOnly;
            _schedulingOptions.UseBlockScheduling = _localSchedulingOptions.UseBlockScheduling;
        	_schedulingOptions.UseGroupScheduling = _localSchedulingOptions.UseGroupScheduling;
        	_schedulingOptions.GroupOnGroupPage = _localSchedulingOptions.GroupOnGroupPage;
            _schedulingOptions.DoNotBreakMaxStaffing = _localSchedulingOptions.DoNotBreakMaxStaffing;
			_schedulingOptions.GroupPageForShiftCategoryFairness = _localSchedulingOptions.GroupPageForShiftCategoryFairness;
        	_schedulingOptions.UseMaxSeats = _localSchedulingOptions.UseMaxSeats;
        	_schedulingOptions.DoNotBreakMaxSeats = _localSchedulingOptions.DoNotBreakMaxSeats;
            _schedulingOptions.UseGroupOptimizing = _localSchedulingOptions.UseGroupOptimizing;
            _schedulingOptions.UseSameDayOffs = _localSchedulingOptions.UseSameDayOffs;
            _schedulingOptions.UseBlockOptimizing = _localSchedulingOptions.UseBlockScheduling;
            _schedulingOptions.TagToUseOnScheduling = _localSchedulingOptions.TagToUseOnScheduling;
            _schedulingOptions.TagToUseOnOptimize = _localSchedulingOptions.TagToUseOnOptimize;
        	_schedulingOptions.ResourceCalculateFrequency = _localSchedulingOptions.ResourceCalculateFrequency;
			_schedulingOptions.ShowTroubleshot = _localSchedulingOptions.ShowTroubleshot;
        }

        private void GetDataFromControls()
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
            _localSchedulingOptions.Fairness = new Percent(trackBar1.Value / 100d);
            _localSchedulingOptions.UseShiftCategoryLimitations = checkBoxUseShiftCategoryRestrictions.Checked;
            if(!_optimize)
			    _localSchedulingOptions.UseGroupScheduling = checkBoxUseGroupScheduling.Checked;
        	_localSchedulingOptions.GroupOnGroupPage = (IGroupPage)comboBoxGrouping.SelectedItem;
			_localSchedulingOptions.GroupPageForShiftCategoryFairness = (IGroupPage)comboBoxGroupingFairness.SelectedItem;
			_localSchedulingOptions.DoNotBreakMaxStaffing = checkBoxDoNotBreakMaxSeats.Checked;
        	_localSchedulingOptions.UseMaxSeats = checkBoxUseMaxSeats.Checked;
        	_localSchedulingOptions.DoNotBreakMaxSeats = checkBoxDoNotBreakMaxSeats.Checked;
            
            if (_optimize)
            {
                _localSchedulingOptions.UseGroupOptimizing = checkBoxUseGroupScheduling.Checked;
                _localSchedulingOptions.TagToUseOnOptimize = (IScheduleTag)comboBoxAdvTag.SelectedItem;
            }
            else
            {
                _localSchedulingOptions.TagToUseOnScheduling = (IScheduleTag)comboBoxAdvTag.SelectedItem;
            }
            _localSchedulingOptions.UseSameDayOffs = !checkBoxUseGroupScheduling.Checked ? checkBoxUseGroupScheduling.Checked : checkBoxUseSameDayOffs.Checked;
        	_localSchedulingOptions.ResourceCalculateFrequency = (int)numericUpDownResourceCalculateEvery.Value;
			_localSchedulingOptions.ShowTroubleshot = checkBoxShowTroubleShot.Checked;

        }

        private void SetDataInControls()
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

            if (MustHaveSetAndOnlyPreferenceDaysVisible())
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

            if(!_optimize)
        	    checkBoxUseGroupScheduling.Checked = _localSchedulingOptions.UseGroupScheduling;
        	comboBoxGrouping.Enabled = checkBoxUseGroupScheduling.Checked;
			checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxStaffing;
        	checkBoxUseMaxSeats.Checked = _localSchedulingOptions.UseMaxSeats;
        	checkBoxDoNotBreakMaxSeats.Enabled = checkBoxUseMaxSeats.Checked;
        	checkBoxDoNotBreakMaxSeats.Checked = _localSchedulingOptions.DoNotBreakMaxSeats;
            checkBoxUseSameDayOffs.Enabled = checkBoxUseGroupScheduling.Checked;

            if (_optimize)
                checkBoxUseGroupScheduling.Checked = _localSchedulingOptions.UseGroupOptimizing;
            checkBoxUseSameDayOffs.Checked = _localSchedulingOptions.UseSameDayOffs;
        	numericUpDownResourceCalculateEvery.Value = _localSchedulingOptions.ResourceCalculateFrequency;
			checkBoxShowTroubleShot.Checked = _localSchedulingOptions.ShowTroubleshot;
        }

        private bool MustHaveSetAndOnlyPreferenceDaysVisible()
        {
            return checkBoxMustHaves.Checked && checkBoxOnlyPreferenceDays.Visible && checkBoxOnlyRotationDays.Enabled;
        }

        private void CheckBoxUseRotationsCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxOnlyRotationDaysCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxUseAvailabilityCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }
        private void CheckBoxUseStudentAvailabilityCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxUsePreferencesCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void ComboBoxAdvShiftCategorySelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxUseShiftCategoryCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseShiftCategory.Checked)
                checkBoxUseBlockScheduling.Checked = false;
            checkBoxUseBlockScheduling.Enabled = !checkBoxUseShiftCategory.Checked;
            comboBoxAdvShiftCategory.Enabled = checkBoxUseShiftCategory.Checked;
        }

        private void TrackBar1ValueChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
            }
        }

       

        private void CheckBoxOnlyAvailabilityDaysCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxOnlyPreferenceDaysCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxUseShiftCategoryRestrictionsCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxMustHavesCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

        private void CheckBoxUseBlockSchedulingCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseBlockScheduling.Checked)
            {
                checkBoxUseShiftCategory.Checked = false;
                radioButtonBetweenDayOff.Checked = true;
            }

            checkBoxUseShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            comboBoxAdvShiftCategory.Enabled = !checkBoxUseBlockScheduling.Checked;
            radioButtonSchedulePeriod.Enabled = checkBoxUseBlockScheduling.Checked;
            radioButtonBetweenDayOff.Enabled = checkBoxUseBlockScheduling.Checked;
			checkBoxUseGroupScheduling.Enabled = !checkBoxUseBlockScheduling.Checked;
			if (checkBoxUseGroupScheduling.Checked && checkBoxUseBlockScheduling.Checked)
				checkBoxUseGroupScheduling.Checked = false;
        }

        private void RadioButtonBetweenDayOffCheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonBetweenDayOff.Checked)
                radioButtonSchedulePeriod.Checked = false;
        }

        private void RadioButtonSchedulePeriodCheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonSchedulePeriod.Checked)
                radioButtonBetweenDayOff.Checked = false;
        }

		private void CheckBoxUseGroupSchedulingCheckedChanged(object sender, EventArgs e)
		{
			comboBoxGrouping.Enabled = checkBoxUseGroupScheduling.Checked;

            if (_optimize)
            {
                checkBoxUseSameDayOffs.Enabled = checkBoxUseGroupScheduling.Checked;

                if (!checkBoxUseGroupScheduling.Checked)
                {
                    checkBoxUseSameDayOffs.Checked = false;
                    checkBoxUseSameDayOffs.Enabled = false;
                }
                else
                {
                    checkBoxUseSameDayOffs.Checked = true;
                    checkBoxUseSameDayOffs.Enabled = true;
                }
            }
            else
            {
                checkBoxUseBlockScheduling.Enabled = !checkBoxUseGroupScheduling.Checked;
                if (checkBoxUseGroupScheduling.Checked && checkBoxUseBlockScheduling.Checked)
                    checkBoxUseBlockScheduling.Checked = false;   
            }
		}

        private void CheckBoxUseSameDayOffsCheckedChanged(object sender, EventArgs e)
        {
            if (_dataLoaded)
            {
                GetDataFromControls();
                SetDataInControls();
            }
        }

		private void ComboBoxGroupingSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_dataLoaded)
			{
				GetDataFromControls();
				SetDataInControls();
			}
		}


		private void ComboBoxGroupingFairnessSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_dataLoaded)
			{
				GetDataFromControls();
				SetDataInControls();
			}
		}

		private void CheckBoxUseMaxSeatsCheckedChanged(object sender, EventArgs e)
		{
			if (!checkBoxUseMaxSeats.Checked)
				checkBoxDoNotBreakMaxSeats.Checked = false;
			checkBoxDoNotBreakMaxSeats.Enabled = checkBoxUseMaxSeats.Checked;
		}
    }
}
