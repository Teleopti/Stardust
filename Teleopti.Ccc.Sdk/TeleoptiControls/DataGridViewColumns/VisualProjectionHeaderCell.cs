using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GridTest;

namespace TeleoptiControls.DataGridViewColumns
{
    public class VisualProjectionHeaderCell : DataGridViewColumnHeaderCell
    {
        public VisualProjectionHeaderCell()
        {
            Style.SelectionForeColor = Color.Black;
        }

        public VisualProjectionHeaderCell(DataGridViewColumnHeaderCell oldHeaderCell) : this()
        {
            base.ContextMenuStrip = oldHeaderCell.ContextMenuStrip;
            ErrorText = oldHeaderCell.ErrorText;
            Tag = oldHeaderCell.Tag;

            ToolTipText = oldHeaderCell.ToolTipText;
            Value = oldHeaderCell.Value;
            base.ValueType = oldHeaderCell.ValueType;

            // Use HasStyle to avoid creating a new style object
            // when the Style property has not previously been set. 
            if (oldHeaderCell.HasStyle)
                Style = oldHeaderCell.Style;

            Style.SelectionForeColor = Color.Black;


            VisualProjectionHeaderCell extendedCell = oldHeaderCell as VisualProjectionHeaderCell;
            if (extendedCell != null)
            {
                //extended props here
            }

        }

        public override object Clone()
        {
            return new VisualProjectionHeaderCell(this);
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, 
            DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, 
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // Use the base method to paint the default appearance.
            DataGridViewPaintParts newpaintparts = paintParts;
            newpaintparts = newpaintparts &= ~DataGridViewPaintParts.ContentForeground;
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue,
                       errorText, cellStyle, advancedBorderStyle, newpaintparts);

            if((paintParts & DataGridViewPaintParts.ContentBackground) == DataGridViewPaintParts.ContentBackground)
            {
                Color c;
                //if(DataGridView.SelectedColumns.Contains(DataGridView.Columns(ColumnIndex)))
                if(OwningColumn.Selected)
                    c = SystemColors.ButtonHighlight;
                else
                    c = SystemColors.ButtonFace;
                using(LinearGradientBrush background = new LinearGradientBrush(cellBounds, Color.White, c, LinearGradientMode.Vertical))
                {
                    graphics.FillRectangle(background, cellBounds);
                }
            }

            if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground)
            {
                TimePeriod timePeriod = ((VisualProjectionColumn)OwningColumn).ViewSpan;
                string dispDate = ((VisualProjectionColumn) OwningColumn).DispalyDate;
                PixelConverter pixelConverter = new PixelConverter(cellBounds.Width, timePeriod, false);
                Font font = new Font("Arial", 8, FontStyle.Bold);

                
                SizeF dispDateSize = graphics.MeasureString(dispDate, font);
                PointF dispDatePoint =
                    new PointF(cellBounds.Left + (cellBounds.Width / 2) - (dispDateSize.Width / 2),
                               cellBounds.Top + 1);
                graphics.DrawString(dispDate, font, Brushes.Black, dispDatePoint);

                string time1 = DateTime.MinValue.Add(TimeSpan.FromHours(12)).ToShortTimeString();
                SizeF size1 = graphics.MeasureString(time1, font);
                float lastX = 0;
                int firstIndex = (int)timePeriod.StartTime.TotalHours;
                for (int i = firstIndex; i <= (int)timePeriod.EndTime.TotalHours; i++)
                {
					if(i == -1) continue;
                    string time = DateTime.MinValue.Add(TimeSpan.FromHours(i)).ToShortTimeString();
                    SizeF size = graphics.MeasureString(time, font);

                    float x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));
                    if (i > firstIndex)
                    {
                        if (x - lastX <= size1.Width)
                            continue;
                    }
                    lastX = x;
                    if (x > 0)
                    {
                        if ((x + (size.Width / 2)) < cellBounds.Width)
                            graphics.DrawString(time, font, Brushes.Black,
                                         new PointF(cellBounds.Left + x - size.Width / 2,
                                                    cellBounds.Top + dispDateSize.Height + 1));
                        graphics.DrawLine(new Pen(Color.LightGray, 1), cellBounds.Left + x, cellBounds.Top + dispDateSize.Height + size.Height + 1,
                                   cellBounds.Left + x, cellBounds.Bottom);
                    }
                }
            }
        }
    }
}
