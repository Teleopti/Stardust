#region imports

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

#endregion

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    /// <summary>
    /// Represent the Date & Time selection control like outlook
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-09-24
    /// </remarks>
    public partial class OutlookDateTimePicker : UserControl
    {

        #region Fileds - Instance Members

        private OutlookTimePicker _time;
        private DateTime _selectedDateTime;
        private DateTimePickerAdv _dateTimePickerAdv;
        private DateTime _previousDateTime;


        #endregion

        #region Properties - Instance Members

        /// <summary>
        /// Gets or sets the time interval in drop down.
        /// </summary>
        /// <value>The time interval in drop down.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public int TimeIntervalInDropDown
        {
            get { return _time.TimeIntervalInDropDown; }
            set { _time.TimeIntervalInDropDown = value; }
        }

        /// <summary>
        /// Gets or sets the selected date time.
        /// </summary>
        /// <value>The selected date time.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public DateTime SelectedDateTime
        {
            get { return GetSelectedDateTime(); }
            set { SetDateTime(value); }
        }

        /// <summary>
        /// Gets the date time picker adv date.
        /// </summary>
        /// <value>The date time picker adv date.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-12-17
        /// </remarks>
        public DateTimePickerAdv DateTimePickerAdvDate 
        { 
            get
            {
                return _dateTimePickerAdv;
            }
        }

        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string TodayButtonText
        {
            get { return this.DateTimePickerAdvDate.Calendar.TodayButton.Text; }
            set
            {
                DateTimePickerAdvDate.Calendar.TodayButton.Text = value;
                DateTimePickerAdvDate.Calendar.TodayButton.Text = value;
            }
        }

        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string NoneButtonText
        {
            get { return this.DateTimePickerAdvDate.Calendar.NoneButton.Text; }
            set
            {
                DateTimePickerAdvDate.Calendar.NoneButton.Text = value;
                DateTimePickerAdvDate.Calendar.NoneButton.Text = value;
            }
        }

        /// <summary>
        /// Gets the outlook time picker.
        /// </summary>
        /// <value>The outlook time picker.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/12/2008
        /// </remarks>
        public OutlookTimePicker OutlookTimePicker
        {
            get { return _time; }
        }


        #endregion

        #region Methods  - Instance Members

        #region Methods  - Instance Members - OutlookDateTimePicker Members - (constructors)

        /// <summary>
        /// Initializes a new instance of the <see cref="OutlookDateTimePicker"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public OutlookDateTimePicker()
        {
            InitializeComponent();
            PrepareControl();
        }

        #endregion

        #region Methods  - Instance Members - OutlookDateTimePicker Members - (helpers)

        /// <summary>
        /// Prepares the control.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        private void PrepareControl()
        {
            DateTimePickerAdvDate.Style = VisualStyle.Office2007;
            DateTimePickerAdvDate.ShowCheckBox = false;

            _time = new OutlookTimePicker();
            _time.Height = 21;
            _time.Margin = new Padding(0);
            _time.SetTimeValue(DateTime.Now.TimeOfDay);
            
            tableLayoutPanelMain.Controls.Add(_time,2,0);
            _previousDateTime = GetSelectedDateTime();
        }

        public bool ReadOnly
        {
            set
            {
                _dateTimePickerAdv.ReadOnly = value;
                _time.ReadOnly = value;
            }
            get
            {
                return _dateTimePickerAdv.ReadOnly;
            }
        }



        /// <summary>
        /// Sets the date time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        private void SetDateTime(DateTime value)
        {
            _selectedDateTime = new DateTime(
                                value.Year,
                                value.Month,
                                value.Day,
                                value.Hour,
                                value.Minute,
                                value.Second);

            DateTimePickerAdvDate.Value = value;
            _time.SetTimeValue(value.TimeOfDay);

        } 

        public void InitializeDateTime(DateTime newDateTime)
        {
            _dateTimePickerAdv.ValueChanged -= _dateTimePickerAdv_ValueChanged;
            _time.TextChanged -= _time_TextChanged;
            SetDateTime(newDateTime);
            _previousDateTime = newDateTime;
            _dateTimePickerAdv.ValueChanged += new System.EventHandler(this._dateTimePickerAdv_ValueChanged);
            _time.TextChanged += new EventHandler(_time_TextChanged);
        }

        /// <summary>
        /// Gets the selected date time.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        private DateTime GetSelectedDateTime()
        {
            if(!DesignMode)
            {
                TimeSpan timeValue = _time.TimeValue().GetValueOrDefault(TimeSpan.Zero);
                _selectedDateTime = DateTimePickerAdvDate.Value.Date.AddSeconds(timeValue.TotalSeconds);
            }
            return _selectedDateTime;
        }
        
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]

        public event DateTimeValueChangedEventHandler DateTimeValueChanged;

        private void _dateTimePickerAdv_ValueChanged(object sender, EventArgs e)
        {
            if (DateTimeValueChanged != null)
            {
                DateTimeValueChanged(_previousDateTime);
                _previousDateTime = GetSelectedDateTime();
            }
        }

        void _time_TextChanged(object sender, EventArgs e)
        {
            if (_time != null)
            {
                if (DateTimeValueChanged != null)
                {
                    DateTimeValueChanged(_previousDateTime);
                    _previousDateTime = GetSelectedDateTime();
                }
            }
        }
        #endregion

        private void OutlookDateTimePicker_Load(object sender, EventArgs e)
        {
            _dateTimePickerAdv.ValueChanged += new System.EventHandler(this._dateTimePickerAdv_ValueChanged);
            _time.TextChanged += new EventHandler(_time_TextChanged);
        }


    }
    public delegate void DateTimeValueChangedEventHandler(DateTime previousDateTime);
       
}
