using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class DropDownCellStaticModel: GridStaticCellModel
    {
        public DropDownCellStaticModel(GridModel grid) : base(grid)
        {
            AllowFloating = false;
            AllowMerging = false;
            ButtonBarSize = new Size(15, 0x12);
        }

        protected DropDownCellStaticModel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new StaticDropDownCellRenderer(control, this);
        }

        protected override Size OnQueryPrefferedClientSize(Graphics g, int rowIndex, int colIndex, GridStyleInfo style, GridQueryBounds queryBounds)
        {
            Size size = base.OnQueryPrefferedClientSize(g, rowIndex, colIndex, style, queryBounds);
            Size empty = Size.Empty;
            Font gdipFont = style.GdipFont;
            if (queryBounds == GridQueryBounds.Height)
            {
                empty = GridMargins.AddMargins(WinFormsUtils.MeasureSampleWString(g, gdipFont), style.ReadOnlyTextMargins.ToMargins());
                if ((ButtonBarSize.Height > empty.Height) && (ButtonBarSize.Height < 0x7fffffff))
                {
                    empty.Height = ButtonBarSize.Height;
                }
                empty.Height++;
            }
            else if (style.ChoiceList != null)
            {
                foreach (string str in style.ChoiceList)
                {
                    Size size3 = GridMargins.AddMargins(g.MeasureString(str, gdipFont).ToSize(), style.ReadOnlyTextMargins.ToMargins());
                    empty.Width = Math.Max(empty.Width, size3.Width);
                }
                empty.Width++;
            }
            return GridUtil.Max(empty, size);
        }
    }

    public class StaticDropDownCellRenderer : GridStaticCellRenderer
    {
        public StaticDropDownCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
            DropDownImp = new GridDropDownCellImp(this);
        }

        public override bool ProcessMouseWheel(MouseEventArgs e)
        {
            return IsDroppedDown;
        }

        public new GridDropDownContainer DropDownContainer
        {
            get
            {
                return (GridDropDownContainer)base.DropDownContainer;
            }
        }
    }
}