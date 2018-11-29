using System;
using System.Collections.Generic;
using System.Globalization;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
    /// <summary>
    /// User control to manage Weekly recurrence patterns.
    /// </summary>
    public partial class WeeklyRecurrenceView : BaseUserControl
    {
        private readonly RecurrentWeeklyMeetingViewModel _model;
        private const string CheckBoxAdvDay = "checkBoxAdvDay";

        /// <summary>
        /// Initializes a new instance of the <see cref="WeeklyRecurrenceView"/> class.
        /// </summary>
        protected WeeklyRecurrenceView()
        {
            InitializeComponent();
            if (DesignMode) return;

            SetTexts();
        }

        public WeeklyRecurrenceView(RecurrentMeetingOptionViewModel model) : this()
        {
            _model = (RecurrentWeeklyMeetingViewModel)model;
            InitializeCheckBoxes();
            SetIncrementCount(_model.IncrementCount);
        }

        /// <summary>
        /// Binds the check boxes.
        /// </summary>
        private void InitializeCheckBoxes()
        {
            IList<DayOfWeek> daysCollection = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);

            for (int i = 1; i <= daysCollection.Count; i++)
            {
                CheckBoxAdv checkBoxAdv = Controls[0].Controls[string.Format(CultureInfo.InvariantCulture, "{0}{1}", CheckBoxAdvDay, i)] as CheckBoxAdv;
                if (checkBoxAdv != null)
                {
                    DayOfWeek dayOfWeek = daysCollection[i - 1];
                    checkBoxAdv.Text = LanguageResourceHelper.TranslateEnumValue( dayOfWeek);
                    checkBoxAdv.Tag = dayOfWeek;
                    checkBoxAdv.Checked = _model[dayOfWeek];
                }
            }
        }

        /// <summary>
        /// Sets the control values.
        /// </summary>
        public void SetIncrementCount(int recurrenceFrequency)
        {
            integerTextBoxExtRecurrenceFrequency.IntegerValue = recurrenceFrequency;
        }

        private void checkBoxAdvDay_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBoxAdv checkBoxAdv = sender as CheckBoxAdv;
            if (checkBoxAdv==null) return;

            DayOfWeek dayOfWeek = (DayOfWeek) checkBoxAdv.Tag;
            _model[dayOfWeek] = checkBoxAdv.Checked;
        }

        private void integerTextBoxExtRecurrenceFrequency_IntegerValueChanged(object sender, EventArgs e)
        {
            _model.IncrementCount = (int) integerTextBoxExtRecurrenceFrequency.IntegerValue;
        }
    }
}
