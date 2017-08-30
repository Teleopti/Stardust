using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
{
    public partial class ShiftsPreferencesPanel : BaseUserControl, IDataExchange
    {

        public IShiftPreferences Preferences { get; private set; }
        private IEnumerable<IActivity> _availableActivity;
        private int _resolution;

        public ShiftsPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		[RemoveMeWithToggle("hideKeepShifts45688", Toggles.ResourcePlanner_RetireKeepPercentageOfShifts_45688)]
		public void Initialize(
            IShiftPreferences extraPreferences, IEnumerable<IActivity> availableActivity , int resolution, bool hideKeepShifts45688)
		{
		    _availableActivity = availableActivity;
		    _resolution = resolution;
			comboBoxAdvActivity.DisplayMember = "Name";
			comboBoxAdvActivity.DataSource = _availableActivity;
            Preferences = extraPreferences;
		    SetDefaultTimePeriod();
            ExchangeData(ExchangeDataOption.DataSourceToControls);
		    SetInitialValues();
			if (hideKeepShifts45688)
			{
				checkBoxKeepShifts.Checked = false;
				checkBoxKeepShifts.Visible = false;
				numericUpDownKeepShifts.Visible = false;
				label1.Visible = false;
			}
		}

        private void SetDefaultTimePeriod()
        {
            fromToTimePicker1.StartTime.DefaultResolution = _resolution;
            fromToTimePicker1.EndTime.DefaultResolution = _resolution;

            fromToTimePicker1.StartTime.TimeIntervalInDropDown = _resolution;
            fromToTimePicker1.EndTime.TimeIntervalInDropDown = _resolution;

            TimeSpan start = TimeSpan.Zero;
            TimeSpan end = start.Add(TimeSpan.FromDays(1));

            fromToTimePicker1.StartTime.CreateAndBindList(start, end);
            fromToTimePicker1.EndTime.CreateAndBindList(start, end);

            fromToTimePicker1.StartTime.SetTimeValue(start);
            fromToTimePicker1.EndTime.SetTimeValue(end);

            fromToTimePicker1.StartTime.TextChanged += startTimeTextChanged;
            fromToTimePicker1.EndTime.TextChanged += endTimeTextChanged;
        }

        private void endTimeTextChanged(object sender, EventArgs e)
        {
            var startTime = fromToTimePicker1.StartTime.TimeValue();
            var endTime = fromToTimePicker1.EndTime.TimeValue();
            if (startTime > endTime)
                fromToTimePicker1.StartTime.SetTimeValue(endTime);
        }

        private void startTimeTextChanged(object sender, EventArgs e)
        {
            var startTime = fromToTimePicker1.StartTime.TimeValue();
            var endTime = fromToTimePicker1.EndTime.TimeValue();
            if (startTime > endTime)
                fromToTimePicker1.EndTime.SetTimeValue(startTime);
        }

        private void SetInitialValues()
        {
            
            fromToTimePicker1.WholeDay.Visible = false;
	        if (comboBoxAdvActivity.SelectedItem == null)
		        comboBoxAdvActivity.SelectedIndex = 0;

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
               
                setDataToControls();
            }
            else
            {
                getDataFromControls();
            }
        }

        #endregion

        
        private void getDataFromControls()
        {
           

            Preferences.KeepShiftCategories = checkBoxKeepShiftCategories.Checked;
            Preferences.KeepStartTimes = checkBoxKeepStartTimes.Checked;
            Preferences.KeepEndTimes = checkBoxKeepEndTimes.Checked;
            Preferences.KeepShifts = checkBoxKeepShifts.Checked;
            Preferences.AlterBetween = checkBoxBetween.Checked ;
            Preferences.SelectedTimePeriod = new TimePeriod(fromToTimePicker1.StartTime.TimeValue(), fromToTimePicker1.EndTime.TimeValue());
            Preferences.KeepShiftsValue = (double) numericUpDownKeepShifts.Value/100;
            IList<IActivity> activityList = new List<IActivity>();

            foreach (IActivity activity in SelectedActivities())
            {
                activityList.Add(activity);
            }
            Preferences.SelectedActivities = activityList;
	        Preferences.KeepActivityLength = checkBoxKeepActivityLength.Checked;
	        Preferences.ActivityToKeepLengthOn = (IActivity)comboBoxAdvActivity.SelectedItem;
        }

        private void setDataToControls()
        {
            
            checkBoxKeepShiftCategories.Checked = Preferences.KeepShiftCategories;
            checkBoxKeepEndTimes.Checked = Preferences.KeepEndTimes;
            checkBoxKeepStartTimes.Checked = Preferences.KeepStartTimes;
            checkBoxKeepShifts.Checked = Preferences.KeepShifts;
            checkBoxBetween.Checked = Preferences.AlterBetween;
            numericUpDownKeepShifts.Value = (decimal) Preferences.KeepShiftsValue * 100;
            fromToTimePicker1.StartTime .SetTimeValue(Preferences.SelectedTimePeriod.StartTime );
            fromToTimePicker1.EndTime .SetTimeValue(Preferences.SelectedTimePeriod.EndTime );
	        checkBoxKeepActivityLength.Checked = Preferences.KeepActivityLength;
			if(Preferences.ActivityToKeepLengthOn != null)
				comboBoxAdvActivity.SelectedItem = Preferences.ActivityToKeepLengthOn;

            IList<IActivity> activities = new List<IActivity>();
            if (Preferences.SelectedActivities != null)
            {
                foreach (IActivity selectedActivity in Preferences.SelectedActivities
                    )
                {
                    foreach (IActivity activity in _availableActivity)
                    {
                        if (activity.Id.Value == selectedActivity.Id.Value)
                            activities.Add(activity);
                    }
                }
                
            }
            else
            {
                Preferences.SelectedActivities = new List<IActivity>();
            }


            IList<IActivity> availableactivities = new List<IActivity>();
            foreach(var currActivity in _availableActivity  )
            {
                if(!Preferences.SelectedActivities.Contains( currActivity ))
                {
                    availableactivities.Add(currActivity);
                }
            }

            twoListSelectorActivities.Initiate(availableactivities, Preferences.SelectedActivities, "Description", UserTexts.Resources.Activities, UserTexts.Resources.DoNotMove);
        }

        public IList<IActivity> SelectedActivities()
        {
            return twoListSelectorActivities.GetSelected<IActivity>();
        }

        private void setNumericUpDownKeepShiftsStatus()
        {
            numericUpDownKeepShifts.Enabled = checkBoxKeepShifts.Checked;
        }

        private void checkBoxKeepShifts_CheckedChanged(object sender, EventArgs e)
        {
            setNumericUpDownKeepShiftsStatus();
        }

		private void checkBoxKeepActivityLength_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxAdvActivity.Enabled = checkBoxKeepActivityLength.Checked;
		}

    }


}
