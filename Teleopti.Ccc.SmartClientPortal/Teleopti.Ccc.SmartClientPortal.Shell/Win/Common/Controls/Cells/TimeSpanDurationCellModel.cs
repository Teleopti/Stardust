using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanDurationCellModel : GridTextBoxCellModel
    {
        public TimeSpanDurationCellModel(GridModel grid) : base(grid)
        {
        }

        protected TimeSpanDurationCellModel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        
        public bool OnlyPositiveValues { get; set; }
        public bool InterpretAsMinutes { get; set; }
        public bool AllowEmptyCell { get; set; }
        public bool DisplaySeconds { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if (AllowEmptyCell && string.IsNullOrWhiteSpace(text))
            {
                style.CellValue = null;
                return true;
            }

            var formatType = TimeFormatsType.HoursMinutes;
            if (DisplaySeconds)
            {
                formatType = TimeFormatsType.HoursMinutesSeconds;
            }

            TimeSpan timeSpan;
            if (!TimeHelper.TryParseLongHourStringDefaultInterpretation(text, TimeSpan.MaxValue, out timeSpan, formatType, InterpretAsMinutes))
                return false;

            if (OnlyPositiveValues && timeSpan < TimeSpan.Zero)
                return false;

            style.CellValue = timeSpan;

            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            String ret = string.Empty;

			if (value is TimeSpan)
			{
				var typedValue = (TimeSpan) value;
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
    }
}
