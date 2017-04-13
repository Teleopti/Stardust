using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanTotalSecondsStaticCellModel : GridStaticCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;

        protected TimeSpanTotalSecondsStaticCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanTotalSecondsStaticCellModel(GridModel grid)
            : base(grid)
        {
        }

        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set
            {
                _numberOfDecimals = Math.Max(Math.Min(3, value), 0);
            }
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeStaticCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            var ret = string.Empty;
            if (value is TimeSpan)
            {
                double d = ((TimeSpan)value).TotalSeconds;

                NumberFormatInfo nfi = (NumberFormatInfo)ci.NumberFormat.Clone();
                nfi.NumberDecimalDigits = NumberOfDecimals;

                ret = ((decimal)d).ToString("N", nfi);
            }

            return ret;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}