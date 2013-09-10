﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public partial class FromToTimeDurationPicker : UserControl
    {
        public FromToTimeDurationPicker()
        {
            InitializeComponent();
        }

        public Office2007OutlookTimeDurationPicker StartTime
        {
            get { return office2007OutlookTimeDurationPickerStartTime; }
            set { office2007OutlookTimeDurationPickerStartTime = value; }
        }

        public Office2007OutlookTimeDurationPicker EndTime
        {
            get { return office2007OutlookTimeDurationPickerEndTime; }
            set { office2007OutlookTimeDurationPickerEndTime = value; }
        }

        public CheckBoxAdv WholeDay
        {
            get { return checkBoxAdvWholeDay; }
            set { checkBoxAdvWholeDay = value; }
        }

        public MinMax<TimeSpan> MinMaxStartTime
        {
            get
            {
                return new MinMax<TimeSpan>(office2007OutlookTimeDurationPickerStartTime.MinValue,
                                            office2007OutlookTimeDurationPickerStartTime.MaxValue);
            }
            set
            {
                office2007OutlookTimeDurationPickerStartTime.MinValue = value.Minimum;
                office2007OutlookTimeDurationPickerStartTime.MaxValue = value.Maximum;
            }
        }

        public MinMax<TimeSpan> MinMaxEndTime
        {
            get
            {
                return new MinMax<TimeSpan>(office2007OutlookTimeDurationPickerEndTime.MinValue,
                                            office2007OutlookTimeDurationPickerEndTime.MaxValue);
            }
            set
            {
                office2007OutlookTimeDurationPickerEndTime.MinValue = value.Minimum;
                office2007OutlookTimeDurationPickerEndTime.MaxValue = value.Maximum;
            }
        }

        [Localizable(true)]
        public string WholeDayText
        {
            get { return checkBoxAdvWholeDay.Text; }
            set { checkBoxAdvWholeDay.Text = value; }
        }
    }
}
