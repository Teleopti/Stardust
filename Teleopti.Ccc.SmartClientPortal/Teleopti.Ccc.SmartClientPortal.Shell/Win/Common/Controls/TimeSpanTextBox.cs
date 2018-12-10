using System;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using HorizontalAlignment=System.Windows.Forms.HorizontalAlignment;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public partial class TimeSpanTextBox : BaseUserControl
    {
        TimeSpan _lastCorrectTime;
        TimeSpan _initResolution;
        private TimeSpan _maximumValue = new TimeSpan(24, 0, 0);
        private bool _defaultInterpretAsMinutes;
		private TimeFormatsType _timeFormat = TimeFormatsType.HoursMinutes;

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
        
        public void SetSize(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                TimeSpanBox.Width = width;
                TimeSpanBox.Height = height;
                Width = width;
                Height = height+1;
            }
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
			if (TimeHelper.TryParseLongHourStringDefaultInterpretation(text, _maximumValue, out timeValue, _timeFormat, _defaultInterpretAsMinutes))
            {
                if (timeValue > _maximumValue)
                {
                    timeValue = _maximumValue;
                }
                if (AllowNegativeValues)
                    _lastCorrectTime = timeValue;
                else if (timeValue.TotalSeconds >= 0)
                    _lastCorrectTime = timeValue;
            }
        }

        private void formatText()
        {
            if (!AllowNegativeValues && _lastCorrectTime < TimeSpan.Zero)
            {
                _lastCorrectTime = TimeSpan.Zero;
            }
            TimeSpanBox.Text = formatTimeSpan(_lastCorrectTime);
        }

	    private string formatTimeSpan(TimeSpan span)
		{
			var ci = CultureInfo.CurrentCulture;
		    return _timeFormat == TimeFormatsType.HoursMinutes
			    ? TimeHelper.GetLongHourMinuteTimeString(span, ci)
			    : TimeHelper.GetLongHourMinuteSecondTimeString(span, ci);
		}

	    public override bool HasHelp
        {
            get
            {
                return false;
            }
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

		[Browsable(true)]
		public TimeFormatsType TimeFormat
		{
			get { return _timeFormat; }
			set { _timeFormat = value; }
		}

        protected override void OnLeave(EventArgs e)
        {
            parse(TimeSpanBox.Text);
            formatText();
            base.OnLeave(e);
        }
    }
}
