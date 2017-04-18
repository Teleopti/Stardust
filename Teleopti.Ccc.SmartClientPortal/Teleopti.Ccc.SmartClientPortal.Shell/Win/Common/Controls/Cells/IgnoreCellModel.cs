using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class IgnoreCellModel : GridStaticCellModel
    {
        public IgnoreCellModel(GridModel grid) : base(grid)
        {
        }

        protected IgnoreCellModel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new IgnoreCellModelRenderer(control, this);

        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            style.Enabled = false;
            return "";
        }
    }

    public class IgnoreCellModelRenderer : GridStaticCellRenderer
    {
        public IgnoreCellModelRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
            SupportsEditing = false;
            SupportsFocusControl = false;
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.Borders.All = new GridBorder(GridBorderStyle.None);
            e.Style.Enabled = false;
            e.Style.BackColor = ColorHelper.DisabledCellColor;
        }
    }
}