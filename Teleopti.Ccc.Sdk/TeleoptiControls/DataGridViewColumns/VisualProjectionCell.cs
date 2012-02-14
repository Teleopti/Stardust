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
    public class VisualProjectionCell : DataGridViewCell
    {
        private Color _defaultColor = Color.Red;

        public VisualProjectionCell() :base()
        {
            setStyle();
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, System.ComponentModel.TypeConverter valueTypeConverter, System.ComponentModel.TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            return string.Empty;
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, 
            DataGridViewElementStates cellState, object value, object formattedValue, string errorText, 
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, 
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText,
                       cellStyle, advancedBorderStyle, paintParts);

            if(RowIndex >= 0)
            {
                // Draw the background of the cell, if specified.
                if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
                    drawBackground(graphics, cellBounds, cellState);

                // Draw the foreground of the cell, if specified.
                if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground)
                {
                    VisualProjection projection = (VisualProjection) Value;
                    if (projection.IsDayOff)
                    {
                        drawDayOffForeground(graphics, cellBounds, projection.DayOffName);
                    }
                    else
                    {
                        drawProjectionForeground(graphics, cellBounds);
                    }     
                }
            }

            // Draw the cell borders, if specified.
            if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
                PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
        }

        

        public override DataGridViewAdvancedBorderStyle AdjustCellBorderStyle(
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, 
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, 
            bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, 
            bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            if (dataGridViewAdvancedBorderStylePlaceholder == null)
                throw new ArgumentNullException("dataGridViewAdvancedBorderStylePlaceholder",
                                                "DataGridViewAdvancedBorderStyle must not be Null");

            if (isCurrentCell())
                dataGridViewAdvancedBorderStylePlaceholder.All = DataGridViewAdvancedCellBorderStyle.OutsetPartial;
            else
            {
                dataGridViewAdvancedBorderStylePlaceholder.Right = DataGridViewAdvancedCellBorderStyle.Single;
                dataGridViewAdvancedBorderStylePlaceholder.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            }
            return dataGridViewAdvancedBorderStylePlaceholder;

        }

        private void drawDayOffForeground(Graphics graphics, Rectangle cellBounds, string dayOffName)
        {
            if (Value == null)
                return;

            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                SizeF textSize = graphics.MeasureString(dayOffName, font);
                graphics.DrawString(dayOffName, font, Brushes.Black, new PointF(cellBounds.X + (cellBounds.Width/2)- (textSize.Width/2), cellBounds.Y));
            }
            
            
        }

        private void drawProjectionForeground(Graphics graphics, Rectangle cellBounds)
        {
            if(Value == null)
                return;

            TimePeriod timePeriod = ((VisualProjectionColumn)OwningColumn).ViewSpan;
            PixelConverter pixelConverter = new PixelConverter(cellBounds.Width, timePeriod, false);

            for (int i = (int)timePeriod.StartTime.TotalHours; i <= (int)timePeriod.EndTime.TotalHours; i++)
            {
                int x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));
                if (x > 0)
                    graphics.DrawLine(new Pen(Color.LightGray, 1), cellBounds.Left + x, cellBounds.Top, cellBounds.Left + x, cellBounds.Bottom);
            }
            foreach (VisualLayer layer in ((VisualProjection)Value).LayerCollection)
            {
                RectangleF rect = getLayerRectangle(pixelConverter, layer.Period, cellBounds);
                if (!rect.IsEmpty)
                {
                    RectangleF rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, layer.Color, 90, false))
                    {
                        graphics.FillRectangle(brush, rect);
                    }
                }
            }
        }

        private void drawBackground(Graphics graphics, Rectangle cellBounds, DataGridViewElementStates cellState)
        {
            using (SolidBrush cellBackground = new SolidBrush(Style.BackColor))
            {
                //if CType(cellState And DataGridViewElementStates.Selected, Boolean) Then
                if (cellState == DataGridViewElementStates.Selected)
                {
                    cellBackground.Color = Color.Blue;
                    if (isCurrentCell())
                        cellBackground.Color = Color.LightBlue;
                }
                else
                {
                    cellBackground.Color = Style.BackColor;
                }

                graphics.FillRectangle(cellBackground, cellBounds);
            }
        }

        private static RectangleF getLayerRectangle(PixelConverter pixelConverter, TimePeriod period, RectangleF clientRect)
        {
            if (clientRect.Height < 9)
                return RectangleF.Empty;

            int x1 = pixelConverter.GetPixelFromTimeSpan(period.StartTime);
            int x2 = pixelConverter.GetPixelFromTimeSpan(period.EndTime);
            if (x2 - x1 < 1)
                return RectangleF.Empty;
            return new RectangleF(clientRect.Left + x1, clientRect.Top + 4, x2 - x1, clientRect.Height - 8);
        }

        private bool isCurrentCell()
        {
            if (DataGridView.CurrentCell != null)
            {
                if (DataGridView.CurrentCell.RowIndex == RowIndex && DataGridView.CurrentCell.ColumnIndex == ColumnIndex)
                    return true;
            }
            return false;
        }

        private void setStyle()
        {
            DataGridViewCellStyle myStyle = new DataGridViewCellStyle();
            myStyle.BackColor = Color.White;
            myStyle.ForeColor = Color.Black;
            myStyle.SelectionBackColor = Color.Blue;
            myStyle.SelectionForeColor = Color.White;
            myStyle.DataSourceNullValue = _defaultColor;
            myStyle.NullValue = Color.Red;
            Style = myStyle;
        }
    }
}
