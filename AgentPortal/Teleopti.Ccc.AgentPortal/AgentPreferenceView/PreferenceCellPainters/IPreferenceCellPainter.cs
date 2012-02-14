using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters
{
    public interface IPreferenceCellPainter
    {
        bool CanPaint(PreferenceCellData cellValue);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
        void Paint(Graphics g, Rectangle clientRectangle, PreferenceCellData cellValue, Preference preference,
                   EffectiveRestriction effectiveRestriction, StringFormat format);
    }
}
