using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    class AbsencePainter : StudentAvailabilityCellPainterBase
    {
        public AbsencePainter(GridControlBase gridControlBase) : base(gridControlBase)
        {
        }

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintAbsence();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            var rect = RestrictionRect(clientRectangle);
            using (var lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, cellValue.DisplayColor, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
            }
            g.DrawString(LongOrShortText(cellValue.DisplayName, cellValue.DisplayShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
            format.Alignment = StringAlignment.Center;

            var text = effectiveRestriction.WorkTimeLimitation.StartTimeString;
            rect = EffectiveRect(clientRectangle);
            g.DrawString(text, GridControlBase.Font, Brushes.Black, new RectangleF(rect.Left, rect.Top + 12, rect.Width, 12), format);
        }
    }
}
