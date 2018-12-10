using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class PercentReadOnlyCellModel : GridStaticCellModel
    {
        private int _numberOfDecimals;
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        protected PercentReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            HorizontalAlignment = GridHorizontalAlignment.Right;
        }

        public PercentReadOnlyCellModel(GridModel grid)
            : base(grid)
        {
            HorizontalAlignment = GridHorizontalAlignment.Right;
        }

        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set {
                _numberOfDecimals = value < 0 ? 0 : value;
            }
        }

        public GridHorizontalAlignment HorizontalAlignment { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new PercentReadOnlyCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            if (value is Percent)
            {
                nfi.PercentDecimalDigits = _numberOfDecimals;
                return ((Percent)value).ToString(nfi);
            }

            double d;
            if (value == null || !double.TryParse(value.ToString(), out d)) return string.Empty;
            return d.ToString("P" + _numberOfDecimals.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }

    public class PercentReadOnlyCellRenderer : GridStaticCellRenderer
    {
        public PercentReadOnlyCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = ((PercentReadOnlyCellModel)Model).HorizontalAlignment;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}