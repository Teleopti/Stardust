using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;

namespace GridTest
{
    [Serializable]
    public class VisualProjectionColumnHeaderCellModel : GridHeaderCellModel
    {
        private VisualProjection _cellValue;

        public VisualProjectionColumnHeaderCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected VisualProjectionColumnHeaderCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected internal VisualProjection CellValue
        {
            get { return _cellValue; }
            set { _cellValue = value; }
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

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
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

        public VisualProjectionColumnHeaderCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        public override void Draw(Graphics g, Rectangle cellRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            base.Draw(g, cellRectangle, rowIndex, colIndex, style);
            base.OnDrawDisplayText(g, cellRectangle, rowIndex, colIndex, style);

            TimePeriod timePeriod = (TimePeriod)style.Tag;
            PixelConverter pixelConverter = new PixelConverter(cellRectangle.Width, timePeriod, false);
            Font font = new Font("Arial", 8, FontStyle.Bold);

            string dispDate = (string)style.CellValue;
            SizeF dispDateSize = g.MeasureString(dispDate, font);
            PointF dispDatePoint =
                new PointF(cellRectangle.Left + (cellRectangle.Width / 2) - (dispDateSize.Width / 2),
                           cellRectangle.Top + 1);
            g.DrawString(dispDate, font, Brushes.Black, dispDatePoint);

            string time1 = DateTime.MinValue.Add(TimeSpan.FromHours(12)).ToShortTimeString();
            SizeF size1 = g.MeasureString(time1, font);
            float lastX = 0;
            int firstIndex = (int) timePeriod.StartTime.TotalHours;
            for (int i = firstIndex; i <= (int)timePeriod.EndTime.TotalHours; i++)
            {
                string time = DateTime.MinValue.Add(TimeSpan.FromHours(i)).ToShortTimeString();
                SizeF size = g.MeasureString(time, font);

                float x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));               
                if(i > firstIndex)
                {
                    if (x-lastX <= size1.Width)
                        continue;
                }
                lastX = x;
                if (x > 0)
                {
                    
                    if ((x + (size.Width / 2)) < cellRectangle.Width)
                        g.DrawString(time, font, Brushes.Black,
                                     new PointF(cellRectangle.Left + x - size.Width/2,
                                                cellRectangle.Top + dispDateSize.Height + 1));
                    g.DrawLine(new Pen(Color.LightGray, 1), cellRectangle.Left + x, cellRectangle.Top + dispDateSize.Height + size.Height + 1,
                               cellRectangle.Left + x, cellRectangle.Bottom);
                }
                
            }
        }

 
    }

    
}
