using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView
{
    [Serializable]
    public class RestrictionWeekHeaderViewCellModel : GridHeaderCellModel
    {
        protected RestrictionWeekHeaderViewCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public RestrictionWeekHeaderViewCellModel(GridModel model)
            : base(model)
        {
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new RestrictionWeekHeaderViewCellRenderer(control, this);
        }
    }

    public class RestrictionWeekHeaderViewCellRenderer : GridHeaderCellRenderer
    {
        private Image _notValidatedImage = Resources.ccc_ForecastValidate;
        private Image _invalidEffectiveImage = Resources.ccc_Cancel_32x32;

        public RestrictionWeekHeaderViewCellRenderer(GridControlBase grid, RestrictionWeekHeaderViewCellModel cellModel) : base(grid, cellModel)
        {
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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
                    g.DrawImage(_invalidEffectiveImage, middleX(clientRectangle) - (_invalidEffectiveImage.Width / 2),
                                middleY(clientRectangle) - (_invalidEffectiveImage.Height / 2));
                return;
            }

            if (cellValue.Validated)
                drawEffective(g, clientRectangle, cellValue, style);
            else
            {
                //Rectangle rect = effectiveRect(clientRectangle);
                Rectangle rect = clientRectangle;
                if (_notValidatedImage != null)
                    g.DrawImage(_notValidatedImage, middleX(rect) - (_notValidatedImage.Width / 2),
                                middleY(rect) - (_notValidatedImage.Height / 2));
            }
            format.Dispose();
        }

        private static void drawEffective(Graphics g, Rectangle clientRectangle, WeekHeaderCellData cellValue, GridStyleInfo style)
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
            return clientRectangle.Left + (clientRectangle.Width / 2);
        }

        private static float middleY(Rectangle clientRectangle)
        {
            return clientRectangle.Top + (clientRectangle.Height / 2);
        }
    }
}