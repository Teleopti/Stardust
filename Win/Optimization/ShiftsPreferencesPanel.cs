using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Win.Common;

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	
    public partial class ShiftsPreferencesPanel : BaseUserControl, IDataExchange
    {

        public IShiftPreferences Preferences { get; private set; }
        private IList<IActivity> _availableActivity;
        private int _resolution;

        public ShiftsPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IShiftPreferences extraPreferences, IList<IActivity > availableActivity , int resolution)
		{

		    _availableActivity = availableActivity;
		    _resolution = resolution;
            Preferences = extraPreferences;
			ExchangeData(ExchangeDataOption.DataSourceToControls);
		    SetInitialValues();
        }

        private void SetInitialValues()
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

            fromToTimePicker1.WholeDay.Visible = false;

            IList<IActivity> activities = new List<IActivity>();
            if (Preferences.DoNotMoveActivitiesGuids   != null)
            {
                foreach (Guid id in Preferences.DoNotMoveActivitiesGuids
                    )
                {
                    foreach (IActivity activity in _availableActivity)
                    {
                        if (activity.Id.Value == id)
                            activities.Add(activity);
                    }
                }
            }

            twoListSelectorActivities.Initiate(_availableActivity, activities, "Description", UserTexts.Resources.Activities, UserTexts.Resources.DoNotMove);
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
            Preferences.KeepStartTimes = checkBoxKeepEndTimes.Checked;
            Preferences.KeepEndTimes = checkBoxKeepStartTimes.Checked;
            Preferences.KeepShifts = checkBoxKeepShifts.Checked;
            Preferences.AlterBetween = checkBoxBetween.Checked ;
            Preferences.KeepShiftsValue = (double)numericUpDownKeepShifts.Value  / 100;

            IList<Guid> guidList = new List<Guid>();

            foreach (IActivity activity in DoNotMoveActivities())
            {
                guidList.Add(activity.Id.Value);
            }
            Preferences.DoNotMoveActivitiesGuids = guidList;
        }

        private void setDataToControls()
        {
            


            checkBoxKeepShiftCategories.Checked = Preferences.KeepShiftCategories;
            checkBoxKeepEndTimes.Checked = Preferences.KeepEndTimes;
            checkBoxKeepStartTimes.Checked = Preferences.KeepStartTimes;
            checkBoxKeepShifts.Checked = Preferences.KeepShifts;
            checkBoxBetween.Checked = Preferences.AlterBetween;
            numericUpDownKeepShifts.Value = (decimal) Preferences.KeepShiftsValue * 100;
        }

        public IList<IActivity> DoNotMoveActivities()
        {
            return twoListSelectorActivities.GetSelected<IActivity>();
        }

    }



}
