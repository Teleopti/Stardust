using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class NumericCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected NumericCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MaxValue = double.MaxValue;
            MinValue = double.MinValue;
            HorizontalAlignment = GridHorizontalAlignment.Right;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public NumericCellModel(GridModel grid)
            : base(grid)
        {
            MaxValue = double.MaxValue;
            MinValue = double.MinValue;
            HorizontalAlignment = GridHorizontalAlignment.Right;
        }

        /// <summary>
        /// Gets or sets the num decimals.
        /// </summary>
        /// <value>The num decimals.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set
            {
                if (value < 0)
                {
                    _numberOfDecimals = 0;
                }
                else
                {
                    _numberOfDecimals = value;
                }
            }
        }

        public GridHorizontalAlignment HorizontalAlignment { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new NumericCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if (string.IsNullOrEmpty(text))
            {
                style.CellValue = 0.0d;
                return true;
            }
            // Make sure value in cell can be coverted to a double
            double d;
            if (!double.TryParse(text, out d))
                return false;
            if (d < MinValue || d > MaxValue)
                return false;

            style.CellValue = d;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);
        
            // Make sure value in cell can be coverted to a double
            double d;
            if (!double.TryParse(value.ToString(), out d))
                return "";

            NumberFormatInfo nfi = (NumberFormatInfo) ci.NumberFormat.Clone();
            nfi.NumberDecimalDigits = NumberOfDecimals;

            if (textInfo == GridCellBaseTextInfo.CopyText)
            {
                //Ugly fix to work with copy/paste in Excel
                return GetFormattedTextForCopyPasteExcel(nfi, d);
            }
            return ((decimal) d).ToString("N", nfi);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string GetFormattedTextForCopyPasteExcel(NumberFormatInfo numberFormatInfo, double value)
        {
            string fixedText = ((decimal) value).ToString("N", numberFormatInfo).Replace((char) 160, (char) 32);
            return fixedText;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }

    public class NumericCellRenderer : GridTextBoxCellRenderer
    {
        public NumericCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = ((NumericCellModel)Model).HorizontalAlignment;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }

	    protected override void OnBeginEdit()
	    {
		    base.OnBeginEdit();
			TextBox.SelectAll();
	    }
    }
}