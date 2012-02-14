using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    public class PersonalAssignmentPainter : StudentAvailabilityCellPainterBase
    {
        private readonly Image _personAssignmentImage = Resources.ccc_Agent_schedule_8x8;
        public PersonalAssignmentPainter() : base(null){}

        public override bool CanPaint(StudentAvailabilityCellData cellValue)
        {
            var painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintPersonalAssignment();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, StudentAvailabilityCellData cellValue, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            var start = clientRectangle.Right - 20;

            var point = new Point(start, clientRectangle.Y + 2);
            var size = new Size(9, 9);
            var rect = new Rectangle(point, size);
            g.DrawImage(_personAssignmentImage, (int) (MiddleX(rect) - (_personAssignmentImage.Width / 2)),
                (int) (MiddleY(rect) - (_personAssignmentImage.Height / 2)));
        }
    }
}
