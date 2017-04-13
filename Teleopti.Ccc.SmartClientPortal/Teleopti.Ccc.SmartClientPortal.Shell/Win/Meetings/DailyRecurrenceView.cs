using System;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.WinCode.Meetings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
    /// <summary>
    /// User control to manage monthly recurrence pattern.
    /// </summary>
    public partial class DailyRecurrenceView : BaseUserControl
    {
        private readonly RecurrentMeetingOptionViewModel _meetingOptionViewModel;

        protected DailyRecurrenceView()
        {
            InitializeComponent();
            if (DesignMode) return;
            SetTexts();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrenceView"/> class.
        /// </summary>
        public DailyRecurrenceView(RecurrentMeetingOptionViewModel meetingOptionViewModel)
            : this()
        {
            _meetingOptionViewModel = meetingOptionViewModel;
            SetDailyMeetingModel(meetingOptionViewModel as RecurrentDailyMeetingViewModel);
        }

        private void SetDailyMeetingModel(RecurrentDailyMeetingViewModel dailyMeetingViewModel)
        {
            if (dailyMeetingViewModel==null) return;

            integerTextBoxIncrementCount.IntegerValue = dailyMeetingViewModel.IncrementCount;
        }

        private void integerTextBoxIncrementCount_IntegerValueChanged(object sender, EventArgs e)
        {
            _meetingOptionViewModel.IncrementCount = (int) integerTextBoxIncrementCount.IntegerValue;
        }
    }
}
