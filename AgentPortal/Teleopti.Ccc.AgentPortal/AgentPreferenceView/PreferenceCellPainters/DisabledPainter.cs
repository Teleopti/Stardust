using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    class DisabledPainter : IPreferenceCellPainter
    {
        public bool CanPaint(PreferenceCellData cellValue)
        {
            PainterHelper painterHelper = new PainterHelper(cellValue);
            return painterHelper.CanPaintDisabled();
        }

        public void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference, EffectiveRestriction effectiveRestriction, StringFormat format)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.LightGray)))
            {
                g.FillRectangle(brush, clientRectangle);
            }
        }
    }
}
