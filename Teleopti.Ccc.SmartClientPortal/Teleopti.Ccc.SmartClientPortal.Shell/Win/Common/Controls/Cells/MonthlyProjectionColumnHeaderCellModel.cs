using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class MonthlyProjectionColumnHeaderCellModel : GridHeaderCellModel
    {
        public MonthlyProjectionColumnHeaderCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected MonthlyProjectionColumnHeaderCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

        public override string GetText(GridStyleInfo style, object value)
        {
            return string.Empty;
        }

        protected override string GetLocalizedString(string value)
        {
            return string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new DateOnlyProjectionColumnHeaderCellRenderer(control, this);
        }
    }

    public class DateOnlyProjectionColumnHeaderCellRenderer : GridHeaderCellRenderer
    {
        public DateOnlyProjectionColumnHeaderCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String,System.IFormatProvider)")]
        public override void Draw(Graphics g, Rectangle cellRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            base.Draw(g, cellRectangle, rowIndex, colIndex, style);
            base.OnDrawDisplayText(g, cellRectangle, rowIndex, colIndex, style);

            DateOnlyPeriod timePeriod = (DateOnlyPeriod)style.Tag;
            LengthToDateCalculator pixelConverter = new LengthToDateCalculator(timePeriod, cellRectangle.Width);
            
            SizeF dispDateSize = new SizeF(0, cellRectangle.Height/4f);

            bool first = true;
            bool isRightToLeft = Grid.IsRightToLeft();

            DateTime currentMonthStart = DateHelper.GetFirstDateInMonth(timePeriod.StartDate.Date, CultureInfo.CurrentCulture);
            do
            {
                string time = currentMonthStart.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.YearMonthPattern,CultureInfo.CurrentUICulture);
                SizeF size = g.MeasureString(time, Grid.Font);

                double xStart = pixelConverter.PositionFromDateTime(new DateOnly(currentMonthStart), isRightToLeft);

                currentMonthStart = CultureInfo.CurrentCulture.Calendar.AddMonths(currentMonthStart, 1);
                double xEnd = pixelConverter.PositionFromDateTime(new DateOnly(currentMonthStart).AddDays((isRightToLeft) ? 0 : -1), isRightToLeft);

                double areaWidth = Math.Abs(xEnd - xStart);
                if (first)
                {
                    first = false;
                    if (size.Width >= areaWidth) continue;
                }
                if (isRightToLeft)
                {
                    xStart = xEnd;
                }
                if (size.Width < cellRectangle.Width && size.Width<areaWidth)
                    g.DrawString(time, Grid.Font, Brushes.Black,
                                 new PointF((float)(cellRectangle.Left + xStart + areaWidth/2 - size.Width / 2),
                                            cellRectangle.Top + dispDateSize.Height + 1));

                if (xStart > 0)
                {
                    g.DrawLine(new Pen(Color.Black, 1), (int)(cellRectangle.Left + xStart), (int)(cellRectangle.Top + dispDateSize.Height + 1),
                               (int)(cellRectangle.Left + xStart), cellRectangle.Bottom);
                }
            } while (currentMonthStart<timePeriod.EndDate.Date);
        }
    }
}