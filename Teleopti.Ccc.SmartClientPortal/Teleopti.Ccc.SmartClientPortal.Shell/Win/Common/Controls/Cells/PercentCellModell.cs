using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class PercentCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;
        private readonly NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        protected PercentCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            HorizontalAlignment = GridHorizontalAlignment.Right;
            MinMax = new MinMax<double>(0,1);
        }

        public PercentCellModel(GridModel grid)
            : base(grid)
        {
            HorizontalAlignment = GridHorizontalAlignment.Right;
            MinMax = new MinMax<double>(0,1);
        }

        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set {
                _numberOfDecimals = value < 0 ? 0 : value;
            }
        }

        public MinMax<double> MinMax { get; set; }

        public GridHorizontalAlignment HorizontalAlignment { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new PercentCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            Percent percentage;
            if (string.IsNullOrEmpty(text))
            {
            	style.CellValue = MinMax.Minimum > 0 ? new Percent(MinMax.Minimum) : new Percent(0);
            	return true;
            }
            
			if (!Percent.TryParse(text,out percentage))
                return false;

            if (percentage.Value < MinMax.Minimum || percentage.Value > MinMax.Maximum)
                return false;

            style.HorizontalAlignment = HorizontalAlignment;
            style.CellValue = percentage;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            if (value is Percent)
            {
                nfi.PercentDecimalDigits = _numberOfDecimals;
                return ((Percent) value).ToString(nfi);
            }
            return string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }

    public class PercentCellRenderer : GridTextBoxCellRenderer
    {
        public PercentCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = ((PercentCellModel)Model).HorizontalAlignment;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}