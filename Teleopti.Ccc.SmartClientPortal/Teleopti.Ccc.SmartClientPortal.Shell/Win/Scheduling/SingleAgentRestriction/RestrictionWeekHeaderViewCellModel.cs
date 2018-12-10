using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction
{
    [Serializable]
    public class RestrictionWeekHeaderViewCellModel  : GridHeaderCellModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionWeekHeaderViewCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected RestrictionWeekHeaderViewCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionWeekHeaderViewCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public RestrictionWeekHeaderViewCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new RestrictionWeekHeaderViewCellRenderer(control, this);

        }
    }

    /// <summary>
    /// Renders the restriction cell
    /// </summary>
    public class RestrictionWeekHeaderViewCellRenderer : GridHeaderCellRenderer
    {
        private Image _invalidEffectiveImage = Resources.ccc_Cancel_32x32;
        public RestrictionWeekHeaderViewCellRenderer(GridControlBase grid, GridHeaderCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
            WeekHeaderCellData cellValue = (WeekHeaderCellData)style.CellValue;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;

            if (cellValue.Invalid)
            {
                if (_invalidEffectiveImage != null)
                    g.DrawImage(_invalidEffectiveImage, middleX(clientRectangle) - (_invalidEffectiveImage.Width / (float)2),
                                middleY(clientRectangle) - (_invalidEffectiveImage.Height / (float)2));
                return;
            }

            drawEffective(g, clientRectangle, cellValue, style);
            
            format.Dispose();
        }

        private static void drawEffective(Graphics g, Rectangle clientRectangle, IWeekHeaderCellData cellValue, GridStyleInfo style)
        {
            
            Brush brush = Brushes.Black;
            if (cellValue.Alert)
                brush = Brushes.Red;

            g.DrawString(cellValue.WeekNumber.ToString(style.CultureInfo), style.GdipFont, brush,
                         clientRectangle.Left + (clientRectangle.Width / 2) - 10, clientRectangle.Top + 5);
            
            string text = UserTexts.Resources.WeeklyWorkTime;
            g.DrawString(text, style.GdipFont, brush,
                         clientRectangle.Left + 2, clientRectangle.Top + 24);
            text = UserTexts.Resources.MaxColon + " " + TimeHelper.GetLongHourMinuteTimeString(cellValue.MaximumWeekWorkTime, style.CultureInfo);
            g.DrawString(text, style.GdipFont, brush,
                         clientRectangle.Left + 2, clientRectangle.Top + 38);
            text = UserTexts.Resources.MinColon + " " + TimeHelper.GetLongHourMinuteTimeString(cellValue.MinimumWeekWorkTime, style.CultureInfo);
            g.DrawString(text, style.GdipFont, brush,
                         clientRectangle.Left + 2, clientRectangle.Top + 52);

        }

        private static float middleX(Rectangle clientRectangle)
        {
            return clientRectangle.Left + (clientRectangle.Width/2);
        }

        private static float middleY(Rectangle clientRectangle)
        {
            return clientRectangle.Top + (clientRectangle.Height/2);
        }
    }
}