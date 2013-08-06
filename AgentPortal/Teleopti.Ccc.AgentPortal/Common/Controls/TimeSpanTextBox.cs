using System;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using HorizontalAlignment=System.Windows.Forms.HorizontalAlignment;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public partial class TimeSpanTextBox : BaseUserControl
    {
        private TimeSpan _lastCorrectTime;
        private TimeSpan _initResolution;
        private TimeSpan _maximumValue = new TimeSpan(24, 0, 0);
        private bool _defaultInterpretAsMinutes;

        public TimeSpanTextBox()
        {
            AllowNegativeValues = true;
            initialize();
        }

        public TimeSpan Value
        {
            get { return _lastCorrectTime; }
        }

        public void SetInitialResolution(TimeSpan initResolution)
        {
            _initResolution = initResolution;
            _lastCorrectTime = _initResolution;
            formatText();
        }

        public bool AllowNegativeValues { get; set; }

        private void initialize()
        {
            InitializeComponent();
            formatText();
        }

        private void parse(string text)
        {
            TimeSpan timeValue;
            if (TimeHelper.TryParseLongHourStringDefaultInterpretation(text,_maximumValue, out timeValue, TimeFormatsType.HoursMinutes, _defaultInterpretAsMinutes))
            {
                if (AllowNegativeValues)
                    _lastCorrectTime = timeValue;
                else if (timeValue.TotalSeconds >= 0)
                    _lastCorrectTime = timeValue;
            }
            else
                _lastCorrectTime = TimeSpan.MaxValue;
        }

        private void formatText()
        {
            if (_lastCorrectTime == TimeSpan.MaxValue)
            {
                TimeSpanBox.SelectAll();
                return;
            }
            CultureInfo ci = CultureInfo.CurrentCulture;
            TimeSpanBox.Text = TimeHelper.GetLongHourMinuteTimeString(_lastCorrectTime, ci);
            TimeSpanBox.SelectAll();
        }
        
        public TimeSpan DefaultResolution
        {
            get { return _lastCorrectTime; }
        }

        [Browsable(true)]
        public TimeSpan MaximumValue
        {
            get { return _maximumValue; }
            set { _maximumValue = value; }
        }

        [Browsable(true)]
        public int TimeSpanBoxWidth
        {
            get { return TimeSpanBox.Width; }
            set
            {
                if (value >= Width)
                    Width = value + 3;
                TimeSpanBox.Width = value;
            }
        }

        [Browsable(true)]
        public int TimeSpanBoxHeight
        {
            get { return TimeSpanBox.Height; }
            set
            {
                if (value >= Height)
                    Height = value + 3;
                TimeSpanBox.Height = value;
            }
        }

        [Browsable(true)]
        public bool DefaultInterpretAsMinutes
        {
            get { return _defaultInterpretAsMinutes; }
            set { _defaultInterpretAsMinutes = value; }
        }

        [Browsable(true)]
        public HorizontalAlignment AlignTextBoxText
        {
            get { return TimeSpanBox.TextAlign; }
            set { TimeSpanBox.TextAlign = value; }
        }

        protected override void OnLeave(EventArgs e)
        {
            parse(TimeSpanBox.Text);
            formatText();
            base.OnLeave(e);
        }
    }
}
