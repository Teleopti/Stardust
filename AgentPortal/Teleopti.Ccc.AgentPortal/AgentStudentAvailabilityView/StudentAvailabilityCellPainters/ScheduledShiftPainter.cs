using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    class ScheduledShiftPainter : StudentAvailabilityCellPainterBase
    {
        public ScheduledShiftPainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintScheduledShift();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            var rect = RestrictionRect(clientRectangle);
            rect = EnlargeRectangle(rect, 1.3, true);
            var gradRect = new Rectangle(rect.Location, rect.Size);
            gradRect.Inflate(0, 16);
            gradRect.Offset(0, 15);
            using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, cellValue.DisplayColor, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, rect, 7, lBrush, 0);
            }
            g.DrawString(LongOrShortText(cellValue.DisplayName, cellValue.DisplayShortName, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
        }
    }
}
