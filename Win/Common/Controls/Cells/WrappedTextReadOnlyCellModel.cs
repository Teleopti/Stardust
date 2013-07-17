using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class WrappedTextReadOnlyCellModel : GridStaticCellModel
    {
        public WrappedTextReadOnlyCellModel(GridModel grid) : base(grid)
        {}

        protected WrappedTextReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new WrappedTextReadOnlyCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            style.CellValue = text;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            String ret = string.Empty;

            var s = value as string;
            if (s != null)
                ret = s;

            return ret;
        }
    }

    public class WrappedTextReadOnlyCellModelRenderer : GridStaticCellRenderer
    {
        public WrappedTextReadOnlyCellModelRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Left;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = true;
        }
    }
}