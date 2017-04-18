using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    class DisabledPainter : PreferenceCellPainterBase
    {
        public DisabledPainter()
            : base(null)
        {}

        public override bool CanPaint(IPreferenceCellData cellValue)
        {
            PainterHelper helper = new PainterHelper(cellValue);
            return helper.CanPaintDisabled();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData cellValue, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.LightGray)))
            {
                g.FillRectangle(brush, clientRectangle);
            }
        }
    }
}