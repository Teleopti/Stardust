using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    /// <summary>
    /// a usercontrol showing two timeperiods and having events that handles updatebutton clicks and timepickerchanges
    /// observe: use the events from either the dropdowns or the buttons to avoid confusion
    /// </summary>
    /// <remarks>
    /// Created by: östenp
    /// Created date: 2008-03-31
    /// </remarks>
    public partial class DateSelectionFromTo : IDateSelectionControl
    {
        /// <summary>
        /// trigged by the prognosis startdate dropdown.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        /// <summary>
        /// Occurs when [value changed and contains no data - use this if you ony want to know something changes].
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-07-04
        /// </remarks>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Occurs when [button clicked - does no datevalidation].
        /// needs refactoring
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-07-04
        /// </remarks>
        public event EventHandler ButtonClickedNoValidation;

        public DateSelectionFromTo()
        {
            InitializeComponent();
            
            DateOnlyPeriod minPeriod = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
                                                          new DateOnly(DateHelper.MaxSmallDateTime));
            dateTimePickerAdvWorkAStartDate.SetAvailableTimeSpan(minPeriod);
            dateTimePickerAdvWorkEndPeriod.SetAvailableTimeSpan(minPeriod);

            dateTimePickerAdvWorkAStartDate.Value = DateTime.Today;
            dateTimePickerAdvWorkEndPeriod.Value = DateTime.Today;

            if (!DesignMode) SetTexts();

            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetCulture(CultureInfo cultureInfo)
        {
			  //dateTimePickerAdvWorkAStartDate.SetCultureInfoSafe(cultureInfo);
			  //dateTimePickerAdvWorkEndPeriod.SetCultureInfoSafe(cultureInfo);
        }

        public bool HideNoneButtons
        {
            get
            {
                return !dateTimePickerAdvWorkEndPeriod.NoneButtonVisible &&
                       !dateTimePickerAdvWorkAStartDate.NoneButtonVisible;
            }
            set
            {
                dateTimePickerAdvWorkAStartDate.NoneButtonVisible = !value;
                dateTimePickerAdvWorkEndPeriod.NoneButtonVisible = !value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is work period valid.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is work period valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public bool IsWorkPeriodValid
        {
            get { return (WorkPeriodStart <= WorkPeriodEnd); }
        }

        /// <summary>
        /// Gets or sets the work period start.
        /// </summary>
        /// <value>The work period start.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public DateOnly WorkPeriodStart
        {
            get { return new DateOnly(dateTimePickerAdvWorkAStartDate.Value.Date); }
            set
            {
                dateTimePickerAdvWorkAStartDate.Value = value.Date;
                dateTimePickerAdvWorkAStartDate.RefreshFields();
            }
        }

        /// <summary>
        /// Gets or sets the work period end.
        /// </summary>
        /// <value>The work period end.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public DateOnly WorkPeriodEnd
        {
            get { return new DateOnly(dateTimePickerAdvWorkEndPeriod.Value.Date); }
            set { dateTimePickerAdvWorkEndPeriod.Value = value.Date; }
        }

        /// <summary>
        /// Gets or sets the label date selection text.
        /// </summary>
        /// <value>The label date selection text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string LabelDateSelectionText
        {
            get { return labelTargetPeriod.Text; }
            set { labelTargetPeriod.Text = value; }
        }

        /// <summary>
        /// Gets or sets the label date selection to text.
        /// </summary>
        /// <value>The label date selection to text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string LabelDateSelectionToText
        {
            get { return labelTargetPeriodTo.Text; }
            set { labelTargetPeriodTo.Text = value; }
        }

        /// <summary>
        /// Gets or sets the button apply text.
        /// </summary>
        /// <value>The button apply text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string ButtonApplyText
        {
            get { return btnApplyChangedPeriod.Text; }
            set { btnApplyChangedPeriod.Text = value; }
        }

        /// <summary>
        /// Gets or sets the null string.
        /// </summary>
        /// <value>The null string.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string NullString
        {
            get { return dateTimePickerAdvWorkAStartDate.NullString; }
            set
            {
                dateTimePickerAdvWorkAStartDate.NullString = value;
                dateTimePickerAdvWorkEndPeriod.NullString = value;
            }
        }

        /// <summary>
        /// Gets or sets the today button text.
        /// </summary>
        /// <value>The today button text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string TodayButtonText
        {
            get { return dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text; }
            set
            {
                dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text = value;
                dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the none button text.
        /// </summary>
        /// <value>The none button text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string NoneButtonText
        {
            get { return dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text; }
            set
            {
                dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text = value;
                dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Text = value;
            }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnApplyChangedPeriod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        private void btnApplyChangedPeriod_Click(object sender, EventArgs e)
        {
            TriggerWorkPeriodChanged();
        }

        private void TriggerWorkPeriodChanged()
        {
        	var dateRangeChangedHandler = DateRangeChanged;
            if (dateRangeChangedHandler != null)
            {
                DateRangeChangedEventArgs args = new DateRangeChangedEventArgs(
                    new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates()));
                dateRangeChangedHandler.Invoke(this, args);
            }

        	var buttonClickedHandler = ButtonClickedNoValidation;
            if(buttonClickedHandler != null)
            {
                buttonClickedHandler.Invoke(this,null);
            }
        }

        public void ShowWarning()
        {
            if (IsWorkPeriodValid == false)
            {
                using (TimedWarningDialog warning = new TimedWarningDialog())
                {
                    warning.WarningShownInSeconds = 2;
                    warning.WarningMessageShown = "WARNING! \n the startdate is after enddate!";
                    warning.WarningShownNearThisControl = labelTargetPeriodTo;
                    warning.ShowDialog(this);
                }
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the dateTimePickerAdvWorkEndPeriod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        private void dateTimePickerAdvWorkEndPeriod_ValueChanged(object sender, EventArgs e)
        {
            InvokeValueChanged(e);
        }

        /// <summary>
        /// Handles the ValueChanged event of the dateTimePickerAdvWorkStartDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        private void dateTimePickerAdvWorkStartDate_ValueChanged(object sender, EventArgs e)
        {
            InvokeValueChanged(e);
        }

        /// <summary>
        /// Invokes the value changed without triggering the datecheck use it if you want to be informed of changes and defer the datevalidation to later
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-07-04
        /// </remarks>
        private void InvokeValueChanged(EventArgs e)
        {
            EventHandler valueChangedHandler = ValueChanged;
            if (valueChangedHandler != null) valueChangedHandler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-15
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            dateTimePickerAdvWorkAStartDate.ValueChanged += dateTimePickerAdvWorkStartDate_ValueChanged;
            dateTimePickerAdvWorkEndPeriod.ValueChanged += dateTimePickerAdvWorkEndPeriod_ValueChanged;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show apply button].
        /// </summary>
        /// <value><c>true</c> if [show apply button]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(true), DefaultValue(true), Category("Teleopti Behavior")]
        public bool ShowApplyButton
        {
            get
            {
                return btnApplyChangedPeriod.Visible;
            }
            set
            {
                btnApplyChangedPeriod.Visible = value;
            }
        }

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(false)]
        public IList<DateOnlyPeriod> GetSelectedDates()
        {
            IList<DateOnlyPeriod> selectedDates = new List<DateOnlyPeriod>();
            if (IsWorkPeriodValid) 
            {
                selectedDates.Add(new DateOnlyPeriod(WorkPeriodStart, WorkPeriodEnd));
                return selectedDates;
            }

            //ShowWarning();
            return selectedDates;
        }

        public void SetErrorOnEndTime(string error)
        {
            _errorProvider.SetError(dateTimePickerAdvWorkEndPeriod, error);
            _errorProvider.SetIconPadding(dateTimePickerAdvWorkEndPeriod, -35);
        }
    }
}