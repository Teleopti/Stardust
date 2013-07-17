using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using System;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class NumericReadOnlyCellModel : GridStaticCellModel, INumericCellModelWithDecimals
    {
        private int _numDecimals;

        protected NumericReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public NumericReadOnlyCellModel(GridModel grid)
            : base(grid)
        {
        }

        public int NumberOfDecimals
        {
            get { return _numDecimals; }
            set
            {
                if (value < 0)
                {
                    _numDecimals = 0;
                }
                else
                {
                    _numDecimals = value;
                }
            }
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new NumericReadOnlyCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            // Make sure value in cell can be coverted to a double
            double d;
            if (value == null ||
                !double.TryParse(value.ToString(), out d) ||
                double.IsNaN(d) || double.IsInfinity(d))
                return "";

            NumberFormatInfo nfi = (NumberFormatInfo) ci.NumberFormat.Clone();
            nfi.NumberDecimalDigits = NumberOfDecimals;

            if (textInfo == GridCellBaseTextInfo.CopyText)
            {
                return NumericCellModel.GetFormattedTextForCopyPasteExcel(nfi, d);
            }
            return ((decimal) d).ToString("N", nfi);

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Renders the numeric cell
    /// </summary>
    public class NumericReadOnlyCellRenderer : GridStaticCellRenderer
    {
        public NumericReadOnlyCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}