using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
{
    public partial class ShiftsPreferencesPanel : BaseUserControl, IDataExchange
    {

        public ShiftPreferences Preferences { get; private set; }
        private IEnumerable<IActivity> _availableActivity;
        private int _resolution;

        public ShiftsPreferencesPanel()
        {
            InitializeComponent();
        }

		public void Initialize(
            ShiftPreferences extraPreferences, IEnumerable<IActivity> availableActivity , int resolution, bool useRightToLeft)
		{
			if (!useRightToLeft)
			{
				if (!DesignMode) SetTextsNoRightToLeft();
			}
			else
			{
				if (!DesignMode) SetTexts();
			}
			_availableActivity = availableActivity;
		    _resolution = resolution;
			comboBoxAdvActivity.DisplayMember = "Name";
			comboBoxAdvActivity.DataSource = _availableActivity;
            Preferences = extraPreferences;
		    SetDefaultTimePeriod();
            ExchangeData(ExchangeDataOption.DataSourceToControls);
		    SetInitialValues();
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
            Preferences.AlterBetween = checkBoxBetween.Checked ;
            Preferences.SelectedTimePeriod = new TimePeriod(fromToTimePicker1.StartTime.TimeValue(), fromToTimePicker1.EndTime.TimeValue());
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
            checkBoxBetween.Checked = Preferences.AlterBetween;
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

		private void checkBoxKeepActivityLength_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxAdvActivity.Enabled = checkBoxKeepActivityLength.Checked;
		}

    }


}
