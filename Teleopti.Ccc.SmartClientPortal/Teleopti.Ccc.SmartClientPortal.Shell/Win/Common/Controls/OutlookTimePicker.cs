using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    /// <summary>
    /// A time picker control which has same functionality as Outlook's
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-14
    /// </remarks>
    [Browsable(true), Category("Teleopti Controls")]
    public class OutlookTimePicker : ComboBoxAdv
    {
        private int _timeIntervalInDropDown = 30;
        private int _defaultResolution;
        private bool _enableNull;
        private TimeSpan _maxTime = TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(-1));
        private bool _formatFromCulture;

        /// <summary>
        /// Gets or sets the time interval in drop down.
        /// </summary>
        /// <value>The time interval in drop down.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-11
        /// </remarks>
        [Browsable(true), Category("Custom settings"), DefaultValue(30)]
        public int TimeIntervalInDropDown
        {
            get { return _timeIntervalInDropDown; }
            set { _timeIntervalInDropDown = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable null].
        /// </summary>
        /// <value><c>true</c> if [enable null]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-01-14
        /// </remarks>
        [Browsable(true), Category("Custom settings"), DefaultValue(false)]
        public bool EnableNull
        {
            get { return _enableNull; }
            set { _enableNull = value; }
        }

        /// <summary>
        /// Gets or sets the max time to show in the drop down.
        /// </summary>
        /// <value>The max time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-01-29
        /// </remarks>
        [Browsable(true), Category("Custom settings")]
        public TimeSpan MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value;}
        }

        /// <summary>
        /// Gets or sets the default resolution.
        /// </summary>
        /// <value>The default resolution.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public int DefaultResolution
        {
            get { return _defaultResolution; }
            set { _defaultResolution = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ObjectCollection Items
        {
            get
            {
                return base.Items;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageIndexItemCollection ItemsImageIndexes
        {
            get { return base.ItemsImageIndexes; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object DataSource
        {
            get { return base.DataSource; }
            set { base.DataSource = value; }
        }

        [Browsable(true), Category("Format from culture"), DefaultValue(false)]
        public bool FormatFromCulture
        {
            get { return _formatFromCulture; }
            set { _formatFromCulture = value; }
        }

        /// <summary>
        /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"/> method.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            IList<string> timeList = new List<string>();
            TimeSpan resolution = TimeSpan.FromMinutes(_timeIntervalInDropDown);
            if (!_formatFromCulture)
            {
                for (TimeSpan timeOfDay = TimeSpan.Zero;
                     timeOfDay < _maxTime;
                     timeOfDay = timeOfDay.Add(resolution))
                {
                    timeList.Add(TimeHelper.GetLongHourMinuteTimeString(timeOfDay, CultureInfo.CurrentCulture));
                }
            }
            else
            {
                CreateAndBindList(timeList);
            }

            if (_enableNull)
            {
                timeList.Insert(0, UserTexts.Resources.None);
            }
            Style = VisualStyle.Office2007;
            string tempText = Text;
            DataSource = timeList;
            _defaultResolution = 1;
            Text = tempText;
        }


        public void CreateAndBindList(IList<string> timeList)
        {
            CreateAndBindList(TimeSpan.Zero, new TimeSpan(23, 59, 59), timeList);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void CreateAndBindList(TimeSpan from, TimeSpan to, IList<string> timeList)
        {
            DataSource = null;
            Items.Clear();

            DateTime maxTime = DateTime.MinValue.Add(to.Add(new TimeSpan(1)));
            for (DateTime timeOfDay = DateTime.MinValue.Add(from);
                timeOfDay < maxTime;
                timeOfDay = timeOfDay.AddMinutes(_timeIntervalInDropDown))
            {
                timeList.Add(timeOfDay.ToShortTimeString());
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDown"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            FormatTextAsTime();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            FormatTextAsTime();
        }

        /// <summary>
        /// Gets the time value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public TimeSpan? TimeValue()
        {
            if (!_formatFromCulture)
            {
				if (!GetTimeInformation(out _, out var timeOfDay))
                {
                    if (_enableNull)
                        timeOfDay = null;
                    else
                    {
                        timeOfDay = TimeSpan.Zero;
                    }
                }
                if (timeOfDay > _maxTime)
                    timeOfDay = _maxTime;
                return timeOfDay;
            }
            else
            {
                TimeSpan? timeOfDay;
				if (!GetTimeInformationFromFormat(out _, out timeOfDay))
                {
                    if (_enableNull)
                        timeOfDay = null;
                    else
                    {
                        timeOfDay = TimeSpan.Zero;
                    }
                }
                if (timeOfDay > _maxTime)
                    timeOfDay = _maxTime;
                return timeOfDay;
            }
        }

        /// <summary>
        /// Sets the time value.
        /// </summary>
        /// <param name="timeAsTimeSpan">The time as time span.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-29
        /// </remarks>
        public void SetTimeValue(TimeSpan? timeAsTimeSpan)
        {
            if (timeAsTimeSpan.HasValue)
            {
                if (_formatFromCulture)
                    Text = TimeOfDayFromTimeSpan(timeAsTimeSpan);
                else if (!_formatFromCulture)
                    Text = TimeHelper.GetLongHourMinuteTimeString(timeAsTimeSpan.Value, CultureInfo.CurrentCulture);
            }
            else if (_enableNull)
            {
                Text = UserTexts.Resources.None;
            }
        }
        public static string TimeOfDayFromTimeSpan(TimeSpan? timeSpan)
        {
            int days = timeSpan.Value.Days;
            string nextDay = "";
            if (days > 0)
                nextDay = " +" + days;

            DateTime d = DateTime.MinValue.Add(timeSpan.Value);
            string ret = d.ToString("t", CultureInfo.CurrentCulture);
            ret += nextDay;
            return ret;
        }
        /// <summary>
        /// Formats the text as time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        private void FormatTextAsTime()
        {
			string timeAsText;
            if (!FormatFromCulture && GetTimeInformation(out timeAsText, out _))
                Text = timeAsText;
            if (FormatFromCulture && GetTimeInformationFromFormat(out timeAsText, out _))
                Text = timeAsText;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode==Keys.Delete && _enableNull)
            {
                Text = UserTexts.Resources.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Gets the time information.
        /// </summary>
        /// <param name="timeAsString">The time as string.</param>
        /// <param name="timeAsTimeSpan">The time as time span.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        private bool GetTimeInformation(out string timeAsString, out TimeSpan? timeAsTimeSpan)
        {
            TimeSpan theTimeSpan;
            if (string.Equals(Text,UserTexts.Resources.None,StringComparison.OrdinalIgnoreCase) && _enableNull)
            {
                timeAsTimeSpan = null;
                timeAsString = UserTexts.Resources.None;
                return true;
            }
            if (TimeHelper.TryParseLongHourString(Text, out theTimeSpan, TimeFormatsType.HoursMinutes))
            {
                timeAsTimeSpan = TimeHelper.FitToDefaultResolution(theTimeSpan, Math.Max(_defaultResolution,1));
                if (timeAsTimeSpan>_maxTime)
                {
                    timeAsTimeSpan = _maxTime;
                }
                timeAsString = TimeHelper.GetLongHourMinuteTimeString(theTimeSpan,CultureInfo.CurrentCulture);
                return true;
            }

            timeAsTimeSpan = TimeSpan.Zero;
            timeAsString = Text;
            return false;
        }
        private bool GetTimeInformationFromFormat(out string timeAsString, out TimeSpan? timeAsTimeSpan)
        {
            if (TimeHelper.TryParse(Text, out timeAsTimeSpan) && timeAsTimeSpan < TimeSpan.FromDays(1))
            {
                timeAsTimeSpan = TimeHelper.FitToDefaultResolution(timeAsTimeSpan.Value, _defaultResolution);
                timeAsString = DateTime.MinValue.Add(timeAsTimeSpan.Value).ToShortTimeString();
                return true;
            }

            timeAsString = Text;
            return false;
        }
    }
}
