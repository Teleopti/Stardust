using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class ScheduledDayOffPainter : PreferenceCellPainterBase
    {
        public ScheduledDayOffPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintScheduledDayOff();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            Rectangle rect = RestrictionRect(clientRectangle);
            using (
                HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, cellValue.DisplayColor,
                                                  Color.LightGray))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawString(LongOrShortText(cellValue.DisplayName, cellValue.DisplayShortName, rect.Width, g),
                         GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);

            var startSize = g.MeasureString(cellValue.ShiftLengthScheduledShift, GridControlBase.Font);
            var paddingLength = Math.Max((rect.Width - startSize.Width) / 2, 0);

            g.DrawString(cellValue.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingLength, rect.Top + 24);
        }
    }
}