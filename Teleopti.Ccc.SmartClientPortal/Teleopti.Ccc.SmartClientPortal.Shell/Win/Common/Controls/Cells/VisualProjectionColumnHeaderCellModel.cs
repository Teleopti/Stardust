using System;
using System.Drawing;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class VisualProjectionColumnHeaderCellModel : GridHeaderCellModel
    {
        public TimeZoneInfo TimeZoneInfo { get; set; }

        public VisualProjectionColumnHeaderCellModel(GridModel grid, TimeZoneInfo timeZoneInfo)
            : base(grid)
        {
            TimeZoneInfo = timeZoneInfo;
        }

        protected VisualProjectionColumnHeaderCellModel(SerializationInfo info, StreamingContext context)
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

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new VisualProjectionColumnHeaderCellRenderer(control, this);
        }
    }

    public class VisualProjectionColumnHeaderCellRenderer : GridHeaderCellRenderer
    {
        private readonly static Font font = new Font("Arial", 8, FontStyle.Bold);

        public VisualProjectionColumnHeaderCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        public override void Draw(Graphics g, Rectangle cellRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
			if (style.Tag == null) return;
            base.Draw(g, cellRectangle, rowIndex, colIndex, style);
            base.OnDrawDisplayText(g, cellRectangle, rowIndex, colIndex, style);

	        var cellModel = style.CellModel as VisualProjectionColumnHeaderCellModel;
	        TimeZoneInfo timeZoneInfo = (cellModel != null && cellModel.TimeZoneInfo != null)
		        ? cellModel.TimeZoneInfo
		        : TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone;

	        var timePeriod = (DateTimePeriod)style.Tag;
            var pixelConverter = new LengthToTimeCalculator(timePeriod, cellRectangle.Width);
            
            string dispDate = (string)style.CellValue;
            SizeF dispDateSize = g.MeasureString(dispDate, font);
            if (dispDateSize.Height == 0)
                dispDateSize = new SizeF(0, cellRectangle.Height/4f);
            g.DrawString(dispDate, font, Brushes.Black,
                                     new PointF(cellRectangle.Left,
                                                cellRectangle.Top));

            string time1 = DateTime.MinValue.Add(TimeSpan.FromHours(12)).ToShortTimeString();
            SizeF size1 = g.MeasureString(time1, font);
            double lastX = 0;
        	int index = 0;

            foreach (DateTimePeriod period in timePeriod.AffectedHourCollection())
            {
                string time = period.StartDateTimeLocal(timeZoneInfo).ToShortTimeString();
                SizeF size = g.MeasureString(time, font);

                double x = pixelConverter.PositionFromDateTime(period.StartDateTime,Grid.IsRightToLeft());
            	index++;
				if (index > 0)
                {
                    if (Math.Abs(x - lastX) <= size1.Width)
                        continue;
                }
                lastX = x;
                if (x > 0)
                {

                    if ((x + (size.Width / 2)) < cellRectangle.Width)
                        g.DrawString(time, font, Brushes.Black,
                                     new PointF((float) (cellRectangle.Left + x - size.Width / 2),
                                                cellRectangle.Top + dispDateSize.Height + 1));
                    g.DrawLine(new Pen(Color.LightGray, 1), (int) (cellRectangle.Left + x), (int) (cellRectangle.Top + dispDateSize.Height + size.Height + 1),
                               (int) (cellRectangle.Left + x), cellRectangle.Bottom);
                }
            }
        }
    }
}