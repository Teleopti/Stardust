using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    class StudentAvailabilityRestrictionsPainter : StudentAvailabilityCellPainterBase
    {
        public StudentAvailabilityRestrictionsPainter(GridControlBase gridControlBase)
            : base(gridControlBase)
        {
        }

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintStudentAvailabilityRestrictions();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            if (cellValue.StudentAvailabilityRestrictions == null) return;
            var rect = RestrictionRect(clientRectangle);
            var gradRect = new Rectangle(rect.Location, rect.Size);
            gradRect.Inflate(0, 16);
            gradRect.Offset(0, 15);

            using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, cellValue.DisplayColor, 90, false))
            {
                for (var i = 0; i < cellValue.StudentAvailabilityRestrictions.Count; i++)
                {
                    var drawRect = new Rectangle(rect.X, rect.Y + i * 16, rect.Width, rect.Height);
                    GridHelper.FillRoundedRectangle(g, drawRect, 7, lBrush, 0);
                    g.DrawString(cellValue.StudentAvailabilityRestrictions[i].ShortDateTimePeriod, GridControlBase.Font, Brushes.Black, MiddleX(drawRect),
                                 MiddleY(drawRect) - 7, format);
                }
                
            }
        }
    }
}
