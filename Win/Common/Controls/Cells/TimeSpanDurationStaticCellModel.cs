﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanDurationStaticCellModel : GridStaticCellModel
    {
        protected TimeSpanDurationStaticCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanDurationStaticCellModel(GridModel grid)
            : base(grid)
        {
        }

        public bool DisplaySeconds { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeStaticCellRenderer(control, this);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {

            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            String ret = string.Empty;

            if (value is TimeSpan)
            {
                var typedValue = (TimeSpan)value;
                if (typedValue == TimeSpan.MinValue)
                    return string.Empty;

                ret = DisplaySeconds ? TimeHelper.GetLongHourMinuteSecondTimeString(typedValue, ci) : TimeHelper.GetLongHourMinuteTimeString(typedValue, ci);
            }
            return ret;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }
    }
}