using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
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

            //Whole day should not be visible anymore, shoud this be taken away?
            timePicker.WholeDay.Visible = false;

            _workload = workload;

            TimeSpan start = workload.Skill.MidnightBreakOffset;
            TimeSpan end = start.Add(TimeSpan.FromHours(24d));

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

            if (_displayPeriod.StartTime == _openHourPeriod.EndTime)
                timePicker.WholeDay.Checked = true;

            timePicker.StartTime.Select();
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
            try
            {
                if (_isClosed)
                {
                    _openHourPeriod = new TimePeriod(_workload.Skill.MidnightBreakOffset, _workload.Skill.MidnightBreakOffset);
                }
                else
                {
                    var startTime = timePicker.StartTime.TimeValue();
                    var endTime = timePicker.EndTime.TimeValue();
                    if (startTime < _workload.Skill.MidnightBreakOffset)
                    {
                        startTime = startTime.Add(TimeSpan.FromDays(1));
                    }
                    if (timePicker.WholeDay.Checked || startTime >= endTime)
                    {
                        _openHourPeriod = new TimePeriod(startTime, endTime.Add(TimeSpan.FromDays(1)));
                    }
                    else
                    {
                        _openHourPeriod = new TimePeriod(startTime, endTime);
                    }
                }

                if (!WinCode.Forecasting.OpenHourHelper.IsValidOpenHour(_openHourPeriod, _workload.Skill.MidnightBreakOffset))
                {
                    MessageBoxOptions option = new MessageBoxOptions();
                    if (RightToLeft == RightToLeft.Yes)
                    {
                        option = MessageBoxOptions.RtlReading;
                    }
                    string message = string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2}", UserTexts.Resources.NotValidOpenHoursForMidnightBreak,
                                                   _workload.Skill.Name, _workload.Skill.MidnightBreakOffset);
                    Syncfusion.Windows.Forms.MessageBoxAdv.Show(message, UserTexts.Resources.OpenHours, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, option, false);
                    DialogResult = DialogResult.None;
                }
            }
            catch (ArgumentException ex)
            {
                MessageBoxOptions option = new MessageBoxOptions();
                if (RightToLeft == RightToLeft.Yes)
                {
                    option = MessageBoxOptions.RtlReading;
                }
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(ex.Message, "  "), UserTexts.Resources.UnknownTimeFormat, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, option, false);
                DialogResult = DialogResult.Abort;
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
        }
    }
}
