using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;
using HorizontalAlignment=System.Windows.Forms.HorizontalAlignment;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// A TextBox user control which handles timeSpans as hours and minutes
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-08-19
    /// </remarks>
    public partial class TimeSpanTextBox : BaseUserControl
    {
        TimeSpan _lastCorrectTime;
        TimeSpan _initResolution;
        private TimeSpan _maximumValue = new TimeSpan(24, 0, 0);
        private bool _allowNegativValue = true;
        private bool _defaultInterpretAsMinutes;

        public TimeSpanTextBox()
        {
            initialize();
        }

        #region Public methods

        public TimeSpan Value
        {
            get { return _lastCorrectTime; }
        }

        /// <summary>
        /// Sets the initial resolution.
        /// </summary>
        /// <param name="initResolution">The init resolution.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-19
        /// </remarks>
        public void SetInitialResolution(TimeSpan initResolution)
        {
            _initResolution = initResolution;
            _lastCorrectTime = _initResolution;
            formatText();
        }
        /// <summary>
        /// Sets the size.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-19
        /// </remarks>
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
        /// <summary>
        /// Gets or sets a value indicating whether [allow negative values]. Defaults to true.
        /// </summary>
        /// <value><c>true</c> if [allow negative values]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-03-27
        /// </remarks>
        public bool AllowNegativeValues
        {
            get { return _allowNegativValue; }
            set { _allowNegativValue = value; }
        }
        public void SetPadding(int left, int right, int top, int bottom)
        {
            Padding = new Padding(left, top, right, bottom);
        }

        #endregion

        #region Private methods

        private void initialize()
        {
            InitializeComponent();
            formatText();
        }

        private void parse(string text)
        {
            TimeSpan timeValue;
			if (TimeHelper.TryParseLongHourStringDefaultInterpretation(text, _maximumValue, out timeValue, TimeFormatsType.HoursMinutes, _defaultInterpretAsMinutes))
            {
                if (timeValue > _maximumValue)
                {
                    timeValue = _maximumValue;
                }
                if (_allowNegativValue)
                    _lastCorrectTime = timeValue;
                else if (timeValue.TotalSeconds >= 0)
                    _lastCorrectTime = timeValue;
            }
        }

        private void formatText()
        {
            var ci = CultureInfo.CurrentCulture;
            if (!AllowNegativeValues && _lastCorrectTime < TimeSpan.Zero)
            {
                _lastCorrectTime = TimeSpan.Zero;
                TimeSpanBox.Text = TimeHelper.GetLongHourMinuteTimeString(TimeSpan.Zero, ci);
                return;
            }
            TimeSpanBox.Text = TimeHelper.GetLongHourMinuteTimeString(_lastCorrectTime, ci);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the default resolution.
        /// </summary>
        /// <value>The default resolution.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-19
        /// </remarks>
        public TimeSpan DefaultResolution
        {
            get { return _lastCorrectTime; }
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

        #endregion

        #region Events

        protected override void OnLeave(EventArgs e)
        {
            parse(TimeSpanBox.Text);
            formatText();
            base.OnLeave(e);
        }
        #endregion
    }
}
