using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Win.Common;

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	
    public partial class ShiftsPreferencesPanel : BaseUserControl, IDataExchange
    {

        public IShiftPreferences Preferences { get; private set; }
        private IOptimizerActivitiesPreferences _model;
        private int _resolution;

        public ShiftsPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IShiftPreferences extraPreferences, IOptimizerActivitiesPreferences model, int resolution)
		{
		    _model = model;
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


            twoListSelectorActivities.Initiate(_model.Activities , _model.DoNotMoveActivities , "Description", UserTexts.Resources.Activities, UserTexts.Resources.DoNotMove);
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

       

        //private void setRadioButtonsStatus()
        //{
        //    radioButtonBetweenDayOff.Enabled = checkBoxBlock.Checked;
        //    radioButtonSchedulePeriod.Enabled = checkBoxBlock.Checked;
        //}

    }



}
