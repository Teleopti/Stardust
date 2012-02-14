﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    [Browsable(true), Category("Teleopti Controls")]
    public partial class Office2007OutlookTimePickerAP : Syncfusion.Windows.Forms.Tools.ComboBoxAdv
    {
        private int _timeIntervalInDropDown = 30;
        private int _defaultResolution = 1;

        //public event EventHandler<EventArgs> SelectedValueChanged;
        public TimeSpan BindableTimeValue
        {
            get { return TimeValue(); }
            set
            {
                SetTimeValue(value);
            }
        }

        public Office2007OutlookTimePickerAP()
        {
            InitializeComponent();
        }

        public Office2007OutlookTimePickerAP(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        [Browsable(true), Category("Custom settings"), DefaultValue(30)]
        public int TimeIntervalInDropDown
        {
            get { return _timeIntervalInDropDown; }
            set { _timeIntervalInDropDown = value; }
        }

        [Browsable(true), Category("Custom settings"), DefaultValue(1)]
        public int DefaultResolution
        {
            get { return _defaultResolution; }
            set { _defaultResolution = value; }
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            FormatTextAsTime();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            FormatTextAsTime();
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
            TimeSpan timeOfDay;
            string timeAsText;
            if (GetTimeInformation(out timeAsText, out timeOfDay))
                Text = timeAsText;
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
        private bool GetTimeInformation(out string timeAsString, out TimeSpan timeAsTimeSpan)
        {
            if (TimeHelper.TryParse(Text, out timeAsTimeSpan) && timeAsTimeSpan < TimeSpan.FromDays(1))
            {
                timeAsTimeSpan = TimeHelper.FitToDefaultResolution(timeAsTimeSpan, _defaultResolution);
                timeAsString = DateTime.MinValue.Add(timeAsTimeSpan).ToShortTimeString();
                return true;
            }

            timeAsString = Text;
            return false;
        }

        public void CreateAndBindList()
        {
            CreateAndBindList(TimeSpan.Zero, new TimeSpan(23, 59, 59));
        }

        public void CreateAndBindList(TimeSpan from, TimeSpan to)
        {
            DataSource = null;
            Items.Clear();

            List<string> timeList = new List<string>();
            DateTime maxTime = DateTime.MinValue.Add(to.Add(new TimeSpan(1)));
            for (DateTime timeOfDay = DateTime.MinValue.Add(from);
                timeOfDay < maxTime;
                timeOfDay = timeOfDay.AddMinutes(_timeIntervalInDropDown))
            {
                timeList.Add(timeOfDay.ToShortTimeString());
            }
            Items.AddRange(timeList.ToArray());
            //DataSource = timeList;
            //note the use of Datasource in this control really made it go crazy
            //when the control was dynamicaly added tho the control from the SkillWizard
        }

        /// <summary>
        /// Gets the time value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public TimeSpan TimeValue()
        {
            TimeSpan timeOfDay;
            string timeAsText;
            if (!GetTimeInformation(out timeAsText, out timeOfDay) && !DesignMode)
                throw new ArgumentException(UserTexts.Resources.MustSpecifyValidTime);
            return timeOfDay;
        }

        /// <summary>
        /// Sets the time value.
        /// </summary>
        /// <param name="timeAsTimeSpan">The time as time span.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-29
        /// </remarks>
        public void SetTimeValue(TimeSpan timeAsTimeSpan)
        {
            Text = DateTime.MinValue.Add(timeAsTimeSpan).ToShortTimeString();
        }
    }
}
