using System;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class OptimizeActivities : BaseRibbonForm, IOptimizerActivitiesPreferencesView
    {
        OptimizerActivitiesPreferencesPresenter _presenter;
 
        public OptimizeActivities(IOptimizerActivitiesPreferences model, int resolution)
        {
            InitializeComponent();
            
            _presenter = new OptimizerActivitiesPreferencesPresenter(model, this, resolution);
            _presenter.Initialize();
      
        }

        public void Initialize(int resolution, IList<IActivity> allActivities, IList<IActivity> selectedActivities)
        {
            SetTexts();

            fromToTimePicker1.StartTime.DefaultResolution = resolution;
            fromToTimePicker1.EndTime.DefaultResolution = resolution;

            fromToTimePicker1.StartTime.TimeIntervalInDropDown = resolution;
            fromToTimePicker1.EndTime.TimeIntervalInDropDown = resolution;

            TimeSpan start = TimeSpan.Zero;
            TimeSpan end = start.Add(TimeSpan.FromDays(1));

            fromToTimePicker1.StartTime.CreateAndBindList(start, end);
            fromToTimePicker1.EndTime.CreateAndBindList(start, end);

            fromToTimePicker1.StartTime.SetTimeValue(start);
            fromToTimePicker1.EndTime.SetTimeValue(end);

            fromToTimePicker1.WholeDay.Visible = false;

            twoListSelectorActivities.Initiate(allActivities, selectedActivities, "Description", UserTexts.Resources.Activities, UserTexts.Resources.DoNotMove);
        }

        public void KeepShiftCategory(bool keep)
        {
            checkBoxShiftCategory.Checked = keep;
        }

        //public void SetColor()
        //{
        //    this.BackColor = ColorHelper.FormBackgroundColor();
        //}

        public void KeepStartTime(bool keep)
        {
            checkBoxStartTime.Checked = keep;
        }

        public void KeepEndTime(bool keep)
        {
            checkBoxEndTime.Checked = keep;
        }

        public void KeepBetween(TimePeriod? dateTimePeriod)
        {
            if (dateTimePeriod.HasValue)
            {
                checkBoxBetween.Checked = true;

                TimeSpan start = dateTimePeriod.Value.StartTime;
                TimeSpan end = dateTimePeriod.Value.EndTime;

                fromToTimePicker1.StartTime.SetTimeValue(start);
                fromToTimePicker1.EndTime.SetTimeValue(end);
            }
            else
            {
                checkBoxBetween.Checked = false;
            }
        }

        public void HideForm()
        {
            this.Hide();
        }

        public IList<IActivity> DoNotMoveActivities()
        {
            return twoListSelectorActivities.GetSelected<IActivity>();
        }

        private void checkBoxShiftCategory_CheckedChanged(object sender, EventArgs e)
        {
            _presenter.OnKeepShiftCategoryCheckedChanged(checkBoxShiftCategory.Checked);
        }

        private void checkBoxStartTime_CheckedChanged(object sender, EventArgs e)
        {
            _presenter.OnKeepStartTimeCheckedChanged(checkBoxStartTime.Checked);
        }

        private void checkBoxEndTime_CheckedChanged(object sender, EventArgs e)
        {
            _presenter.OnKeepEndTimeCheckedChanged(checkBoxEndTime.Checked);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _presenter.OnButtonOkClick(checkBoxBetween.Checked, fromToTimePicker1.StartTime.TimeValue(), fromToTimePicker1.EndTime.TimeValue());
        }
        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _presenter.OnButtonCancelClick();
        }

        public bool IsCanceled()
        {
            return _presenter.IsCanceled;
        }

		/// <summary>
		/// bugfix for 19200: System crashed when closing the "Optimize Activity" dialog.
		/// Focus from fromToTimePicker1 must be taken away
		/// </summary>
		private void OptimizeActivities_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			twoListSelectorActivities.Select();
		}
    }
}
