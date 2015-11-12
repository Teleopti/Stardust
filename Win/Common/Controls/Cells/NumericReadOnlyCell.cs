using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using System;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
	public class NumericReadOnlyCellModel : GridStaticCellModel, INumericCellModelWithDecimals, ICustomPreferredCellSize
    {
        private int _numDecimals;

        protected NumericReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
			MinValue = double.MinValue;
			MaxValue = double.MaxValue;
        }

        public NumericReadOnlyCellModel(GridModel grid)
            : base(grid)
        {
			MinValue = double.MinValue;
			MaxValue = double.MaxValue;
        }

		public double MaxValue { get; set; }

		public double MinValue { get; set; }

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

		public Size CustomPreferredCellSize(Graphics g, int rowIndex, int colIndex, GridStyleInfo style, GridQueryBounds queryBounds)
	    {
			if (Math.Abs(MaxValue - double.MaxValue) < 0.1)
				return base.CalculatePreferredCellSize(g, rowIndex, colIndex, style, queryBounds);

			var currentValue = style.CellValue;
			style.CellValue = MaxValue;
			var size = base.CalculatePreferredCellSize(g, rowIndex, colIndex, style, queryBounds);
			style.CellValue = currentValue;
			return size;
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