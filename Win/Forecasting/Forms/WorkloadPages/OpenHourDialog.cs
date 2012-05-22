﻿using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages
{
    public partial class OpenHourDialog : BaseRibbonForm
    {
        private TimePeriod _openHourPeriod;
        private bool _isClosed;
        private TimePeriod _displayPeriod;
        private readonly IWorkload _workload;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenHourDialog"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-20
        /// </remarks>
        public OpenHourDialog()
        {

        }
        public OpenHourDialog(TimePeriod openHourPeriod, IWorkload workload, bool isClosed)
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            SetColor();

            _workload = workload;

            TimeSpan start = workload.Skill.MidnightBreakOffset;
            TimeSpan end = start.Add(TimeSpan.FromHours(24d));

            timePicker.MinMaxStartTime = new MinMax<TimeSpan>(start, end);
            timePicker.MinMaxEndTime = new MinMax<TimeSpan>(start, end);
            timePicker.StartTime.CreateAndBindList(start, end);
            timePicker.EndTime.CreateAndBindList(start, end);

            int defaultResolution = _workload.Skill.DefaultResolution;
            timePicker.StartTime.TimeIntervalInDropDown = defaultResolution;
            timePicker.EndTime.TimeIntervalInDropDown = defaultResolution;

            _openHourPeriod = openHourPeriod;
            if (!isClosed)
            {
                _displayPeriod = new TimePeriod(_openHourPeriod.StartTime, _openHourPeriod.EndTime);
            }
            else
            {
                _displayPeriod = new TimePeriod(
                    TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(8), workload.Skill.DefaultResolution),
                    TimeHelper.FitToDefaultResolution(TimeSpan.FromHours(17), workload.Skill.DefaultResolution)); //Must to this to enable users with higher resolution than one hour
            }

            timePicker.StartTime.Select();

            timePicker.StartTime.TextChanged += startTimeTextChanged;
            timePicker.EndTime.TextChanged += endTimeTextChanged;
        }

        private void endTimeTextChanged(object sender, EventArgs e)
        {
            checkIfInputTimePeriodIsValid();
        }

        private void startTimeTextChanged(object sender, EventArgs e)
        {
            checkIfInputTimePeriodIsValid();
        }

        private void SetColor()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            groupBoxOpenHour.BackColor = ColorHelper.WizardBackgroundColor();
        }


        public TimePeriod OpenHourPeriod
        {
            get
            {
                return _openHourPeriod;
            }
        }

        public bool IsOpenHoursClosed
        {
            get { return _isClosed; }
            set{chbClose.Checked = value;}
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_isClosed)
            {
                _openHourPeriod = new TimePeriod(_workload.Skill.MidnightBreakOffset,
                                                 _workload.Skill.MidnightBreakOffset);
                DialogResult = DialogResult.OK;
                return;
            }
            if (checkIfInputTimePeriodIsValid())
                DialogResult = DialogResult.OK;
        }

        private bool checkIfInputTimePeriodIsValid()
        {
            try
            {
                var startTime = timePicker.StartTime.TimeValue();
                var endTime = timePicker.EndTime.TimeValue();
                if(startTime >= endTime)
                {
                    errorProvider1.SetError(chbClose,
                                            string.Format(TeleoptiPrincipal.Current.Regional.UICulture, UserTexts.Resources.StartTimeShouldBeEarlierThanEndTimeDot,
                                                          DateTime.MinValue.Add(startTime).ToShortTimeStringWithDays(),
                                                          DateTime.MinValue.Add(endTime).ToShortTimeStringWithDays()));
                    return false;
                }
                _openHourPeriod = new TimePeriod(startTime, endTime);
                errorProvider1.Clear();
                return true;
            }
            catch (ArgumentException ex)
            {
                errorProvider1.SetError(chbClose, ex.Message);
                return false;
            }
        }

        private void OpenHourDialog_Load(object sender, EventArgs e)
        {
            timePicker.StartTime.DefaultResolution = _workload.Skill.DefaultResolution;
            timePicker.EndTime.DefaultResolution = _workload.Skill.DefaultResolution;
            timePicker.StartTime.SetTimeValue(_displayPeriod.StartTime);
            timePicker.EndTime.SetTimeValue(_displayPeriod.EndTime);
        }

        private void chbClose_CheckedChanged(object sender, EventArgs e)
        {
            _isClosed = ((CheckBox)sender).Checked;
            timePicker.Enabled = !_isClosed;
            if (_isClosed) errorProvider1.Clear();
            else checkIfInputTimePeriodIsValid();
        }
    }
}
