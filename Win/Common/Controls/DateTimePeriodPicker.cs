using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public partial class DateTimePeriodPicker : BaseUserControlWithUnitOfWork
    {
        private DateTimePeriod _period;
        private bool _doNotRaiseEvent;
        private bool _Raising;
        private readonly bool _formIsInDesign;
        private DateTime _baseDate;

        /// <summary>
        /// Occurs when value changed. Will not occur when the Period value is set.
        /// </summary>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-10    
        /// /// </remarks>
        public event EventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodPicker"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-07-10
        /// /// </remarks>
        public DateTimePeriodPicker()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            DateTimeFrom.ValueChanged += DateTimeFrom_ValueChanged;
            DateTimeTo.ValueChanged += DateTimeTo_ValueChanged;
            chkOverMidnight.CheckStateChanged += chkOverMidnight_CheckStateChanged;
            
            DateTimeTo.KeyDown += CheckEnterAndSuppress;
            DateTimeFrom.KeyDown += CheckEnterAndSuppress;

            try
            {
                CultureInfo inf = StateHolderReader.Instance.StateReader.SessionScopeData.LoggedOnPerson.PermissionInformation.Culture();
                DateTimeFrom.CustomFormat = inf.DateTimeFormat.ShortTimePattern;
                DateTimeTo.CustomFormat = inf.DateTimeFormat.ShortTimePattern;
                theDate.Format = DateTimePickerFormat.Custom;
                theDate.CustomFormat = inf.DateTimeFormat.ShortDatePattern;
            }
            catch (InvalidOperationException)
            {
                // if we get this we are not runnig, the form is in design we can't use any builtin functions
                // depending on that we are up and running
                _formIsInDesign = true;
            }
            
        }

        #region Events
        
        // Suncfusion crashes after an Enter and a mouse click
        static void CheckEnterAndSuppress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        void chkOverMidnight_CheckStateChanged(object sender, EventArgs e)
        {
            if (_formIsInDesign) return;

            if (chkOverMidnight.Checked)
            {
                DateTimeTo.Value.AddDays(1);
            }
            else
            {
                DateTimeTo.Value.AddDays(-1);
            }
            CheckMinMax();

            if(_doNotRaiseEvent) return;
            RaiseValueChanged();
        }

        void DateTimeTo_ValueChanged(object sender, EventArgs e)
        {
            if (_formIsInDesign) return;

            if (_doNotRaiseEvent) return;
            
            CheckMinMax();
            SetPeriod();

            RaiseValueChanged();
        }

        void DateTimeFrom_ValueChanged(object sender, EventArgs e)
        {
            if (_formIsInDesign) return;

            if (_doNotRaiseEvent) return;
            CheckMinMax();
            SetPeriod();
            
            RaiseValueChanged();
            
        }
        #endregion


        /// <summary>
        /// Gets or sets the base date. From this date the layer can continue over midnight. If it starts next date it can not continue over midnight. 
        /// </summary>
        /// <value>The base date.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-16    
        /// /// </remarks>
        public DateTime BaseDate
        {
            get { return _baseDate; }

            set 
            {
                _baseDate = value;
                theDate.MaxValue = new DateTime(2999,1,1);
                theDate.MinValue = new DateTime();
                theDate.MinValue = _baseDate;
                theDate.MaxValue = _baseDate.AddDays(1);
            } 
        }


        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-07-14
        /// /// </remarks>
        public DateTimePeriod Period
        {
            get
            {
                return _period;
            }
            set
            {

                if (_Raising) return;

                _period = value;
                if (_formIsInDesign) return;

                theDate.Value = _period.LocalStartDateTime.Date;
                
                SetState();
                SetTootip();
                if (PeriodIsEmpty()) return;
                
                _doNotRaiseEvent = true;

                chkOverMidnight.Checked = value.LocalStartDateTime.Date < value.LocalEndDateTime.Date;
                
                DateTimeFrom.Value = value.LocalStartDateTime;

                DateTimeTo.MinValue = new DateTime();
                DateTimeTo.MaxValue = new DateTime(2111, 11, 11);
                DateTimeTo.MinValue = value.LocalEndDateTime;
                DateTimeTo.MaxValue = value.LocalEndDateTime;

                DateTimeTo.Value = value.LocalEndDateTime;

                _doNotRaiseEvent = false;
                CheckMinMax();
            }
        }
        #region Private Methods

        private void SetState()
        {
            if (PeriodIsEmpty())
            {
                theDate.Enabled = false;
                DateTimeFrom.Enabled = false;
                DateTimeTo.Enabled = false;
                chkOverMidnight.Enabled = false;
                return;
            }
            theDate.Enabled = true;
            DateTimeFrom.Enabled = true;
            DateTimeTo.Enabled = true;
            chkOverMidnight.Enabled = true;
        }

        private void CheckMinMax()
        {
            // Return if setting is in progress otherwise strange things can happen here
            if(_doNotRaiseEvent) return;
            if (_formIsInDesign) return;

            //TODO GEt the interval
            if (!chkOverMidnight.Checked)
            {
                DateTimeTo.MaxValue = DateTimeFrom.Value.Date.AddHours(24);
                DateTimeTo.MinValue = DateTimeFrom.Value.AddMinutes(1);
                
            }
            else
            {
                DateTimeTo.MinValue = DateTimeFrom.Value.Date.AddDays(1);
                DateTimeTo.MaxValue = DateTimeTo.MinValue.Add(DateTimeFrom.Value.TimeOfDay);
            }
            SetPeriod();
            SetTootip();
        }
        private void SetPeriod()
        {
            _period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(DateTimeFrom.Value), TimeZoneHelper.ConvertToUtc(DateTimeTo.Value));
            
        }
        private void RaiseValueChanged()
        {
            _Raising = true;
            var args = new EventArgs();
            ValueChanged(this, args);
            _Raising = false;
        }
    private void SetTootip()
    {
        if (PeriodIsEmpty())
        {
          toolTip1.RemoveAll();
            return;
        }
        toolTip1.SetToolTip(DateTimeTo, _period.LocalEndDateTime.ToString(CultureInfo.CurrentUICulture));
        toolTip1.SetToolTip(DateTimeFrom, _period.LocalStartDateTime.ToString(CultureInfo.CurrentUICulture));

        toolTip1.SetToolTip(this, _period.LocalStartDateTime.ToString(CultureInfo.CurrentUICulture) + " - " + _period.LocalEndDateTime.ToString(CultureInfo.CurrentUICulture));

    }

        private bool PeriodIsEmpty()
        {
            return _period.StartDateTime.Date == TimeZoneHelper.ConvertToUtc(new DateTime());
        }

        #endregion


    }
}
