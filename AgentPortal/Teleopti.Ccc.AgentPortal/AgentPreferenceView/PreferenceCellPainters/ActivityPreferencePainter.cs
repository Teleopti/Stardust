using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    public class ActivityPreferencePainter : PreferenceCellPainterBase
    {
        private readonly Image _magnifierImage = Resources.magnifier_8x8;
        public ActivityPreferencePainter() : base(null) { }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintActivityPreference();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            int start = clientRectangle.Right - 30;

            Point point = new Point(start, clientRectangle.Y + 2);
            Size size = new Size(9, 9);
            Rectangle rect = new Rectangle(point, size);
            g.DrawImage(_magnifierImage, MiddleX(rect) - (_magnifierImage.Width / 2),
                MiddleY(rect) - (_magnifierImage.Height / 2));
        }
    }
}
