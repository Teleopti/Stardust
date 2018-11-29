using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    public class TimePeriodCellButton : GridCellButton
    {
        static readonly GridIconPaint iconPainter = new GridIconPaint("CellButtons.", typeof(TimePeriodCellButton).Assembly);

        public TimePeriodCellButton(GridStaticCellRenderer control)
            : base(control)
        {
        }

        public override void Draw(Graphics g, int rowIndex, int colIndex, bool bActive, GridStyleInfo style)
        {
            base.Draw(g, rowIndex, colIndex, bActive, style);

            bool hovering = IsHovering(rowIndex, colIndex);
            bool mouseDown = IsMouseDown(rowIndex, colIndex);
            bool disabled = !style.Clickable;

            ButtonState buttonState = ButtonState.Normal;
            if (disabled)
                buttonState |= ButtonState.Inactive | ButtonState.Flat;

            else if (!hovering && !mouseDown)
                buttonState |= ButtonState.Flat;

            Point ptOffset = Point.Empty;
            if (mouseDown)
            {
                ptOffset = new Point(1, 1);
                buttonState |= ButtonState.Pushed;
            }
            DrawButton(g, Bounds, buttonState, style);

            // Instead of using GridIconPaint you could simple use Image.Draw here
            // with an existing bitmap. GridIconPaint is convenient because it lets
            // us easily draw over existing background and replace the black color
            // in the bitmap with a different color.
            iconPainter.PaintIcon(g, Bounds, ptOffset, Resources.ccc_BrowseDots, Color.Black);
        }
    }

    [Serializable]
    public class TimePeriodCellModel : GridStaticCellModel
    {
        protected TimePeriodCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ButtonBarSize = new Size(20, 20);
        }

        public TimePeriodCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimePeriodCellRenderer(control, this);
        }

        protected override Size OnQueryPrefferedClientSize(Graphics g, int rowIndex, int colIndex, GridStyleInfo style, GridQueryBounds queryBounds)
        {
            Size size = base.OnQueryPrefferedClientSize(g, rowIndex, colIndex, style, queryBounds);
            return new Size(size.Width + 5, size.Height); // base method already consides ButtonBarSize, but let's add some more pixels here.
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            IList<TimePeriod> typedValue;
            if (TimeHelper.TryParseTimePeriods(text, out typedValue))
            {
                style.CellValue = typedValue;
                return true;
            }

            if (text.Trim() == UserTexts.Resources.Closed)
            {
                style.CellValue = new List<TimePeriod>();
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            StringBuilder ret = new StringBuilder();

            var typedValue = value as IList<TimePeriod>;
            if (typedValue != null)
            {
                if (typedValue.Count == 0)
                {
                    return UserTexts.Resources.Closed;
                }

                ret.Append(typedValue[0].ToShortTimeString());
                //Multiple openHours should not be implemented yet
                // Yes it should /Micke
                for (int i = 1; i < typedValue.Count; i++)
                {
                    ret.Append(string.Concat(";", typedValue[i].ToShortTimeString()));
                }
            }
            else
            {
                ret.Append(UserTexts.Resources.Closed);
            }

            return ret.ToString();
        }
    }

    public class TimePeriodCellRenderer : GridStaticCellRenderer
    {
        public TimePeriodCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
            AddButton(new TimePeriodCellButton(this));
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.ShowButtons = GridShowButtons.Show;
            e.Style.Clickable = true;
            e.Style.TextAlign = GridTextAlign.Left;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }

        protected override Rectangle OnLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds, Rectangle[] buttonsBounds)
        {
            Rectangle rightArea;

            // Here you specify where the button should be drawn within the cell.
            if (Grid.RightToLeft == RightToLeft.Yes)
            {
                rightArea =
                    Rectangle.FromLTRB(innerBounds.Left, innerBounds.Top, innerBounds.Left + 20, innerBounds.Bottom);
            }
            else
            {
                rightArea =
                    Rectangle.FromLTRB(innerBounds.Right - 20, innerBounds.Top, innerBounds.Right, innerBounds.Bottom);
            }
            buttonsBounds[0] = GridUtil.CenterInRect(rightArea, new Size(20, 20));

            // Here you return the area where the text should be drawn/edited.
            innerBounds.Width -= 10;
            return innerBounds;
        }
    }
}