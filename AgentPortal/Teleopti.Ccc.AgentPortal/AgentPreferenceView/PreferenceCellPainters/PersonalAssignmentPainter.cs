using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    public class PersonalAssignmentPainter : PreferenceCellPainterBase
    {
        private readonly Image _personAssignmentImage = Resources.ccc_Agent_schedule_8x8;
        public PersonalAssignmentPainter() : base(null){}

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintPersonalAssignment();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            int start = clientRectangle.Right - 20;

            Point point = new Point(start, clientRectangle.Y + 2);
            Size size = new Size(9, 9);
            Rectangle rect = new Rectangle(point, size);
            g.DrawImage(_personAssignmentImage, MiddleX(rect) - (_personAssignmentImage.Width / 2),
                MiddleY(rect) - (_personAssignmentImage.Height / 2));
        }
    }
}
