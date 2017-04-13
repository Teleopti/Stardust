using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    public interface IPreferenceCellPainter
    {
        bool CanPaint(IPreferenceCellData cellValue);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
        void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference,
                   IEffectiveRestriction effectiveRestriction, StringFormat format);
    }
}