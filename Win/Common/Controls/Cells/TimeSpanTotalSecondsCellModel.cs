using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanTotalSecondsCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;

        protected TimeSpanTotalSecondsCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanTotalSecondsCellModel(GridModel grid)
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

        public bool OnlyPositiveValues { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            double maxValue = TimeSpan.MaxValue.TotalSeconds;
            double minValue = TimeSpan.MinValue.TotalSeconds;

            if (string.IsNullOrWhiteSpace(text))
            {
                style.CellValue = TimeSpan.Zero;
                return true;
            }

            // Make sure text can be coverted to a double
            double d;
            if (!double.TryParse(text, out d))
                return false;

            if (d < 0 && OnlyPositiveValues)
                return false;

            if (d > maxValue || d < minValue)
                return false;

            style.CellValue = TimeSpan.FromSeconds(d);
            return true;
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

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}