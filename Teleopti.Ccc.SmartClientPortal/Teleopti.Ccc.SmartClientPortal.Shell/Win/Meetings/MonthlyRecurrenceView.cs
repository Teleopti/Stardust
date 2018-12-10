using System;
using System.Collections.Generic;
using System.Globalization;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
    /// <summary>
    /// User control to manage monthly recurrence pattern.
    /// </summary>
    public partial class MonthlyRecurrenceView : BaseUserControl
    {
        private RecurrentMonthlyByDayMeetingViewModel _monthlyByDayMeetingViewModel;
        private RecurrentMonthlyByWeekMeetingViewModel _monthlyByWeekMeetingViewModel;
        public event EventHandler<CustomEventArgs<RecurrentMeetingType>> OtherRecurrentMeetingTypeRequested;

        protected MonthlyRecurrenceView()
        {
            InitializeComponent();
            if (DesignMode) return;
            SetTexts();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrenceView"/> class.
        /// </summary>
        public MonthlyRecurrenceView(RecurrentMeetingOptionViewModel meetingOptionViewModel)
            : this()
        {
            BindFrequencyDropDown();
            BindDayOfRecurrenceDropDown();
            
            SetMonthlyByDay(meetingOptionViewModel as RecurrentMonthlyByDayMeetingViewModel);
            SetMonthlyByWeek(meetingOptionViewModel as RecurrentMonthlyByWeekMeetingViewModel);
        }

        private void SetMonthlyByDay(RecurrentMonthlyByDayMeetingViewModel monthlyByDayMeetingViewModel)
        {
            _monthlyByDayMeetingViewModel = monthlyByDayMeetingViewModel;
            bool enable = _monthlyByDayMeetingViewModel != null;

            integerTextBoxEveryMonths1.Enabled = enable;
            integerTextBoxDayOfMonth.Enabled = enable;
            comboDropDownDayOfRecurrence.Enabled = !enable;
            comboDropDownFrequency.Enabled = !enable;
            integerTextBoxEveryMonths2.Enabled = !enable;

            if (!enable) return;

            radioButtonAdvMonthlyByDay.Checked = true;
            integerTextBoxDayOfMonth.IntegerValue = _monthlyByDayMeetingViewModel.DayInMonth;
            integerTextBoxEveryMonths1.IntegerValue = _monthlyByDayMeetingViewModel.IncrementCount;
        }

        private void SetMonthlyByWeek(RecurrentMonthlyByWeekMeetingViewModel monthlyByWeekMeetingViewModel)
        {
            _monthlyByWeekMeetingViewModel = monthlyByWeekMeetingViewModel;
            bool enable = _monthlyByWeekMeetingViewModel != null;

            integerTextBoxEveryMonths1.Enabled = !enable;
            integerTextBoxDayOfMonth.Enabled = !enable;
            comboDropDownDayOfRecurrence.Enabled = enable;
            comboDropDownFrequency.Enabled = enable;
            integerTextBoxEveryMonths2.Enabled = enable;

            if (!enable) return;

            radioButtonAdvMonthlyByWeek.Checked = true;
            comboDropDownDayOfRecurrence.SelectedItem = _monthlyByWeekMeetingViewModel.DayOfWeek;
            comboDropDownFrequency.SelectedItem = _monthlyByWeekMeetingViewModel.WeekOfMonth;
            integerTextBoxEveryMonths2.IntegerValue = _monthlyByWeekMeetingViewModel.IncrementCount;

            comboDropDownFrequency.SelectedValue = monthlyByWeekMeetingViewModel.WeekOfMonth;
            comboDropDownDayOfRecurrence.SelectedValue = monthlyByWeekMeetingViewModel.DayOfWeek;
        }

        /// <summary>
        /// Binds the comboDropDownFrequency drop down.
        /// </summary>
        private void BindFrequencyDropDown()
        {
            IList<KeyValuePair<WeekNumber, string>> bindableList =
                LanguageResourceHelper.TranslateEnumToList<WeekNumber>();

            comboDropDownFrequency.DisplayMember = "Value";
            comboDropDownFrequency.ValueMember = "Key";
            comboDropDownFrequency.DataSource = bindableList;
        }

        /// <summary>
        /// Binds the comboDropDownDayOfRecurrence drop down.
        /// </summary>
        private void BindDayOfRecurrenceDropDown()
        {
            IList<DayOfWeek> daysCollection = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);

            IList<KeyValuePair<DayOfWeek, string>> bindableList = new List<KeyValuePair<DayOfWeek, string>>();

            foreach (DayOfWeek dayOfWeek in daysCollection)
            {
                bindableList.Add( new KeyValuePair<DayOfWeek, string>(dayOfWeek , LanguageResourceHelper.TranslateEnumValue(dayOfWeek)) );
            }

            comboDropDownDayOfRecurrence.DisplayMember = "Value";
            comboDropDownDayOfRecurrence.ValueMember = "Key";
            comboDropDownDayOfRecurrence.DataSource = bindableList;
        }

        private void radioButtonAdvMonthly_CheckChanged(object sender, EventArgs e)
        {
            RadioButtonAdv radioButtonAdv = sender as RadioButtonAdv;
            if (radioButtonAdv==null) return;
            if (!radioButtonAdv.Checked) return;

        	var handler = OtherRecurrentMeetingTypeRequested;
            if (handler!=null)
            {
                handler.Invoke(this,new CustomEventArgs<RecurrentMeetingType>((RecurrentMeetingType)radioButtonAdv.CheckedInt));
            }
        }

        private void integerTextBoxDayOfMonth_IntegerValueChanged(object sender, EventArgs e)
        {
            if (_monthlyByDayMeetingViewModel==null) return;
            _monthlyByDayMeetingViewModel.DayInMonth = (int) integerTextBoxDayOfMonth.IntegerValue;
        }

        private void integerTextBoxEveryMonths1_IntegerValueChanged(object sender, EventArgs e)
        {
            if (_monthlyByDayMeetingViewModel == null) return;
            _monthlyByDayMeetingViewModel.IncrementCount = (int) integerTextBoxEveryMonths1.IntegerValue;
        }

        private void comboDropDownDayOfRecurrence_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_monthlyByWeekMeetingViewModel == null) return;
            _monthlyByWeekMeetingViewModel.DayOfWeek = (DayOfWeek) comboDropDownDayOfRecurrence.SelectedValue;
        }

        private void integerTextBoxEveryMonths2_IntegerValueChanged(object sender, EventArgs e)
        {
            if (_monthlyByWeekMeetingViewModel == null) return;
            _monthlyByWeekMeetingViewModel.IncrementCount = (int) integerTextBoxEveryMonths2.IntegerValue;
        }

        private void comboDropDownFrequency_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_monthlyByWeekMeetingViewModel==null) return;
            _monthlyByWeekMeetingViewModel.WeekOfMonth = (WeekNumber) comboDropDownFrequency.SelectedValue;
        }
    }
}
