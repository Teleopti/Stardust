using System.Drawing;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class MustHavePainter: PreferenceCellPainterBase
    {        
        private readonly Image _mustHaveImage = Resources.heart_8x8;

        public MustHavePainter()
            : base(null)
        {
        }

        public override bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintMustHave();
        }

        public override void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            int start = clientRectangle.Right - 10;
            
            Point point = new Point(start, clientRectangle.Y+2);
            Size size = new Size(9,9);
            Rectangle rect = new Rectangle(point, size);
            g.DrawImage(_mustHaveImage, MiddleX(rect) - (_mustHaveImage.Width / 2),
                MiddleY(rect) - (_mustHaveImage.Height / 2));
        }
    }
}
